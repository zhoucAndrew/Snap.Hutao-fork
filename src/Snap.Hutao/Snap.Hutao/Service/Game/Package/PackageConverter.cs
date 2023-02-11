﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.DependencyInjection.Annotation.HttpClient;
using Snap.Hutao.Core.ExceptionService;
using Snap.Hutao.Core.IO;
using Snap.Hutao.Model.Binding.LaunchGame;
using Snap.Hutao.Web.Hoyolab.SdkStatic.Hk4e.Launcher;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using static Snap.Hutao.Service.Game.GameConstants;

namespace Snap.Hutao.Service.Game.Package;

/// <summary>
/// 游戏文件包转换器
/// </summary>
[HttpClient(HttpClientConfigration.Default)]
internal class PackageConverter
{
    private readonly JsonSerializerOptions options;
    private readonly HttpClient httpClient;

    /// <summary>
    /// 构造一个新的游戏文件转换器
    /// </summary>
    /// <param name="resourceClient">资源客户端</param>
    /// <param name="options">Json序列化选项</param>
    /// <param name="httpClient">http客户端</param>
    public PackageConverter(JsonSerializerOptions options, HttpClient httpClient)
    {
        this.options = options;
        this.httpClient = httpClient;
    }

    /// <summary>
    /// 异步检查替换游戏资源
    /// 调用前需要确认本地文件与服务器上的不同
    /// </summary>
    /// <param name="targetScheme">目标启动方案</param>
    /// <param name="gameResouce">游戏资源</param>
    /// <param name="gameFolder">游戏目录</param>
    /// <param name="progress">进度</param>
    /// <returns>替换结果与资源</returns>
    public async Task<bool> EnsureGameResourceAsync(LaunchScheme targetScheme, GameResource gameResouce, string gameFolder, IProgress<PackageReplaceStatus> progress)
    {
        await ThreadHelper.SwitchToBackgroundAsync();
        string scatteredFilesUrl = gameResouce.Game.Latest.DecompressedPath;
        Uri pkgVersionUri = new($"{scatteredFilesUrl}/pkg_version");
        ConvertDirection direction = targetScheme.IsOversea ? ConvertDirection.ChineseToOversea : ConvertDirection.OverseaToChinese;

        progress.Report(new(SH.ServiceGamePackageRequestPackageVerion));
        Dictionary<string, VersionItem> remoteItems = default!;
        try
        {
            using (Stream remoteSteam = await httpClient.GetStreamAsync(pkgVersionUri).ConfigureAwait(false))
            {
                remoteItems = await GetVersionItemsAsync(remoteSteam).ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            ThrowHelper.PackageConvert(SH.ServiceGamePackageRequestPackageVerionFailed, ex);
        }

        Dictionary<string, VersionItem> localItems;
        using (FileStream localSteam = File.OpenRead(Path.Combine(gameFolder, "pkg_version")))
        {
            localItems = await GetVersionItemsAsync(localSteam, direction, ConvertRemoteName).ConfigureAwait(false);
        }

        IEnumerable<ItemOperationInfo> diffOperations = GetItemOperationInfos(remoteItems, localItems);
        return await ReplaceGameResourceAsync(diffOperations, gameFolder, scatteredFilesUrl, direction, progress).ConfigureAwait(false);
    }

    /// <summary>
    /// 检查过时文件与Sdk
    /// </summary>
    /// <param name="resource">游戏资源</param>
    /// <param name="gameFolder">游戏文件夹</param>
    /// <returns>任务</returns>
    public async Task EnsureDeprecatedFilesAndSdkAsync(GameResource resource, string gameFolder)
    {
        if (resource.DeprecatedFiles != null)
        {
            foreach (NameMd5 file in resource.DeprecatedFiles)
            {
                string filePath = Path.Combine(gameFolder, file.Name);
                if (File.Exists(filePath))
                {
                    File.Move(filePath, $"{filePath}.backup");
                }
            }
        }

        string sdkDllBackup = Path.Combine(gameFolder, YuanShenData, "Plugins\\PCGameSDK.dll.backup");
        string sdkDll = Path.Combine(gameFolder, YuanShenData, "Plugins\\PCGameSDK.dll");
        string sdkVersionBackup = Path.Combine(gameFolder, YuanShenData, "sdk_pkg_version.backup");
        string sdkVersion = Path.Combine(gameFolder, YuanShenData, "sdk_pkg_version");

        // Only bilibili's sdk is not null
        if (resource.Sdk != null)
        {
            if (File.Exists(sdkDllBackup) && File.Exists(sdkVersionBackup))
            {
                FileOperation.Move(sdkDllBackup, sdkDll, false);
                FileOperation.Move(sdkVersionBackup, sdkVersion, false);
            }
            else
            {
                using (Stream sdkWebStream = await httpClient.GetStreamAsync(resource.Sdk.Path).ConfigureAwait(false))
                {
                    using (ZipArchive zip = new(sdkWebStream))
                    {
                        foreach (ZipArchiveEntry entry in zip.Entries)
                        {
                            if (entry.CompressedLength != 0)
                            {
                                string targetPath = Path.Combine(gameFolder, entry.FullName);
                                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                                entry.ExtractToFile(targetPath, true);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // backup
            FileOperation.Move(sdkDll, sdkDllBackup, true);
            FileOperation.Move(sdkVersion, sdkVersionBackup, true);
        }
    }

    private static string ConvertRemoteName(string remoteName, ConvertDirection direction)
    {
        // 我们已经提前重命名了整个 Data 文件夹 所以需要将 RemoteName 中的 Data 同样替换
        if (remoteName.StartsWith(YuanShenData) || remoteName.StartsWith(GenshinImpactData))
        {
            return direction switch
            {
                ConvertDirection.OverseaToChinese => $"{YuanShenData}{remoteName[GenshinImpactData.Length..]}",
                ConvertDirection.ChineseToOversea => $"{GenshinImpactData}{remoteName[YuanShenData.Length..]}",
                _ => remoteName,
            };
        }

        return remoteName;
    }

    private static IEnumerable<ItemOperationInfo> GetItemOperationInfos(Dictionary<string, VersionItem> remote, Dictionary<string, VersionItem> local)
    {
        foreach ((string remoteName, VersionItem remoteItem) in remote)
        {
            if (local.TryGetValue(remoteName, out VersionItem? localItem))
            {
                if (remoteItem.Md5 != localItem.Md5)
                {
                    // 本地发现了同名且不同MD5的项，需要替换为服务器上的项
                    yield return new(ItemOperationType.Replace, remoteItem, localItem);
                }

                local.Remove(remoteName);
            }
            else
            {
                // 本地没有发现同名项
                yield return new(ItemOperationType.Add, remoteItem, remoteItem);
            }
        }

        IEnumerable<ItemOperationInfo> removes = local.Select(kvp => new ItemOperationInfo(ItemOperationType.Remove, kvp.Value, kvp.Value));

        foreach (ItemOperationInfo item in removes)
        {
            yield return item;
        }
    }

    private static void RenameDataFolder(string gameFolder, ConvertDirection direction)
    {
        string yuanShenData = Path.Combine(gameFolder, YuanShenData);
        string genshinImpactData = Path.Combine(gameFolder, GenshinImpactData);

        // We have check the exe path previously
        // so we assume the data folder is present
        if (direction == ConvertDirection.ChineseToOversea)
        {
            if (Directory.Exists(yuanShenData))
            {
                Directory.Move(yuanShenData, genshinImpactData);
            }
        }
        else
        {
            if (Directory.Exists(genshinImpactData))
            {
                Directory.Move(genshinImpactData, yuanShenData);
            }
        }
    }

    private static void MoveToCache(string cacheFilePath, string targetFullPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(cacheFilePath)!);
        File.Move(targetFullPath, cacheFilePath, true);
    }

    private static async Task CopyToWithProgressAsync(Stream source, Stream target, string name, long totalBytes, IProgress<PackageReplaceStatus> progress)
    {
        const int bufferSize = 81920;

        long totalBytesRead = 0;
        int bytesRead;
        Memory<byte> buffer = new byte[bufferSize];
        do
        {
            bytesRead = await source.ReadAsync(buffer).ConfigureAwait(false);
            await target.WriteAsync(buffer[..bytesRead]).ConfigureAwait(false);

            totalBytesRead += bytesRead;
            progress.Report(new(name, totalBytesRead, totalBytes));

            if (bytesRead <= 0)
            {
                break;
            }
        }
        while (bytesRead > 0);
    }

    private async Task<bool> ReplaceGameResourceAsync(IEnumerable<ItemOperationInfo> operations, string gameFolder, string scatteredFilesUrl, ConvertDirection direction, IProgress<PackageReplaceStatus> progress)
    {
        // 重命名 _Data 目录
        try
        {
            RenameDataFolder(gameFolder, direction);
        }
        catch (IOException)
        {
            // Access to the path is denied.
            // When user install the game in special folder like 'Program Files'
            return false;
        }

        // Cache folder
        string cacheFolder = Path.Combine(Core.CoreEnvironment.DataFolder, "ServerCache");

        // 执行下载与移动操作
        foreach (ItemOperationInfo info in operations)
        {
            progress.Report(new($"{info.Target}"));

            string targetFilePath = Path.Combine(gameFolder, info.Target);
            string cacheFilePath = Path.Combine(cacheFolder, info.Target);
            string moveToFilePath = Path.Combine(cacheFolder, info.MoveTo);

            switch (info.Type)
            {
                case ItemOperationType.Add:
                    await ReplaceFromCacheOrWebAsync(cacheFilePath, targetFilePath, scatteredFilesUrl, info, progress).ConfigureAwait(false);
                    break;
                case ItemOperationType.Replace:
                    {
                        MoveToCache(moveToFilePath, targetFilePath);
                        await ReplaceFromCacheOrWebAsync(cacheFilePath, targetFilePath, scatteredFilesUrl, info, progress).ConfigureAwait(false);
                        break;
                    }

                case ItemOperationType.Remove:
                    MoveToCache(moveToFilePath, targetFilePath);
                    break;

                default:
                    break;
            }
        }

        // 重新下载所有 *pkg_version 文件
        await ReplacePackageVersionsAsync(scatteredFilesUrl, gameFolder).ConfigureAwait(false);
        return true;
    }

    private async Task ReplaceFromCacheOrWebAsync(string cacheFilePath, string targetFilePath, string scatteredFilesUrl, ItemOperationInfo info, IProgress<PackageReplaceStatus> progress)
    {
        if (File.Exists(cacheFilePath))
        {
            string remoteMd5 = await Digest.GetFileMd5Async(cacheFilePath).ConfigureAwait(false);
            if (info.Md5 == remoteMd5.ToLowerInvariant() && new FileInfo(cacheFilePath).Length == info.TotalBytes)
            {
                // Valid, move it to target path
                // There shouldn't be any file in the path/name
                File.Move(cacheFilePath, targetFilePath, false);
                return;
            }
            else
            {
                // Invalid file, delete it
                File.Delete(cacheFilePath);
            }
        }

        // Cache no item, download it anyway.
        using (FileStream fileStream = File.Create(targetFilePath))
        {
            while (true)
            {
                using (HttpResponseMessage response = await httpClient.GetAsync($"{scatteredFilesUrl}/{info.Target}", HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    long totalBytes = response.Content.Headers.ContentLength ?? 0;
                    using (Stream webStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        try
                        {
                            await CopyToWithProgressAsync(webStream, fileStream, info.Target, totalBytes, progress).ConfigureAwait(false);
                            fileStream.Seek(0, SeekOrigin.Begin);
                            string remoteMd5 = await Digest.GetStreamMd5Async(fileStream).ConfigureAwait(false);
                            if (info.Md5 == remoteMd5.ToLowerInvariant())
                            {
                                return;
                            }
                        }
                        catch
                        {
                            // System.IO.IOException: The response ended prematurely.
                            // System.IO.IOException: Received an unexpected EOF or 0 bytes from the transport stream.

                            // We want to retry forever.
                        }
                    }
                }
            }
        }
    }

    private async Task ReplacePackageVersionsAsync(string scatteredFilesUrl, string gameFolder)
    {
        foreach (string versionFilePath in Directory.EnumerateFiles(gameFolder, "*pkg_version"))
        {
            string versionFileName = Path.GetFileName(versionFilePath);

            if (versionFileName == "sdk_pkg_version")
            {
                // Skiping the sdk_pkg_version file,
                // it can't be claimed from remote.
                continue;
            }

            using (FileStream versionFileStream = File.Create(versionFilePath))
            {
                using (Stream webStream = await httpClient.GetStreamAsync($"{scatteredFilesUrl}/{versionFileName}").ConfigureAwait(false))
                {
                    await webStream.CopyToAsync(versionFileStream).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task<Dictionary<string, VersionItem>> GetVersionItemsAsync(Stream stream)
    {
        Dictionary<string, VersionItem> results = new();
        using (StreamReader reader = new(stream))
        {
            while (await reader.ReadLineAsync().ConfigureAwait(false) is string raw)
            {
                if (!string.IsNullOrEmpty(raw))
                {
                    VersionItem item = JsonSerializer.Deserialize<VersionItem>(raw, options)!;
                    results.Add(item.RemoteName, item);
                }
            }
        }

        return results;
    }

    private async Task<Dictionary<string, VersionItem>> GetVersionItemsAsync(Stream stream, ConvertDirection direction, Func<string, ConvertDirection, string> nameConverter)
    {
        Dictionary<string, VersionItem> results = new();

        using (StreamReader reader = new(stream))
        {
            while (await reader.ReadLineAsync().ConfigureAwait(false) is string raw)
            {
                if (!string.IsNullOrEmpty(raw))
                {
                    VersionItem item = JsonSerializer.Deserialize<VersionItem>(raw, options)!;
                    results.Add(nameConverter(item.RemoteName, direction), item);
                }
            }
        }

        return results;
    }
}