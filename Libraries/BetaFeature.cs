// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Mods.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace ChudOS.Libraries;

internal abstract class BetaFeature : MonoBehaviour
{
    private static readonly Dictionary<string, BetaFeature> Active = new(StringComparer.Ordinal);
    private readonly CreativeTools.Session session = new();
    protected string Mode = "";
    protected bool primary, secondary;
    protected float nextAction;
    protected readonly List<GameObject> Objects = new();
    protected Camera? View;
    internal static void Enable<T>(string name) where T : BetaFeature
    {
        Disable(name);
        var obj = new GameObject("ChudOS " + name);
        var feature = obj.AddComponent<T>();
        feature.Mode = name;
        Active[name] = feature;
        PortTools.Register(ClearAll);
    }
    internal static void Disable(string name)
    {
        if (!Active.TryGetValue(name, out var feature)) return;
        Active.Remove(name);
        if (feature != null) { feature.enabled = false; feature.CleanupOnce(); Object.Destroy(feature.gameObject); }
    }
    private static void ClearAll()
    {
        foreach (string name in Active.Keys.ToArray()) { ButtonManager.SetEnabled(name, false); Disable(name); }
    }
    private bool built, cleaned;
    private void Update()
    {
        if (!CreativeTools.Ready) return;
        if (session.Changed() && built) { ButtonManager.SetEnabled(Mode, false); return; }
        try
        {
            if (!built) { built = true; View = Camera.main; Setup(); }
            if (!enabled) return;
            Tick();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ChudOS " + Mode + "] " + ex.GetType().Name + ": " + ex.Message);
            CreativeTools.Notice(Mode + " stopped after an error. See the log.");
            ButtonManager.SetEnabled(Mode, false);
        }
    }
    private void FixedUpdate()
    {
        if (!built || !enabled || !CreativeTools.Ready) return;
        try { PhysicsTick(); }
        catch (Exception ex) { Debug.LogWarning("[ChudOS " + Mode + "] " + ex.Message); ButtonManager.SetEnabled(Mode, false); }
    }
    protected virtual void Setup() { }
    protected virtual void Tick() { }
    protected virtual void PhysicsTick() { }
    protected virtual void Cleanup() { }
    private void CleanupOnce()
    {
        if (cleaned) return;
        cleaned = true;
        try { if (built) Cleanup(); }
        finally { foreach (var obj in Objects) if (obj != null) Object.Destroy(obj); Objects.Clear(); }
    }
    private void OnDestroy() { CleanupOnce(); if (Active.TryGetValue(Mode, out var current) && current == this) Active.Remove(Mode); }
    protected GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 size, bool solid = false, bool dynamic = false)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = "ChudOS " + name;
        obj.transform.SetParent(transform, true);
        obj.transform.position = position;
        obj.transform.localScale = size;
        obj.layer = solid ? 0 : 2;
        var collider = obj.GetComponent<Collider>();
        collider.enabled = solid;
        if (!solid) Object.Destroy(collider);
        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = CreativeTools.Material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        CreativeTools.Paint(renderer, CreativeTools.Theme);
        if (dynamic)
        {
            var body = obj.AddComponent<Rigidbody>();
            body.mass = 0.2f; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }
        Objects.Add(obj);
        return obj;
    }
    protected LineRenderer Line(string name, int count = 2, bool loop = false)
    {
        var line = CreativeTools.Line(name, count, 0.015f, loop);
        line.transform.SetParent(transform, true); Objects.Add(line.gameObject);
        return line;
    }
    protected TextMesh Label(string text, Vector3 position, float size = 0.035f)
    {
        var obj = new GameObject("ChudOS Label"); obj.transform.SetParent(transform, true); obj.transform.position = position;
        var label = obj.AddComponent<TextMesh>(); label.text = text; label.fontSize = 48; label.characterSize = size; label.anchor = TextAnchor.MiddleCenter;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); label.color = CreativeTools.Theme;
        obj.GetComponent<Renderer>().sharedMaterial = label.font.material;
        Objects.Add(obj); return label;
    }
    protected bool Place(out Vector3 point, out Vector3 normal)
    {
        normal = Vector3.up;
        bool hit = GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, out point);
        bool edge = CreativeTools.Pressed(hit, ref primary);
        if (hit) normal = GunLib.LastHit.normal;
        return edge;
    }
    protected static bool A => InputLib.RightControllerAButton || Keyboard.current?.spaceKey.isPressed == true;
    protected static bool B => InputLib.RightControllerBButton || Keyboard.current?.rKey.isPressed == true;
    protected static bool X => InputLib.LeftControllerXButton || Keyboard.current?.fKey.isPressed == true;
    protected static bool LG => InputLib.LeftGrab || Keyboard.current?.leftShiftKey.isPressed == true;
    protected static bool RG => InputLib.RightGrab || Mouse.current?.rightButton.isPressed == true;
    protected static Vector3 Forward
    {
        get { Vector3 v = CreativeTools.Head.forward; v.y = 0f; return v.sqrMagnitude > 0.001f ? v.normalized : Vector3.forward; }
    }
    protected Vector3 Floor(float distance = 2f)
    {
        Vector3 p = CreativeTools.Position + Forward * distance;
        return Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out var hit, 10f, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore) ? hit.point : p - Vector3.up;
    }
    protected void Unavailable(string reason) { CreativeTools.Notice(Mode + ": " + reason); ButtonManager.SetEnabled(Mode, false); }
}
