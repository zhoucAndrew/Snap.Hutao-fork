﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Win32;
using Snap.Hutao.Core.ExceptionService;
using Snap.Hutao.Model.Entity;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Snap.Hutao.Service.Game;

/// <summary>
/// 注册表操作
/// </summary>
internal static class RegistryInterop
{
    private const string GenshinKey = @"HKEY_CURRENT_USER\Software\miHoYo\原神";
    private const string SdkKey = "MIHOYOSDK_ADL_PROD_CN_h3123967166";

    /// <summary>
    /// 设置键值
    /// 需要支持
    /// https://learn.microsoft.com/zh-cn/windows/win32/fileio/maximum-file-path-limitation
    /// </summary>
    /// <param name="account">账户</param>
    /// <returns>账号是否设置</returns>
    public static bool Set(GameAccount? account)
    {
        if (account != null)
        {
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(account.MihoyoSDK));
            string path = $"HKCU:{GenshinKey[@"HKEY_CURRENT_USER\".Length..]}";
            string command = $"""
                $value = [Convert]::FromBase64String('{base64}');
                Set-ItemProperty -Path '{path}' -Name '{SdkKey}' -Value $value -Force;
                """;

            string psExecutablePath = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";

            ProcessStartInfo startInfo = new()
            {
                Arguments = command,
                WorkingDirectory = Path.GetDirectoryName(psExecutablePath),
                CreateNoWindow = true,
                FileName = psExecutablePath,
            };

            try
            {
                Process.Start(startInfo)?.WaitForExit();
            }
            catch (Win32Exception ex)
            {
                ThrowHelper.RuntimeEnvironment(SH.ServiceGameRegisteryInteropLongPathsDisabled, ex);
            }

            if (Get() == account.MihoyoSDK)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 在注册表中获取账号信息
    /// </summary>
    /// <returns>当前注册表中的信息</returns>
    public static string? Get()
    {
        object? sdk = Registry.GetValue(GenshinKey, SdkKey, Array.Empty<byte>());

        if (sdk is byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        return null;
    }
}