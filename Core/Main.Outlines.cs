// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine;

namespace ChudOS.Core;

public sealed partial class Main
{
    private static void RefreshButtonOutline(GameObject button)
    {
        var outline = button.transform.Find("Neon Outline");
        if (!OutlineButtons || TextOnlyMenu)
        {
            if (outline != null) outline.gameObject.SetActive(false);
            return;
        }
        if (outline != null) { outline.gameObject.SetActive(true); return; }
        GameObject obj = new("Neon Outline");
        obj.transform.SetParent(button.transform, false);
        var line = obj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.sharedMaterial = BorderMat;
        line.startColor = line.endColor = Color.white;
        line.widthMultiplier = 0.008f;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.positionCount = 24;
        for (int corner = 0; corner < 4; corner++)
        {
            float centerY = corner < 2 ? 0.40f : -0.40f;
            float centerZ = corner == 0 || corner == 3 ? 0.46f : -0.46f;
            for (int step = 0; step < 6; step++)
            {
                float angle = (corner * 90f + step * 18f) * Mathf.Deg2Rad;
                line.SetPosition(corner * 6 + step, new Vector3(0.52f, centerY + Mathf.Sin(angle) * 0.10f, centerZ + Mathf.Cos(angle) * 0.04f));
            }
        }
    }
}
