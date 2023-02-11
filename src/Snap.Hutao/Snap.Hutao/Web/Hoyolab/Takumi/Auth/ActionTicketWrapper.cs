﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Web.Hoyolab.Passport;

namespace Snap.Hutao.Web.Hoyolab.Takumi.Auth;

/// <summary>
/// 操作凭证包装器
/// </summary>
public class ActionTicketWrapper
{
    /// <summary>
    /// 凭证
    /// </summary>
    [JsonPropertyName("ticket")]
    public string Ticket { get; set; } = default!;

    /// <summary>
    /// 是否验证
    /// </summary>
    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; }

    /// <summary>
    /// 账户信息
    /// </summary>
    [JsonPropertyName("account_info")]
    public UserInformation AccountInfo { get; set; } = default!;
}
