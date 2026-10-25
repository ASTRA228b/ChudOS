// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers.Particles;
using ParticleEffectBase = ChudOS.Core.Managers.Particles.ParticleEffectBase;
using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Mods.Visuals.Particles;

internal sealed class FireFists : ParticleEffectBase
{
    private ParticleSystem left = null!, right = null!;
    internal FireFists() : base("Fire Fists") { }
    internal override void Create()
    {
        left = Particles("Left Fist", Color.red, 0.05f, 1.5f, 0.25f, 40, 15f, 0.08f);
        right = Particles("Right Fist", Color.red, 0.05f, 1.5f, 0.25f, 40, 15f, 0.08f);
    }
    internal override bool Tick(float delta)
    {
        var player = GTPlayer.Instance;
        if (player == null) return false;
        var leftHand = player.LeftHand.controllerTransform;
        var rightHand = player.RightHand.controllerTransform;
        if (leftHand == null || rightHand == null) return false;
        left.transform.SetPositionAndRotation(leftHand.position, leftHand.rotation);
        right.transform.SetPositionAndRotation(rightHand.position, rightHand.rotation);
        return true;
    }
}
