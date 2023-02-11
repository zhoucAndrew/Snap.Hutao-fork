﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Intrinsic;

namespace Snap.Hutao.Model.Binding.AvatarProperty;

/// <summary>
/// 装备基类
/// </summary>
public class EquipBase : NameIconDescription
{
    /// <summary>
    /// 等级
    /// </summary>
    public string Level { get; set; } = default!;

    /// <summary>
    /// 品质
    /// </summary>
    public ItemQuality Quality { get; set; }

    /// <summary>
    /// 主属性
    /// </summary>
    public Pair<string, string> MainProperty { get; set; } = default!;
}
