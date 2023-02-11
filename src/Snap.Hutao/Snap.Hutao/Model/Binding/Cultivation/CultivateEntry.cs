﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Binding.Gacha.Abstraction;

namespace Snap.Hutao.Model.Binding.Cultivation;

/// <summary>
/// 养成物品
/// </summary>
public class CultivateEntry : ItemBase
{
    /// <summary>
    /// 构造一个新的养成清单入口
    /// </summary>
    /// <param name="entry">实体入口</param>
    /// <param name="itemBase">对应物品</param>
    /// <param name="items">物品列表</param>
    public CultivateEntry(Entity.CultivateEntry entry, ItemBase itemBase, List<CultivateItem> items)
    {
        Id = entry.Id;
        EntryId = entry.InnerId;
        Name = itemBase.Name;
        Icon = itemBase.Icon;
        Badge = itemBase.Badge;
        Quality = itemBase.Quality;
        Items = items;
    }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 入口Id
    /// </summary>
    public Guid EntryId { get; set; }

    /// <summary>
    /// 实体
    /// </summary>
    public List<CultivateItem> Items { get; set; } = default!;

    /// <summary>
    /// 是否为今日的材料
    /// </summary>
    public bool IsToday { get => Items.Any(i => i.IsToday); }

    /// <summary>
    /// 星期中的日期
    /// </summary>
    public DaysOfWeek DaysOfWeek
    {
        get => Items.FirstOrDefault(i => i.DaysOfWeek != DaysOfWeek.Any)?.DaysOfWeek ?? DaysOfWeek.Any;
    }
}