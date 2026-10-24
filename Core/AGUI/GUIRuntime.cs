// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using UnityEngine;

namespace ChudOS.Core.AGUI;

internal sealed class GUIRuntime : MonoBehaviour
{
    private void OnGUI()
    {
        NotifyLib.InitToast();
        GUIManager.CreateGUIs();
    }
}
