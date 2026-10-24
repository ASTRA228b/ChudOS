// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using UnityEngine;

namespace ChudOS.Core;

public sealed partial class Main
{
    public static bool SoundboardControlOpen { get; private set; }
    private static readonly List<GameObject> SearchObjects = new();
    private static TextMesh? SongTitle, SongArtist, SongElapsed, SongRemaining, SongStatus;
    private static Transform? SongProgress;
    private static float nextSongUpdate;

    public static void OpenSoundboardControl()
    {
        SearchManager.Close();
        SoundboardControlOpen = true;
        RequestRefresh();
    }

    public static void CloseSoundboardControl()
    {
        SoundboardControlOpen = false;
        CategoryManager.Open("Main");
        RequestRefresh();
    }

    private static void BuildSoundboardControl()
    {
        if (MenuMain == null) return;
        GameObject? icon = IconLoader.Create("Music", MenuMain.transform, new Vector3(0.704f, 0.32f, 0f), new Vector2(0.18f, 0.18f));
        if (icon != null) PageObjects.Add(icon);
        SongTitle = Text("", 0.12f, 0f, 0.023f);
        SongArtist = Text("", -0.025f, 0f, 0.014f, color: SecondaryTextColor);
        SongStatus = Text("", -0.12f, 0f, 0.010f, color: SecondaryTextColor);
        SongElapsed = Text("00:00", -0.23f, -0.39f, 0.010f);
        SongRemaining = Text("-00:00", -0.23f, 0.39f, 0.010f);
        foreach (TextMesh label in new[] { SongTitle, SongArtist, SongStatus, SongElapsed, SongRemaining }) PageObjects.Add(label.gameObject);
        PageObjects.Add(Cube("Sound Progress Track", new Vector3(0.626f, -0.23f, 0f), new Vector3(0.015f, 0.012f, 0.59f), HeaderMat));
        GameObject fill = Cube("Sound Progress", new Vector3(0.639f, -0.23f, -0.295f), new Vector3(0.015f, 0.014f, 0.001f), BorderMat);
        SongProgress = fill.transform;
        PageObjects.Add(fill);
        ControlButton("<<<", -0.43f, -0.31f, 0.27f, SoundboardManager.Previous);
        ControlButton(SoundManager.IsPlaying ? "||" : ">", -0.43f, 0f, 0.27f, SoundboardManager.TogglePause);
        ControlButton(">>>", -0.43f, 0.31f, 0.27f, SoundboardManager.Next);
        ControlButton("Back to Main", -0.65f, 0f, 0.9f, CloseSoundboardControl);
        nextSongUpdate = 0f;
        UpdateSoundboardControl();
    }

    private static void ControlButton(string label, float y, float z, float width, Action action)
    {
        GameObject button = MakeButton(label, y, z, width, 0.16f, track: PageObjects);
        button.GetComponent<MenuButtonCollider>().OnPress = action;
    }

    private static string SongTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }

    private static void UpdateSoundboardControl()
    {
        if (!SoundboardControlOpen || SongTitle == null || Time.unscaledTime < nextSongUpdate) return;
        nextSongUpdate = Time.unscaledTime + 0.2f;
        SoundboardManager.SoundData? song = SoundboardManager.Playing ?? SoundboardManager.Selected;
        SongTitle.text = song?.Title ?? TranslationManager.T("No sound selected");
        if (SongTitle.text.Length > 48) SongTitle.text = SongTitle.text.Substring(0, 45) + "...";
        FitButtonText(SongTitle, SongTitle.text, 0.9f);
        if (SongArtist != null)
        {
            SongArtist.text = string.IsNullOrWhiteSpace(song?.Artist) ? TranslationManager.T("Unknown Artist") : song!.Artist;
            if (SongArtist.text.Length > 40) SongArtist.text = SongArtist.text.Substring(0, 37) + "...";
            FitButtonText(SongArtist, SongArtist.text, 0.85f);
            SongArtist.characterSize = Mathf.Min(SongArtist.characterSize, 0.014f);
        }
        if (SongStatus != null) SongStatus.text = TranslationManager.T(SoundboardManager.IsLoading ? "Loading..." : SoundManager.IsPaused ? "Paused" : SoundManager.IsPlaying ? "Playing" : "Stopped");
        float length = SoundManager.CurrentLength, time = SoundManager.CurrentTime;
        if (SongElapsed != null) SongElapsed.text = SongTime(time);
        if (SongRemaining != null) SongRemaining.text = "-" + SongTime(Mathf.Max(0f, length - time));
        if (SongProgress != null)
        {
            float progress = length > 0f ? Mathf.Clamp01(time / length) : 0f;
            SongProgress.localScale = new Vector3(0.015f, 0.014f, Mathf.Max(0.001f, 0.59f * progress));
            SongProgress.localPosition = new Vector3(0.639f, -0.23f, -0.295f + 0.295f * progress);
        }
    }
}
