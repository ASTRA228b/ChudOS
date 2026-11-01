// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Safety;

internal static class ExpansionConflicts
{
    private static readonly HashSet<string> Movement = new(StringComparer.Ordinal) { "Upside Down Body", "Airplane", "Bird Fly", "Bubble Locomotion", "Zipline Launcher", "Rideable Rockets", "Nailgun Climbing", "Auto Walk", "Auto Branch", "Air Swim", "Controller Flick", "Wall Kick", "Ledge Hang", "Parkour Vault", "Wind Tunnel Builder", "Moving Walkway", "Bungee Anchor" };
    private static readonly HashSet<string> ExistingMovement = new(StringComparer.Ordinal) { "Car", "Fly (X)", "Trigger Fly", "Joystick Fly (LJ)", "BarkFly (JR)(JL)", "Velocity BarkFly (JR)(JL)", "Hand Fly (B)", "WASD Fly", "NoClip Fly", "Drone Fly", "Velocity Movement", "Fly Towards Gun", "Spider Man", "Grappling Hooks", "Wall Walk", "Wall Walk V2", "Low Gravity", "High Gravity", "Zero Gravity", "Throw", "Double Jump", "Rewind", "Momentum Bank", "Surface Skate", "Air Paddle", "Gravity Compass", "Landing Brake", "Advanced Checkpoints", "Climby Hands", "Auto Climb", "Bunny Hop", "PC Walking", "Predictions" };
    private static readonly HashSet<string> Toys = new(StringComparer.Ordinal) { "Physics Sandbox", "Domino Builder", "Marble Run", "Force Lab", "Rope Bridge Builder", "RC Drone", "Boomerang", "Paper Planes", "Yo-Yo", "Juggling Practice", "Mini Bowling", "Ring Toss", "Virtual Darts", "Cup Stacking", "Air Hockey", "Pocket Basketball", "Rhythm Drums", "Metronome", "Reaction Trainer", "Memory Pads", "Precision Landing Challenge", "Balance Beam Challenge", "Laser Maze", "Throwable Target Range", "Theremin", "Echo Race", "Gate Runner", "Beat Targets" };
    private static readonly HashSet<string> Guns = new(StringComparer.Ordinal) { "Move Lucy Gun", "Prop Control Gun", "Teleport Gun", "Draw Gun", "Tag Gun", "Platform Gun", "Fly Towards Gun", "Rig Gun", "Portal Gun", "Hug Gun", "Follow Player Gun", "Orbit Player Gun", "Copy Movement Gun", "Look At Gun", "Jumpscare Gun", "Zipline Launcher", "Nailgun Climbing", "Wind Tunnel Builder", "Moving Walkway", "Bungee Anchor", "Surface Inspector", "Cosmetic Inspector", "Physics Sandbox", "Domino Builder", "Marble Run", "Rope Bridge Builder", "Projectile Gun", "Impact Painter" };
    private static readonly HashSet<string> Spammers = new(StringComparer.Ordinal) { "Snowball Spam", "Water Balloon Spam", "Projectile Gun", "Projectile Pattern Emitter", "Impact Painter", "Bubble Wand", "Confetti Cannon", "Firework Sequencer" };
    private static readonly HashSet<string> Holdables = new(StringComparer.Ordinal) { "Handheld Camera", "Squeeze Toy", "Custom Flashlight", "Measuring Tape", "Holdable Browser", "Equip Premade Holdable" };
    internal static void OnEnable(ButtonManager.ButtonData button)
    {
        if (button.Type != ButtonType.Toggle) return;
        foreach (var other in ButtonManager.Buttons)
        {
            if (other == button || other.Type != ButtonType.Toggle || !other.Enabled) continue;
            bool conflict = Movement.Contains(button.Name) && (Movement.Contains(other.Name) || ExistingMovement.Contains(other.Name)) || ExistingMovement.Contains(button.Name) && Movement.Contains(other.Name);
            conflict |= Toys.Contains(button.Name) && Toys.Contains(other.Name);
            conflict |= Guns.Contains(button.Name) && Guns.Contains(other.Name);
            conflict |= Spammers.Contains(button.Name) && Spammers.Contains(other.Name);
            conflict |= Holdables.Contains(button.Name) && Holdables.Contains(other.Name);
            conflict |= (button.Name is "Photo Mode" or "Camera Dolly") && (other.Name is "Photo Mode" or "Camera Dolly" or "Free Camera" or "Smooth Desktop Camera") || (button.Name is "Free Camera" or "Smooth Desktop Camera") && (other.Name is "Photo Mode" or "Camera Dolly");
            conflict |= button.Name == "Fullbright" && other.Name == "Ambient Tint" || button.Name == "Ambient Tint" && other.Name == "Fullbright";
            conflict |= (button.Name is "Sticky Holdables" or "Physical Holdables" or "Prop Control Gun") && (other.Name is "Sticky Holdables" or "Physical Holdables" or "Prop Control Gun");
            if (conflict) ButtonManager.SetEnabled(other, false);
        }
    }
}
