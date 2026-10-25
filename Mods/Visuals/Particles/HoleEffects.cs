// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers.Particles;
using ParticleEffectBase = ChudOS.Core.Managers.Particles.ParticleEffectBase;
using UnityEngine;

namespace ChudOS.Mods.Visuals.Particles;

internal abstract class HoleEffect : ParticleEffectBase
{
    private readonly bool outward;
    protected HoleEffect(string name, bool outward) : base(name) { this.outward = outward; }
    internal override void Create()
    {
        var system = Particles("Swirl", outward ? Color.white : Color.black, 0.4f, 2f, 0f, 150, 30f, 2.5f);
        var main = system.main;
        main.startColor = outward
            ? new ParticleSystem.MinMaxGradient(Color.white, new Color(0.9f, 0.9f, 0.9f))
            : new ParticleSystem.MinMaxGradient(Color.black, new Color(0.1f, 0.1f, 0.1f));
        var rotation = system.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        var velocity = system.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.radial = outward ? 1.5f : -1.5f;
        velocity.orbitalY = 0.75f;
    }
}
internal sealed class BlackHoleEffect : HoleEffect
{
    internal BlackHoleEffect() : base("Black Hole", false) { }
}
internal sealed class WhiteHoleEffect : HoleEffect
{
    internal WhiteHoleEffect() : base("White Hole", true) { }
}
