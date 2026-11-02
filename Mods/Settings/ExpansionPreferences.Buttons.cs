// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static partial class ExpansionPreferences
{
    [MenuButton("Gradient Theme", ButtonType.Option, Category = "Settings", Order = 800, Options = new[] { "Off", "Aurora", "Sunset", "Midnight", "Ice", "Cotton Candy", "Custom" }, Notify = false)]
    public static void SetGradient() => GradientChanged("Gradient Theme");

    [MenuButton("Gradient Start Color", ButtonType.Option, Category = "Settings", Order = 801, Options = new[] { "Purple", "Blue", "Mint", "Cyan", "Pink", "Orange", "Red", "White" }, Notify = false)]
    public static void SetGradientStart() => GradientChanged("Gradient Start Color");

    [MenuButton("Gradient End Color", ButtonType.Option, Category = "Settings", Order = 802, Options = new[] { "Blue", "Mint", "Cyan", "Pink", "Orange", "Red", "White", "Purple" }, Notify = false)]
    public static void SetGradientEnd() => GradientChanged("Gradient End Color");

    [MenuButton("Gradient Enabled Start", ButtonType.Option, Category = "Settings", Order = 803, Options = new[] { "Mint", "Cyan", "Pink", "Orange", "Red", "White", "Purple", "Blue" }, Notify = false)]
    public static void SetEnabledStart() => GradientChanged("Gradient Enabled Start");

    [MenuButton("Gradient Enabled End", ButtonType.Option, Category = "Settings", Order = 804, Options = new[] { "Cyan", "Pink", "Orange", "Red", "White", "Purple", "Blue", "Mint" }, Notify = false)]
    public static void SetEnabledEnd() => GradientChanged("Gradient Enabled End");

    [MenuButton("Gradient Direction", ButtonType.Option, Category = "Settings", Order = 805, Options = new[] { "Vertical", "Horizontal", "Diagonal" }, Notify = false)]
    public static void SetGradientDirection() => GradientChanged("Gradient Direction");

    [MenuButton("Gradient Animation", ButtonType.Option, Category = "Settings", Order = 806, Options = new[] { "Off", "On" }, Notify = false)]
    public static void SetGradientAnimation() => GradientChanged("Gradient Animation");

    [MenuButton("Gradient Speed", ButtonType.Option, Category = "Settings", Order = 807, Options = new[] { "Normal", "Slow", "Fast", "Very Fast" }, Notify = false)]
    public static void SetGradientSpeed() => GradientChanged("Gradient Speed");

    [MenuButton("Menu Font", ButtonType.Option, Category = "Settings", Order = 808, Options = new[] { "Original", "Segoe UI", "Verdana", "Consolas" }, Notify = false)]
    public static void SetMenuFont() => Changed();

    [MenuButton("Board Font", ButtonType.Option, Category = "Settings", Order = 809, Options = new[] { "Original", "Segoe UI", "Verdana", "Consolas" }, Notify = false)]
    public static void SetBoardFont() => Changed();

    [MenuButton("GUI Font", ButtonType.Option, Category = "Settings", Order = 810, Options = new[] { "Original", "Segoe UI", "Verdana", "Consolas" }, Notify = false)]
    public static void SetGUIFont() => Changed();

    [MenuButton("Menu Text Size", ButtonType.Option, Category = "Settings", Order = 811, Options = new[] { "100%", "85%", "115%" }, Notify = false)]
    public static void SetMenuTextSize() => Changed();

    [MenuButton("GUI Text Size", ButtonType.Option, Category = "Settings", Order = 812, Options = new[] { "100%", "85%", "115%" }, Notify = false)]
    public static void SetGUITextSize() => Changed();

    [MenuButton("Holdable Hand", ButtonType.Option, Category = "Settings", Order = 820, Options = new[] { "Right", "Left" }, Notify = false)]
    public static void SetHoldableHand() => HoldableChanged();

    [MenuButton("Holdable Size", ButtonType.Option, Category = "Settings", Order = 821, Options = new[] { "1", "0.5", "0.75", "1.25", "1.5", "2" }, Notify = false)]
    public static void SetHoldableSize() => HoldableChanged();

    [MenuButton("Holdable Color", ButtonType.Option, Category = "Settings", Order = 822, Options = new[] { "Original", "Theme", "Player", "Purple", "Blue", "Mint", "Cyan", "Pink", "Orange", "Red", "White" }, Notify = false)]
    public static void SetHoldableColor() => HoldableChanged();

    [MenuButton("Holdable Offset X", ButtonType.Option, Category = "Settings", Order = 823, Options = new[] { "0", "-0.1", "-0.05", "0.05", "0.1" }, Notify = false)]
    public static void SetHoldableX() => HoldableChanged();

    [MenuButton("Holdable Offset Y", ButtonType.Option, Category = "Settings", Order = 824, Options = new[] { "0", "-0.1", "-0.05", "0.05", "0.1" }, Notify = false)]
    public static void SetHoldableY() => HoldableChanged();

    [MenuButton("Holdable Offset Z", ButtonType.Option, Category = "Settings", Order = 825, Options = new[] { "0", "-0.1", "-0.05", "0.05", "0.1" }, Notify = false)]
    public static void SetHoldableZ() => HoldableChanged();

    [MenuButton("Holdable Pitch", ButtonType.Option, Category = "Settings", Order = 826, Options = new[] { "0", "-90", "-45", "45", "90", "180" }, Notify = false)]
    public static void SetHoldablePitch() => HoldableChanged();

    [MenuButton("Holdable Yaw", ButtonType.Option, Category = "Settings", Order = 827, Options = new[] { "0", "-90", "-45", "45", "90", "180" }, Notify = false)]
    public static void SetHoldableYaw() => HoldableChanged();

    [MenuButton("Holdable Roll", ButtonType.Option, Category = "Settings", Order = 828, Options = new[] { "0", "-90", "-45", "45", "90", "180" }, Notify = false)]
    public static void SetHoldableRoll() => HoldableChanged();

    [MenuButton("Holdable Throw Strength", ButtonType.Option, Category = "Settings", Order = 829, Options = new[] { "1", "0.5", "1.5", "2" }, Notify = false)]
    public static void SetThrowPower() => HoldableChanged();

    [MenuButton("Holdable Vibrations", ButtonType.Option, Category = "Settings", Order = 830, Options = new[] { "On", "Off" }, Notify = false)]
    public static void SetHoldableVibrations() => HoldableChanged();

    [MenuButton("Flashlight Range", ButtonType.Option, Category = "Settings", Order = 832, Options = new[] { "12", "6", "20", "30" }, Notify = false)]
    public static void SetLightRange() => HoldableChanged();

    [MenuButton("Flashlight Brightness", ButtonType.Option, Category = "Settings", Order = 833, Options = new[] { "1", "0.5", "2", "3" }, Notify = false)]
    public static void SetLightBrightness() => HoldableChanged();

    [MenuButton("Lucy Physics Bounce", ButtonType.Option, Category = "Settings", Order = 834, Options = new[] { "Off", "On" }, Notify = false)]
    public static void SetLucyBounce() => HoldableChanged();

    [MenuButton("Nearby Alert Targets", ButtonType.Option, Category = "Settings", Order = 835, Options = new[] { "Everyone", "Infected Only" }, Notify = false)]
    public static void SetAlertTargets() => HoldableChanged();

    [MenuButton("Tracer Filter", ButtonType.Option, Category = "Settings", Order = 836, Options = new[] { "Everyone", "Equipped Cosmetics" }, Notify = false)]
    public static void SetTracerFilter() => HoldableChanged();

    [MenuButton("Draw Output", ButtonType.Option, Category = "Settings", Order = 837, Options = new[] { "Original", "Ghost Reactor Local" }, Notify = false)]
    public static void SetDrawOutput() => HoldableChanged();

}
