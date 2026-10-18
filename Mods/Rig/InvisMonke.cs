using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class InvisMonke : RigBehaviour
{
    protected override void Update()
    {
        if (!RigInput.Instance.RightSecondary.WasPressed)
            return;

        RigRuntime.ToggleRig(!RigRuntime.IsRigEnabled, new Vector3(999f, 999f, 999f));
    }

    protected override void OnDisable() => RigRuntime.ToggleRig(true);
}