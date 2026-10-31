// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Visuals;

public static class CreativeVisuals
{
    private static readonly CreativeTools.Session Session = new();
    private static GameObject? ribbonRoot, sparksRoot;
    private static TrailRenderer? ribbon;
    private static ParticleSystem? sparks;
    private static readonly bool[] Touching = new bool[2];
    private static readonly Vector3[] HandPositions = new Vector3[2];
    private static bool sampledHands, sampledLanding, grounded;
    private static float lastVertical;
    private static readonly LineRenderer?[] Pulses = new LineRenderer?[4];
    private static readonly Vector3[] PulsePositions = new Vector3[4], PulseNormals = new Vector3[4];
    private static readonly float[] PulseStarts = new float[4], PulsePower = new float[4];
    private static int pulseIndex;
    private static bool Ready()
    {
        if (Session.Changed()) Clear();
        if (!CreativeTools.Ready) { Clear(); return false; }
        PortTools.Register(Clear);
        return true;
    }
    [MenuButton("Momentum Ribbon", ButtonType.Toggle, Category = "Visuals", Order = 400, runOption = RunOption.LateUpdate, Description = "A personal movement ribbon that changes width and color with your speed")]
    public static void MomentumRibbon()
    {
        if (!Ready()) return;
        if (ribbon == null)
        {
            ribbonRoot = new GameObject("ChudOS Momentum Ribbon");
            ribbon = ribbonRoot.AddComponent<TrailRenderer>();
            ribbon.sharedMaterial = CreativeTools.Material;
            ribbon.time = 0.9f; ribbon.minVertexDistance = 0.04f;
            ribbon.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ribbon.receiveShadows = false;
            ribbonRoot.transform.position = CreativeTools.Position;
            ribbon.Clear();
        }
        ribbonRoot!.transform.position = CreativeTools.Position;
        float speed = CreativeTools.Body.linearVelocity.magnitude / CreativeTools.Scale, amount = Mathf.Clamp01(speed / 12f);
        Color color = Color.Lerp(CreativeSettings.Color(CreativeTools.Theme), Color.white, amount);
        ribbon.startColor = color; ribbon.endColor = new Color(color.r, color.g, color.b, 0f);
        ribbon.startWidth = Mathf.Lerp(0.015f, 0.09f, amount) * CreativeTools.Scale * CreativeSettings.EffectSize;
        ribbon.endWidth = 0f; ribbon.emitting = speed > 0.3f;
    }
    [MenuDisable("Momentum Ribbon")]
    public static void StopRibbon()
    {
        if (ribbonRoot != null) Object.Destroy(ribbonRoot);
        ribbonRoot = null; ribbon = null;
    }
    [MenuButton("Contact Sparks", ButtonType.Toggle, Category = "Visuals", Order = 401, runOption = RunOption.LateUpdate, Description = "Local sparks appear when your hands first contact a surface")]
    public static void ContactSparks()
    {
        if (!Ready()) return;
        if (sparks == null)
        {
            sparksRoot = new GameObject("ChudOS Contact Sparks");
            sparks = sparksRoot.AddComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = sparks.main;
            main.playOnAwake = false; main.loop = false; main.startLifetime = 0.25f; main.startSpeed = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = sparks.emission; emission.enabled = false;
            var shape = sparks.shape; shape.enabled = false;
            var renderer = sparks.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = CreativeTools.Material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
        var settings = sparks.main;
        settings.maxParticles = Mathf.RoundToInt(96 * CreativeSettings.EffectQuality);
        float dt = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
        for (int i = 0; i < 2; i++)
        {
            Vector3 position = (i == 0 ? CreativeTools.Left.position : CreativeTools.Right.position) - CreativeTools.Position;
            bool touching = GTPlayer.Instance.IsHandTouching(i == 0);
            float speed = sampledHands ? (position - HandPositions[i]).magnitude / dt : 0f;
            if (sampledHands && touching && !Touching[i] && speed > 0.3f * CreativeTools.Scale && speed < 20f * CreativeTools.Scale)
            {
                ParticleSystem.EmitParams emit = new() { position = position + CreativeTools.Position, startColor = CreativeSettings.Color(CreativeTools.Theme), startSize = 0.025f * CreativeTools.Scale * CreativeSettings.EffectSize, startLifetime = 0.25f };
                int count = Mathf.Clamp(Mathf.RoundToInt(speed * CreativeSettings.EffectIntensity * CreativeSettings.EffectQuality), 2, 18);
                for (int p = 0; p < count; p++) { emit.velocity = UnityEngine.Random.insideUnitSphere * Mathf.Min(speed, 3f); sparks.Emit(emit, 1); }
            }
            HandPositions[i] = position; Touching[i] = touching;
        }
        sampledHands = true;
    }
    [MenuDisable("Contact Sparks")]
    public static void StopSparks()
    {
        if (sparksRoot != null) Object.Destroy(sparksRoot);
        sparksRoot = null; sparks = null; sampledHands = false; Touching[0] = Touching[1] = false;
    }
    [MenuButton("Landing Pulse", ButtonType.Toggle, Category = "Visuals", Order = 402, runOption = RunOption.LateUpdate, Description = "A local expanding ring appears when you land")]
    public static void LandingPulse()
    {
        if (!Ready()) return;
        bool onGround = CreativeTools.Ground(out RaycastHit hit);
        if (sampledLanding && onGround && !grounded && lastVertical < -1.5f * CreativeTools.Scale)
        {
            int slot = pulseIndex++ % Pulses.Length;
            if (Pulses[slot] == null) Pulses[slot] = CreativeTools.Line("Landing Pulse", 40, 0.025f, true);
            PulsePositions[slot] = hit.point + hit.normal * 0.015f; PulseNormals[slot] = hit.normal;
            PulseStarts[slot] = Time.unscaledTime; PulsePower[slot] = Mathf.Clamp(-lastVertical / CreativeTools.Scale * 0.12f, 0.4f, 2f);
        }
        for (int i = 0; i < Pulses.Length; i++)
        {
            LineRenderer? line = Pulses[i];
            if (line == null) continue;
            float age = (Time.unscaledTime - PulseStarts[i]) / 0.65f;
            line.enabled = age < 1f;
            if (!line.enabled) continue;
            CreativeTools.Circle(line, PulsePositions[i], PulseNormals[i], Mathf.Lerp(0.05f, PulsePower[i], age) * CreativeTools.Scale * CreativeSettings.EffectSize);
            Color color = CreativeSettings.Color(CreativeTools.Theme); color.a = 1f - age;
            line.startColor = line.endColor = color;
        }
        grounded = onGround; lastVertical = CreativeTools.Body.linearVelocity.y; sampledLanding = true;
    }
    [MenuDisable("Landing Pulse")]
    public static void StopPulses()
    {
        for (int i = 0; i < Pulses.Length; i++) { if (Pulses[i] != null) Object.Destroy(Pulses[i]!.gameObject); Pulses[i] = null; }
        sampledLanding = grounded = false; lastVertical = 0f; pulseIndex = 0;
    }
    internal static void Clear() { StopRibbon(); StopSparks(); StopPulses(); }
}
