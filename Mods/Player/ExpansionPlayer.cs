// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using Photon.Voice.PUN;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Player;

internal sealed class ExpansionPlayer : BetaFeature
{
    private sealed class Mark
    {
        internal VRRig Rig = null!;
        internal LineRenderer Line = null!;
        internal PhotonVoiceView? Voice;
        internal SkinnedMeshRenderer? Skin;
        internal Mesh? Baked;
        internal float LastOutline;
        internal readonly List<Vector3> Vertices = new();
        internal readonly List<Vector2> Points = new(), Hull = new();
    }
    private readonly Dictionary<VRRig, Mark> marks = new();
    private readonly Dictionary<AudioSource, float> volumes = new();
    private VRRig? selected;
    private string details = "Aim and select a player to inspect their loaded cosmetics.";
    private float scan, notice, sample;
    private Rect window = new(180, 100, 410, 360);
    private Vector2 scroll;
    private GUIStyle? textStyle;
    protected override void Tick()
    {
        if (Time.unscaledTime >= scan)
        {
            scan = Time.unscaledTime + 1f;
            var actors = VRRigCache.ActiveRigs.Where(r => r != null && r != VRRig.LocalRig).OrderBy(r => (r.transform.position - CreativeTools.Position).sqrMagnitude).Take(Mode == "Outline ESP" ? 4 : 16).ToArray();
            foreach (var pair in marks.ToArray())
                if (pair.Key == null || !actors.Contains(pair.Key)) { Remove(pair.Value); marks.Remove(pair.Key!); }
            foreach (var rig in actors)
            {
                if (rig == null || rig == VRRig.LocalRig || marks.ContainsKey(rig)) continue;
                if (marks.Count >= 16) break;
                Mark mark = new() { Rig = rig, Line = Line(Mode + " player", Mode == "FOV Cone Viewer" ? 5 : 2), Voice = rig.GetComponentInChildren<PhotonVoiceView>(true), Skin = rig.GetComponentInChildren<SkinnedMeshRenderer>(true) };
                if (Mode == "Outline ESP") mark.Baked = new Mesh();
                marks[rig] = mark;
            }
        }
        if (Mode == "Cosmetic Inspector")
        {
            if (GunLib.UpdateRigGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, ref selected) && selected != null && Time.unscaledTime >= sample)
            {
                sample = Time.unscaledTime + 0.5f;
                details = PlayerInspectionDetails.Describe(selected);
            }
        }
        foreach (var mark in marks.Values)
        {
            VRRig rig = mark.Rig;
            if (rig == null || rig.head?.rigTarget == null) continue;
            Vector3 p = rig.head.rigTarget.position;
            float distance = Vector3.Distance(p, CreativeTools.Head.position);
            mark.Line.enabled = distance < ExpansionSettings.VisualRange && Mode is not "Player Audio Panel" and not "Cosmetic Inspector";
            if (!mark.Line.enabled) continue;
            if (Mode == "Nearby Player Alert")
            {
                if (ExpansionPreferences.InfectedAlerts && !PlayerInspectionDetails.IsInfected(rig)) { mark.Line.enabled = false; continue; }
                mark.Line.enabled = distance < 4f;
                if (distance < 4f && Time.unscaledTime >= notice) { notice = Time.unscaledTime + 4f; CreativeTools.Notice("Nearby player: " + rig.gameObject.name + " (" + distance.ToString("F1") + "m)"); }
            }
            if (Mode == "Voice Radar") mark.Line.enabled = mark.Voice != null && mark.Voice.IsSpeaking;
            if (Mode == "FOV Cone Viewer")
            {
                Vector3 f = rig.head.rigTarget.forward * 2f, r = rig.head.rigTarget.right * 0.8f;
                mark.Line.SetPosition(0, p); mark.Line.SetPosition(1, p + f - r); mark.Line.SetPosition(2, p + f + r); mark.Line.SetPosition(3, p); mark.Line.SetPosition(4, p + f);
            }
            else if (Mode != "Outline ESP") { mark.Line.SetPosition(0, CreativeTools.Head.position - Vector3.up * 0.2f); mark.Line.SetPosition(1, p); }
        }
        if (Mode == "Outline ESP" && Time.unscaledTime >= sample)
        {
            sample = Time.unscaledTime + 0.05f;
            Mark? oldest = null;
            foreach (var mark in marks.Values) if (mark.Line.enabled && (oldest == null || mark.LastOutline < oldest.LastOutline)) oldest = mark;
            if (oldest != null) { Outline(oldest); oldest.LastOutline = Time.unscaledTime; }
        }
    }
    private void Outline(Mark mark)
    {
        if (mark.Skin == null || mark.Baked == null || View == null) return;
        mark.Skin.BakeMesh(mark.Baked); mark.Baked.GetVertices(mark.Vertices); mark.Points.Clear();
        float depth = Vector3.Dot(mark.Rig.transform.position - View.transform.position, View.transform.forward);
        if (depth < 0.1f) { mark.Line.enabled = false; return; }
        int step = Mathf.Max(1, mark.Vertices.Count / 160);
        for (int i = 0; i < mark.Vertices.Count; i += step)
        {
            Vector3 p = View.WorldToViewportPoint(mark.Skin.transform.TransformPoint(mark.Vertices[i]));
            if (p.z > 0.1f) mark.Points.Add(new Vector2(p.x, p.y));
        }
        mark.Points.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
        mark.Hull.Clear();
        foreach (var p in mark.Points) { while (mark.Hull.Count >= 2 && ExpansionMath.Cross(mark.Hull[^2], mark.Hull[^1], p) <= 0f) mark.Hull.RemoveAt(mark.Hull.Count - 1); mark.Hull.Add(p); }
        int lower = mark.Hull.Count;
        for (int i = mark.Points.Count - 2; i >= 0; i--) { var p = mark.Points[i]; while (mark.Hull.Count > lower && ExpansionMath.Cross(mark.Hull[^2], mark.Hull[^1], p) <= 0f) mark.Hull.RemoveAt(mark.Hull.Count - 1); mark.Hull.Add(p); }
        mark.Line.positionCount = mark.Hull.Count;
        for (int i = 0; i < mark.Hull.Count; i++) mark.Line.SetPosition(i, View.ViewportToWorldPoint(new Vector3(mark.Hull[i].x, mark.Hull[i].y, depth)));
    }
    private void OnGUI()
    {
        if (Mode is "Cosmetic Inspector" or "Player Audio Panel") window = MenuHelper.MakeWindow(851100 + (Mode == "Cosmetic Inspector" ? 1 : 2), window, Contents, Mode);
    }
    private void Contents(int id)
    {
        scroll = GUILayout.BeginScrollView(scroll);
        textStyle ??= new GUIStyle(GUI.skin.label) { wordWrap = true, richText = false };
        textStyle.normal.textColor = ChudOS.Core.Main.TextColorValue;
        if (Mode == "Cosmetic Inspector") GUILayout.Label(details, textStyle);
        else foreach (var mark in marks.Values)
        {
            if (mark.Rig == null) continue;
            MenuHelper.Label(mark.Rig.gameObject.name);
            var speaker = mark.Voice != null ? mark.Voice.SpeakerInUse : null;
            var audio = speaker != null ? speaker.GetComponent<AudioSource>() : null;
            if (audio == null) { MenuHelper.Label("Voice source not ready"); continue; }
            if (!volumes.ContainsKey(audio)) volumes[audio] = audio.volume;
            audio.volume = MenuHelper.Slider(audio.volume, 0f, 1f);
        }
        GUILayout.EndScrollView(); GUI.DragWindow();
    }
    private void Remove(Mark mark) { if (mark.Line != null) { Objects.Remove(mark.Line.gameObject); Object.Destroy(mark.Line.gameObject); } if (mark.Baked != null) Object.Destroy(mark.Baked); }
    protected override void Cleanup()
    {
        foreach (var mark in marks.Values) Remove(mark);
        foreach (var pair in volumes) if (pair.Key != null) pair.Key.volume = pair.Value;
        marks.Clear(); volumes.Clear(); GunLib.Hide();
    }
}
