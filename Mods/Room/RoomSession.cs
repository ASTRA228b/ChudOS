// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections;
using ChudOS.Libraries.Notifications;
using ChudOS.Core.Managers;
using ChudOS.Stuff;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Room;

public sealed class RoomSession : MonoBehaviourPunCallbacks
{
    private static RoomSession? instance;
    private static string lastRoom = "";
    private Coroutine? queued;
    private static bool currentMap = true;

    private void Awake()
    {
        instance = this;
        RememberRoom();
    }

    public override void OnJoinedRoom() => RememberRoom();

    private static void RememberRoom()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
            lastRoom = PhotonNetwork.CurrentRoom.Name;
    }

    internal static void Reconnect()
    {
        RememberRoom();
        Queue(lastRoom, true);
    }

    internal static void JoinLastRoom() => Queue(lastRoom);
    internal static void JoinRoom(string code) => Queue(code);

    [MenuButton("Join Random: Current Map", ButtonType.Toggle, Category = "Settings", Order = 245, DefaultEnabled = true, runOption = RunOption.Once, Notify = false)]
    public static void CurrentMapOn() => currentMap = true;
    [MenuDisable("Join Random: Current Map")]
    public static void CurrentMapOff() => currentMap = false;

    internal static void JoinRandom()
    {
        var controller = PhotonNetworkController.Instance;
        if (instance == null || controller == null) return;
        GorillaNetworkJoinTrigger? trigger = null;
        if (!currentMap) trigger = GorillaComputer.instance?.GetJoinTriggerForZone("forest");
        else
        {
            var candidate = controller.currentJoinTrigger;
            if (candidate != null && candidate.isActiveAndEnabled && !candidate.isSubsOnly && ZoneManagement.instance != null && ZoneManagement.instance.IsZoneActive(candidate.zone)) trigger = candidate;
            if (trigger == null)
                foreach (var item in UnityEngine.Object.FindObjectsByType<GorillaNetworkJoinTrigger>(FindObjectsSortMode.None))
                    if (item != null && item.isActiveAndEnabled && ZoneManagement.instance != null && ZoneManagement.instance.IsZoneActive(item.zone) && !item.isSubsOnly) { trigger = item; break; }
        }
        if (trigger == null)
        {
            NotifyLib.SendNotificationALL("No public room join is available for this map.", 4000);
            return;
        }
        Cancel();
        instance.queued = instance.StartCoroutine(instance.Join("", trigger, currentMap));
    }

    private static void Queue(string code, bool reconnect = false)
    {
        if (instance == null || string.IsNullOrWhiteSpace(code))
        {
            NotifyLib.SendNotificationALL("No previous room is available this session.", 3500);
            return;
        }
        Cancel();
        if (!reconnect && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom?.Name == code)
        {
            NotifyLib.SendNotificationALL("You are already in that room.", 3000);
            return;
        }
        instance.queued = instance.StartCoroutine(instance.Join(code));
    }

    private IEnumerator Join(string code, GorillaNetworkJoinTrigger? publicTrigger = null, bool requireActiveZone = false)
    {
        yield return null;
        var network = NetworkSystem.Instance;
        if (network == null)
        {
            NotifyLib.SendNotificationALL("The room network is not ready.", 3500);
            queued = null;
            yield break;
        }

        float deadline = Time.unscaledTime + 20f;
        var leaving = network.ReturnToSinglePlayer();
        while (!leaving.IsCompleted && Time.unscaledTime < deadline) yield return null;
        if (!leaving.IsCompleted || leaving.IsFaulted || leaving.IsCanceled)
        {
            if (leaving.IsFaulted) Debug.LogException(leaving.Exception!);
            NotifyLib.SendNotificationALL("Reconnect could not leave the current session.", 4000);
            queued = null;
            yield break;
        }

        while (network.netState != NetSystemState.Idle && Time.unscaledTime < deadline) yield return null;
        if (network.netState != NetSystemState.Idle || PhotonNetworkController.Instance == null)
        {
            NotifyLib.SendNotificationALL("Reconnect timed out waiting for the network.", 4000);
            queued = null;
            yield break;
        }

        float readyAt = Time.unscaledTime + 0.5f;
        while (Time.unscaledTime < readyAt) yield return null;
        queued = null;
        if (PhotonNetworkController.Instance == null) { NotifyLib.SendNotificationALL("The room network is no longer available.", 3500); yield break; }
        if (requireActiveZone && (publicTrigger == null || ZoneManagement.instance == null || !ZoneManagement.instance.IsZoneActive(publicTrigger.zone)))
        {
            NotifyLib.SendNotificationALL("The selected map is no longer available.", 3500);
            yield break;
        }
        if (code.Length > 0) PhotonNetworkController.Instance.AttemptToAutoJoinSpecificRoom(code, JoinType.Solo);
        else if (publicTrigger != null) PhotonNetworkController.Instance.AttemptToJoinPublicRoom(publicTrigger, JoinType.Solo);
        else NotifyLib.SendNotificationALL("The selected map is no longer available.", 3500);
    }

    internal static void Cancel()
    {
        if (instance == null || instance.queued == null) return;
        instance.StopCoroutine(instance.queued);
        instance.queued = null;
    }

    private void OnDestroy()
    {
        Cancel();
        if (instance == this) instance = null;
    }
}
