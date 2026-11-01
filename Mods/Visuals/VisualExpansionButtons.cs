// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Visuals;

public static class VisualExpansionButtons
{
    [MenuButton("Minimap", ButtonType.Toggle, Category = "Visuals", Order = 700, runOption = RunOption.Once, Controls = "", Description = "Live overhead desktop view of the loaded scene.")]
    public static void EnableMinimap() => BetaFeature.Enable<ExpansionVisuals>("Minimap");
    [MenuDisable("Minimap")]
    public static void DisableMinimap() => BetaFeature.Disable("Minimap");

    [MenuButton("World Mirror", ButtonType.Toggle, Category = "Visuals", Order = 701, runOption = RunOption.Once, Controls = "", Description = "Place a local live camera mirror in front of you.")]
    public static void EnableWorldMirror() => BetaFeature.Enable<ExpansionVisuals>("World Mirror");
    [MenuDisable("World Mirror")]
    public static void DisableWorldMirror() => BetaFeature.Disable("World Mirror");

    [MenuButton("Fullbright", ButtonType.Toggle, Category = "Visuals", Order = 702, runOption = RunOption.Once, Controls = "", Description = "Adds flat ambient light and a local light; restores lighting on disable.")]
    public static void EnableFullbright() => BetaFeature.Enable<ExpansionVisuals>("Fullbright");
    [MenuDisable("Fullbright")]
    public static void DisableFullbright() => BetaFeature.Disable("Fullbright");

    [MenuButton("Underwater Vision", ButtonType.Toggle, Category = "Visuals", Order = 703, runOption = RunOption.Once, Controls = "", Description = "Disables loaded native underwater camera/particle effects and restores them on disable.")]
    public static void EnableUnderwaterVision() => BetaFeature.Enable<ExpansionVisuals>("Underwater Vision");
    [MenuDisable("Underwater Vision")]
    public static void DisableUnderwaterVision() => BetaFeature.Disable("Underwater Vision");

    [MenuButton("Skybox Editor", ButtonType.Toggle, Category = "Visuals", Order = 704, runOption = RunOption.Once, Controls = "", Description = "Edit a private copy of the current skybox tint.")]
    public static void EnableSkyboxEditor() => BetaFeature.Enable<ExpansionVisuals>("Skybox Editor");
    [MenuDisable("Skybox Editor")]
    public static void DisableSkyboxEditor() => BetaFeature.Disable("Skybox Editor");

    [MenuButton("Surface Inspector", ButtonType.Toggle, Category = "Visuals", Order = 705, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Aim at a collider to see its name, layer, type and surface normal.")]
    public static void EnableSurfaceInspector() => BetaFeature.Enable<ExpansionVisuals>("Surface Inspector");
    [MenuDisable("Surface Inspector")]
    public static void DisableSurfaceInspector() => BetaFeature.Disable("Surface Inspector");

    [MenuButton("Wind Barrier Viewer", ButtonType.Toggle, Category = "Visuals", Order = 706, runOption = RunOption.Once, Controls = "", Description = "Marks loaded native wind-barrier components.")]
    public static void EnableWindBarrierViewer() => BetaFeature.Enable<ExpansionVisuals>("Wind Barrier Viewer");
    [MenuDisable("Wind Barrier Viewer")]
    public static void DisableWindBarrierViewer() => BetaFeature.Disable("Wind Barrier Viewer");

    [MenuButton("Projectile Trajectory Preview", ButtonType.Toggle, Category = "Visuals", Order = 707, runOption = RunOption.Once, Controls = "", Description = "Local ballistic aiming guide using Toy Launch Power; not a server hit prediction.")]
    public static void EnableProjectileTrajectoryPreview() => BetaFeature.Enable<ExpansionVisuals>("Projectile Trajectory Preview");
    [MenuDisable("Projectile Trajectory Preview")]
    public static void DisableProjectileTrajectoryPreview() => BetaFeature.Disable("Projectile Trajectory Preview");

    [MenuButton("Photo Mode", ButtonType.Toggle, Category = "Visuals", Order = 708, runOption = RunOption.Once, Controls = "(A)", Description = "Desktop photo camera and screenshot capture to ChudOS/Photos.")]
    public static void EnablePhotoMode() => BetaFeature.Enable<ExpansionVisuals>("Photo Mode");
    [MenuDisable("Photo Mode")]
    public static void DisablePhotoMode() => BetaFeature.Disable("Photo Mode");

    [MenuButton("Camera Dolly", ButtonType.Toggle, Category = "Visuals", Order = 709, runOption = RunOption.Once, Controls = "(X)(A)", Description = "Record head-pose camera keyframes with X and play them with A.")]
    public static void EnableCameraDolly() => BetaFeature.Enable<ExpansionVisuals>("Camera Dolly");
    [MenuDisable("Camera Dolly")]
    public static void DisableCameraDolly() => BetaFeature.Disable("Camera Dolly");

    [MenuButton("NPC Tracker", ButtonType.Toggle, Category = "Visuals", Order = 710, runOption = RunOption.Once, Controls = "", Description = "Tracks supported loaded NPC/critter components.")]
    public static void EnableNPCTracker() => BetaFeature.Enable<ExpansionVisuals>("NPC Tracker");
    [MenuDisable("NPC Tracker")]
    public static void DisableNPCTracker() => BetaFeature.Disable("NPC Tracker");

    [MenuButton("Resource Tracker", ButtonType.Toggle, Category = "Visuals", Order = 711, runOption = RunOption.Once, Controls = "", Description = "Tracks supported loaded collectible/resource components.")]
    public static void EnableResourceTracker() => BetaFeature.Enable<ExpansionVisuals>("Resource Tracker");
    [MenuDisable("Resource Tracker")]
    public static void DisableResourceTracker() => BetaFeature.Disable("Resource Tracker");

}
