using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Visuals;

public static class NewVisualMods
{
    private sealed class Prediction
    {
        internal LineRenderer Line = null!;
        internal Vector3 LastPosition, Velocity;
        internal bool Sampled;
    }
    private static readonly Dictionary<VRRig, Prediction> predictions = new();
    private static readonly List<VRRig> stale = new(16);
    private static readonly Dictionary<Transform, LineRenderer> triggers = new();
    private static readonly List<Transform> staleTriggers = new();
    private static readonly HashSet<Transform> activeTriggers = new();
    private static readonly HashSet<GameObject> cosmetics = new();
    private static readonly List<LineRenderer> strokes = new();
    private static LineRenderer? stroke;
    private static int strokePoints;
    private static Vector3 lastDrawPoint;
    private static float nextPredictionScan, nextTriggerScan, nextCosmeticScan;
    private static Transform? triggerRoot;

    [MenuButton("Jump Predictions", ButtonType.Toggle, Category = "Visuals", Order = 101, runOption = RunOption.FixedUpdate, Description = "Estimated future paths of moving remote players")]
    public static void JumpPredictions()
    {
        if (!PortTools.Ready || NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom)
        {
            StopJumpPredictions();
            return;
        }
        PortTools.Register(Cleanup);
        if (Time.unscaledTime >= nextPredictionScan)
        {
            nextPredictionScan = Time.unscaledTime + 0.5f;
            stale.Clear();
            foreach (var pair in predictions)
                if (pair.Key == null || VRRigCache.ActiveRigs == null || !VRRigCache.ActiveRigs.Contains(pair.Key)) stale.Add(pair.Key!);
            foreach (VRRig rig in stale)
            {
                if (predictions[rig].Line != null) Object.Destroy(predictions[rig].Line.gameObject);
                predictions.Remove(rig);
            }
            if (VRRigCache.ActiveRigs != null)
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                    if (rig != null && !rig.isLocal && !rig.isOfflineVRRig && rig != VRRig.LocalRig && !predictions.ContainsKey(rig))
                        predictions.Add(rig, new Prediction { Line = PortTools.Line("ChudOS Jump Prediction", 0.025f, 25) });
        }
        foreach (var pair in predictions)
        {
            VRRig rig = pair.Key;
            Prediction state = pair.Value;
            if (rig == null || state.Line == null) continue;
            Vector3 position = rig.syncPos;
            if (state.Sampled)
            {
                Vector3 measured = (position - state.LastPosition) / Time.fixedDeltaTime;
                state.Velocity = Vector3.Lerp(state.Velocity, measured, 0.35f);
            }
            state.LastPosition = position;
            state.Sampled = true;
            LineRenderer line = state.Line;
            line.enabled = state.Velocity.sqrMagnitude >= 2.25f;
            if (!line.enabled) continue;
            line.startColor = line.endColor = rig.playerColor;
            line.positionCount = 25;
            Vector3 velocity = state.Velocity;
            for (int i = 0; i < 25; i++)
            {
                line.SetPosition(i, position);
                Vector3 delta = velocity * 0.1f;
                if (i > 0 && delta.sqrMagnitude > 0.00001f && Physics.Raycast(position, delta.normalized, out RaycastHit hit, delta.magnitude, GTPlayer.Instance.locomotionEnabledLayers, QueryTriggerInteraction.Ignore))
                {
                    line.SetPosition(i, hit.point);
                    line.positionCount = i + 1;
                    break;
                }
                position += delta;
                velocity += Physics.gravity * 0.1f;
            }
        }
    }
    [MenuDisable("Jump Predictions")]
    public static void StopJumpPredictions()
    {
        foreach (var state in predictions.Values) if (state.Line != null) Object.Destroy(state.Line.gameObject);
        predictions.Clear(); stale.Clear(); nextPredictionScan = 0f;
    }

    [MenuButton("Show Room Triggers", ButtonType.Toggle, Category = "Visuals", Order = 102, Description = "Outlines room transition trigger volumes")]
    public static void ShowRoomTriggers()
    {
        PortTools.Register(Cleanup);
        if (Time.unscaledTime < nextTriggerScan) return;
        nextTriggerScan = Time.unscaledTime + 1f;
        if (triggerRoot == null)
        {
            GameObject root = GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab");
            if (root != null) triggerRoot = root.transform;
        }
        activeTriggers.Clear();
        if (triggerRoot != null)
            foreach (Transform child in triggerRoot)
            {
                if (!child.gameObject.activeInHierarchy) continue;
                activeTriggers.Add(child);
                if (!triggers.TryGetValue(child, out LineRenderer? line) || line == null)
                    triggers[child] = line = PortTools.Line("ChudOS Room Trigger", 0.025f, 16);
                Collider collider = child.GetComponent<Collider>();
                Bounds b = collider != null ? collider.bounds : new Bounds(child.position, child.lossyScale);
                Vector3 min = b.min, max = b.max;
                for (int i = 0; i < BoxPath.Length; i++)
                {
                    int c = BoxPath[i];
                    line.SetPosition(i, new Vector3((c & 1) == 0 ? min.x : max.x, (c & 2) == 0 ? min.y : max.y, (c & 4) == 0 ? min.z : max.z));
                }
                line.startColor = line.endColor = Color.red;
            }
        staleTriggers.Clear();
        foreach (var pair in triggers)
            if (pair.Key == null || !activeTriggers.Contains(pair.Key)) staleTriggers.Add(pair.Key!);
        foreach (Transform key in staleTriggers)
        {
            if (triggers[key] != null) Object.Destroy(triggers[key].gameObject);
            triggers.Remove(key);
        }
    }
    private static readonly int[] BoxPath = { 0, 1, 3, 2, 0, 4, 5, 1, 5, 7, 3, 7, 6, 2, 6, 4 };
    [MenuDisable("Show Room Triggers")]
    public static void StopRoomTriggers()
    {
        foreach (var line in triggers.Values) if (line != null) Object.Destroy(line.gameObject);
        triggers.Clear(); activeTriggers.Clear(); staleTriggers.Clear(); triggerRoot = null; nextTriggerScan = 0f;
    }

    [MenuEnable("Draw Gun")]
    public static void StartDrawGun() { PortTools.Register(Cleanup); PortTools.ClaimPointer("Draw Gun"); }

    [MenuButton("Draw Gun", ButtonType.Toggle, Category = "Visuals", Order = 103,Description = "Right grip aims; hold trigger to draw. Disable to clear")]
    public static void DrawGun()
    {
        if (!PortTools.Ready) return;
        bool draw = GunLib.UpdateGun(GorillaTagger.Instance.rightHandTransform, InputLib.RightGrab, InputLib.RightTrigger, out Vector3 point);
        if (!draw) { stroke = null; strokePoints = 0; return; }
        if (stroke == null || strokePoints >= 512)
        {
            if (strokes.Count >= 64)
            {
                if (strokes[0] != null) Object.Destroy(strokes[0].gameObject);
                strokes.RemoveAt(0);
            }
            stroke = PortTools.Line("ChudOS Draw Stroke", 0.03f, 0);
            strokes.Add(stroke);
            strokePoints = 0;
        }
        if (strokePoints > 0 && (point - lastDrawPoint).sqrMagnitude < 0.0025f) return;
        stroke.positionCount = ++strokePoints;
        stroke.SetPosition(strokePoints - 1, point);
        lastDrawPoint = point;
    }
    [MenuDisable("Draw Gun")]
    public static void StopDrawGun()
    {
        foreach (var line in strokes) if (line != null) Object.Destroy(line.gameObject);
        strokes.Clear(); stroke = null; strokePoints = 0; GunLib.Hide();
    }

    [MenuButton("Hide Head Cosmetics", ButtonType.Toggle, Category = "Visuals", Order = 104, Description = "Hides your local head cosmetics while enabled")]
    public static void HideHeadCosmetics()
    {
        if (VRRig.LocalRig == null || Time.unscaledTime < nextCosmeticScan) return;
        PortTools.Register(Cleanup);
        nextCosmeticScan = Time.unscaledTime + 0.5f;
        VRRig rig = VRRig.LocalRig;
        Transform? headCosmetics = rig.mainCamera != null ? rig.mainCamera.transform.Find("HeadCosmetics") : null;
        Transform head = rig.transform.Find("rig/head");
        foreach (GameObject cosmetic in rig.cosmetics)
            if (cosmetic != null && cosmetic.activeSelf && ((headCosmetics != null && cosmetic.transform.parent == headCosmetics) || (head != null && cosmetic.transform.parent == head)))
            {
                cosmetics.Add(cosmetic);
                cosmetic.SetActive(false);
            }
    }
    [MenuDisable("Hide Head Cosmetics")]
    public static void RestoreHeadCosmetics()
    {
        foreach (GameObject cosmetic in cosmetics) if (cosmetic != null) cosmetic.SetActive(true);
        cosmetics.Clear(); nextCosmeticScan = 0f;
    }

    private static void Cleanup()
    {
        StopJumpPredictions(); StopRoomTriggers(); StopDrawGun(); RestoreHeadCosmetics();
    }
}
