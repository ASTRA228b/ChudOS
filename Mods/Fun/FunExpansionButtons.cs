// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Fun;

public static class FunExpansionButtons
{
    [MenuButton("Physics Sandbox", ButtonType.Toggle, Category = "Fun", Order = 700, runOption = RunOption.Once, Controls = "(RG)(RT)(A)(B)(X)", Description = "Aim/click places a prop. RG/RMB grabs and releases it. B/R changes shape, X/F joins the last two props, A/Space resets.")]
    public static void EnablePhysicsSandbox() => BetaFeature.Enable<ExpansionToys>("Physics Sandbox");
    [MenuDisable("Physics Sandbox")]
    public static void DisablePhysicsSandbox() => BetaFeature.Disable("Physics Sandbox");

    [MenuButton("Domino Builder", ButtonType.Toggle, Category = "Fun", Order = 701, runOption = RunOption.Once, Controls = "(RG)(RT)(A)(X)", Description = "Aim/click adds a domino. X/F tips the first domino. RG/RMB grabs props; A/Space resets.")]
    public static void EnableDominoBuilder() => BetaFeature.Enable<ExpansionToys>("Domino Builder");
    [MenuDisable("Domino Builder")]
    public static void DisableDominoBuilder() => BetaFeature.Disable("Domino Builder");

    [MenuButton("Marble Run", ButtonType.Toggle, Category = "Fun", Order = 702, runOption = RunOption.Once, Controls = "(RG)(RT)(A)", Description = "Aim/click adds a ramp. RG/RMB moves the marble; A/Space rebuilds the course.")]
    public static void EnableMarbleRun() => BetaFeature.Enable<ExpansionToys>("Marble Run");
    [MenuDisable("Marble Run")]
    public static void DisableMarbleRun() => BetaFeature.Disable("Marble Run");

    [MenuButton("Force Lab", ButtonType.Toggle, Category = "Fun", Order = 703, runOption = RunOption.Once, Controls = "(LG)(RG)(A)", Description = "LG/Shift attracts the local props; RG/RMB repels them. A/Space resets.")]
    public static void EnableForceLab() => BetaFeature.Enable<ExpansionToys>("Force Lab");
    [MenuDisable("Force Lab")]
    public static void DisableForceLab() => BetaFeature.Disable("Force Lab");

    [MenuButton("Rope Bridge Builder", ButtonType.Toggle, Category = "Fun", Order = 704, runOption = RunOption.Once, Controls = "(RG)(RT)(A)", Description = "Aim/click two anchors 1–10m apart to build a local hinged bridge. A/Space clears it.")]
    public static void EnableRopeBridgeBuilder() => BetaFeature.Enable<ExpansionToys>("Rope Bridge Builder");
    [MenuDisable("Rope Bridge Builder")]
    public static void DisableRopeBridgeBuilder() => BetaFeature.Disable("Rope Bridge Builder");

    [MenuButton("RC Drone", ButtonType.Toggle, Category = "Fun", Order = 705, runOption = RunOption.Once, Controls = "(Left Stick Axis)(A)(LG)", Description = "Left stick or arrows steer the drone. A/Space climbs; LG/Shift descends. The GUI shows its camera.")]
    public static void EnableRCDrone() => BetaFeature.Enable<ExpansionToys>("RC Drone");
    [MenuDisable("RC Drone")]
    public static void DisableRCDrone() => BetaFeature.Disable("RC Drone");

    [MenuButton("Boomerang", ButtonType.Toggle, Category = "Fun", Order = 706, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Grab with RG/RMB, swing and release. It returns toward the hand after a short flight. A/Space resets.")]
    public static void EnableBoomerang() => BetaFeature.Enable<ExpansionToys>("Boomerang");
    [MenuDisable("Boomerang")]
    public static void DisableBoomerang() => BetaFeature.Disable("Boomerang");

    [MenuButton("Paper Planes", ButtonType.Toggle, Category = "Fun", Order = 707, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Grab with RG/RMB, swing and release the plane to glide. A/Space resets.")]
    public static void EnablePaperPlanes() => BetaFeature.Enable<ExpansionToys>("Paper Planes");
    [MenuDisable("Paper Planes")]
    public static void DisablePaperPlanes() => BetaFeature.Disable("Paper Planes");

    [MenuButton("Yo-Yo", ButtonType.Toggle, Category = "Fun", Order = 708, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Move your hand to pull the tethered yo-yo. RG/RMB grabs it; A/Space resets.")]
    public static void EnableYoYo() => BetaFeature.Enable<ExpansionToys>("Yo-Yo");
    [MenuDisable("Yo-Yo")]
    public static void DisableYoYo() => BetaFeature.Disable("Yo-Yo");

    [MenuButton("Juggling Practice", ButtonType.Toggle, Category = "Fun", Order = 709, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Grab and release the balls with RG/RMB; hand motion supplies throw velocity. A/Space resets.")]
    public static void EnableJugglingPractice() => BetaFeature.Enable<ExpansionToys>("Juggling Practice");
    [MenuDisable("Juggling Practice")]
    public static void DisableJugglingPractice() => BetaFeature.Disable("Juggling Practice");

    [MenuButton("Mini Bowling", ButtonType.Toggle, Category = "Fun", Order = 710, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Roll the ball toward the pins using RG/RMB grab and release. A/Space resets.")]
    public static void EnableMiniBowling() => BetaFeature.Enable<ExpansionToys>("Mini Bowling");
    [MenuDisable("Mini Bowling")]
    public static void DisableMiniBowling() => BetaFeature.Disable("Mini Bowling");

    [MenuButton("Ring Toss", ButtonType.Toggle, Category = "Fun", Order = 711, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Grab and toss rings over the peg with RG/RMB. A/Space resets.")]
    public static void EnableRingToss() => BetaFeature.Enable<ExpansionToys>("Ring Toss");
    [MenuDisable("Ring Toss")]
    public static void DisableRingToss() => BetaFeature.Disable("Ring Toss");

    [MenuButton("Virtual Darts", ButtonType.Toggle, Category = "Fun", Order = 712, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Grab and throw a dart toward the board with RG/RMB. A/Space resets.")]
    public static void EnableVirtualDarts() => BetaFeature.Enable<ExpansionToys>("Virtual Darts");
    [MenuDisable("Virtual Darts")]
    public static void DisableVirtualDarts() => BetaFeature.Disable("Virtual Darts");

    [MenuButton("Cup Stacking", ButtonType.Toggle, Category = "Fun", Order = 713, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Grab the cylinders with RG/RMB and stack them on the surface. A/Space resets.")]
    public static void EnableCupStacking() => BetaFeature.Enable<ExpansionToys>("Cup Stacking");
    [MenuDisable("Cup Stacking")]
    public static void DisableCupStacking() => BetaFeature.Disable("Cup Stacking");

    [MenuButton("Air Hockey", ButtonType.Toggle, Category = "Fun", Order = 714, runOption = RunOption.Once, Controls = "(Hands)(RG)(A)", Description = "Push the puck with your hands or grab it with RG/RMB. A/Space resets.")]
    public static void EnableAirHockey() => BetaFeature.Enable<ExpansionToys>("Air Hockey");
    [MenuDisable("Air Hockey")]
    public static void DisableAirHockey() => BetaFeature.Disable("Air Hockey");

    [MenuButton("Pocket Basketball", ButtonType.Toggle, Category = "Fun", Order = 715, runOption = RunOption.Once, Controls = "(RG)(A)", Description = "Grab and throw the ball through the hoop with RG/RMB. A/Space resets.")]
    public static void EnablePocketBasketball() => BetaFeature.Enable<ExpansionToys>("Pocket Basketball");
    [MenuDisable("Pocket Basketball")]
    public static void DisablePocketBasketball() => BetaFeature.Disable("Pocket Basketball");

    [MenuButton("Rhythm Drums", ButtonType.Toggle, Category = "Fun", Order = 716, runOption = RunOption.Once, Controls = "(Hands)(A)", Description = "Touch the four pads with either hand to play notes. A/Space resets.")]
    public static void EnableRhythmDrums() => BetaFeature.Enable<ExpansionToys>("Rhythm Drums");
    [MenuDisable("Rhythm Drums")]
    public static void DisableRhythmDrums() => BetaFeature.Disable("Rhythm Drums");

    [MenuButton("Metronome", ButtonType.Toggle, Category = "Fun", Order = 717, runOption = RunOption.Once, Controls = "(GUI)", Description = "Use the practice window to change BPM. Practice Audio Volume controls its sound.")]
    public static void EnableMetronome() => BetaFeature.Enable<ExpansionToys>("Metronome");
    [MenuDisable("Metronome")]
    public static void DisableMetronome() => BetaFeature.Disable("Metronome");

    [MenuButton("Reaction Trainer", ButtonType.Toggle, Category = "Fun", Order = 718, runOption = RunOption.Once, Controls = "(Hands)(A)", Description = "Touch the highlighted pad as quickly as possible. A/Space resets the score.")]
    public static void EnableReactionTrainer() => BetaFeature.Enable<ExpansionToys>("Reaction Trainer");
    [MenuDisable("Reaction Trainer")]
    public static void DisableReactionTrainer() => BetaFeature.Disable("Reaction Trainer");

    [MenuButton("Memory Pads", ButtonType.Toggle, Category = "Fun", Order = 719, runOption = RunOption.Once, Controls = "(Hands)(A)", Description = "Watch the flashing sequence, then repeat it by touching the pads. Lift your hand between taps; A/Space resets.")]
    public static void EnableMemoryPads() => BetaFeature.Enable<ExpansionToys>("Memory Pads");
    [MenuDisable("Memory Pads")]
    public static void DisableMemoryPads() => BetaFeature.Disable("Memory Pads");

    [MenuButton("Precision Landing Challenge", ButtonType.Toggle, Category = "Fun", Order = 720, runOption = RunOption.Once, Controls = "(Body)(A)", Description = "Land on the marked pad to measure your distance from its center. A/Space relocates it.")]
    public static void EnablePrecisionLandingChallenge() => BetaFeature.Enable<ExpansionToys>("Precision Landing Challenge");
    [MenuDisable("Precision Landing Challenge")]
    public static void DisablePrecisionLandingChallenge() => BetaFeature.Disable("Precision Landing Challenge");

    [MenuButton("Balance Beam Challenge", ButtonType.Toggle, Category = "Fun", Order = 721, runOption = RunOption.Once, Controls = "(Body)(A)", Description = "Cross the narrow physical beam to its far end. A/Space resets the timer.")]
    public static void EnableBalanceBeamChallenge() => BetaFeature.Enable<ExpansionToys>("Balance Beam Challenge");
    [MenuDisable("Balance Beam Challenge")]
    public static void DisableBalanceBeamChallenge() => BetaFeature.Disable("Balance Beam Challenge");

    [MenuButton("Laser Maze", ButtonType.Toggle, Category = "Fun", Order = 722, runOption = RunOption.Once, Controls = "(Body)(A)", Description = "Reach the far end without touching the laser lines. Touches are counted locally; A/Space resets.")]
    public static void EnableLaserMaze() => BetaFeature.Enable<ExpansionToys>("Laser Maze");
    [MenuDisable("Laser Maze")]
    public static void DisableLaserMaze() => BetaFeature.Disable("Laser Maze");

    [MenuButton("Throwable Target Range", ButtonType.Toggle, Category = "Fun", Order = 723, runOption = RunOption.Once, Controls = "(Projectiles)(A)", Description = "Hit the local targets with active native projectiles. A/Space resets the targets.")]
    public static void EnableThrowableTargetRange() => BetaFeature.Enable<ExpansionToys>("Throwable Target Range");
    [MenuDisable("Throwable Target Range")]
    public static void DisableThrowableTargetRange() => BetaFeature.Disable("Throwable Target Range");

    [MenuButton("Theremin", ButtonType.Toggle, Category = "Fun", Order = 724, runOption = RunOption.Once, Controls = "(Hands)", Description = "Right-hand height changes pitch. Left-hand distance changes volume. Uses Practice Audio Volume.")]
    public static void EnableTheremin() => BetaFeature.Enable<ExpansionToys>("Theremin");
    [MenuDisable("Theremin")]
    public static void DisableTheremin() => BetaFeature.Disable("Theremin");

}
