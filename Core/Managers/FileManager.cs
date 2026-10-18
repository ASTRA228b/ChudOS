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


using ChudOS.Stuff;
using ChudOS.Libraries.Notifications;
using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ChudOS.Core.Managers;

public static class FileManager
{
    private static bool Initialized;
    private static bool Downloading;
    private static bool MenuStateLoaded;

    private const string SoundURL = "https://iimenu-lts-serverdata.vercel.app/nt4.mp3";

    public static string RootFolder => Path.Combine(Paths.GameRootPath, "ChudOS");
    public static string SoundFolder => Path.Combine(RootFolder, "Sounds");
    public static string SoundLinks => Path.Combine(RootFolder, "sounds.txt");
    public static string SettingsPath => Path.Combine(RootFolder, "settings.json");
    public static string MenuStatePath => Path.Combine(RootFolder, "menu-state.json");

    public sealed class SoundEntry
    {
        public string Name = "";
        public string Source = "";

        public SoundEntry(string name, string source)
        {
            Name = name;
            Source = source;
        }
    }

    public sealed class SavedMenuState
    {
        public List<SavedButton> Buttons { get; set; } = new();
    }

    public sealed class SavedButton
    {
        public string Name { get; set; } = "";
        public bool Enabled { get; set; }
        public int OptionIndex { get; set; }
    }

    public static void Initialize(MonoBehaviour R)
    {
        if (Initialized) return;

        try
        {
            Directory.CreateDirectory(SoundFolder);

            string readme = Path.Combine(SoundFolder, "README.txt");
            string sample = Path.Combine(SoundFolder, "nt4.mp3");

            if (!File.Exists(readme))
            {
                string content = "CHUDOS - SOUNDS\n\n" +
                    "Place MP3, WAV, or OGG audio in this folder.\n" +
                    "In the parent folder, add direct HTTPS links to sounds.txt, one per line.\n" +
                    "Optional names: My Sound | https://example.com/audio.mp3\n" +
                    "Lines starting with # are ignored. Use Reload Sounds in the GUI.\n" +
                    "Only use audio you have permission to play.\n";

                File.WriteAllText(readme, content);
            }

            if (!File.Exists(SoundLinks))
                File.WriteAllText(SoundLinks, "# Direct HTTPS audio URLs, one per line\n# My Sound | https://example.com/audio.mp3\n");

            if (!File.Exists(sample) && R != null)
                R.StartCoroutine(DownloadSounds(sample));

            Initialized = true;
            Debug.Log($"[{Constantss.GUID}] Sound folder ready: {RootFolder}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] FileManager: {ex}");
        }
    }

    public static void SaveMenuState()
    {
        try
        {
            if (!ButtonManager.Initialized)
                throw new InvalidOperationException("ButtonManager is not initialized.");

            Directory.CreateDirectory(RootFolder);
            SavedMenuState state = new();

            foreach (var button in ButtonManager.Buttons)
            {
                if (button.Type != ButtonType.Toggle && button.Type != ButtonType.Option) continue;

                state.Buttons.Add(new SavedButton
                {
                    Name = button.Name,
                    Enabled = button.Enabled,
                    OptionIndex = button.OptionIndex
                });
            }

            if (state.Buttons.Count == 0)
                throw new InvalidOperationException("No toggle or option buttons were registered.");

            string json = JsonConvert.SerializeObject(state, Formatting.Indented);
            File.WriteAllText(MenuStatePath, json);

            int enabled = state.Buttons.Count(b => b.Enabled);
            int options = state.Buttons.Count(b => ButtonManager.GetButton(b.Name)?.Type == ButtonType.Option);

            Debug.Log($"[{Constantss.GUID}] Saved {state.Buttons.Count} buttons, {enabled} enabled, {options} options to {MenuStatePath}");
            NotifyLib.SendNotificationALL($"Saved menu state! ({enabled} mods enabled)", 3500);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] SaveMenuState: {ex}");
            NotifyLib.SendNotificationALL("Failed to save menu state!", 3500);
        }
    }

    public static void LoadMenuState()
    {
        if (MenuStateLoaded || !File.Exists(MenuStatePath)) return;

        if (!ButtonManager.Initialized)
        {
            Debug.LogWarning($"[{Constantss.GUID}] Cannot restore menu state before button initialization.");
            return;
        }

        try
        {
            string json = File.ReadAllText(MenuStatePath);
            SavedMenuState? state = JsonConvert.DeserializeObject<SavedMenuState>(json);

            if (state?.Buttons == null || state.Buttons.Count == 0)
            {
                Debug.LogWarning($"[{Constantss.GUID}] Menu state file is empty or invalid: {MenuStatePath}");
                return;
            }

            int restoredOptions = 0;
            int restoredToggles = 0;

            foreach (SavedButton saved in state.Buttons)
            {
                if (saved == null || string.IsNullOrWhiteSpace(saved.Name)) continue;

                var button = ButtonManager.GetButton(saved.Name);
                if (button == null || button.Type != ButtonType.Option) continue;

                int count = button.Attribute.Options.Length;
                if (count == 0) continue;

                int index = Mathf.Clamp(saved.OptionIndex, 0, count - 1);

                try
                {
                    if (button.OptionIndex != index)
                        ButtonManager.ChangeOption(button, index - button.OptionIndex);
                    else
                        button.Run();

                    restoredOptions++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{Constantss.GUID}] Failed restoring option {saved.Name}: {ex}");
                }
            }

            foreach (SavedButton saved in state.Buttons)
            {
                if (saved == null || string.IsNullOrWhiteSpace(saved.Name)) continue;

                var button = ButtonManager.GetButton(saved.Name);
                if (button == null || button.Type != ButtonType.Toggle) continue;

                ButtonManager.SetEnabled(button, saved.Enabled);
                restoredToggles++;
            }

            if (restoredOptions == 0 && restoredToggles == 0)
            {
                Debug.LogWarning($"[{Constantss.GUID}] No matching registered buttons were found in the saved menu state.");
                return;
            }

            MenuStateLoaded = true;

            Debug.Log($"[{Constantss.GUID}] Restored {restoredToggles} toggle states and {restoredOptions} options.");
            NotifyLib.SendNotificationALL("Last saved menu state loaded!", 3500);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] LoadMenuState: {ex}");
        }
    }

    public static List<SoundEntry> GetSounds()
    {
        List<SoundEntry> sounds = new();

        try
        {
            if (Directory.Exists(SoundFolder))
            {
                foreach (string file in Directory.GetFiles(SoundFolder))
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext == ".mp3" || ext == ".wav" || ext == ".ogg")
                        sounds.Add(new SoundEntry(Path.GetFileNameWithoutExtension(file), file));
                }
            }

            if (File.Exists(SoundLinks))
            {
                foreach (string raw in File.ReadAllLines(SoundLinks))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;

                    int split = line.IndexOf('|');
                    string url = (split >= 0 ? line.Substring(split + 1) : line).Trim();

                    if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || uri.Scheme != Uri.UriSchemeHttps) continue;

                    string ext = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
                    if (ext != ".mp3" && ext != ".wav" && ext != ".ogg") continue;

                    string name = split >= 0 ? line.Substring(0, split).Trim() : Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                    sounds.Add(new SoundEntry(string.IsNullOrWhiteSpace(name) ? "Remote Sound" : name, url));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] Sounds: {ex}");
        }

        return sounds;
    }

    public static IEnumerator DownloadSounds(string SavePath)
    {
        if (File.Exists(SavePath) || Downloading) yield break;

        Downloading = true;

        using UnityWebRequest req = UnityWebRequest.Get(SoundURL);
        req.timeout = 12;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[{Constantss.GUID}] Optional sample sound unavailable: {req.error}");
            Downloading = false;
            yield break;
        }

        try
        {
            if (!File.Exists(SavePath))
                File.WriteAllBytes(SavePath, req.downloadHandler.data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] Download save failed: {ex}");
        }

        Downloading = false;
    }
}
