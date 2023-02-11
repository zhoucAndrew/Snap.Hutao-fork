﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Intrinsic;
using Snap.Hutao.Model.Metadata.Converter;
using Snap.Hutao.Model.Metadata.Weapon;

namespace Snap.Hutao.Model.Binding.Hutao;

/// <summary>
/// 胡桃数据库武器
/// </summary>
public class ComplexWeapon
{
    /// <summary>
    /// 构造一个胡桃数据库武器
    /// </summary>
    /// <param name="weapon">元数据武器</param>
    /// <param name="rate">率</param>
    public ComplexWeapon(Weapon weapon, double rate)
    {
        Name = weapon.Name;
        Icon = EquipIconConverter.IconNameToUri(weapon.Icon);
        Quality = weapon.Quality;
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
    /// 星级
    /// </summary>
    public ItemQuality Quality { get; set; }

    /// <summary>
    /// 比率
    /// </summary>
    public string Rate { get; set; } = default!;
}