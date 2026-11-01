// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using UnityEngine;

namespace ChudOS.Libraries;

internal abstract class ExpansionWindowFeature : BetaFeature
{
    private Rect window = new(90, 90, 430, 430);
    private bool visible;
    private string windowKey = "", title = "";
    private int windowId;

    protected void OpenWindow(int id, string name)
    {
        windowId = id; title = name; windowKey = "Expansion " + Mode; visible = true;
        GUIManager.Register(windowKey, value => visible = value, () => visible && this != null && enabled, CreateGUI, () => window);
    }
    private void CreateGUI()
    {
        if (visible && enabled) window = MenuHelper.MakeWindow(windowId, window, Window, title);
    }
    protected virtual void Window(int id) => GUI.DragWindow();
    protected override void Cleanup()
    {
        if (windowKey.Length > 0) GUIManager.Unregister(windowKey);
    }
}
