﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.SpiralAbyss;

/// <summary>
/// 表示一次战斗
/// </summary>
public class Battle
{
    /// <summary>
    /// 索引
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public long Timestamp { get; set; } = default!;

    /// <summary>
    /// 参战角色
    /// </summary>
    [JsonPropertyName("avatars")]
    public List<Avatar> Avatars { get; set; } = default!;
}
