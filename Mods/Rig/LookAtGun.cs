using ChudOS.Libraries;

namespace ChudOS.Mods.Rig;

internal sealed class LookAtGun : RigBehaviour
{
    private VRRig? target;

    protected override void LateUpdate()
    {
        if (GunLib.UpdateRigGun(GorillaTagger.Instance.rightHandTransform, InputLib.RightGrab, InputLib.RightTrigger, ref target) && target != null)
            VRRig.LocalRig.head.rigTarget.LookAt(target.head.rigTarget);
    }

    protected override void OnDisable()
    {
        target = null;
        GunLib.Hide();
    }
}
