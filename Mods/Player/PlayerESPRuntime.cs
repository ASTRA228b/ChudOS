// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Player;


public static class PlayerESPRuntime
{
    private static readonly Dictionary<VRRig, LineRenderer> Tracers = new();
    private static readonly Dictionary<VRRig, BoneState> Skeletons = new();
    private static readonly Dictionary<VRRig, LineRenderer> Boxes2D = new();
    private static readonly Dictionary<VRRig, MeshRenderer> Boxes3D = new();
    private static readonly Dictionary<VRRig, MeshRenderer> Boxes4D = new();
    private static readonly Dictionary<VRRig, TrailRenderer> Trails = new();
    private static readonly List<VRRig> ToRemove = new();
    private static Material? OverlayMaterial;
    private static readonly MaterialPropertyBlock TintBlock = new();

    private sealed class BoneState
    {
        public VRRig Rig = null!;
        public bool SkeletonEnabled;
        public bool RendererEnabled;
        public Material? PreviousMaterial;
        public Material? CustomMaterial;

        public void Restore()
        {
            if (Rig != null && Rig.skeleton != null && Rig.skeleton.renderer != null)
            {
                Rig.skeleton.renderer.sharedMaterial = PreviousMaterial;
                Rig.skeleton.renderer.enabled = RendererEnabled;
                Rig.skeleton.enabled = SkeletonEnabled;
            }
            if (CustomMaterial != null) Object.Destroy(CustomMaterial);
        }
    }

    private static bool IsRemote(VRRig rig) => rig != null && rig != VRRig.LocalRig && !rig.isLocal && !rig.isOfflineVRRig;
    private static Color RigColor(VRRig rig) => rig.playerColor;


    private static void Sync<T>(Dictionary<VRRig, T> items, Func<VRRig, T?> spawn, Action<T> destroy) where T : class
    {
        var rigs = VRRigCache.ActiveRigs;
        ToRemove.Clear();
        foreach (var pair in items)
            if (!IsRemote(pair.Key) || rigs == null || !rigs.Contains(pair.Key)) ToRemove.Add(pair.Key);

        foreach (VRRig rig in ToRemove)
        {
            destroy(items[rig]);
            items.Remove(rig);
        }
        ToRemove.Clear();

        if (rigs == null) return;
        foreach (VRRig rig in rigs)
        {
            if (!IsRemote(rig) || items.ContainsKey(rig)) continue;
            T? visual = spawn(rig);
            if (visual != null) items.Add(rig, visual);
        }
    }

    private static void Clear<T>(Dictionary<VRRig, T> items, Action<T> destroy) where T : class
    {
        foreach (T visual in items.Values) destroy(visual);
        items.Clear();
    }

    private static Material? GetMaterial()
    {
        if (OverlayMaterial != null) return OverlayMaterial;
        Shader shader = Shader.Find("Hidden/Internal-Colored") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        if (shader == null) return null;

        OverlayMaterial = new Material(shader) { name = "ChudOS Player ESP" };
        OverlayMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        OverlayMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        OverlayMaterial.SetInt("_Cull", (int)CullMode.Off);
        OverlayMaterial.SetInt("_ZWrite", 0);
        OverlayMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
        OverlayMaterial.renderQueue = (int)RenderQueue.Transparent;
        OverlayMaterial.color = Color.white;
        return OverlayMaterial;
    }

    private static LineRenderer? MakeLine(string name, VRRig rig, int count, bool worldSpace, float width)
    {
        Material? material = GetMaterial();
        if (material == null) return null;
        GameObject obj = new(name);
        if (!worldSpace) obj.transform.SetParent(rig.transform, false);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.useWorldSpace = worldSpace;
        line.positionCount = count;
        line.startWidth = width;
        line.endWidth = width;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.alignment = LineAlignment.View;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        return line;
    }

    private static void Tint(Renderer renderer, Color color, float alpha)
    {
        TintBlock.Clear();
        TintBlock.SetColor("_Color", new Color(color.r, color.g, color.b, alpha));
        renderer.SetPropertyBlock(TintBlock);
    }

    public static void BodyTracers()
    {
        GTPlayer player = GTPlayer.Instance;
        if (player == null || player.headCollider == null) return;
        Sync(Tracers, rig => MakeLine("ChudOS Body Tracer", rig, 2, true, 0.012f), line => { if (line != null) Object.Destroy(line.gameObject); });
        Vector3 origin = player.headCollider.transform.position + Vector3.down * 0.6f;
        foreach (var pair in Tracers)
        {
            if (pair.Key == null || pair.Value == null) continue;
            pair.Value.SetPosition(0, origin);
            pair.Value.SetPosition(1, pair.Key.transform.position);
            pair.Value.startColor = RigColor(pair.Key);
            pair.Value.endColor = RigColor(pair.Key);
        }
    }

    public static void ClearBodyTracers() => Clear(Tracers, line => { if (line != null) Object.Destroy(line.gameObject); });

    private static BoneState? MakeBone(VRRig rig)
    {
        if (rig.skeleton == null || rig.skeleton.renderer == null) return null;
        Material? overlay = GetMaterial();
        if (overlay == null) return null;
        BoneState state = new()
        {
            Rig = rig,
            SkeletonEnabled = rig.skeleton.enabled,
            RendererEnabled = rig.skeleton.renderer.enabled,
            PreviousMaterial = rig.skeleton.renderer.sharedMaterial,
            CustomMaterial = new Material(overlay)
        };
        rig.skeleton.renderer.sharedMaterial = state.CustomMaterial;
        rig.skeleton.renderer.enabled = true;
        rig.skeleton.enabled = true;
        return state;
    }

    public static void BoneESP()
    {
        Sync(Skeletons, MakeBone, state => state.Restore());
        foreach (var pair in Skeletons)
        {
            if (pair.Key == null || pair.Key.skeleton == null || pair.Key.skeleton.renderer == null) continue;
            pair.Key.skeleton.enabled = true;
            pair.Key.skeleton.renderer.enabled = true;
            if (pair.Value.CustomMaterial != null) pair.Value.CustomMaterial.color = RigColor(pair.Key);
        }
    }

    public static void ClearBoneESP() => Clear(Skeletons, state => state.Restore());

    private static LineRenderer? Make2DBox(VRRig rig)
    {
        LineRenderer? line = MakeLine("ChudOS 2D Box", rig, 5, false, 0.012f);
        if (line == null) return null;
        line.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        line.SetPosition(0, new Vector3(-0.32f, -0.46f, 0f));
        line.SetPosition(1, new Vector3( 0.32f, -0.46f, 0f));
        line.SetPosition(2, new Vector3( 0.32f,  0.46f, 0f));
        line.SetPosition(3, new Vector3(-0.32f,  0.46f, 0f));
        line.SetPosition(4, new Vector3(-0.32f, -0.46f, 0f));
        return line;
    }

    public static void BoxESP2D()
    {
        Sync(Boxes2D, Make2DBox, line => { if (line != null) Object.Destroy(line.gameObject); });
        Transform? view = Camera.main != null ? Camera.main.transform : GTPlayer.Instance?.headCollider?.transform;
        foreach (var pair in Boxes2D)
        {
            if (pair.Key == null || pair.Value == null) continue;
            if (view != null)
            {
                Vector3 direction = view.position - pair.Value.transform.position;
                if (direction.sqrMagnitude > 0.0001f) pair.Value.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
            pair.Value.startColor = RigColor(pair.Key);
            pair.Value.endColor = RigColor(pair.Key);
        }
    }

    public static void ClearBoxESP2D() => Clear(Boxes2D, line => { if (line != null) Object.Destroy(line.gameObject); });

    private static MeshRenderer? MakeBox(VRRig rig, bool flat)
    {
        Material? material = GetMaterial();
        if (material == null) return null;
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = flat ? "ChudOS 4D Box" : "ChudOS 3D Box";
        obj.transform.SetParent(rig.transform, false);
        obj.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = flat ? new Vector3(0.85f, 0.9f, 0.003f) : new Vector3(0.55f, 0.85f, 0.48f);
        Collider col = obj.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return renderer;
    }

    public static void BoxESP3D()
    {
        Sync(Boxes3D, rig => MakeBox(rig, false), box => { if (box != null) Object.Destroy(box.gameObject); });
        foreach (var pair in Boxes3D)
            if (pair.Key != null && pair.Value != null) Tint(pair.Value, RigColor(pair.Key), 0.15f);
    }

    public static void ClearBoxESP3D() => Clear(Boxes3D, box => { if (box != null) Object.Destroy(box.gameObject); });

    public static void BoxESP4D()
    {
        Sync(Boxes4D, rig => MakeBox(rig, true), box => { if (box != null) Object.Destroy(box.gameObject); });
        foreach (var pair in Boxes4D)
            if (pair.Key != null && pair.Value != null) Tint(pair.Value, RigColor(pair.Key), 0.22f);
    }

    public static void ClearBoxESP4D() => Clear(Boxes4D, box => { if (box != null) Object.Destroy(box.gameObject); });

    private static TrailRenderer? MakeTrail(VRRig rig)
    {
        Material? material = GetMaterial();
        if (material == null) return null;
        GameObject obj = new("ChudOS Player Trail");
        obj.transform.SetParent(rig.transform, false);
        obj.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        TrailRenderer trail = obj.AddComponent<TrailRenderer>();
        trail.sharedMaterial = material;
        trail.time = 2.5f;
        trail.minVertexDistance = 0.04f;
        trail.startWidth = 0.035f;
        trail.endWidth = 0.008f;
        trail.numCornerVertices = 3;
        trail.numCapVertices = 3;
        trail.alignment = LineAlignment.View;
        trail.emitting = true;
        return trail;
    }

    public static void TrailsESP()
    {
        Sync(Trails, MakeTrail, trail => { if (trail != null) Object.Destroy(trail.gameObject); });
        foreach (var pair in Trails)
        {
            if (pair.Key == null || pair.Value == null) continue;
            Color color = RigColor(pair.Key);
            pair.Value.startColor = color;
            pair.Value.endColor = new Color(color.r, color.g, color.b, 0f);
        }
    }

    public static void ClearTrailsESP() => Clear(Trails, trail => { if (trail != null) Object.Destroy(trail.gameObject); });
}
