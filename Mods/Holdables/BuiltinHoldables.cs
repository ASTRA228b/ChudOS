// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Holdables;

internal sealed class BuiltinHoldables : IDisposable
{
    internal static readonly (string Id, string Name)[] Models =
    {
        ("sword", "Toy Sword"), ("hammer", "Toy Hammer"), ("shield", "Round Shield"), ("wand", "Magic Wand"),
        ("banana", "Banana"), ("flower", "Flower"), ("microphone", "Toy Microphone"), ("flag", "ChudOS Flag"), ("cross", "Wooden Cross"),
        ("bisexual-flag", "Bisexual Flag"), ("gay-flag", "Gay Pride Flag"), ("trans-flag", "Trans Flag")
    };
    private static readonly Color Wood = new(0.38f, 0.19f, 0.08f), Metal = new(0.75f, 0.8f, 0.87f), Gold = new(1f, 0.7f, 0.1f), Dark = new(0.08f, 0.08f, 0.1f);
    private readonly Dictionary<string, GameObject> prefabs = new(StringComparer.Ordinal);
    private readonly Dictionary<Color, Material> materials = new();

    internal GameObject Get(string id, Transform parent)
    {
        if (prefabs.TryGetValue(id, out var cached)) return cached;
        var root = new GameObject("ChudOS Builtin " + id); root.SetActive(false); root.transform.SetParent(parent, false);
        switch (id)
        {
            case "sword":
                Part(root, "Handle", PrimitiveType.Cylinder, new(0, 0, 0), new(0.04f, 0.09f, 0.04f), Wood);
                Part(root, "Guard", PrimitiveType.Cube, new(0, 0.1f, 0), new(0.19f, 0.025f, 0.05f), Gold);
                Part(root, "Blade", PrimitiveType.Cube, new(0, 0.34f, 0), new(0.055f, 0.45f, 0.016f), Metal);
                Part(root, "Tip", PrimitiveType.Cube, new(0, 0.57f, 0), new(0.035f, 0.035f, 0.016f), Metal, new(0, 0, 45)); break;
            case "hammer":
                Part(root, "Handle", PrimitiveType.Cylinder, new(0, 0.09f, 0), new(0.045f, 0.19f, 0.045f), Wood);
                Part(root, "Head", PrimitiveType.Cube, new(0, 0.31f, 0), new(0.28f, 0.15f, 0.14f), Metal); break;
            case "shield":
                Part(root, "Shield", PrimitiveType.Cylinder, new(0, 0.12f, 0.04f), new(0.4f, 0.02f, 0.4f), Metal, new(90, 0, 0));
                Part(root, "Center", PrimitiveType.Sphere, new(0, 0.12f, 0.075f), Vector3.one * 0.12f, Gold);
                Part(root, "Grip", PrimitiveType.Cube, new(0, 0, 0), new(0.035f, 0.13f, 0.04f), Wood); break;
            case "wand":
                Part(root, "Handle", PrimitiveType.Cylinder, new(0, 0.1f, 0), new(0.025f, 0.19f, 0.025f), Dark);
                Part(root, "Orb", PrimitiveType.Sphere, new(0, 0.32f, 0), Vector3.one * 0.09f, new Color(0.6f, 0.2f, 1f));
                Part(root, "Band", PrimitiveType.Cylinder, new(0, 0.26f, 0), new(0.04f, 0.018f, 0.04f), Gold); break;
            case "banana":
                for (int i = 0; i < 7; i++)
                {
                    float angle = (i - 3) * 12f * Mathf.Deg2Rad;
                    Part(root, "Banana Segment", PrimitiveType.Capsule, new(Mathf.Cos(angle) * 0.22f - 0.2f, Mathf.Sin(angle) * 0.22f + 0.12f, 0), new(0.065f, 0.05f, 0.065f), Color.yellow, new(0, 0, -angle * Mathf.Rad2Deg));
                }
                Part(root, "Stem", PrimitiveType.Cube, new(-0.025f, 0.285f, 0), new(0.025f, 0.045f, 0.025f), Wood); break;
            case "flower":
                Part(root, "Stem", PrimitiveType.Cylinder, new(0, 0.12f, 0), new(0.016f, 0.2f, 0.016f), new Color(0.15f, 0.6f, 0.2f));
                Part(root, "Center", PrimitiveType.Sphere, new(0, 0.35f, 0), Vector3.one * 0.07f, Gold);
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * Mathf.PI / 4f;
                    Part(root, "Petal", PrimitiveType.Sphere, new(Mathf.Cos(angle) * 0.06f, 0.35f + Mathf.Sin(angle) * 0.06f, 0), new(0.075f, 0.075f, 0.025f), new Color(1f, 0.35f, 0.65f));
                }
                Part(root, "Leaf", PrimitiveType.Sphere, new(0.04f, 0.14f, 0), new(0.1f, 0.025f, 0.04f), new Color(0.15f, 0.6f, 0.2f), new(0, 0, 25)); break;
            case "microphone":
                Part(root, "Handle", PrimitiveType.Cylinder, new(0, 0.05f, 0), new(0.045f, 0.12f, 0.045f), Dark);
                Part(root, "Grille", PrimitiveType.Sphere, new(0, 0.21f, 0), new(0.1f, 0.12f, 0.1f), Metal);
                Part(root, "Band", PrimitiveType.Cylinder, new(0, 0.15f, 0), new(0.055f, 0.015f, 0.055f), Gold); break;
            case "flag":
                Part(root, "Pole", PrimitiveType.Cylinder, new(0, 0.2f, 0), new(0.018f, 0.3f, 0.018f), Wood);
                Part(root, "Cloth", PrimitiveType.Cube, new(0.14f, 0.42f, 0), new(0.28f, 0.17f, 0.008f), new Color(0.6f, 0.08f, 1f));
                Part(root, "Stripe", PrimitiveType.Cube, new(0.14f, 0.42f, 0.005f), new(0.28f, 0.035f, 0.006f), Color.white); break;
            case "bisexual-flag":
                Flag(root, new[] { new Color(0.839f, 0.008f, 0.439f), new Color(0.608f, 0.310f, 0.588f), new Color(0.000f, 0.220f, 0.659f) }, new[] { 0.4f, 0.2f, 0.4f }); break;
            case "gay-flag":
                Flag(root, new[] { new Color(0.894f, 0.012f, 0.012f), new Color(1f, 0.549f, 0f), new Color(1f, 0.929f, 0f), new Color(0f, 0.502f, 0.149f), new Color(0f, 0.302f, 1f), new Color(0.459f, 0.027f, 0.529f) }); break;
            case "trans-flag":
                Color blue = new(0.357f, 0.808f, 0.980f), pink = new(0.961f, 0.663f, 0.722f);
                Flag(root, new[] { blue, pink, Color.white, pink, blue }); break;
            case "cross":
                Part(root, "Upright", PrimitiveType.Cube, new(0, 0.15f, 0), new(0.055f, 0.48f, 0.045f), Wood);
                Part(root, "Crossbar", PrimitiveType.Cube, new(0, 0.26f, 0), new(0.29f, 0.055f, 0.045f), Wood); break;
            default: Object.Destroy(root); throw new ArgumentException("Unknown built-in holdable.", nameof(id));
        }
        prefabs[id] = root;
        return root;
    }
    private void Flag(GameObject root, Color[] colors, float[]? proportions = null)
    {
        Part(root, "Pole", PrimitiveType.Cylinder, new(0, 0.2f, 0), new(0.018f, 0.3f, 0.018f), Wood);
        float top = 0.505f;
        for (int i = 0; i < colors.Length; i++)
        {
            float height = 0.17f * (proportions == null ? 1f / colors.Length : proportions[i]);
            Part(root, "Stripe " + i, PrimitiveType.Cube, new(0.14f, top - height * 0.5f, 0), new(0.28f, height, 0.008f), colors[i]);
            top -= height;
        }
    }
    private void Part(GameObject root, string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Vector3 rotation = default)
    {
        var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.layer = 2;
        obj.transform.SetParent(root.transform, false); obj.transform.localPosition = position; obj.transform.localRotation = Quaternion.Euler(rotation); obj.transform.localScale = scale;
        var collider = obj.GetComponent<Collider>(); collider.enabled = false; Object.Destroy(collider);
        if (!materials.TryGetValue(color, out var material)) { material = new Material(CreativeTools.Material.shader) { name = "ChudOS Holdable Color", color = color }; materials[color] = material; }
        var renderer = obj.GetComponent<Renderer>(); renderer.sharedMaterial = material; renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
    }
    public void Dispose()
    {
        foreach (var obj in prefabs.Values) if (obj != null) Object.Destroy(obj);
        foreach (var material in materials.Values) if (material != null) Object.Destroy(material);
        prefabs.Clear(); materials.Clear();
    }
}
