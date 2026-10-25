// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers.Particles;
using ParticleEffectBase = ChudOS.Core.Managers.Particles.ParticleEffectBase;
using UnityEngine;

namespace ChudOS.Mods.Visuals.Particles;

internal sealed class NebulaStorm : ParticleEffectBase
{
    internal NebulaStorm() : base("Nebula Storm") { }
    internal override void Create()
    {
        var cloud = Particles("Nebula", Color.white, 3f, 4f, 0f, 100, 5f, 2f);
        var main = cloud.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.2f, 0.3f, 0.6f, 0.3f), new Color(0.6f, 0.1f, 0.7f, 0.4f));
        var stars = Particles("Stars", Color.white, 0.05f, 2f, 0f, 50, 10f, 2.5f);
        var starMain = stars.main;
        starMain.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(1f, 1f, 0.8f));
    }
}
