using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;

namespace ChudOS.Mods.SoundBoard;

public static class VoiceMods
{
    private static readonly Dictionary<GorillaPlayerScoreboardLine, NetPlayer> muted = new();
    private static readonly List<GorillaPlayerScoreboardLine> staleMutes = new();
    private static float nextMuteScan, nextPitchCheck;
    private static Recorder? mutedRecorder, pitchedRecorder;
    private static bool wasRecording, ownedShifter, oldShifterEnabled;
    private static ChudOSMicPitchShifter? shifter;
    private static float originalPitch;
    private static readonly float[] Pitches = { 1f, 0.5f, 0.75f, 1.25f, 1.5f, 2f };

    [MenuButton("Mute Other Players", ButtonType.Toggle, Category = "Soundboard", Order = 100, Description = "Locally mutes others; disabling restores mutes changed by this button")]
    public static void MuteOtherPlayers()
    {
        if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom)
        {
            RestorePlayerMutes();
            return;
        }
        if (Time.unscaledTime < nextMuteScan) return;
        nextMuteScan = Time.unscaledTime + 1f;
        PortTools.Register(Cleanup);
        staleMutes.Clear();
        foreach (var pair in muted)
            if (pair.Key == null || pair.Key.linePlayer != pair.Value) staleMutes.Add(pair.Key!);
        foreach (var line in staleMutes) muted.Remove(line);
        foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
        {
            if (line == null || line.linePlayer == null || line.linePlayer == NetworkSystem.Instance.LocalPlayer ||
                line.muteButton == null || line.muteButton.isAutoOn || line.muteButton.isOn) continue;
            muted[line] = line.linePlayer;
            line.muteButton.isOn = true;
            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
        }
    }
    [MenuDisable("Mute Other Players")]
    public static void RestorePlayerMutes()
    {
        foreach (var pair in muted)
        {
            var line = pair.Key;
            if (line == null || line.linePlayer != pair.Value || line.muteButton == null) continue;
            line.muteButton.isOn = false;
            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
        }
        muted.Clear(); staleMutes.Clear(); nextMuteScan = 0f;
    }

    private static Recorder? Recorder => GorillaTagger.Instance != null ? GorillaTagger.Instance.myRecorder : null;

    [MenuButton("Microphone Mute", ButtonType.Toggle, Category = "Soundboard", Order = 101, Description = "Mutes the outgoing recorder while enabled")]
    public static void MicrophoneMute()
    {
        Recorder? recorder = Recorder;
        if (recorder == null) return;
        PortTools.Register(Cleanup);
        if (mutedRecorder != recorder)
        {
            RestoreMicrophone();
            mutedRecorder = recorder;
            wasRecording = recorder.IsRecording;
        }
        if (recorder.IsRecording) recorder.StopRecording();
    }
    [MenuDisable("Microphone Mute")]
    public static void RestoreMicrophone()
    {
        if (mutedRecorder != null && wasRecording && !mutedRecorder.IsRecording) mutedRecorder.StartRecording();
        mutedRecorder = null; wasRecording = false;
    }

    [MenuButton("Microphone Pitch", ButtonType.Option, Category = "Soundboard", Order = 102, Options = new[] { "Normal", "0.5x", "0.75x", "1.25x", "1.5x", "2x" }, Description = "Changes outgoing voice pitch; Normal restores the original processor")]
    public static void MicrophonePitch()
    {
        PortTools.Register(Cleanup, MaintainPitch);
        int mode = ButtonManager.GetButton("Microphone Pitch")?.OptionIndex ?? 0;
        if (mode == 0) { RestorePitch(); return; }
        ApplyPitch(mode);
    }

    private static void MaintainPitch()
    {
        if (Time.unscaledTime < nextPitchCheck) return;
        nextPitchCheck = Time.unscaledTime + 1f;
        int mode = ButtonManager.GetButton("Microphone Pitch")?.OptionIndex ?? 0;
        if (mode == 0) return;
        if (pitchedRecorder != Recorder || shifter == null || !shifter.enabled || !Mathf.Approximately(shifter.PitchFactor, Pitches[mode])) ApplyPitch(mode);
    }

    private static void ApplyPitch(int mode)
    {
        Recorder? recorder = Recorder;
        if (recorder == null) return;
        if (pitchedRecorder != recorder || shifter == null)
        {
            RestorePitch();
            pitchedRecorder = recorder;
            shifter = recorder.GetComponent<ChudOSMicPitchShifter>();
            ownedShifter = shifter == null;
            if (shifter == null) shifter = recorder.gameObject.AddComponent<ChudOSMicPitchShifter>();
            originalPitch = shifter.PitchFactor;
            oldShifterEnabled = shifter.enabled;
        }
        shifter.enabled = true;
        shifter.PitchFactor = Pitches[mode];
        if (recorder.IsRecording) recorder.RestartRecording(true);
    }

    private static void RestorePitch()
    {
        if (shifter != null)
        {
            if (ownedShifter)
            {
                shifter.enabled = false;
                UnityEngine.Object.Destroy(shifter);
            }
            else { shifter.PitchFactor = originalPitch; shifter.enabled = oldShifterEnabled; }
            if (pitchedRecorder != null && pitchedRecorder.IsRecording) pitchedRecorder.RestartRecording(true);
        }
        shifter = null; pitchedRecorder = null; ownedShifter = false;
    }

    private static void Cleanup() { RestorePlayerMutes(); RestoreMicrophone(); RestorePitch(); }
}

internal sealed class ChudOSMicPitchShifter : VoiceComponent
{
    internal float PitchFactor = 1f;
    private FloatPitchProcessor? floats;
    private ShortPitchProcessor? shorts;

    public void PhotonVoiceCreated(PhotonVoiceCreatedParams parameters)
    {
        if (!enabled) return;
        if (parameters.Voice is LocalVoiceAudioFloat floatVoice)
        {
            floats = new FloatPitchProcessor(PitchFactor);
            floatVoice.AddPostProcessor(floats);
        }
        else if (parameters.Voice is LocalVoiceAudioShort shortVoice)
        {
            shorts = new ShortPitchProcessor(PitchFactor);
            shortVoice.AddPostProcessor(shorts);
        }
    }

    private void OnDisable()
    {
        if (floats != null) floats.Bypass = true;
        if (shorts != null) shorts.Bypass = true;
    }

    private sealed class FloatPitchProcessor : IProcessor<float>
    {
        private readonly float pitch;
        private float[] output = Array.Empty<float>();
        internal volatile bool Bypass;
        internal FloatPitchProcessor(float factor) => pitch = Math.Clamp(factor, 0.5f, 2f);
        public float[] Process(float[] input)
        {
            if (Bypass || input.Length == 0) return input;
            if (output.Length != input.Length) output = new float[input.Length];
            Array.Clear(output, 0, output.Length);
            float position = 0f;
            for (int i = 0; i < output.Length && position < input.Length; i++, position += pitch)
            {
                int a = (int)position, b = Math.Min(a + 1, input.Length - 1);
                output[i] = input[a] + (input[b] - input[a]) * (position - a);
            }
            return output;
        }
        public void Dispose() => Bypass = true;
    }

    private sealed class ShortPitchProcessor : IProcessor<short>
    {
        private readonly float pitch;
        private short[] output = Array.Empty<short>();
        internal volatile bool Bypass;
        internal ShortPitchProcessor(float factor) => pitch = Math.Clamp(factor, 0.5f, 2f);
        public short[] Process(short[] input)
        {
            if (Bypass || input.Length == 0) return input;
            if (output.Length != input.Length) output = new short[input.Length];
            Array.Clear(output, 0, output.Length);
            float position = 0f;
            for (int i = 0; i < output.Length && position < input.Length; i++, position += pitch)
            {
                int a = (int)position, b = Math.Min(a + 1, input.Length - 1);
                output[i] = (short)(input[a] + (input[b] - input[a]) * (position - a));
            }
            return output;
        }
        public void Dispose() => Bypass = true;
    }
}
