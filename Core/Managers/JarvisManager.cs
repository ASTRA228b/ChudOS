// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections;
using System.Collections.Concurrent;
using ChudOS.Core.AGUI;
using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using ChudOS.Mods.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

namespace ChudOS.Core.Managers;

internal sealed class JarvisManager : MonoBehaviour
{
    private static JarvisManager? instance;
    private readonly JarvisAI ai = new();
    private readonly ConcurrentQueue<(int Kind, string Text)> events = new();
    private JarvisSpeech? speech;
    private KeywordRecognizer? wake;
    private IJarvisCapture? recognition;
    private Coroutine? job;
    private bool active, held, talkHeld, inputFailed, applyingCommand, rebuildRecognition, clearRequested;
    private float nextDictate, nextWakeRetry, nextWakeCheck;
    private bool wakeErrorShown;
    private IJarvisCapture? observedRecognition;
    private int observedRevision = -1;
    private SpeechSystemStatus observedStatus;
    internal static string State { get; private set; } = "Disabled";
    internal static string LastInput { get; private set; } = "";
    internal static string LastReply { get; private set; } = "";
    internal static string LastHeard { get; private set; } = "";
    internal static string SpeechDiagnostic { get; private set; } = "Not started. Windows default input device.";
    internal static string ReplySource { get; private set; } = "Jarvis";
    internal static bool Busy => instance != null && instance.job != null;
    internal static bool Active => instance != null && instance.active;

    internal static void Initialize(GameObject host)
    {
        if (instance != null) return;
        instance = host.AddComponent<JarvisManager>();
        instance.speech = new JarvisSpeech(host);
        instance.enabled = false;
        GUIManager.Register("Jarvis", value => JarvisGUI.Visible = value, () => JarvisGUI.Visible, JarvisGUI.CreateGUI, () => JarvisGUI.Window);
        ButtonManager.OnNotification += instance.OnNotification;
    }
    internal static void SetActive(bool value)
    {
        if (instance == null) return;
        Cancel();
        instance.active = value;
        instance.enabled = value;
        instance.DisposeRecognizers();
        State = value ? "Ready" : "Disabled";
        if (value)
        {
            instance.inputFailed = false;
            instance.nextWakeRetry = 0f;
            instance.wakeErrorShown = false;
            instance.StartWake();
            instance.StartJob(instance.Say("Jarvis is ready. Say " + JarvisSettings.WakeWords[JarvisSettings.WakeWord] + ", wait for the tone, then say your command.", true));
        }
    }
    internal static void ExecuteLocal(Action action)
    {
        if (instance == null) { action(); return; }
        instance.applyingCommand = true;
        try { action(); } finally { instance.applyingCommand = false; }
    }
    internal static void Restart()
    {
        if (instance?.applyingCommand == true) { instance.rebuildRecognition = true; return; }
        if (instance != null && instance.active) SetActive(true);
    }
    internal static void Cancel()
    {
        if (instance == null) return;
        if (instance.applyingCommand) return;
        instance.ai.Cancel();
        instance.speech?.Stop();
        if (instance.job != null) instance.StopCoroutine(instance.job);
        instance.job = null;
        instance.StopDictation();
        while (instance.events.TryDequeue(out _)) { }
        State = instance.active ? "Ready" : "Disabled";
        instance.StartWake();
    }
    internal static void Submit(string input)
    {
        if (instance == null) return;
        if (!instance.active) { Display("Enable Jarvis first. You can then type or use your voice."); return; }
        if (Busy) { Display("Jarvis is busy. Use Stop Jarvis Response to cancel first."); return; }
        input = input.Trim();
        if (input.Length == 0) return;
        if (input.Length > 1000) { Display("Keep your request under one thousand characters."); return; }
        instance.StopWake();
        instance.StartJob(instance.Handle(input));
    }
    internal static void Listen(bool pushToTalk = false)
    {
        if (instance == null) return;
        if (!instance.active) { Display("Enable Jarvis before starting voice input."); return; }
        if (!Busy) instance.StartJob(instance.Capture(pushToTalk));
    }
    internal static void ClearConversation()
    {
        if (instance == null || !instance.active) { Display("Enable Jarvis first."); return; }
        if (instance.applyingCommand) { instance.clearRequested = true; return; }
        Cancel();
        instance.StartJob(instance.ClearRoutine());
    }
    private void Update()
    {
        while (events.TryDequeue(out var item))
        {
            if (item.Kind == 3) { if (!Busy) Listen(); }
        }
        PollRecognition();
        if (!Busy && Time.unscaledTime >= nextWakeCheck)
        {
            nextWakeCheck = Time.unscaledTime + 1f;
            if (wake?.IsRunning != true) StartWake();
        }
        bool pressed = Keyboard.current?.f8Key.wasPressedThisFrame == true;
        talkHeld = Keyboard.current?.f8Key.isPressed == true;
        if (JarvisSettings.Activation == 1 && !inputFailed)
        {
            bool now = false;
            try { now = ModInput.Held(JarvisSettings.Input); }
            catch (Exception) { inputFailed = true; Display("Controller voice input is unavailable. Use F8 or Ask Jarvis. Restart Jarvis to retry the controller."); }
            pressed |= now && !held; held = now; talkHeld |= now;
        }
        if (pressed && !Busy) Listen(JarvisSettings.Activation == 1);
    }
    private void StartJob(IEnumerator routine)
    {
        if (job != null) return;
        job = StartCoroutine(Run(routine));
    }
    private IEnumerator Run(IEnumerator routine)
    {
        // Defer once so a synchronous coroutine cannot finish before its handle is assigned.
        yield return null;
        try { yield return routine; }
        finally
        {
            job = null;
            if (rebuildRecognition) { DisposeRecognizers(); rebuildRecognition = false; }
            if (active) { State = "Ready"; StartWake(); }
            if (active && clearRequested) { clearRequested = false; StartJob(ClearRoutine()); }
        }
    }
    private IEnumerator Handle(string input)
    {
        LastInput = input;
        string cleaned = JarvisCommands.CleanInput(input);
        if (cleaned.Equals("cancel", StringComparison.OrdinalIgnoreCase) || cleaned.Equals("never mind", StringComparison.OrdinalIgnoreCase)) { yield return Say("Cancelled."); yield break; }
        if (JarvisCommands.TryParse(cleaned, out var command, out string problem))
        {
            ReplySource = "Local command";
            if (problem.Length == 0)
            {
                try { JarvisCommands.Execute(command, out problem); }
                catch (Exception ex) { Debug.LogWarning("[ChudOS Jarvis] Local command failed: " + ex.GetType().Name); problem = "That command could not be completed."; }
            }
            // Voice settings can cancel this coroutine while a command is executing.
            if (job != null && active) yield return Say(problem);
            yield break;
        }
        if (JarvisSettings.Provider == 0) { ReplySource = "Local assistant"; yield return Say("Try enable platforms, open movement, or set menu theme to OG Blue. Choose GPT Jarvis or ChudGPT Jarvis for conversation."); yield break; }
        State = "Thinking";
        ReplySource = JarvisSettings.Provider == 1 ? "GPT service" : "ChudGPT";
        string reply = "";
        yield return ai.Ask(cleaned, value => reply = value);
        if (active && job != null) yield return Say(reply);
    }
    private IEnumerator Say(string text, bool showAlways = false)
    {
        StopWake();
        Display(text, showAlways);
        State = "Speaking";
        if (speech != null) yield return speech.Speak(text);
        if (speech?.Error != null) Display(speech.Error);
    }
    private IEnumerator ClearRoutine()
    {
        StopWake(); State = "Clearing conversation";
        string result = "";
        yield return ai.Clear(value => result = value);
        yield return Say(result);
    }
    private IEnumerator Capture(bool pushToTalk)
    {
        StopWake();
        StopDictation();
        ReplySource = "Speech recognition";
        LastHeard = "";
        bool local = JarvisSettings.SpeechInput == 1 || JarvisSettings.SpeechInput == 0 && JarvisSettings.Provider == 0;
        SpeechDiagnostic = local ? "Preparing local command recognition." : "Switching from wake word to dictation.";
        State = "Preparing microphone";
        if (Application.platform != RuntimePlatform.WindowsPlayer) { yield return Say("Voice recognition needs Windows. Use the Jarvis text window instead."); yield break; }
        float deadline;
        if (!local)
        {
            if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running) PhraseRecognitionSystem.Shutdown();
            deadline = Time.realtimeSinceStartup + 5f;
            while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped && Time.realtimeSinceStartup < deadline) yield return null;
            if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped) { yield return Say("Speech recognition is busy. Use the Jarvis text window."); yield break; }
        }
        yield return Say("Yes? Wait for the tone.");
        yield return new WaitForSecondsRealtime(0.15f);
        State = "Starting microphone";
        IJarvisCapture? capture = null;
        string startError = "";
        try
        {
            capture = local ? new JarvisLocalRecognition() : new JarvisRecognition(JarvisSettings.ListenSeconds, JarvisSettings.SilenceSeconds);
            recognition = capture;
            capture.Start();
        }
        catch (Exception ex) { startError = ex.GetType().Name + " (0x" + unchecked((uint)ex.HResult).ToString("X8") + ")"; }
        if (startError.Length > 0 || capture == null)
        {
            SpeechDiagnostic = "Speech input failed to start: " + startError;
            StopDictation();
            yield return Say(local ? "Local command recognition could not start. Check Windows microphone access or use typed input." : "Windows dictation could not start. Check Online Speech Recognition or choose Local Commands under Jarvis Speech Input.");
            yield break;
        }
        string text = "", failure = "";
        bool silentTimeout = false, started = false;
        try
        {
            // Count the listening window only after Windows reports that the recognizer is running.
            deadline = Time.realtimeSinceStartup + 5f;
            while (capture.Status == SpeechSystemStatus.Stopped && capture.Error.Length == 0 && capture.Completion == null && Time.realtimeSinceStartup < deadline)
            {
                PollRecognition();
                yield return null;
            }
            if (capture.Status == SpeechSystemStatus.Running)
            {
                started = true;
                State = "Listening — speak now";
                Display("Listening — speak now.", true);
                speech?.SignalListening();
                deadline = Time.realtimeSinceStartup + JarvisSettings.ListenSeconds;
                while (capture.FinalText.Length == 0 && capture.Error.Length == 0 && capture.Completion == null && Time.realtimeSinceStartup < deadline && (!pushToTalk || talkHeld))
                {
                    PollRecognition();
                    yield return null;
                }
            }
            bool startTimedOut = !started && capture.Error.Length == 0 && capture.Completion == null;
            bool windowExpired = started && Time.realtimeSinceStartup >= deadline && capture.Completion == null;
            capture.Stop();
            State = "Finishing transcription";
            // Complete can arrive before Result. Keep accepting queued final text during this grace period.
            deadline = Time.realtimeSinceStartup + 2f;
            do
            {
                PollRecognition();
                if ((capture.FinalText.Length > 0 || capture.Error.Length > 0) && capture.Status != SpeechSystemStatus.Running) break;
                yield return null;
            }
            while (Time.realtimeSinceStartup < deadline);
            PollRecognition();
            silentTimeout = capture.SilenceTimeout || windowExpired && capture.FinalText.Length == 0 && capture.PartialText.Length == 0 && capture.Error.Length == 0 && capture.Completion is null or DictationCompletionCause.Canceled;
            if (capture.FinalText.Length > 1000) failure = "That request was too long. Please keep it short.";
            else if (capture.Accepts(JarvisSettings.Confidence)) text = capture.FinalText;
            else if (capture.FinalText.Length > 0)
            {
                failure = "Windows transcribed your words with " + capture.Confidence + " confidence. Check Heard in the Jarvis window. Repeat more clearly or change Jarvis Recognition in Settings.";
            }
            else if (startTimedOut && capture.Error.Length == 0)
            {
                SpeechDiagnostic = capture.Backend + " never reached Running: " + capture.Status;
                failure = "Speech input did not become ready. Check Windows speech settings or use typed input.";
            }
            else failure = capture.Failure();
            Debug.Log("[ChudOS Jarvis] " + capture.Backend + ": " + (capture.Completion?.ToString() ?? "stopped") + "; final result: " + (capture.FinalText.Length > 0) + "; confidence: " + capture.Confidence);
        }
        finally
        {
            capture.Dispose();
            if (recognition == capture) recognition = null;
        }
        if (text.Length > 0) yield return Handle(text);
        else if (silentTimeout) Display("No command heard. Say " + JarvisSettings.WakeWords[JarvisSettings.WakeWord] + ", wait for the tone, then speak.");
        else yield return Say(failure);
    }
    private void StartWake()
    {
        if (!active || job != null || Time.unscaledTime < nextWakeRetry || Application.platform != RuntimePlatform.WindowsPlayer) return;
        try
        {
            if (wake == null) { wake = new KeywordRecognizer(new[] { JarvisSettings.WakeWords[JarvisSettings.WakeWord] }); wake.OnPhraseRecognized += OnWake; }
            if (!wake.IsRunning) wake.Start();
            wakeErrorShown = false;
        }
        catch (Exception)
        {
            State = "Wake word unavailable; use F8 or typed input";
            nextWakeRetry = Time.unscaledTime + 3f;
            if (!wakeErrorShown) Display("Wake-word recognition couldn't start. Check Windows microphone and speech settings. F8 and typed input are still available.", true);
            wakeErrorShown = true;
        }
    }
    private void StopWake() { if (wake?.IsRunning == true) wake.Stop(); }
    private void PollRecognition()
    {
        if (recognition == null) return;
        recognition.Poll();
        SpeechSystemStatus status = recognition.Status;
        if (observedRecognition == recognition && observedRevision == recognition.Revision && observedStatus == status) return;
        observedRecognition = recognition;
        observedRevision = recognition.Revision;
        observedStatus = status;
        LastHeard = recognition.FinalText.Length > 0 ? recognition.FinalText : recognition.PartialText;
        SpeechDiagnostic = recognition.Backend + ": " + (recognition.Error.Length > 0 ? recognition.Error : status + "; " + (recognition.Completion?.ToString() ?? "capturing") + (recognition.FinalText.Length > 0 ? "; confidence: " + recognition.Confidence : ""));
    }
    private void StopDictation() { recognition?.Dispose(); recognition = null; }
    private void OnWake(PhraseRecognizedEventArgs args) => events.Enqueue((3, args.text));
    private void OnNotification(string text)
    {
        if (!active || !JarvisSettings.Dictate || Busy || Time.unscaledTime < nextDictate) return;
        nextDictate = Time.unscaledTime + 3f;
        StartJob(Say(text));
    }
    private static void Display(string text, bool showAlways = false)
    {
        LastReply = text.Length > 8000 ? text.Substring(0, 8000) : text;
        if (showAlways || JarvisSettings.Notifications) NotifyLib.SendNotificationALL("Jarvis: " + (LastReply.Length > 220 ? LastReply.Substring(0, 220) + "..." : LastReply), 5500);
    }
    private void DisposeRecognizers()
    {
        StopWake(); StopDictation();
        if (wake != null) { wake.OnPhraseRecognized -= OnWake; wake.Dispose(); wake = null; }
    }
    private void OnDestroy()
    {
        if (instance != this) return;
        active = false;
        Cancel(); DisposeRecognizers(); ai.Dispose(); speech?.Dispose();
        ButtonManager.OnNotification -= OnNotification;
        GUIManager.Unregister("Jarvis"); JarvisGUI.Visible = false;
        instance = null; State = "Disabled";
    }
}
