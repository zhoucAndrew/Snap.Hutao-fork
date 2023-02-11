﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Model.Metadata;

/// <summary>
/// 等级与参数
/// </summary>
/// <typeparam name="TLevel">等级的类型</typeparam>
/// <typeparam name="TParam">参数的类型</typeparam>
public class LevelParam<TLevel, TParam>
{
    /// <summary>
    /// 默认的构造器
    /// </summary>
    [JsonConstructor]
    public LevelParam()
    {
    }

    /// <summary>
    /// 构造一个新的等级与参数
    /// </summary>
    /// <param name="level">等级</param>
    /// <param name="parameters">参数</param>
    public LevelParam(TLevel level, IList<TParam> parameters)
    {
        Level = level;
        Parameters = parameters;
    }

    /// <summary>
    /// 等级
    /// </summary>
    public TLevel Level { get; set; } = default!;

    /// <summary>
    /// 参数
    /// </summary>
    public IList<TParam> Parameters { get; set; } = default!;
}