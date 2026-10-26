// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChudOS.Core.Managers;

public static partial class MacroManager
{
    private static readonly string[] CoordinateNames = { "x", "y", "z", "w" };
    internal static RecordingData? ReadRecording(string path)
    {
        using var file = File.OpenRead(path);
        if (file.Length > MaximumFileSize) throw new InvalidDataException("Macro file is too large.");
        bool compressed = Path.GetExtension(path).Equals(".macro", StringComparison.OrdinalIgnoreCase);
        using var gzip = compressed ? new GZipStream(file, CompressionMode.Decompress, true) : null;
        Stream input = gzip != null ? gzip : file;
        using var output = new MemoryStream();
        byte[] buffer = new byte[8192];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (output.Length + read > MaximumFileSize) throw new InvalidDataException("Expanded macro is too large.");
            output.Write(buffer, 0, read);
        }
        output.Position = 0;
        using var text = new StreamReader(output, new UTF8Encoding(false, true));
        using var reader = new JsonTextReader(text) { MaxDepth = 16, DateParseHandling = DateParseHandling.None };
        var root = JObject.Load(reader);
        while (reader.Read()) if (reader.TokenType != JsonToken.Comment) throw new InvalidDataException("Unexpected data after macro.");
        if (root["positions"] == null) return root.ToObject<RecordingData>(JsonSerializer.Create(JsonSettings));
        if (root["positions"] is not JArray positions || positions.Count < 2 || positions.Count > MaximumFrames)
            throw new InvalidDataException("Unsupported macro frame count.");
        bool rigPose = positions[0] is JObject first && first["rigPosition"] is JObject;
        float step = rigPose ? Step : root["step-time"] == null ? 0.1f : Number(root["step-time"]);
        if (!Finite(step) || step <= 0f || (positions.Count - 1) * step > MaximumDuration) throw new InvalidDataException("Invalid macro sample interval.");
        var data = new RecordingData { Name = (string?)root["name"] ?? Path.GetFileNameWithoutExtension(path), HasRigPose = rigPose, ImportFormat = rigPose ? "hamburbur" : "Poison", Frames = new(positions.Count) };
        for (int i = 0; i < positions.Count; i++)
        {
            var entry = positions[i];
            if (!rigPose)
            {
                data.Frames.Add(new Frame
                {
                    Time = i * step, Body = ReadValues(entry["position"], false), Velocity = ReadValues(entry["velocity"], false),
                    LeftPosition = ReadValues(entry["leftHand"]?["position"], false), LeftRotation = ReadValues(entry["leftHand"]?["rotation"], true),
                    RightPosition = ReadValues(entry["rightHand"]?["position"], false), RightRotation = ReadValues(entry["rightHand"]?["rotation"], true),
                    LeftGrip = Flag(entry["leftGrip"]), RightGrip = Flag(entry["rightGrip"]), LeftTrigger = Flag(entry["leftTrigger"]), RightTrigger = Flag(entry["rightTrigger"])
                });
                continue;
            }
            data.Frames.Add(new Frame
            {
                Time = i * step, Body = ReadValues(entry["rigPosition"], false), Velocity = ReadValues(entry["velocity"], false),
                HeadPosition = ReadValues(entry["headPosition"], false), HeadRotation = ReadValues(entry["headRotation"], true), RigRotation = ReadValues(entry["rigRotation"], true),
                LeftPosition = ReadValues(entry["leftHandPosition"], false), LeftRotation = ReadValues(entry["leftHandRotation"], true),
                RightPosition = ReadValues(entry["rightHandPosition"], false), RightRotation = ReadValues(entry["rightHandRotation"], true)
            });
        }
        if (!Valid(data)) throw new InvalidDataException("Macro contains invalid poses or timing.");
        return data;
    }

    private static float Number(JToken? token)
    {
        if (token == null || token.Type != JTokenType.Integer && token.Type != JTokenType.Float) throw new InvalidDataException("Invalid macro number.");
        return token.Value<float>();
    }
    private static bool Flag(JToken? token)
    {
        if (token == null) return false;
        if (token.Type != JTokenType.Boolean) throw new InvalidDataException("Invalid macro input flag.");
        return token.Value<bool>();
    }

    private static float[] ReadValues(JToken? token, bool rotation)
    {
        if (token is not JObject obj) throw new InvalidDataException("Missing macro pose.");
        float[] values = new float[rotation ? 4 : 3];
        for (int i = 0; i < values.Length; i++)
        {
            var value = obj[CoordinateNames[i]];
            if (value == null || value.Type != JTokenType.Integer && value.Type != JTokenType.Float) throw new InvalidDataException("Invalid macro coordinate.");
            values[i] = value.Value<float>();
        }
        return values;
    }

    private static JObject Pose(float[] values) => values.Length == 4
        ? new JObject { ["x"] = values[0], ["y"] = values[1], ["z"] = values[2], ["w"] = values[3] }
        : new JObject { ["x"] = values[0], ["y"] = values[1], ["z"] = values[2] };

    internal static void WriteRecording(RecordingData data, string path, bool compressed)
    {
        if (data.ImportFormat == "Poison" && !compressed)
        {
            var positions = new JArray();
            foreach (var frame in data.Frames)
                positions.Add(new JObject
                {
                    ["position"] = Pose(frame.Body), ["velocity"] = Pose(frame.Velocity),
                    ["leftHand"] = new JObject { ["position"] = Pose(frame.LeftPosition), ["rotation"] = Pose(frame.LeftRotation) },
                    ["rightHand"] = new JObject { ["position"] = Pose(frame.RightPosition), ["rotation"] = Pose(frame.RightRotation) },
                    ["leftGrip"] = frame.LeftGrip, ["rightGrip"] = frame.RightGrip, ["leftTrigger"] = frame.LeftTrigger, ["rightTrigger"] = frame.RightTrigger
                });
            var root = new JObject { ["name"] = data.Name, ["enabled"] = false, ["step-time"] = data.Frames[1].Time - data.Frames[0].Time, ["positions"] = positions };
            File.WriteAllText(path, root.ToString(Formatting.None), new UTF8Encoding(false));
            return;
        }
        if (!compressed) { File.WriteAllText(path, JsonConvert.SerializeObject(data, JsonSettings), new UTF8Encoding(false)); return; }
        if (!data.HasRigPose) throw new InvalidDataException("This recording has no imported rig pose.");
        var frames = new JArray();
        foreach (var frame in data.Frames)
            frames.Add(new JObject
            {
                ["rigPosition"] = Pose(frame.Body), ["rigRotation"] = Pose(frame.RigRotation), ["velocity"] = Pose(frame.Velocity),
                ["headPosition"] = Pose(frame.HeadPosition), ["headRotation"] = Pose(frame.HeadRotation),
                ["leftHandPosition"] = Pose(frame.LeftPosition), ["leftHandRotation"] = Pose(frame.LeftRotation),
                ["rightHandPosition"] = Pose(frame.RightPosition), ["rightHandRotation"] = Pose(frame.RightRotation)
            });
        using var file = File.Create(path);
        using var gzip = new GZipStream(file, CompressionLevel.Optimal);
        using var writer = new StreamWriter(gzip, new UTF8Encoding(false));
        new JObject { ["name"] = data.Name, ["positions"] = frames }.WriteTo(new JsonTextWriter(writer));
    }
}
