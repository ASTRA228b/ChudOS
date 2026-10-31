// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Rig;

public static class PoseSequencer
{
    private sealed class Pose { internal readonly Vector3[] Positions = new Vector3[3]; internal readonly Quaternion[] Rotations = new Quaternion[3]; }
    private static readonly List<Pose> Poses = new();
    [MenuButton("Pose Sequencer", ButtonType.Toggle, Category = "Rig", Order = 400, Controls = "(X)(A)(B)", runOption = RunOption.Once, Description = "X saves your current head and hand pose; A starts or stops the sequence; B clears saved poses")]
    public static void Enable() => RigRuntime.Enable<Sequence>("Pose Sequencer");
    [MenuDisable("Pose Sequencer")]
    public static void Disable() => RigRuntime.Disable("Pose Sequencer");
    private sealed class Sequence : RigBehaviour
    {
        private bool saveInput, playInput, clearInput, playing;
        private float started;
        private Vector3 origin;
        private Quaternion rotation;
        private readonly CreativeTools.Session session = new();
        protected override void OnEnable()
        {
            session.Changed();
            saveInput = InputLib.LeftControllerXButton; playInput = InputLib.RightControllerAButton; clearInput = InputLib.RightControllerBButton;
        }
        protected override void LateUpdate()
        {
            if (session.Changed()) { Poses.Clear(); Stop(); }
            bool save = CreativeTools.Pressed(InputLib.LeftControllerXButton, ref saveInput), play = CreativeTools.Pressed(InputLib.RightControllerAButton, ref playInput), clear = CreativeTools.Pressed(InputLib.RightControllerBButton, ref clearInput);
            if (clear) { Poses.Clear(); Stop(); return; }
            if (save && !playing && Poses.Count < 12) Save();
            if (play)
            {
                if (playing) Stop();
                else if (Poses.Count >= 2)
                {
                    origin = CreativeTools.Head.position - Vector3.up * 0.35f * CreativeTools.Scale;
                    rotation = Quaternion.Euler(0f, CreativeTools.Head.eulerAngles.y, 0f);
                    started = Time.unscaledTime; playing = true; RigRuntime.ToggleRig(false);
                }
                else CreativeTools.Notice("Save at least two poses first.");
            }
            if (!playing) return;
            float progress = (Time.unscaledTime - started) / CreativeSettings.PoseTime;
            int index = Mathf.FloorToInt(progress) % Poses.Count;
            Pose a = Poses[index], b = Poses[(index + 1) % Poses.Count];
            float blend = Mathf.SmoothStep(0f, 1f, progress - Mathf.Floor(progress));
            VRRig rig = VRRig.LocalRig;
            rig.transform.SetPositionAndRotation(origin, rotation);
            Apply(rig.head.rigTarget, a, b, 0, blend); Apply(rig.leftHand.rigTarget, a, b, 1, blend); Apply(rig.rightHand.rigTarget, a, b, 2, blend);
        }
        private static void Save()
        {
            Vector3 center = CreativeTools.Head.position - Vector3.up * 0.35f * CreativeTools.Scale;
            Quaternion inverse = Quaternion.Inverse(Quaternion.Euler(0f, CreativeTools.Head.eulerAngles.y, 0f));
            Pose pose = new();
            Capture(pose, CreativeTools.Head, 0, center, inverse); Capture(pose, CreativeTools.Left, 1, center, inverse); Capture(pose, CreativeTools.Right, 2, center, inverse);
            Poses.Add(pose);
            CreativeTools.Notice($"Pose {Poses.Count} saved.");
        }
        private static void Capture(Pose pose, Transform target, int index, Vector3 center, Quaternion inverse)
        {
            pose.Positions[index] = inverse * (target.position - center) / CreativeTools.Scale;
            pose.Rotations[index] = inverse * target.rotation;
        }
        private void Apply(Transform target, Pose a, Pose b, int index, float blend) => target.SetPositionAndRotation(origin + rotation * Vector3.Lerp(a.Positions[index], b.Positions[index], blend) * CreativeTools.Scale, rotation * Quaternion.Slerp(a.Rotations[index], b.Rotations[index], blend));
        private void Stop() { playing = false; RigRuntime.RestorePose(); }
        protected override void OnDisable() => Stop();
    }
}
