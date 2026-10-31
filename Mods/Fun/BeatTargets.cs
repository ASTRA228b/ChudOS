// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Fun;

public static class BeatTargets
{
    private static readonly CreativeTools.Session Session = new();
    private static Transform? target;
    private static float expires;
    private static int score;
    private static bool active;
    [MenuButton("Beat Targets", ButtonType.Toggle, Category = "Fun", Order = 402, runOption = RunOption.LateUpdate, Description = "Play a soundboard song and touch the local targets that appear with its louder beats")]
    public static void Tick()
    {
        if (Session.Changed()) Clear();
        if (!CreativeTools.Ready) { Clear(); return; }
        PortTools.Register(Clear);
        if (target == null) target = CreativeTools.Marker("Beat Target", 0.13f * CreativeTools.Scale * CreativeSettings.EffectSize);
        if (!SoundManager.IsPlaying) { target.gameObject.SetActive(false); active = false; return; }
        if (CreativeTools.AudioBeat)
        {
            Transform head = CreativeTools.Head;
            Vector3 position = head.position + head.forward * (0.6f * CreativeTools.Scale);
            position += head.right * UnityEngine.Random.Range(-0.35f, 0.35f) * CreativeTools.Scale;
            position += head.up * UnityEngine.Random.Range(-0.3f, 0.1f) * CreativeTools.Scale;
            target.position = position;
            target.localScale = Vector3.one * (0.13f * CreativeTools.Scale * CreativeSettings.EffectSize);
            CreativeTools.Paint(target.GetComponent<Renderer>(), CreativeSettings.Color(CreativeTools.Theme));
            target.gameObject.SetActive(true); expires = Time.unscaledTime + 0.7f; active = true;
        }
        if (!active) { target.gameObject.SetActive(false); return; }
        float radius = 0.12f * CreativeTools.Scale * CreativeSettings.EffectSize;
        bool hit = (CreativeTools.Left.position - target.position).sqrMagnitude < radius * radius || (CreativeTools.Right.position - target.position).sqrMagnitude < radius * radius;
        if (hit) { score++; if (score % 10 == 0) CreativeTools.Notice($"Beat Targets: {score}"); }
        if (!hit && Time.unscaledTime < expires) return;
        target.gameObject.SetActive(false); active = false;
    }
    [MenuDisable("Beat Targets")]
    public static void Clear()
    {
        if (target != null) Object.Destroy(target.gameObject);
        target = null; active = false; score = 0;
    }
}
