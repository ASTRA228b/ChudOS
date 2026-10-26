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
    [MenuButton("Hug Gun", ButtonType.Toggle, Category = "Rig", Order = 18, runOption = RunOption.Once, Description = "Aim and hold trigger to hug the front of the selected avatar")]
    public static void EnableHugGun() => RigRuntime.Enable<HugGun>("Hug Gun");
    [MenuDisable("Hug Gun")]
    public static void DisableHugGun() => RigRuntime.Disable("Hug Gun");

    [MenuButton("Follow Player Gun", ButtonType.Toggle, Category = "Rig", Order = 19, runOption = RunOption.Once, Description = "Aim and hold trigger to follow behind a selected player")]
    public static void EnableFollowGun() => RigRuntime.Enable<FollowPlayerGun>("Follow Player Gun");
    [MenuDisable("Follow Player Gun")]
    public static void DisableFollowGun() => RigRuntime.Disable("Follow Player Gun");

    [MenuButton("Orbit Player Gun", ButtonType.Toggle, Category = "Rig", Order = 20, runOption = RunOption.Once, Description = "Aim and hold trigger to orbit a selected player")]
    public static void EnableOrbitGun() => RigRuntime.Enable<OrbitPlayerGun>("Orbit Player Gun");
    [MenuDisable("Orbit Player Gun")]
    public static void DisableOrbitGun() => RigRuntime.Disable("Orbit Player Gun");

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

    [MenuButton("Rig Gun", ButtonType.Toggle, Category = "Rig", Order = 21, runOption = RunOption.Once, Description = "Place your rig just above the gun hit point")]
    public static void EnableRigGun() => RigRuntime.Enable<RigGun>("Rig Gun");
    [MenuDisable("Rig Gun")]
    public static void DisableRigGun() => RigRuntime.Disable("Rig Gun");

    [MenuButton("Spazz Rig", ButtonType.Toggle, Category = "Rig", Order = 22, runOption = RunOption.Once, Description = "Hold A to shake your rig's tracking offsets")]
    public static void EnableSpazzRig() => RigRuntime.Enable<SpazzRig>("Spazz Rig");
    [MenuDisable("Spazz Rig")]
    public static void DisableSpazzRig() => RigRuntime.Disable("Spazz Rig");

    [MenuButton("Spazz Hands", ButtonType.Toggle, Category = "Rig", Order = 23, runOption = RunOption.Once, Description = "Hold A to randomize your rig's hand poses")]
    public static void EnableSpazzHands() => RigRuntime.Enable<SpazzHands>("Spazz Hands");
    [MenuDisable("Spazz Hands")]
    public static void DisableSpazzHands() => RigRuntime.Disable("Spazz Hands");

    [MenuButton("Fixed LongArms", ButtonType.Toggle, Category = "Rig", Order = 24, runOption = RunOption.Once, Description = "Use the original LongArm Slider method at a fixed 1.5 scale")]
    public static void EnableFixedLongArms() => RigRuntime.Enable<FixedLongArms>("Fixed LongArms");
    [MenuDisable("Fixed LongArms")]
    public static void DisableFixedLongArms() => RigRuntime.Disable("Fixed LongArms");

    [MenuButton("Grab Rig", ButtonType.Toggle, Category = "Rig", Order = 25, runOption = RunOption.Once, Description = "Hold right grip to move your own rig with your hand")]
    public static void EnableGrabRig() => RigRuntime.Enable<GrabRig>("Grab Rig");
    [MenuDisable("Grab Rig")]
    public static void DisableGrabRig() => RigRuntime.Disable("Grab Rig");

    [MenuButton("Fake Lag", ButtonType.Toggle, Category = "Rig", Order = 26, runOption = RunOption.Once, Description = "Update your local rig pose every half second")]
    public static void EnableFakeLag() => RigRuntime.Enable<FakeLag>("Fake Lag");
    [MenuDisable("Fake Lag")]
    public static void DisableFakeLag() => RigRuntime.Disable("Fake Lag");

    [MenuButton("Helicopter", ButtonType.Toggle, Category = "Rig", Order = 27, runOption = RunOption.Once, Description = "Hold A to spin and raise your rig with its arms extended")]
    public static void EnableHelicopter() => RigRuntime.Enable<Helicopter>("Helicopter");
    [MenuDisable("Helicopter")]
    public static void DisableHelicopter() => RigRuntime.Disable("Helicopter");

    [MenuButton("T-Pose", ButtonType.Toggle, Category = "Rig", Order = 28, runOption = RunOption.Once, Description = "Hold A to extend your rig's arms sideways")]
    public static void EnableTPose() => RigRuntime.Enable<TPose>("T-Pose");
    [MenuDisable("T-Pose")]
    public static void DisableTPose() => RigRuntime.Disable("T-Pose");

    [MenuButton("Freeze Rig Body", ButtonType.Toggle, Category = "Rig", Order = 29, runOption = RunOption.Once, Description = "Anchor your rig's body while your hands and head rotation follow tracking")]
    public static void EnableFreezeRigBody() => RigRuntime.Enable<FreezeRigBody>("Freeze Rig Body");
    [MenuDisable("Freeze Rig Body")]
    public static void DisableFreezeRigBody() => RigRuntime.Disable("Freeze Rig Body");

    [MenuButton("Freeze Rig Limbs", ButtonType.Toggle, Category = "Rig", Order = 30, runOption = RunOption.Once, Description = "Keep your rig's head and hands stationary while its body follows you")]
    public static void EnableFreezeRigLimbs() => RigRuntime.Enable<FreezeRigLimbs>("Freeze Rig Limbs");
    [MenuDisable("Freeze Rig Limbs")]
    public static void DisableFreezeRigLimbs() => RigRuntime.Disable("Freeze Rig Limbs");

    [MenuButton("Jumpscare Gun", ButtonType.Toggle, Category = "Rig", Order = 31, runOption = RunOption.Once, Description = "Aim and hold trigger to pose your rig in front of the selected player's face")]
    public static void EnableJumpscareGun() => RigRuntime.Enable<JumpscareGun>("Jumpscare Gun");
    [MenuDisable("Jumpscare Gun")]
    public static void DisableJumpscareGun() => RigRuntime.Disable("Jumpscare Gun");

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
