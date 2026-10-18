using ChudOS.Libraries;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class ShadowCloneGun : RigBehaviour
{
    private const float SideOffset = 0.9f;
    private VRRig? target;

    protected override void LateUpdate()
    {
        if (!GunLib.UpdateRigGun(GorillaTagger.Instance.rightHandTransform, InputLib.RightGrab, InputLib.RightTrigger, ref target) || target == null)
        {
            RigRuntime.ToggleRig(true);
            return;
        }

        RigRuntime.ToggleRig(false);
        VRRig local = VRRig.LocalRig;
        Vector3 offset = target.transform.right * SideOffset;
        local.transform.SetPositionAndRotation(target.transform.position + offset, target.transform.rotation);
        local.head.rigTarget.rotation = target.head.rigTarget.rotation;
        local.leftHand.rigTarget.SetPositionAndRotation(target.leftHand.rigTarget.position + offset, target.leftHand.rigTarget.rotation);
        local.rightHand.rigTarget.SetPositionAndRotation(target.rightHand.rigTarget.position + offset, target.rightHand.rigTarget.rotation);
        local.leftHand.calcT = target.leftHand.calcT;
        local.rightHand.calcT = target.rightHand.calcT;
    }

    protected override void OnDisable()
    {
        target = null;
        GunLib.Hide();
        RigRuntime.ToggleRig(true);
    }
}
