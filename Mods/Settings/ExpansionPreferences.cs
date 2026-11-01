// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core;
using ChudOS.Core.Managers;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Settings;

public static partial class ExpansionPreferences
{
    internal static int Revision;
    private static readonly float[] Sizes = { 1f, 0.85f, 1.15f };
    private static readonly float[] Offsets = { 0f, -0.1f, -0.05f, 0.05f, 0.1f };
    private static readonly float[] Angles = { 0f, -90f, -45f, 45f, 90f, 180f };
    private static readonly float[] HoldableSizes = { 1f, 0.5f, 0.75f, 1.25f, 1.5f, 2f };
    private static readonly float[] ThrowPowers = { 1f, 0.5f, 1.5f, 2f };
    private static readonly float[] AnimationSpeeds = { 0.15f, 0.05f, 0.3f, 0.6f };
    private static readonly float[] LightRanges = { 12f, 6f, 20f, 30f };
    private static readonly float[] LightPowers = { 1f, 0.5f, 2f, 3f };
    internal static readonly Color[] Colors = { new Color32(155, 18, 254, 255), new Color32(45, 145, 255, 255), new Color32(45, 255, 170, 255), new Color32(25, 220, 255, 255), new Color32(255, 90, 175, 255), new Color32(255, 145, 45, 255), new Color32(255, 65, 80, 255), Color.white };

    internal static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;
    internal static int Gradient => Index("Gradient Theme");
    internal static int GradientDirection => Index("Gradient Direction");
    internal static bool AnimatedGradient => Index("Gradient Animation") == 1;
    internal static float GradientSpeed => AnimationSpeeds[Index("Gradient Speed")];
    internal static int MenuFont => Index("Menu Font");
    internal static int BoardFont => Index("Board Font");
    internal static int GUIFont => Index("GUI Font");
    internal static float MenuTextSize => Sizes[Index("Menu Text Size")];
    internal static float GUITextSize => Sizes[Index("GUI Text Size")];
    internal static bool LeftHand => Index("Holdable Hand") == 1;
    internal static float HoldableSize => HoldableSizes[Index("Holdable Size")];
    internal static float ThrowPower => ThrowPowers[Index("Holdable Throw Strength")];
    internal static bool Vibrations => Index("Holdable Vibrations") == 0;
    internal static Vector3 HoldableOffset => new(Offsets[Index("Holdable Offset X")], Offsets[Index("Holdable Offset Y")], Offsets[Index("Holdable Offset Z")]);
    internal static Vector3 HoldableRotation => new(Angles[Index("Holdable Pitch")], Angles[Index("Holdable Yaw")], Angles[Index("Holdable Roll")]);
    internal static bool OriginalHoldableColors => Index("Holdable Color") == 0;
    internal static Color HoldableColor => Index("Holdable Color") switch { 0 or 1 => ChudOS.Core.Main.PrimaryColorValue, 2 => VRRig.LocalRig != null ? VRRig.LocalRig.playerColor : Color.white, _ => Colors[Mathf.Clamp(Index("Holdable Color") - 3, 0, Colors.Length - 1)] };
    internal static float FlashlightRange => LightRanges[Index("Flashlight Range")];
    internal static float FlashlightBrightness => LightPowers[Index("Flashlight Brightness")];
    internal static bool LucyBounce => Index("Lucy Physics Bounce") == 1;
    internal static bool InfectedAlerts => Index("Nearby Alert Targets") == 1;
    internal static bool CosmeticTracersOnly => Index("Tracer Filter") == 1;
    internal static bool ReactorDrawing => Index("Draw Output") == 1;
    internal static Color PressedStart => Colors[(Index("Gradient Enabled Start") + 2) % Colors.Length];
    internal static Color PressedEnd => Colors[(Index("Gradient Enabled End") + 3) % Colors.Length];

    internal static void GradientColors(out Color first, out Color second)
    {
        (first, second) = Gradient switch
        {
            1 => (Colors[2], Colors[1]), 2 => (Colors[5], Colors[4]), 3 => (Colors[0], Colors[1]),
            4 => (Colors[3], Colors[7]), 5 => (Colors[4], Colors[3]),
            _ => (Colors[Index("Gradient Start Color")], Colors[(Index("Gradient End Color") + 1) % Colors.Length])
        };
    }
    private static void Changed() { Revision++; ChudOS.Core.Main.RefreshExpansionAppearance(); }
    private static void HoldableChanged()
    {
        Revision++;
        string trigger = LeftHand ? "(LT)/(LMB)" : "(RT)/(LMB)";
        if (ButtonManager.GetButton("Handheld Camera") is { } camera) camera.Attribute.Controls = trigger;
        if (ButtonManager.GetButton("Measuring Tape") is { } tape) tape.Attribute.Controls = trigger + "(X)";
        if (ButtonManager.GetButton("Squeeze Toy") is { } toy) toy.Attribute.Controls = LeftHand ? "(LG)/(Shift)" : "(RG)/(RMB)";
    }
    private static void Reset(params string[] names)
    {
        foreach (string name in names)
        {
            var button = ButtonManager.GetButton(name);
            if (button != null && button.OptionIndex != 0) ButtonManager.ChangeOption(button, -button.OptionIndex);
        }
        Changed();
    }
    [MenuButton("Reset Fonts", ButtonType.Press, Category = "Settings", Order = 813, Notify = false)]
    public static void ResetFonts() => Reset("Menu Font", "Board Font", "GUI Font", "Menu Text Size", "GUI Text Size");
    [MenuButton("Reset Holdable Placement", ButtonType.Press, Category = "Settings", Order = 831, Notify = false)]
    public static void ResetHoldablePlacement() => Reset("Holdable Size", "Holdable Offset X", "Holdable Offset Y", "Holdable Offset Z", "Holdable Pitch", "Holdable Yaw", "Holdable Roll");
}
