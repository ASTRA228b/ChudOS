using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Rig;

public static class MaxQuestScore
{
    private static bool enabled;
    private static NetworkSystem? subscribed;
    private static VRRig? scoreRig;
    private static int originalScore;

    [MenuButton("Max Quest Score", ButtonType.Toggle, Category = "Rig", Order = 16, runOption = RunOption.Once, Description = "Sets your rig's displayed quest score while enabled")]
    public static void Enable()
    {
        enabled = true;
        PortTools.Register(Cleanup, Maintain);
        Maintain();
    }

    private static void Maintain()
    {
        if (!enabled) return;
        if (subscribed != NetworkSystem.Instance)
        {
            if (subscribed != null) subscribed.OnJoinedRoomEvent -= Apply;
            subscribed = NetworkSystem.Instance;
            if (subscribed != null) subscribed.OnJoinedRoomEvent += Apply;
        }
        if (VRRig.LocalRig != null && scoreRig != VRRig.LocalRig) Apply();
    }

    private static void Apply()
    {
        if (!enabled || VRRig.LocalRig == null) return;
        if (scoreRig != VRRig.LocalRig)
        {
            RestoreScore();
            scoreRig = VRRig.LocalRig;
            originalScore = scoreRig.GetCurrentQuestScore();
        }
        scoreRig.SetQuestScore(int.MaxValue);
    }

    private static void RestoreScore()
    {
        if (scoreRig != null) scoreRig.SetQuestScore(originalScore);
        scoreRig = null;
    }

    [MenuDisable("Max Quest Score")]
    public static void Disable() => Cleanup();

    private static void Cleanup()
    {
        enabled = false;
        if (subscribed != null) subscribed.OnJoinedRoomEvent -= Apply;
        subscribed = null;
        RestoreScore();
    }
}
