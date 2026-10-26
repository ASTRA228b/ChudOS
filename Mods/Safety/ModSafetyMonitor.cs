// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Safety;

internal sealed class ModSafetyMonitor : MonoBehaviourPunCallbacks
{
    internal static bool ErrorProtection = true, LeaveProtection = false, MasterProtection = true;
    private static ModSafetyMonitor? instance;
    private readonly HashSet<ButtonManager.ButtonData> failed = new();
    private readonly CallWindow masterChanges = new(4, 3f);
    private Photon.Realtime.Room? previousRoom;
    private bool handling;

    internal static void Ensure()
    {
        if (instance != null) return;
        ChudOS.Core.Main main = UnityEngine.Object.FindFirstObjectByType<ChudOS.Core.Main>();
        if (main != null) main.gameObject.AddComponent<ModSafetyMonitor>();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
        previousRoom = PhotonNetwork.CurrentRoom;
        Application.logMessageReceived += OnLog;
    }

    private void Update()
    {
        if (!ReferenceEquals(previousRoom, PhotonNetwork.CurrentRoom))
        {
            bool left = previousRoom != null;
            previousRoom = PhotonNetwork.CurrentRoom;
            masterChanges.Reset();
            NetworkSafety.Reset();
            ModGuards.ClearTemporaryState();
            if (left && LeaveProtection) DisableGameplayMods();
        }
        if (failed.Count == 0) return;
        var pending = failed.ToArray();
        failed.Clear();
        handling = true;
        try
        {
            foreach (var button in pending)
            {
                if (!button.Enabled) continue;
                ButtonManager.SetEnabled(button, false);
                SafetyMods.Toast($"Stopped {button.Name} after a mod error.");
            }
        }
        finally { handling = false; }
    }

    private void OnLog(string message, string trace, LogType type)
    {
        if (!ErrorProtection || handling || (type != LogType.Exception && type != LogType.Error) || string.IsNullOrEmpty(trace)) return;
        foreach (var button in ButtonManager.Buttons)
        {
            if (!button.Enabled || button.Type != ButtonType.Toggle || button.Category == "Settings") continue;
            var method = button.Run.Method;
            string? owner = method.DeclaringType?.FullName;
            if (owner == null || !owner.StartsWith("ChudOS.Mods.", StringComparison.Ordinal)) continue;
            if (trace.Contains(owner + "." + method.Name + " (") || trace.Contains(owner + "." + method.Name + "(")) failed.Add(button);
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player player)
    {
        if (!MasterProtection || !PhotonNetwork.InRoom) { masterChanges.Reset(); return; }
        if (masterChanges.TryTake(Time.unscaledTime)) return;
        masterChanges.Reset();
        DisableGameplayMods();
        NetworkSystem.Instance?.ReturnToSinglePlayer();
        SafetyMods.Toast("Left the room after five rapid master changes.");
    }

    internal static void DisableGameplayMods()
    {
        foreach (var button in ButtonManager.Buttons)
            if (button.Type == ButtonType.Toggle && button.Enabled && button.Category != "Safety" && button.Category != "Settings") ButtonManager.SetEnabled(button, false);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
        failed.Clear();
        if (instance == this) instance = null;
    }
}
