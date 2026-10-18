using BepInEx;
using ChudOS.Core;
using ChudOS.Core.Managers.CustomBoards;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS;

[BepInPlugin(Constantss.GUID, Constantss.Name, Constantss.Version)]
public sealed class Plugin : BaseUnityPlugin
{
    private GameObject? Runtime;

    private void Awake()
    {
        PatchLoader.Apply();
        Runtime = new GameObject(Constantss.ObjectName);
        Runtime.AddComponent<Changetext>();
        Runtime.AddComponent<Main>();
        DontDestroyOnLoad(Runtime);
    }

    private void OnDestroy()
    {
        if (Runtime != null) Destroy(Runtime);
    }
}
