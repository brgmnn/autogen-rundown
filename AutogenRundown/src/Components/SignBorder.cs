using System.Collections.Generic;
using AutogenRundown.Managers;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Components;

internal class SignBorder : MonoBehaviour
{
    private const float Padding = 0.5f;
    private const float LineWidth = 1.0f;
    private const float ZOffset = -0.005f;

    private static readonly Dictionary<int, Texture2D> textureCache = new();

    private MeshFilter? meshFilter;
    private MeshRenderer? meshRenderer;
    private Material? material;
    private LG_Sign? sign;
    private bool boundsResolved;
    private int frameCount;

    public int ZoneNumber { get; private set; }

    public void Setup(LG_Sign sign, Color color, int zoneNumber)
    {
        this.sign = sign;
        ZoneNumber = zoneNumber;

        var borderGo = new GameObject("SignBorder_Mesh");
        borderGo.transform.SetParent(sign.m_text.transform, false);
        borderGo.layer = sign.gameObject.layer;

        meshFilter = borderGo.AddComponent<MeshFilter>();
        meshRenderer = borderGo.AddComponent<MeshRenderer>();

        var shader = Shader.Find("Standard");
        if (shader == null)
        {
            Plugin.Logger.LogError("SignBorder: Standard shader not found");
            return;
        }
        material = new Material(shader);
        material.color = color;
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Glossiness", 0f);

        // Cutout mode for weathered paint effect
        material.SetFloat("_Mode", 1);
        material.SetOverrideTag("RenderType", "TransparentCutout");
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
        material.SetFloat("_ZWrite", 1f);
        material.EnableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 2450;
        material.SetFloat("_Cutoff", 0.35f);

        if (!textureCache.TryGetValue(zoneNumber, out var tex))
        {
            tex = GenerateWeatheredTexture(zoneNumber);
            textureCache[zoneNumber] = tex;
        }
        material.mainTexture = tex;

        meshRenderer.material = material;

        Plugin.Logger.LogDebug($"SignBorder: Setup complete, shader={material.shader.name}, layer={borderGo.layer}");

        SignBorderManager.Register(this);
    }

    public void SetColor(Color color)
    {
        if (material == null)
            return;

        material.color = color;
    }

    public void SetVisible(bool visible)
    {
        if (meshRenderer != null)
            meshRenderer.enabled = visible;
    }

    private void LateUpdate()
    {
        if (boundsResolved || sign == null || meshFilter == null)
            return;

        frameCount++;

        // Force mesh update after a few frames if bounds still invalid
        if (frameCount == 3)
            sign.m_text.ForceMeshUpdate();

        var bounds = sign.m_text.textBounds;

        // Reject sentinel/inverted bounds (TMP returns huge values when uninitialized)
        if (bounds.min.x >= bounds.max.x)
            return;

        var min = bounds.min;
        var max = bounds.max;

        float p = Padding;
        float t = LineWidth;
        float z = ZOffset;

        meshFilter.mesh = BuildBorderMesh(
            min.x - p, min.y - p,
            max.x + p, max.y + p,
            t, z);

        boundsResolved = true;
        Plugin.Logger.LogDebug($"SignBorder: Bounds resolved, min={min}, max={max}");
    }

    private static Mesh BuildBorderMesh(float left, float bottom, float right, float top, float thickness, float z)
    {
        var mesh = new Mesh();
        var verts = new Vector3[16];
        var tris = new int[24];

        // Bottom edge
        verts[0] = new Vector3(left, bottom, z);
        verts[1] = new Vector3(right, bottom, z);
        verts[2] = new Vector3(right, bottom + thickness, z);
        verts[3] = new Vector3(left, bottom + thickness, z);

        // Top edge
        verts[4] = new Vector3(left, top - thickness, z);
        verts[5] = new Vector3(right, top - thickness, z);
        verts[6] = new Vector3(right, top, z);
        verts[7] = new Vector3(left, top, z);

        // Left edge
        verts[8]  = new Vector3(left, bottom + thickness, z);
        verts[9]  = new Vector3(left + thickness, bottom + thickness, z);
        verts[10] = new Vector3(left + thickness, top - thickness, z);
        verts[11] = new Vector3(left, top - thickness, z);

        // Right edge
        verts[12] = new Vector3(right - thickness, bottom + thickness, z);
        verts[13] = new Vector3(right, bottom + thickness, z);
        verts[14] = new Vector3(right, top - thickness, z);
        verts[15] = new Vector3(right - thickness, top - thickness, z);

        for (int i = 0; i < 4; i++)
        {
            int vi = i * 4;
            int ti = i * 6;
            tris[ti]     = vi;     tris[ti + 1] = vi + 2; tris[ti + 2] = vi + 1;
            tris[ti + 3] = vi;     tris[ti + 4] = vi + 3; tris[ti + 5] = vi + 2;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;

        var normals = new Vector3[16];
        for (int i = 0; i < 16; i++)
            normals[i] = new Vector3(0, 0, -1);
        mesh.normals = normals;

        var uvs = new Vector2[16];
        for (int i = 0; i < 4; i++)
        {
            int vi = i * 4;
            uvs[vi]     = new Vector2(0, 0);
            uvs[vi + 1] = new Vector2(1, 0);
            uvs[vi + 2] = new Vector2(1, 1);
            uvs[vi + 3] = new Vector2(0, 1);
        }
        mesh.uv = uvs;

        return mesh;
    }

    private static Texture2D GenerateWeatheredTexture(int seed)
    {
        const int size = 128;
        const int octaves = 3;
        const float baseScale = 6f;
        const float persistence = 0.5f;
        const float lacunarity = 2f;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var offset = new Vector2(seed * 17.3f, seed * 31.7f);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = x / (float)size;
            float v = y / (float)size;

            // Multi-octave Perlin noise
            float noise = 0f, amp = 1f, freq = 1f;
            for (int o = 0; o < octaves; o++)
            {
                noise += amp * Mathf.PerlinNoise(
                    u * baseScale * freq + offset.x,
                    v * baseScale * freq + offset.y);
                amp *= persistence;
                freq *= lacunarity;
            }

            // Edge fade — paint wears more at edges
            float edgeDist = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
            float edgeFade = Mathf.SmoothStep(0f, 0.3f, edgeDist);

            float alpha = Mathf.Clamp01(noise * edgeFade);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        tex.Apply();
        Plugin.Logger.LogDebug($"SignBorder: Generated texture for zone {seed}, sample alpha={tex.GetPixel(64, 64).a}");
        return tex;
    }

    public void OnDestroy()
    {
        SignBorderManager.Unregister(this);

        if (meshFilter != null)
            Destroy(meshFilter.gameObject);
    }

    static SignBorder()
    {
        ClassInjector.RegisterTypeInIl2Cpp<SignBorder>();
    }
}
