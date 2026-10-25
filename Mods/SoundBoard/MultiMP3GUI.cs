// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.SoundBoard;

public static class MultiMP3GUI
{
    private static readonly string[] Qualities = { "128", "192", "256", "320", "Best" };
    private static string url = "";
    private static int quality = 1;

    [MenuButton("Download YouTube Audio", ButtonType.Press, Category = "Soundboard", Order = 10, Notify = false)]
    public static void Open() => SoundboardGUI.OpenDownloads();
    [MenuButton("Download Clipboard Audio", ButtonType.Press, Category = "Soundboard", Order = 11, Notify = false, Description = "Download the YouTube link copied to the Windows clipboard")]
    public static void DownloadClipboard()
    {
        if (MultiMP3Manager.Busy) return;
        url = GUIUtility.systemCopyBuffer ?? "";
        MultiMP3Manager.Download(url, quality);
    }

    public static void CreateGUI()
    {
        MenuHelper.Label("YouTube link");
        bool enabled = GUI.enabled;
        GUI.enabled = enabled && !MultiMP3Manager.Busy;
        url = GUILayout.TextField(url, 2048);
        if (MenuHelper.Button("Paste")) url = GUIUtility.systemCopyBuffer ?? "";
        MenuHelper.Space();
        MenuHelper.Label("MP3 Quality");
        quality = MenuHelper.Toolbar(quality, Qualities);
        if (MenuHelper.Button("Download")) MultiMP3Manager.Download(url, quality);
        GUI.enabled = enabled;
        if (MultiMP3Manager.Busy && MenuHelper.Button("Cancel Download")) MultiMP3Manager.Cancel();
        MenuHelper.Space();
        MenuHelper.Label(MultiMP3Manager.Status);
    }
}
