// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine;

namespace ChudOS.Core.Managers;

internal static class SoundboardTextFit
{
    internal static void Apply(TextMesh text, float width, float height)
    {
        if (text == null) return;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.021f;
        if (text.font != null) text.font.RequestCharactersInTexture(text.text, text.fontSize, text.fontStyle);
        var renderer = text.GetComponent<MeshRenderer>();
        if (renderer == null) return;
        Vector3 size = renderer.localBounds.size;
        if (size.x <= 0f || size.y <= 0f) return;
        text.characterSize *= Mathf.Min(1f, Mathf.Min(width / size.x, height / size.y));
    }
}
