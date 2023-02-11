﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Web.Hoyolab;
using Snap.Hutao.Web.Hoyolab.Takumi.Binding;
using System.Collections.ObjectModel;
using BindingUser = Snap.Hutao.Model.Binding.User.User;

namespace Snap.Hutao.Service.User;

/// <summary>
/// 用户服务
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 获取或设置当前用户
    /// </summary>
    BindingUser? Current { get; set; }

    /// <summary>
    /// 异步获取角色与用户集合
    /// </summary>
    /// <returns>角色与用户集合</returns>
    Task<ObservableCollection<Model.Binding.User.UserAndUid>> GetRoleCollectionAsync();

    /// <summary>
    /// 初始化用户服务及所有用户
    /// 异步获取同步的用户信息集合
    /// 对集合的操作应通过服务抽象完成
    /// 此操作不能取消
    /// </summary>
    /// <returns>准备完成的用户信息集合</returns>
    Task<ObservableCollection<BindingUser>> GetUserCollectionAsync();

    /// <summary>
    /// 获取角色信息
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>对应的角色信息</returns>
    UserGameRole? GetUserGameRoleByUid(string uid);

    /// <summary>
    /// 尝试异步处理输入的Cookie
    /// </summary>
    /// <param name="cookie">Cookie</param>
    /// <returns>处理的结果</returns>
    Task<ValueResult<UserOptionResult, string>> ProcessInputCookieAsync(Cookie cookie);

    /// <summary>
    /// 异步移除用户
    /// </summary>
    /// <param name="user">待移除的用户</param>
    /// <returns>任务</returns>
    Task RemoveUserAsync(BindingUser user);
}