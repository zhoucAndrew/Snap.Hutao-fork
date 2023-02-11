// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Control;
using Snap.Hutao.ViewModel;

namespace Snap.Hutao.View.Page;

/// <summary>
/// ����ҳ��
/// </summary>
public sealed partial class CultivationPage : ScopedPage
{
    /// <summary>
    /// ����һ���µ�����ҳ��
    /// </summary>
    public CultivationPage()
    {
        InitializeWith<CultivationViewModel>();
        InitializeComponent();
    }
}
