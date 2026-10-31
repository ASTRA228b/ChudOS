// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using ChudOS.Mods.Movement;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ChudOS.Libraries;

internal static class CreativeTools
{
    private static Material? material;
    private static readonly MaterialPropertyBlock Colors = new();
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly float[] Samples = new float[256];
    private static AudioClip? audioClip, unavailableClip;
    private static int audioFrame = -1;
    private static float audioTime, average, nextBeat, level;
    private static bool beat;
    internal static bool Ready => PortTools.Ready && GTPlayer.Instance.bodyCollider != null && GTPlayer.Instance.headCollider != null && GTPlayer.Instance.LeftHand.controllerTransform != null && GTPlayer.Instance.RightHand.controllerTransform != null && Finite(GTPlayer.Instance.scale) && GTPlayer.Instance.scale > 0f;
    internal static Rigidbody Body => GorillaTagger.Instance.rigidbody;
    internal static Vector3 Position => GTPlayer.Instance.bodyCollider.transform.position;
    internal static Transform Head => GTPlayer.Instance.headCollider.transform;
    internal static Transform Left => GTPlayer.Instance.LeftHand.controllerTransform;
    internal static Transform Right => GTPlayer.Instance.RightHand.controllerTransform;
    internal static float Scale => Mathf.Clamp(GTPlayer.Instance.scale, 0.2f, 3f);
    internal static Color Theme => ChudOS.Core.Main.PrimaryColorValue;
    internal static float AudioLevel { get { Analyze(); return level; } }
    internal static bool AudioBeat { get { Analyze(); return beat; } }
    internal static Material Material
    {
        get
        {
            if (material != null) return material;
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader == null) throw new InvalidOperationException("No suitable effect shader is available.");
            material = new Material(shader) { name = "ChudOS Creative Effects" };
            PortTools.Register(Release);
            return material;
        }
    }
    internal sealed class Session
    {
        private GTPlayer? player;
        private object? room;
        private int scene = -1;
        internal bool Changed()
        {
            var current = GTPlayer.Instance;
            object? currentRoom = PhotonNetwork.CurrentRoom;
            int currentScene = SceneManager.GetActiveScene().handle;
            bool changed = player != current || !ReferenceEquals(room, currentRoom) || scene != currentScene;
            player = current; room = currentRoom; scene = currentScene;
            return changed;
        }
    }
    internal static bool Pressed(bool held, ref bool previous) { bool pressed = held && !previous; previous = held; return pressed; }
    internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    internal static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
    internal static Vector3 Vector(float[]? values, Vector3 fallback)
    {
        if (values == null || values.Length < 3) return fallback;
        Vector3 result = new(values[0], values[1], values[2]);
        return Finite(result) ? result : fallback;
    }
    internal static bool Ground(out RaycastHit hit, float extra = 0.12f)
    {
        Bounds bounds = GTPlayer.Instance.bodyCollider.bounds;
        return Physics.Raycast(bounds.center, Vector3.down, out hit, bounds.extents.y + extra * Scale, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore) && hit.collider != null && hit.collider.GetComponentInParent<VRRig>() == null && hit.normal.y > 0.4f;
    }
    internal static Transform Marker(string name, float size)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "ChudOS " + name;
        obj.layer = 2;
        Collider collider = obj.GetComponent<Collider>();
        collider.enabled = false;
        Object.Destroy(collider);
        obj.transform.localScale = Vector3.one * size;
        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = Material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        Paint(renderer, Theme);
        return obj.transform;
    }
    internal static LineRenderer Line(string name, int points, float width, bool loop = false)
    {
        LineRenderer line = PortTools.Line("ChudOS " + name, width, points);
        line.sharedMaterial = Material;
        line.loop = loop;
        line.gameObject.layer = 2;
        return line;
    }
    internal static void Paint(Renderer renderer, Color color) { Colors.Clear(); Colors.SetColor(ColorId, color); renderer.SetPropertyBlock(Colors); }
    internal static void Circle(LineRenderer line, Vector3 center, Vector3 normal, float radius)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up);
        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, center + rotation * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }
    internal static void Notice(string text) => NotifyLib.SendNotificationALL(text, 3500);
    private static void Analyze()
    {
        if (audioFrame == Time.frameCount) return;
        audioFrame = Time.frameCount;
        beat = false;
        AudioClip? clip = SoundManager.CurrentClip;
        float time = SoundManager.CurrentTime;
        if (clip == null || !SoundManager.IsPlaying || clip.loadState != AudioDataLoadState.Loaded) { level = average = 0f; audioClip = clip; audioTime = time; return; }
        bool changed = audioClip != clip || Mathf.Abs(time - audioTime) > 0.25f;
        audioClip = clip; audioTime = time;
        if (unavailableClip == clip) { level = 0f; return; }
        int sample = Mathf.Clamp((int)(time * clip.frequency), 0, Mathf.Max(0, clip.samples - 1));
        if (!clip.GetData(Samples, sample)) { unavailableClip = clip; level = 0f; return; }
        float sum = 0f;
        for (int i = 0; i < Samples.Length; i++) sum += Samples[i] * Samples[i];
        level = Mathf.Sqrt(sum / Samples.Length);
        if (!Finite(level)) level = 0f;
        if (changed) { average = level; nextBeat = Time.unscaledTime + 0.2f; return; }
        beat = level > 0.015f && level > average * 1.45f && Time.unscaledTime >= nextBeat;
        if (beat) nextBeat = Time.unscaledTime + 0.22f;
        average = Mathf.Lerp(average, level, 1f - Mathf.Exp(-4f * Mathf.Min(Time.unscaledDeltaTime, 0.1f)));
    }
    private static void Release()
    {
        if (material != null) Object.Destroy(material);
        material = null; audioClip = unavailableClip = null; audioFrame = -1; level = average = 0f;
    }
}
