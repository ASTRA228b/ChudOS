// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Stuff;
using System.Diagnostics;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class MacroMods
{
    public static bool Visible;
    public static Rect Window = new(220f, 130f, 430f, 490f);
    private static string name = "New Macro";
    private static string? deleteFile;

    [MenuButton("Macros GUI", ButtonType.Press, Category = "Movement", Order = 150, Notify = false)]
    public static void Open() => SetVisible(!Visible);
    public static void SetVisible(bool visible) { Visible = visible; deleteFile = null; if (!visible) MacroManager.ClearPreview(); }

    [MenuButton("Macro Controls", ButtonType.Toggle, Category = "Macros", Order = 0, runOption = RunOption.Once, Description = "Hold LT to record; hold RT within one metre of the start to play")]
    public static void EnableControls() => MacroManager.SetControls(true);
    [MenuDisable("Macro Controls")]
    public static void DisableControls() => MacroManager.SetControls(false);

    [MenuButton("Record Macro", ButtonType.Press, Category = "Macros", Order = 1, Notify = false)]
    public static void Record() => MacroManager.StartRecording();
    [MenuButton("Stop Macro", ButtonType.Press, Category = "Macros", Order = 2, Notify = false)]
    public static void Stop() => MacroManager.Stop();
    [MenuButton("Play Selected Macro", ButtonType.Press, Category = "Macros", Order = 3, Notify = false)]
    public static void Play() => MacroManager.Play();
    [MenuButton("Next Macro", ButtonType.Press, Category = "Macros", Order = 4, Notify = false)]
    public static void Next() => MacroManager.Select(1);
    [MenuButton("Reload Macros", ButtonType.Press, Category = "Macros", Order = 5, Notify = false)]
    public static void Reload() => MacroManager.Reload();
    [MenuButton("Open Macros Folder", ButtonType.Press, Category = "Macros", Order = 6, Notify = false)]
    public static void OpenFolder()
    {
        Directory.CreateDirectory(MacroManager.Folder);
        Process.Start(new ProcessStartInfo(MacroManager.Folder) { UseShellExecute = true });
    }

    public static void CreateGUI()
    {
        if (Visible) Window = MenuHelper.MakeWindow(847190, Window, WindowContents, "ChudOS Macros");
    }

    private static void WindowContents(int id)
    {
        var selected = MacroManager.Selected;
        MenuHelper.Label($"Selected: {selected?.Name ?? "None"}");
        MenuHelper.Label(MacroManager.Recording ? "Recording..." : MacroManager.IsPlaying ? "Playing..." : "Idle");
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("<")) MacroManager.Select(-1);
        if (MenuHelper.Button(">")) MacroManager.Select(1);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Record")) MacroManager.StartRecording();
        if (MenuHelper.Button("Play")) MacroManager.Play();
        if (MenuHelper.Button("Stop")) MacroManager.Stop();
        GUILayout.EndHorizontal();

        MenuHelper.Space();
        MenuHelper.Label("Recording name");
        name = GUILayout.TextField(name, 60);
        bool enabled = GUI.enabled;
        GUI.enabled = enabled && MacroManager.HasDraft && !MacroManager.Recording;
        if (MenuHelper.Button("Save Recording")) MacroManager.SaveDraft(name);
        GUI.enabled = enabled && selected != null && !MacroManager.Recording && !MacroManager.IsPlaying;
        if (MenuHelper.Button("Rename Selected")) MacroManager.Rename(name);
        if (MenuHelper.Button(deleteFile != null && deleteFile == selected?.File ? "Confirm Delete" : "Delete Selected"))
        {
            if (deleteFile != null && deleteFile == selected?.File) { MacroManager.Delete(); deleteFile = null; }
            else deleteFile = selected?.File;
        }
        GUI.enabled = enabled;
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Preview Route")) MacroManager.Preview();
        if (MenuHelper.Button("Clear Preview")) MacroManager.ClearPreview();
        GUILayout.EndHorizontal();
        if (MenuHelper.Button("Reload Macros")) MacroManager.Reload();
        if (MenuHelper.Button("Open Macros Folder")) OpenFolder();
        MenuHelper.Label("VR: enable Macro Controls. LT records; RT plays near the start.");
        if (MenuHelper.Button("Close")) SetVisible(false);
        GUI.DragWindow(new Rect(0f, 0f, Window.width, 25f));
    }
}
