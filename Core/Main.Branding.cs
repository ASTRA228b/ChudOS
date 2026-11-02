// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Core;

public sealed partial class Main
{
    private static void CreateRearBranding()
    {
        if (MenuMain == null) return;
        TextMesh version = Text("v" + Constantss.Version, -1.055f, 0.30f, 0.011f, true, SecondaryTextColor, TextAnchor.MiddleLeft);
        Vector3 right = version.transform.localRotation * Vector3.right;
        Vector3 position = version.transform.localPosition - right * 0.065f; position.x = 0.374f;
        GameObject? logo = IconLoader.Create("ChudGPT", MenuMain.transform, position, new Vector2(0.095f, 0.095f));
        if (logo == null) return;
        logo.name = "Rear ChudGPT Logo";
        logo.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
    }
}
