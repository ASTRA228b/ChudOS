// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using UnityEngine;
using ChudOS.Core.Managers;

namespace ChudOS.Core.AGUI;

public static class ArrayListGUI
{
    private static GUIStyle? Something;
    private static string[] Names = Array.Empty<string>();
    private static readonly GUIContent Header = new();
    public static bool HasEntries => GUIManager.ArrayListEnabled && Names.Length > 0;

    public static void Initialize()
    {
        ButtonManager.OnButtonsChanged -= Refresh;
        ButtonManager.OnButtonsChanged += Refresh;
        Refresh();
    }
    private static void Refresh()
    {
        Names = ButtonManager.GetEnabledNames().ToArray();
        Header.text = $"ChudOS | {Names.Length} ACTIVE";
    }
    public static void Shutdown()
    {
        ButtonManager.OnButtonsChanged -= Refresh;
        Names = Array.Empty<string>();
        Something = null;
    }
    public static void CreateArray()
    {
        if (!HasEntries || Event.current.type != EventType.Repaint) return;
        Something ??= new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight };
        Something.normal.textColor = ChudOS.Core.Main.PrimaryColorValue;
        GUI.Label(new Rect(Screen.width - 300, 12, 280, 25), Header, Something);
        for (int i = 0; i < Names.Length; i++) GUI.Label(new Rect(Screen.width - 300, 41 + i * 21, 280, 22), Names[i], Something);
    }
}
