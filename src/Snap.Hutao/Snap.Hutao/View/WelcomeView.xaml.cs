// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Snap.Hutao.ViewModel;

namespace Snap.Hutao.View;

/// <summary>
/// ��ӭ��ͼ
/// </summary>
public sealed partial class WelcomeView : UserControl
{
    private readonly IServiceScope serviceScope;

    /// <summary>
    /// ����һ���µĻ�ӭ��ͼ
    /// </summary>
    public WelcomeView()
    {
        InitializeComponent();
        serviceScope = Ioc.Default.CreateScope();
        DataContext = serviceScope.ServiceProvider.GetRequiredService<WelcomeViewModel>();
    }

    private void OnUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        DataContext = null;
        serviceScope.Dispose();
    }
}
