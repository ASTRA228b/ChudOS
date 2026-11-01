// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using Newtonsoft.Json;
using UnityEngine;

namespace ChudOS.Mods.Safety;

public static class PoseClipboard
{
    private static float[] Position(Transform value) => new[] { value.position.x, value.position.y, value.position.z };
    private static float[] Rotation(Transform value) => new[] { value.rotation.x, value.rotation.y, value.rotation.z, value.rotation.w };
    [MenuButton("Copy Pose Data", ButtonType.Press, Category = "Safety", Order = 810, Description = "Copy your local body, head and hand transforms for placement/debugging.")]
    public static void Copy()
    {
        if (!CreativeTools.Ready) { CreativeTools.Notice("Player tracking is not ready."); return; }
        var body = GorillaLocomotion.GTPlayer.Instance.bodyCollider.transform;
        GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(new
        {
            Body = new { Position = Position(body), Rotation = Rotation(body) }, Head = new { Position = Position(CreativeTools.Head), Rotation = Rotation(CreativeTools.Head) },
            LeftHand = new { Position = Position(CreativeTools.Left), Rotation = Rotation(CreativeTools.Left) }, RightHand = new { Position = Position(CreativeTools.Right), Rotation = Rotation(CreativeTools.Right) }
        }, Formatting.Indented);
        CreativeTools.Notice("Pose data copied.");
    }
}
