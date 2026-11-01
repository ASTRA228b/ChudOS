// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Movement;

public static class MovementExpansionButtons
{
    [MenuButton("Airplane", ButtonType.Toggle, Category = "Movement", Order = 700, runOption = RunOption.Once, Controls = "(Arms Out)", Description = "Glide with spread arms; aim both wrists to steer.")]
    public static void EnableAirplane() => BetaFeature.Enable<ExpansionMovement>("Airplane");
    [MenuDisable("Airplane")]
    public static void DisableAirplane() => BetaFeature.Disable("Airplane");

    [MenuButton("Bird Fly", ButtonType.Toggle, Category = "Movement", Order = 701, runOption = RunOption.Once, Controls = "(Flap Arms)", Description = "Flap both arms down while airborne to gain lift.")]
    public static void EnableBirdFly() => BetaFeature.Enable<ExpansionMovement>("Bird Fly");
    [MenuDisable("Bird Fly")]
    public static void DisableBirdFly() => BetaFeature.Disable("Bird Fly");

    [MenuButton("Bubble Locomotion", ButtonType.Toggle, Category = "Movement", Order = 702, runOption = RunOption.Once, Controls = "(LG)(RG)", Description = "Push the bubble boundary with either grip to move.")]
    public static void EnableBubbleLocomotion() => BetaFeature.Enable<ExpansionMovement>("Bubble Locomotion");
    [MenuDisable("Bubble Locomotion")]
    public static void DisableBubbleLocomotion() => BetaFeature.Disable("Bubble Locomotion");

    [MenuButton("Zipline Launcher", ButtonType.Toggle, Category = "Movement", Order = 703, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Select two world anchors, then grip near the line to ride.")]
    public static void EnableZiplineLauncher() => BetaFeature.Enable<ExpansionMovement>("Zipline Launcher");
    [MenuDisable("Zipline Launcher")]
    public static void DisableZiplineLauncher() => BetaFeature.Disable("Zipline Launcher");

    [MenuButton("Rideable Rockets", ButtonType.Toggle, Category = "Movement", Order = 704, runOption = RunOption.Once, Controls = "(LG)(RG)", Description = "Grip either hand rocket and aim its upward axis.")]
    public static void EnableRideableRockets() => BetaFeature.Enable<ExpansionMovement>("Rideable Rockets");
    [MenuDisable("Rideable Rockets")]
    public static void DisableRideableRockets() => BetaFeature.Disable("Rideable Rockets");

    [MenuButton("Nailgun Climbing", ButtonType.Toggle, Category = "Movement", Order = 705, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Place solid pegs, then grip near a peg to climb.")]
    public static void EnableNailgunClimbing() => BetaFeature.Enable<ExpansionMovement>("Nailgun Climbing");
    [MenuDisable("Nailgun Climbing")]
    public static void DisableNailgunClimbing() => BetaFeature.Disable("Nailgun Climbing");

    [MenuButton("Auto Walk", ButtonType.Toggle, Category = "Movement", Order = 706, runOption = RunOption.Once, Controls = "", Description = "Walk in your horizontal head direction.")]
    public static void EnableAutoWalk() => BetaFeature.Enable<ExpansionMovement>("Auto Walk");
    [MenuDisable("Auto Walk")]
    public static void DisableAutoWalk() => BetaFeature.Disable("Auto Walk");

    [MenuButton("Auto Branch", ButtonType.Toggle, Category = "Movement", Order = 707, runOption = RunOption.Once, Controls = "(RG)", Description = "Traverse toward a nearby forward branch while holding RG.")]
    public static void EnableAutoBranch() => BetaFeature.Enable<ExpansionMovement>("Auto Branch");
    [MenuDisable("Auto Branch")]
    public static void DisableAutoBranch() => BetaFeature.Disable("Auto Branch");

    [MenuButton("Air Swim", ButtonType.Toggle, Category = "Movement", Order = 708, runOption = RunOption.Once, Controls = "", Description = "Creates a personal copy of a loaded water volume; requires a water-containing scene.")]
    public static void EnableAirSwim() => BetaFeature.Enable<ExpansionMovement>("Air Swim");
    [MenuDisable("Air Swim")]
    public static void DisableAirSwim() => BetaFeature.Disable("Air Swim");

    [MenuButton("Controller Flick", ButtonType.Toggle, Category = "Movement", Order = 709, runOption = RunOption.Once, Controls = "(X)", Description = "Store a hand motion as a short inertial locomotion stroke.")]
    public static void EnableControllerFlick() => BetaFeature.Enable<ExpansionMovement>("Controller Flick");
    [MenuDisable("Controller Flick")]
    public static void DisableControllerFlick() => BetaFeature.Disable("Controller Flick");

    [MenuButton("Wall Kick", ButtonType.Toggle, Category = "Movement", Order = 710, runOption = RunOption.Once, Controls = "(A)", Description = "Press A near a wall to kick away from its surface.")]
    public static void EnableWallKick() => BetaFeature.Enable<ExpansionMovement>("Wall Kick");
    [MenuDisable("Wall Kick")]
    public static void DisableWallKick() => BetaFeature.Disable("Wall Kick");

    [MenuButton("Ledge Hang", ButtonType.Toggle, Category = "Movement", Order = 711, runOption = RunOption.Once, Controls = "(RG)", Description = "Hold RG near a ledge to catch it; release to let go.")]
    public static void EnableLedgeHang() => BetaFeature.Enable<ExpansionMovement>("Ledge Hang");
    [MenuDisable("Ledge Hang")]
    public static void DisableLedgeHang() => BetaFeature.Disable("Ledge Hang");

    [MenuButton("Parkour Vault", ButtonType.Toggle, Category = "Movement", Order = 712, runOption = RunOption.Once, Controls = "(A)", Description = "Press A before a low obstacle with clear headroom.")]
    public static void EnableParkourVault() => BetaFeature.Enable<ExpansionMovement>("Parkour Vault");
    [MenuDisable("Parkour Vault")]
    public static void DisableParkourVault() => BetaFeature.Disable("Parkour Vault");

    [MenuButton("Wind Tunnel Builder", ButtonType.Toggle, Category = "Movement", Order = 713, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Place a local directional wind volume.")]
    public static void EnableWindTunnelBuilder() => BetaFeature.Enable<ExpansionMovement>("Wind Tunnel Builder");
    [MenuDisable("Wind Tunnel Builder")]
    public static void DisableWindTunnelBuilder() => BetaFeature.Disable("Wind Tunnel Builder");

    [MenuButton("Moving Walkway", ButtonType.Toggle, Category = "Movement", Order = 714, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Place a local conveyor surface; it accelerates you along its arrow.")]
    public static void EnableMovingWalkway() => BetaFeature.Enable<ExpansionMovement>("Moving Walkway");
    [MenuDisable("Moving Walkway")]
    public static void DisableMovingWalkway() => BetaFeature.Disable("Moving Walkway");

    [MenuButton("Bungee Anchor", ButtonType.Toggle, Category = "Movement", Order = 715, runOption = RunOption.Once, Controls = "(RG)(RT)(B)", Description = "Place an elastic tether anchor; B detaches.")]
    public static void EnableBungeeAnchor() => BetaFeature.Enable<ExpansionMovement>("Bungee Anchor");
    [MenuDisable("Bungee Anchor")]
    public static void DisableBungeeAnchor() => BetaFeature.Disable("Bungee Anchor");

}
