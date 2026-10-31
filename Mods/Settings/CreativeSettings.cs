// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Settings;

public static class CreativeSettings
{
    internal static float BankCapacity = 12f, SkateSpeed = 6f, PaddlePower = 3f, CompassGravity = 9.81f, LandingDistance = 1f, ClimbPower = 5f, HopPower = 6f;
    internal static float GateRadius = 0.65f, PoseTime = 1f, CameraTime = 0.12f, EffectSize = 1f, EffectIntensity = 1f, EffectQuality = 1f;
    internal static int ClimbMode, HaloStyle, EffectColor;
    private static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;
    [MenuButton("Momentum Bank Capacity", ButtonType.Option, Category = "Settings", Order = 400, Options = new[] { "12", "6", "18", "24" }, Notify = false)]
    public static void SetBank() => BankCapacity = new[] { 12f, 6f, 18f, 24f }[Index("Momentum Bank Capacity")];
    [MenuButton("Surface Skate Speed", ButtonType.Option, Category = "Settings", Order = 401, Options = new[] { "6", "3", "9", "12" }, Notify = false)]
    public static void SetSkate() => SkateSpeed = new[] { 6f, 3f, 9f, 12f }[Index("Surface Skate Speed")];
    [MenuButton("Air Paddle Power", ButtonType.Option, Category = "Settings", Order = 402, Options = new[] { "3", "1.5", "4.5", "6" }, Notify = false)]
    public static void SetPaddle() => PaddlePower = new[] { 3f, 1.5f, 4.5f, 6f }[Index("Air Paddle Power")];
    [MenuButton("Compass Gravity", ButtonType.Option, Category = "Settings", Order = 403, Options = new[] { "9.81", "5", "15", "20" }, Notify = false)]
    public static void SetGravity() => CompassGravity = new[] { 9.81f, 5f, 15f, 20f }[Index("Compass Gravity")];
    [MenuButton("Landing Brake Distance", ButtonType.Option, Category = "Settings", Order = 404, Options = new[] { "1", "0.5", "1.5", "2" }, Notify = false)]
    public static void SetLanding() => LandingDistance = new[] { 1f, 0.5f, 1.5f, 2f }[Index("Landing Brake Distance")];
    [MenuButton("Auto Climb Power", ButtonType.Option, Category = "Settings", Order = 405, Options = new[] { "5", "3", "7", "9" }, Notify = false)]
    public static void SetClimbPower() => ClimbPower = new[] { 5f, 3f, 7f, 9f }[Index("Auto Climb Power")];
    [MenuButton("Auto Climb Mode", ButtonType.Option, Category = "Settings", Order = 406, Options = new[] { "Both Hands", "Either Hand" }, Notify = false)]
    public static void SetClimbMode() => ClimbMode = Index("Auto Climb Mode");
    [MenuButton("Bunny Hop Power", ButtonType.Option, Category = "Settings", Order = 407, Options = new[] { "6", "4", "8", "10" }, Notify = false)]
    public static void SetHop() => HopPower = new[] { 6f, 4f, 8f, 10f }[Index("Bunny Hop Power")];
    [MenuButton("Gate Runner Size", ButtonType.Option, Category = "Settings", Order = 408, Options = new[] { "Normal", "Small", "Large" }, Notify = false)]
    public static void SetGate() => GateRadius = new[] { 0.65f, 0.4f, 1f }[Index("Gate Runner Size")];
    [MenuButton("Pose Transition Time", ButtonType.Option, Category = "Settings", Order = 409, Options = new[] { "1s", "0.5s", "2s", "3s" }, Notify = false)]
    public static void SetPose() => PoseTime = new[] { 1f, 0.5f, 2f, 3f }[Index("Pose Transition Time")];
    [MenuButton("Desktop Camera Smoothing", ButtonType.Option, Category = "Settings", Order = 410, Options = new[] { "Normal", "Light", "Smooth", "Very Smooth" }, Notify = false)]
    public static void SetCamera() => CameraTime = new[] { 0.12f, 0.06f, 0.2f, 0.3f }[Index("Desktop Camera Smoothing")];
    [MenuButton("Beat Halo Style", ButtonType.Option, Category = "Settings", Order = 411, Options = new[] { "Halo", "Audio Orbs" }, Notify = false)]
    public static void SetHalo() => HaloStyle = Index("Beat Halo Style");
    [MenuButton("Effect Size", ButtonType.Option, Category = "Settings", Order = 412, Options = new[] { "Normal", "Small", "Large" }, Notify = false)]
    public static void SetEffectSize() { EffectSize = new[] { 1f, 0.65f, 1.4f }[Index("Effect Size")]; RefreshEffects(); }
    [MenuButton("Effect Intensity", ButtonType.Option, Category = "Settings", Order = 413, Options = new[] { "Normal", "Low", "High" }, Notify = false)]
    public static void SetIntensity() { EffectIntensity = new[] { 1f, 0.5f, 1.5f }[Index("Effect Intensity")]; RefreshEffects(); }
    [MenuButton("Effect Quality", ButtonType.Option, Category = "Settings", Order = 414, Options = new[] { "Normal", "Low", "High" }, Notify = false)]
    public static void SetQuality() { EffectQuality = new[] { 1f, 0.5f, 1.5f }[Index("Effect Quality")]; RefreshEffects(); }
    [MenuButton("Effect Colors", ButtonType.Option, Category = "Settings", Order = 415, Options = new[] { "Original", "Menu Theme", "Player Color" }, Notify = false)]
    public static void SetColor() { EffectColor = Index("Effect Colors"); RefreshEffects(); }
    internal static Color Color(Color original)
    {
        if (EffectColor == 1) return ChudOS.Core.Main.PrimaryColorValue;
        return EffectColor == 2 && VRRig.LocalRig != null ? VRRig.LocalRig.playerColor : original;
    }
    private static void RefreshEffects() => ChudOS.Core.Managers.Particles.ParticleManager.ApplySettings();
}
