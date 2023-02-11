﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Snap.Hutao.Core;
using Snap.Hutao.Core.Database;
using Snap.Hutao.Core.ExceptionService;
using Snap.Hutao.Core.IO.Ini;
using Snap.Hutao.Extension;
using Snap.Hutao.Model.Binding.LaunchGame;
using Snap.Hutao.Model.Entity;
using Snap.Hutao.Model.Entity.Database;
using Snap.Hutao.Service.Game.Locator;
using Snap.Hutao.Service.Game.Package;
using Snap.Hutao.Service.Game.Unlocker;
using Snap.Hutao.View.Dialog;
using Snap.Hutao.Web.Hoyolab.SdkStatic.Hk4e.Launcher;
using Snap.Hutao.Web.Response;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using static Snap.Hutao.Service.Game.GameConstants;

namespace Snap.Hutao.Service.Game;

/// <summary>
/// 游戏服务
/// </summary>
[Injection(InjectAs.Singleton, typeof(IGameService))]
[SuppressMessage("", "CA1001")]
internal class GameService : IGameService
{
    private const string GamePathKey = $"{nameof(GameService)}.Cache.{SettingEntry.GamePath}";

    private readonly IServiceScopeFactory scopeFactory;
    private readonly IMemoryCache memoryCache;
    private readonly PackageConverter packageConverter;
    private readonly SemaphoreSlim gameSemaphore = new(1);

    private ObservableCollection<GameAccount>? gameAccounts;

    /// <summary>
    /// 构造一个新的游戏服务
    /// </summary>
    /// <param name="scopeFactory">范围工厂</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="packageConverter">游戏文件包转换器</param>
    public GameService(IServiceScopeFactory scopeFactory, IMemoryCache memoryCache, PackageConverter packageConverter)
    {
        this.scopeFactory = scopeFactory;
        this.memoryCache = memoryCache;
        this.packageConverter = packageConverter;
    }

    /// <inheritdoc/>
    public async ValueTask<ValueResult<bool, string>> GetGamePathAsync()
    {
        if (memoryCache.TryGetValue(GamePathKey, out object? value))
        {
            return new(true, (value as string)!);
        }
        else
        {
            using (IServiceScope scope = scopeFactory.CreateScope())
            {
                AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                SettingEntry entry = await appDbContext.Settings.SingleOrAddAsync(SettingEntry.GamePath, string.Empty).ConfigureAwait(false);

                // Cannot find in setting
                if (string.IsNullOrEmpty(entry.Value))
                {
                    IEnumerable<IGameLocator> gameLocators = scope.ServiceProvider.GetRequiredService<IEnumerable<IGameLocator>>();

                    // Try locate by unity log
                    IGameLocator locator = gameLocators.Single(l => l.Name == nameof(UnityLogGameLocator));
                    ValueResult<bool, string> result = await locator.LocateGamePathAsync().ConfigureAwait(false);

                    if (!result.IsOk)
                    {
                        // Try locate by registry
                        locator = gameLocators.Single(l => l.Name == nameof(RegistryLauncherLocator));
                        result = await locator.LocateGamePathAsync().ConfigureAwait(false);
                    }

                    if (result.IsOk)
                    {
                        // Save result.
                        entry.Value = result.Value;
                        await appDbContext.Settings.UpdateAndSaveAsync(entry).ConfigureAwait(false);
                    }
                    else
                    {
                        return new(false, SH.ServiceGamePathLocateFailed);
                    }
                }

                if (entry.Value == null)
                {
                    return new(false, null!);
                }

                // Set cache and return.
                string path = memoryCache.Set(GamePathKey, entry.Value);
                return new(true, path);
            }
        }
    }

    /// <inheritdoc/>
    public string GetGamePathSkipLocator()
    {
        if (memoryCache.TryGetValue(GamePathKey, out object? value))
        {
            return (value as string)!;
        }
        else
        {
            using (IServiceScope scope = scopeFactory.CreateScope())
            {
                AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                SettingEntry entry = appDbContext.Settings.SingleOrAdd(SettingEntry.GamePath, string.Empty);

                // Set cache and return.
                return memoryCache.Set(GamePathKey, entry.Value!);
            }
        }
    }

    /// <inheritdoc/>
    public void OverwriteGamePath(string path)
    {
        // sync cache
        memoryCache.Set(GamePathKey, path);

        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            SettingEntry entry = appDbContext.Settings.SingleOrAdd(SettingEntry.GamePath, string.Empty);
            entry.Value = path;
            appDbContext.Settings.UpdateAndSave(entry);
        }
    }

    /// <inheritdoc/>
    public MultiChannel GetMultiChannel()
    {
        string gamePath = GetGamePathSkipLocator();
        string configPath = Path.Combine(Path.GetDirectoryName(gamePath) ?? string.Empty, ConfigFileName);

        if (!File.Exists(configPath))
        {
            return new(null, null, configPath);
        }

        using (FileStream stream = File.OpenRead(configPath))
        {
            List<IniElement> elements = IniSerializer.Deserialize(stream).ToList();
            string? channel = elements.OfType<IniParameter>().FirstOrDefault(p => p.Key == "channel")?.Value;
            string? subChannel = elements.OfType<IniParameter>().FirstOrDefault(p => p.Key == "sub_channel")?.Value;

            return new(channel, subChannel);
        }
    }

    /// <inheritdoc/>
    public bool SetMultiChannel(LaunchScheme scheme)
    {
        string gamePath = GetGamePathSkipLocator();
        string configPath = Path.Combine(Path.GetDirectoryName(gamePath)!, ConfigFileName);

        List<IniElement> elements;
        try
        {
            using (FileStream readStream = File.OpenRead(configPath))
            {
                elements = IniSerializer.Deserialize(readStream).ToList();
            }
        }
        catch (FileNotFoundException ex)
        {
            throw new GameFileOperationException(string.Format(SH.ServiceGameSetMultiChannelConfigFileNotFound, configPath), ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new GameFileOperationException(string.Format(SH.ServiceGameSetMultiChannelConfigFileNotFound, configPath), ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new GameFileOperationException(SH.ServiceGameSetMultiChannelUnauthorizedAccess, ex);
        }

        bool changed = false;

        foreach (IniElement element in elements)
        {
            if (element is IniParameter parameter)
            {
                if (parameter.Key == "channel")
                {
                    if (parameter.Value != scheme.Channel)
                    {
                        parameter.Value = scheme.Channel;
                        changed = true;
                    }
                }

                if (parameter.Key == "sub_channel")
                {
                    if (parameter.Value != scheme.SubChannel)
                    {
                        parameter.Value = scheme.SubChannel;
                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            using (FileStream writeStream = File.Create(configPath))
            {
                IniSerializer.Serialize(writeStream, elements);
            }
        }

        return changed;
    }

    /// <inheritdoc/>
    public async Task<bool> EnsureGameResourceAsync(LaunchScheme launchScheme, IProgress<PackageReplaceStatus> progress)
    {
        string gamePath = GetGamePathSkipLocator();
        string gameFolder = Path.GetDirectoryName(gamePath)!;
        string gameFileName = Path.GetFileName(gamePath);

        progress.Report(new(SH.ServiceGameEnsureGameResourceQueryResourceInformation));
        Response<GameResource> response = await Ioc.Default
            .GetRequiredService<ResourceClient>()
            .GetResourceAsync(launchScheme)
            .ConfigureAwait(false);

        if (response.IsOk())
        {
            GameResource resource = response.Data;

            if (!LaunchSchemeMatchesExecutable(launchScheme, gameFileName))
            {
                bool replaced = await packageConverter
                    .EnsureGameResourceAsync(launchScheme, resource, gameFolder, progress)
                    .ConfigureAwait(false);

                if (replaced)
                {
                    // We need to change the gamePath if we switched.
                    string exeName = launchScheme.IsOversea ? GenshinImpactFileName : YuanShenFileName;
                    OverwriteGamePath(Path.Combine(gameFolder, exeName));
                }
                else
                {
                    // We can't start the game
                    // when we failed to convert game
                    return false;
                }
            }

            if (!launchScheme.IsOversea)
            {
                await packageConverter.EnsureDeprecatedFilesAndSdkAsync(resource, gameFolder).ConfigureAwait(false);
            }

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool IsGameRunning()
    {
        if (gameSemaphore.CurrentCount == 0)
        {
            return true;
        }

        return Process.GetProcessesByName(YuanShenFileName).Any()
            || Process.GetProcessesByName(GenshinImpactFileName).Any();
    }

    /// <inheritdoc/>
    public async Task<ObservableCollection<GameAccount>> GetGameAccountCollectionAsync()
    {
        if (gameAccounts == null)
        {
            using (IServiceScope scope = scopeFactory.CreateScope())
            {
                AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await ThreadHelper.SwitchToMainThreadAsync();
                gameAccounts = appDbContext.GameAccounts.AsNoTracking().ToObservableCollection();
            }
        }

        return gameAccounts;
    }

    /// <inheritdoc/>
    public async ValueTask LaunchAsync(LaunchConfiguration configuration)
    {
        if (IsGameRunning())
        {
            return;
        }

        string gamePath = GetGamePathSkipLocator();

        if (string.IsNullOrWhiteSpace(gamePath))
        {
            return;
        }

        // https://docs.unity.cn/cn/current/Manual/PlayerCommandLineArguments.html
        // TODO: impl monitor option.
        string commandLine = new CommandLineBuilder()
            .AppendIf("-popupwindow", configuration.IsBorderless)
            .Append("-screen-fullscreen", configuration.IsFullScreen ? 1 : 0)
            .AppendIf("-window-mode", configuration.IsExclusive, "exclusive")
            .Append("-screen-width", configuration.ScreenWidth)
            .Append("-screen-height", configuration.ScreenHeight)
            .ToString();

        Process game = new()
        {
            StartInfo = new()
            {
                Arguments = commandLine,
                FileName = gamePath,
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Path.GetDirectoryName(gamePath),
            },
        };

        using (await gameSemaphore.EnterAsync().ConfigureAwait(false))
        {
            if (configuration.UnlockFPS)
            {
                IGameFpsUnlocker unlocker = new GameFpsUnlocker(game, configuration.TargetFPS);

                TimeSpan findModuleDelay = TimeSpan.FromMilliseconds(100);
                TimeSpan findModuleLimit = TimeSpan.FromMilliseconds(10000);
                TimeSpan adjustFpsDelay = TimeSpan.FromMilliseconds(2000);
                if (game.Start())
                {
                    await unlocker.UnlockAsync(findModuleDelay, findModuleLimit, adjustFpsDelay).ConfigureAwait(false);
                }
            }
            else
            {
                if (game.Start())
                {
                    await game.WaitForExitAsync().ConfigureAwait(false);
                }
            }
        }
    }

    /// <inheritdoc/>
    public async ValueTask DetectGameAccountAsync()
    {
        Must.NotNull(gameAccounts!);

        string? registrySdk = RegistryInterop.Get();
        if (!string.IsNullOrEmpty(registrySdk))
        {
            GameAccount? account = null;
            try
            {
                account = gameAccounts.SingleOrDefault(a => a.MihoyoSDK == registrySdk);
            }
            catch (InvalidOperationException ex)
            {
                ThrowHelper.UserdataCorrupted(SH.ServiceGameDetectGameAccountMultiMatched, ex);
            }

            if (account == null)
            {
                // ContentDialog must be created by main thread.
                await ThreadHelper.SwitchToMainThreadAsync();
                (bool isOk, string name) = await new LaunchGameAccountNameDialog().GetInputNameAsync().ConfigureAwait(false);

                if (isOk)
                {
                    account = GameAccount.Create(name, registrySdk);

                    // sync database
                    await ThreadHelper.SwitchToBackgroundAsync();
                    using (IServiceScope scope = scopeFactory.CreateScope())
                    {
                        await scope.ServiceProvider
                            .GetRequiredService<AppDbContext>()
                            .GameAccounts
                            .AddAndSaveAsync(account)
                            .ConfigureAwait(false);
                    }

                    // sync cache
                    await ThreadHelper.SwitchToMainThreadAsync();
                    gameAccounts.Add(account);
                }
            }
        }
    }

    /// <inheritdoc/>
    public bool SetGameAccount(GameAccount account)
    {
        return RegistryInterop.Set(account);
    }

    /// <inheritdoc/>
    public void AttachGameAccountToUid(GameAccount gameAccount, string uid)
    {
        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            gameAccount.UpdateAttachUid(uid);
            scope.ServiceProvider.GetRequiredService<AppDbContext>().GameAccounts.UpdateAndSave(gameAccount);
        }
    }

    /// <inheritdoc/>
    public async ValueTask ModifyGameAccountAsync(GameAccount gameAccount)
    {
        (bool isOk, string name) = await new LaunchGameAccountNameDialog().GetInputNameAsync().ConfigureAwait(true);

        if (isOk)
        {
            gameAccount.UpdateName(name);

            // sync database
            await ThreadHelper.SwitchToBackgroundAsync();
            using (IServiceScope scope = scopeFactory.CreateScope())
            {
                AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await appDbContext.GameAccounts.UpdateAndSaveAsync(gameAccount).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc/>
    public async ValueTask RemoveGameAccountAsync(GameAccount gameAccount)
    {
        await ThreadHelper.SwitchToMainThreadAsync();
        gameAccounts!.Remove(gameAccount);

        await ThreadHelper.SwitchToBackgroundAsync();
        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().GameAccounts.RemoveAndSaveAsync(gameAccount).ConfigureAwait(false);
        }
    }

    private static bool LaunchSchemeMatchesExecutable(LaunchScheme launchScheme, string gameFileName)
    {
        return (launchScheme.IsOversea && gameFileName == GenshinImpactFileName)
            || (!launchScheme.IsOversea && gameFileName == YuanShenFileName);
    }
}