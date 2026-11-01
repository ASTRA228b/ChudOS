// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Text;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Player;

internal sealed class PalmPanels : BetaFeature
{
    private readonly StringBuilder text = new(512);
    private TextMesh? label;
    private Renderer? renderer;
    private float nextText;

    [MenuButton("Palm Player List", ButtonType.Toggle, Category = "Player", Order = 800, runOption = RunOption.Once, Description = "A local room roster beside your right hand.")]
    public static void EnablePlayers() => Enable<PalmPanels>("Palm Player List");
    [MenuDisable("Palm Player List")]
    public static void DisablePlayers() => Disable("Palm Player List");
    [MenuButton("Palm Stats", ButtonType.Toggle, Category = "Player", Order = 801, runOption = RunOption.Once, Description = "A local speed, FPS, ping and room panel beside your left hand.")]
    public static void EnableStats() => Enable<PalmPanels>("Palm Stats");
    [MenuDisable("Palm Stats")]
    public static void DisableStats() => Disable("Palm Stats");
    protected override void Setup() { label = Label("", CreativeTools.Position, 0.006f); label.richText = false; renderer = label.GetComponent<Renderer>(); }
    protected override void Tick()
    {
        if (label == null || renderer == null) return;
        Transform hand = Mode == "Palm Player List" ? CreativeTools.Right : CreativeTools.Left;
        label.transform.position = hand.position + hand.up * 0.14f + hand.forward * 0.05f;
        label.transform.rotation = Quaternion.LookRotation(label.transform.position - CreativeTools.Head.position);
        bool facing = Vector3.Dot(hand.up, (CreativeTools.Head.position - hand.position).normalized) > 0.1f;
        renderer.enabled = facing;
        if (!facing || Time.unscaledTime < nextText) return;
        nextText = Time.unscaledTime + 0.33f; text.Clear();
        if (Mode == "Palm Player List")
        {
            text.AppendLine("ROOM PLAYERS");
            if (!PhotonNetwork.InRoom) text.Append("Offline");
            else foreach (var player in PhotonNetwork.PlayerList) text.Append(player.IsMasterClient ? "* " : "").Append(player.NickName).AppendLine();
        }
        else
        {
            text.Append("Speed: ").Append(CreativeTools.Body.linearVelocity.magnitude.ToString("F1")).AppendLine(" m/s");
            text.Append("FPS: ").Append(Mathf.RoundToInt(1f / Mathf.Max(Time.smoothDeltaTime, 0.001f))).AppendLine();
            text.Append("Ping: ").Append(PhotonNetwork.GetPing()).AppendLine(" ms");
            text.Append("Room: ").Append(PhotonNetwork.CurrentRoom?.Name ?? "Offline").AppendLine();
            text.Append("Players: ").Append(PhotonNetwork.CurrentRoom?.PlayerCount ?? 0);
        }
        label.font = ExpansionFonts.GetFont(ChudOS.Mods.Settings.ExpansionPreferences.MenuFont);
        renderer.sharedMaterial = label.font.material; label.text = text.ToString();
    }
}
