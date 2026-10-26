// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections;
using ChudOS.Libraries.Notifications;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Room;

public sealed class RoomSession : MonoBehaviourPunCallbacks
{
    private static RoomSession? instance;
    private static string lastRoom = "";
    private Coroutine? queued;

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

    private IEnumerator Join(string code)
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
        PhotonNetworkController.Instance.AttemptToAutoJoinSpecificRoom(code, JoinType.Solo);
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
