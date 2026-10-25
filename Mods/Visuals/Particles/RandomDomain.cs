// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers.Particles;
using ParticleEffectBase = ChudOS.Core.Managers.Particles.ParticleEffectBase;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ChudOS.Mods.Visuals.Particles;

internal sealed class RandomDomain : ParticleEffectBase
{
    private const int PoolSize = 48;
    private const float Interval = 0.05f;
    private readonly GameObject[] lines = new GameObject[PoolSize], orbs = new GameObject[PoolSize];
    private readonly LineRenderer[] renderers = new LineRenderer[PoolSize];
    private readonly Renderer[] orbRenderers = new Renderer[PoolSize];
    private readonly float[] lineRemaining = new float[PoolSize], orbRemaining = new float[PoolSize];
    private readonly MaterialPropertyBlock properties = new();
    private float nextSpawn;
    private int cursor;
    internal RandomDomain() : base("Random Domain") { }
    internal override void Create()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            var line = new GameObject("Domain Line");
            line.transform.SetParent(Root.transform, false);
            lines[i] = line;
            renderers[i] = Line(line, 0.05f);
            line.SetActive(false);
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Domain Orb";
            orb.SetActive(false);
            orb.transform.SetParent(Root.transform, false);
            orb.transform.localScale = Vector3.one * 0.3f;
            var collider = orb.GetComponent<Collider>();
            collider.enabled = false;
            Object.Destroy(collider);
            var renderer = orb.GetComponent<Renderer>();
            renderer.sharedMaterial = ParticleManager.LineMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            orbs[i] = orb;
            orbRenderers[i] = renderer;
        }
    }
    internal override bool Tick(float delta)
    {
        for (int i = 0; i < PoolSize; i++)
        {
            if (lineRemaining[i] > 0f)
            {
                lineRemaining[i] -= delta;
                if (lineRemaining[i] <= 0f) lines[i].SetActive(false);
            }
            if (orbRemaining[i] > 0f)
            {
                orbRemaining[i] -= delta;
                if (orbRemaining[i] <= 0f) orbs[i].SetActive(false);
            }
        }
        nextSpawn -= delta;
        if (nextSpawn > 0f) return true;
        nextSpawn = Interval;
        int slot = cursor;
        cursor = (cursor + 1) % PoolSize;
        Vector3 center = Root.transform.position;
        var line = renderers[slot];
        line.startColor = line.endColor = Random.ColorHSV();
        line.SetPosition(0, center + Offset());
        line.SetPosition(1, center + Offset());
        lineRemaining[slot] = 1.5f;
        lines[slot].SetActive(true);
        orbs[slot].transform.position = center + Offset();
        properties.Clear();
        properties.SetColor("_Color", Random.ColorHSV());
        orbRenderers[slot].SetPropertyBlock(properties);
        orbRemaining[slot] = 2f;
        orbs[slot].SetActive(true);
        return true;
    }
    private static Vector3 Offset() => new(Random.Range(-25f, 25f), Random.Range(-25f, 25f), Random.Range(-25f, 25f));
    protected override void OnStop()
    {
        nextSpawn = 0f;
        cursor = 0;
        for (int i = 0; i < PoolSize; i++)
        {
            lineRemaining[i] = orbRemaining[i] = 0f;
            if (lines[i] != null) lines[i].SetActive(false);
            if (orbs[i] != null) orbs[i].SetActive(false);
        }
    }
}
