namespace ChudOS.Mods.Rig;

internal sealed class GhostMonke : RigBehaviour
{
    protected override void Update()
    {
        if (!RigInput.Instance.RightPrimary.WasPressed)
            return;

        RigRuntime.ToggleRig(!RigRuntime.IsRigEnabled);
    }

    protected override void OnDisable() => RigRuntime.ToggleRig(true);
}