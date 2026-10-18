using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class Griddy : RigBehaviour
{
    protected override void Update()
    {
        if (!RigInput.Instance.RightPrimary.IsPressed)
        {
            if (!RigRuntime.IsRigEnabled)
                RigRuntime.ToggleRig(true);

            return;
        }

        RigRuntime.ToggleRig(false);

        Vector3 bodyOffset = VRRig.LocalRig.transform.forward * (5f * Time.deltaTime);
        RigRuntime.RigPosition += bodyOffset;
        VRRig.LocalRig.head.rigTarget.transform.rotation = VRRig.LocalRig.transform.rotation;

        VRRig.LocalRig.leftHand.rigTarget.transform.position = VRRig.LocalRig.transform.position + VRRig.LocalRig.transform.right * -0.33f + VRRig.LocalRig.transform.forward * (0.5f * Mathf.Cos(Time.frameCount / 10f)) + VRRig.LocalRig.transform.up * (-0.5f * Mathf.Abs(Mathf.Sin(Time.frameCount / 10f)));

        VRRig.LocalRig.rightHand.rigTarget.transform.position = VRRig.LocalRig.transform.position + VRRig.LocalRig.transform.right * 0.33f + VRRig.LocalRig.transform.forward * (0.5f * Mathf.Cos(Time.frameCount / 10f)) + VRRig.LocalRig.transform.up * (-0.5f * Mathf.Abs(Mathf.Sin(Time.frameCount / 10f)));

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation = VRRig.LocalRig.transform.rotation;
        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = VRRig.LocalRig.transform.rotation;

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation *= Quaternion.Euler(VRRig.LocalRig.leftHand.trackingRotationOffset);

        VRRig.LocalRig.rightHand.rigTarget.transform.rotation *= Quaternion.Euler(VRRig.LocalRig.rightHand.trackingRotationOffset);
    }
}