﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Core.Json.Converter;

/// <summary>
/// 逗号分隔列表转换器
/// </summary>
internal class SeparatorCommaInt32EnumerableConverter : JsonConverter<IEnumerable<int>>
{
    /// <inheritdoc/>
    public override IEnumerable<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? source = reader.GetString();
        IEnumerable<int>? ids = source?.Split(',').Select(int.Parse);
        return ids ?? Enumerable.Empty<int>();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IEnumerable<int> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(string.Join(',', value));
    }
}