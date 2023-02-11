﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Service.Game;

/// <summary>
/// 游戏文件操作异常
/// </summary>
internal class GameFileOperationException : Exception
{
    /// <summary>
    /// 构造一个新的用户数据损坏异常
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="innerException">内部错误</param>
    public GameFileOperationException(string message, Exception innerException)
        : base(string.Format(SH.ServiceGameFileOperationExceptionMessage, message), innerException)
    {
    }
}