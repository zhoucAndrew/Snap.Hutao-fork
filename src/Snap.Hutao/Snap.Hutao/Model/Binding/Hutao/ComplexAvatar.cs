﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Intrinsic;
using Snap.Hutao.Model.Metadata.Avatar;
using Snap.Hutao.Model.Metadata.Converter;

namespace Snap.Hutao.Model.Binding.Hutao;

/// <summary>
/// 角色
/// </summary>
public class ComplexAvatar
{
    /// <summary>
    /// 构造一个胡桃数据库角色
    /// </summary>
    /// <param name="avatar">元数据角色</param>
    /// <param name="rate">率</param>
    public ComplexAvatar(Avatar avatar, double rate)
    {
        Name = avatar.Name;
        Icon = AvatarIconConverter.IconNameToUri(avatar.Icon);
        SideIcon = AvatarSideIconConverter.IconNameToUri(avatar.SideIcon);
        Quality = avatar.Quality;
        Rate = $"{rate:P3}";
    }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 图标
    /// </summary>
    public Uri Icon { get; set; } = default!;

    /// <summary>
    /// 侧面图标
    /// </summary>
    public Uri SideIcon { get; set; } = default!;

    /// <summary>
    /// 星级
    /// </summary>
    public ItemQuality Quality { get; set; }

    /// <summary>
    /// 比率
    /// </summary>
    public string Rate { get; set; } = default!;
}