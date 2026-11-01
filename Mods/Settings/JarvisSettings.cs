// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class JarvisSettings
{
    internal static int Provider, Activation, Input, WakeWord, Voice, Model, SpeechInput;
    internal static float Volume = 0.75f;
    internal static float ListenSeconds = 12f, SilenceSeconds = 3f;
    internal static int Confidence;
    internal static bool Spoken = true, Notifications = true, Controls = true, Dictate;
    internal static readonly string[] WakeWords = { "jarvis", "assistant", "friday", "system" };
    internal static readonly string[] Voices = { "Windows Default", "Brian", "Amy", "Russell", "Nicole", "Matthew", "Joanna", "Raveena" };

    [MenuButton("Jarvis", ButtonType.Toggle, Category = "Main", Order = 20, runOption = RunOption.Once, Notify = false, Description = "0.1.5 beta: spoken local commands or AI. Uses Windows speech; F8 also starts listening.")]
    public static void Enable() => JarvisManager.SetActive(true);
    [MenuDisable("Jarvis")]
    public static void Disable() => JarvisManager.SetActive(false);
    [MenuButton("Jarvis GUI", ButtonType.Press, Category = "Main", Order = 21, Notify = false)]
    public static void Open() => ChudOS.Core.AGUI.JarvisGUI.Visible = !ChudOS.Core.AGUI.JarvisGUI.Visible;
    [MenuButton("Ask Jarvis", ButtonType.Press, Category = "Main", Order = 22, Notify = false, Controls = "(F8)")]
    public static void Ask() => JarvisManager.Listen();
    [MenuButton("Stop Jarvis Response", ButtonType.Press, Category = "Main", Order = 23, Notify = false)]
    public static void Stop() => JarvisManager.Cancel();

    [MenuButton("Jarvis Provider", ButtonType.Option, Category = "Settings", Order = 310, Options = new[] { "Voice Assistant", "GPT Jarvis", "ChudGPT Jarvis" }, Notify = false)]
    public static void SetProvider() { Provider = Index("Jarvis Provider"); JarvisManager.Cancel(); }
    [MenuButton("ChudGPT Model", ButtonType.Option, Category = "Settings", Order = 311, Options = new[] { "Public", "Music", "Buggy", "700", "1300", "1500", "1600", "Ultimate", "Plus", "Pro", "Code", "Mega", "Plato", "Plato Mini" }, Notify = false)]
    public static void SetModel() { Model = Index("ChudGPT Model"); JarvisManager.Cancel(); }
    [MenuButton("Jarvis Activation", ButtonType.Option, Category = "Settings", Order = 312, Options = new[] { "Wake Word", "Push To Talk" }, Notify = false, Description = "Wake words stay active whenever Jarvis is ready. Push To Talk additionally enables the selected controller binding and hold-to-talk on F8.")]
    public static void SetActivation() { Activation = Index("Jarvis Activation"); JarvisManager.Restart(); }
    [MenuButton("Jarvis Input", ButtonType.Option, Category = "Settings", Order = 313, Options = new[] { "RT", "LT", "RG", "LG", "A", "B", "X", "Y", "RJ", "LJ" }, Notify = false)]
    public static void SetInput() => Input = Index("Jarvis Input");
    [MenuButton("Jarvis Wake Word", ButtonType.Option, Category = "Settings", Order = 314, Options = new[] { "Jarvis", "Assistant", "Friday", "System" }, Notify = false)]
    public static void SetWakeWord() { WakeWord = Index("Jarvis Wake Word"); JarvisManager.Restart(); }
    [MenuButton("Jarvis Voice", ButtonType.Option, Category = "Settings", Order = 315, Options = new[] { "Windows Default", "Brian", "Amy", "Russell", "Nicole", "Matthew", "Joanna", "Raveena" }, Notify = false)]
    public static void SetVoice() { Voice = Index("Jarvis Voice"); JarvisManager.Cancel(); }
    [MenuButton("Jarvis Volume", ButtonType.Option, Category = "Settings", Order = 316, Options = new[] { "75%", "25%", "50%", "100%" }, Notify = false)]
    public static void SetVolume() => Volume = new[] { 0.75f, 0.25f, 0.5f, 1f }[Index("Jarvis Volume")];
    [MenuButton("Jarvis Spoken Replies", ButtonType.Toggle, Category = "Settings", Order = 317, DefaultEnabled = true, runOption = RunOption.Once, Notify = false)]
    public static void SpeakOn() => Spoken = true;
    [MenuDisable("Jarvis Spoken Replies")]
    public static void SpeakOff() { Spoken = false; JarvisManager.Cancel(); }
    [MenuButton("Jarvis Notifications", ButtonType.Toggle, Category = "Settings", Order = 318, DefaultEnabled = true, runOption = RunOption.Once, Notify = false)]
    public static void NotifyOn() => Notifications = true;
    [MenuDisable("Jarvis Notifications")]
    public static void NotifyOff() => Notifications = false;
    [MenuButton("Jarvis Menu Controls", ButtonType.Toggle, Category = "Settings", Order = 319, DefaultEnabled = true, runOption = RunOption.Once, Notify = false)]
    public static void ControlsOn() => Controls = true;
    [MenuDisable("Jarvis Menu Controls")]
    public static void ControlsOff() => Controls = false;
    [MenuButton("Jarvis Dictate Notifications", ButtonType.Toggle, Category = "Settings", Order = 320, runOption = RunOption.Once, Notify = false)]
    public static void DictateOn() => Dictate = true;
    [MenuDisable("Jarvis Dictate Notifications")]
    public static void DictateOff() => Dictate = false;
    [MenuButton("Clear Jarvis Conversation", ButtonType.Press, Category = "Settings", Order = 321, Notify = false)]
    public static void Clear() => JarvisManager.ClearConversation();
    [MenuButton("Restart Jarvis", ButtonType.Press, Category = "Settings", Order = 322, Notify = false)]
    public static void Restart() => JarvisManager.Restart();
    [MenuButton("Jarvis Listening Time", ButtonType.Option, Category = "Settings", Order = 323, Options = new[] { "12 Seconds", "8 Seconds", "20 Seconds", "30 Seconds" }, Notify = false)]
    public static void SetListenTime() => ListenSeconds = new[] { 12f, 8f, 20f, 30f }[Index("Jarvis Listening Time")];
    [MenuButton("Jarvis Silence Time", ButtonType.Option, Category = "Settings", Order = 324, Options = new[] { "3 Seconds", "2 Seconds", "5 Seconds", "8 Seconds" }, Notify = false)]
    public static void SetSilenceTime() => SilenceSeconds = new[] { 3f, 2f, 5f, 8f }[Index("Jarvis Silence Time")];
    [MenuButton("Jarvis Recognition", ButtonType.Option, Category = "Settings", Order = 325, Options = new[] { "Balanced", "High Confidence Only", "Allow Low Confidence" }, Notify = false, Description = "Balanced accepts Medium and High. Allow Low Confidence also accepts uncertain final transcriptions, which may recognize the wrong command.")]
    public static void SetConfidence() => Confidence = Index("Jarvis Recognition");
    [MenuButton("Windows Microphone Settings", ButtonType.Press, Category = "Settings", Order = 326, Notify = false)]
    public static void OpenMicrophone() => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("ms-settings:sound") { UseShellExecute = true });
    [MenuButton("Windows Speech Settings", ButtonType.Press, Category = "Settings", Order = 327, Notify = false)]
    public static void OpenSpeech() => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("ms-settings:privacy-speech") { UseShellExecute = true });
    [MenuButton("Jarvis Speech Input", ButtonType.Option, Category = "Settings", Order = 330, Options = new[] { "Automatic", "Local Commands", "Windows Dictation" }, Notify = false, Description = "Automatic uses local command recognition for Voice Assistant and Windows dictation for AI conversation.")]
    public static void SetSpeechInput() { SpeechInput = Index("Jarvis Speech Input"); JarvisManager.Restart(); }
    private static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;
}
