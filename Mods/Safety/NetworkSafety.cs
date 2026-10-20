// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using GorillaGameModes;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Safety;

internal static class NetworkSafety
{
    internal static bool RPCProtection = true;
    internal static bool ModdedRoomGuard = true;
    private static readonly CallWindow Calls = new(4, 1f);
    private static readonly Dictionary<VRRig, float> Targets = new();
    private static Photon.Realtime.Room? room;
    private static float nextNotice;

    internal static bool AllowTagMod(string name)
    {
        bool allowed = PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null;
        if (allowed && ModdedRoomGuard)
        {
            object? mode = PhotonNetwork.CurrentRoom!.CustomProperties["gameMode"];
            allowed = !PhotonNetwork.CurrentRoom.IsVisible && mode is string value && value.IndexOf("MODDED", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        if (allowed) return true;
        ButtonManager.SetEnabled(name, false);
        if (Time.unscaledTime >= nextNotice)
        {
            nextNotice = Time.unscaledTime + 3f;
            SafetyMods.Toast(!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null ? "Join a room before using tag tools." : "Tag tools require a private MODDED room while Modded Tag Guard is on.");
        }
        return false;
    }

    internal static bool TryRPC()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady || PhotonNetwork.LocalPlayer == null) return false;
        if (!ReferenceEquals(room, PhotonNetwork.CurrentRoom)) Reset();
        return !RPCProtection || Calls.TryTake(Time.unscaledTime);
    }

    internal static bool TryTag(VRRig target)
    {
        if (!PhotonNetwork.InRoom || NetworkSystem.Instance == null || GorillaTagger.Instance == null) return false;
        VRRig local = VRRig.LocalRig;
        if (local == null || !local.enabled || target == null || !target.gameObject.activeInHierarchy || target.isLocal || target.isOfflineVRRig || target.Creator == null) return false;
        if (GorillaGameManager.instance is not GorillaTagManager || !TagManager.IsTagged(local) || TagManager.IsTagged(target)) return false;
        Transform left = GorillaTagger.Instance.leftHandTransform, right = GorillaTagger.Instance.rightHandTransform;
        if (left == null || right == null || !Finite(left.position) || !Finite(right.position) || !Finite(target.transform.position)) return false;
        float reach = GorillaTagger.Instance.maxTagDistance;
        if (!Finite(reach) || reach <= 0f) return false;
        reach = Mathf.Min(reach, 2f);
        float distance = Mathf.Min((left.position - target.transform.position).sqrMagnitude, (right.position - target.transform.position).sqrMagnitude);
        if (distance > reach * reach) return false;
        if (!ReferenceEquals(room, PhotonNetwork.CurrentRoom)) Reset();
        float now = Time.unscaledTime;
        if (Targets.TryGetValue(target, out float next) && now < next) return false;
        if (!TryRPC()) return false;
        Targets[target] = now + 0.5f;
        GameMode.ReportTag(target.Creator);
        return true;
    }

    internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    internal static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);

    internal static void Reset()
    {
        room = PhotonNetwork.CurrentRoom;
        Targets.Clear();
        Calls.Reset();
    }
}
