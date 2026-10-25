// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using UnityEngine;

namespace ChudOS.Core;

public sealed partial class Main
{
    public static bool SoundboardControlOpen { get; private set; }
    private static readonly List<GameObject> SearchObjects = new();
    private static SoundboardMarquee? SongTitle, SongArtist;
    private static TextMesh? SongElapsed, SongRemaining, SongStatus, SongPlaybackFallback;
    private static Transform? SongProgress;
    private static GameObject? SongPlayIcon, SongPauseIcon;
    private static float nextSongUpdate;
    private static int lastElapsed = -1, lastRemaining = -1;

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
    private static void SoundboardChanged()
    {
        if (!SoundboardControlOpen) RequestRefresh();
        else nextSongUpdate = 0f;
    }
    private static void BuildSoundboardControl()
    {
        if (MenuMain == null) return;
        GameObject? icon = IconLoader.Create("Music", MenuMain.transform, new Vector3(0.704f, 0.32f, 0f), new Vector2(0.18f, 0.18f));
        if (icon != null) PageObjects.Add(icon);
        SongTitle = SoundboardMarquee.Create(MenuMain.transform, new Vector3(0.696f, 0.12f, 0f), TextColor);
        PageObjects.Add(SongTitle.gameObject);
        SongArtist = SoundboardMarquee.Create(MenuMain.transform, new Vector3(0.696f, -0.025f, 0f), SecondaryTextColor, 0.85f, 0.085f, 0.0014f);
        PageObjects.Add(SongArtist.gameObject);
        SongStatus = Text("", -0.12f, 0f, 0.010f, color: SecondaryTextColor);
        SongElapsed = Text("00:00", -0.23f, -0.39f, 0.010f);
        SongRemaining = Text("-00:00", -0.23f, 0.39f, 0.010f);
        foreach (TextMesh label in new[] { SongStatus, SongElapsed, SongRemaining }) PageObjects.Add(label.gameObject);
        PageObjects.Add(Cube("Sound Progress Track", new Vector3(0.626f, -0.23f, 0f), new Vector3(0.015f, 0.012f, 0.59f), HeaderMat));
        GameObject fill = Cube("Sound Progress", new Vector3(0.639f, -0.23f, -0.295f), new Vector3(0.015f, 0.014f, 0.001f), BorderMat);
        SongProgress = fill.transform;
        PageObjects.Add(fill);
        ControlButton("<<<", -0.43f, -0.31f, 0.27f, SoundboardManager.Previous);
        ControlButton("", -0.43f, 0f, 0.27f, SoundboardManager.TogglePause);
        ControlButton(">>>", -0.43f, 0.31f, 0.27f, SoundboardManager.Next);
        ControlButton("Back to Main", -0.65f, 0f, 0.9f, CloseSoundboardControl);
        SongPlayIcon = PlaybackIcon("Play");
        SongPauseIcon = PlaybackIcon("Pause");
        SongPlaybackFallback = null;
        if (SongPlayIcon == null || SongPauseIcon == null)
        {
            if (SongPlayIcon != null) SongPlayIcon.SetActive(false);
            if (SongPauseIcon != null) SongPauseIcon.SetActive(false);
            SongPlaybackFallback = Text(">", -0.43f, 0f, 0.021f);
            PageObjects.Add(SongPlaybackFallback.gameObject);
        }
        lastElapsed = lastRemaining = -1;
        nextSongUpdate = 0f;
        UpdateSoundboardControl();
    }
    private static GameObject? PlaybackIcon(string name)
    {
        var icon = IconLoader.Create(name, MenuMain!.transform, new Vector3(0.704f, -0.43f, 0f), new Vector2(0.09f, 0.09f));
        if (icon == null) return null;
        var properties = new MaterialPropertyBlock();
        properties.SetColor("_Color", TextColor);
        icon.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        PageObjects.Add(icon);
        return icon;
    }
    private static void ControlButton(string label, float y, float z, float width, Action action)
    {
        GameObject button = MakeButton(label, y, z, width, 0.16f, track: PageObjects);
        var trigger = button.GetComponent<MenuButtonCollider>();
        trigger.OnPress = action;
        if (trigger.Label != null) SoundboardTextFit.Apply(trigger.Label, width - 0.08f, 0.12f);
    }
    private static string SongTime(int total) => $"{total / 60:00}:{total % 60:00}";
    private static void SetSongText(TextMesh? label, string value)
    {
        if (label != null && label.text != value) label.text = value;
    }
    private static void UpdateSoundboardControl()
    {
        if (!SoundboardControlOpen || SongTitle == null || !SongTitle.gameObject.activeInHierarchy || Time.unscaledTime < nextSongUpdate) return;
        nextSongUpdate = Time.unscaledTime + 0.2f;
        var song = SoundboardManager.Playing ?? SoundboardManager.Selected;
        SongTitle.SetSong(song?.Path ?? "", song?.Title ?? TranslationManager.T("No sound selected"));
        string artist = string.IsNullOrWhiteSpace(song?.Artist) ? TranslationManager.T("Unknown Artist") : song!.Artist;
        if (SongArtist != null) SongArtist.SetSong(song?.Path ?? "", artist);
        SetSongText(SongStatus, TranslationManager.T(SoundboardManager.IsLoading ? "Loading..." : SoundManager.IsPaused ? "Paused" : SoundManager.IsPlaying ? "Playing" : "Stopped"));
        bool playing = SoundManager.IsPlaying;
        if (SongPlaybackFallback != null) SetSongText(SongPlaybackFallback, playing ? "||" : ">");
        else
        {
            if (SongPlayIcon != null && SongPlayIcon.activeSelf == playing) SongPlayIcon.SetActive(!playing);
            if (SongPauseIcon != null && SongPauseIcon.activeSelf != playing) SongPauseIcon.SetActive(playing);
        }
        float length = SoundManager.CurrentLength, time = SoundManager.CurrentTime;
        int elapsed = Mathf.Max(0, Mathf.FloorToInt(time)), remaining = Mathf.Max(0, Mathf.FloorToInt(length - time));
        if (elapsed != lastElapsed) { lastElapsed = elapsed; SetSongText(SongElapsed, SongTime(elapsed)); }
        if (remaining != lastRemaining) { lastRemaining = remaining; SetSongText(SongRemaining, "-" + SongTime(remaining)); }
        if (SongProgress != null)
        {
            float progress = length > 0f ? Mathf.Clamp01(time / length) : 0f;
            SongProgress.localScale = new Vector3(0.015f, 0.014f, Mathf.Max(0.001f, 0.59f * progress));
            SongProgress.localPosition = new Vector3(0.639f, -0.23f, -0.295f + 0.295f * progress);
        }
    }
}
