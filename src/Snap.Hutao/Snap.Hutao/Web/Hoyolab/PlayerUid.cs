﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web.Hoyolab;

/// <summary>
/// 玩家 Uid
/// </summary>
public readonly struct PlayerUid
{
    /// <summary>
    /// UID 的实际值
    /// </summary>
    public readonly string Value;

    /// <summary>
    /// 地区代码
    /// </summary>
    public readonly string Region;

    /// <summary>
    /// 构造一个新的玩家 Uid 结构
    /// </summary>
    /// <param name="value">uid</param>
    /// <param name="region">服务器，当提供该参数时会无条件信任</param>
    public PlayerUid(string value, string? region = default)
    {
        Must.Argument(value.Length == 9, "uid 应为9位数字");
        Value = value;
        Region = region ?? EvaluateRegion(value[0]);
    }

    public static implicit operator PlayerUid(string source)
    {
        return new(source);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value;
    }

    private static string EvaluateRegion(char first)
    {
        return first switch
        {
            >= '1' and <= '4' => "cn_gf01", // 国服
            '5' => "cn_qd01",               // 渠道
            '6' => "os_usa",                // 美服
            '7' => "os_euro",               // 欧服
            '8' => "os_asia",               // 亚服
            '9' => "os_cht",                // 台服
            _ => throw Must.NeverHappen(),
        };
    }
}