// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Holdables;

internal sealed class LocalHoldables : ExpansionWindowFeature
{
    private static readonly string[] Tools = { "Handheld Camera", "Squeeze Toy", "Custom Flashlight", "Measuring Tape" };
    private GameObject? item;
    private Renderer? renderer;
    private Camera? camera;
    private RenderTexture? preview;
    private Light? light;
    private LineRenderer? tape;
    private TextMesh? distance;
    private Vector3 first, second;
    private float nextRender;
    private bool measuring, finished, triggerHeld, buttonHeld, squeezeHeld;
    private int appearance = -1;
    private Transform Hand => ExpansionPreferences.LeftHand ? CreativeTools.Left : CreativeTools.Right;
    private Transform OtherHand => ExpansionPreferences.LeftHand ? CreativeTools.Right : CreativeTools.Left;
    private bool Grip => ExpansionPreferences.LeftHand ? LG : RG;
    private bool Trigger => (ExpansionPreferences.LeftHand ? InputLib.LeftTrigger : InputLib.RightTrigger) || Mouse.current?.leftButton.isPressed == true;
    private static void Select(string name)
    {
        foreach (string other in Tools) if (other != name) ButtonManager.SetEnabled(other, false);
        Enable<LocalHoldables>(name);
    }
    [MenuButton("Handheld Camera", ButtonType.Toggle, Category = "Holdables", Order = 10, runOption = RunOption.Once, Controls = "(RT)/(LMB)", Description = "Aim the handheld camera; trigger saves its local view.")]
    public static void EnableCamera() => Select("Handheld Camera");
    [MenuDisable("Handheld Camera")]
    public static void DisableCamera() => Disable("Handheld Camera");
    [MenuButton("Squeeze Toy", ButtonType.Toggle, Category = "Holdables", Order = 11, runOption = RunOption.Once, Controls = "(RG)/(RMB)")]
    public static void EnableToy() => Select("Squeeze Toy");
    [MenuDisable("Squeeze Toy")]
    public static void DisableToy() => Disable("Squeeze Toy");
    [MenuButton("Custom Flashlight", ButtonType.Toggle, Category = "Holdables", Order = 12, runOption = RunOption.Once, Controls = "(A)/(Space)", Description = "Aim with the selected hand. A or Space toggles the light.")]
    public static void EnableLight() => Select("Custom Flashlight");
    [MenuDisable("Custom Flashlight")]
    public static void DisableLight() => Disable("Custom Flashlight");
    [MenuButton("Measuring Tape", ButtonType.Toggle, Category = "Holdables", Order = 13, runOption = RunOption.Once, Controls = "(RT)/(LMB)(X)", Description = "Trigger starts/finishes measuring to the other hand; X or F resets.")]
    public static void EnableTape() => Select("Measuring Tape");
    [MenuDisable("Measuring Tape")]
    public static void DisableTape() => Disable("Measuring Tape");

    protected override void Setup()
    {
        item = Shape(Mode, Mode == "Squeeze Toy" ? PrimitiveType.Sphere : PrimitiveType.Cube, Hand.position, Vector3.one * 0.12f);
        renderer = item.GetComponent<Renderer>();
        if (Mode == "Custom Flashlight") { light = item.AddComponent<Light>(); light.type = LightType.Spot; light.spotAngle = 55f; light.shadows = LightShadows.None; }
        if (Mode == "Handheld Camera")
        {
            var obj = new GameObject("ChudOS Handheld Camera Lens"); obj.transform.SetParent(item.transform, false);
            camera = obj.AddComponent<Camera>(); camera.enabled = false; camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.fieldOfView = 65f; camera.nearClipPlane = 0.05f; camera.farClipPlane = 100f; camera.cullingMask = ~(1 << 2);
            preview = new RenderTexture(256, 144, 16); preview.Create(); camera.targetTexture = preview;
            OpenWindow(870200, "Handheld Camera");
        }
        if (Mode == "Measuring Tape") { tape = Line("Measuring Tape"); tape.enabled = false; distance = Label("Trigger: start measuring", Hand.position); }
    }
    protected override void Tick()
    {
        if (item == null) return;
        item.transform.SetPositionAndRotation(Hand.TransformPoint(ExpansionPreferences.HoldableOffset), Hand.rotation * Quaternion.Euler(ExpansionPreferences.HoldableRotation));
        Vector3 shape = Mode == "Squeeze Toy" ? Vector3.one * 0.12f : Mode == "Custom Flashlight" ? new Vector3(0.06f, 0.06f, 0.18f) : new Vector3(0.13f, 0.09f, 0.07f);
        if (Mode == "Squeeze Toy")
        {
            if (Grip && HoldableFeedback.AllowInput) shape = new Vector3(0.15f, 0.06f, 0.15f);
            if (CreativeTools.Pressed(Grip && HoldableFeedback.AllowInput, ref squeezeHeld)) HoldableFeedback.Pulse(ExpansionPreferences.LeftHand);
        }
        item.transform.localScale = Vector3.Lerp(item.transform.localScale, shape * ExpansionPreferences.HoldableSize, 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime));
        if (appearance != ExpansionPreferences.Revision)
        {
            appearance = ExpansionPreferences.Revision;
            if (renderer != null) CreativeTools.Paint(renderer, ExpansionPreferences.HoldableColor);
            if (light != null) { light.range = ExpansionPreferences.FlashlightRange; light.intensity = ExpansionPreferences.FlashlightBrightness; light.color = ExpansionPreferences.HoldableColor; }
        }
        if (light != null && CreativeTools.Pressed(A, ref buttonHeld) && HoldableFeedback.AllowInput) { light.enabled = !light.enabled; HoldableFeedback.Pulse(ExpansionPreferences.LeftHand); }
        bool pressed = CreativeTools.Pressed(Trigger, ref triggerHeld);
        if (camera != null)
        {
            if (Time.unscaledTime >= nextRender) { nextRender = Time.unscaledTime + 0.1f; camera.Render(); }
            if (pressed && HoldableFeedback.AllowInput) SavePhoto();
        }
        if (tape == null || distance == null) return;
        if (CreativeTools.Pressed(X, ref buttonHeld) && HoldableFeedback.AllowInput) { measuring = finished = false; tape.enabled = false; distance.text = "Trigger: start measuring"; }
        if (pressed && HoldableFeedback.AllowInput)
        {
            if (!measuring) { first = Hand.position; measuring = true; finished = false; }
            else if (!finished) finished = true;
            else { first = Hand.position; finished = false; }
            HoldableFeedback.Pulse(ExpansionPreferences.LeftHand);
        }
        if (measuring)
        {
            if (!finished) second = OtherHand.position;
            tape.enabled = true; tape.SetPosition(0, first); tape.SetPosition(1, second);
            distance.transform.position = (first + second) * 0.5f + Vector3.up * 0.05f;
            if (Time.unscaledTime >= nextAction) { nextAction = Time.unscaledTime + 0.1f; distance.text = Vector3.Distance(first, second).ToString("F2") + " m"; }
        }
        else distance.transform.position = Hand.position + Vector3.up * 0.1f;
        if (View != null) distance.transform.rotation = Quaternion.LookRotation(distance.transform.position - View.transform.position);
    }
    private void SavePhoto()
    {
        if (camera == null || preview == null) return;
        Texture2D? image = null;
        RenderTexture previous = RenderTexture.active;
        try
        {
            camera.Render(); RenderTexture.active = preview;
            image = new Texture2D(preview.width, preview.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, preview.width, preview.height), 0, 0); image.Apply();
            string folder = Path.Combine(FileManager.RootFolder, "Photos"); Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder, "Handheld-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".png"), image.EncodeToPNG());
            CreativeTools.Notice("Handheld photo saved."); HoldableFeedback.Pulse(ExpansionPreferences.LeftHand);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { CreativeTools.Notice("The photo could not be saved."); }
        finally { RenderTexture.active = previous; if (image != null) Object.Destroy(image); }
    }
    protected override void Window(int id)
    {
        if (preview != null) GUILayout.Label(preview, GUILayout.Width(384), GUILayout.Height(216));
        if (MenuHelper.Button("Save Photo")) SavePhoto();
        GUI.DragWindow();
    }
    protected override void Cleanup()
    {
        if (camera != null) { camera.enabled = false; camera.targetTexture = null; }
        if (preview != null) { preview.Release(); Object.Destroy(preview); }
        base.Cleanup();
    }
}
