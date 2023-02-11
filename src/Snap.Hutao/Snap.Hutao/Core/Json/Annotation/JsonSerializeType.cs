﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Core.Json.Annotation;

/// <summary>
/// Json 序列化类型
/// </summary>
public enum JsonSerializeType
{
    /// <summary>
    /// Int32
    /// </summary>
    Int32,

    /// <summary>
    /// 字符串包裹的数字
    /// </summary>
    Int32AsString,

    /// <summary>
    /// 名称字符串
    /// </summary>
    String,
}