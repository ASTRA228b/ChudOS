// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Diagnostics;
using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using DiscordRPC;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Main;

internal sealed class DiscordPresence : MonoBehaviour
{
    private const string ButtonName = "Discord Rich Presence";
    internal const string ApplicationId = "1564819192656437389";
    internal const string LogoUrl = "https://chudgpt-landing.vercel.app/chudgpt-logo.png";
    private static readonly string[] DiscordProcesses = { "Discord", "DiscordPTB", "DiscordCanary", "DiscordDevelopment" };
    private static DiscordPresence? instance;
    private DiscordRpcClient? client;
    private Harmony? artworkGuard;
    private Timestamps? timestamps;
    private bool ready, cleaned;
    private float nextPoll, nextProcessCheck, nextPresence, readyDeadline;
    private string failure = "", previousState = "";

    [MenuButton("Discord Rich Presence", ButtonType.Toggle, Category = "Main", Order = 30, runOption = RunOption.Once, Notify = false, Description = "Show ChudOS on Discord. Requires the Discord desktop app.")]
    public static void Enable()
    {
        Stop();
        if (!DiscordRunning()) { DisableWithNotice("Discord is not open. Discord Rich Presence was turned off."); return; }
        var obj = new GameObject("ChudOS Discord Presence");
        Object.DontDestroyOnLoad(obj);
        instance = obj.AddComponent<DiscordPresence>();
    }
    [MenuDisable("Discord Rich Presence")]
    public static void Stop()
    {
        var current = instance; instance = null;
        if (current == null) return;
        current.enabled = false; current.Cleanup(); Object.Destroy(current.gameObject);
    }
    private void Start()
    {
        try
        {
            artworkGuard = new Harmony("ChudOS.DiscordArtwork");
            artworkGuard.Patch(AccessTools.Method(typeof(DiscordRPC.Assets), "Merge"), prefix: new HarmonyMethod(typeof(DiscordArtworkGuard), nameof(DiscordArtworkGuard.Prefix)));
            timestamps = Timestamps.Now; readyDeadline = Time.unscaledTime + 10f; nextProcessCheck = Time.unscaledTime + 2f;
            client = new DiscordRpcClient(ApplicationId, autoEvents: false);
            client.OnReady += (_, _) => { ready = true; previousState = ""; nextPresence = 0f; };
            client.OnClose += (_, _) => { if (!cleaned) failure = "Discord disconnected. Rich Presence was turned off."; };
            client.OnError += (_, _) => { if (!cleaned) failure = "Discord rejected the presence connection. Rich Presence was turned off."; };
            if (!client.Initialize()) failure = "Discord Rich Presence could not start. It was turned off.";
        }
        catch (Exception ex) { UnityEngine.Debug.LogWarning("[ChudOS Discord] " + ex.GetType().Name); failure = "Discord Rich Presence could not start. It was turned off."; }
    }
    private void Update()
    {
        float now = Time.unscaledTime;
        if (now < nextPoll) return;
        nextPoll = now + 0.1f;
        if (now >= nextProcessCheck)
        {
            nextProcessCheck = now + 2f;
            if (!DiscordRunning()) { DisableWithNotice("Discord was closed. Discord Rich Presence was turned off."); return; }
        }
        try
        {
            client?.Invoke();
            if (failure.Length > 0) { DisableWithNotice(failure); return; }
            if (!ready)
            {
                if (now >= readyDeadline) DisableWithNotice("Discord did not finish connecting. Rich Presence was turned off.");
                return;
            }
            if (now < nextPresence) return;
            nextPresence = now + 15f;
            var room = PhotonNetwork.CurrentRoom;
            string state = room == null ? "Solo / offline" : (room.IsVisible ? "Public lobby" : "Private lobby") + " · " + room.PlayerCount + " players";
            if (state == previousState || client == null) return;
            client.SetPresence(BuildPresence(state, timestamps!));
            previousState = state;
        }
        catch (Exception ex) { UnityEngine.Debug.LogWarning("[ChudOS Discord] " + ex.GetType().Name); DisableWithNotice("Discord Rich Presence stopped after a connection error."); }
    }
    internal static RichPresence BuildPresence(string state, Timestamps started) => new()
    {
        Details = "ChudOS · Gorilla Tag · v" + Constantss.Version,
        State = state,
        Timestamps = started,
        Assets = new DiscordRPC.Assets { LargeImageKey = LogoUrl, LargeImageText = "ChudOS v" + Constantss.Version + " · Made by Astra" },
        Buttons = new[]
        {
            new DiscordRPC.Button { Label = "Download ChudOS", Url = "https://github.com/ASTRA228b/ChudOS/releases" },
            new DiscordRPC.Button { Label = "Source Code", Url = "https://github.com/ASTRA228b/ChudOS" }
        }
    };
    private static bool DiscordRunning()
    {
        foreach (string name in DiscordProcesses)
        {
            Process[] processes;
            try { processes = Process.GetProcessesByName(name); }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException) { continue; }
            bool found = processes.Length > 0;
            foreach (var process in processes) process.Dispose();
            if (found) return true;
        }
        return false;
    }
    private static void DisableWithNotice(string text)
    {
        NotifyLib.SendNotificationALL(text, 4500);
        ButtonManager.SetEnabled(ButtonName, false);
        Stop();
    }
    private void Cleanup()
    {
        if (cleaned) return;
        cleaned = true;
        var previous = client; client = null;
        try { previous?.ClearPresence(); }
        catch (Exception ex) { UnityEngine.Debug.LogWarning("[ChudOS Discord] Presence cleanup: " + ex.GetType().Name); }
        finally
        {
            previous?.Dispose();
            artworkGuard?.UnpatchSelf(); artworkGuard = null;
        }
    }
    private void OnDestroy() { Cleanup(); if (instance == this) instance = null; }
}

internal static class DiscordArtworkGuard
{
    private static readonly System.Reflection.FieldInfo Large = AccessTools.Field(typeof(DiscordRPC.Assets), "_largeimagekey");
    private static readonly System.Reflection.FieldInfo Small = AccessTools.Field(typeof(DiscordRPC.Assets), "_smallimagekey");

    internal static void Prefix(DiscordRPC.Assets other)
    {
        if (Large.GetValue(other) == null) Large.SetValue(other, "");
        if (Small.GetValue(other) == null) Small.SetValue(other, "");
    }
}
