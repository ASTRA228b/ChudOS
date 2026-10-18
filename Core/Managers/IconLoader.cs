// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ChudOS.Core.Managers;


public static class IconLoader
{
    private static readonly Dictionary<string, Material> Materials = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<Texture2D> Textures = new();
    private static Mesh? FlippedQuadMesh;

    public static GameObject? Create(string iconName, Transform parent, Vector3 position, Vector2 size)
    {
        Material? mat = Get(iconName);
        if (mat == null) return null;
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = "Icon " + iconName;
        obj.layer = 2;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        
        obj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
 
        Texture2D? texture = mat.mainTexture as Texture2D;
        float aspect = texture != null ? (float)texture.width / Mathf.Max(1, texture.height) : 1f;
        float sy = Mathf.Max(Mathf.Abs(parent.lossyScale.y), 0.0001f);
        float sz = Mathf.Max(Mathf.Abs(parent.lossyScale.z), 0.0001f);
        float worldHeight = Mathf.Min(size.y * sy, size.x * sz / aspect);
        float worldWidth = worldHeight * aspect;
        obj.transform.localScale = new Vector3(worldWidth / sz, worldHeight / sy, 1f);
  
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        if (FlippedQuadMesh == null)
        {
            FlippedQuadMesh = UnityEngine.Object.Instantiate(filter.sharedMesh);
            Vector2[] uv = FlippedQuadMesh.uv;
            for (int i = 0; i < uv.Length; i++) uv[i].x = 1f - uv[i].x;
            FlippedQuadMesh.uv = uv;
        }
        filter.sharedMesh = FlippedQuadMesh;
        UnityEngine.Object.Destroy(obj.GetComponent<Collider>());
        obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return obj;
    }

    private static Material? Get(string name)
    {
        if (Materials.TryGetValue(name, out Material existing)) return existing;
        string fullName = "ChudOS.Icons." + name + ".png";
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName);
        if (stream == null) { Debug.LogWarning("[ChudOS] Icon missing: " + fullName); return null; }
        byte[] data = new byte[checked((int)stream.Length)];
        int read = 0;
        while (read < data.Length) { int length = stream.Read(data, read, data.Length - read); if (length <= 0) break; read += length; }
        Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(texture, data)) { UnityEngine.Object.Destroy(texture); return null; }
        texture.filterMode = FilterMode.Bilinear;
        Textures.Add(texture);
        Shader shader = Shader.Find("Unlit/Transparent") ?? Shader.Find("Sprites/Default");
        if (shader == null) { Debug.LogWarning("[ChudOS] Transparent icon shader unavailable"); return null; }
        Material material = new(shader) { mainTexture = texture, color = Color.white };
        Materials.Add(name, material);
        return material;
    }

    public static void Cleanup()
    {
        foreach (Material m in Materials.Values) if (m != null) UnityEngine.Object.Destroy(m);
        Materials.Clear();
        foreach (Texture2D texture in Textures) if (texture != null) UnityEngine.Object.Destroy(texture);
        Textures.Clear();
        if (FlippedQuadMesh != null) UnityEngine.Object.Destroy(FlippedQuadMesh);
        FlippedQuadMesh = null;
    }
}
