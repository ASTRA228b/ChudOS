// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Rig;

public static class RigMods
{
    [MenuButton("Copy Movement Gun", ButtonType.Toggle, Category = "Rig", Order = 0, runOption = RunOption.Once, Description = "Right grip aims; hold trigger to copy a selected rig")]
    public static void EnableCopyMovementGun() => RigRuntime.Enable<CopyMovementGun>("Copy Movement Gun");

    [MenuDisable("Copy Movement Gun")]
    public static void DisableCopyMovementGun() => RigRuntime.Disable("Copy Movement Gun");

    [MenuButton("Dance", ButtonType.Toggle, Category = "Rig", Order = 1, runOption = RunOption.Once, Description = "Hold A to dance")]
    public static void EnableDance() => RigRuntime.Enable<Dance>("Dance");

    [MenuDisable("Dance")]
    public static void DisableDance() => RigRuntime.Disable("Dance");

    [MenuButton("Ghost Animations", ButtonType.Toggle, Category = "Rig", Order = 2, runOption = RunOption.Once, Description = "Threshold-based movement and rotation of your avatar")]
    public static void EnableGhostAnimations() => RigRuntime.Enable<GhostAnimations>("Ghost Animations");

    [MenuDisable("Ghost Animations")]
    public static void DisableGhostAnimations() => RigRuntime.Disable("Ghost Animations");

    [MenuButton("Ghost Monke", ButtonType.Toggle, Category = "Rig", Order = 3, runOption = RunOption.Once, Description = "Press A to freeze or release your avatar")]
    public static void EnableGhostMonke() => RigRuntime.Enable<GhostMonke>("Ghost Monke");

    [MenuDisable("Ghost Monke")]
    public static void DisableGhostMonke() => RigRuntime.Disable("Ghost Monke");

    [MenuButton("Griddy", ButtonType.Toggle, Category = "Rig", Order = 4, runOption = RunOption.Once, Description = "Hold A to perform the moving dance")]
    public static void EnableGriddy() => RigRuntime.Enable<Griddy>("Griddy");

    [MenuDisable("Griddy")]
    public static void DisableGriddy() => RigRuntime.Disable("Griddy");

    [MenuButton("Invis Monke", ButtonType.Toggle, Category = "Rig", Order = 5, runOption = RunOption.Once, Description = "Press B to move your avatar out of view or restore it")]
    public static void EnableInvisMonke() => RigRuntime.Enable<InvisMonke>("Invis Monke");

    [MenuDisable("Invis Monke")]
    public static void DisableInvisMonke() => RigRuntime.Disable("Invis Monke");

    [MenuButton("Jerk Off", ButtonType.Toggle, Category = "Rig", Order = 6, runOption = RunOption.Once, Description = "Applies the upstream right-hand gesture")]
    public static void EnableJerkOff() => RigRuntime.Enable<JerkOff>("Jerk Off");

    [MenuDisable("Jerk Off")]
    public static void DisableJerkOff() => RigRuntime.Disable("Jerk Off");

    [MenuButton("Look At Gun", ButtonType.Toggle, Category = "Rig", Order = 7, runOption = RunOption.Once, Description = "Right grip aims; hold trigger to look at a selected player")]
    public static void EnableLookAtGun() => RigRuntime.Enable<LookAtGun>("Look At Gun");

    [MenuDisable("Look At Gun")]
    public static void DisableLookAtGun() => RigRuntime.Disable("Look At Gun");

    [MenuButton("PC Rig", ButtonType.Toggle, Category = "Rig", Order = 8, runOption = RunOption.Once, Description = "Desktop avatar posing; choose a style with PC Rig Mode")]
    public static void EnablePCRig() => RigRuntime.Enable<PCRig>("PC Rig");

    [MenuDisable("PC Rig")]
    public static void DisablePCRig() => RigRuntime.Disable("PC Rig");

    [MenuButton("Rec Room Rig", ButtonType.Toggle, Category = "Rig", Order = 9, runOption = RunOption.Once, Description = "Turns your torso using your head and hand directions")]
    public static void EnableRecRoomRig() => RigRuntime.Enable<RecRoomRig>("Rec Room Rig");

    [MenuDisable("Rec Room Rig")]
    public static void DisableRecRoomRig() => RigRuntime.Disable("Rec Room Rig");

    [MenuButton("Rig Tweaks", ButtonType.Toggle, Category = "Rig", Order = 10, runOption = RunOption.Once, Description = "Arrows or sticks rotate the avatar; R resets; remaps head and hands")]
    public static void EnableRigTweaks() => RigRuntime.Enable<RigTweaks>("Rig Tweaks");

    [MenuDisable("Rig Tweaks")]
    public static void DisableRigTweaks() => RigRuntime.Disable("Rig Tweaks");

    [MenuButton("Rig Tweaks V2", ButtonType.Toggle, Category = "Rig", Order = 11, runOption = RunOption.Once, Description = "Arrows or sticks rotate the avatar; R resets; preserves existing mapping")]
    public static void EnableRigTweaksV2() => RigRuntime.Enable<RigTweaksV2>("Rig Tweaks V2");

    [MenuDisable("Rig Tweaks V2")]
    public static void DisableRigTweaksV2() => RigRuntime.Disable("Rig Tweaks V2");

    [MenuButton("Shadow Clone Gun", ButtonType.Toggle, Category = "Rig", Order = 12, runOption = RunOption.Once, Description = "Right grip aims; hold trigger to copy a player beside them")]
    public static void EnableShadowCloneGun() => RigRuntime.Enable<ShadowCloneGun>("Shadow Clone Gun");

    [MenuDisable("Shadow Clone Gun")]
    public static void DisableShadowCloneGun() => RigRuntime.Disable("Shadow Clone Gun");

    [MenuButton("Smooth Rig", ButtonType.Toggle, Category = "Rig", Order = 13, runOption = RunOption.Once, Description = "Smooths your avatar and hand movement")]
    public static void EnableSmoothRig() => RigRuntime.Enable<SmoothRig>("Smooth Rig");

    [MenuDisable("Smooth Rig")]
    public static void DisableSmoothRig() => RigRuntime.Disable("Smooth Rig");

    [MenuButton("Spin Bot", ButtonType.Toggle, Category = "Rig", Order = 14, runOption = RunOption.Once, Description = "Rotates your avatar around its vertical axis")]
    public static void EnableSpinBot() => RigRuntime.Enable<SpinBot>("Spin Bot");

    [MenuDisable("Spin Bot")]
    public static void DisableSpinBot() => RigRuntime.Disable("Spin Bot");

    [MenuButton("PC Rig Mode", ButtonType.Option, Category = "Rig", Order = 15, Options = new[] { "Wings", "Skellon", "Lemming", "Head", "Rec Room" }, Description = "Choose the desktop avatar pose used by PC Rig")]
    public static void ChangePcRigMode()
    {
        int index = ButtonManager.GetButton("PC Rig Mode")?.OptionIndex ?? 0;
        PCRig.CurrentRigMode = index switch
        {
            1 => RigMode.Skellon,
            2 => RigMode.Lemming,
            3 => RigMode.Head,
            4 => RigMode.RecRoom,
            _ => RigMode.Wings
        };
    }
}