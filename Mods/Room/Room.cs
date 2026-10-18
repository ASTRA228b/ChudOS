using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Room;

public class Room : MonoBehaviourPunCallbacks // room helpers
{
    // Only forest for now.
    private static readonly string Zoner = "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Forest, Tree Exit";
    private static string? pendingRegion;
    public static bool IsEnabled;

    public static void JoinRandom()
    {
        if (PhotonNetwork.InRoom)
        {
            Leave();
        }
        GameObject Val = GameObject.Find(Zoner);
        Val.GetComponent<GorillaTriggerBox>().OnBoxTriggered();
    }

    public static void Leave()
    {
        PhotonNetwork.Disconnect();
    }

    public static void JoinRoom(string Name)
    {
        if (PhotonNetwork.InRoom)
        {
            Leave();
        }
        PhotonNetworkController.Instance.AttemptToAutoJoinSpecificRoom(Name, JoinType.Solo);
    }

    public static void ConnectToRegion(string region)
    {
        if (string.IsNullOrEmpty(region))
            return;

        string? currentRegion = PhotonNetwork.CloudRegion?.Split('/')[0];

        if (currentRegion == region)
            return;

        pendingRegion = region;

        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnected)
        {
            pendingRegion = null;
            PhotonNetwork.ConnectToRegion(region);
        }
        else
            PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        if (string.IsNullOrEmpty(pendingRegion))
            return;

        string region = pendingRegion;
        pendingRegion = null;

        PhotonNetwork.ConnectToRegion(region);
    }
}

[HarmonyPatch(typeof(GorillaNetworkJoinTrigger), nameof(GorillaNetworkJoinTrigger.OnBoxTriggered))]
public static class NoNetworkTriggersPatch
{
    private static bool Prefix() => !Room.IsEnabled;
}