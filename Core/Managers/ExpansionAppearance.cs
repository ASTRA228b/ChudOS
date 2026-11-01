// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Mods.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Core.Managers;

internal static class ExpansionAppearance
{
    private sealed class MaterialState
    {
        internal Shader Shader = null!;
        internal Texture? Texture;
        internal Vector2 Offset, Scale;
        internal string TextureProperty = "", GradientProperty = "";
        internal int RenderQueue;
    }
    private static readonly Dictionary<Material, MaterialState> materials = new();
    private static readonly Texture2D?[] gradients = new Texture2D?[3];
    private static int revision = -1;
    private static float nextAnimation;
    private static bool active, shaderWarning;

    internal static void Apply(Material?[] targets)
    {
        if (ExpansionPreferences.Gradient == 0) return;
        Shader shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent Cutout");
        if (shader == null)
        {
            if (!shaderWarning) { Debug.LogWarning("[ChudOS] No opaque texture shader is available for gradients; keeping the selected theme."); shaderWarning = true; }
            return;
        }
        if (revision != ExpansionPreferences.Revision)
        {
            revision = ExpansionPreferences.Revision;
            ExpansionPreferences.GradientColors(out Color first, out Color second);
            BuildGradient(0, first * 0.075f, second * 0.075f);
            BuildGradient(1, first * 0.18f, second * 0.18f);
            BuildGradient(2, ExpansionPreferences.PressedStart, ExpansionPreferences.PressedEnd);
        }
        for (int i = 0; i < targets.Length; i++)
        {
            Material? material = targets[i];
            if (material == null) continue;
            if (!materials.TryGetValue(material, out var state))
            {
                string property = TextureProperty(material);
                state = new MaterialState { Shader = material.shader, TextureProperty = property, RenderQueue = material.renderQueue };
                if (property.Length > 0) { state.Texture = material.GetTexture(property); state.Offset = material.GetTextureOffset(property); state.Scale = material.GetTextureScale(property); }
                materials[material] = state;
            }
            material.shader = shader;
            state.GradientProperty = TextureProperty(material);
            if (state.GradientProperty.Length == 0) { material.shader = state.Shader; continue; }
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 0f);
            if (material.HasProperty("_ZWrite")) material.SetInt("_ZWrite", 1);
            if (material.HasProperty("_SrcBlend")) material.SetInt("_SrcBlend", 1);
            if (material.HasProperty("_DstBlend")) material.SetInt("_DstBlend", 0);
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            material.SetTexture(state.GradientProperty, gradients[i <= 1 ? 0 : i <= 3 ? 1 : 2]);
            material.SetTextureScale(state.GradientProperty, Vector2.one); material.SetTextureOffset(state.GradientProperty, Vector2.zero);
            active = true;
        }
    }
    private static string TextureProperty(Material material) => material.HasProperty("_BaseMap") ? "_BaseMap" : material.HasProperty("_MainTex") ? "_MainTex" : "";
    private static void BuildGradient(int index, Color first, Color second)
    {
        if (gradients[index] != null) Object.Destroy(gradients[index]);
        const int size = 16;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "ChudOS Gradient " + index, wrapMode = TextureWrapMode.Mirror, filterMode = FilterMode.Bilinear };
        first.a = second.a = 1f;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float t = ExpansionPreferences.GradientDirection switch { 1 => x / (size - 1f), 2 => (x + y) / ((size - 1f) * 2f), _ => y / (size - 1f) };
            pixels[y * size + x] = Color.Lerp(first, second, t);
        }
        texture.SetPixels(pixels); texture.Apply(false, true); gradients[index] = texture;
    }
    internal static void Tick()
    {
        if (!active || !ChudOS.Core.Main.IsMenuOpen || ExpansionPreferences.Gradient == 0 || !ExpansionPreferences.AnimatedGradient || Time.unscaledTime < nextAnimation) return;
        nextAnimation = Time.unscaledTime + 0.05f;
        float movement = Mathf.Repeat(Time.unscaledTime * ExpansionPreferences.GradientSpeed, 2f);
        Vector2 offset = ExpansionPreferences.GradientDirection == 1 ? new Vector2(movement, 0f) : new Vector2(0f, movement);
        foreach (var pair in materials) if (pair.Key != null && pair.Value.GradientProperty.Length > 0) pair.Key.SetTextureOffset(pair.Value.GradientProperty, offset);
    }
    internal static void Restore()
    {
        active = false;
        foreach (var pair in materials)
        {
            if (pair.Key == null) continue;
            var state = pair.Value;
            pair.Key.shader = state.Shader; pair.Key.renderQueue = state.RenderQueue;
            if (state.TextureProperty.Length > 0) { pair.Key.SetTexture(state.TextureProperty, state.Texture); pair.Key.SetTextureOffset(state.TextureProperty, state.Offset); pair.Key.SetTextureScale(state.TextureProperty, state.Scale); }
        }
    }
    internal static void Cleanup()
    {
        Restore(); materials.Clear();
        foreach (var texture in gradients) if (texture != null) Object.Destroy(texture);
        Array.Clear(gradients, 0, gradients.Length); revision = -1; shaderWarning = false;
        ExpansionFonts.Cleanup();
    }
}
