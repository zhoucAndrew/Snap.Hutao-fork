// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Snap.Hutao.Model.Binding.User;
using Snap.Hutao.Service.User;
using Snap.Hutao.Web.Bridge;

namespace Snap.Hutao.View.Dialog;

/// <summary>
/// ������Ϸ��¼�Ի���
/// </summary>
public sealed partial class CommunityGameRecordDialog : ContentDialog
{
    private readonly IServiceScope scope;
    [SuppressMessage("", "IDE0052")]
    private MiHoYoJSInterface? jsInterface;

    /// <summary>
    /// ����һ���µ�������Ϸ��¼�Ի���
    /// </summary>
    /// <param name="window">����</param>
    public CommunityGameRecordDialog()
    {
        InitializeComponent();
        scope = Ioc.Default.CreateScope();
        XamlRoot = scope.ServiceProvider.GetRequiredService<MainWindow>().Content.XamlRoot;
    }

    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        InitializeAsync().SafeForget();
    }

    private async Task InitializeAsync()
    {
        await WebView.EnsureCoreWebView2Async();
        CoreWebView2 coreWebView2 = WebView.CoreWebView2;
        User? user = scope.ServiceProvider.GetRequiredService<IUserService>().Current;

        if (user == null)
        {
            return;
        }

        coreWebView2.SetCookie(user.CookieToken, user.Ltoken, null).SetMobileUserAgent();
        jsInterface = new(coreWebView2, scope.ServiceProvider);
        coreWebView2.Navigate("https://webstatic.mihoyo.com/app/community-game-records/index.html");
    }

    private void OnContentDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        jsInterface = null;
        scope.Dispose();
    }
}
