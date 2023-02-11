﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Binding.Gacha;
using Snap.Hutao.Model.Binding.Gacha.Abstraction;
using Snap.Hutao.Model.Binding.Hutao;
using Snap.Hutao.Model.Calculable;
using Snap.Hutao.Model.Intrinsic;
using Snap.Hutao.Model.Metadata.Abstraction;
using Snap.Hutao.Model.Metadata.Converter;
using Snap.Hutao.Web.Hoyolab.Takumi.Event.Calculate;

namespace Snap.Hutao.Model.Metadata.Weapon;

/// <summary>
/// 武器的接口实现
/// </summary>
public partial class Weapon : IStatisticsItemSource, ISummaryItemSource, INameQuality, ICalculableSource<ICalculableWeapon>
{
    /// <summary>
    /// [非元数据] 搭配数据
    /// </summary>
    [JsonIgnore]
    public ComplexWeaponCollocation? Collocation { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public ItemQuality Quality
    {
        get => RankLevel;
    }

    /// <inheritdoc/>
    public ICalculableWeapon ToCalculable()
    {
        return new CalculableWeapon(this);
    }

    /// <summary>
    /// 转换为基础物品
    /// </summary>
    /// <returns>基础物品</returns>
    public ItemBase ToItemBase()
    {
        return new()
        {
            Name = Name,
            Icon = EquipIconConverter.IconNameToUri(Icon),
            Badge = WeaponTypeIconConverter.WeaponTypeToIconUri(WeaponType),
            Quality = RankLevel,
        };
    }

    /// <summary>
    /// 转换到统计物品
    /// </summary>
    /// <param name="count">个数</param>
    /// <returns>统计物品</returns>
    public StatisticsItem ToStatisticsItem(int count)
    {
        return new()
        {
            Name = Name,
            Icon = EquipIconConverter.IconNameToUri(Icon),
            Badge = WeaponTypeIconConverter.WeaponTypeToIconUri(WeaponType),
            Quality = RankLevel,
            Count = count,
        };
    }

    /// <summary>
    /// 转换到简述统计物品
    /// </summary>
    /// <param name="lastPull">距上个五星</param>
    /// <param name="time">时间</param>
    /// <param name="isUp">是否为Up物品</param>
    /// <returns>简述统计物品</returns>
    public SummaryItem ToSummaryItem(int lastPull, DateTimeOffset time, bool isUp)
    {
        return new()
        {
            Name = Name,
            Icon = EquipIconConverter.IconNameToUri(Icon),
            Badge = WeaponTypeIconConverter.WeaponTypeToIconUri(WeaponType),
            Time = time,
            Quality = RankLevel,
            LastPull = lastPull,
            IsUp = isUp,
        };
    }
}