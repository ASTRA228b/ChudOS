// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Core.Managers;

public static class MenuFeedback
{
    private static readonly int[] SoundIds = { 8, 9, 66, 67, 84, 106, -1 };
    private static readonly float[] Volumes = { 0.4f, 0.25f, 0.5f, 0.75f, 1f };
    private static AudioSource? source;
    private static int sound;
    private static float volume = 0.4f, nextClick;
    private static bool vibrations = true;

    [MenuButton("Click Sound", ButtonType.Option, Category = "Settings", Order = 40, Options = new[] { "Original", "Wood", "Keyboard", "Default", "Bubble", "Jar", "Off" }, Notify = false)]
    public static void SetSound() => sound = Mathf.Clamp(ButtonManager.GetButton("Click Sound")?.OptionIndex ?? 0, 0, SoundIds.Length - 1);

    [MenuButton("Click Volume", ButtonType.Option, Category = "Settings", Order = 41, Options = new[] { "40%", "25%", "50%", "75%", "100%" }, Notify = false)]
    public static void SetVolume() => volume = Volumes[Mathf.Clamp(ButtonManager.GetButton("Click Volume")?.OptionIndex ?? 0, 0, Volumes.Length - 1)];

    [MenuButton("Click Vibrations", ButtonType.Toggle, Category = "Settings", Order = 42, DefaultEnabled = true, runOption = RunOption.Once, Notify = false)]
    public static void EnableVibrations() => vibrations = true;
    [MenuDisable("Click Vibrations")]
    public static void DisableVibrations() => vibrations = false;

    public static void Play(bool? rightHand = null)
    {
        if (Time.unscaledTime < nextClick) return;
        nextClick = Time.unscaledTime + 0.04f;
        if (vibrations && rightHand.HasValue && GorillaTagger.Instance != null)
            GorillaTagger.Instance.StartVibration(rightHand.Value, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
        int id = SoundIds[sound];
        var materials = GTPlayer.Instance != null ? GTPlayer.Instance.materialData : null;
        if (id < 0 || materials == null || materials.Count == 0) return;
        if (id >= materials.Count) id = materials.Count > 8 ? 8 : 0;
        var surface = materials[id];
        AudioClip? clip = surface.overrideAudio ? surface.audio : materials[0].audio;
        if (clip == null) clip = materials[0].audio;
        if (clip == null) return;
        if (source == null)
        {
            GameObject obj = new("ChudOS Click Feedback");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            source.volume = 1f;
        }
        source.PlayOneShot(clip, volume);
    }

    public static void Shutdown()
    {
        if (source != null) UnityEngine.Object.Destroy(source.gameObject);
        source = null;
    }
}
