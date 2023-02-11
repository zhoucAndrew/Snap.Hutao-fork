﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace Snap.Hutao.Core.Windowing;

/// <summary>
/// 窗体子类管理器
/// </summary>
/// <typeparam name="TWindow">窗体类型</typeparam>
internal class WindowSubclassManager<TWindow> : IDisposable
    where TWindow : Window, IExtendedWindowSource
{
    private const int WindowSubclassId = 101;
    private const int DragBarSubclassId = 102;

    private readonly TWindow window;
    private readonly HWND hwnd;
    private readonly bool isLegacyDragBar;
    private HWND hwndDragBar;

    // We have to explictly hold a reference to SUBCLASSPROC
    private SUBCLASSPROC? windowProc;
    private SUBCLASSPROC? dragBarProc;

    /// <summary>
    /// 构造一个新的窗体子类管理器
    /// </summary>
    /// <param name="window">窗体实例</param>
    /// <param name="hwnd">窗体句柄</param>
    /// <param name="isLegacyDragBar">是否为经典标题栏区域</param>
    public WindowSubclassManager(TWindow window, HWND hwnd, bool isLegacyDragBar)
    {
        this.window = window;
        this.hwnd = hwnd;
        this.isLegacyDragBar = isLegacyDragBar;
    }

    /// <summary>
    /// 尝试设置窗体子类
    /// </summary>
    /// <returns>是否设置成功</returns>
    public unsafe bool TrySetWindowSubclass()
    {
        windowProc = new(OnSubclassProcedure);
        bool windowHooked = SetWindowSubclass(hwnd, windowProc, WindowSubclassId, 0);

        bool titleBarHooked = true;

        // only hook up drag bar proc when not use legacy Window.ExtendsContentIntoTitleBar
        if (isLegacyDragBar)
        {
            titleBarHooked = false;
            hwndDragBar = FindWindowEx(hwnd, default, "DRAG_BAR_WINDOW_CLASS", string.Empty);

            if (!hwndDragBar.IsNull)
            {
                dragBarProc = new(OnDragBarProcedure);
                titleBarHooked = SetWindowSubclass(hwndDragBar, dragBarProc, DragBarSubclassId, 0);
            }
        }

        return windowHooked && titleBarHooked;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        RemoveWindowSubclass(hwnd, windowProc, WindowSubclassId);
        if (isLegacyDragBar)
        {
            RemoveWindowSubclass(hwnd, dragBarProc, DragBarSubclassId);
        }

        windowProc = null;
        dragBarProc = null;
    }

    private unsafe LRESULT OnSubclassProcedure(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
    {
        switch (uMsg)
        {
            case WM_GETMINMAXINFO:
                {
                    double scalingFactor = Persistence.GetScaleForWindowHandle(hwnd);
                    window.ProcessMinMaxInfo((MINMAXINFO*)lParam.Value, scalingFactor);
                    break;
                }

            case WM_NCRBUTTONDOWN:
            case WM_NCRBUTTONUP:
                {
                    return new(0);
                }
        }

        return DefSubclassProc(hwnd, uMsg, wParam, lParam);
    }

    private LRESULT OnDragBarProcedure(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
    {
        switch (uMsg)
        {
            case WM_NCRBUTTONDOWN:
            case WM_NCRBUTTONUP:
                {
                    return new(0);
                }
        }

        return DefSubclassProc(hwnd, uMsg, wParam, lParam);
    }
}