// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using GorillaLocomotion.Swimming;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Visuals;

internal sealed class ExpansionVisuals : BetaFeature
{
    private Camera? camera, original;
    private RenderTexture? texture;
    private Material? sky, oldSky, screenMaterial;
    private Color ambient;
    private AmbientMode ambientMode;
    private float ambientIntensity, nextScan, started;
    private readonly List<(Behaviour Item, bool Enabled)> effects = new();
    private readonly List<(ParticleSystem Item, bool Emitting)> snow = new();
    private readonly List<(Transform Target, LineRenderer Line)> targets = new();
    private readonly List<(Vector3 Position, Quaternion Rotation)> points = new();
    private LineRenderer? trajectory;
    private TextMesh? label;
    private Vector3 mirrorPoint, tint, appliedTint = new(-1f, -1f, -1f);
    private bool playing, originalEnabled;
    private Rect window = new(35, 70, 350, 320);
    protected override void Setup()
    {
        if (Mode == "Fullbright")
        {
            ambient = RenderSettings.ambientLight; ambientMode = RenderSettings.ambientMode; ambientIntensity = RenderSettings.ambientIntensity;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = Color.white; RenderSettings.ambientIntensity = 1f;
            var light = gameObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 0.8f; light.shadows = LightShadows.None;
        }
        if (Mode == "Underwater Vision")
        {
            foreach (var effect in Object.FindObjectsByType<UnderwaterCameraEffect>(FindObjectsSortMode.None)) { effects.Add((effect, effect.enabled)); effect.enabled = false; }
            foreach (var effect in Object.FindObjectsByType<UnderwaterParticleEffects>(FindObjectsSortMode.None)) { effects.Add((effect, effect.enabled)); effect.enabled = false; }
            if (effects.Count == 0) { Unavailable("load a map with underwater camera effects first."); return; }
        }
        if (Mode == "Skybox Editor")
        {
            oldSky = RenderSettings.skybox;
            Shader shader = oldSky != null ? oldSky.shader : Shader.Find("Skybox/Procedural");
            if (shader == null) { Unavailable("no supported skybox shader is loaded."); return; }
            sky = oldSky != null ? new Material(oldSky) : new Material(shader);
            tint = new Vector3(CreativeTools.Theme.r, CreativeTools.Theme.g, CreativeTools.Theme.b); RenderSettings.skybox = sky;
        }
        if (Mode is "Minimap" or "World Mirror")
        {
            var obj = new GameObject("ChudOS Render Camera"); obj.transform.SetParent(transform);
            camera = obj.AddComponent<Camera>(); camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.cullingMask = ~(1 << 2); camera.nearClipPlane = 0.05f; camera.farClipPlane = 160f;
            texture = new RenderTexture(256, 256, 16); texture.Create(); camera.targetTexture = texture;
            if (Mode == "Minimap") { camera.orthographic = true; camera.orthographicSize = 22f; }
            else
            {
                mirrorPoint = CreativeTools.Head.position + Forward * 2f;
                var screen = Shape("Mirror Screen", PrimitiveType.Quad, mirrorPoint, new Vector3(1.6f, 1.2f, 1f));
                screen.transform.rotation = Quaternion.LookRotation(Forward);
                Shader shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Sprites/Default");
                screenMaterial = new Material(shader) { mainTexture = texture }; screen.GetComponent<Renderer>().sharedMaterial = screenMaterial;
            }
        }
        if (Mode is "Photo Mode" or "Camera Dolly")
        {
            original = Camera.allCameras.Where(c => c.enabled && !c.stereoEnabled && c.targetTexture == null).OrderByDescending(c => c.depth).FirstOrDefault();
            if (original == null) { Unavailable("a desktop camera is required."); return; }
            originalEnabled = original.enabled;
            var obj = new GameObject("ChudOS Photo Camera"); obj.transform.SetParent(transform);
            camera = obj.AddComponent<Camera>(); camera.CopyFrom(original); camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.transform.SetPositionAndRotation(original.transform.position, original.transform.rotation);
            original.enabled = false;
        }
        if (Mode == "Projectile Trajectory Preview") trajectory = Line("Throwable Preview", 32);
        if (Mode == "Surface Inspector") label = Label("Aim at a surface", CreativeTools.Right.position);
        if (Mode == "Snow Control")
        {
            foreach (var item in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
                if (item.main.loop && (item.name.IndexOf("snowfall", StringComparison.OrdinalIgnoreCase) >= 0 || item.name.IndexOf("snowflake", StringComparison.OrdinalIgnoreCase) >= 0 || item.name.IndexOf("weather", StringComparison.OrdinalIgnoreCase) >= 0 && item.name.IndexOf("snow", StringComparison.OrdinalIgnoreCase) >= 0) && !item.transform.IsChildOf(transform))
                { var emission = item.emission; snow.Add((item, emission.enabled)); emission.enabled = false; }
            if (snow.Count == 0) CreativeTools.Notice("Snow Control: this loaded scene has no matching snowfall particles.");
        }
    }
    protected override void Tick()
    {
        if (label != null && View != null) label.transform.rotation = Quaternion.LookRotation(label.transform.position - View.transform.position);
        switch (Mode)
        {
            case "Minimap":
                if (camera != null) { camera.transform.position = CreativeTools.Position + Vector3.up * 35f; camera.transform.rotation = Quaternion.Euler(90f, CreativeTools.Head.eulerAngles.y, 0f); }
                break;
            case "World Mirror":
                if (camera != null)
                {
                    Vector3 toward = (CreativeTools.Head.position - mirrorPoint).normalized;
                    camera.transform.position = mirrorPoint - toward * 0.1f;
                    camera.transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
                }
                break;
            case "Skybox Editor":
                if (sky != null && tint != appliedTint) { appliedTint = tint; Color c = new(tint.x, tint.y, tint.z); if (sky.HasProperty("_Tint")) sky.SetColor("_Tint", c); if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", c); }
                break;
            case "Surface Inspector":
                GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, out var point);
                if (label != null && GunLib.HasHit)
                {
                    var hit = GunLib.LastHit; label.transform.position = point + Vector3.up * 0.15f;
                    if (Time.unscaledTime >= nextScan) { nextScan = Time.unscaledTime + 0.2f; label.text = hit.collider.name + "\nLayer: " + hit.collider.gameObject.layer + " | " + hit.collider.GetType().Name + "\nNormal: " + hit.normal.ToString("F2"); }
                }
                break;
            case "Projectile Trajectory Preview":
                if (trajectory != null)
                {
                    Vector3 origin = CreativeTools.Right.position, velocity = CreativeTools.Right.forward * ExpansionSettings.ToyPower + CreativeTools.Body.linearVelocity;
                    for (int i = 0; i < trajectory.positionCount; i++) { float t = i * 0.05f; trajectory.SetPosition(i, origin + velocity * t + Physics.gravity * (0.5f * t * t)); }
                }
                break;
            case "Photo Mode":
                if (camera != null && CreativeTools.Pressed(A, ref primary)) Capture();
                break;
            case "Camera Dolly":
                if (CreativeTools.Pressed(X, ref primary) && points.Count < 24) { points.Add((CreativeTools.Head.position, CreativeTools.Head.rotation)); CreativeTools.Notice("Dolly keyframe " + points.Count); }
                if (CreativeTools.Pressed(A, ref secondary)) { playing = !playing && points.Count > 1; started = Time.unscaledTime; }
                if (camera != null && playing)
                {
                    float t = (Time.unscaledTime - started) * 0.3f; int i = Mathf.FloorToInt(t);
                    if (i >= points.Count - 1) playing = false;
                    else camera.transform.SetPositionAndRotation(Vector3.Lerp(points[i].Position, points[i + 1].Position, Mathf.SmoothStep(0, 1, t - i)), Quaternion.Slerp(points[i].Rotation, points[i + 1].Rotation, Mathf.SmoothStep(0, 1, t - i)));
                }
                break;
            case "Wind Barrier Viewer": case "NPC Tracker": case "Resource Tracker":
                if (Time.unscaledTime >= nextScan) Scan();
                foreach (var target in targets)
                {
                    if (target.Target == null || target.Line == null) continue;
                    Vector3 p = target.Target.position;
                    target.Line.SetPosition(0, CreativeTools.Right.position); target.Line.SetPosition(1, p);
                    target.Line.enabled = (p - CreativeTools.Position).sqrMagnitude < ExpansionSettings.VisualRange * ExpansionSettings.VisualRange;
                }
                break;
        }
    }
    private void Scan()
    {
        nextScan = Time.unscaledTime + 5f;
        foreach (var target in targets) if (target.Line != null) { Objects.Remove(target.Line.gameObject); Object.Destroy(target.Line.gameObject); }
        targets.Clear();
        string[] words = Mode == "Wind Barrier Viewer" ? new[] { "windbarrier", "wind_barrier", "windbox" } : Mode == "NPC Tracker" ? new[] { "gorillaghost", "lucy", "butterfly", "critter", "beeswarm", "batflock", "creature" } : new[] { "collectible", "miningresource", "resourcepickup", "crystalpickup" };
        foreach (MonoBehaviour component in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (component == null || component.transform.IsChildOf(transform)) continue;
            string name = (component.GetType().Name + component.name).Replace(" ", "").ToLowerInvariant();
            if (!words.Any(name.Contains) || targets.Any(t => t.Target == component.transform)) continue;
            targets.Add((component.transform, Line(Mode + " marker")));
            if (targets.Count >= 48) break;
        }
    }
    private void Capture()
    {
        string folder = Path.Combine(FileManager.RootFolder, "Photos"); Directory.CreateDirectory(folder);
        ScreenCapture.CaptureScreenshot(Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".png")); CreativeTools.Notice("Photo saved to ChudOS/Photos.");
    }
    private void OnGUI()
    {
        if (Mode is "Minimap" or "Skybox Editor" or "Photo Mode" or "Camera Dolly") window = MenuHelper.MakeWindow(850000 + Mode.GetHashCode() % 10000, window, WindowContents, Mode);
    }
    private void WindowContents(int id)
    {
        if (Mode == "Minimap" && texture != null) GUILayout.Label(texture, GUILayout.Width(256), GUILayout.Height(256));
        if (Mode == "Skybox Editor") { MenuHelper.Label("Sky tint"); tint.x = MenuHelper.Slider(tint.x, 0f, 1f); tint.y = MenuHelper.Slider(tint.y, 0f, 1f); tint.z = MenuHelper.Slider(tint.z, 0f, 1f); }
        if (Mode == "Photo Mode" && camera != null) { MenuHelper.Label("Field of view"); camera.fieldOfView = MenuHelper.Slider(camera.fieldOfView, 25f, 110f); if (MenuHelper.Button("Save Photo")) Capture(); }
        if (Mode == "Camera Dolly") { MenuHelper.Label("X / F: add head-pose keyframe. A / Space: play."); MenuHelper.Label("Keyframes: " + points.Count); if (MenuHelper.Button("Clear Keyframes")) { playing = false; points.Clear(); } }
        GUI.DragWindow();
    }
    protected override void Cleanup()
    {
        if (Mode == "Fullbright") { RenderSettings.ambientLight = ambient; RenderSettings.ambientMode = ambientMode; RenderSettings.ambientIntensity = ambientIntensity; }
        if (Mode == "Skybox Editor") { if (RenderSettings.skybox == sky) RenderSettings.skybox = oldSky; if (sky != null) Object.Destroy(sky); }
        foreach (var effect in effects) if (effect.Item != null) effect.Item.enabled = effect.Enabled;
        foreach (var item in snow) if (item.Item != null) { var emission = item.Item.emission; emission.enabled = item.Emitting; }
        if (original != null) original.enabled = originalEnabled;
        if (camera != null) { camera.enabled = false; camera.targetTexture = null; }
        if (texture != null) { texture.Release(); Object.Destroy(texture); }
        if (screenMaterial != null) Object.Destroy(screenMaterial);
        GunLib.Hide();
    }
}
