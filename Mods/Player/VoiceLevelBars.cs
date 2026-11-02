// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Player;

internal sealed class VoiceLevelBars : MonoBehaviour
{
    private sealed class Entry
    {
        internal GameObject Object = null!;
        internal Renderer Renderer = null!;
        internal GorillaSpeakerLoudness? Loudness;
    }
    private static VoiceLevelBars? instance;
    private readonly Dictionary<VRRig, Entry> bars = new();
    private readonly List<VRRig> removed = new();
    private float nextScan, nextVisual;
    private Color color;
    private bool painted;

    [MenuButton("Voice Level Bars", ButtonType.Toggle, Category = "Player", Order = 40, runOption = RunOption.Once, Description = "Show received microphone levels above players")]
    public static void EnableBars()
    {
        DisableBars();
        var obj = new GameObject("ChudOS Voice Level Bars"); Object.DontDestroyOnLoad(obj);
        instance = obj.AddComponent<VoiceLevelBars>();
    }
    [MenuDisable("Voice Level Bars")]
    public static void DisableBars()
    {
        var current = instance; instance = null;
        if (current == null) return;
        current.enabled = false; current.Clear(); Object.Destroy(current.gameObject);
    }
    private void Update()
    {
        float now = Time.unscaledTime;
        if (now >= nextScan) { nextScan = now + 1f; Scan(); }
        if (now < nextVisual) return;
        nextVisual = now + 1f / 30f;
        Color nextColor = CreativeTools.Theme;
        bool repaint = !painted || color != nextColor; color = nextColor; painted = true;
        foreach (var pair in bars)
        {
            VRRig rig = pair.Key; Entry entry = pair.Value;
            if (rig == null || entry.Object == null) continue;
            bool hasHead = rig.headMesh != null && rig.gameObject.activeInHierarchy;
            float level = hasHead && entry.Loudness != null ? Mathf.Clamp01(entry.Loudness.Loudness * 3f) : 0f;
            bool visible = level > 0.015f;
            if (entry.Object.activeSelf != visible) entry.Object.SetActive(visible);
            if (repaint) CreativeTools.Paint(entry.Renderer, color);
            if (!visible || rig.headMesh == null) continue;
            float scale = Mathf.Clamp(Mathf.Abs(rig.transform.lossyScale.y), 0.2f, 3f);
            float height = (0.02f + level * 0.18f) * scale;
            Transform bar = entry.Object.transform;
            bar.position = rig.headMesh.transform.position + Vector3.up * (0.28f * scale + height * 0.5f);
            bar.rotation = Quaternion.identity; bar.localScale = new Vector3(0.045f * scale, height, 0.045f * scale);
        }
    }
    private void Scan()
    {
        var rigs = VRRigCache.ActiveRigs;
        removed.Clear();
        foreach (var pair in bars)
            if (pair.Key == null || pair.Key.isLocal || rigs == null || !rigs.Contains(pair.Key)) removed.Add(pair.Key!);
        foreach (var rig in removed)
        {
            GameObject obj = bars[rig].Object; bars.Remove(rig);
            if (obj != null) { obj.SetActive(false); Object.Destroy(obj); }
        }
        if (rigs == null) return;
        foreach (VRRig rig in rigs)
        {
            if (rig == null || rig.isLocal) continue;
            if (bars.TryGetValue(rig, out var existing))
            {
                if (existing.Loudness == null) existing.Loudness = rig.GetComponent<GorillaSpeakerLoudness>();
                continue;
            }
            if (bars.Count >= 32) continue;
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = "ChudOS Voice Level"; obj.layer = 2;
            obj.transform.SetParent(transform, false);
            var collider = obj.GetComponent<Collider>(); collider.enabled = false; Object.Destroy(collider);
            Renderer renderer = obj.GetComponent<Renderer>(); renderer.sharedMaterial = CreativeTools.Material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            CreativeTools.Paint(renderer, CreativeTools.Theme); obj.SetActive(false);
            bars.Add(rig, new Entry { Object = obj, Renderer = renderer, Loudness = rig.GetComponent<GorillaSpeakerLoudness>() });
        }
    }
    private void Clear()
    {
        foreach (var entry in bars.Values)
            if (entry.Object != null) { entry.Object.SetActive(false); Object.Destroy(entry.Object); }
        bars.Clear(); removed.Clear();
    }
    private void OnDestroy() { Clear(); if (instance == this) instance = null; }
}
