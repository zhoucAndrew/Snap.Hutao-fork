﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Web.WebView2.Core;
using Snap.Hutao.Web.Hoyolab.Passport;

namespace Snap.Hutao.Web.Hoyolab;

/// <summary>
/// 封装了米哈游的Cookie
/// </summary>
public partial class Cookie
{
    private readonly SortedDictionary<string, string> inner;

    /// <summary>
    /// 构造一个空白的Cookie
    /// </summary>
    public Cookie()
        : this(new())
    {
    }

    /// <summary>
    /// 构造一个新的Cookie
    /// </summary>
    /// <param name="dict">源</param>
    private Cookie(SortedDictionary<string, string> dict)
    {
        inner = dict;
    }

    public string this[string key]
    {
        get => inner[key];
        set => inner[key] = value;
    }

    /// <summary>
    /// 解析Cookie字符串
    /// </summary>
    /// <param name="cookieString">cookie字符串</param>
    /// <returns>新的Cookie对象</returns>
    public static Cookie Parse(string cookieString)
    {
        SortedDictionary<string, string> cookieMap = new();
        cookieString = cookieString.Replace(" ", string.Empty);
        string[] values = cookieString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (string[] parts in values.Select(c => c.Split('=', 2)))
        {
            string name = parts[0].Trim();
            string value = parts.Length == 1 ? string.Empty : parts[1].Trim();

            // System.ArgumentException: An item with the same key has already been added.
            // cookieMap.Add(name, value);
            cookieMap[name] = value;
        }

        return new(cookieMap);
    }

    public static Cookie FromCoreWebView2Cookies(IReadOnlyList<CoreWebView2Cookie> webView2Cookies)
    {
        SortedDictionary<string, string> cookieMap = new();

        foreach (CoreWebView2Cookie cookie in webView2Cookies)
        {
            cookieMap.Add(cookie.Name, cookie.Value);
        }

        return new(cookieMap);
    }

    /// <summary>
    /// 从登录结果创建Cookie
    /// </summary>
    /// <param name="loginResult">登录结果</param>
    /// <returns>Cookie</returns>
    public static Cookie FromLoginResult(LoginResult? loginResult)
    {
        if (loginResult == null)
        {
            return new();
        }

        SortedDictionary<string, string> cookieMap = new()
        {
            [STUID] = loginResult.UserInfo.Aid,
            [STOKEN] = loginResult.Token.Token,
            [MID] = loginResult.UserInfo.Mid,
        };

        return new(cookieMap);
    }

    public bool TryGetAsStoken([NotNullWhen(true)] out Cookie? cookie)
    {
        bool hasMid = TryGetValue(MID, out string? mid);
        bool hasStoken = TryGetValue(STOKEN, out string? stoken);
        bool hasStuid = TryGetValue(STUID, out string? stuid);

        if (hasMid && hasStoken && hasStuid)
        {
            cookie = new Cookie(new()
            {
                [MID] = mid!,
                [STOKEN] = stoken!,
                [STUID] = stuid!,
            });

            return true;
        }

        cookie = null;
        return false;
    }

    public bool TryGetAsLtoken([NotNullWhen(true)] out Cookie? cookie)
    {
        bool hasLtoken = TryGetValue(LTOKEN, out string? ltoken);
        bool hasStuid = TryGetValue(LTUID, out string? ltuid);

        if (hasLtoken && hasStuid)
        {
            cookie = new Cookie(new()
            {
                [LTOKEN] = ltoken!,
                [LTUID] = ltuid!,
            });

            return true;
        }

        cookie = null;
        return false;
    }

    public bool TryGetAsCookieToken([NotNullWhen(true)] out Cookie? cookie)
    {
        bool hasAccountId = TryGetValue(ACCOUNT_ID, out string? accountId);
        bool hasCookieToken = TryGetValue(COOKIE_TOKEN, out string? cookieToken);

        if (hasAccountId && hasCookieToken)
        {
            cookie = new Cookie(new()
            {
                [ACCOUNT_ID] = accountId!,
                [COOKIE_TOKEN] = cookieToken!,
            });

            return true;
        }

        cookie = null;
        return false;
    }

    /// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        return inner.TryGetValue(key, out value);
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>值或默认值</returns>
    public string? GetValueOrDefault(string key)
    {
        return inner.GetValueOrDefault(key);
    }

    /// <summary>
    /// 转换为Cookie的字符串表示
    /// </summary>
    /// <returns>Cookie的字符串表示</returns>
    public override string ToString()
    {
        return string.Join(';', inner.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }
}