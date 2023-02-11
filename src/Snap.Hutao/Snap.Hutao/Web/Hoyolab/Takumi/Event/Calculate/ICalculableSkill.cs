﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web.Hoyolab.Takumi.Event.Calculate;

/// <summary>
/// 可计算的技能
/// </summary>
public interface ICalculableSkill : ICalculable
{
    /// <summary>
    /// 技能组Id
    /// </summary>
    int GruopId { get; }

    /// <summary>
    /// 最小等级
    /// </summary>
    int LevelMin { get; }

    /// <summary>
    /// 最大等级
    /// </summary>
    int LevelMax { get; }
}