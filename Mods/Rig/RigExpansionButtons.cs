// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Stuff;

namespace ChudOS.Mods.Rig;

public static class RigExpansionButtons
{
    [MenuButton("Weeping Angel", ButtonType.Toggle, Category = "Rig", Order = 700, runOption = RunOption.Once, Description = "Freeze your avatar while another player looks toward it; move back toward your body when unseen.")]
    public static void EnableWeepingAngel() => RigRuntime.Enable<AngelPose>("Weeping Angel");
    [MenuDisable("Weeping Angel")]
    public static void DisableWeepingAngel() => RigRuntime.Disable("Weeping Angel");

    [MenuButton("Minecraft Animations", ButtonType.Toggle, Category = "Rig", Order = 701, runOption = RunOption.Once, Description = "Animate your avatar arms with block-style walking swings.")]
    public static void EnableMinecraftAnimations() => RigRuntime.Enable<MinecraftPose>("Minecraft Animations");
    [MenuDisable("Minecraft Animations")]
    public static void DisableMinecraftAnimations() => RigRuntime.Disable("Minecraft Animations");

    [MenuButton("Chicken Rig", ButtonType.Toggle, Category = "Rig", Order = 702, runOption = RunOption.Once, Description = "Keep avatar hands near the shoulders and flap them.")]
    public static void EnableChickenRig() => RigRuntime.Enable<ChickenPose>("Chicken Rig");
    [MenuDisable("Chicken Rig")]
    public static void DisableChickenRig() => RigRuntime.Disable("Chicken Rig");

    [MenuButton("Amputate Rig", ButtonType.Toggle, Category = "Rig", Order = 703, runOption = RunOption.Once, Description = "Hide compatible arm bones on your own avatar and restore their scale when disabled.")]
    public static void EnableAmputateRig() => RigRuntime.Enable<AmputatePose>("Amputate Rig");
    [MenuDisable("Amputate Rig")]
    public static void DisableAmputateRig() => RigRuntime.Disable("Amputate Rig");

    [MenuButton("Floating Rig", ButtonType.Toggle, Category = "Rig", Order = 704, runOption = RunOption.Once, Description = "Float your avatar with a small vertical bob.")]
    public static void EnableFloatingRig() => RigRuntime.Enable<FloatingPose>("Floating Rig");
    [MenuDisable("Floating Rig")]
    public static void DisableFloatingRig() => RigRuntime.Disable("Floating Rig");

    [MenuButton("Eye Contact", ButtonType.Toggle, Category = "Rig", Order = 705, runOption = RunOption.Once, Description = "Point your avatar head toward the nearest player within ten metres.")]
    public static void EnableEyeContact() => RigRuntime.Enable<EyeContactPose>("Eye Contact");
    [MenuDisable("Eye Contact")]
    public static void DisableEyeContact() => RigRuntime.Disable("Eye Contact");

}
