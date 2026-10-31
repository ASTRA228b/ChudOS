// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Core.Managers.Particles;

internal static class ParticleManager
{
    private static readonly Dictionary<string, ParticleEffectBase> effects = new();
    private static Material? particleMaterial, lineMaterial;
    internal static Material ParticleMaterial => particleMaterial ??= CreateMaterial("Particles/Standard Unlit", "Sprites/Default");
    internal static Material LineMaterial => lineMaterial ??= CreateMaterial("Sprites/Default", "Unlit/Color");

    private static Material CreateMaterial(string preferred, string fallback)
    {
        Shader shader = Shader.Find(preferred) ?? Shader.Find(fallback);
        if (shader == null) throw new InvalidOperationException("No suitable particle shader is available.");
        var material = new Material(shader) { name = "ChudOS Particle Material" };
        if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
        if (material.HasProperty("_TintColor")) material.SetColor("_TintColor", Color.white);
        return material;
    }
    internal static void Enable(string name, Func<ParticleEffectBase> factory, Vector3 position)
    {
        if (!PortTools.Ready)
        {
            ButtonManager.SetEnabled(name, false);
            NotifyLib.SendNotificationALL("Wait until the player has loaded.", 3000);
            return;
        }
        PortTools.Register(Shutdown);
        if (effects.TryGetValue(name, out var previous) && !previous.Alive)
        {
            previous.Destroy();
            effects.Remove(name);
        }
        if (!effects.TryGetValue(name, out var effect))
        {
            ParticleEffectBase? created = null;
            try
            {
                _ = ParticleMaterial;
                _ = LineMaterial;
                created = factory();
                created.Create();
                effects.Add(name, created);
                effect = created;
            }
            catch { created?.Destroy(); throw; }
        }
        try { effect.Start(position); }
        catch { effect.Stop(); throw; }
        PortTools.Register(Shutdown, Tick);
    }
    internal static void Disable(string name)
    {
        if (effects.TryGetValue(name, out var effect)) effect.Stop();
        UpdateRegistration();
    }
    internal static void ApplySettings()
    {
        foreach (var effect in effects.Values) if (effect.Alive) effect.ApplySettings();
    }
    private static void Tick()
    {
        float delta = Mathf.Min(Time.deltaTime, 0.25f);
        foreach (var pair in effects)
        {
            if (!pair.Value.Enabled) continue;
            try { if (PortTools.Ready && pair.Value.Alive && pair.Value.Tick(delta)) continue; }
            catch (Exception ex) { Debug.LogWarning($"[ChudOS] {pair.Key}: {ex.Message}"); }
            pair.Value.Stop();
            ButtonManager.SetEnabled(pair.Key, false);
        }
        UpdateRegistration();
    }
    private static void UpdateRegistration()
    {
        foreach (var effect in effects.Values) if (effect.Enabled) return;
        PortTools.RemoveTick(Tick);
    }
    internal static void Shutdown()
    {
        foreach (var pair in effects)
        {
            ButtonManager.SetEnabled(pair.Key, false);
            pair.Value.Destroy();
        }
        effects.Clear();
        PortTools.RemoveTick(Tick);
        if (particleMaterial != null) Object.Destroy(particleMaterial);
        if (lineMaterial != null) Object.Destroy(lineMaterial);
        particleMaterial = lineMaterial = null;
    }
}
