﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.Json.Converter;
using Snap.Hutao.Model.Intrinsic;
using Snap.Hutao.Model.Primitive;

namespace Snap.Hutao.Web.Enka.Model;

/// <summary>
/// 角色信息
/// </summary>
public class AvatarInfo
{
    /// <summary>
    /// 角色Id
    /// Character ID
    /// </summary>
    [JsonPropertyName("avatarId")]
    public AvatarId AvatarId { get; set; }

    /// <summary>
    /// 基础属性
    /// Character Info Properties List
    /// <see cref="PlayerProperty.PROP_EXP"/>
    /// </summary>
    [JsonPropertyName("propMap")]
    [JsonConverter(typeof(StringEnumKeyDictionaryConverter))]
    public Dictionary<PlayerProperty, TypeValue> PropMap { get; set; } = default!;

    /// <summary>
    /// 命座 Id
    /// </summary>
    [JsonPropertyName("talentIdList")]
    public List<int>? TalentIdList { get; set; }

    /// <summary>
    /// 属性 Map
    /// Map of Character's Combat Properties.
    /// </summary>
    [JsonPropertyName("fightPropMap")]
    [JsonConverter(typeof(StringEnumKeyDictionaryConverter))]
    public Dictionary<FightProperty, double> FightPropMap { get; set; } = default!;

    /// <summary>
    /// 技能组Id
    /// Character Skill Set ID
    /// </summary>
    [JsonPropertyName("skillDepotId")]
    public int SkillDepotId { get; set; }

    /// <summary>
    /// List of Unlocked Skill Ids
    /// 被动天赋
    /// </summary>
    [JsonPropertyName("inherentProudSkillList")]
    public IList<int> InherentProudSkillList { get; set; } = default!;

    /// <summary>
    /// Map of Skill Levels
    /// </summary>
    [JsonPropertyName("skillLevelMap")]
    public Dictionary<string, int> SkillLevelMap { get; set; } = default!;

    /// <summary>
    /// 装备列表
    /// 最后一个为武器
    /// List of Equipments: Weapon, Artifacts
    /// </summary>
    [JsonPropertyName("equipList")]
    public List<Equip> EquipList { get; set; } = default!;

    /// <summary>
    /// 好感度信息
    /// Character Friendship Level
    /// </summary>
    [JsonPropertyName("fetterInfo")]
    public FetterInfo FetterInfo { get; set; } = default!;

    /// <summary>
    /// 皮肤 Id
    /// </summary>
    [JsonPropertyName("costumeId")]
    public int? CostumeId { get; set; }

    /// <summary>
    /// 命座额外技能等级
    /// </summary>
    [JsonPropertyName("proudSkillExtraLevelMap")]
    public Dictionary<string, int>? ProudSkillExtraLevelMap { get; set; } = default!;
}