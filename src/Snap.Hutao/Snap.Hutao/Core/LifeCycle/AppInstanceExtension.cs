﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Windows.AppLifecycle;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.Com;
using static Windows.Win32.PInvoke;

namespace Snap.Hutao.Core.LifeCycle;

/// <summary>
/// App 实例拓展
/// </summary>
internal static class AppInstanceExtension
{
    // Hold the reference here to prevent memory corruption.
    private static HANDLE redirectEventHandle = HANDLE.Null;

    /// <summary>
    /// 同步非阻塞重定向
    /// </summary>
    /// <param name="appInstance">app实例</param>
    /// <param name="args">参数</param>
    [SuppressMessage("", "VSTHRD002")]
    [SuppressMessage("", "VSTHRD110")]
    public static unsafe void RedirectActivationTo(this AppInstance appInstance, AppActivationArguments args)
    {
        redirectEventHandle = CreateEvent(default(SECURITY_ATTRIBUTES*), true, false, null);
        Task.Run(() =>
        {
            appInstance.RedirectActivationToAsync(args).AsTask().Wait();
            SetEvent(redirectEventHandle);
        });

        ReadOnlySpan<HANDLE> handles = new(in redirectEventHandle);
        CoWaitForMultipleObjects((uint)CWMO_FLAGS.CWMO_DEFAULT, INFINITE, handles, out uint _);
    }
}