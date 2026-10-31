// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class C4Mod
{
    private static readonly CreativeTools.Session Session = new();
    private static Transform? charge;
    private static bool detonateInput;
    [MenuButton("C4", ButtonType.Toggle, Category = "Movement", Order = 501, Controls = "(RG)(A)", runOption = RunOption.FixedUpdate, Description = "Hold RG to position a charge; release to leave it there; press A to detonate. Desktop: G and Space")]
    public static void Tick()
    {
        if (Session.Changed()) Clear();
        if (!CreativeTools.Ready) { Clear(); return; }
        PortTools.Register(Clear);
        if (PortBindings.DesktopBlocked) { detonateInput = false; return; }
        bool desktop = !XRSettings.isDeviceActive;
        bool place = desktop ? Keyboard.current?.gKey.isPressed == true : InputLib.RightGrab;
        bool detonate = desktop ? Keyboard.current?.spaceKey.isPressed == true : InputLib.RightControllerAButton;
        bool pressed = CreativeTools.Pressed(detonate, ref detonateInput);
        if (place)
        {
            if (charge == null) charge = CreativeTools.Marker("C4", 0.16f * CreativeTools.Scale);
            charge.position = desktop ? CreativeTools.Head.position + CreativeTools.Head.forward * 0.6f : CreativeTools.Right.position;
            CreativeTools.Paint(charge.GetComponent<Renderer>(), CreativeTools.Theme);
        }
        if (!pressed || charge == null) return;
        Vector3 impulse = PortMovementMath.ChargeImpulse(CreativeTools.Position - charge.position, PortSettings.C4Power, PortSettings.C4Radius, CreativeTools.Scale);
        if (CreativeTools.Finite(impulse)) CreativeTools.Body.AddForce(impulse, ForceMode.VelocityChange);
        Object.Destroy(charge.gameObject);
        charge = null;
    }
    [MenuEnable("C4")]
    public static void Enable() => PortBindings.Refresh();
    [MenuDisable("C4")]
    public static void Clear()
    {
        if (charge != null) Object.Destroy(charge.gameObject);
        charge = null;
        detonateInput = false;
    }
}
