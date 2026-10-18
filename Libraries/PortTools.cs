using ChudOS.Core.Managers;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Libraries;

internal static class PortTools
{
    private static Material? lineMaterial;
    internal static bool Ready => GTPlayer.Instance != null && GorillaTagger.Instance != null && GorillaTagger.Instance.rigidbody != null && ControllerInputPoller.instance != null;
    internal static void Register(Action cleanup, Action? tick = null) => PortLifetime.Register(cleanup, tick);

    internal static void ClaimPointer(string selected)
    {
        foreach (string name in PointerButtons)
            if (name != selected) ButtonManager.SetEnabled(name, false);
    }
    private static readonly string[] PointerButtons = { "Teleport Gun", "Draw Gun", "Tag Gun" };

    internal static void Teleport(Vector3 bodyPosition)
    {
        GTPlayer player = GTPlayer.Instance;
        player.TeleportTo(player.transform.position + bodyPosition - player.bodyCollider.transform.position, player.transform.rotation, false);
        GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
    }

    internal static LineRenderer Line(string name, float width, int count = 2)
    {
        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader == null) throw new InvalidOperationException("No line shader is available.");
            lineMaterial = new Material(shader) { name = "ChudOS Port Lines" };
        }
        GameObject obj = new(name);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.sharedMaterial = lineMaterial;
        line.useWorldSpace = true;
        line.positionCount = count;
        line.startWidth = line.endWidth = width;
        line.numCapVertices = line.numCornerVertices = 3;
        line.startColor = line.endColor = new Color(0.65f, 0.15f, 1f);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        return line;
    }

    internal static void Release()
    {
        if (lineMaterial != null) Object.Destroy(lineMaterial);
        lineMaterial = null;
    }
}

internal sealed class PortLifetime : MonoBehaviour
{
    private static PortLifetime? instance;
    private readonly HashSet<Action> cleanup = new();
    private readonly HashSet<Action> ticks = new();

    internal static void Register(Action action, Action? tick)
    {
        if (instance == null)
        {
            ChudOS.Core.Main main = Object.FindFirstObjectByType<ChudOS.Core.Main>();
            if (main == null) throw new InvalidOperationException("ChudOS runtime is unavailable.");
            instance = main.GetComponent<PortLifetime>() ?? main.gameObject.AddComponent<PortLifetime>();
        }
        instance.cleanup.Add(action);
        if (tick != null) instance.ticks.Add(tick);
    }

    private void LateUpdate()
    {
        foreach (Action tick in ticks) tick();
    }

    private void OnDestroy()
    {
        foreach (Action action in cleanup)
        {
            try { action(); }
            catch (Exception ex) { Debug.LogException(ex); }
        }
        cleanup.Clear();
        ticks.Clear();
        PortTools.Release();
        if (instance == this) instance = null;
    }
}
