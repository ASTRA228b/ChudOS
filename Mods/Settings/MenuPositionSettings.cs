// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class MenuPositionSettings
{
    internal static int Mode;
    internal static float Distance, Height;

    [MenuButton("Menu Alignment", ButtonType.Option, Category = "Settings", Order = 240, Options = new[] { "Auto", "Align To Palm", "Fixed" }, Notify = false)]
    public static void Alignment() { Mode = ButtonManager.GetButton("Menu Alignment")!.OptionIndex; ChudOS.Core.Main.ApplyMenuPosition(); }
    [MenuButton("Menu Distance", ButtonType.Option, Category = "Settings", Order = 241, Options = new[] { "Default", "Close", "Far" }, Notify = false)]
    public static void SetDistance() { Distance = new[] { 0f, -0.03f, 0.03f }[ButtonManager.GetButton("Menu Distance")!.OptionIndex]; ChudOS.Core.Main.ApplyMenuPosition(); }
    [MenuButton("Menu Height", ButtonType.Option, Category = "Settings", Order = 242, Options = new[] { "Default", "Lower", "Higher" }, Notify = false)]
    public static void SetHeight() { Height = new[] { 0f, -0.03f, 0.03f }[ButtonManager.GetButton("Menu Height")!.OptionIndex]; ChudOS.Core.Main.ApplyMenuPosition(); }
    [MenuButton("Recenter Menu", ButtonType.Press, Category = "Settings", Order = 243, Notify = false)]
    public static void Recenter() => ChudOS.Core.Main.ApplyMenuPosition();
    [MenuButton("Reset Menu Position", ButtonType.Press, Category = "Settings", Order = 244, Notify = false)]
    public static void Reset()
    {
        foreach (string name in new[] { "Menu Alignment", "Menu Distance", "Menu Height" })
        {
            var button = ButtonManager.GetButton(name);
            if (button != null) ButtonManager.ChangeOption(button, -button.OptionIndex);
        }
        Mode = 0; Distance = Height = 0f;
        Recenter();
    }
}
