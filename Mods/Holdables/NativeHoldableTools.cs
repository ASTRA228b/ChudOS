// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Holdables;

internal sealed class NativeHoldableTools : BetaFeature
{
    private sealed class Item
    {
        internal TransferrableObject Object = null!;
        internal TransferrableObject.PositionState Hand;
        internal Rigidbody? Proxy;
        internal bool WasHeld, Suppressed;
        internal Vector3 PreviousPosition, Velocity;
    }
    private readonly List<Item> items = new();
    private float nextScan;
    private bool dropHeld;

    [MenuButton("Sticky Holdables", ButtonType.Toggle, Category = "Holdables", Order = 2, runOption = RunOption.Once, Controls = "(B)/(R)", Description = "Keep your grabbed items held; B explicitly releases them.")]
    public static void EnableSticky() => Enable<NativeHoldableTools>("Sticky Holdables");
    [MenuDisable("Sticky Holdables")]
    public static void DisableSticky() => Disable("Sticky Holdables");
    [MenuButton("Physical Holdables", ButtonType.Toggle, Category = "Holdables", Order = 3, runOption = RunOption.Once, Description = "Local physics proxies for supported dropped items owned by you.")]
    public static void EnablePhysical() => Enable<NativeHoldableTools>("Physical Holdables");
    [MenuDisable("Physical Holdables")]
    public static void DisablePhysical() => Disable("Physical Holdables");
    private static bool Held(TransferrableObject item) => item.currentState is TransferrableObject.PositionState.InLeftHand or TransferrableObject.PositionState.InRightHand;
    private void Scan()
    {
        nextScan = Time.unscaledTime + 1f;
        for (int i = items.Count - 1; i >= 0; i--)
            if (items[i].Object == null || !items[i].Object.IsMyItem())
            {
                if (items[i].Proxy != null) { Objects.Remove(items[i].Proxy!.gameObject); Object.Destroy(items[i].Proxy!.gameObject); }
                items.RemoveAt(i);
            }
        foreach (var item in Object.FindObjectsByType<TransferrableObject>(FindObjectsSortMode.None))
        {
            if (items.Count >= 16) break;
            if (!item.IsMyItem() || items.Any(record => record.Object == item)) continue;
            items.Add(new Item { Object = item, PreviousPosition = item.transform.position });
        }
    }
    private static void Release(Item item)
    {
        var equipment = EquipmentInteractor.instance;
        if (equipment == null) return;
        GameObject hand = item.Hand == TransferrableObject.PositionState.InLeftHand ? equipment.leftHand : equipment.rightHand;
        if (hand != null) item.Object.OnRelease(null!, hand);
    }
    protected override void Tick()
    {
        if (Time.unscaledTime >= nextScan) Scan();
        bool drop = CreativeTools.Pressed(B, ref dropHeld) && HoldableFeedback.AllowInput;
        foreach (var item in items)
        {
            if (item.Object == null || !item.Object.IsMyItem()) { if (item.Proxy != null) item.Proxy.gameObject.SetActive(false); continue; }
            bool held = Held(item.Object);
            if (Mode == "Sticky Holdables")
            {
                if (drop && item.WasHeld) { Release(item); item.Suppressed = true; item.WasHeld = false; }
                else if (held) { if (!item.Suppressed) { item.Hand = item.Object.currentState; item.WasHeld = true; } }
                else if (item.WasHeld && !item.Suppressed) item.Object.currentState = item.Hand;
                if (item.Suppressed && (InputLib.LeftGrab || InputLib.RightGrab) && held) item.Suppressed = false;
                continue;
            }
            Vector3 position = item.Object.transform.position;
            if (held)
            {
                item.Velocity = Vector3.ClampMagnitude((position - item.PreviousPosition) / Mathf.Max(Time.unscaledDeltaTime, 0.001f), 20f);
                if (item.Proxy != null) item.Proxy.gameObject.SetActive(false);
            }
            else if (item.WasHeld && item.Object.currentState is TransferrableObject.PositionState.Dropped or TransferrableObject.PositionState.None)
            {
                if (item.Proxy == null)
                {
                    var obj = Shape("Holdable Physics Proxy", PrimitiveType.Sphere, position, Vector3.one * 0.15f, true, true);
                    obj.GetComponent<Renderer>().enabled = false; item.Proxy = obj.GetComponent<Rigidbody>();
                    Physics.IgnoreCollision(obj.GetComponent<Collider>(), GorillaLocomotion.GTPlayer.Instance.bodyCollider);
                    Physics.IgnoreCollision(obj.GetComponent<Collider>(), GorillaLocomotion.GTPlayer.Instance.headCollider);
                    foreach (var collider in item.Object.GetComponentsInChildren<Collider>(true)) Physics.IgnoreCollision(obj.GetComponent<Collider>(), collider);
                }
                item.Proxy.gameObject.SetActive(true); item.Proxy.position = position; item.Proxy.rotation = item.Object.transform.rotation;
                item.Proxy.linearVelocity = item.Velocity * ChudOS.Mods.Settings.ExpansionPreferences.ThrowPower; item.Proxy.angularVelocity = Vector3.zero;
            }
            else if (item.Object.currentState is not TransferrableObject.PositionState.Dropped and not TransferrableObject.PositionState.None && item.Proxy != null) item.Proxy.gameObject.SetActive(false);
            item.WasHeld = held; item.PreviousPosition = position;
        }
    }
    private void LateUpdate()
    {
        if (!enabled || Mode != "Physical Holdables") return;
        foreach (var item in items)
            if (item.Object != null && item.Object.IsMyItem() && item.Object.currentState is TransferrableObject.PositionState.Dropped or TransferrableObject.PositionState.None && item.Proxy != null && item.Proxy.gameObject.activeSelf)
                item.Object.transform.SetPositionAndRotation(item.Proxy.position, item.Proxy.rotation);
    }
    protected override void Cleanup()
    {
        if (Mode == "Sticky Holdables") foreach (var item in items) if (item.Object != null && item.Object.IsMyItem() && item.WasHeld && !item.Suppressed) Release(item);
        items.Clear();
    }
}
