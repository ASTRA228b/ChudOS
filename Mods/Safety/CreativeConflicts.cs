// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Safety;

internal static class CreativeConflicts
{
    private static readonly HashSet<string> Added = new(StringComparer.Ordinal) { "Momentum Bank", "Surface Skate", "Air Paddle", "Gravity Compass", "Landing Brake", "Advanced Checkpoints", "Climby Hands", "Auto Climb", "Bunny Hop", "PC Walking" };
    private static readonly HashSet<string> Practice = new(StringComparer.Ordinal) { "Echo Race", "Gate Runner", "Beat Targets" };
    private static readonly HashSet<string> Existing = new(StringComparer.Ordinal)
    {
        "Car", "Fly (X)", "Trigger Fly", "Joystick Fly (LJ)", "BarkFly (JR)(JL)", "Velocity BarkFly (JR)(JL)", "Hand Fly (B)", "WASD Fly", "NoClip Fly", "Drone Fly", "Velocity Movement", "Fly Towards Gun", "Spider Man", "Grappling Hooks", "Wall Walk", "Wall Walk V2", "Low Gravity", "High Gravity", "Zero Gravity", "Throw", "Double Jump", "Rewind"
    };
    internal static void OnEnable(ButtonManager.ButtonData button)
    {
        if (button.Type != ButtonType.Toggle) return;
        if (Practice.Contains(button.Name))
            foreach (string name in Practice) if (name != button.Name) ButtonManager.SetEnabled(name, false);
        bool added = Added.Contains(button.Name);
        if (!added && !Existing.Contains(button.Name)) return;
        foreach (var other in ButtonManager.Buttons)
            if (other.Name != button.Name && other.Type == ButtonType.Toggle && (Added.Contains(other.Name) || added && Existing.Contains(other.Name))) ButtonManager.SetEnabled(other, false);
    }
}
