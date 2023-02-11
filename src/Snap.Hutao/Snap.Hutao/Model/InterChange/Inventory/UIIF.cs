﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;

namespace Snap.Hutao.Model.InterChange.Inventory;

/// <summary>
/// 统一可交换物品格式
/// </summary>
internal class UIIF
{
    /// <summary>
    /// 当前发行的版本
    /// </summary>
    public const string CurrentVersion = "v1.0";

    private static readonly ImmutableList<string> SupportedVersion = new List<string>()
    {
        CurrentVersion,
    }.ToImmutableList();

    /// <summary>
    /// 信息
    /// </summary>
    [JsonPropertyName("info")]
    public UIIFInfo Info { get; set; } = default!;

    /// <summary>
    /// 列表
    /// </summary>
    [JsonPropertyName("list")]
    public List<UIIFItem> List { get; set; } = default!;
}