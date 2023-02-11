﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Binding.AvatarProperty;
using Snap.Hutao.Web.Enka.Model;

namespace Snap.Hutao.Service.AvatarInfo.Factory;

/// <summary>
/// 简述工厂
/// </summary>
internal interface ISummaryFactory
{
    /// <summary>
    /// 异步创建简述对象
    /// </summary>
    /// <param name="playerInfo">玩家信息</param>
    /// <param name="avatarInfos">角色列表</param>
    /// <param name="token">取消令牌</param>
    /// <returns>简述对象</returns>
    Task<Summary> CreateAsync(PlayerInfo playerInfo, IEnumerable<Web.Enka.Model.AvatarInfo> avatarInfos, CancellationToken token);
}