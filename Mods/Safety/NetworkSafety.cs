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

    internal static string? TagReadiness()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null || NetworkSystem.Instance == null || GorillaTagger.Instance == null) return "Join a room before using tag tools.";
        VRRig local = VRRig.LocalRig;
        if (local == null || !local.enabled || local.Creator == null) return "Tag tools need your normal local rig.";
        if (GorillaGameManager.instance is not GorillaTagManager manager) return "This game mode does not support tag tools.";
        if (!manager.LocalIsTagged(local.Creator)) return "You must be tagged before tagging other players.";
        return null;
    }

    internal static string? TagRejection(VRRig? target)
    {
        string? readiness = TagReadiness();
        if (readiness != null) return readiness;
        VRRig local = VRRig.LocalRig;
        var manager = (GorillaTagManager)GorillaGameManager.instance;
        if (target == null || target == local || !target.gameObject.activeInHierarchy || target.isLocal || target.isOfflineVRRig || target.Creator == null) return "Select another active player.";
        if (!manager.LocalCanTag(local.Creator, target.Creator)) return "That player cannot currently be tagged.";
        Transform left = GorillaTagger.Instance.leftHandTransform, right = GorillaTagger.Instance.rightHandTransform;
        if (left == null || right == null || !Finite(left.position) || !Finite(right.position) || !Finite(target.transform.position)) return "Player tracking is not ready.";
        float reach = GorillaTagger.Instance.maxTagDistance;
        if (!Finite(reach) || reach <= 0f) return "Tag reach is not available.";
        reach = Mathf.Min(reach, 2f);
        float distance = Mathf.Min((left.position - target.transform.position).sqrMagnitude, (right.position - target.transform.position).sqrMagnitude);
        return distance <= reach * reach ? null : "The selected player is outside normal hand reach.";
    }

    internal static bool CanTagTarget(VRRig? target) => TagRejection(target) == null;
    private static float nextTagNotice;
    internal static bool TryTag(VRRig target, bool notify = false)
    {
        string? rejection = TagRejection(target);
        if (rejection != null)
        {
            if (notify && Time.unscaledTime >= nextTagNotice) { nextTagNotice = Time.unscaledTime + 3f; SafetyMods.Toast(rejection); }
            return false;
        }
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
