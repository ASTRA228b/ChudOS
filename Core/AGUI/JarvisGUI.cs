// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Mods.Settings;
using UnityEngine;

namespace ChudOS.Core.AGUI;

internal static class JarvisGUI
{
    internal static bool Visible;
    internal static Rect Window = new(260f, 90f, 520f, 650f);
    private static string input = "";
    private static Vector2 scroll;
    private static bool editing;
    internal static bool Editing => Visible && editing;
    private static GUIStyle? textStyle;
    internal static void CreateGUI()
    {
        if (Visible) Window = MenuHelper.MakeWindow(847215, Window, Contents, "ChudOS Jarvis — 0.1.5 Beta");
    }
    private static void Contents(int id)
    {
        MenuHelper.Label("Status: " + JarvisManager.State);
        MenuHelper.Label("Wake word: " + JarvisSettings.WakeWords[JarvisSettings.WakeWord] + " — wait for Listening, then speak your command.");
        MenuHelper.Label("Wake-word choices: Jarvis, Assistant, Friday, System (Settings > Jarvis Wake Word).");
        if (MenuHelper.Button(JarvisManager.Active ? "Disable Jarvis" : "Enable Jarvis")) ButtonManager.SetEnabled("Jarvis", !JarvisManager.Active);
        Option("Jarvis Provider");
        if (JarvisSettings.Provider == 2) Option("ChudGPT Model");
        Option("Jarvis Speech Input");
        MenuHelper.Label("Automatic: local commands for Voice Assistant; Windows dictation for AI conversation. Local Commands also works with AI providers; type free-form questions.");
        Option("Jarvis Recognition");
        Option("Jarvis Listening Time");
        Option("Jarvis Silence Time");
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Microphone Settings")) JarvisSettings.OpenMicrophone();
        if (MenuHelper.Button("Speech Settings")) JarvisSettings.OpenSpeech();
        GUILayout.EndHorizontal();
        if (JarvisSettings.Provider == 2) MenuHelper.Label("Experimental models. If unavailable, switch the provider to GPT Jarvis.");
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Listen (F8)")) JarvisManager.Listen();
        if (MenuHelper.Button("Stop")) JarvisManager.Cancel();
        if (MenuHelper.Button("Clear Conversation")) JarvisManager.ClearConversation();
        GUILayout.EndHorizontal();
        GUI.SetNextControlName("ChudOS.Jarvis.Input");
        input = GUILayout.TextField(input, 1000);
        editing = GUI.GetNameOfFocusedControl() == "ChudOS.Jarvis.Input";
        if (MenuHelper.Button("Send") && input.Trim().Length > 0) { JarvisManager.Submit(input); input = ""; }
        textStyle ??= new GUIStyle(GUI.skin.label) { wordWrap = true, richText = false };
        textStyle.normal.textColor = Main.TextColorValue;
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.MinHeight(120f));
        GUILayout.Label("Speech: " + JarvisManager.SpeechDiagnostic, textStyle);
        GUILayout.Label("Heard: " + JarvisManager.LastHeard, textStyle);
        GUILayout.Label("Reply source: " + JarvisManager.ReplySource, textStyle);
        GUILayout.Label("You: " + JarvisManager.LastInput, textStyle);
        GUILayout.Label("Jarvis: " + JarvisManager.LastReply, textStyle);
        GUILayout.EndScrollView();
        MenuHelper.Label("Try: enable platforms / set menu theme to OG Blue / open movement.");
        MenuHelper.Label("Uses the Windows default microphone. Wait for Listening — speak now. Replies play locally.");
        if (MenuHelper.Button("Close")) Visible = false;
        GUI.DragWindow();
    }
    private static void Option(string name)
    {
        var button = ButtonManager.GetButton(name);
        if (button == null) return;
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("<")) ButtonManager.ChangeOption(button, -1);
        MenuHelper.Label(name + ": " + button.Option);
        if (MenuHelper.Button(">")) ButtonManager.ChangeOption(button, 1);
        GUILayout.EndHorizontal();
    }
}
