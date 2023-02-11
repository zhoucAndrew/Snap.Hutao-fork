// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Control;
using Snap.Hutao.ViewModel;

namespace Snap.Hutao.View.Page;

/// <summary>
/// ����ҳ��
/// </summary>
public sealed partial class TestPage : ScopedPage
{
    /// <summary>
    /// ����һ���µĲ���ҳ��
    /// </summary>
    public TestPage()
    {
        InitializeWith<TestViewModel>();
        InitializeComponent();
    }
}