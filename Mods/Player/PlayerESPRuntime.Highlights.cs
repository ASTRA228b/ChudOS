// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections.Generic;
using ChudOS.Mods.Settings;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Player;

public static partial class PlayerESPRuntime
{
    private static readonly Dictionary<VRRig, ChamsState> ChamsItems = new();
    private static readonly Dictionary<VRRig, LineRenderer> BeaconItems = new();

    private sealed class ChamsState
    {
        private SkinnedMeshRenderer? source, overlay;
        private Mesh? mesh;
        private Transform? rootBone;
        private readonly List<Material> materials = new();

        internal void Tick(VRRig rig)
        {
            SkinnedMeshRenderer skin = rig.mainSkin;
            if (skin == null) { Restore(); return; }
            if (source != skin || overlay == null)
            {
                Restore();
                Material? material = GetMaterial();
                if (material == null) return;
                GameObject obj = new("ChudOS Chams");
                obj.layer = skin.gameObject.layer;
                obj.transform.SetParent(skin.transform, false);
                overlay = obj.AddComponent<SkinnedMeshRenderer>();
                overlay.shadowCastingMode = ShadowCastingMode.Off;
                overlay.receiveShadows = false;
                overlay.updateWhenOffscreen = true;
                overlay.quality = skin.quality;
                source = skin;
            }

            Mesh currentMesh = skin.sharedMesh;
            if (currentMesh == null) { overlay.enabled = false; return; }
            if (mesh != currentMesh || rootBone != skin.rootBone)
            {
                mesh = currentMesh;
                rootBone = skin.rootBone;
                overlay.sharedMesh = mesh;
                overlay.rootBone = rootBone;
                overlay.bones = skin.bones;
                materials.Clear();
                Material? material = GetMaterial();
                if (material == null) { Restore(); return; }
                for (int i = 0; i < mesh.subMeshCount; i++) materials.Add(material);
                overlay.SetSharedMaterials(materials);
            }

            overlay.localBounds = skin.localBounds;
            overlay.enabled = skin.enabled && skin.gameObject.activeInHierarchy;
            for (int i = 0; i < currentMesh.blendShapeCount; i++) overlay.SetBlendShapeWeight(i, skin.GetBlendShapeWeight(i));
            Color color = PlayerHighlightSettings.ChamsTheme ? ChudOS.Core.Main.PrimaryColorValue : RigColor(rig);
            Tint(overlay, color, PlayerHighlightSettings.ChamsOpacity);
        }

        internal void Restore()
        {
            if (overlay != null)
            {
                overlay.gameObject.SetActive(false);
                Object.Destroy(overlay.gameObject);
            }
            overlay = null;
            source = null;
            mesh = null;
            rootBone = null;
            materials.Clear();
        }
    }

    public static void Chams()
    {
        if (VRRig.LocalRig == null) { ClearChams(); return; }
        Sync(ChamsItems, rig => new ChamsState(), state => state.Restore());
        foreach (var pair in ChamsItems) if (pair.Key != null) pair.Value.Tick(pair.Key);
    }
    public static void ClearChams() => Clear(ChamsItems, state => state.Restore());

    private static LineRenderer? MakeBeacon(VRRig rig)
    {
        LineRenderer? line = MakeLine("ChudOS Beacon", rig, 2, true, PlayerHighlightSettings.BeaconWidth);
        if (line != null) line.gameObject.layer = rig.gameObject.layer;
        return line;
    }

    public static void Beacons()
    {
        if (VRRig.LocalRig == null) { ClearBeacons(); return; }
        Sync(BeaconItems, MakeBeacon, RemoveBeacon);
        foreach (var pair in BeaconItems)
        {
            VRRig rig = pair.Key;
            LineRenderer line = pair.Value;
            if (rig == null || line == null) continue;
            line.enabled = rig.gameObject.activeInHierarchy;
            if (!line.enabled) continue;
            float scale = Mathf.Clamp(rig.scaleFactor, 0.2f, 3f);
            Vector3 head = rig.headMesh != null ? rig.headMesh.transform.position : rig.transform.position + Vector3.up * (0.5f * scale);
            Vector3 start = head + Vector3.up * (0.2f * scale);
            Color color = PlayerHighlightSettings.BeaconTheme ? ChudOS.Core.Main.PrimaryColorValue : RigColor(rig);
            line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = PlayerHighlightSettings.BeaconWidth * scale;
            line.SetPosition(0, start);
            line.SetPosition(1, start + Vector3.up * (PlayerHighlightSettings.BeaconHeight * scale));
        }
    }

    private static void RemoveBeacon(LineRenderer line)
    {
        if (line == null) return;
        line.enabled = false;
        Object.Destroy(line.gameObject);
    }
    public static void ClearBeacons() => Clear(BeaconItems, RemoveBeacon);
    public static void ClearHighlights() { ClearChams(); ClearBeacons(); }
}
