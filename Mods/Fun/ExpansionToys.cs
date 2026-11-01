// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Fun;

internal sealed class ExpansionToys : BetaFeature
{
    private readonly List<Rigidbody> pieces = new();
    private readonly List<GameObject> pads = new();
    private readonly List<Vector3> pins = new();
    private readonly List<int> sequence = new();
    private readonly Dictionary<SlingshotProjectile, Vector3> shots = new();
    private Rigidbody? held, mainBall, drone;
    private Rigidbody? leftProxy, rightProxy;
    private Vector3 basePoint, rightVelocity, lastRight, goal, bridgeFirst;
    private LineRenderer? rope;
    private TextMesh? scoreLabel;
    private AudioSource? audio;
    private AudioClip? note;
    private Camera? droneCamera;
    private RenderTexture? droneTexture;
    private bool wasGrip, xInput, aInput, bInput, wasGrounded, bridgeStart, scored, demonstrating;
    private int score, target, memoryStep, demonstration, shape, placements, paintedPad = -2, touchedPad = -1;
    private Color paintedTheme;
    private float nextBeat, began, lastThrow, nextShotScan, nextScore, bpm = 100f;
    private Rect window = new(60, 90, 340, 300);

    protected override void Setup()
    {
        basePoint = Floor(2f) + Vector3.up * 0.08f; lastRight = CreativeTools.Right.position;
        scoreLabel = Label(Mode + "\nA/Space: reset | RG/RMB: grab", basePoint + Vector3.up * 1.5f, 0.025f);
        score = placements = 0; scored = false; began = Time.unscaledTime; paintedPad = -2; touchedPad = -1;
        if (Mode is "Rhythm Drums" or "Metronome" or "Theremin")
        {
            audio = gameObject.AddComponent<AudioSource>(); audio.spatialBlend = 0f; audio.playOnAwake = false;
            note = Tone(Mode == "Theremin"); audio.clip = note; audio.loop = Mode == "Theremin"; if (audio.loop) audio.Play();
        }
        if (Mode == "Metronome" || Mode == "Theremin") return;
        MakeHands();
        switch (Mode)
        {
            case "Physics Sandbox":
                Table(); Piece(PrimitiveType.Cube, basePoint + Vector3.up * 0.5f, Vector3.one * 0.25f); break;
            case "Domino Builder":
                Table(); for (int i = 0; i < 14; i++) Piece(PrimitiveType.Cube, basePoint + Forward * (i * 0.16f - 1f) + Vector3.up * 0.25f, new Vector3(0.18f, 0.5f, 0.045f)); break;
            case "Marble Run":
                Table(); for (int i = 0; i < 4; i++) { var ramp = Shape("Ramp", PrimitiveType.Cube, basePoint + Vector3.up * (0.25f + i * 0.35f) + Forward * (i * 0.7f - 1f), new Vector3(0.8f, 0.08f, 1f), true); ramp.transform.rotation = Quaternion.LookRotation(Forward) * Quaternion.Euler(18f, 0, 0); }
                mainBall = Piece(PrimitiveType.Sphere, basePoint + Vector3.up * 2f + Forward * 1.1f, Vector3.one * 0.12f); break;
            case "Force Lab":
                Table(); for (int i = 0; i < 10; i++) Piece(PrimitiveType.Sphere, basePoint + new Vector3((i % 5 - 2) * 0.3f, 0.3f, i / 5 * 0.4f), Vector3.one * 0.15f); break;
            case "Rope Bridge Builder":
                scoreLabel!.text = "Rope Bridge: aim and click two anchors (1–10m apart)."; break;
            case "RC Drone":
                drone = Piece(PrimitiveType.Cube, CreativeTools.Head.position + Forward, new Vector3(0.35f, 0.08f, 0.35f)); drone.useGravity = false;
                droneCamera = drone.gameObject.AddComponent<Camera>(); droneCamera.stereoTargetEye = StereoTargetEyeMask.None; droneCamera.nearClipPlane = 0.1f; droneCamera.farClipPlane = 100f;
                droneTexture = new RenderTexture(256, 192, 16); droneTexture.Create(); droneCamera.targetTexture = droneTexture;
                scoreLabel!.text = "RC Drone: arrows/left stick steer; A up, LG down."; break;
            case "Boomerang":
                mainBall = Piece(PrimitiveType.Cube, CreativeTools.Right.position + Forward * 0.3f, new Vector3(0.35f, 0.04f, 0.08f)); break;
            case "Paper Planes":
                mainBall = Piece(PrimitiveType.Cube, CreativeTools.Right.position + Forward * 0.3f, new Vector3(0.4f, 0.025f, 0.3f)); break;
            case "Yo-Yo":
                mainBall = Piece(PrimitiveType.Cylinder, CreativeTools.Right.position - Vector3.up * 0.7f, new Vector3(0.14f, 0.035f, 0.14f)); rope = Line("Yo-Yo string"); break;
            case "Juggling Practice":
                Table(); for (int i = 0; i < 3; i++) Piece(PrimitiveType.Sphere, basePoint + new Vector3((i - 1) * 0.25f, 0.3f, 0), Vector3.one * 0.16f); break;
            case "Mini Bowling":
                Table(2f, 5f); mainBall = Piece(PrimitiveType.Sphere, basePoint - Forward * 1.7f + Vector3.up * 0.2f, Vector3.one * 0.3f);
                for (int row = 0; row < 4; row++) for (int col = 0; col <= row; col++) { var p = basePoint + Forward * (1f + row * 0.25f) + Vector3.Cross(Vector3.up, Forward) * ((col - row * 0.5f) * 0.25f); Piece(PrimitiveType.Capsule, p + Vector3.up * 0.25f, new Vector3(0.12f, 0.25f, 0.12f)); pins.Add(p); } break;
            case "Ring Toss":
                Table(); goal = basePoint + Forward * 0.8f + Vector3.up * 0.25f; Shape("Peg", PrimitiveType.Cylinder, goal, new Vector3(0.05f, 0.3f, 0.05f), true);
                for (int i = 0; i < 3; i++) Ring(basePoint - Forward * 0.6f + Vector3.up * (0.2f + i * 0.1f)); break;
            case "Virtual Darts":
                goal = basePoint + Forward * 1.4f + Vector3.up * 1.2f;
                var board = Shape("Dartboard", PrimitiveType.Cylinder, goal, new Vector3(1f, 0.04f, 1f), true); board.transform.rotation = Quaternion.FromToRotation(Vector3.up, -Forward);
                for (int i = 0; i < 3; i++) Piece(PrimitiveType.Capsule, CreativeTools.Right.position + Vector3.up * (i * 0.15f), new Vector3(0.035f, 0.15f, 0.035f)); break;
            case "Cup Stacking":
                Table(); for (int i = 0; i < 6; i++) Piece(PrimitiveType.Cylinder, basePoint + new Vector3((i % 3 - 1) * 0.3f, 0.18f, i / 3 * 0.3f), new Vector3(0.22f, 0.16f, 0.22f)); break;
            case "Air Hockey":
                Table(2f, 4f); Shape("Left Rail", PrimitiveType.Cube, basePoint - Vector3.Cross(Vector3.up, Forward) + Vector3.up * 0.12f, new Vector3(0.05f, 0.25f, 4f), true).transform.rotation = Quaternion.LookRotation(Forward);
                Shape("Right Rail", PrimitiveType.Cube, basePoint + Vector3.Cross(Vector3.up, Forward) + Vector3.up * 0.12f, new Vector3(0.05f, 0.25f, 4f), true).transform.rotation = Quaternion.LookRotation(Forward);
                mainBall = Piece(PrimitiveType.Cylinder, basePoint + Vector3.up * 0.1f, new Vector3(0.18f, 0.04f, 0.18f)); mainBall.constraints = RigidbodyConstraints.FreezeRotation; break;
            case "Pocket Basketball":
                Table(3f, 4f); goal = basePoint + Forward * 1.1f + Vector3.up * 1.25f;
                var hoop = Ring(goal, true); hoop.isKinematic = true;
                Shape("Backboard", PrimitiveType.Cube, goal + Forward * 0.3f + Vector3.up * 0.15f, new Vector3(0.9f, 0.7f, 0.05f), true).transform.rotation = Quaternion.LookRotation(Forward);
                mainBall = Piece(PrimitiveType.Sphere, basePoint - Forward + Vector3.up * 0.4f, Vector3.one * 0.22f); break;
            case "Rhythm Drums": case "Reaction Trainer": case "Memory Pads":
                for (int i = 0; i < 4; i++) pads.Add(Shape("Touch Pad", PrimitiveType.Cube, CreativeTools.Head.position + Forward * 0.65f + Vector3.Cross(Vector3.up, Forward) * ((i - 1.5f) * 0.25f) - Vector3.up * 0.35f, new Vector3(0.2f, 0.12f, 0.2f)));
                target = Mode == "Memory Pads" ? -1 : UnityEngine.Random.Range(0, 4); nextBeat = Time.unscaledTime + 1f; sequence.Add(UnityEngine.Random.Range(0, 4)); demonstrating = Mode == "Memory Pads"; break;
            case "Precision Landing Challenge":
                goal = basePoint; Shape("Landing Pad", PrimitiveType.Cube, goal, new Vector3(0.8f, 0.08f, 0.8f), true); break;
            case "Balance Beam Challenge":
                Shape("Balance Beam", PrimitiveType.Cube, basePoint + Vector3.up * 0.35f, new Vector3(0.14f, 0.12f, 4f), true).transform.rotation = Quaternion.LookRotation(Forward);
                goal = basePoint + Forward * 1.8f + Vector3.up * 0.4f; break;
            case "Laser Maze":
                for (int i = 0; i < 8; i++)
                {
                    var laser = Line("Maze Laser"); Vector3 middle = basePoint + Forward * (i * 0.5f - 1.5f) + Vector3.up * (0.4f + i % 3 * 0.25f), side = Vector3.Cross(Vector3.up, Forward) * 0.8f;
                    laser.SetPosition(0, middle - side); laser.SetPosition(1, middle + side); pads.Add(laser.gameObject);
                }
                goal = basePoint + Forward * 2.5f; break;
            case "Throwable Target Range":
                for (int i = 0; i < 4; i++) pads.Add(Shape("Throwable Target", PrimitiveType.Sphere, basePoint + Forward * (1.5f + i * 0.8f) + Vector3.up * (0.4f + i % 2 * 0.5f), Vector3.one * 0.45f)); break;
        }
    }
    private void Table(float width = 2f, float length = 2.5f)
    {
        var table = Shape("Practice Surface", PrimitiveType.Cube, basePoint - Vector3.up * 0.06f, new Vector3(width, 0.12f, length), true); table.transform.rotation = Quaternion.LookRotation(Forward);
    }
    private Rigidbody Piece(PrimitiveType type, Vector3 position, Vector3 size)
    {
        var obj = Shape("Practice Prop", type, position, size, true, true); var body = obj.GetComponent<Rigidbody>();
        body.mass = ExpansionSettings.PropMass; pieces.Add(body); return body;
    }
    private Rigidbody Ring(Vector3 position, bool wide = false)
    {
        var obj = new GameObject("ChudOS Ring"); obj.transform.SetParent(transform); obj.transform.position = position; Objects.Add(obj);
        float radius = wide ? 0.28f : 0.17f;
        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI / 6f; var segment = Shape("Ring Segment", PrimitiveType.Cube, position + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius, new Vector3(0.04f, 0.04f, radius * 0.55f), true);
            segment.transform.SetParent(obj.transform, true); segment.transform.rotation = Quaternion.Euler(0, -a * Mathf.Rad2Deg, 0);
        }
        var body = obj.AddComponent<Rigidbody>(); body.mass = ExpansionSettings.PropMass; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; pieces.Add(body); return body;
    }
    private void MakeHands()
    {
        leftProxy = Hand("Left Practice Hand", CreativeTools.Left.position); rightProxy = Hand("Right Practice Hand", CreativeTools.Right.position);
    }
    private Rigidbody Hand(string name, Vector3 position)
    {
        var obj = Shape(name, PrimitiveType.Sphere, position, Vector3.one * 0.13f, true); obj.GetComponent<Renderer>().enabled = false;
        Physics.IgnoreCollision(obj.GetComponent<Collider>(), GorillaLocomotion.GTPlayer.Instance.bodyCollider);
        var rb = obj.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false; return rb;
    }
    protected override void Tick()
    {
        bool reset = CreativeTools.Pressed(A, ref aInput);
        bool alternate = CreativeTools.Pressed(X, ref xInput);
        bool cycle = CreativeTools.Pressed(B, ref bInput);
        if (reset && Mode != "RC Drone") { ResetScene(); return; }
        if (cycle && Mode == "Physics Sandbox") shape = (shape + 1) % 3;
        if (Mode is "Physics Sandbox" or "Domino Builder" or "Marble Run")
        {
            if (Place(out var point, out var normal) && placements < ExpansionSettings.GeometryLimit && pieces.Count < ExpansionSettings.GeometryLimit)
            {
                placements++;
                if (Mode == "Marble Run") { var ramp = Shape("Placed Ramp", PrimitiveType.Cube, point + normal * 0.06f, new Vector3(0.7f, 0.08f, 0.9f), true); ramp.transform.rotation = Quaternion.LookRotation(Forward) * Quaternion.Euler(20, 0, 0); }
                else Piece(Mode == "Domino Builder" || shape == 0 ? PrimitiveType.Cube : shape == 1 ? PrimitiveType.Sphere : PrimitiveType.Capsule, point + normal * 0.25f, Mode == "Domino Builder" ? new Vector3(0.18f, 0.5f, 0.045f) : Vector3.one * 0.25f);
            }
            if (Mode == "Domino Builder" && alternate && pieces.Count > 0) pieces[0].AddForce(Forward * 0.35f, ForceMode.Impulse);
            if (Mode == "Physics Sandbox" && alternate && pieces.Count >= 2)
            {
                Rigidbody last = pieces[^1], prior = pieces[^2];
                if (last.GetComponent<FixedJoint>() == null) { var joint = last.gameObject.AddComponent<FixedJoint>(); joint.connectedBody = prior; joint.breakForce = 100f; }
            }
        }
        if (Mode == "Rope Bridge Builder" && Place(out var bridgePoint, out _) && pieces.Count == 0)
        {
            if (!bridgeStart) { bridgeFirst = bridgePoint; bridgeStart = true; CreativeTools.Notice("First bridge anchor set. Select the second anchor."); }
            else if (Vector3.Distance(bridgeFirst, bridgePoint) > 1f) BuildBridge(bridgeFirst, bridgePoint);
        }
        if (Mode == "Rhythm Drums" || Mode == "Reaction Trainer" || Mode == "Memory Pads") Pads();
        if (Mode == "Metronome" && Time.unscaledTime >= nextBeat)
        { nextBeat = Time.unscaledTime + 60f / bpm; if (audio != null && note != null) audio.PlayOneShot(note, ExpansionSettings.AudioVolume); }
        if (Mode == "Theremin" && audio != null)
        { audio.volume = Mathf.Clamp01(Vector3.Distance(CreativeTools.Left.position, CreativeTools.Head.position) - 0.15f) * ExpansionSettings.AudioVolume; audio.pitch = Mathf.Clamp((CreativeTools.Right.position.y - CreativeTools.Position.y + 0.5f) * 1.8f, 0.3f, 3f); }
        if (Mode == "Throwable Target Range" && Time.unscaledTime >= nextShotScan) ScanShots();
        if (Mode == "Precision Landing Challenge")
        {
            bool grounded = CreativeTools.Ground(out var ground);
            if (grounded && !wasGrounded && (ground.point - goal).sqrMagnitude < 1f) { score++; CreativeTools.Notice("Landing " + score + ": " + Vector3.Distance(ground.point, goal).ToString("F2") + "m from center."); }
            wasGrounded = grounded;
        }
        if (Mode == "Balance Beam Challenge" && !scored && Vector3.Distance(CreativeTools.Position, goal + Vector3.up * 0.4f) < 0.65f)
        { scored = true; CreativeTools.Notice("Beam complete: " + (Time.unscaledTime - began).ToString("F2") + "s"); }
        if (Mode == "Laser Maze")
        {
            bool touched = pads.Any(p => p != null && DistanceToSegment(CreativeTools.Position, p.GetComponent<LineRenderer>().GetPosition(0), p.GetComponent<LineRenderer>().GetPosition(1)) < 0.22f);
            if (touched && Time.unscaledTime > nextBeat) { nextBeat = Time.unscaledTime + 1f; score++; CreativeTools.Notice("Laser touches: " + score); }
            if (!scored && Vector3.Distance(CreativeTools.Position, goal + Vector3.up * 0.4f) < 0.6f) { scored = true; CreativeTools.Notice("Maze finished: " + score + " touches."); }
        }
        if (scoreLabel != null && View != null) scoreLabel.transform.rotation = Quaternion.LookRotation(scoreLabel.transform.position - View.transform.position);
    }
    protected override void PhysicsTick()
    {
        float dt = Mathf.Max(Time.fixedDeltaTime, 0.001f); Vector3 hand = CreativeTools.Right.position;
        rightVelocity = Vector3.ClampMagnitude((hand - lastRight) / dt, 20f); lastRight = hand;
        if (leftProxy != null) leftProxy.MovePosition(CreativeTools.Left.position);
        if (rightProxy != null) rightProxy.MovePosition(hand);
        bool grip = RG;
        if (grip && !wasGrip)
        {
            float nearest = 0.35f;
            foreach (var piece in pieces) if (piece != null && !piece.isKinematic && Vector3.Distance(piece.position, hand) < nearest) { nearest = Vector3.Distance(piece.position, hand); held = piece; }
            if (held != null) { held.isKinematic = true; foreach (var collider in held.GetComponentsInChildren<Collider>()) collider.enabled = false; }
        }
        if (held != null)
        {
            if (grip) { held.MovePosition(hand); held.MoveRotation(CreativeTools.Right.rotation); }
            else
            {
                held.isKinematic = false; foreach (var collider in held.GetComponentsInChildren<Collider>()) collider.enabled = true;
                held.linearVelocity = Vector3.ClampMagnitude(rightVelocity, ExpansionSettings.ToyPower); lastThrow = Time.unscaledTime; held = null;
            }
        }
        wasGrip = grip;
        foreach (var piece in pieces) if (piece != null && piece != held && !piece.isKinematic)
        {
            if ((piece.position - basePoint).sqrMagnitude > 900f) { piece.position = basePoint + Vector3.up; piece.linearVelocity = Vector3.zero; piece.angularVelocity = Vector3.zero; }
            if (Mode == "Force Lab")
            {
                Vector3 delta = (LG ? CreativeTools.Left.position : hand) - piece.position;
                if ((LG || RG) && delta.sqrMagnitude < 25f) piece.AddForce(delta.normalized * (LG ? 1f : -1f) * ExpansionSettings.ToyPower, ForceMode.Acceleration);
            }
        }
        if (Mode == "RC Drone" && drone != null && drone != held)
        {
            Vector2 axis = Vector2.zero; InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out axis);
            var k = Keyboard.current; if (k != null) axis += new Vector2((k.rightArrowKey.isPressed ? 1 : 0) - (k.leftArrowKey.isPressed ? 1 : 0), (k.upArrowKey.isPressed ? 1 : 0) - (k.downArrowKey.isPressed ? 1 : 0));
            Vector3 direction = Forward * axis.y + Vector3.Cross(Vector3.up, Forward) * axis.x + Vector3.up * ((A ? 1 : 0) - (LG ? 1 : 0));
            drone.linearVelocity = Vector3.MoveTowards(drone.linearVelocity, Vector3.ClampMagnitude(direction, 1f) * ExpansionSettings.ToyPower * 0.5f, dt * 6f);
        }
        if (mainBall != null && mainBall != held && !mainBall.isKinematic)
        {
            if (Mode == "Boomerang" && lastThrow > 0f && Time.unscaledTime - lastThrow > 0.7f)
            { mainBall.linearVelocity = Vector3.MoveTowards(mainBall.linearVelocity, (hand - mainBall.position) * 4f, dt * 18f); mainBall.angularVelocity = Vector3.up * 20f; }
            if (Mode == "Paper Planes") { mainBall.AddForce(-Physics.gravity * 0.7f - mainBall.linearVelocity * 0.1f, ForceMode.Acceleration); if (mainBall.linearVelocity.sqrMagnitude > 1f) mainBall.MoveRotation(Quaternion.Slerp(mainBall.rotation, Quaternion.LookRotation(mainBall.linearVelocity), dt * 2f)); }
            if (Mode == "Yo-Yo")
            { Vector3 delta = hand - mainBall.position; if (delta.magnitude > 0.8f) mainBall.AddForce(delta.normalized * (delta.magnitude - 0.8f) * 50f - mainBall.linearVelocity * 2f, ForceMode.Acceleration); if (rope != null) { rope.SetPosition(0, hand); rope.SetPosition(1, mainBall.position); } }
            if (Mode == "Air Hockey" && Mathf.Abs(Vector3.Dot(mainBall.position - basePoint, Forward)) > 2f)
            { score++; mainBall.position = basePoint + Vector3.up * 0.12f; mainBall.linearVelocity = Vector3.zero; CreativeTools.Notice("Hockey goals: " + score); }
            if (Mode == "Pocket Basketball")
            { Vector3 delta = mainBall.position - goal; bool inHoop = new Vector2(delta.x, delta.z).magnitude < 0.2f && Mathf.Abs(delta.y) < 0.15f && mainBall.linearVelocity.y < -0.1f; if (inHoop && !scored) { score++; scored = true; CreativeTools.Notice("Baskets: " + score); } if (Mathf.Abs(delta.y) > 0.5f) scored = false; }
        }
        if (Time.unscaledTime >= nextScore && scoreLabel != null)
        {
            nextScore = Time.unscaledTime + 0.2f;
            if (Mode == "Mini Bowling") { int down = pieces.Skip(1).Count(p => p != null && Vector3.Dot(p.transform.up, Vector3.up) < 0.5f); scoreLabel.text = "Mini Bowling: " + down + "/10 pins | A resets"; }
            if (Mode == "Ring Toss") scoreLabel.text = "Rings on peg: " + pieces.Count(p => p != null && p.position.y < goal.y + 0.3f && new Vector2(p.position.x - goal.x, p.position.z - goal.z).magnitude < 0.12f);
            if (Mode == "Cup Stacking") scoreLabel.text = "Stack height: " + (pieces.Count > 0 ? pieces.Max(p => p.position.y) - basePoint.y : 0f).ToString("F2") + "m";
        }
        if (Mode == "Virtual Darts") foreach (var piece in pieces)
            if (piece != null && piece != held && !piece.isKinematic && Vector3.Distance(piece.position, goal) < 0.55f && Mathf.Abs(Vector3.Dot(piece.position - goal, Forward)) < 0.15f)
            { piece.isKinematic = true; score++; CreativeTools.Notice("Dart hit " + score + ": " + Vector3.Distance(piece.position, goal).ToString("F2") + "m from center"); }
    }
    private void Pads()
    {
        if (pads.Count == 0) return;
        int touch = pads.FindIndex(p => Vector3.Distance(p.transform.position, CreativeTools.Right.position) < 0.16f || Vector3.Distance(p.transform.position, CreativeTools.Left.position) < 0.16f);
        bool freshTouch = touch >= 0 && touch != touchedPad;
        touchedPad = touch;
        if (Mode == "Memory Pads" && demonstrating)
        {
            if (Time.unscaledTime >= nextBeat)
            {
                if (demonstration == sequence.Count) { demonstrating = false; target = -1; }
                else { target = sequence[demonstration++]; nextBeat = Time.unscaledTime + 0.8f; }
            }
            PaintPads(Time.unscaledTime < nextBeat - 0.2f ? target : -1); return;
        }
        PaintPads(Mode == "Reaction Trainer" ? target : -1);
        if (Time.unscaledTime < nextBeat) return;
        if (!freshTouch) return;
        nextBeat = Time.unscaledTime + 0.4f;
        if (Mode == "Rhythm Drums" && audio != null && note != null) { audio.pitch = 0.6f + touch * 0.4f; audio.PlayOneShot(note, ExpansionSettings.AudioVolume); }
        if (Mode == "Reaction Trainer" && touch == target) { score++; CreativeTools.Notice("Reaction: " + ((Time.unscaledTime - began) * 1000f).ToString("F0") + "ms | " + score + " hits"); target = (target + UnityEngine.Random.Range(1, 4)) % 4; began = Time.unscaledTime; }
        if (Mode == "Memory Pads")
        {
            if (touch != sequence[memoryStep]) { sequence.Clear(); sequence.Add(UnityEngine.Random.Range(0, 4)); memoryStep = demonstration = score = 0; demonstrating = true; target = -1; nextBeat = Time.unscaledTime + 1f; CreativeTools.Notice("Sequence missed. Starting again."); }
            else if (++memoryStep == sequence.Count) { score++; memoryStep = demonstration = 0; if (sequence.Count < 16) sequence.Add(UnityEngine.Random.Range(0, 4)); demonstrating = true; target = -1; nextBeat = Time.unscaledTime + 1f; CreativeTools.Notice("Memory round " + score + " complete."); }
        }
    }
    private void PaintPads(int active)
    {
        Color color = CreativeTools.Theme;
        if (paintedPad == active && paintedTheme == color) return;
        paintedPad = active; paintedTheme = color;
        for (int i = 0; i < pads.Count; i++) CreativeTools.Paint(pads[i].GetComponent<Renderer>(), i == active ? Color.white : CreativeTools.Theme);
    }
    private void BuildBridge(Vector3 from, Vector3 to)
    {
        if (Vector3.Distance(from, to) > 10f) { CreativeTools.Notice("Bridge anchors must be within 10m."); return; }
        int count = Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(from, to) / 0.3f), 4, 24); Rigidbody? prior = null;
        for (int i = 0; i <= count; i++)
        {
            var body = Piece(PrimitiveType.Cube, Vector3.Lerp(from, to, i / (float)count), new Vector3(0.65f, 0.08f, Vector3.Distance(from, to) / count * 0.95f));
            body.rotation = Quaternion.LookRotation(to - from); body.isKinematic = i == 0 || i == count;
            if (prior != null) { var joint = body.gameObject.AddComponent<HingeJoint>(); joint.connectedBody = prior; joint.axis = Vector3.right; joint.enableCollision = false; joint.useLimits = true; joint.limits = new JointLimits { min = -15f, max = 15f }; }
            prior = body;
        }
        bridgeStart = false;
    }
    private void ScanShots()
    {
        nextShotScan = Time.unscaledTime + 0.2f;
        foreach (var old in shots.Keys.Where(s => s == null || !s.gameObject.activeInHierarchy).ToArray()) shots.Remove(old);
        foreach (var shot in Object.FindObjectsByType<SlingshotProjectile>(FindObjectsSortMode.None).Take(64))
        {
            Vector3 p = shot.transform.position; if (!shots.TryGetValue(shot, out var previous)) previous = p;
            foreach (var pad in pads) if (pad.activeSelf && DistanceToSegment(pad.transform.position, previous, p) < 0.28f) { pad.SetActive(false); score++; CreativeTools.Notice("Targets: " + score + "/" + pads.Count); }
            shots[shot] = p;
        }
    }
    private static float DistanceToSegment(Vector3 p, Vector3 a, Vector3 b) => ExpansionMath.DistanceToSegment(p, a, b);
    private AudioClip Tone(bool continuous)
    {
        int samples = continuous ? 22050 : 4410; var data = new float[samples];
        for (int i = 0; i < samples; i++) data[i] = Mathf.Sin(i * Mathf.PI * 2f * 440f / 22050f) * (continuous ? 0.18f : Mathf.Exp(-i / 650f) * 0.6f);
        var clip = AudioClip.Create("ChudOS Practice Tone", samples, 1, 22050, false); clip.SetData(data, 0); return clip;
    }
    private void ResetScene()
    {
        if (held != null) held = null;
        if (audio != null) { audio.Stop(); Object.Destroy(audio); audio = null; }
        if (note != null) Object.Destroy(note);
        if (droneTexture != null) { droneTexture.Release(); Object.Destroy(droneTexture); droneTexture = null; }
        foreach (var obj in Objects) if (obj != null) Object.Destroy(obj);
        Objects.Clear(); pieces.Clear(); pads.Clear(); pins.Clear(); shots.Clear(); sequence.Clear();
        mainBall = drone = held = null; rope = null; bridgeStart = false; memoryStep = demonstration = 0; wasGrip = false;
        Setup();
    }
    private void OnGUI()
    {
        if (Mode is "RC Drone" or "Metronome" or "Theremin" or "Physics Sandbox") window = MenuHelper.MakeWindow(852000 + Mode.GetHashCode() % 10000, window, Contents, Mode);
    }
    private void Contents(int id)
    {
        if (droneTexture != null) GUILayout.Label(droneTexture, GUILayout.Width(256), GUILayout.Height(192));
        if (Mode == "Metronome") { bpm = MenuHelper.Slider(bpm, 40f, 220f); MenuHelper.Label("BPM: " + Mathf.RoundToInt(bpm)); }
        if (Mode == "Theremin") MenuHelper.Label("Right hand height: pitch. Left hand distance: volume.");
        if (Mode == "Physics Sandbox")
        {
            MenuHelper.Label("Aim/click: place. B/R: shape. RG/RMB: grab. X/F: join last two props."); MenuHelper.Label("Shape: " + (shape == 0 ? "Cube" : shape == 1 ? "Sphere" : "Capsule") + " | Props: " + pieces.Count);
            if (held != null)
            {
                MenuHelper.Label("Held prop scale (X/Y/Z)"); Vector3 size = held.transform.localScale;
                size.x = MenuHelper.Slider(size.x, 0.06f, 1.5f); size.y = MenuHelper.Slider(size.y, 0.06f, 1.5f); size.z = MenuHelper.Slider(size.z, 0.06f, 1.5f); held.transform.localScale = size;
            }
        }
        GUI.DragWindow();
    }
    protected override void Cleanup()
    {
        if (audio != null) { audio.Stop(); Object.Destroy(audio); }
        if (note != null) Object.Destroy(note);
        if (droneCamera != null) droneCamera.targetTexture = null;
        if (droneTexture != null) { droneTexture.Release(); Object.Destroy(droneTexture); }
        GunLib.Hide();
    }
}
