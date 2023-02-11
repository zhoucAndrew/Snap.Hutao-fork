﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.SpiralAbyss;

/// <summary>
/// 层
/// </summary>
public class Floor
{
    /// <summary>
    /// 层号
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = default!;

    /// <summary>
    /// 是否解锁
    /// </summary>
    [JsonPropertyName("is_unlock")]
    public bool IsUnlock { get; set; } = default!;

    /// <summary>
    /// 结束时间
    /// </summary>
    [JsonPropertyName("settle_time")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public long SettleTime { get; set; } = default!;

    /// <summary>
    /// 星数
    /// </summary>
    [JsonPropertyName("star")]
    public int Star { get; set; }

    /// <summary>
    /// 最大星数
    /// </summary>
    [JsonPropertyName("max_star")]
    public int MaxStar { get; set; }

    /// <summary>
    /// 层信息
    /// </summary>
    [JsonPropertyName("levels")]
    public List<Level> Levels { get; set; } = default!;
}
