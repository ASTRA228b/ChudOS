// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Visuals;

internal sealed class OptimizeGame : BetaFeature
{
    private int textures, antiAliasing, lights, maxLod;
    private float shadowDistance, lodBias;
    private bool softParticles, reflections;
    private ShadowQuality shadows;
    private AnisotropicFiltering filtering;
    private bool captured;

    [MenuButton("Optimize Game", ButtonType.Toggle, Category = "Visuals", Order = 720, runOption = RunOption.Once, Description = "Lowers textures, shadows, anti-aliasing and model detail. Restores previous quality settings when disabled.")]
    public static void EnableOptimization() => Enable<OptimizeGame>("Optimize Game");
    [MenuDisable("Optimize Game")]
    public static void DisableOptimization() => Disable("Optimize Game");

    protected override void Setup()
    {
        textures = QualitySettings.globalTextureMipmapLimit;
        antiAliasing = QualitySettings.antiAliasing;
        lights = QualitySettings.pixelLightCount;
        maxLod = QualitySettings.maximumLODLevel;
        shadowDistance = QualitySettings.shadowDistance;
        lodBias = QualitySettings.lodBias;
        softParticles = QualitySettings.softParticles;
        reflections = QualitySettings.realtimeReflectionProbes;
        shadows = QualitySettings.shadows;
        filtering = QualitySettings.anisotropicFiltering;
        captured = true;

        QualitySettings.globalTextureMipmapLimit = Mathf.Max(textures, 2);
        QualitySettings.antiAliasing = 0;
        QualitySettings.pixelLightCount = 0;
        QualitySettings.maximumLODLevel = Mathf.Max(maxLod, 1);
        QualitySettings.shadowDistance = 0f;
        QualitySettings.lodBias = Mathf.Min(lodBias, 0.35f);
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
    }

    protected override void Cleanup()
    {
        if (!captured) return;
        captured = false;
        QualitySettings.globalTextureMipmapLimit = textures;
        QualitySettings.antiAliasing = antiAliasing;
        QualitySettings.pixelLightCount = lights;
        QualitySettings.maximumLODLevel = maxLod;
        QualitySettings.shadowDistance = shadowDistance;
        QualitySettings.lodBias = lodBias;
        QualitySettings.softParticles = softParticles;
        QualitySettings.realtimeReflectionProbes = reflections;
        QualitySettings.shadows = shadows;
        QualitySettings.anisotropicFiltering = filtering;
    }
}
