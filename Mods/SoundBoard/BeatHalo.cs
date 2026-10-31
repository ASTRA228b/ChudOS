// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;
using MenuMain = ChudOS.Core.Main;

namespace ChudOS.Mods.Soundboard;

public static class BeatHalo
{
    private static LineRenderer? halo;
    private static readonly Transform?[] Orbs = new Transform?[12];
    private static float envelope;
    [MenuButton("Beat Halo", ButtonType.Toggle, Category = "Soundboard", Order = 400, runOption = RunOption.LateUpdate, Description = "A local music-reactive halo or orb ring around the soundboard's music icon")]
    public static void Tick()
    {
        PortTools.Register(Clear);
        bool visible = MenuMain.MenuMain != null && MenuMain.MenuMain.activeInHierarchy && MenuMain.SoundboardControlOpen && SoundManager.IsPlaying;
        if (!visible)
        {
            if (halo != null) halo.enabled = false;
            foreach (Transform? orb in Orbs) if (orb != null) orb.gameObject.SetActive(false);
            envelope = 0f;
            return;
        }
        Transform menu = MenuMain.MenuMain!.transform;
        envelope = Mathf.Lerp(envelope, Mathf.Clamp01(CreativeTools.AudioLevel * 8f), 1f - Mathf.Exp(-12f * Mathf.Min(Time.unscaledDeltaTime, 0.1f)));
        float radius = (0.12f + envelope * 0.05f * CreativeSettings.EffectIntensity) * CreativeSettings.EffectSize;
        Color color = CreativeSettings.Color(CreativeTools.Theme);
        if (CreativeSettings.HaloStyle == 0)
        {
            if (halo == null) halo = CreativeTools.Line("Beat Halo", 40, 0.003f, true);
            halo.enabled = true; halo.startColor = halo.endColor = color;
            for (int i = 0; i < halo.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / halo.positionCount;
                halo.SetPosition(i, menu.TransformPoint(new Vector3(0.714f, 0.32f + Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius)));
            }
            halo.startWidth = halo.endWidth = Mathf.Max(0.0005f, menu.TransformVector(Vector3.up * 0.008f).magnitude);
            foreach (Transform? orb in Orbs) if (orb != null) orb.gameObject.SetActive(false);
            return;
        }
        if (halo != null) halo.enabled = false;
        int count = CreativeSettings.EffectQuality < 1f ? 6 : 12;
        for (int i = 0; i < Orbs.Length; i++)
        {
            if (i >= count) { if (Orbs[i] != null) Orbs[i]!.gameObject.SetActive(false); continue; }
            if (Orbs[i] == null) Orbs[i] = CreativeTools.Marker("Audio Orb", 0.01f);
            Transform orb = Orbs[i]!;
            float angle = i * Mathf.PI * 2f / count + Time.unscaledTime * 0.5f;
            orb.position = menu.TransformPoint(new Vector3(0.714f, 0.32f + Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius));
            orb.localScale = Vector3.one * menu.TransformVector(Vector3.up * (0.025f + envelope * 0.02f)).magnitude * CreativeSettings.EffectSize;
            CreativeTools.Paint(orb.GetComponent<Renderer>(), color);
            orb.gameObject.SetActive(true);
        }
    }
    [MenuDisable("Beat Halo")]
    public static void Clear()
    {
        if (halo != null) Object.Destroy(halo.gameObject);
        halo = null;
        for (int i = 0; i < Orbs.Length; i++) { if (Orbs[i] != null) Object.Destroy(Orbs[i]!.gameObject); Orbs[i] = null; }
        envelope = 0f;
    }
}
