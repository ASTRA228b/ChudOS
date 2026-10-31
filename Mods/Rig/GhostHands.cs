// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Rig;

internal static class GhostHands
{
    private static readonly GameObject?[] Hands = new GameObject?[2];
    private static Material? material;
    private static bool enabled = true;
    private static Color color;
    [MenuButton("Ghost Hand Guides", ButtonType.Toggle, Category = "Settings", Order = 263, DefaultEnabled = true, runOption = RunOption.Once, Notify = false)]
    public static void Enable() => enabled = true;
    [MenuDisable("Ghost Hand Guides")]
    public static void Disable() { enabled = false; Hide(); }
    internal static void Tick(bool supported, bool rigDisabled)
    {
        if (!enabled || !supported || !rigDisabled || !RigRuntime.Ready) { Hide(); return; }
        Ensure();
        float scale = GTPlayer.Instance.scale;
        if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f) { Hide(); return; }
        if (color != VRRig.LocalRig.playerColor) { color = VRRig.LocalRig.playerColor; material!.color = color; }
        Follow(0, GTPlayer.Instance.LeftHand.controllerTransform, scale);
        Follow(1, GTPlayer.Instance.RightHand.controllerTransform, scale);
    }
    private static void Ensure()
    {
        if (material != null && Hands[0] != null && Hands[1] != null) return;
        Clear();
        var shader = Shader.Find("Unlit/Color") ?? Shader.Find("GorillaTag/UberShader");
        if (shader == null) throw new InvalidOperationException("Hand guide shader is unavailable.");
        color = VRRig.LocalRig.playerColor;
        material = new Material(shader) { color = color };
        PortTools.Register(Clear);
        for (int i = 0; i < Hands.Length; i++)
        {
            var hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hand.name = i == 0 ? "ChudOS Left Hand Guide" : "ChudOS Right Hand Guide";
            hand.layer = 2;
            Object.DontDestroyOnLoad(hand);
            var collider = hand.GetComponent<Collider>();
            collider.enabled = false;
            Object.Destroy(collider);
            hand.GetComponent<Renderer>().sharedMaterial = material;
            Hands[i] = hand;
        }
    }
    private static void Follow(int index, Transform controller, float scale)
    {
        var hand = Hands[index]!;
        if (!hand.activeSelf) hand.SetActive(true);
        hand.transform.SetPositionAndRotation(controller.position, controller.rotation);
        hand.transform.localScale = Vector3.one * (0.045f * scale);
    }
    internal static void Hide() { foreach (var hand in Hands) if (hand != null && hand.activeSelf) hand.SetActive(false); }
    internal static void Clear()
    {
        for (int i = 0; i < Hands.Length; i++) { if (Hands[i] != null) Object.Destroy(Hands[i]); Hands[i] = null; }
        if (material != null) Object.Destroy(material);
        material = null;
    }
}
