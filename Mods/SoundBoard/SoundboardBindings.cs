// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.SoundBoard;

internal sealed class SoundboardBindings : MonoBehaviour
{
    private static SoundboardBindings? instance;
    private static int binding;
    private bool previous, controllerFailed;
    private int lastBinding = -1;

    [MenuButton("Sound Bindings", ButtonType.Toggle, Category = "Soundboard", Order = 43, runOption = RunOption.Once, Controls = "(F9)", Description = "Use the selected controller input or F9 to play/stop the selected sound")]
    public static void Enable()
    {
        Disable();
        var obj = new GameObject("ChudOS Sound Bindings"); Object.DontDestroyOnLoad(obj);
        instance = obj.AddComponent<SoundboardBindings>();
    }
    [MenuDisable("Sound Bindings")]
    public static void Disable()
    {
        if (instance == null) return;
        instance.enabled = false; Object.Destroy(instance.gameObject); instance = null;
    }
    [MenuButton("Soundboard Bind Input", ButtonType.Option, Category = "Settings", Order = 850, Options = new[] { "RT", "LT", "RG", "LG", "A", "B", "X", "Y", "RJ", "LJ" }, Notify = false)]
    public static void SetBinding() => binding = ButtonManager.GetButton("Soundboard Bind Input")?.OptionIndex ?? 0;

    private void Update()
    {
        if (JarvisGUI.Editing || CustomThemeGUI.Editing) { lastBinding = -1; previous = false; return; }
        bool changed = lastBinding != binding;
        if (changed) { lastBinding = binding; controllerFailed = false; }
        bool held = false;
        if (!controllerFailed && UnityEngine.XR.XRSettings.isDeviceActive && ControllerInputPoller.instance != null)
        {
            try { held = ModInput.Held(binding); }
            catch (Exception ex)
            {
                controllerFailed = true;
                Debug.LogWarning("[ChudOS Sound Bindings] Controller input: " + ex.GetType().Name);
                NotifyLib.SendNotificationALL("Soundboard controller input is unavailable. F9 still works. Change the input or re-enable Sound Bindings to retry.", 5000);
            }
        }
        if (changed) previous = held;
        bool pressed = held && !previous; previous = held;
        if (Keyboard.current?.f9Key.wasPressedThisFrame == true || pressed) SoundboardManager.TogglePlay();
    }
    private void OnDestroy() { if (instance == this) instance = null; }
}
