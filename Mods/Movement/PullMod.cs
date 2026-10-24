// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using GorillaLocomotion;
using Newtonsoft.Json;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class PullMod
{
    private static bool lastLeft, lastRight, tracking;
    private static GTPlayer? owner;
    private static float releaseUntil, touchUntil;
    private static int hand, input, activation;
    private static float releaseWindow = 0.10f, touchLinger = 0.06f;

    [MenuButton("Pull Mod", ButtonType.Toggle, Category = "Movement", Order = 99, runOption = RunOption.FixedUpdate, Description = "Astra's PullMod Hub with selectable methods and activation")]
    public static void Tick()
    {
        if (!PortTools.Ready) { Stop(); return; }
        PortTools.Register(Stop);
        var player = GTPlayer.Instance;
        if (player.bodyCollider == null || !PullMethods.Finite(GorillaTagger.Instance.rigidbody.linearVelocity)) { Stop(); return; }
        bool left = player.IsHandTouching(true), right = player.IsHandTouching(false);
        if (!tracking || owner != player)
        {
            Stop();
            tracking = true;
            owner = player;
            lastLeft = left;
            lastRight = right;
        }
        bool leftInput = hand != 2 && MovementInput.Left(input);
        bool rightInput = hand != 1 && MovementInput.Right(input);
        float now = Time.fixedTime;
        if ((!left && lastLeft && leftInput) || (!right && lastRight && rightInput)) releaseUntil = now + releaseWindow;
        bool touching = (left && leftInput) || (right && rightInput);
        if (touching) touchUntil = now + touchLinger;
        lastLeft = left;
        lastRight = right;
        if (!leftInput && !rightInput) { releaseUntil = touchUntil = 0f; return; }
        bool active = activation == 2 || (activation == 0 ? now < releaseUntil : touching || now < touchUntil);
        if (active) PullMethods.Run();
    }

    [MenuDisable("Pull Mod")]
    public static void Stop()
    {
        owner = null;
        tracking = lastLeft = lastRight = false;
        releaseUntil = touchUntil = 0f;
    }

    private static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;

    [MenuButton("Pull Method", ButtonType.Option, Category = "Settings", Order = 110, Options = new[] { "PMV2", "PMV1", "Regular", "Gravity", "Surface", "Burst", "Inertia", "Elastic", "Directional", "Curve", "Air Pull", "Ground Pull" }, Notify = false)]
    public static void SetMethod() { PullMethods.Method = Index("Pull Method") switch { 0 => PullMethods.PullMethod.PMV2, 1 => PullMethods.PullMethod.PMV1, _ => (PullMethods.PullMethod)Index("Pull Method") }; }

    [MenuButton("Pull Hand", ButtonType.Option, Category = "Settings", Order = 111, Options = new[] { "Either", "Left", "Right" }, Notify = false)]
    public static void SetHand() { hand = Index("Pull Hand"); Stop(); }

    [MenuButton("Pull Input", ButtonType.Option, Category = "Settings", Order = 112, Options = new[] { "Grip", "Trigger", "Joystick Click", "A", "B", "X", "Y" }, Notify = false)]
    public static void SetInput() { input = Index("Pull Input"); Stop(); }

    [MenuButton("Pull Activation", ButtonType.Option, Category = "Settings", Order = 113, Options = new[] { "Release", "Touch", "Hold" }, Notify = false)]
    public static void SetActivation() { activation = Index("Pull Activation"); Stop(); }

    [MenuButton("Pull Power", ButtonType.Option, Category = "Settings", Order = 114, Options = new[] { "0.025", "0.010", "0.015", "0.020", "0.035", "0.050", "0.070" }, Notify = false)]
    public static void SetPower() { PullMethods.PullPower = new[] { 0.025f, 0.010f, 0.015f, 0.020f, 0.035f, 0.050f, 0.070f }[Index("Pull Power")]; }

    [MenuButton("Pull Uphill Power", ButtonType.Option, Category = "Settings", Order = 115, Options = new[] { "0.020", "0", "0.010", "0.025", "0.035", "0.050" }, Notify = false)]
    public static void SetUphill() { PullMethods.UpHillPower = new[] { 0.020f, 0f, 0.010f, 0.025f, 0.035f, 0.050f }[Index("Pull Uphill Power")]; }

    [MenuButton("Pull Momentum", ButtonType.Option, Category = "Settings", Order = 116, Options = new[] { "0.001", "0", "0.0005", "0.002", "0.005" }, Notify = false)]
    public static void SetMomentum() { PullMethods.Momentum = new[] { 0.001f, 0f, 0.0005f, 0.002f, 0.005f }[Index("Pull Momentum")]; }

    [MenuButton("Pull Maximum", ButtonType.Option, Category = "Settings", Order = 117, Options = new[] { "0.070", "0.025", "0.050", "0.100", "0.150" }, Notify = false)]
    public static void SetMaximum() { PullMethods.MaxPull = new[] { 0.070f, 0.025f, 0.050f, 0.100f, 0.150f }[Index("Pull Maximum")]; }

    [MenuButton("Pull Velocity Limit", ButtonType.Option, Category = "Settings", Order = 118, Options = new[] { "15", "5", "10", "20", "25", "30" }, Notify = false)]
    public static void SetVelocity() { PullMethods.MaxVelocity = new[] { 15f, 5f, 10f, 20f, 25f, 30f }[Index("Pull Velocity Limit")]; }

    [MenuButton("Pull Extra Power", ButtonType.Option, Category = "Settings", Order = 119, Options = new[] { "1", "0.5", "1.5", "2", "3" }, Notify = false)]
    public static void SetExtra() { PullMethods.ExtraPower = new[] { 1f, 0.5f, 1.5f, 2f, 3f }[Index("Pull Extra Power")]; }

    [MenuButton("Pull Clamp Velocity", ButtonType.Option, Category = "Settings", Order = 120, Options = new[] { "ON", "OFF" }, Notify = false)]
    public static void SetClamp() { PullMethods.ClampVelocity = Index("Pull Clamp Velocity") == 0; }

    [MenuButton("Pull Release Window", ButtonType.Option, Category = "Settings", Order = 121, Options = new[] { "0.10", "0.04", "0.06", "0.15", "0.20" }, Notify = false)]
    public static void SetRelease() { releaseWindow = new[] { 0.10f, 0.04f, 0.06f, 0.15f, 0.20f }[Index("Pull Release Window")]; }

    [MenuButton("Pull Touch Linger", ButtonType.Option, Category = "Settings", Order = 122, Options = new[] { "0.06", "0", "0.02", "0.04", "0.10" }, Notify = false)]
    public static void SetLinger() { touchLinger = new[] { 0.06f, 0f, 0.02f, 0.04f, 0.10f }[Index("Pull Touch Linger")]; }

    [MenuButton("Pull Preset Slot", ButtonType.Option, Category = "Settings", Order = 123, Options = new[] { "1", "2", "3" }, Notify = false)]
    public static void SelectSlot() { }

    private static readonly string[] Settings = { "Pull Method", "Pull Hand", "Pull Input", "Pull Activation", "Pull Power", "Pull Uphill Power", "Pull Momentum", "Pull Maximum", "Pull Velocity Limit", "Pull Extra Power", "Pull Clamp Velocity", "Pull Release Window", "Pull Touch Linger" };
    private static string PresetPath => Path.Combine(FileManager.RootFolder, "PullPresets", "slot-" + (Index("Pull Preset Slot") + 1) + ".json");

    [MenuButton("Save Pull Preset", ButtonType.Press, Category = "Settings", Order = 125, Notify = false)]
    public static void SavePreset()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PresetPath)!);
            var values = Settings.ToDictionary(name => name, Index);
            File.WriteAllText(PresetPath, JsonConvert.SerializeObject(values, Formatting.Indented));
            NotifyLib.SendNotificationALL("Pull preset saved.", 3000);
        }
        catch (Exception ex) { Debug.LogWarning($"[ChudOS] Pull preset save: {ex.Message}"); NotifyLib.SendNotificationALL("Could not save pull preset.", 3000); }
    }

    [MenuButton("Load Pull Preset", ButtonType.Press, Category = "Settings", Order = 126, Notify = false)]
    public static void LoadPreset()
    {
        try
        {
            if (!File.Exists(PresetPath)) { NotifyLib.SendNotificationALL("This pull preset slot is empty.", 3000); return; }
            var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(PresetPath));
            if (values == null) return;
            Stop();
            foreach (string name in Settings)
                if (values.TryGetValue(name, out int value)) ApplyOption(name, value);
            NotifyLib.SendNotificationALL("Pull preset loaded.", 3000);
        }
        catch (Exception ex) { Debug.LogWarning($"[ChudOS] Pull preset load: {ex.Message}"); NotifyLib.SendNotificationALL("Could not load pull preset.", 3000); }
    }

    [MenuButton("Reset Pull Settings", ButtonType.Press, Category = "Settings", Order = 127, Notify = false)]
    public static void ResetSettings()
    {
        Stop();
        foreach (string name in Settings) ApplyOption(name, 0);
    }

    private static void ApplyOption(string name, int index)
    {
        var button = ButtonManager.GetButton(name);
        if (button == null) return;
        index = Mathf.Clamp(index, 0, button.Attribute.Options.Length - 1);
        if (button.OptionIndex != index) ButtonManager.ChangeOption(button, index - button.OptionIndex);
        else button.Run();
    }
}
