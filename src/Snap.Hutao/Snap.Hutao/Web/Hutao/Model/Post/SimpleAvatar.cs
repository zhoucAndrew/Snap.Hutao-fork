﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.Avatar;

namespace Snap.Hutao.Web.Hutao.Model.Post;

/// <summary>
/// 角色详情 角色
/// </summary>
public class SimpleAvatar
{
    /// <summary>
    /// 构造一个新的角色详情 角色
    /// </summary>
    /// <param name="character">角色</param>
    public SimpleAvatar(Character character)
    {
        AvatarId = character.Id;
        WeaponId = character.Weapon.Id;
        ReliquarySetIds = character.Reliquaries.Select(r => r.ReliquarySet.Id);
        ActivedConstellationNumber = character.ActivedConstellationNum;
    }

    /// <summary>
    /// 角色 Id
    /// </summary>
    public int AvatarId { get; set; }

    /// <summary>
    /// 武器 Id
    /// </summary>
    public int WeaponId { get; set; }

    /// <summary>
    /// 圣遗物套装Id
    /// </summary>
    public IEnumerable<int> ReliquarySetIds { get; set; } = default!;

    /// <summary>
    /// 命座
    /// </summary>
    public int ActivedConstellationNumber { get; set; }
}
