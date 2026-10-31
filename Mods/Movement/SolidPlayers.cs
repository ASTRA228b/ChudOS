// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class SolidPlayers
{
    private sealed class Proxy { internal GameObject Root = null!; internal Transform Body = null!, Left = null!, Right = null!; }
    private static readonly Dictionary<VRRig, Proxy> Proxies = new();
    private static readonly List<VRRig> Removed = new();
    private static readonly CreativeTools.Session Session = new();
    private static float nextScan;
    [MenuButton("Solid Players", ButtonType.Toggle, Category = "Movement", Order = 505, runOption = RunOption.FixedUpdate, Description = "Add local collision around other players; choose body or hand coverage in Settings")]
    public static void Tick()
    {
        if (Session.Changed()) Stop();
        if (!CreativeTools.Ready || NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom) { Stop(); return; }
        PortTools.Register(Stop);
        if (Time.unscaledTime >= nextScan)
        {
            nextScan = Time.unscaledTime + 0.5f;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
                if (Valid(rig) && !Proxies.ContainsKey(rig)) Proxies.Add(rig, Create(rig));
        }
        Removed.Clear();
        foreach (var entry in Proxies)
        {
            VRRig rig = entry.Key;
            Proxy proxy = entry.Value;
            if (!Valid(rig) || proxy.Root == null) { Removed.Add(rig); continue; }
            float scale = Mathf.Clamp(rig.scaleFactor, 0.2f, 3f);
            bool body = PortSettings.PlayerCollisionParts != 1, hands = PortSettings.PlayerCollisionParts != 0;
            proxy.Body.gameObject.SetActive(body); proxy.Left.gameObject.SetActive(hands); proxy.Right.gameObject.SetActive(hands);
            if (body)
            {
                Vector3 center = rig.bodyRenderer != null ? rig.bodyRenderer.transform.position : rig.transform.position;
                Set(proxy.Body, center, rig.transform.rotation, new Vector3(0.32f, 0.55f, 0.32f) * scale);
            }
            if (hands)
            {
                Set(proxy.Left, rig.leftHand.rigTarget.position, Quaternion.identity, Vector3.one * 0.16f * scale);
                Set(proxy.Right, rig.rightHand.rigTarget.position, Quaternion.identity, Vector3.one * 0.16f * scale);
            }
        }
        foreach (VRRig rig in Removed)
        {
            if (Proxies[rig].Root != null) Object.Destroy(Proxies[rig].Root);
            Proxies.Remove(rig);
        }
    }
    private static bool Valid(VRRig? rig) => rig != null && rig != VRRig.LocalRig && !rig.isLocal && rig.gameObject.activeInHierarchy && rig.leftHand.rigTarget != null && rig.rightHand.rigTarget != null && CreativeTools.Finite(rig.transform.position) && CreativeTools.Finite(rig.scaleFactor) && rig.scaleFactor > 0f;
    private static Proxy Create(VRRig rig)
    {
        GameObject root = new("ChudOS Solid Player");
        root.transform.SetParent(rig.transform, false);
        Proxy proxy = new() { Root = root };
        proxy.Body = ColliderObject("Body", root.transform, false);
        proxy.Left = ColliderObject("Left Hand", root.transform, true);
        proxy.Right = ColliderObject("Right Hand", root.transform, true);
        return proxy;
    }
    private static Transform ColliderObject(string name, Transform parent, bool sphere)
    {
        GameObject obj = new(name);
        obj.layer = CollisionLayer();
        obj.transform.SetParent(parent, false);
        if (sphere) obj.AddComponent<SphereCollider>().radius = 0.5f;
        else obj.AddComponent<BoxCollider>();
        return obj.transform;
    }
    private static void Set(Transform target, Vector3 position, Quaternion rotation, Vector3 size)
    {
        target.SetPositionAndRotation(position, rotation);
        Vector3 inherited = target.parent.lossyScale;
        target.localScale = new Vector3(size.x / Mathf.Max(Mathf.Abs(inherited.x), 0.001f), size.y / Mathf.Max(Mathf.Abs(inherited.y), 0.001f), size.z / Mathf.Max(Mathf.Abs(inherited.z), 0.001f));
    }
    internal static int CollisionLayer()
    {
        int mask = NoClipState.CollisionLayers;
        for (int i = 0; i < 32; i++) if ((mask & (1 << i)) != 0) return i;
        throw new InvalidOperationException("No locomotion collision layer is available.");
    }
    [MenuDisable("Solid Players")]
    public static void Stop()
    {
        foreach (Proxy proxy in Proxies.Values) if (proxy.Root != null) Object.Destroy(proxy.Root);
        Proxies.Clear(); Removed.Clear(); nextScan = 0f;
    }
}
