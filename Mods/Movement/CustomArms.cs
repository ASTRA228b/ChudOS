// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class CustomArms
{
    private static readonly float[] Speeds = { 0.25f, 0.1f, 0.5f, 1f };
    private static float length = 1.1f, speed = 0.25f;

    [MenuButton("Custom Arms", ButtonType.Toggle, Category = "Movement", Order = 164, runOption = RunOption.LateUpdate, Description = "Left trigger increases arm scale; right trigger decreases it")]
    public static void Tick()
    {
        if (!PortTools.Ready) { Stop(); return; }
        float input = (InputLib.LeftTrigger ? 1f : 0f) - (InputLib.RightTrigger ? 1f : 0f);
        float delta = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
        length = Mathf.Clamp(length + input * speed * delta, 0.5f, 2.5f);
        LongArmMods.ApplyLength(length);
    }
    [MenuEnable("Custom Arms")]
    public static void Enable() => PortTools.Register(Stop);
    [MenuDisable("Custom Arms")]
    public static void Stop() => LongArmMods.Reset();

    [MenuButton("Custom Arms Speed", ButtonType.Option, Category = "Settings", Order = 231, Options = new[] { "0.25", "0.1", "0.5", "1" }, Notify = false, Description = "Scale adjustment per second while holding a trigger")]
    public static void SetSpeed()
    {
        int index = ButtonManager.GetButton("Custom Arms Speed")?.OptionIndex ?? 0;
        speed = Speeds[Mathf.Clamp(index, 0, Speeds.Length - 1)];
    }
    [MenuButton("Reset Custom Arms", ButtonType.Press, Category = "Settings", Order = 232)]
    public static void ResetLength()
    {
        length = 1.1f;
        if (ButtonManager.GetButton("Custom Arms")?.Enabled == true) LongArmMods.ApplyLength(length);
    }
}
