// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers.Particles;
using ChudOS.Libraries;
using ChudOS.Mods.Visuals.Particles;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Visuals;

public static class ParticleMods
{
    private static Vector3 Center()
    {
        if (!PortTools.Ready) return Vector3.zero;
        Vector3 forward = GorillaTagger.Instance.headCollider.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        return GorillaTagger.Instance.bodyCollider.transform.position + forward.normalized * 3f;
    }
    [MenuButton("Fire Fists", ButtonType.Toggle, Category = "Visuals", Order = 120, runOption = RunOption.Once, Description = "Flame particles follow both controller hands")]
    public static void FireFists() => ParticleManager.Enable("Fire Fists", () => new FireFists(), Center());
    [MenuDisable("Fire Fists")]
    public static void StopFireFists() => ParticleManager.Disable("Fire Fists");

    [MenuButton("Forest Lightning", ButtonType.Toggle, Category = "Visuals", Order = 121, runOption = RunOption.Once, Description = "Local lightning strikes around Forest")]
    public static void Lightning() => ParticleManager.Enable("Forest Lightning", () => new ForestLightning(), new Vector3(-63.2589f, 9.4352f, -65.2775f));
    [MenuDisable("Forest Lightning")]
    public static void StopLightning() => ParticleManager.Disable("Forest Lightning");

    [MenuButton("Nebula Storm", ButtonType.Toggle, Category = "Visuals", Order = 122, runOption = RunOption.Once, Description = "Place a nebula cloud and stars ahead of you")]
    public static void Nebula() => ParticleManager.Enable("Nebula Storm", () => new NebulaStorm(), Center());
    [MenuDisable("Nebula Storm")]
    public static void StopNebula() => ParticleManager.Disable("Nebula Storm");

    [MenuButton("Black Hole", ButtonType.Toggle, Category = "Visuals", Order = 123, runOption = RunOption.Once, Description = "Place a dark inward particle swirl")]
    public static void BlackHole() => ParticleManager.Enable("Black Hole", () => new BlackHoleEffect(), Center());
    [MenuDisable("Black Hole")]
    public static void StopBlackHole() => ParticleManager.Disable("Black Hole");

    [MenuButton("White Hole", ButtonType.Toggle, Category = "Visuals", Order = 124, runOption = RunOption.Once, Description = "Place a bright outward particle swirl")]
    public static void WhiteHole() => ParticleManager.Enable("White Hole", () => new WhiteHoleEffect(), Center());
    [MenuDisable("White Hole")]
    public static void StopWhiteHole() => ParticleManager.Disable("White Hole");

    [MenuButton("Random Domain", ButtonType.Toggle, Category = "Visuals", Order = 125, runOption = RunOption.Once, Description = "Local colored lines and floating orbs")]
    public static void Domain() => ParticleManager.Enable("Random Domain", () => new RandomDomain(), Center());
    [MenuDisable("Random Domain")]
    public static void StopDomain() => ParticleManager.Disable("Random Domain");
}
