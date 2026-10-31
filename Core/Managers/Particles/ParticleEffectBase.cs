// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Core.Managers.Particles;

internal abstract class ParticleEffectBase
{
    protected readonly GameObject Root;
    private readonly List<ParticleSystem> systems = new();
    private sealed class ParticleDefaults
    {
        internal ParticleSystem System = null!;
        internal ParticleSystem.MinMaxCurve Size, Rate;
        internal ParticleSystem.MinMaxGradient Color;
        internal int Maximum;
    }
    private readonly List<ParticleDefaults> defaults = new();
    internal bool Enabled { get; private set; }
    internal bool Alive => Root != null;

    protected ParticleEffectBase(string name)
    {
        Root = new GameObject($"ChudOS {name}");
        Root.SetActive(false);
    }
    internal abstract void Create();
    internal virtual bool Tick(float delta) => true;
    protected virtual void OnStop() { }

    internal void Start(Vector3 position)
    {
        Root.transform.position = position;
        if (!Tick(0f)) throw new InvalidOperationException("The effect could not find its player transforms.");
        Enabled = true;
        ApplySettings();
        Root.SetActive(true);
        foreach (var system in systems) if (system != null) system.Play(false);
    }
    internal void Stop()
    {
        Enabled = false;
        if (Root == null) return;
        foreach (var system in systems) if (system != null) system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        OnStop();
        Root.SetActive(false);
    }
    internal void Destroy()
    {
        Stop();
        if (Root != null) Object.Destroy(Root);
        systems.Clear();
        defaults.Clear();
    }
    protected ParticleSystem Particles(string name, Color color, float size, float lifetime, float speed, int maximum, float rate, float radius)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(Root.transform, false);
        var system = obj.AddComponent<ParticleSystem>();
        var main = system.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startColor = color;
        main.startSize = size;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.maxParticles = maximum;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = system.emission;
        emission.rateOverTime = rate;
        var shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius;
        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = ParticleManager.ParticleMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        systems.Add(system);
        defaults.Add(new ParticleDefaults { System = system, Size = main.startSize, Color = main.startColor, Rate = emission.rateOverTime, Maximum = main.maxParticles });
        ApplySettings();
        return system;
    }
    internal void ApplySettings()
    {
        foreach (ParticleDefaults entry in defaults)
        {
            if (entry.System == null) continue;
            var main = entry.System.main;
            var emission = entry.System.emission;
            main.startSize = ScaleCurve(entry.Size, ChudOS.Mods.Settings.CreativeSettings.EffectSize);
            main.maxParticles = Mathf.Max(1, Mathf.RoundToInt(entry.Maximum * ChudOS.Mods.Settings.CreativeSettings.EffectQuality));
            float multiplier = ChudOS.Mods.Settings.CreativeSettings.EffectIntensity * ChudOS.Mods.Settings.CreativeSettings.EffectQuality;
            emission.rateOverTime = ScaleCurve(entry.Rate, multiplier);
            main.startColor = ChudOS.Mods.Settings.CreativeSettings.EffectColor == 0 ? entry.Color : new ParticleSystem.MinMaxGradient(ChudOS.Mods.Settings.CreativeSettings.Color(Color.white));
        }
    }
    private static ParticleSystem.MinMaxCurve ScaleCurve(ParticleSystem.MinMaxCurve curve, float multiplier)
    {
        if (curve.mode == ParticleSystemCurveMode.Constant) curve.constant *= multiplier;
        else if (curve.mode == ParticleSystemCurveMode.TwoConstants) { curve.constantMin *= multiplier; curve.constantMax *= multiplier; }
        else curve.curveMultiplier *= multiplier;
        return curve;
    }
    protected LineRenderer Line(GameObject obj, float width)
    {
        var line = obj.AddComponent<LineRenderer>();
        line.sharedMaterial = ParticleManager.LineMaterial;
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = line.endWidth = width;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        return line;
    }
}
