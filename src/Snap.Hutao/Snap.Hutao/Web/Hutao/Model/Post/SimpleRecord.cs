﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.Avatar;
using Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.SpiralAbyss;

namespace Snap.Hutao.Web.Hutao.Model.Post;

/// <summary>
/// 记录
/// </summary>
public class SimpleRecord
{
    /// <summary>
    /// 构造一个新的记录
    /// </summary>
    /// <param name="uid">uid</param>
    /// <param name="characters">详细的角色信息</param>
    /// <param name="spiralAbyss">深渊信息</param>
    public SimpleRecord(string uid, List<Character> characters, SpiralAbyss spiralAbyss)
    {
        Uid = uid;
        Identity = "Snap Hutao";
        SpiralAbyss = new(spiralAbyss);
        Avatars = characters.Select(a => new SimpleAvatar(a));
    }

    /// <summary>
    /// Uid
    /// </summary>
    public string Uid { get; set; } = default!;

    /// <summary>
    /// 上传者身份
    /// </summary>
    public string Identity { get; set; } = default!;

    /// <summary>
    /// 深境螺旋
    /// </summary>
    public SimpleSpiralAbyss SpiralAbyss { get; set; } = default!;

    /// <summary>
    /// 角色
    /// </summary>
    public IEnumerable<SimpleAvatar> Avatars { get; set; } = default!;
}
