﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web.Hoyolab.Hk4e.Event.GachaInfo;

/// <summary>
/// 祈愿记录分页
/// </summary>
public class GachaLogPage
{
    /// <summary>
    /// 页码
    /// </summary>
    [JsonPropertyName("page")]
    public string Page { get; set; } = default!;

    /// <summary>
    /// 尺寸
    /// </summary>
    [JsonPropertyName("size")]
    public string Size { get; set; } = default!;

    /// <summary>
    /// 总页数
    /// </summary>
    [Obsolete("总是为 0")]
    [JsonPropertyName("total")]
    public string Total { get; set; } = default!;

    /// <summary>
    /// 总页数
    /// 总是为 0
    /// </summary>
    [JsonPropertyName("list")]
    public List<GachaLogItem> List { get; set; } = default!;

    /// <summary>
    /// 地区
    /// </summary>
    [JsonPropertyName("region")]
    public string Region { get; set; } = default!;
}
