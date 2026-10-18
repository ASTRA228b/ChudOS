
using System.Collections.Generic;
using System.Linq;
using ChudOS.Libraries;
using ChudOS.Stuff;
using ChudOS.Core.Managers;
using GorillaGameModes;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Player;

public static class PlayerMods
{
    private const float TagTimeout = 8f;
    private const float TargetTimeout = 3f;
    private const float TagReportInterval = 0.2f;

    private static bool RigDisabled;
    private static bool PreviousRigState;
    private static bool HandMoved;
    private static Vector3 PreviousControllerHand;
    private static Vector3 PreviousRigHand;

    private static VRRig? Target;
    private static float TagTimer;
    private static float NextTagReport;
    private static float SelfStartTime;

    private static List<VRRig>? AllTargets;
    private static int TargetIndex;
    private static float TargetStartTime;

    private static bool AuraModified;
    private static float PreviousTagDistance;

    [MenuButton("Body Tracers", ButtonType.Toggle, Category = "Player", Order = 0, runOption = RunOption.LateUpdate, Description = "Draw local tracers from you to other players")]
    public static void BodyTracers() => PlayerESPRuntime.BodyTracers();

    [MenuDisable("Body Tracers")]
    public static void StopBodyTracers() => PlayerESPRuntime.ClearBodyTracers();

    [MenuButton("Bone ESP", ButtonType.Toggle, Category = "Player", Order = 1, runOption = RunOption.LateUpdate, Description = "Show other players' skeleton visuals")]
    public static void BoneESP() => PlayerESPRuntime.BoneESP();

    [MenuDisable("Bone ESP")]
    public static void StopBoneESP() => PlayerESPRuntime.ClearBoneESP();

    [MenuButton("2D Box ESP", ButtonType.Toggle, Category = "Player", Order = 2, runOption = RunOption.LateUpdate, Description = "Billboard outlines around players")]
    public static void BoxESP2D() => PlayerESPRuntime.BoxESP2D();

    [MenuDisable("2D Box ESP")]
    public static void StopBoxESP2D() => PlayerESPRuntime.ClearBoxESP2D();

    [MenuButton("3D Box ESP", ButtonType.Toggle, Category = "Player", Order = 3, runOption = RunOption.LateUpdate, Description = "Transparent 3D boxes around players")]
    public static void BoxESP3D() => PlayerESPRuntime.BoxESP3D();

    [MenuDisable("3D Box ESP")]
    public static void StopBoxESP3D() => PlayerESPRuntime.ClearBoxESP3D();

    [MenuButton("4D Box ESP", ButtonType.Toggle, Category = "Player", Order = 4, runOption = RunOption.LateUpdate, Description = "Thin transparent panels around players")]
    public static void BoxESP4D() => PlayerESPRuntime.BoxESP4D();

    [MenuDisable("4D Box ESP")]
    public static void StopBoxESP4D() => PlayerESPRuntime.ClearBoxESP4D();

    [MenuButton("Trails ESP", ButtonType.Toggle, Category = "Player", Order = 5, runOption = RunOption.LateUpdate, Description = "Show other players' recent movement trails")]
    public static void TrailsESP() => PlayerESPRuntime.TrailsESP();

    [MenuDisable("Trails ESP")]
    public static void StopTrailsESP() => PlayerESPRuntime.ClearTrailsESP();

    [MenuButton("Tag Gun", ButtonType.Toggle, Category = "Player", Order = 6, runOption = RunOption.Update, Description = "Aim at a player and attempt to tag them")]
    public static void TG()
    {
        if (!Ready() || GTPlayer.Instance == null || ControllerInputPoller.instance == null)
        {
            TGStopTagging();
            GunLib.Hide();
            return;
        }
        bool grip = InputLib.RightGrab;
        bool trigger = InputLib.RightTrigger;
        bool selected = GunLib.UpdateGun(GorillaTagger.Instance.rightHandTransform, grip, trigger, out Vector3 point);
        if (!grip || !trigger)
        {
            TGStopTagging();
            return;
        }
        if (Target == null && selected && GunLib.HasHit)
        {
            Collider collider = GunLib.LastHit.collider;
            VRRig? rig = collider != null ? collider.GetComponentInParent<VRRig>() : null;
            if (rig != null && !rig.isLocal && !TagManager.IsTagged(rig))
            {
                Target = rig;
                TagTimer = 0f;
                NextTagReport = 0f;
            }
        }
        if (Target == null) return;
        if (TagManager.IsTagged(Target))
        {
            TGStopTagging();
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            TagManager.Instance.AddInfected(Target.Creator);
            TGStopTagging();
            return;
        }
        if (!CanMoveHand())
        {
            TGStopTagging();
            return;
        }
        Vector3 targetPosition = Target.transform.position;
        MoveRig(targetPosition + Vector3.down * 3f);
        MoveHand(targetPosition);
        ReportTag(Target.Creator);
        TagTimer += Time.deltaTime;
        if (TagTimer >= TagTimeout) TGStopTagging();
    }

    [MenuEnable("Tag Gun")]
    public static void TGStart()
    {
        ButtonManager.SetEnabled("Tag Self", false);
        ButtonManager.SetEnabled("Tag All", false);
    }

    private static void TGStopTagging()
    {
        RestoreRig();
        Target = null;
        TagTimer = 0f;
        NextTagReport = 0f;
    }

    [MenuDisable("Tag Gun")]
    public static void TGStop()
    {
        TGStopTagging();
        GunLib.Cleanup();
    }

    [MenuButton("Tag Self", ButtonType.Toggle, Category = "Player", Order = 7, runOption = RunOption.Update, Description = "Attempt to tag yourself")]
    public static void TagSelf()
    {
        if (!Ready() || TagManager.IsTagged(VRRig.LocalRig))
        {
            ButtonManager.SetEnabled("Tag Self", false);
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            TagManager.Instance.AddInfected(NetworkSystem.Instance.LocalPlayer);
            ButtonManager.SetEnabled("Tag Self", false);
            return;
        }
        if (Time.time - SelfStartTime >= TagTimeout)
        {
            ButtonManager.SetEnabled("Tag Self", false);
            return;
        }
        VRRig? taggedPlayer = GetRemoteRigs().FirstOrDefault(rig => TagManager.IsTagged(rig));
        if (taggedPlayer == null || taggedPlayer.rightHand.rigTarget == null)
        {
            ButtonManager.SetEnabled("Tag Self", false);
            return;
        }
        MoveRig(taggedPlayer.rightHand.rigTarget.transform.position);
    }

    [MenuEnable("Tag Self")]
    public static void TagSelfStart()
    {
        ButtonManager.SetEnabled("Tag Gun", false);
        ButtonManager.SetEnabled("Tag All", false);
        SelfStartTime = Time.time;
    }

    [MenuDisable("Tag Self")]
    public static void TagSelfStop()
    {
        RestoreRig();
        SelfStartTime = 0f;
    }

    [MenuButton("Tag Aura", ButtonType.Toggle, Category = "Player", Order = 8, runOption = RunOption.Update, Description = "Increase your tag reach while tagged")]
    public static void TagAura()
    {
        if (!Ready() || !TagManager.IsTagged(VRRig.LocalRig))
        {
            RestoreTagAura();
            return;
        }
        if (!AuraModified)
        {
            PreviousTagDistance = GorillaTagger.Instance.maxTagDistance;
            AuraModified = true;
        }
        GorillaTagger.Instance.maxTagDistance = float.MaxValue;
    }

    [MenuDisable("Tag Aura")]
    public static void TagAuraStop() => RestoreTagAura();

    private static void RestoreTagAura()
    {
        if (AuraModified && GorillaTagger.Instance != null) GorillaTagger.Instance.maxTagDistance = PreviousTagDistance;
        AuraModified = false;
    }

    [MenuButton("Tag All", ButtonType.Toggle, Category = "Player", Order = 9, runOption = RunOption.Update, Description = "Attempt to tag all untagged players")]
    public static void TagAll()
    {
        if (!Ready() || !TagManager.IsTagged(VRRig.LocalRig))
        {
            ButtonManager.SetEnabled("Tag All", false);
            return;
        }
        if (AllTargets == null)
        {
            AllTargets = GetRemoteRigs().Where(rig => !TagManager.IsTagged(rig)).ToList();
            TargetIndex = 0;
            TargetStartTime = Time.time;
        }
        if (TargetIndex >= AllTargets.Count)
        {
            ButtonManager.SetEnabled("Tag All", false);
            return;
        }

        VRRig rig = AllTargets[TargetIndex];
        if (rig == null || TagManager.IsTagged(rig))
        {
            NextTarget();
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            TagManager.Instance.AddInfected(rig.Creator);
            NextTarget();
            return;
        }
        if (Time.time - TargetStartTime >= TargetTimeout)
        {
            NextTarget();
            return;
        }
        if (!CanMoveHand())
        {
            ButtonManager.SetEnabled("Tag All", false);
            return;
        }
        Vector3 position = rig.transform.position;
        MoveRig(position + Vector3.down * 3f);
        MoveHand(position);
        ReportTag(rig.Creator);
    }

    [MenuEnable("Tag All")]
    public static void TagAllStart()
    {
        ButtonManager.SetEnabled("Tag Gun", false);
        ButtonManager.SetEnabled("Tag Self", false);
        AllTargets = null;
        TargetIndex = 0;
        TargetStartTime = Time.time;
        NextTagReport = 0f;
    }

    [MenuDisable("Tag All")]
    public static void TagAllStop()
    {
        RestoreRig();
        AllTargets = null;
        TargetIndex = 0;
        TargetStartTime = 0f;
        NextTagReport = 0f;
    }

    private static void NextTarget()
    {
        TargetIndex++;
        TargetStartTime = Time.time;
        NextTagReport = 0f;
    }

    private static IEnumerable<VRRig> GetRemoteRigs() => Resources.FindObjectsOfTypeAll<VRRig>().Where(rig => rig != null && rig.gameObject.activeInHierarchy && !rig.isLocal);

    private static bool Ready()
    {
        return PhotonNetwork.InRoom && NetworkSystem.Instance != null && GorillaTagger.Instance != null && VRRig.LocalRig != null;
    }

    private static bool CanMoveHand()
    {
        return GTPlayer.Instance != null && VRRig.LocalRig != null && GTPlayer.Instance.LeftHand.controllerTransform != null && VRRig.LocalRig.leftHand.rigTarget != null;
    }

    private static void MoveRig(Vector3 position)
    {
        if (VRRig.LocalRig == null)  return;
        if (!RigDisabled)
        {
            PreviousRigState = VRRig.LocalRig.enabled;
            RigDisabled = true;
        }
        VRRig.LocalRig.enabled = false;
        VRRig.LocalRig.transform.position = position;
    }

    private static void MoveHand(Vector3 position)
    {
        if (!CanMoveHand()) return;
        Transform controller = GTPlayer.Instance.LeftHand.controllerTransform;
        Transform rigHand = VRRig.LocalRig.leftHand.rigTarget.transform;
        if (!HandMoved)
        {
            PreviousControllerHand = controller.position;
            PreviousRigHand = rigHand.position;
            HandMoved = true;
        }
        controller.position = position;
        rigHand.position = position;
    }

    private static void ReportTag(NetPlayer player)
    {
        if (player == null || Time.time < NextTagReport) return;
        GameMode.ReportTag(player);
        NextTagReport = Time.time + TagReportInterval;
    }

    private static void RestoreRig()
    {
        if (HandMoved && CanMoveHand())
        {
            GTPlayer.Instance.LeftHand.controllerTransform.position = PreviousControllerHand;
            VRRig.LocalRig.leftHand.rigTarget.transform.position = PreviousRigHand;
        }
        if (RigDisabled && VRRig.LocalRig != null) VRRig.LocalRig.enabled = PreviousRigState;
        RigDisabled = false;
        HandMoved = false;
    }
}
