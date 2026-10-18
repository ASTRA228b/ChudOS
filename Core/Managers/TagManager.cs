using Photon.Pun;

namespace ChudOS.Core.Managers;

public sealed class TagManager
{
    public static TagManager Instance { get; } = new();
    private readonly HashSet<VRRig> taggedRigs = new();
    private TagManager() { }

    public HashSet<VRRig> TaggedRigs
    {
        get
        {
            taggedRigs.Clear();
            if (VRRigCache.ActiveRigs != null)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (IsTagged(rig)) taggedRigs.Add(rig);
                }
            }
            return taggedRigs;
        }
    }

    public static bool IsTagged(VRRig? rig)
    {
        if (rig == null) return false;
        return rig.setMatIndex == 1 || rig.setMatIndex == 2 || rig.setMatIndex == 11;
    }

    public void AddInfected(NetPlayer player)
    {
        if (player == null || !PhotonNetwork.InRoom || NetworkSystem.Instance == null || !NetworkSystem.Instance.IsMasterClient) return;
        GorillaTagManager? manager = GorillaGameManager.instance as GorillaTagManager;
        if (manager == null) return;
        if (manager.isCurrentlyTag)
        {
            if (manager.currentIt != player)
            {
                var changeCurrentItMethod = typeof(GorillaTagManager).GetMethod("ChangeCurrentIt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                changeCurrentItMethod?.Invoke(manager, new object[] { player, true });
            }
        }
        else if (manager.currentInfected != null && !manager.currentInfected.Contains(player))
        {
            manager.AddInfectedPlayer(player);
        }
    }
}
