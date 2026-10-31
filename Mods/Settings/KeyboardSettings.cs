// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class KeyboardSettings
{
    internal static int Style, Placement, Language, Appearance, Spacing, Numbers;
    internal static float Size = 1f;
    [MenuButton("Keyboard Style", ButtonType.Option, Category = "Settings", Order = 260, Options = new[] { "Classic", "Side Panel", "Compact", "Wide" }, Notify = false)]
    public static void SetStyle() { Style = ButtonManager.GetButton("Keyboard Style")!.OptionIndex; ChudOS.Core.Main.RefreshKeyboardLayout(); }
    [MenuButton("Keyboard Placement", ButtonType.Option, Category = "Settings", Order = 261, Options = new[] { "Right", "Left", "Below" }, Notify = false)]
    public static void SetPlacement() { Placement = ButtonManager.GetButton("Keyboard Placement")!.OptionIndex; ChudOS.Core.Main.RefreshKeyboardLayout(); }
    [MenuButton("Keyboard Size", ButtonType.Option, Category = "Settings", Order = 262, Options = new[] { "Normal", "Small", "Large" }, Notify = false)]
    public static void SetSize() { Size = new[] { 1f, 0.85f, 1.15f }[ButtonManager.GetButton("Keyboard Size")!.OptionIndex]; ChudOS.Core.Main.RefreshKeyboardLayout(); }
    [MenuButton("Keyboard Language", ButtonType.Option, Category = "Settings", Order = 266, Options = new[] { "English", "Portuguese", "Spanish", "French", "German", "Italian" }, Notify = false)]
    public static void SetLanguage() { Language = ButtonManager.GetButton("Keyboard Language")!.OptionIndex; ChudOS.Core.Main.RefreshKeyboardLayout(); }
    [MenuButton("Keyboard Keys", ButtonType.Option, Category = "Settings", Order = 267, Options = new[] { "Theme", "Accent", "Text Only" }, Notify = false)]
    public static void SetAppearance() { Appearance = ButtonManager.GetButton("Keyboard Keys")!.OptionIndex; ChudOS.Core.Main.RefreshKeyboardLayout(); }
    [MenuButton("Keyboard Spacing", ButtonType.Option, Category = "Settings", Order = 268, Options = new[] { "Normal", "Tight", "Wide" }, Notify = false)]
    public static void SetSpacing() { Spacing = ButtonManager.GetButton("Keyboard Spacing")!.OptionIndex; ChudOS.Core.Main.RefreshKeyboardLayout(); }
    [MenuButton("Keyboard Numbers", ButtonType.Option, Category = "Settings", Order = 269, Options = new[] { "Auto", "Show", "Hide" }, Notify = false)]
    public static void SetNumbers() { Numbers = ButtonManager.GetButton("Keyboard Numbers")!.OptionIndex; ChudOS.Core.Main.RefreshKeyboardLayout(); }
}
