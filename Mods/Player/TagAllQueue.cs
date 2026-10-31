// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Mods.Safety;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Player;

internal static class TagAllQueue
{
    private static readonly Queue<VRRig> Targets = new();
    private static readonly HashSet<VRRig> Seen = new();
    private static object? room;
    private static float deadline, nextAttempt;
    private static int submitted;
    private static string? emptyReason;

    internal static void Start()
    {
        Clear();
        room = PhotonNetwork.CurrentRoom;
        deadline = Time.unscaledTime + 10f;
        emptyReason = NetworkSafety.TagReadiness();
        if (emptyReason != null) return;
        if (VRRigCache.ActiveRigs == null) return;
        foreach (var rig in VRRigCache.ActiveRigs)
            if (NetworkSafety.CanTagTarget(rig) && Seen.Add(rig)) Targets.Enqueue(rig);
        Seen.Clear();
    }
    internal static void Tick()
    {
        if (!ReferenceEquals(room, PhotonNetwork.CurrentRoom)) { Stop(false); return; }
        if (Time.unscaledTime >= deadline) { Stop(true); return; }
        if (Time.unscaledTime < nextAttempt) return;
        nextAttempt = Time.unscaledTime + 0.05f;
        int count = Targets.Count;
        while (count-- > 0)
        {
            var target = Targets.Dequeue();
            if (!NetworkSafety.CanTagTarget(target)) continue;
            if (NetworkSafety.TryTag(target)) submitted++;
            else Targets.Enqueue(target);
            break;
        }
        if (Targets.Count == 0) Stop(true);
    }
    private static void Stop(bool notify)
    {
        int completed = submitted, remaining = Targets.Count;
        string? reason = emptyReason ?? NetworkSafety.TagReadiness();
        ButtonManager.SetEnabled("Tag All", false);
        if (notify) SafetyMods.Toast(completed == 0 ? reason ?? "No nearby eligible tag requests were submitted." : $"Submitted {completed} nearby tag request(s)." + (remaining > 0 ? " Remaining requests timed out." : ""));
    }
    internal static void Clear() { Targets.Clear(); Seen.Clear(); room = null; deadline = nextAttempt = 0f; submitted = 0; emptyReason = null; }
}
