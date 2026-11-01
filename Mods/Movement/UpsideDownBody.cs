// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Mods.Movement;

internal sealed class UpsideDownBody : BetaFeature
{
    private GTPlayer? player;
    private Quaternion rotation;
    [MenuButton("Upside Down Body", ButtonType.Toggle, Category = "Movement", Order = 820, runOption = RunOption.Once, Description = "Invert body orientation and gravity using the native override APIs.")]
    public static void EnableBody() => Enable<UpsideDownBody>("Upside Down Body");
    [MenuDisable("Upside Down Body")]
    public static void DisableBody() => Disable("Upside Down Body");
    protected override void Setup() { player = GTPlayer.Instance; rotation = Quaternion.Euler(0f, CreativeTools.Head.eulerAngles.y, 180f); player.SetGravityOverride(this, ApplyGravity); }
    private void ApplyGravity(GTPlayer current) => current.AddForce(-Physics.gravity, ForceMode.Acceleration);
    protected override void Tick()
    {
        if (GTPlayer.Instance != player) { Unavailable("the player instance changed."); return; }
        GTPlayerTransform.ApplyRotationOverride(in rotation, Time.frameCount);
    }
    protected override void Cleanup() { if (player != null) player.UnsetGravityOverride(this); }
}
