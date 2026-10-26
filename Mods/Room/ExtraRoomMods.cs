// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Room;

public static class ExtraRoomMods
{
    private const string MapPath = "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab";
    private static GameObject? mapTriggers;
    private static bool originalMapState, leaveHeld;
    private static int leaveInput;
    internal static bool QuitBoxDisabled;

    [MenuButton("Report All", ButtonType.Press, Category = "Room", Order = 8, Description = "Submit a Cheating report for every other player in the room")]
    public static void ReportAll()
    {
        if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom) return;
        foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
            GorillaPlayerScoreboardLine.ReportPlayer(player.UserId, GorillaPlayerLineButton.ButtonType.Cheating, player.NickName);
    }

    [MenuButton("Key-Leave", ButtonType.Toggle, Category = "Room", Order = 9, Description = "Press your selected input to leave the room")]
    public static void KeyLeave()
    {
        bool held = ModInput.Held(leaveInput);
        if (held && !leaveHeld)
        {
            RoomSession.Cancel();
            if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
                _ = NetworkSystem.Instance.ReturnToSinglePlayer();
        }
        leaveHeld = held;
    }

    [MenuEnable("Key-Leave")]
    public static void StartKeyLeave()
    {
        leaveHeld = ModInput.Held(leaveInput);
        PortTools.Register(Clear);
    }
    [MenuDisable("Key-Leave")]
    public static void StopKeyLeave() => leaveHeld = false;

    [MenuButton("Key-Leave Input", ButtonType.Option, Category = "Room", Order = 10, Options = new[] { "Right Trigger", "Left Trigger", "Right Grip", "Left Grip", "A", "B", "X", "Y", "Right Joystick Click", "Left Joystick Click" }, Notify = false)]
    public static void SetLeaveInput()
    {
        leaveInput = ButtonManager.GetButton("Key-Leave Input")?.OptionIndex ?? 0;
        leaveHeld = ModInput.Held(leaveInput);
    }

    [MenuButton("Disable Quit Box", ButtonType.Toggle, Category = "Room", Order = 11, runOption = RunOption.Once)]
    public static void DisableQuitBox()
    {
        QuitBoxDisabled = true;
        PortTools.Register(Clear);
    }
    [MenuDisable("Disable Quit Box")]
    public static void RestoreQuitBox() => QuitBoxDisabled = false;

    [MenuButton("Disable Map Triggers", ButtonType.Toggle, Category = "Room", Order = 12, runOption = RunOption.Once)]
    public static void DisableMapTriggers()
    {
        if (mapTriggers != null) return;
        GameObject obj = GameObject.Find(MapPath);
        if (obj == null) throw new InvalidOperationException("Map transition triggers are not loaded.");
        mapTriggers = obj;
        originalMapState = obj.activeSelf;
        PortTools.Register(Clear);
        obj.SetActive(false);
    }
    [MenuDisable("Disable Map Triggers")]
    public static void RestoreMapTriggers()
    {
        if (mapTriggers != null) mapTriggers.SetActive(originalMapState);
        mapTriggers = null;
    }

    [MenuButton("Reconnect", ButtonType.Press, Category = "Room", Order = 13)]
    public static void Reconnect() => RoomSession.Reconnect();
    [MenuButton("Cancel Reconnect", ButtonType.Press, Category = "Room", Order = 14)]
    public static void CancelReconnect() => RoomSession.Cancel();
    [MenuButton("Join Last Room", ButtonType.Press, Category = "Room", Order = 15)]
    public static void JoinLastRoom() => RoomSession.JoinLastRoom();

    internal static void Clear()
    {
        leaveHeld = false;
        RestoreQuitBox();
        RestoreMapTriggers();
    }
}

[HarmonyPatch(typeof(GorillaQuitBox), nameof(GorillaQuitBox.OnBoxTriggered))]
internal static class RoomQuitBoxPatch
{
    private static bool Prefix() => !ExtraRoomMods.QuitBoxDisabled;
}
