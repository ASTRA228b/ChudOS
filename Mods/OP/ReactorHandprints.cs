// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.OP;

public static class ReactorHandprints
{
    private static float nextPrint, nextNotice;
    internal static bool Stamp(Vector3 point, Vector3 normal)
    {
        var reactor = GhostReactor.instance;
        if (reactor == null || !reactor.isActiveAndEnabled || !ZoneManagement.IsInZone(reactor.zone)) return false;
        if (Time.unscaledTime < nextPrint) return true;
        nextPrint = Time.unscaledTime + 0.25f;
        reactor.AddHandprint(point, Quaternion.FromToRotation(Vector3.up, normal.sqrMagnitude > 0.001f ? normal : Vector3.up));
        return true;
    }
    [MenuButton("GR Trail", ButtonType.Toggle, Category = "OP", Order = 21, Description = "Local Ghost Reactor handprint trail. Requires the active Ghost Reactor scene.")]
    public static void Trail()
    {
        if (!CreativeTools.Ready) return;
        if (!Stamp(CreativeTools.Position, Vector3.up)) ExplainUnavailable();
    }
    internal static void ExplainUnavailable()
    {
        if (Time.unscaledTime < nextNotice) return;
        nextNotice = Time.unscaledTime + 5f;
        CreativeTools.Notice("Local handprints require an active Ghost Reactor scene.");
    }
}
