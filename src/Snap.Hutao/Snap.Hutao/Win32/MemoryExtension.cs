﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text;
using Windows.Win32.Foundation;

namespace Snap.Hutao.Win32;

/// <summary>
/// 内存拓展 for <see cref="Memory{T}"/> and <see cref="Span{T}"/>
/// </summary>
internal static class MemoryExtension
{
    /// <summary>
    /// 将 __CHAR_256 转换到 字符串
    /// </summary>
    /// <param name="char256">目标字符数组</param>
    /// <returns>结果字符串</returns>
    public static unsafe string AsString(this ref __CHAR_256 char256)
    {
        fixed (CHAR* pszModule = &char256._0)
        {
            return Encoding.UTF8.GetString((byte*)pszModule, StringLength(pszModule));
        }
    }

    private static unsafe int StringLength(CHAR* pszStr)
    {
        int len = 0;
        while (*pszStr++ != 0)
        {
            ++len;
        }

        return len;
    }
}