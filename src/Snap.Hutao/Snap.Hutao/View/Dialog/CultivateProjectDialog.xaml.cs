// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Snap.Hutao.Model.Entity;
using Snap.Hutao.Service.User;

namespace Snap.Hutao.View.Dialog;

/// <summary>
/// ���ɼƻ��Ի���
/// </summary>
public sealed partial class CultivateProjectDialog : ContentDialog
{
    /// <summary>
    /// ����һ���µ����ɼƻ��Ի���
    /// </summary>
    /// <param name="window">����</param>
    public CultivateProjectDialog()
    {
        InitializeComponent();
        XamlRoot = Ioc.Default.GetRequiredService<MainWindow>().Content.XamlRoot;
    }

    /// <summary>
    /// ����һ���µģ��û�ָ���ļƻ�
    /// </summary>
    /// <returns>�ƻ�</returns>
    public async ValueTask<ValueResult<bool, CultivateProject>> CreateProjectAsync()
    {
        await ThreadHelper.SwitchToMainThreadAsync();
        ContentDialogResult result = await ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            string text = InputText.Text;
            string? uid = AttachUidBox.IsChecked == true
                ? Ioc.Default.GetRequiredService<IUserService>().Current?.SelectedUserGameRole?.GameUid
                : null;

            CultivateProject project = CultivateProject.Create(text, uid);
            return new(true, project);
        }

        return new(false, null!);
    }
}
