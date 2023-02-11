﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Snap.Hutao.Control.Extension;
using Snap.Hutao.Core.Database;
using Snap.Hutao.Core.LifeCycle;
using Snap.Hutao.Model.Binding.LaunchGame;
using Snap.Hutao.Model.Entity;
using Snap.Hutao.Model.Entity.Database;
using Snap.Hutao.Service.Abstraction;
using Snap.Hutao.Service.Game;
using Snap.Hutao.Service.Navigation;
using Snap.Hutao.Service.User;
using Snap.Hutao.View.Dialog;
using Snap.Hutao.Web.Hoyolab.Takumi.Binding;
using System.Collections.ObjectModel;
using System.IO;

namespace Snap.Hutao.ViewModel;

/// <summary>
/// 启动游戏视图模型
/// </summary>
[Injection(InjectAs.Scoped)]
internal class LaunchGameViewModel : Abstraction.ViewModel
{
    /// <summary>
    /// 启动游戏目标 Uid
    /// </summary>
    public const string DesiredUid = nameof(DesiredUid);

    private static readonly string TrueString = true.ToString();
    private static readonly string FalseString = false.ToString();

    private readonly IServiceProvider serviceProvider;
    private readonly IGameService gameService;
    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    private readonly List<LaunchScheme> knownSchemes = LaunchScheme.KnownSchemes.ToList();

    private LaunchScheme? selectedScheme;
    private ObservableCollection<GameAccount>? gameAccounts;
    private GameAccount? selectedGameAccount;
    private bool isExclusive;
    private bool isFullScreen;
    private bool isBorderless;
    private int screenWidth;
    private int screenHeight;
    private bool unlockFps;
    private int targetFps;

    /// <summary>
    /// 构造一个新的启动游戏视图模型
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    public LaunchGameViewModel(IServiceProvider serviceProvider)
    {
        gameService = serviceProvider.GetRequiredService<IGameService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        this.serviceProvider = serviceProvider;

        OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
        LaunchCommand = new AsyncRelayCommand(LaunchAsync);
        DetectGameAccountCommand = new AsyncRelayCommand(DetectGameAccountAsync);
        ModifyGameAccountCommand = new AsyncRelayCommand<GameAccount>(ModifyGameAccountAsync);
        RemoveGameAccountCommand = new AsyncRelayCommand<GameAccount>(RemoveGameAccountAsync);
        AttachGameAccountCommand = new RelayCommand<GameAccount>(AttachGameAccountToCurrentUserGameRole);
    }

    /// <summary>
    /// 已知的服务器方案
    /// </summary>
    public List<LaunchScheme> KnownSchemes { get => knownSchemes; }

    /// <summary>
    /// 当前选择的服务器方案
    /// </summary>
    public LaunchScheme? SelectedScheme { get => selectedScheme; set => SetProperty(ref selectedScheme, value); }

    /// <summary>
    /// 游戏账号集合
    /// </summary>
    public ObservableCollection<GameAccount>? GameAccounts { get => gameAccounts; set => SetProperty(ref gameAccounts, value); }

    /// <summary>
    /// 选中的账号
    /// </summary>
    public GameAccount? SelectedGameAccount { get => selectedGameAccount; set => SetProperty(ref selectedGameAccount, value); }

    /// <summary>
    /// 是否为独占全屏
    /// </summary>
    public bool IsExclusive
    {
        get => isExclusive; set
        {
            if (SetProperty(ref isExclusive, value))
            {
                if (value)
                {
                    IsFullScreen = true;
                }
            }
        }
    }

    /// <summary>
    /// 全屏
    /// </summary>
    public bool IsFullScreen
    {
        get => isFullScreen; set
        {
            if (SetProperty(ref isFullScreen, value))
            {
                if (value)
                {
                    IsBorderless = false;
                }
            }
        }
    }

    /// <summary>
    /// 无边框
    /// </summary>
    public bool IsBorderless
    {
        get => isBorderless; set
        {
            if (SetProperty(ref isBorderless, value))
            {
                if (value)
                {
                    IsExclusive = false;
                    IsFullScreen = false;
                }
            }
        }
    }

    /// <summary>
    /// 宽度
    /// </summary>
    public int ScreenWidth { get => screenWidth; set => SetProperty(ref screenWidth, value); }

    /// <summary>
    /// 高度
    /// </summary>
    public int ScreenHeight { get => screenHeight; set => SetProperty(ref screenHeight, value); }

    /// <summary>
    /// 解锁帧率
    /// </summary>
    public bool UnlockFps { get => unlockFps; set => SetProperty(ref unlockFps, value); }

    /// <summary>
    /// 目标帧率
    /// </summary>
    public int TargetFps { get => targetFps; set => SetProperty(ref targetFps, value); }

    /// <summary>
    /// 是否提权
    /// </summary>
    [SuppressMessage("", "CA1822")]
    public bool IsElevated { get => Activation.GetElevated(); }

    /// <summary>
    /// 打开界面命令
    /// </summary>
    public ICommand OpenUICommand { get; }

    /// <summary>
    /// 启动游戏命令
    /// </summary>
    public ICommand LaunchCommand { get; }

    /// <summary>
    /// 检测游戏账号命令
    /// </summary>
    public ICommand DetectGameAccountCommand { get; }

    /// <summary>
    /// 修改游戏账号命令
    /// </summary>
    public ICommand ModifyGameAccountCommand { get; }

    /// <summary>
    /// 删除游戏账号命令
    /// </summary>
    public ICommand RemoveGameAccountCommand { get; }

    /// <summary>
    /// 绑定到Uid命令
    /// </summary>
    public ICommand AttachGameAccountCommand { get; }

    private async Task OpenUIAsync()
    {
        if (File.Exists(gameService.GetGamePathSkipLocator()))
        {
            try
            {
                using (await EnterCriticalExecutionAsync().ConfigureAwait(false))
                {
                    MultiChannel multi = gameService.GetMultiChannel();
                    if (string.IsNullOrEmpty(multi.ConfigFilePath))
                    {
                        SelectedScheme = KnownSchemes.FirstOrDefault(s => s.Channel == multi.Channel && s.SubChannel == multi.SubChannel);
                    }
                    else
                    {
                        serviceProvider.GetRequiredService<IInfoBarService>().Warning(SH.ViewModelLaunchGameMultiChannelReadFail);
                    }

                    GameAccounts = await gameService.GetGameAccountCollectionAsync().ConfigureAwait(true);

                    // Sync uid
                    if (memoryCache.TryGetValue(DesiredUid, out object? value) && value is string uid)
                    {
                        SelectedGameAccount = GameAccounts.FirstOrDefault(g => g.AttachUid == uid);
                        memoryCache.Remove(DesiredUid);
                    }

                    // Sync from Settings
                    RetiveSetting();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
        else
        {
            serviceProvider.GetRequiredService<IInfoBarService>().Warning(SH.ViewModelLaunchGamePathInvalid);
            await serviceProvider.GetRequiredService<INavigationService>()
                .NavigateAsync<View.Page.SettingPage>(INavigationAwaiter.Default, true)
                .ConfigureAwait(false);
        }
    }

    private void RetiveSetting()
    {
        DbSet<SettingEntry> settings = appDbContext.Settings;

        isFullScreen = settings.SingleOrAdd(SettingEntry.LaunchIsFullScreen, TrueString).GetBoolean();
        OnPropertyChanged(nameof(IsFullScreen));

        isBorderless = settings.SingleOrAdd(SettingEntry.LaunchIsBorderless, FalseString).GetBoolean();
        OnPropertyChanged(nameof(IsBorderless));

        screenWidth = settings.SingleOrAdd(SettingEntry.LaunchScreenWidth, "1920").GetInt32();
        OnPropertyChanged(nameof(ScreenWidth));

        screenHeight = settings.SingleOrAdd(SettingEntry.LaunchScreenHeight, "1080").GetInt32();
        OnPropertyChanged(nameof(ScreenHeight));

        unlockFps = settings.SingleOrAdd(SettingEntry.LaunchUnlockFps, FalseString).GetBoolean();
        OnPropertyChanged(nameof(UnlockFps));

        targetFps = settings.SingleOrAdd(SettingEntry.LaunchTargetFps, "60").GetInt32();
        OnPropertyChanged(nameof(TargetFps));
    }

    private void SaveSetting()
    {
        DbSet<SettingEntry> settings = appDbContext.Settings;
        settings.SingleOrAdd(SettingEntry.LaunchIsExclusive, FalseString).SetBoolean(IsExclusive);
        settings.SingleOrAdd(SettingEntry.LaunchIsFullScreen, FalseString).SetBoolean(IsFullScreen);
        settings.SingleOrAdd(SettingEntry.LaunchIsBorderless, FalseString).SetBoolean(IsBorderless);
        settings.SingleOrAdd(SettingEntry.LaunchScreenWidth, "1920").SetInt32(ScreenWidth);
        settings.SingleOrAdd(SettingEntry.LaunchScreenHeight, "1080").SetInt32(ScreenHeight);
        settings.SingleOrAdd(SettingEntry.LaunchUnlockFps, FalseString).SetBoolean(UnlockFps);
        settings.SingleOrAdd(SettingEntry.LaunchTargetFps, "60").SetInt32(TargetFps);
        appDbContext.SaveChanges();
    }

    private async Task LaunchAsync()
    {
        IInfoBarService infoBarService = serviceProvider.GetRequiredService<IInfoBarService>();

        if (gameService.IsGameRunning())
        {
            return;
        }

        if (SelectedScheme != null)
        {
            try
            {
                if (gameService.SetMultiChannel(SelectedScheme))
                {
                    // Channel changed, we need to change local file.
                    // Note that if channel changed successfully, the
                    // access level is already high enough.
                    await ThreadHelper.SwitchToMainThreadAsync();
                    LaunchGamePackageConvertDialog dialog = new();
                    Progress<Service.Game.Package.PackageReplaceStatus> progress = new(state => dialog.State = state.Clone());
                    await using (await dialog.BlockAsync().ConfigureAwait(false))
                    {
                        if (!await gameService.EnsureGameResourceAsync(SelectedScheme, progress).ConfigureAwait(false))
                        {
                            infoBarService.Warning(SH.ViewModelLaunchGameEnsureGameResourceFail);
                        }
                    }
                }

                if (SelectedGameAccount != null)
                {
                    if (!gameService.SetGameAccount(SelectedGameAccount))
                    {
                        infoBarService.Warning(SH.ViewModelLaunchGameSwitchGameAccountFail);
                    }
                }

                SaveSetting();

                LaunchConfiguration configuration = new(IsExclusive, IsFullScreen, IsBorderless, ScreenWidth, ScreenHeight, IsElevated && UnlockFps, TargetFps);
                await gameService.LaunchAsync(configuration).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                infoBarService.Error(ex);
            }
        }
    }

    private async Task DetectGameAccountAsync()
    {
        try
        {
            await gameService.DetectGameAccountAsync().ConfigureAwait(false);
        }
        catch (Core.ExceptionService.UserdataCorruptedException ex)
        {
            serviceProvider.GetRequiredService<IInfoBarService>().Error(ex);
        }
    }

    private void AttachGameAccountToCurrentUserGameRole(GameAccount? gameAccount)
    {
        if (gameAccount != null)
        {
            IUserService userService = serviceProvider.GetRequiredService<IUserService>();
            IInfoBarService infoBarService = serviceProvider.GetRequiredService<IInfoBarService>();

            if (userService.Current?.SelectedUserGameRole is UserGameRole role)
            {
                gameService.AttachGameAccountToUid(gameAccount, role.GameUid);
            }
            else
            {
                infoBarService.Warning(SH.MustSelectUserAndUid);
            }
        }
    }

    private async Task ModifyGameAccountAsync(GameAccount? gameAccount)
    {
        if (gameAccount != null)
        {
            await gameService.ModifyGameAccountAsync(gameAccount).ConfigureAwait(false);
        }
    }

    private async Task RemoveGameAccountAsync(GameAccount? gameAccount)
    {
        if (gameAccount != null)
        {
            await gameService.RemoveGameAccountAsync(gameAccount).ConfigureAwait(false);
        }
    }
}