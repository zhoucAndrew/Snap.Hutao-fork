﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Intrinsic;

namespace Snap.Hutao.Web.Hoyolab.Takumi.Event.Calculate;

/// <summary>
/// 要消耗的物品信息
/// </summary>
public class Item
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 图标Url
    /// </summary>
    [JsonPropertyName("icon_url")]
    public Uri Icon { get; set; } = default!;

    /// <summary>
    /// 数量
    /// </summary>
    [JsonPropertyName("num")]
    public int Num { get; set; }

    /// <summary>
    /// 物品星级 仅有家具为有效值
    /// </summary>
    [JsonPropertyName("level")]
    public ItemQuality Level { get; set; }
}