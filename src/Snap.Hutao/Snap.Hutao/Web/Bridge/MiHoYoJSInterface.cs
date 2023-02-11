﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using Snap.Hutao.Core.Database;
using Snap.Hutao.Extension;
using Snap.Hutao.Model.Binding.User;
using Snap.Hutao.Model.Entity.Database;
using Snap.Hutao.Service.User;
using Snap.Hutao.Web.Bridge.Model;
using Snap.Hutao.Web.Hoyolab;
using Snap.Hutao.Web.Hoyolab.Bbs.User;
using Snap.Hutao.Web.Hoyolab.DynamicSecret;
using Snap.Hutao.Web.Hoyolab.Passport;
using Snap.Hutao.Web.Hoyolab.Takumi.Auth;
using System.Runtime.InteropServices;
using System.Text;

namespace Snap.Hutao.Web.Bridge;

/// <summary>
/// 调用桥
/// </summary>
[SuppressMessage("", "CA1001")]
[SuppressMessage("", "SA1600")]
public class MiHoYoJSInterface
{
    private const string InitializeJsInterfaceScript2 = """
        window.MiHoYoJSInterface = {
            postMessage: function(arg) { chrome.webview.postMessage(arg) },
            closePage: function() { this.postMessage('{"method":"closePage"}') },
        };
        """;

    private const string HideScrollBarScript = """
        let st = document.createElement('style');
        st.innerHTML = '::-webkit-scrollbar{display:none}';
        document.querySelector('body').appendChild(st);
        """;

    private readonly CoreWebView2 webView;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<MiHoYoJSInterface> logger;
    private readonly SemaphoreSlim webMessageSemaphore = new(1);

    /// <summary>
    /// 构造一个新的调用桥
    /// </summary>
    /// <param name="webView">webview2</param>
    /// <param name="serviceProvider">服务提供器</param>
    public MiHoYoJSInterface(CoreWebView2 webView, IServiceProvider serviceProvider)
    {
        this.webView = webView;
        this.serviceProvider = serviceProvider;

        logger = serviceProvider.GetRequiredService<ILogger<MiHoYoJSInterface>>();

        webView.WebMessageReceived += OnWebMessageReceived;
        webView.DOMContentLoaded += OnDOMContentLoaded;
        webView.NavigationStarting += OnNavigationStarting;
    }

    public event Action? ClosePageRequested;

    /// <summary>
    /// 获取ActionTicket
    /// </summary>
    /// <param name="jsParam">参数</param>
    /// <returns>响应</returns>
    public virtual async Task<IJsResult?> GetActionTicketAsync(JsParam<ActionTypePayload> jsParam)
    {
        User user = serviceProvider.GetRequiredService<IUserService>().Current!;
        return await serviceProvider
            .GetRequiredService<AuthClient>()
            .GetActionTicketByStokenAsync(jsParam.Payload!.ActionType, user.Entity)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 获取Http请求头
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>Http请求头</returns>
    public virtual JsResult<Dictionary<string, string>> GetHttpRequestHeader(JsParam param)
    {
        return new()
        {
            Data = new Dictionary<string, string>()
            {
                { "x-rpc-client_type", "5" },
                { "x-rpc-device_id",  Core.CoreEnvironment.HoyolabDeviceId },
                { "x-rpc-app_version", Core.CoreEnvironment.HoyolabXrpcVersion },
            },
        };
    }

    /// <summary>
    /// 异步获取账户信息
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>响应</returns>
    public virtual JsResult<Dictionary<string, string>> GetCookieInfo(JsParam param)
    {
        User user = serviceProvider.GetRequiredService<IUserService>().Current!;

        return new()
        {
            Data = new()
            {
                [Cookie.LTUID] = user.Ltoken![Cookie.LTUID],
                [Cookie.LTOKEN] = user.Ltoken[Cookie.LTOKEN],
                [Cookie.LOGIN_TICKET] = string.Empty,
            },
        };
    }

    /// <summary>
    /// 获取1代动态密钥
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>响应</returns>
    public virtual JsResult<Dictionary<string, string>> GetDynamicSecrectV1(JsParam param)
    {
        string salt = Core.CoreEnvironment.DynamicSecrets[SaltType.LK2];
        long t = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string r = GetRandomString();
        string check = Core.Convert.ToMd5HexString($"salt={salt}&t={t}&r={r}").ToLowerInvariant();

        return new() { Data = new() { ["DS"] = $"{t},{r},{check}", }, };

        static string GetRandomString()
        {
            const string RandomRange = "abcdefghijklmnopqrstuvwxyz1234567890";

            StringBuilder sb = new(6);

            for (int i = 0; i < 6; i++)
            {
                int pos = Random.Shared.Next(0, RandomRange.Length);
                sb.Append(RandomRange[pos]);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// 获取2代动态密钥
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>响应</returns>
    public virtual JsResult<Dictionary<string, string>> GetDynamicSecrectV2(JsParam<DynamicSecrect2Playload> param)
    {
        string salt = Core.CoreEnvironment.DynamicSecrets[SaltType.X4];
        long t = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int r = GetRandom();
        string b = param.Payload.Body;
        string q = param.Payload.GetQueryParam();
        string check = Core.Convert.ToMd5HexString($"salt={salt}&t={t}&r={r}&b={b}&q={q}").ToLowerInvariant();

        return new() { Data = new() { ["DS"] = $"{t},{r},{check}" } };

        static int GetRandom()
        {
            int rand = Random.Shared.Next(100000, 200000);
            return rand == 100000 ? 642367 : rand;
        }
    }

    /// <summary>
    /// 获取用户基本信息
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>响应</returns>
    public virtual JsResult<Dictionary<string, object>> GetUserInfo(JsParam param)
    {
        User user = serviceProvider.GetRequiredService<IUserService>().Current!;
        UserInfo info = user.UserInfo!;

        return new()
        {
            Data = new()
            {
                ["id"] = info.Uid,
                ["gender"] = info.Gender,
                ["nickname"] = info.Nickname,
                ["introduce"] = info.Introduce,
                ["avatar_url"] = info.AvatarUrl,
            },
        };
    }

    /// <summary>
    /// 获取CookieToken
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>响应</returns>
    public virtual async Task<JsResult<Dictionary<string, string>>> GetCookieTokenAsync(JsParam<CookieTokenPayload> param)
    {
        User user = serviceProvider.GetRequiredService<IUserService>().Current!;
        string cookieToken = string.Empty;
        if (param.Payload!.ForceRefresh)
        {
            Response.Response<UidCookieToken> cookieTokenResponse = await Ioc.Default
                .GetRequiredService<PassportClient2>()
                .GetCookieAccountInfoBySTokenAsync(user.Entity, default)
                .ConfigureAwait(false);
            if (cookieTokenResponse.IsOk())
            {
                cookieToken = cookieTokenResponse.Data.CookieToken;
            }

            // Check null and create a new one to avoid System.NullReferenceException
            user.CookieToken ??= new();

            // sync ui and database
            user.CookieToken[Cookie.COOKIE_TOKEN] = cookieToken!;
            serviceProvider.GetRequiredService<AppDbContext>().Users.UpdateAndSave(user.Entity);
        }
        else
        {
            cookieToken = user.CookieToken![Cookie.COOKIE_TOKEN];
        }

        return new() { Data = new() { [Cookie.COOKIE_TOKEN] = cookieToken! } };
    }

    /// <summary>
    /// 关闭
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>响应</returns>
    public virtual async Task<IJsResult?> ClosePageAsync(JsParam param)
    {
        await ThreadHelper.SwitchToMainThreadAsync();
        if (webView.CanGoBack)
        {
            webView.GoBack();
        }
        else
        {
            ClosePageRequested?.Invoke();
        }

        return null;
    }

    /// <summary>
    /// 调整分享设置
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>响应</returns>
    public virtual IJsResult? ConfigureShare(JsParam param)
    {
        return null;
    }

    /// <summary>
    /// 获取状态栏高度
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns>结果</returns>
    public virtual JsResult<Dictionary<string, object>> GetStatusBarHeight(JsParam param)
    {
        return new() { Data = new() { ["statusBarHeight"] = 0 } };
    }

    public virtual async Task<IJsResult?> PushPageAsync(JsParam<PushPagePayload> param)
    {
        await ThreadHelper.SwitchToMainThreadAsync();
        webView.Navigate(param.Payload.Page);
        return null;
    }

    public virtual Task<IJsResult?> ShowAlertDialogAsync(JsParam param)
    {
        return Task.FromException<IJsResult?>(new NotImplementedException());
    }

    public virtual IJsResult? StartRealPersonValidation(JsParam param)
    {
        throw new NotImplementedException();
    }

    public virtual IJsResult? StartRealnameAuth(JsParam param)
    {
        throw new NotImplementedException();
    }

    public virtual IJsResult? GenAuthKey(JsParam param)
    {
        throw new NotImplementedException();
    }

    public virtual IJsResult? GenAppAuthKey(JsParam param)
    {
        throw new NotImplementedException();
    }

    public virtual IJsResult? OpenSystemBrowser(JsParam param)
    {
        throw new NotImplementedException();
    }

    public virtual IJsResult? SaveLoginTicket(JsParam param)
    {
        throw new NotImplementedException();
    }

    public virtual Task<IJsResult?> GetNotificationSettingsAsync(JsParam param)
    {
        throw new NotImplementedException();
    }

    public virtual IJsResult? ShowToast(JsParam param)
    {
        throw new NotImplementedException();
    }

    private async Task<string> ExecuteCallbackScriptAsync(string callback, string? payload = null)
    {
        if (string.IsNullOrEmpty(callback))
        {
            // prevent executing this callback
            return string.Empty;
        }

        string js = new StringBuilder()
            .Append("javascript:mhyWebBridge(")
            .Append('"')
            .Append(callback)
            .Append('"')
            .AppendIf(payload != null, ',')
            .Append(payload)
            .Append(')')
            .ToString();

        logger?.LogInformation("[ExecuteScript: {callback}]\n{payload}", callback, payload);

        await ThreadHelper.SwitchToMainThreadAsync();
        try
        {
            return await webView.ExecuteScriptAsync(js);
        }
        catch (COMException)
        {
            // COMException (0x8007139F): 组或资源的状态不是执行请求操作的正确状态。 (0x8007139F)
            // webview is disposing or disposed
            return string.Empty;
        }
    }

    [SuppressMessage("", "VSTHRD100")]
    private async void OnWebMessageReceived(CoreWebView2 webView2, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string message = args.TryGetWebMessageAsString();
        logger.LogInformation("[OnRawMessage]\n{message}", message);
        JsParam param = JsonSerializer.Deserialize<JsParam>(message)!;

        logger.LogInformation("[OnMessage]\nMethod  : {method}\nPayload : {payload}\nCallback: {callback}", param.Method, param.Payload, param.Callback);
        using (await webMessageSemaphore.EnterAsync().ConfigureAwait(false))
        {
            IJsResult? result = await TryGetJsResultFromJsParamAsync(param).ConfigureAwait(false);

            if (result != null && param.Callback != null)
            {
                await ExecuteCallbackScriptAsync(param.Callback, result.ToString()).ConfigureAwait(false);
            }
        }
    }

    private IJsResult? LogUnhandledMessage([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message, params object?[] param)
    {
        logger.LogWarning(message, param);
        return default;
    }

    private async Task<IJsResult?> TryGetJsResultFromJsParamAsync(JsParam param)
    {
        try
        {
            return param.Method switch
            {
                "closePage" => await ClosePageAsync(param).ConfigureAwait(false),
                "configure_share" => ConfigureShare(param),
                "eventTrack" => null,
                "getActionTicket" => await GetActionTicketAsync(param).ConfigureAwait(false),
                "getCookieInfo" => GetCookieInfo(param),
                "getCookieToken" => await GetCookieTokenAsync(param).ConfigureAwait(false),
                "getDS" => GetDynamicSecrectV1(param),
                "getDS2" => GetDynamicSecrectV2(param),
                "getHTTPRequestHeaders" => GetHttpRequestHeader(param),
                "getStatusBarHeight" => GetStatusBarHeight(param),
                "getUserInfo" => GetUserInfo(param),
                "hideLoading" => null,
                "login" => null,
                "pushPage" => await PushPageAsync(param).ConfigureAwait(false),
                "showLoading" => null,
                _ => LogUnhandledMessage("Unhandled Message Type: {method}", param.Method),
            };
        }
        catch (ObjectDisposedException)
        {
            // The dialog is already closed.
            return null;
        }
    }

    private void OnDOMContentLoaded(CoreWebView2 coreWebView2, CoreWebView2DOMContentLoadedEventArgs args)
    {
        coreWebView2.ExecuteScriptAsync(HideScrollBarScript).AsTask().SafeForget(logger);
    }

    private void OnNavigationStarting(CoreWebView2 coreWebView2, CoreWebView2NavigationStartingEventArgs args)
    {
        if (new Uri(args.Uri).Host.EndsWith("mihoyo.com"))
        {
            // Execute this solve issue: When open same site second time,there might be no bridge init.
            coreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(InitializeJsInterfaceScript2).AsTask().SafeForget(logger);
            coreWebView2.ExecuteScriptAsync(InitializeJsInterfaceScript2).AsTask().SafeForget(logger);
        }
    }
}