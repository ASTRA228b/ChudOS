// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Safety;

internal static class ModGuards
{
    internal const string DetectedWarning = "WARNING: Mods in Detected may trigger detection or result in a ban. Safety settings do not make them safe to use.";
    private static readonly string[] VelocityControllers = { "Car", "Fly (X)", "Trigger Fly", "Joystick Fly (LJ)", "BarkFly (JR)(JL)", "Velocity BarkFly (JR)(JL)", "Hand Fly (B)", "WASD Fly", "NoClip Fly", "Drone Fly", "Velocity Movement", "Fly Towards Gun" };
    private static readonly Dictionary<string, float> errorNotices = new(StringComparer.Ordinal);

    internal static void WarnDetected() => NotifyLib.SendNotificationALL(TranslationManager.T(DetectedWarning), 6500);
    internal static void OnEnable(ButtonManager.ButtonData button)
    {
        if (button.Name == "Size Changer")
        {
            ButtonManager.SetEnabled("Long Arms", false);
            ButtonManager.SetEnabled("Fixed LongArms", false);
        }
        if (button.Name == "Predictions")
            foreach (var other in ButtonManager.Buttons)
                if (other.Category == "Rig" && other.Type == ButtonType.Toggle && other.Name != "Max Quest Score") ButtonManager.SetEnabled(other, false);
        if ((MacroManager.IsPlaying || MacroManager.Recording) && MacroManager.Conflicts(button)) MacroManager.Stop();
        if (button.Category == "Detected") WarnDetected();
        if (VelocityControllers.Contains(button.Name))
            foreach (string name in VelocityControllers)
                if (name != button.Name) ButtonManager.SetEnabled(name, false);
    }

    internal static bool ShouldLogError(string name)
    {
        float now = Time.unscaledTime;
        if (errorNotices.TryGetValue(name, out float next) && now < next) return false;
        errorNotices[name] = now + 3f;
        return true;
    }

    internal static void ClearTemporaryState()
    {
        foreach (string name in new[] { "Drone Fly", "Velocity Movement", "Throw", "Double Jump", "Fly Towards Gun", "Punch", "Fixed LongArms" }) ButtonManager.SetEnabled(name, false);
        ChudOS.Mods.Movement.ExtraMovementMods.Clear();
        ChudOS.Mods.Movement.PunchMods.Stop();
        ChudOS.Mods.Movement.NoClipMods.Clear();
        ButtonManager.SetEnabled("Predictions", false);
        ButtonManager.SetEnabled("PitGeo", false);
        ChudOS.Mods.Movement.Predictions.Stop();
        ChudOS.Mods.Movement.PitGeo.Stop();
        ChudOS.Core.Managers.Particles.ParticleManager.Shutdown();
        ButtonManager.SetEnabled("Long Arms", false);
        ChudOS.Mods.Movement.LongArmMods.Reset();
        ButtonManager.SetEnabled("Macro Controls", false);
        MacroManager.Shutdown();
        ChudOS.Mods.Movement.TravelMods.StopPearl();
        ChudOS.Mods.Movement.TravelMods.StopCar();
        ChudOS.Mods.Movement.TravelMods.StopReverse();
        ChudOS.Mods.Movement.PullMod.Stop();
        ChudOS.Mods.Movement.PlatformMods.Clear();
        GunLib.Hide();
        errorNotices.Clear();
    }

    [MenuButton("Reset Temporary Mod State", ButtonType.Press, Category = "Safety", Order = 30, Description = "Disable movement and rig toggles, clear targets and projectiles, and restore temporary state")]
    public static void Recover()
    {
        foreach (var button in ButtonManager.Buttons)
            if (button.Enabled && button.Type == ButtonType.Toggle && (button.Category == "Movement" || button.Category == "Rig" || button.Category == "Detected")) ButtonManager.SetEnabled(button, false);
        ChudOS.Mods.Fun.FunMods.FixAll();
        ClearTemporaryState();
    }
}
