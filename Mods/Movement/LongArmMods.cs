// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class LongArmMods
{
    private static readonly float[] Lengths = { 1.1f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.05f, 1.15f, 1.2f, 1.3f, 1.4f, 1.5f, 1.75f, 2f, 2.25f, 2.5f };
    private static readonly float[] LegacyLengths = { 2.5f, 1f, 1.5f, 2f, 3f, 4f, 5.5f, 0.1f, 0.5f };
    private static readonly float[] Heights = { 1f, 0.5f, 0.75f, 1.25f, 1.5f, 2f, 2.5f, 3f, 5.5f };
    private static Transform? owner;
    private static Vector3 original;
    private static float length = 1.1f, height = 1f;

    [MenuButton("Long Arms", ButtonType.Toggle, Category = "Movement", Order = 151, runOption = RunOption.LateUpdate, Description = "Use the configured horizontal player scale and height")]
    public static void LongArms() => ApplyLength(length);

    internal static void ApplyLength(float value)
    {
        if (!PortTools.Ready) { Reset(); return; }
        Transform player = GTPlayer.Instance.transform;
        if (owner != player)
        {
            Reset();
            owner = player;
            original = player.localScale;
        }
        value = Mathf.Clamp(value, 0.5f, 2.5f);
        player.localScale = Vector3.Scale(original, new Vector3(value, height, value));
    }
    [MenuEnable("Long Arms")]
    public static void Enable()
    {
        ButtonManager.SetEnabled("Size Changer", false);
        ButtonManager.SetEnabled("Fixed LongArms", false);
        PortTools.Register(Reset);
    }
    [MenuDisable("Long Arms")]
    public static void Disable() => Reset();

    [MenuButton("Long Arm Length", ButtonType.Option, Category = "Settings", Order = 170, Options = new[] { "1.1", "0.5", "0.6", "0.7", "0.8", "0.9", "1", "1.05", "1.15", "1.2", "1.3", "1.4", "1.5", "1.75", "2", "2.25", "2.5" }, Description = "Horizontal player scale while Long Arms is enabled")]
    public static void SetLength()
    {
        int index = ButtonManager.GetButton("Long Arm Length")?.OptionIndex ?? 0;
        length = Lengths[Mathf.Clamp(index, 0, Lengths.Length - 1)];
    }
    [MenuButton("Long Arm Height", ButtonType.Option, Category = "Settings", Order = 171, Options = new[] { "1", "0.5", "0.75", "1.25", "1.5", "2", "2.5", "3", "5.5" }, Description = "Vertical player scale while Long Arms is enabled")]
    public static void SetHeight()
    {
        int index = ButtonManager.GetButton("Long Arm Height")?.OptionIndex ?? 0;
        height = Heights[Mathf.Clamp(index, 0, Heights.Length - 1)];
    }
    internal static int LegacyLengthIndex(int index)
    {
        if (index <= 0 || index >= LegacyLengths.Length) return 0;
        float value = Mathf.Clamp(LegacyLengths[index], 0.5f, 2.5f);
        int nearest = 0;
        float distance = float.MaxValue;
        for (int i = 0; i < Lengths.Length; i++)
        {
            float current = Mathf.Abs(Lengths[i] - value);
            if (current >= distance) continue;
            nearest = i;
            distance = current;
        }
        return nearest;
    }
    public static void Reset()
    {
        if (owner != null) owner.localScale = original;
        owner = null;
    }
}
