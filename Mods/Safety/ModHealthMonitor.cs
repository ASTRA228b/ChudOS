// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using UnityEngine;

namespace ChudOS.Mods.Safety;

internal sealed class ModHealthMonitor : BetaFeature
{
    private readonly Queue<string> errors = new();
    private readonly object sync = new();
    private string[] displayed = Array.Empty<string>();
    private float nextSample;
    private string status = "";
    private Rect window = new(230, 90, 470, 450);
    private Vector2 scroll;
    private GUIStyle? textStyle;
    protected override void Setup() => Application.logMessageReceivedThreaded += Log;
    private void Log(string message, string stack, LogType type)
    {
        if (type is not LogType.Error and not LogType.Exception || message.IndexOf("ChudOS", StringComparison.OrdinalIgnoreCase) < 0 && stack.IndexOf("ChudOS", StringComparison.OrdinalIgnoreCase) < 0) return;
        // Keep only a short first line; never store request bodies or stack dumps in the panel.
        string line = message.Split('\n')[0]; if (line.Length > 180) line = line.Substring(0, 180);
        lock (sync) { if (errors.Count == 0 || errors.Last() != line) errors.Enqueue(line); while (errors.Count > 12) errors.Dequeue(); }
    }
    protected override void Tick()
    {
        if (Time.unscaledTime < nextSample) return;
        nextSample = Time.unscaledTime + 1f;
        lock (sync) displayed = errors.ToArray();
        status = "Player: " + (CreativeTools.Ready ? "ready" : "waiting") + "\nRoom: " + (Photon.Pun.PhotonNetwork.InRoom ? "joined" : "offline") + "\nThrowable: " + ChudOS.Mods.Spammers.ThrowableSpam.Status + "\nJarvis: " + JarvisManager.State;
    }
    private void OnGUI() => window = MenuHelper.MakeWindow(856100, window, Contents, "ChudOS Mod Health");
    private void Contents(int id)
    {
        MenuHelper.Label(status); MenuHelper.Label("Recent grouped ChudOS errors"); scroll = GUILayout.BeginScrollView(scroll);
        textStyle ??= new GUIStyle(GUI.skin.label) { wordWrap = true, richText = false };
        textStyle.normal.textColor = ChudOS.Core.Main.TextColorValue;
        foreach (string error in displayed) GUILayout.Label(error, textStyle);
        GUILayout.EndScrollView();
        if (MenuHelper.Button("Clear Displayed Errors")) { lock (sync) errors.Clear(); displayed = Array.Empty<string>(); }
        GUI.DragWindow();
    }
    protected override void Cleanup() => Application.logMessageReceivedThreaded -= Log;
}
