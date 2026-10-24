// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Reflection;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.SoundBoard;
using ChudOS.Stuff;
using GorillaNetworking;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChudOS.Mods.Main;

public static class PCButtonClick
{
    private static readonly RaycastHit[] Hits = new RaycastHit[128];
    private static readonly Dictionary<Type, MethodInfo?> Handlers = new();
    private static GameObject? hand;
    private static Collider? handCollider;
    private static CursorLockMode oldLock;
    private static bool oldVisible, ownsCursor;
    private static float nextClick;

    [MenuButton("PC Button Click", ButtonType.Toggle, Category = "Main", Order = 3, Description = "Click the gorilla computer keyboard and in-game buttons with your mouse")]
    public static void Tick()
    {
        Mouse? mouse = Mouse.current;
        if (!Application.isFocused || mouse == null || mouse.rightButton.isPressed) return;
        if (!ownsCursor)
        {
            oldLock = Cursor.lockState;
            oldVisible = Cursor.visible;
            ownsCursor = true;
            PortTools.Register(Disable);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (!mouse.leftButton.wasPressedThisFrame || Time.unscaledTime < nextClick || GunLib.Enabled) return;
        Vector2 position = mouse.position.ReadValue();
        if (SoundboardGUI.Visible && SoundboardGUI.Window.Contains(new Vector2(position.x, Screen.height - position.y))) return;
        Camera? camera = null;
        foreach (Camera candidate in Camera.allCameras)
            if (candidate.isActiveAndEnabled && candidate.stereoTargetEye == StereoTargetEyeMask.None && candidate.targetTexture == null && (camera == null || candidate.depth > camera.depth)) camera = candidate;
        camera ??= Camera.main;
        if (camera == null) return;
        Ray ray = camera.ScreenPointToRay(position);
        int count = Physics.RaycastNonAlloc(ray, Hits, 100f, Physics.AllLayers, QueryTriggerInteraction.Collide);
        if (count == Hits.Length) return;
        Array.Sort(Hits, 0, count, HitOrder.Instance);
        for (int i = 0; i < count; i++)
        {
            Collider collider = Hits[i].collider;
            if (collider == null || collider == handCollider) continue;
            if (collider.GetComponentInParent<MenuButtonCollider>() != null) return;
            if (collider.GetComponentInParent<VRRig>() != null) continue;
            if (GTPlayer.Instance != null && collider.transform.IsChildOf(GTPlayer.Instance.transform)) continue;
            foreach (MonoBehaviour script in collider.GetComponentsInParent<MonoBehaviour>())
            {
                if (script == null || !script.isActiveAndEnabled) continue;
                Type type = script.GetType();
                if (type.Assembly != typeof(GorillaComputer).Assembly || (!type.Name.Contains("Button") && !type.Name.Contains("Pressable"))) continue;
                if (!Handlers.TryGetValue(type, out MethodInfo? method))
                {
                    for (Type? current = type; current != null && method == null; current = current.BaseType)
                        method = current.GetMethod("OnTriggerEnter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, null, new[] { typeof(Collider) }, null);
                    Handlers[type] = method;
                }
                if (method == null) continue;
                EnsureHand();
                hand!.transform.position = Hits[i].point;
                nextClick = Time.unscaledTime + 0.2f;
                try { method.Invoke(script, new object[] { handCollider! }); }
                catch (TargetInvocationException ex) { Debug.LogWarning($"[ChudOS] PC button: {ex.InnerException?.Message ?? ex.Message}"); }
                return;
            }
            if (!collider.isTrigger) return;
        }
    }

    private static void EnsureHand()
    {
        if (hand != null) return;
        hand = new GameObject("ChudOS PC Button Hand") { layer = 2 };
        UnityEngine.Object.DontDestroyOnLoad(hand);
        SphereCollider sphere = hand.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 0.005f;
        sphere.enabled = false;
        Rigidbody body = hand.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.detectCollisions = false;
        hand.AddComponent<GorillaTriggerColliderHandIndicator>();
        handCollider = sphere;
    }

    [MenuDisable("PC Button Click")]
    public static void Disable()
    {
        if (ownsCursor && Cursor.lockState == CursorLockMode.None) { Cursor.lockState = oldLock; Cursor.visible = oldVisible; }
        ownsCursor = false;
        if (hand != null) UnityEngine.Object.Destroy(hand);
        hand = null;
        handCollider = null;
    }

    private sealed class HitOrder : IComparer<RaycastHit>
    {
        internal static readonly HitOrder Instance = new();
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
