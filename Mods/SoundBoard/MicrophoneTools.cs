// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using Photon.Voice.Unity;

namespace ChudOS.Mods.SoundBoard;

internal sealed class MicrophoneTools : BetaFeature
{
    private Recorder? recorder;
    private bool echo, captured;
    private static Recorder? Current => NetworkSystem.Instance != null && NetworkSystem.Instance.VoiceConnection != null ? NetworkSystem.Instance.VoiceConnection.PrimaryRecorder : GorillaTagger.Instance != null ? GorillaTagger.Instance.myRecorder : null;

    [MenuButton("Microphone Echo", ButtonType.Toggle, Category = "Soundboard", Order = 810, runOption = RunOption.Once, Description = "Test the game voice recorder's debug echo. Restores its old state on disable.")]
    public static void EnableEcho() => Enable<MicrophoneTools>("Microphone Echo");
    [MenuDisable("Microphone Echo")]
    public static void DisableEcho() => Disable("Microphone Echo");
    [MenuButton("Refresh Microphone", ButtonType.Press, Category = "Soundboard", Order = 811, Description = "Restart the active game recorder. This does not restart Windows Jarvis recognition.")]
    public static void Refresh()
    {
        var recorder = Current;
        if (recorder == null) { CreativeTools.Notice("The game microphone recorder is not ready."); return; }
        if (!recorder.IsRecording) { CreativeTools.Notice("The recorder is stopped or muted; its recording state was left unchanged."); return; }
        recorder.RestartRecording(true); CreativeTools.Notice("Game microphone restart requested.");
    }
    protected override void Setup()
    {
        recorder = Current;
        if (recorder == null) { Unavailable("the voice recorder is not ready."); return; }
        echo = recorder.DebugEchoMode; captured = true; recorder.DebugEchoMode = true;
    }
    protected override void Tick() { if (recorder == null || recorder != Current) Unavailable("the voice recorder changed; enable echo again."); }
    protected override void Cleanup() { if (captured && recorder != null) recorder.DebugEchoMode = echo; }
}
