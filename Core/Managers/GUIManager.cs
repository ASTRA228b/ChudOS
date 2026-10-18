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

using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace ChudOS.Core.Managers;

public static class GUIManager
{
    public static bool PCView;
    public static bool ArrayListEnabled = true;
    public static Key GUIInput = Key.LeftAlt;
    public static Key CloseAllInput = Key.LeftCtrl;
    public static readonly Key[] Inputs = { Key.LeftCtrl, Key.LeftAlt, Key.X, Key.DownArrow };
    private static readonly Dictionary<string, (Action<bool> Set, Func<bool> Get)> Windows = new();
    public static void Register(string name, Action<bool> setter, Func<bool> getter) => Windows[name] = (setter, getter);
    public static void Toggle(string name) { if (Windows.TryGetValue(name, out var window)) window.Set(!window.Get()); }
    public static void CloseAll() { foreach (var window in Windows.Values) window.Set(false); }
    public static void ToggleAll()
    {
        bool anyOpen = false;
        foreach (var window in Windows.Values) if (window.Get()) { anyOpen = true; break; }
        foreach (var window in Windows.Values) window.Set(!anyOpen);
    }
    public static void Tick()
    {
        Keyboard? keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard[CloseAllInput].wasPressedThisFrame) CloseAll();
        else if (GUIInput != CloseAllInput && keyboard[GUIInput].wasPressedThisFrame) ToggleAll();
    }
    public static void SetOpenKey(int index) => GUIInput = Inputs[UnityEngine.Mathf.Clamp(index, 0, Inputs.Length - 1)];
    public static void SetCloseKey(int index) => CloseAllInput = Inputs[UnityEngine.Mathf.Clamp(index, 0, Inputs.Length - 1)];
}
