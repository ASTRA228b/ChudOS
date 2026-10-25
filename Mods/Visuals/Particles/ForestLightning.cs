// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers.Particles;
using ParticleEffectBase = ChudOS.Core.Managers.Particles.ParticleEffectBase;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChudOS.Mods.Visuals.Particles;

internal sealed class ForestLightning : ParticleEffectBase
{
    private const int PoolSize = 8;
    private const float Interval = 0.08f;
    private readonly GameObject[] bolts = new GameObject[PoolSize];
    private readonly LineRenderer[] lines = new LineRenderer[PoolSize];
    private readonly Light[] lights = new Light[PoolSize];
    private readonly float[] remaining = new float[PoolSize], flashes = new float[PoolSize];
    private float nextStrike;
    private int cursor;
    internal ForestLightning() : base("Forest Lightning") { }
    internal override void Create()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            var obj = new GameObject("Lightning Strike");
            obj.transform.SetParent(Root.transform, false);
            bolts[i] = obj;
            lines[i] = Line(obj, 0.1f);
            lines[i].startColor = Color.white;
            lines[i].endColor = new Color(0.5f, 0.5f, 1f);
            var light = obj.AddComponent<Light>();
            light.color = Color.white;
            light.range = 10f;
            light.shadows = LightShadows.None;
            lights[i] = light;
            obj.SetActive(false);
        }
    }
    internal override bool Tick(float delta)
    {
        for (int i = 0; i < PoolSize; i++)
        {
            if (remaining[i] <= 0f) continue;
            remaining[i] -= delta;
            flashes[i] -= delta;
            if (flashes[i] <= 0f) lights[i].enabled = false;
            if (remaining[i] <= 0f) bolts[i].SetActive(false);
        }
        nextStrike -= delta;
        if (nextStrike > 0f) return true;
        nextStrike = Interval;
        int slot = cursor;
        cursor = (cursor + 1) % PoolSize;
        Vector3 center = Root.transform.position;
        Vector3 start = center + new Vector3(Random.Range(-25f, 25f), Random.Range(10f, 20f), Random.Range(-25f, 25f));
        Vector3 end = center + new Vector3(Random.Range(-10f, 10f), -Random.Range(10f, 20f), Random.Range(-10f, 10f));
        bolts[slot].transform.position = Vector3.Lerp(start, end, 0.5f);
        lines[slot].SetPosition(0, start);
        lines[slot].SetPosition(1, end);
        remaining[slot] = Random.Range(0.1f, 0.4f);
        flashes[slot] = 0.08f;
        lights[slot].intensity = Random.Range(2.5f, 4f);
        lights[slot].enabled = true;
        bolts[slot].SetActive(true);
        return true;
    }
    protected override void OnStop()
    {
        nextStrike = 0f;
        cursor = 0;
        for (int i = 0; i < PoolSize; i++)
        {
            remaining[i] = flashes[i] = 0f;
            if (bolts[i] != null) bolts[i].SetActive(false);
        }
    }
}
