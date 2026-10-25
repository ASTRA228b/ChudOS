// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Mods.Movement;

internal static class NoClipState
{
    [Flags]
    internal enum Owner { None = 0, NoClip = 1, Fly = 2 }
    private static Owner owners;
    private static GTPlayer? player;
    private static Collider? body, head;
    private static LayerMask layers;
    private static bool bodyTrigger, headTrigger;
    internal static LayerMask CollisionLayers => player != null ? layers : GTPlayer.Instance.locomotionEnabledLayers;

    internal static void Set(Owner owner, bool enabled)
    {
        if (!enabled)
        {
            owners &= ~owner;
            if (owners == Owner.None) Restore();
            return;
        }
        var current = GTPlayer.Instance;
        if (current == null) { Clear(); return; }
        if (player != current)
        {
            Restore();
            player = current;
            layers = current.locomotionEnabledLayers;
            body = current.bodyCollider;
            head = current.headCollider;
            bodyTrigger = body != null && body.isTrigger;
            headTrigger = head != null && head.isTrigger;
        }
        owners |= owner;
        player.locomotionEnabledLayers = 0;
        if (body != null) body.isTrigger = true;
        if (head != null) head.isTrigger = true;
    }
    private static void Restore()
    {
        if (player != null) player.locomotionEnabledLayers = layers;
        if (body != null) body.isTrigger = bodyTrigger;
        if (head != null) head.isTrigger = headTrigger;
        player = null;
        body = head = null;
    }
    internal static void Clear() { owners = Owner.None; Restore(); }
}
