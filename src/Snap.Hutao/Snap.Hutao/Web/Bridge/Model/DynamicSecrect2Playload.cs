﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Web.Bridge.Model;

/// <summary>
/// DS2请求
/// </summary>
public class DynamicSecrect2Playload
{
    /// <summary>
    /// q
    /// </summary>
    [JsonPropertyName("query")]
    public Dictionary<string, JsonElement> Query { get; set; } = default!;

    /// <summary>
    /// b
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = default!;

    /// <summary>
    /// 获取排序后的的查询参数
    /// </summary>
    /// <returns>查询参数</returns>
    public string GetQueryParam()
    {
        IEnumerable<string> parts = Query.OrderBy(x => x.Key).Select(x =>
        {
            if (x.Value.ValueKind == JsonValueKind.True || x.Value.ValueKind == JsonValueKind.False)
            {
                return $"{x.Key}={x.Value.ToString().ToLowerInvariant()}";
            }

            return $"{x.Key}={x.Value}";
        });

        return string.Join('&', parts);
    }
}
