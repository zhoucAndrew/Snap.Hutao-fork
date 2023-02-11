﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Common;

namespace Snap.Hutao.Service.Game.Package;

/// <summary>
/// 包更新状态
/// </summary>
public class PackageReplaceStatus
{
    /// <summary>
    /// 构造一个新的包更新状态
    /// </summary>
    /// <param name="description">描述</param>
    public PackageReplaceStatus(string description)
    {
        Description = description;
    }

    /// <summary>
    /// 构造一个新的包更新状态
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="bytesRead">读取的字节数</param>
    /// <param name="totalBytes">总字节数</param>
    public PackageReplaceStatus(string name, long bytesRead, long totalBytes)
    {
        Percent = (double)bytesRead / totalBytes;
        Description = $"{name}\n{Converters.ToFileSizeString(bytesRead)}/{Converters.ToFileSizeString(totalBytes)}";
    }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 是否有进度
    /// </summary>
    public bool IsIndeterminate { get => Percent < 0; }

    /// <summary>
    /// 进度
    /// </summary>
    public double Percent { get; set; } = -1;

    /// <summary>
    /// 克隆
    /// </summary>
    /// <returns>克隆的实例</returns>
    public PackageReplaceStatus Clone()
    {
        // 进度需要在主线程上创建
        return new(Description) { Percent = Percent };
    }
}