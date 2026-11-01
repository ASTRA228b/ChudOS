// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.OP;

internal sealed class PropControlGun : BetaFeature
{
    private TransferrableObject? selected;
    private Rigidbody? body;
    private bool kinematic, gravity;
    private float distance;
    private Vector3 target;

    [MenuButton("Prop Control Gun", ButtonType.Toggle, Category = "OP", Order = 20, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Select and hold a locally owned dropped transferable object.")]
    public static void EnableGun() => Enable<PropControlGun>("Prop Control Gun");
    [MenuDisable("Prop Control Gun")]
    public static void DisableGun() => Disable("Prop Control Gun");
    protected override void Tick()
    {
        bool shooting = GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, out var point);
        if (!shooting) { Release(); return; }
        if (selected == null && GunLib.HasHit)
        {
            var item = GunLib.LastHit.collider.GetComponentInParent<TransferrableObject>();
            if (item == null || !item.IsMyItem() || item.currentState is not TransferrableObject.PositionState.Dropped and not TransferrableObject.PositionState.None) return;
            selected = item; body = selected.GetComponent<Rigidbody>();
            distance = Mathf.Clamp(Vector3.Distance(GunLib.AimHand.position, point), 0.5f, 8f);
            if (body != null) { kinematic = body.isKinematic; gravity = body.useGravity; body.isKinematic = true; }
        }
        if (selected == null) return;
        if (!selected.IsMyItem() || selected.currentState is not TransferrableObject.PositionState.Dropped and not TransferrableObject.PositionState.None) { Release(); return; }
        Vector3 origin = GunLib.AimHand.position;
        target = origin + (point - origin).normalized * distance;
    }
    private void LateUpdate() { if (enabled && selected != null && selected.IsMyItem()) selected.transform.position = target; }
    private void Release()
    {
        if (body != null) { body.isKinematic = kinematic; body.useGravity = gravity; }
        selected = null; body = null;
    }
    protected override void Cleanup() { Release(); GunLib.Hide(); }
}
