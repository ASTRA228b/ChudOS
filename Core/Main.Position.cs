// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Mods.Settings;
using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Core;

public sealed partial class Main
{
    private static readonly Vector3 PanelCenter = new(0.558f, 0f, 0f);
    private static Transform? positionHand;
    private static bool positionDirty = true;
    private static Vector3 previousUp = Vector3.up;

    public static void ApplyMenuPosition() => positionDirty = true;
    private static void UpdateMenuPosition()
    {
        if (OnPC || !IsMenuOpen || MenuMain == null || GorillaTagger.Instance == null) return;
        var hand = GorillaTagger.Instance.leftHandTransform;
        if (hand == null) return;
        var menu = MenuMain.transform;
        if (MenuPositionSettings.Mode == 2)
        {
            UpdateFixedMenuPosition(menu, hand);
            positionHand = hand;
            positionDirty = false;
            return;
        }
        bool reset = positionDirty || positionHand != hand || menu.parent != hand;
        if (MenuPositionSettings.Mode != 0 && !reset) return;
        if (menu.parent != hand) menu.SetParent(hand, false);
        menu.localScale = VRMenuScale;
        Vector3 center = Vector3.Scale(PanelCenter, VRMenuScale);
        Vector3 anchor = VRMenuPosition + center + new Vector3(MenuPositionSettings.Distance, MenuPositionSettings.Height, 0f);
        if (reset)
        {
            menu.localPosition = VRMenuPosition + new Vector3(MenuPositionSettings.Distance, MenuPositionSettings.Height, 0f);
            menu.localRotation = Quaternion.identity;
            if (MenuPositionSettings.Mode == 1) AlignToPalm();
        }
        if (MenuPositionSettings.Mode == 0 && GorillaTagger.Instance.headCollider != null)
        {
            var head = GorillaTagger.Instance.headCollider.transform;
            Vector3 normal = head.position - hand.TransformPoint(anchor);
            Vector3 up = Vector3.ProjectOnPlane(head.up, normal);
            if (up.sqrMagnitude < 0.0001f) up = Vector3.ProjectOnPlane(previousUp, normal);
            if (up.sqrMagnitude < 0.0001f) up = Vector3.ProjectOnPlane(head.forward, normal);
            if (Finite(normal) && Finite(up) && normal.sqrMagnitude > 0.0001f && up.sqrMagnitude > 0.0001f)
            {
                Vector3 localNormal = hand.localToWorldMatrix.transpose.MultiplyVector(normal).normalized;
                Vector3 localUp = Vector3.ProjectOnPlane(hand.InverseTransformVector(up), localNormal).normalized;
                if (Finite(localNormal) && Finite(localUp) && localNormal.sqrMagnitude > 0.0001f && localUp.sqrMagnitude > 0.0001f)
                {
                    Quaternion target = Quaternion.LookRotation(Vector3.Cross(localNormal, localUp), localUp);
                    menu.localRotation = reset ? target : Quaternion.RotateTowards(menu.localRotation, target, 720f * Mathf.Min(Time.unscaledDeltaTime, 0.05f));
                    previousUp = up.normalized;
                }
            }
            menu.localPosition = anchor - menu.localRotation * center;
        }
        positionHand = hand;
        positionDirty = false;
    }
    private static void UpdateFixedMenuPosition(Transform menu, Transform hand)
    {
        Transform? parent = Instance != null ? Instance.transform : null;
        if (menu.parent != parent) menu.SetParent(parent, false);
        float scale = GTPlayer.Instance != null ? GTPlayer.Instance.scale : 1f;
        if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f) scale = 1f;
        menu.localScale = new Vector3(0.1f, VRMenuScale.y, VRMenuScale.z) * scale;
        Vector3 offset = new Vector3(MenuPositionSettings.Distance, MenuPositionSettings.Height, 0f) * scale;
        menu.SetPositionAndRotation(hand.position + hand.rotation * offset, hand.rotation);
    }

    private static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsNaN(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.x) && !float.IsInfinity(value.y) && !float.IsInfinity(value.z);
}
