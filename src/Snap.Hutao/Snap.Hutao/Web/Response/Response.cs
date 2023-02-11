﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Service.Abstraction;
using Snap.Hutao.Web.Bridge.Model;
using System.Runtime.CompilerServices;

namespace Snap.Hutao.Web.Response;

/// <summary>
/// 提供 <see cref="Response{T}"/> 的非泛型基类
/// </summary>
public class Response
{
    /// <summary>
    /// 构造一个新的响应
    /// </summary>
    /// <param name="returnCode">返回代码</param>
    /// <param name="message">消息</param>
    [JsonConstructor]
    public Response(int returnCode, string message)
    {
        ReturnCode = returnCode;
        Message = message;
#if DEBUG
        Ioc.Default.GetRequiredService<ILogger<Response>>().LogInformation("Response [{resp}]", ToString());
#endif
    }

    /// <summary>
    /// 返回代码
    /// </summary>
    [JsonPropertyName("retcode")]
    public int ReturnCode { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    /// <summary>
    /// 返回本体或带有消息提示的默认值
    /// </summary>
    /// <typeparam name="TData">类型</typeparam>
    /// <param name="response">本体</param>
    /// <param name="callerName">调用方法名称</param>
    /// <returns>本体或默认值，当本体为 null 时 返回默认值</returns>
    public static Response<TData> DefaultIfNull<TData>(Response<TData>? response, [CallerMemberName] string callerName = default!)
    {
        // 0x26F19335 is a magic number that hashed from "Snap.Hutao"
        return response ?? new(0x26F19335, $"[{callerName}] 中的 [{typeof(TData).Name}] 请求异常", default);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Format(SH.WebResponseFormat, ReturnCode, Message);
    }
}

/// <summary>
/// Mihoyo 标准API响应
/// </summary>
/// <typeparam name="TData">数据类型</typeparam>
[SuppressMessage("", "SA1402")]
public class Response<TData> : Response, IJsResult
{
    /// <summary>
    /// 构造一个新的 Mihoyo 标准API响应
    /// </summary>
    /// <param name="returnCode">返回代码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    [JsonConstructor]
    public Response(int returnCode, string message, TData? data)
        : base(returnCode, message)
    {
        Data = data;
    }

    /// <summary>
    /// 数据
    /// </summary>
    [JsonPropertyName("data")]
    public TData? Data { get; set; }

    /// <summary>
    /// 响应是否正常
    /// </summary>
    /// <param name="response">响应</param>
    /// <returns>是否Ok</returns>
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsOk()
    {
        if (ReturnCode == 0)
        {
#pragma warning disable CS8775
            return true;
#pragma warning restore CS8775
        }
        else
        {
            Ioc.Default.GetRequiredService<IInfoBarService>().Error(ToString());
            return false;
        }
    }

    /// <inheritdoc/>
    string IJsResult.ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}