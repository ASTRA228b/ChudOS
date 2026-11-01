// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Libraries;
using ChudOS.Mods.Safety;
using ChudOS.Stuff;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using UnityEngine;

namespace ChudOS.Mods.OP;

internal sealed class RoomAuthorityControls : ExpansionWindowFeature
{
    private PhotonPlayer[] players = Array.Empty<PhotonPlayer>();
    private float nextPlayers, nextRequest;
    private Vector2 scroll;

    [MenuButton("Guardian Controls", ButtonType.Toggle, Category = "OP", Order = 0, runOption = RunOption.Once, Description = "Guardian assignment controls for a supported room where you are master.")]
    public static void EnableGuardian() => Enable<RoomAuthorityControls>("Guardian Controls");
    [MenuDisable("Guardian Controls")]
    public static void DisableGuardian() => Disable("Guardian Controls");
    [MenuButton("Team Controls", ButtonType.Toggle, Category = "OP", Order = 1, runOption = RunOption.Once, Description = "Monke Ball team assignments using the native host-authorized team RPC.")]
    public static void EnableTeams() => Enable<RoomAuthorityControls>("Team Controls");
    [MenuDisable("Team Controls")]
    public static void DisableTeams() => Disable("Team Controls");

    protected override void Setup() => OpenWindow(870100 + (Mode == "Team Controls" ? 1 : 0), Mode);
    protected override void Tick()
    {
        if (Time.unscaledTime < nextPlayers) return;
        nextPlayers = Time.unscaledTime + 1f;
        players = PhotonNetwork.InRoom ? PhotonNetwork.PlayerList : Array.Empty<PhotonPlayer>();
    }
    private bool CanRequest(PhotonPlayer player)
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) { CreativeTools.Notice("This control requires a joined room where you are master."); return false; }
        if (!PhotonNetwork.PlayerList.Contains(player)) { CreativeTools.Notice("That player is no longer in the room."); return false; }
        if (Time.unscaledTime < nextRequest || !NetworkSafety.TryRPC()) return false;
        nextRequest = Time.unscaledTime + 0.75f;
        return true;
    }
    private void SetGuardian(PhotonPlayer player, bool remove)
    {
        if (GorillaGameManager.instance is not GorillaGuardianManager) { CreativeTools.Notice("Guardian mode is not active."); return; }
        if (!CanRequest(player)) return;
        NetPlayer target = NetPlayer.Get(player);
        if (target == null) return;
        foreach (var zone in GorillaGuardianZoneManager.zoneManagers)
        {
            if (zone == null || !zone.enabled || !zone.IsZoneValid()) continue;
            if (remove) { if (zone.CurrentGuardian == target) zone.SetGuardian(null!); }
            else if (zone.CurrentGuardian == null) { zone.SetGuardian(target); return; }
        }
    }
    private void SetTeam(PhotonPlayer player, int team)
    {
        var game = MonkeBallGame.Instance;
        if (game == null || !game.isActiveAndEnabled || game.photonView == null || !ZoneManagement.IsInZone(GTZone.arena)) { CreativeTools.Notice("The Monke Ball arena is not active."); return; }
        if (team < -1 || team >= game.team.Count || !CanRequest(player)) return;
        game.photonView.RPC("SetTeamRPC", RpcTarget.All, team, player);
    }
    protected override void Window(int id)
    {
        MenuHelper.Label(PhotonNetwork.IsMasterClient ? "Room authority: master" : "Room authority: unavailable");
        scroll = GUILayout.BeginScrollView(scroll);
        foreach (var player in players)
        {
            if (player == null) continue;
            MenuHelper.Label(player.NickName);
            GUILayout.BeginHorizontal();
            if (Mode == "Guardian Controls") { if (MenuHelper.Button("Assign")) SetGuardian(player, false); if (MenuHelper.Button("Remove")) SetGuardian(player, true); }
            else { if (MenuHelper.Button("Blue")) SetTeam(player, 0); if (MenuHelper.Button("Red")) SetTeam(player, 1); if (MenuHelper.Button("None")) SetTeam(player, -1); }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView(); GUI.DragWindow();
    }
}
