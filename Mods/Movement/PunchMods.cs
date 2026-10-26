// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class PunchMods
{
    private static readonly VRRig?[] players = new VRRig?[20];
    private static readonly Vector3[] lastRight = new Vector3[20], lastLeft = new Vector3[20];

    [MenuButton("Punch", ButtonType.Toggle, Category = "Movement", Order = 163, Description = "Other players' hand movements knock you back; configure Punch Power in Settings")]
    public static void Punch()
    {
        if (!PortTools.Ready || VRRig.LocalRig == null) { Stop(); return; }
        PortTools.Register(Stop);
        int index = 0;
        Vector3 localPosition = VRRig.LocalRig.bodyTransform.position;
        foreach (VRRig rig in VRRigCache.ActiveRigs)
        {
            if (rig == null || rig == VRRig.LocalRig) continue;
            if (index >= players.Length) break;
            Vector3 left = rig.leftHandTransform.position, right = rig.rightHandTransform.position;
            if (players[index] == rig)
            {
                Apply(left, lastLeft[index], localPosition);
                Apply(right, lastRight[index], localPosition);
            }
            players[index] = rig;
            lastLeft[index] = left;
            lastRight[index] = right;
            index++;
        }
        for (int i = index; i < players.Length; i++) players[i] = null;
    }

    private static void Apply(Vector3 position, Vector3 previous, Vector3 localPosition)
    {
        if (Vector3.Distance(position, localPosition) >= 0.25f) return;
        Vector3 velocity = position - previous;
        if (velocity.sqrMagnitude > 0.0001f) GorillaTagger.Instance.rigidbody.linearVelocity += velocity.normalized * ExtraMovementSettings.PunchPower;
    }

    [MenuDisable("Punch")]
    public static void Stop()
    {
        Array.Clear(players, 0, players.Length);
        Array.Clear(lastLeft, 0, lastLeft.Length);
        Array.Clear(lastRight, 0, lastRight.Length);
    }
}
