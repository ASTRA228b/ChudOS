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

using BepInEx;
using ChudOS.Libraries;
using GorillaTagScripts;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Safety;

internal static class SafetyRuntime
{
    private const string ModeSelectorPath = "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/ModeSelector_Group";
    private static GameObject? Selector;
    private static bool SavedSelectorVisible;
    private static bool SelectorSaved;
    private static float NextSelectorLookup;
    private static float LastNicknameAlert;
    private static string LastWarnedName = "";
    private static bool WasInRoom;
    private static float NextGC;
    private static float NextAlertScan;
    private static float LastReportNotify;
    private static float NextZoneScan;
    private static GameObject? ReportZone;
    private static float ReportDistance = 0.35f;
    private static float NextRPCFlush;
    private static float NextLurkerScan;
    private static bool PowerFrozen;
    private static bool RigEnabledBeforeFreeze;
    private static Vector3 FrozenPosition;
    private static bool OverlaySaved;
    private static bool PreviousOverlay;

    public static void FakePowerOff()
    {
        if (GorillaTagger.Instance == null || GorillaTagger.Instance.rigidbody == null || VRRig.LocalRig == null) return;
        Rigidbody rb = GorillaTagger.Instance.rigidbody;
        bool pressed = ControllerInputPoller.instance != null && InputLib.LeftJoystickClick;
        if (!pressed) { RestoreFakePowerOff(); return; }
        if (!PowerFrozen)
        {
            FrozenPosition = rb.position;
            RigEnabledBeforeFreeze = VRRig.LocalRig.enabled;
            PowerFrozen = true;
        }
        rb.position = FrozenPosition;
        rb.linearVelocity = Vector3.zero;
        VRRig.LocalRig.enabled = false;
    }

    public static void RestoreFakePowerOff()
    {
        if (!PowerFrozen) return;
        if (VRRig.LocalRig != null) VRRig.LocalRig.enabled = RigEnabledBeforeFreeze;
        PowerFrozen = false;
    }

    public static void FakeReportOverlay()
    {
        GorillaLocomotion.GTPlayer? player = GorillaLocomotion.GTPlayer.Instance;
        if (player == null || ControllerInputPoller.instance == null) return;
        if (!OverlaySaved) { PreviousOverlay = player.inOverlay; OverlaySaved = true; }
        player.inOverlay = PreviousOverlay || InputLib.LeftControllerXButton;
    }

    public static void RestoreReportOverlay()
    {
        if (OverlaySaved && GorillaLocomotion.GTPlayer.Instance != null) GorillaLocomotion.GTPlayer.Instance.inOverlay = PreviousOverlay;
        OverlaySaved = false;
    }

    private static readonly Dictionary<LurkerGhost, bool> Lurkers = new();
    private static VRRig? fingerRig;
    private static float[]? fingerPose;

    public static void NoFinger()
    {
        VRRig rig = VRRig.LocalRig;
        if (rig == null) return;
        if (fingerRig != rig)
        {
            RestoreFingers();
            fingerRig = rig;
            fingerPose = new[] { rig.leftHand.calcT, rig.rightHand.calcT, rig.leftIndex.calcT, rig.leftMiddle.calcT, rig.leftThumb.calcT, rig.rightIndex.calcT, rig.rightMiddle.calcT, rig.rightThumb.calcT };
            PortTools.Register(RestoreFingers);
        }
        rig.leftHand.calcT = rig.rightHand.calcT = 0f;
        rig.leftIndex.calcT = rig.leftMiddle.calcT = rig.leftThumb.calcT = 0f;
        rig.rightIndex.calcT = rig.rightMiddle.calcT = rig.rightThumb.calcT = 0f;
    }

    public static void RestoreFingers()
    {
        if (fingerRig != null && fingerPose != null)
        {
            fingerRig.leftHand.calcT = fingerPose[0]; fingerRig.rightHand.calcT = fingerPose[1];
            fingerRig.leftIndex.calcT = fingerPose[2]; fingerRig.leftMiddle.calcT = fingerPose[3]; fingerRig.leftThumb.calcT = fingerPose[4];
            fingerRig.rightIndex.calcT = fingerPose[5]; fingerRig.rightMiddle.calcT = fingerPose[6]; fingerRig.rightThumb.calcT = fingerPose[7];
        }
        fingerRig = null;
        fingerPose = null;
    }

    public static void AntiLurker()
    {
        if (Time.unscaledTime < NextLurkerScan) return;
        NextLurkerScan = Time.unscaledTime + 1f;
        PortTools.Register(RestoreLurkers);
        foreach (LurkerGhost lurker in UnityEngine.Object.FindObjectsByType<LurkerGhost>(FindObjectsSortMode.None))
        {
            if (lurker == null) continue;
            if (!Lurkers.ContainsKey(lurker)) Lurkers.Add(lurker, lurker.gameObject.activeSelf);
            lurker.gameObject.SetActive(false);
        }
    }

    public static void RestoreLurkers()
    {
        foreach (var pair in Lurkers) if (pair.Key != null) pair.Key.gameObject.SetActive(pair.Value);
        Lurkers.Clear();
        NextLurkerScan = 0f;
    }
    public static void ClearMyRPCs()
    {
        if (Time.unscaledTime < NextRPCFlush) { SafetyMods.Toast("RPC cleanup cooldown active."); return; }
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null) { SafetyMods.Toast("No room to clear RPCs from."); return; }
        if (!NetworkSafety.TryRPC()) { SafetyMods.Toast("RPC cleanup is temporarily limited."); return; }
        NextRPCFlush = Time.unscaledTime + 5f;
        PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
        SafetyMods.Toast("Requested cleanup of local player's buffered RPCs.");
    }

    public static void FakeValveTracking()
    {
        if (ControllerInputPoller.instance == null || !InputLib.RightJoystickClick) return;
        if (VRRig.LocalRig == null || VRRig.LocalRig.head == null || VRRig.LocalRig.head.rigTarget == null) return;
        VRRig.LocalRig.head.rigTarget.transform.rotation = Quaternion.identity;
    }

    public static void HideModeSelector()
    {
        if (Selector == null && Time.unscaledTime >= NextSelectorLookup)
        {
            NextSelectorLookup = Time.unscaledTime + 1f;
            Selector = GameObject.Find(ModeSelectorPath);
        }
        if (Selector == null) return;
        if (!SelectorSaved) { SavedSelectorVisible = Selector.activeSelf; SelectorSaved = true; }
        if (Selector.activeSelf) Selector.SetActive(false);
    }

    public static void RestoreModeSelector()
    {
        if (Selector != null && SelectorSaved) Selector.SetActive(SavedSelectorVisible);
        Selector = null;
        SelectorSaved = false;
    }

    public static void CheckNickname()
    {
        string nick = PhotonNetwork.NickName ?? "";
        if (string.IsNullOrWhiteSpace(nick)) SafetyMods.Toast("Nickname is empty.");
        else if (nick.Length > 12) SafetyMods.Toast($"Nickname is {nick.Length} characters (over 12).");
        else if (!ValidName(nick)) SafetyMods.Toast("Use only A-Z and 0-9 in your nickname. This is not a ban-list check.");
        else SafetyMods.Toast($"Nickname length OK: {nick.Length}/12. Not a ban-list validation.");
    }

    public static void ChangeNickname(bool fromFile)
    {
        if (PhotonNetwork.InRoom) { SafetyMods.Toast("Leave the room before changing nickname."); return; }
        string nickname = fromFile ? PickCustomName() : $"monke{UnityEngine.Random.Range(1000, 10000)}";
        if (string.IsNullOrWhiteSpace(nickname)) { SafetyMods.Toast("No valid custom nicknames available."); return; }
        if (!ValidName(nickname)) { SafetyMods.Toast("Nickname must contain 1-12 letters or digits."); return; }
        PhotonNetwork.NickName = nickname;
        SafetyMods.Toast($"Nickname changed to {nickname}.");
    }

    private static string PickCustomName()
    {
        string path = Path.Combine(Paths.GameRootPath, "ChudOS", "identities.txt");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            if (!File.Exists(path)) File.WriteAllText(path, "monke\nbanana\nchud\n");
            string[] names = File.ReadAllLines(path).Select(s => s.Trim()).Where(ValidName).ToArray();
            return names.Length == 0 ? "" : names[UnityEngine.Random.Range(0, names.Length)];
        }
        catch (Exception ex) { Debug.LogWarning($"[ChudOS] Could not read identities.txt: {ex.Message}"); return ""; }
    }

    public static void BeginWatchRoomLeave() => WasInRoom = PhotonNetwork.InRoom;

    private static bool ValidName(string name) => name.Length > 0 && name.Length <= 12 && name.All(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9');

    public static void WatchRoomLeave()
    {
        bool isInRoom = PhotonNetwork.InRoom;
        if (WasInRoom && !isInRoom) ChangeNickname(false);
        WasInRoom = isInRoom;
    }

    public static void NicknameLengthAlert()
    {
        if (Time.unscaledTime - LastNicknameAlert < 3f) return;
        LastNicknameAlert = Time.unscaledTime;
        string nickname = PhotonNetwork.NickName ?? "";
        if (nickname.Length > 12 && nickname != LastWarnedName)
        {
            LastWarnedName = nickname;
            SafetyMods.Toast("Nickname is longer than 12 characters.");
        }
        else if (nickname.Length <= 12) LastWarnedName = "";
    }

    public static void ResetGCTime() => NextGC = Time.unscaledTime + 60f;

    public static void AutoGC()
    {
        if (Time.unscaledTime < NextGC) return;
        NextGC = Time.unscaledTime + 60f;
        GC.Collect();
    }

    public static void SetReportRange(int index)
    {
        float[] distances = { 0.35f, 0.7f, 1.5f };
        ReportDistance = distances[Mathf.Clamp(index, 0, distances.Length - 1)];
        if (ReportZone != null) ReportZone.transform.localScale = Vector3.one * (ReportDistance * 2f);
    }

    private static Transform? MyReportButton()
    {
        if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom) return null;
        if (GorillaScoreboardTotalUpdater.allScoreboardLines == null) return null;
        foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
        {
            if (line == null || line.linePlayer != NetworkSystem.Instance.LocalPlayer || line.reportButton == null) continue;
            return line.reportButton.transform;
        }
        return null;
    }

    public static void ReportAlert()
    {
        if (Time.unscaledTime < NextAlertScan) return;
        NextAlertScan = Time.unscaledTime + 0.25f;
        if (Time.unscaledTime - LastReportNotify < 4f) return;
        Transform? report = MyReportButton();
        if (report == null) return;
        Vector3 point = report.position;
        foreach (VRRig rig in UnityEngine.Object.FindObjectsByType<VRRig>(FindObjectsSortMode.None))
        {
            if (rig == null || rig == VRRig.LocalRig || rig.isOfflineVRRig || rig.leftHandTransform == null || rig.rightHandTransform == null) continue;
            if (Vector3.Distance(rig.leftHandTransform.position, point) > ReportDistance && Vector3.Distance(rig.rightHandTransform.position, point) > ReportDistance) continue;
            LastReportNotify = Time.unscaledTime;
            SafetyMods.Toast("A hand is near your report button (proximity only; not a confirmed report).");
            break;
        }
    }

    public static void DrawReportZone() // Draw cuz why not
    {
        if (Time.unscaledTime < NextZoneScan) return;
        NextZoneScan = Time.unscaledTime + 0.25f;
        Transform? target = MyReportButton();
        if (target == null) { ClearReportZone(); return; }
        if (ReportZone == null)
        {
            ReportZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ReportZone.name = "ChudOS.LocalReportZone";
            ReportZone.layer = 2;
            Collider collider = ReportZone.GetComponent<Collider>();
            if (collider != null) { collider.enabled = false; UnityEngine.Object.Destroy(collider); }
            Renderer render = ReportZone.GetComponent<Renderer>();
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                Material material = new(shader) { color = new Color(1f, 0.15f, 0.25f, 0.16f) };
                render.material = material;
            }
        }
        ReportZone.transform.position = target.position;
        ReportZone.transform.localScale = Vector3.one * (ReportDistance * 2f);
    }

    public static void ClearReportZone()
    {
        if (ReportZone != null)
        {
            Renderer renderer = ReportZone.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null) UnityEngine.Object.Destroy(renderer.material);
            UnityEngine.Object.Destroy(ReportZone);
        }
        ReportZone = null;
    }


    private static float NextAntiReportScan;
    private static float NextAntiReportAction;
    private static bool AntiReportContact;

    public static void ResetAntiReport()
    {
        NextAntiReportScan = 0f;
        NextAntiReportAction = 0f;
        AntiReportContact = false;
    }

    public static void AntiReportDisconnect()
    {
        if (Time.unscaledTime < NextAntiReportScan) return;
        NextAntiReportScan = Time.unscaledTime + 0.05f;

        if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom)
        {
            AntiReportContact = false;
            return;
        }

        VRRig? detected = FindNearbyReportRig();

        if (detected == null)
        {
            AntiReportContact = false;
            return;
        }

        if (AntiReportContact || Time.unscaledTime < NextAntiReportAction) return;

        AntiReportContact = true;
        NextAntiReportAction = Time.unscaledTime + 2f;

        string playerName = GetReportPlayerName(detected);

        SafetyMods.Toast($"[ANTI-REPORT] {playerName} is near your report button. Disconnecting.");

        NetworkSystem.Instance.ReturnToSinglePlayer();
    }

    private static VRRig? FindNearbyReportRig()
    {
        if (NetworkSystem.Instance == null || NetworkSystem.Instance.LocalPlayer == null)
            return null;

        if (GorillaScoreboardTotalUpdater.allScoreboardLines == null || VRRigCache.ActiveRigs == null)
            return null;

        float range = ReportDistance * ReportDistance;

        foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
        {
            if (line == null || line.linePlayer != NetworkSystem.Instance.LocalPlayer || line.reportButton == null)
                continue;

            Vector3 position = line.reportButton.transform.position;

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig == VRRig.LocalRig || rig.isLocal || rig.isOfflineVRRig)
                    continue;

                if (rig.leftHandTransform == null || rig.rightHandTransform == null)
                    continue;

                float left = (rig.leftHandTransform.position - position).sqrMagnitude;
                float right = (rig.rightHandTransform.position - position).sqrMagnitude;

                if (left <= range || right <= range)
                    return rig;
            }
        }

        return null;
    }

    private static string GetReportPlayerName(VRRig rig)
    {
        try
        {
            PhotonView? view = Traverse.Create(rig).Field("photonView").GetValue() as PhotonView;
            return view?.Owner?.NickName ?? "Another player";
        }
        catch
        {
            return "Another player";
        }
    }

}
