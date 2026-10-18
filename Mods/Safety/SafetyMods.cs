// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Safety;

public static class SafetyMods
{
    [MenuButton("Disable Movement", ButtonType.Press, Category = "Safety", Order = 0, Notify = false)]
    public static void DisableMovement() => Toast($"Disabled {DisableCategory("Movement")} movement toggle(s).");

    [MenuButton("Disable All Mods", ButtonType.Press, Category = "Safety", Order = 1, Notify = false)]
    public static void DisableAllMods()
    {
        int count = 0;
        foreach (ButtonManager.ButtonData button in new System.Collections.Generic.List<ButtonManager.ButtonData>(ButtonManager.Buttons))
        {
            if (button.Type != ButtonType.Toggle || !button.Enabled || button.Category == "Settings") continue;
            ButtonManager.SetEnabled(button, false);
            count++;
        }
        Toast($"Disabled {count} mod toggle(s).");
    }

    [MenuButton("Stop All Sounds", ButtonType.Press, Category = "Safety", Order = 2, Notify = false)]
    public static void StopSounds() { SoundboardManager.Stop(); Toast("Soundboard stopped."); }

    [MenuButton("Close All Windows", ButtonType.Press, Category = "Safety", Order = 3, Notify = false)]
    public static void CloseAllWindows() { GUIManager.CloseAll(); Toast("Closed registered GUI windows."); }

    [MenuButton("Return Home", ButtonType.Press, Category = "Safety", Order = 4, Notify = false)]
    public static void ReturnHome() => CategoryManager.GoHome();

    [MenuButton("Leave Room", ButtonType.Press, Category = "Safety", Order = 5, Notify = false, Description = "Disconnect from the current room")]
    public static void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom) { Toast("Not currently in a room."); return; }
        PhotonNetwork.Disconnect();
        Toast("Disconnecting from room.");
    }


    [MenuButton("No Finger Gestures", ButtonType.Toggle, Category = "Safety", Order = 6, runOption = RunOption.Update, Description = "Suppress local animated finger grip/trigger floats")]
    public static void NoFinger()
    {
        ControllerInputPoller.instance.leftControllerGripFloat = 0f;
        ControllerInputPoller.instance.rightControllerGripFloat = 0f;
        ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
        ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
        ControllerInputPoller.instance.leftControllerPrimaryButton = false;
        ControllerInputPoller.instance.leftControllerSecondaryButton = false;
        ControllerInputPoller.instance.rightControllerPrimaryButton = false;
        ControllerInputPoller.instance.rightControllerSecondaryButton = false;
        ControllerInputPoller.instance.leftControllerPrimaryButtonTouch = false;
        ControllerInputPoller.instance.leftControllerSecondaryButtonTouch = false;
        ControllerInputPoller.instance.rightControllerPrimaryButtonTouch = false;
        ControllerInputPoller.instance.rightControllerSecondaryButtonTouch = false;
    }

    [MenuButton("Fake Valve Tracking", ButtonType.Toggle, Category = "Safety", Order = 7, runOption = RunOption.Update, Description = "Hold right joystick click to freeze the local rig head angle")]
    public static void FakeValveTracking() => SafetyRuntime.FakeValveTracking();

    [MenuButton("Hide Mode Selector", ButtonType.Toggle, Category = "Safety", Order = 8, runOption = RunOption.Update, Description = "Locally hide the mode selector; restore on disable")]
    public static void HideModeSelector() => SafetyRuntime.HideModeSelector();
    [MenuDisable("Hide Mode Selector")]
    public static void RestoreModeSelector() => SafetyRuntime.RestoreModeSelector();

    [MenuButton("Room Status", ButtonType.Press, Category = "Safety", Order = 9, Notify = false, Description = "Display your current multiplayer state")]
    public static void RoomStatus() => Toast(PhotonNetwork.InRoom ? $"In room: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0} player(s)." : "You are currently offline / not in a room.");

    [MenuButton("Check My Nickname", ButtonType.Press, Category = "Safety", Order = 10, Notify = false, Description = "Check the nickname length; not a game ban-list check")]
    public static void CheckNickname() => SafetyRuntime.CheckNickname();

    [MenuButton("Random Nickname", ButtonType.Press, Category = "Safety", Order = 11, Notify = false, Description = "Choose a random local nickname while outside a room")]
    public static void RandomNickname() => SafetyRuntime.ChangeNickname(false);

    [MenuButton("Custom Nickname", ButtonType.Press, Category = "Safety", Order = 12, Notify = false, Description = "Choose from ChudOS/identities.txt while outside a room")]
    public static void CustomNickname() => SafetyRuntime.ChangeNickname(true);

    [MenuButton("Name On Room Leave", ButtonType.Toggle, Category = "Safety", Order = 13, runOption = RunOption.Update, Description = "Change nickname once when leaving a room")]
    public static void NameOnRoomLeave() => SafetyRuntime.WatchRoomLeave();
    [MenuEnable("Name On Room Leave")]
    public static void NameOnRoomLeaveEnable() => SafetyRuntime.BeginWatchRoomLeave();

    [MenuButton("Nickname Length Alert", ButtonType.Toggle, Category = "Safety", Order = 14, runOption = RunOption.Update, Description = "Warn if nickname exceeds 12 characters")]
    public static void NicknameLengthAlert() => SafetyRuntime.NicknameLengthAlert();

    [MenuButton("Report Proximity Alert", ButtonType.Toggle, Category = "Safety", Order = 15, runOption = RunOption.Update, Description = "Notify if another rig's hand nears your scoreboard report button")]
    public static void ReportAlert() => SafetyRuntime.ReportAlert();

    [MenuButton("Report Alert Range", ButtonType.Option, Category = "Safety", Order = 16, Options = new[] { "0.35m", "0.7m", "1.5m" }, Description = "Detection distance for report proximity")]
    public static void ReportRange() => SafetyRuntime.SetReportRange(ButtonManager.GetButton("Report Alert Range")!.OptionIndex);

    [MenuButton("Visualize Report Zone", ButtonType.Toggle, Category = "Safety", Order = 17, runOption = RunOption.Update, Description = "Local transparent sphere at your scoreboard report button")]
    public static void ReportZone() => SafetyRuntime.DrawReportZone();
    [MenuDisable("Visualize Report Zone")]
    public static void DisableReportZone() => SafetyRuntime.ClearReportZone();

    [MenuButton("Auto GC (60s)", ButtonType.Toggle, Category = "Safety", Order = 18, runOption = RunOption.Update, Description = "Optional managed GC every 60 seconds; may cause frame drops")]
    public static void AutoGC() => SafetyRuntime.AutoGC();
    [MenuEnable("Auto GC (60s)")]
    public static void StartAutoGC() => SafetyRuntime.ResetGCTime();

    [MenuButton("Fake Power Off", ButtonType.Toggle, Category = "Safety", Order = 19, runOption = RunOption.Update, Description = "Hold left joystick click to temporarily freeze the local rig; release restores")]
    public static void FakePowerOff() => SafetyRuntime.FakePowerOff();
    [MenuDisable("Fake Power Off")]
    public static void DisableFakePowerOff() => SafetyRuntime.RestoreFakePowerOff();

    [MenuButton("Fake Report Overlay", ButtonType.Toggle, Category = "Safety", Order = 20, runOption = RunOption.Update, Description = "Show local in-overlay pose while holding left X; restores on disable")]
    public static void FakeReportOverlay() => SafetyRuntime.FakeReportOverlay();
    [MenuDisable("Fake Report Overlay")]
    public static void DisableFakeReportOverlay() => SafetyRuntime.RestoreReportOverlay();

    [MenuButton("Anti Lurker (Local)", ButtonType.Toggle, Category = "Safety", Order = 21, runOption = RunOption.Update, Description = "If a LurkerGhost possesses your local player, return it to patrol")]
    public static void AntiLurker() => SafetyRuntime.AntiLurker();

    [MenuButton("Clear My Buffered RPCs", ButtonType.Press, Category = "Safety", Order = 22, Notify = false, Description = "Clean this player's buffered RPCs, limited to once per 5 seconds")]
    public static void ClearRPCs() => SafetyRuntime.ClearMyRPCs();


    [MenuButton("Anti Report Disconnect", ButtonType.Toggle, Category = "Safety", Order = 23, runOption = RunOption.Update, Description = "Disconnect when another player approaches your scoreboard report button")]
    public static void AntiReportDisconnect() => SafetyRuntime.AntiReportDisconnect();

    [MenuEnable("Anti Report Disconnect")]
    public static void EnableAntiReportDisconnect() => SafetyRuntime.ResetAntiReport();

    [MenuDisable("Anti Report Disconnect")]
    public static void DisableAntiReportDisconnect() => SafetyRuntime.ResetAntiReport();


    private static int DisableCategory(string category)
    {
        int count = 0;
        foreach (ButtonManager.ButtonData button in ButtonManager.Buttons)
        {
            if (button.Type != ButtonType.Toggle || !button.Enabled || !string.Equals(button.Category, category, StringComparison.OrdinalIgnoreCase)) continue;
            ButtonManager.SetEnabled(button, false);
            count++;
        }
        return count;
    }

    internal static void Toast(string message) => NotifyLib.MessageToast(message, 3500);
}
