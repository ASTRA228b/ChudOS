// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using BepInEx;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Main;

public static class MainUtilities
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extra);

    [MenuButton("Open Game Folder", ButtonType.Press, Category = "Main", Order = 31)]
    public static void OpenFolder() => Process.Start(new ProcessStartInfo(Paths.GameRootPath) { UseShellExecute = true });
    [MenuButton("Restart Game", ButtonType.Press, Category = "Main", Order = 32, Description = "Exit Gorilla Tag, then relaunch it through Steam")]
    public static void Restart()
    {
        using var current = Process.GetCurrentProcess();
        string script = RestartScript(current.Id);
        string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        try
        {
            using var launcher = Process.Start(new ProcessStartInfo("powershell.exe", "-NoProfile -NonInteractive -EncodedCommand " + encoded)
            {
                UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden
            });
            if (launcher == null) { NotifyLib.SendNotificationALL("The restart helper could not start.", 3500); return; }
            Application.Quit();
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            UnityEngine.Debug.LogWarning("[ChudOS Restart] " + ex.GetType().Name);
            NotifyLib.SendNotificationALL("The restart helper could not start. The game will stay open.", 4000);
        }
    }
    internal static string RestartScript(int processId) => "$game = Get-Process -Id " + processId.ToString(System.Globalization.CultureInfo.InvariantCulture) + " -ErrorAction SilentlyContinue; if ($game -and !$game.WaitForExit(30000)) { exit 1 }; Start-Process -FilePath 'steam://rungameid/1533390' -WindowStyle Hidden";

    [MenuButton("Desktop Media Play / Pause", ButtonType.Press, Category = "Soundboard", Order = 40)]
    public static void PlayPause() => MediaKey(0xB3);
    [MenuButton("Desktop Media Next", ButtonType.Press, Category = "Soundboard", Order = 41)]
    public static void Next() => MediaKey(0xB0);
    [MenuButton("Desktop Media Previous", ButtonType.Press, Category = "Soundboard", Order = 42)]
    public static void Previous() => MediaKey(0xB1);
    private static void MediaKey(byte key)
    {
        if (Application.platform != RuntimePlatform.WindowsPlayer) return;
        keybd_event(key, 0, 0, UIntPtr.Zero); keybd_event(key, 0, 2, UIntPtr.Zero);
    }
}
