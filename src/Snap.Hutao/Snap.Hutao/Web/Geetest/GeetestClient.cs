﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.DependencyInjection.Annotation.HttpClient;
using Snap.Hutao.Web.Hoyolab.Takumi.GameRecord;
using System.Net.Http;

namespace Snap.Hutao.Web.Geetest;

/// <summary>
/// 极验客户端
/// </summary>
[HttpClient(HttpClientConfigration.Default)]
internal class GeetestClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions options;

    /// <summary>
    /// 构造一个新的极验客户端
    /// </summary>
    /// <param name="httpClient">http客户端</param>
    /// <param name="options">Json 序列化选项</param>
    public GeetestClient(HttpClient httpClient, JsonSerializerOptions options)
    {
        this.httpClient = httpClient;
        this.options = options;
    }

    /// <summary>
    /// 获取gt类型
    /// </summary>
    /// <param name="gt">gt</param>
    /// <returns>类型</returns>
    public async Task<GeetestResult<JsonElement>?> GetTypeAsync(string gt)
    {
        string raw = await httpClient.GetStringAsync(ApiEndpoints.GeetestGetType(gt)).ConfigureAwait(false);
        raw = raw[0] == '(' ? raw[1..^1] : raw; // remove surrounded ( )
        return JsonSerializer.Deserialize<GeetestResult<JsonElement>>(raw, options);
    }

    /// <summary>
    /// 获取验证方式
    /// 概率获取validate
    /// </summary>
    /// <param name="gt">gt</param>
    /// <param name="challenge">验证流水号</param>
    /// <returns>验证方式</returns>
    public async Task<GeetestResult<GeetestData>?> GetAjaxAsync(string gt, string challenge)
    {
        string raw = await httpClient.GetStringAsync(ApiEndpoints.GeetestAjax(gt, challenge)).ConfigureAwait(false);
        raw = raw[0] == '(' ? raw[1..^1] : raw; // remove surrounded ( )
        return JsonSerializer.Deserialize<GeetestResult<GeetestData>>(raw, options);
    }

    /// <summary>
    /// 获取验证方式
    /// 概率获取validate
    /// </summary>
    /// <param name="registration">验证注册</param>
    /// <returns>验证方式</returns>
    public async Task<GeetestResult<GeetestData>?> GetAjaxAsync(VerificationRegistration registration)
    {
        try
        {
            return await GetAjaxAsync(registration.Gt, registration.Challenge).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
