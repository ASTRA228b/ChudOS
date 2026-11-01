// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChudOS.Mods.Holdables;

internal sealed class LocalOrbEffects : BetaFeature
{
    private sealed class Orb
    {
        internal GameObject Object = null!;
        internal Rigidbody Body = null!;
        internal TrailRenderer Trail = null!;
        internal Renderer Renderer = null!;
        internal float Expires;
    }
    private readonly List<Orb> pool = new();
    private float nextLeft, nextRight;
    [MenuButton("Dual Hand Orbs", ButtonType.Toggle, Category = "Holdables", Order = 14, runOption = RunOption.Once, Controls = "(LG)(RG)", Description = "Local physics orbs emitted from either hand.")]
    public static void EnableOrbs() => Enable<LocalOrbEffects>("Dual Hand Orbs");
    [MenuDisable("Dual Hand Orbs")]
    public static void DisableOrbs() => Disable("Dual Hand Orbs");
    [MenuButton("Laser Eyes", ButtonType.Toggle, Category = "Fun", Order = 810, runOption = RunOption.Once, Controls = "(RG)/(O)", Description = "Local head-directed projectiles with trails.")]
    public static void EnableEyes() => Enable<LocalOrbEffects>("Laser Eyes");
    [MenuDisable("Laser Eyes")]
    public static void DisableEyes() => Disable("Laser Eyes");
    protected override void Setup()
    {
        for (int i = 0; i < 24; i++)
        {
            var obj = Shape("Local Orb", PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.07f, true, true);
            Physics.IgnoreCollision(obj.GetComponent<Collider>(), GorillaLocomotion.GTPlayer.Instance.bodyCollider);
            Physics.IgnoreCollision(obj.GetComponent<Collider>(), GorillaLocomotion.GTPlayer.Instance.headCollider);
            foreach (var other in pool) Physics.IgnoreCollision(obj.GetComponent<Collider>(), other.Object.GetComponent<Collider>());
            var trail = obj.AddComponent<TrailRenderer>(); trail.sharedMaterial = CreativeTools.Material; trail.time = 0.3f; trail.startWidth = 0.035f; trail.endWidth = 0f;
            pool.Add(new Orb { Object = obj, Body = obj.GetComponent<Rigidbody>(), Trail = trail, Renderer = obj.GetComponent<Renderer>() });
            obj.SetActive(false);
        }
    }
    protected override void Tick()
    {
        foreach (var orb in pool) if (orb.Object.activeSelf && Time.unscaledTime >= orb.Expires) orb.Object.SetActive(false);
        if (!HoldableFeedback.AllowInput) return;
        if (Mode == "Laser Eyes")
        {
            if ((InputLib.RightGrab || Keyboard.current?.oKey.isPressed == true) && Time.unscaledTime >= nextRight)
            {
                nextRight = Time.unscaledTime + 0.2f; Emit(CreativeTools.Head.position + CreativeTools.Head.forward * 0.25f, CreativeTools.Head.forward);
            }
            return;
        }
        if (LG && Time.unscaledTime >= nextLeft) { nextLeft = Time.unscaledTime + 0.25f; if (Emit(CreativeTools.Left.position + CreativeTools.Left.forward * 0.15f, CreativeTools.Left.forward)) HoldableFeedback.Pulse(true); }
        if (RG && Time.unscaledTime >= nextRight) { nextRight = Time.unscaledTime + 0.25f; if (Emit(CreativeTools.Right.position + CreativeTools.Right.forward * 0.15f, CreativeTools.Right.forward)) HoldableFeedback.Pulse(false); }
    }
    private bool Emit(Vector3 position, Vector3 direction)
    {
        Orb? available = null;
        foreach (var orb in pool) if (!orb.Object.activeSelf) { available = orb; break; }
        if (available == null) return false;
        available.Object.transform.position = position; available.Object.transform.localScale = Vector3.one * 0.07f * ExpansionPreferences.HoldableSize;
        available.Object.SetActive(true); available.Trail.Clear(); available.Trail.startColor = available.Trail.endColor = ExpansionPreferences.HoldableColor;
        CreativeTools.Paint(available.Renderer, ExpansionPreferences.HoldableColor);
        available.Body.linearVelocity = direction.normalized * 10f * ExpansionPreferences.ThrowPower; available.Body.angularVelocity = Vector3.zero;
        available.Body.useGravity = Mode != "Laser Eyes"; available.Expires = Time.unscaledTime + 3f;
        return true;
    }
}
