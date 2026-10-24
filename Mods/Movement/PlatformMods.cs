// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class PlatformMods
{
    private static readonly string[] Styles = { "Platforms", "Invisible Platforms", "Sticky Platforms", "Plank Platforms", "Custom Platforms" };
    private static readonly GameObject?[] hands = new GameObject?[2];
    private static readonly Queue<GameObject> placed = new();
    private static Material? material;
    private static Vector3 customScale = new(0.5f, 0.05f, 0.5f);
    private static bool customVisible = true, flat, gunPressed;
    private static int colorIndex;
    private static float nextPlacement;

    internal static void Activate(string name)
    {
        foreach (string other in Styles) if (other != name) ButtonManager.SetEnabled(other, false);
        ClearHands();
        PortTools.Register(Cleanup);
    }

    internal static void Tick(int mode)
    {
        if (!PortTools.Ready) { ClearHands(); return; }
        PortTools.Register(Cleanup);
        Vector3 correction = Vector3.zero;
        int attached = 0;
        for (int i = 0; i < 2; i++)
        {
            bool held = i == 0 ? InputLib.LeftGrab : InputLib.RightGrab;
            var hand = i == 0 ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
            if (!held || hand == null) { Remove(hands[i]); hands[i] = null; continue; }
            if (hands[i] == null)
            {
                Vector3 scale = mode == 3 ? new Vector3(0.25f, 0.06f, 1.2f) : mode == 4 ? customScale : new Vector3(0.25f, 0.3f, 0.25f);
                Quaternion rotation = mode == 4 && flat ? Quaternion.identity : hand.rotation;
                hands[i] = Create(hand.position, rotation, scale, mode != 1 && (mode != 4 || customVisible));
            }
            if (mode == 2) { correction += hands[i]!.transform.position - hand.position; attached++; }
        }
        if (attached > 0)
        {
            GTPlayer.Instance.transform.position += Vector3.ClampMagnitude(correction / attached, 0.2f);
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
        }
    }

    private static GameObject Create(Vector3 position, Quaternion rotation, Vector3 scale, bool visible)
    {
        if (material == null)
        {
            var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Platform shader unavailable.");
            material = new Material(shader);
        }
        material.color = ColorValue();
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "ChudOS Platform";
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.localScale = scale;
        var renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.enabled = visible;
        return obj;
    }

    private static Color ColorValue() => colorIndex switch
    {
        1 => GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null ? Color.white : GorillaTagger.Instance.offlineVRRig.playerColor,
        2 => Color.cyan,
        3 => Color.white,
        _ => ChudOS.Core.Main.PrimaryColorValue
    };

    private static void Remove(GameObject? obj) { if (obj != null) { obj.SetActive(false); Object.Destroy(obj); } }
    public static void ClearHands() { for (int i = 0; i < hands.Length; i++) { Remove(hands[i]); hands[i] = null; } }

    [MenuButton("Invisible Platforms", ButtonType.Toggle, Category = "Movement", Order = 140, runOption = RunOption.FixedUpdate, Description = "Hold either grip to place an invisible solid platform")]
    public static void Invisible() => Tick(1);
    [MenuEnable("Invisible Platforms")]
    public static void StartInvisible() => Activate("Invisible Platforms");
    [MenuDisable("Invisible Platforms")]
    public static void StopInvisible() => ClearHands();

    [MenuButton("Sticky Platforms", ButtonType.Toggle, Category = "Movement", Order = 141, runOption = RunOption.FixedUpdate, Description = "Hold a grip to anchor that hand; move your body around the anchor")]
    public static void Sticky() => Tick(2);
    [MenuEnable("Sticky Platforms")]
    public static void StartSticky() => Activate("Sticky Platforms");
    [MenuDisable("Sticky Platforms")]
    public static void StopSticky() => ClearHands();

    [MenuButton("Plank Platforms", ButtonType.Toggle, Category = "Movement", Order = 142, runOption = RunOption.FixedUpdate, Description = "Hold either grip to place a long, thin plank")]
    public static void Plank() => Tick(3);
    [MenuEnable("Plank Platforms")]
    public static void StartPlank() => Activate("Plank Platforms");
    [MenuDisable("Plank Platforms")]
    public static void StopPlank() => ClearHands();

    [MenuButton("Custom Platforms", ButtonType.Toggle, Category = "Movement", Order = 143, runOption = RunOption.FixedUpdate, Description = "Grip platforms using your custom scale, rotation, color and visibility")]
    public static void Custom() => Tick(4);
    [MenuEnable("Custom Platforms")]
    public static void StartCustom() => Activate("Custom Platforms");
    [MenuDisable("Custom Platforms")]
    public static void StopCustom() => ClearHands();

    [MenuButton("Platform Gun", ButtonType.Toggle, Category = "Movement", Order = 144, runOption = RunOption.FixedUpdate, Description = "Aim with grip and press trigger to place a custom platform; PC: right mouse + left click")]
    public static void PlatformGun()
    {
        if (!PortTools.Ready) { StopGun(); return; }
        PortTools.Register(Cleanup);
        bool trigger = GunLib.SelectPressed;
        bool hit = GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, trigger, out Vector3 point);
        if (hit && !gunPressed && Time.unscaledTime >= nextPlacement)
        {
            nextPlacement = Time.unscaledTime + 0.25f;
            while (placed.Count > 0 && placed.Peek() == null) placed.Dequeue();
            while (placed.Count >= 20) Remove(placed.Dequeue());
            Vector3 normal = GunLib.LastHit.normal;
            Quaternion rotation = flat ? Quaternion.identity : Quaternion.FromToRotation(Vector3.up, normal);
            var obj = Create(point + normal * (customScale.y / 2f), rotation, customScale, customVisible);
            placed.Enqueue(obj);
            Object.Destroy(obj, 30f);
        }
        gunPressed = trigger;
    }
    [MenuEnable("Platform Gun")]
    public static void StartGun() { StopGun(); PortTools.ClaimPointer("Platform Gun"); }
    [MenuDisable("Platform Gun")]
    public static void StopGun() { while (placed.Count > 0) Remove(placed.Dequeue()); gunPressed = false; nextPlacement = 0f; GunLib.Hide(); }

    private static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;
    private static void Refresh() { ClearHands(); if (material != null) material.color = ColorValue(); }
    [MenuButton("Platform Width", ButtonType.Option, Category = "Settings", Order = 160, Options = new[] { "0.5", "0.25", "0.75", "1", "1.5", "2" }, Notify = false)]
    public static void SetWidth() { customScale.x = new[] { 0.5f, 0.25f, 0.75f, 1f, 1.5f, 2f }[Index("Platform Width")]; Refresh(); }
    [MenuButton("Platform Height", ButtonType.Option, Category = "Settings", Order = 161, Options = new[] { "0.05", "0.025", "0.1", "0.25", "0.5" }, Notify = false)]
    public static void SetHeight() { customScale.y = new[] { 0.05f, 0.025f, 0.1f, 0.25f, 0.5f }[Index("Platform Height")]; Refresh(); }
    [MenuButton("Platform Length", ButtonType.Option, Category = "Settings", Order = 162, Options = new[] { "0.5", "0.25", "0.75", "1", "1.5", "2" }, Notify = false)]
    public static void SetLength() { customScale.z = new[] { 0.5f, 0.25f, 0.75f, 1f, 1.5f, 2f }[Index("Platform Length")]; Refresh(); }
    [MenuButton("Platform Color", ButtonType.Option, Category = "Settings", Order = 163, Options = new[] { "Theme", "Player", "Cyan", "White" }, Notify = false)]
    public static void SetColor() { colorIndex = Index("Platform Color"); Refresh(); }
    [MenuButton("Platform Visibility", ButtonType.Option, Category = "Settings", Order = 164, Options = new[] { "Visible", "Invisible" }, Notify = false)]
    public static void SetVisibility() { customVisible = Index("Platform Visibility") == 0; Refresh(); }
    [MenuButton("Platform Rotation", ButtonType.Option, Category = "Settings", Order = 165, Options = new[] { "Hand / Surface", "Flat" }, Notify = false)]
    public static void SetRotation() { flat = Index("Platform Rotation") == 1; Refresh(); }

    internal static void Clear() { ClearHands(); StopGun(); }
    private static void Cleanup() { Clear(); if (material != null) Object.Destroy(material); material = null; }
}
