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

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using UnityEngine;

namespace ChudOS.Mods.SoundBoard;

public static class SoundboardGUI
{
    public static bool Visible;
    public static Rect Window = new(165f, 145f, 530f, 460f);
    private static Vector2 Scroll;
    private static int Tab;
    private static readonly string[] Tabs = { "Sounds", "Settings" };

    public static void CreateGUI()
    {
        if (!Visible) return;
        Window = MenuHelper.MakeWindow(847189, Window, WindowContents, "ChudOS Soundboard");
    }

    private static void WindowContents(int id)
    {
        Tabs[0] = TranslationManager.T("Sounds");
        Tabs[1] = TranslationManager.T("Settings");
        Tab = MenuHelper.Toolbar(Tab, Tabs);
        switch (Tab)
        {
            case 0: Sounds(); break;
            case 1: Settings(); break;
        }

        MenuHelper.Space();
        if (MenuHelper.Button("Close")) Visible = false;
        GUI.DragWindow(new Rect(0f, 0f, Window.width, 25f));
    }

    public static void Sounds()
    {
        MenuHelper.Label($"Selected: {SoundboardManager.CurrentName}");
        MenuHelper.Label($"Playing: {SoundboardManager.Playing?.Title ?? "None"}");
        MenuHelper.Label($"{SoundManager.CurrentTime:F1}s / {SoundManager.CurrentLength:F1}s");

        float progress = SoundManager.CurrentLength > 0f ? Mathf.Clamp01(SoundManager.CurrentTime / SoundManager.CurrentLength) : 0f;
        bool enabled = GUI.enabled;
        GUI.enabled = false;
        GUILayout.HorizontalSlider(progress, 0f, 1f, GlobalStyles.SliderStyle, GlobalStyles.SliderThumbStyle);
        GUI.enabled = enabled;

        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("<<")) SoundboardManager.Previous();
        if (MenuHelper.Button(SoundManager.IsPaused ? "Resume" : SoundManager.IsPlaying ? "Pause" : "Play"))
        {
            SoundboardManager.TogglePause();
        }
        if (MenuHelper.Button("Stop")) SoundboardManager.Stop();
        if (MenuHelper.Button(">>")) SoundboardManager.Next();
        GUILayout.EndHorizontal();

        MenuHelper.Space(8f);
        MenuHelper.Label($"Sounds ({SoundboardManager.Sounds.Count})");
        Scroll = GUILayout.BeginScrollView(Scroll, GUILayout.Height(225f));
        if (SoundboardManager.Sounds.Count == 0) MenuHelper.Label("No sounds found. Add sounds to the Sounds folder.");

        int selectedIndex = SoundboardManager.SelectedIndex;
        for (int i = 0; i < SoundboardManager.Sounds.Count; i++)
        {
            int index = i;
            bool selected = selectedIndex == index;
            bool playing = SoundboardManager.Playing?.Path == SoundboardManager.Sounds[i].Path && (SoundManager.IsPlaying || SoundManager.IsPaused || SoundboardManager.IsLoading);
            string title = SoundboardManager.Sounds[i].Name;
            if (title.Length > 40) title = title.Substring(0, 37) + "...";
            string name = $"{(playing ? "[Playing] " : selected ? "[Selected] " : "")}{title}";
            if (MenuHelper.Button(name))
            {
                SoundboardManager.Select(index);
                SoundboardManager.Play();
            }
        }
        GUILayout.EndScrollView();
        if (MenuHelper.Button("Reload Sounds")) SoundboardManager.Reload();
    }

    public static void Settings()
    {
        MenuHelper.Label($"Playback Mode: {SoundManager.Mode}");
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Local")) SoundboardManager.SetPlaybackMode(SoundManager.PlaybackMode.Local);
        if (MenuHelper.Button("Mic")) SoundboardManager.SetPlaybackMode(SoundManager.PlaybackMode.Microphone);
        if (MenuHelper.Button("Both")) SoundboardManager.SetPlaybackMode(SoundManager.PlaybackMode.Both);
        GUILayout.EndHorizontal();

        MenuHelper.Space(10f);
        MenuHelper.Label($"Local Volume: {SoundboardManager.Presets[SoundboardManager.VolumeIndex]:P0}");
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("-")) SoundboardManager.SetVolume((SoundboardManager.VolumeIndex - 1 + SoundboardManager.Presets.Length) % SoundboardManager.Presets.Length);
        if (MenuHelper.Button("+")) SoundboardManager.SetVolume((SoundboardManager.VolumeIndex + 1) % SoundboardManager.Presets.Length);
        GUILayout.EndHorizontal();

        MenuHelper.Space(8f);
        MenuHelper.Label($"Microphone Volume: {SoundboardManager.Presets[SoundboardManager.MicVolumeIndex]:P0}");
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("-")) SoundboardManager.SetMicVolume((SoundboardManager.MicVolumeIndex - 1 + SoundboardManager.Presets.Length) % SoundboardManager.Presets.Length);
        if (MenuHelper.Button("+")) SoundboardManager.SetMicVolume((SoundboardManager.MicVolumeIndex + 1) % SoundboardManager.Presets.Length);
        GUILayout.EndHorizontal();

        MenuHelper.Space(10f);
        // The manager synchronizes these GUI controls with the registered Toggle buttons.
        if (MenuHelper.Button($"Autoplay: {(SoundboardManager.AutoPlay ? "ON" : "OFF")}")) SoundboardManager.SetAutoPlay(!SoundboardManager.AutoPlay);
        if (MenuHelper.Button($"Repeat: {(SoundboardManager.Repeat ? "ON" : "OFF")}")) SoundboardManager.SetRepeat(!SoundboardManager.Repeat);
        if (MenuHelper.Button($"Shuffle: {(SoundboardManager.Shuffle ? "ON" : "OFF")}")) SoundboardManager.SetShuffle(!SoundboardManager.Shuffle);

        MenuHelper.Space(8f);
        MenuHelper.Label($"Loaded Sounds: {SoundboardManager.Sounds.Count}");
    }
}
