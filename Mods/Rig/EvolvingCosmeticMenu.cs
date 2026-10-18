using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Rig;

public static class EvolvingCosmeticMenu
{
    [MenuButton("Evolving Cosmetic", ButtonType.Press, Category = "Rig", Order = 17, Description = "Manage stages, local age and stage cycling for your worn evolving cosmetics")]
    public static void Open()
    {
        ChudOS.Core.Main main = Object.FindFirstObjectByType<ChudOS.Core.Main>();
        if (main == null) return;
        EvolvingCosmeticManager manager = main.GetComponent<EvolvingCosmeticManager>() ?? main.gameObject.AddComponent<EvolvingCosmeticManager>();
        manager.Open();
    }
}
