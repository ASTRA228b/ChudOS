// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Spammers;

internal sealed class ExpansionEmitters : BetaFeature
{
    private sealed class Particle { internal GameObject Object = null!; internal Vector3 Velocity; internal float Born, Life; internal bool Rocket; }
    private readonly List<Particle> particles = new();
    private readonly Dictionary<SlingshotProjectile, int> projectiles = new();
    private readonly List<SlingshotProjectile> returned = new();
    private readonly Queue<GameObject> marks = new();
    private static int sequence;
    private int pattern;
    private float nextNotice, nextBurst, nextPrune;
    private Rect window = new(110, 90, 330, 160);
    protected override void Setup()
    {
        if (Mode is "Bubble Wand" or "Confetti Cannon" or "Firework Sequencer")
            for (int i = 0; i < ExpansionSettings.GeometryLimit; i++)
            {
                var obj = Shape("Local Emitter Particle", Mode == "Confetti Cannon" ? PrimitiveType.Cube : PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.06f);
                obj.SetActive(false); particles.Add(new Particle { Object = obj });
            }
    }
    protected override void Tick()
    {
        float now = Time.unscaledTime;
        if (Mode is "Projectile Gun" or "Impact Painter")
        {
            bool firing = GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, out var point);
            if (firing && now >= nextAction && !ChudOS.Core.Main.IsMenuOpen)
            {
                nextAction = now + ExpansionSettings.EmitterInterval;
                if (Mode == "Projectile Gun") Launch(CreativeTools.Right.position, (point - CreativeTools.Right.position).normalized * ExpansionSettings.ToyPower);
                else
                {
                    var mark = Shape("Impact Mark (Local)", PrimitiveType.Cylinder, point + GunLib.LastHit.normal * 0.005f, new Vector3(0.16f, 0.004f, 0.16f));
                    mark.transform.rotation = Quaternion.FromToRotation(Vector3.up, GunLib.LastHit.normal); marks.Enqueue(mark);
                    while (marks.Count > ExpansionSettings.GeometryLimit) { var old = marks.Dequeue(); Objects.Remove(old); if (old != null) Object.Destroy(old); }
                }
            }
        }
        else if ((InputLib.RightTrigger || UnityEngine.InputSystem.Mouse.current?.leftButton.isPressed == true) && now >= nextAction && !ChudOS.Core.Main.IsMenuOpen)
        {
            nextAction = now + ExpansionSettings.EmitterInterval;
            Vector3 p = CreativeTools.Right.position;
            if (Mode == "Projectile Pattern Emitter")
            {
                Vector3 velocity = pattern == 0 ? Vector3.up * ExpansionSettings.ToyPower + UnityEngine.Random.insideUnitSphere * 2f : pattern == 1 ? Vector3.down * 3f : new Vector3(Mathf.Cos(now * 3f), 0.3f, Mathf.Sin(now * 3f)) * ExpansionSettings.ToyPower;
                Launch(pattern == 1 ? p + Vector3.up * 2f : p, velocity);
            }
            else if (Mode == "Bubble Wand") Emit(p, CreativeTools.Right.forward * 0.7f + Vector3.up * 0.4f, 4f, 0.12f, false);
            else if (Mode == "Confetti Cannon") for (int i = 0; i < 4; i++) Emit(p, CreativeTools.Right.forward * 3f + Vector3.up * 2f + UnityEngine.Random.insideUnitSphere * 2f, 3f, 0.035f, false);
            else if (Mode == "Firework Sequencer" && now >= nextBurst) { nextBurst = now + 1.2f; Emit(p, Vector3.up * 5f, 1f, 0.07f, true); }
        }
        foreach (var particle in particles)
        {
            if (!particle.Object.activeSelf) continue;
            float age = now - particle.Born;
            if (age >= particle.Life)
            {
                bool burst = particle.Rocket; Vector3 p = particle.Object.transform.position; particle.Object.SetActive(false);
                if (burst) for (int i = 0; i < 12; i++) Emit(p, UnityEngine.Random.onUnitSphere * 3f, 1.2f, 0.04f, false);
                continue;
            }
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            if (Mode != "Bubble Wand") particle.Velocity += Physics.gravity * (0.2f * dt);
            particle.Object.transform.position += particle.Velocity * dt;
            particle.Object.transform.Rotate(new Vector3(20, 40, 60) * dt, Space.Self);
            if (Mode == "Bubble Wand" && age > 0.2f && (Vector3.Distance(particle.Object.transform.position, CreativeTools.Left.position) < 0.15f || Vector3.Distance(particle.Object.transform.position, CreativeTools.Right.position) < 0.15f)) particle.Object.SetActive(false);
        }
        if (now >= nextPrune)
        {
            nextPrune = now + 0.25f; returned.Clear();
            foreach (var pair in projectiles) if (pair.Key == null || !pair.Key.gameObject.activeSelf || pair.Key.myProjectileCount != pair.Value || pair.Key.projectileOwner != null) returned.Add(pair.Key!);
            foreach (var projectile in returned) projectiles.Remove(projectile);
        }
    }
    private void Emit(Vector3 position, Vector3 velocity, float life, float scale, bool rocket)
    {
        var particle = particles.FirstOrDefault(p => !p.Object.activeSelf);
        if (particle == null) return;
        particle.Born = Time.unscaledTime; particle.Life = life; particle.Velocity = velocity; particle.Rocket = rocket;
        particle.Object.transform.position = position; particle.Object.transform.localScale = Vector3.one * scale;
        CreativeTools.Paint(particle.Object.GetComponent<Renderer>(), Mode == "Bubble Wand" ? new Color(0.5f, 0.85f, 1f, 0.4f) : Color.HSVToRGB(UnityEngine.Random.value, 0.65f, 1f));
        particle.Object.SetActive(true);
    }
    private void Launch(Vector3 position, Vector3 velocity)
    {
        if (projectiles.Count >= ExpansionSettings.GeometryLimit) return;
        var maker = SnowballMaker.rightHandInstance;
        var throwable = maker?.snowballs?.FirstOrDefault(s => s != null && s.projectilePrefab != null);
        if (throwable == null || ObjectPools.instance == null)
        {
            if (Time.unscaledTime >= nextNotice) { nextNotice = Time.unscaledTime + 8f; CreativeTools.Notice("Local projectile preview requires a loaded throwable prefab. Use a native throwable first."); }
            return;
        }
        GameObject obj = ObjectPools.instance.Instantiate(throwable.projectilePrefab, true);
        if (obj == null) return;
        var projectile = obj.GetComponent<SlingshotProjectile>();
        if (projectile == null) { ObjectPools.instance.Destroy(obj); return; }
        // A local preview deliberately emits no room message and claims no network visibility.
        projectile.Launch(position, Vector3.ClampMagnitude(velocity, 30f), null!, false, false, ++sequence, 1f, true, CreativeTools.Theme);
        projectiles[projectile] = projectile.myProjectileCount;
    }
    private void OnGUI()
    {
        if (Mode == "Projectile Pattern Emitter") window = MenuHelper.MakeWindow(853100, window, Contents, "Local Projectile Patterns");
    }
    private void Contents(int id)
    {
        MenuHelper.Label("Pattern: " + (pattern == 0 ? "Fountain" : pattern == 1 ? "Rain" : "Orbit"));
        if (MenuHelper.Button("Next Pattern")) pattern = (pattern + 1) % 3;
        MenuHelper.Label("Local preview only. Hold RT / LMB with the menu closed."); GUI.DragWindow();
    }
    protected override void Cleanup()
    {
        foreach (var pair in projectiles) if (pair.Key != null && pair.Key.gameObject.activeSelf && pair.Key.myProjectileCount == pair.Value && pair.Key.projectileOwner == null && ObjectPools.instance != null) ObjectPools.instance.Destroy(pair.Key.gameObject);
        projectiles.Clear(); GunLib.Hide();
    }
}
