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
        borderGo.transform.SetParent(sign.transform, false);
        borderGo.layer = sign.gameObject.layer;

        meshFilter = borderGo.AddComponent<MeshFilter>();
        meshRenderer = borderGo.AddComponent<MeshRenderer>();

        var shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Plugin.Logger.LogError("SignBorder: Sprites/Default shader not found");
            return;
        }
        material = new Material(shader);
        material.color = color;
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

        // Reject sentinel/inverted bounds BEFORE coordinate transform
        // (TMP returns huge inverted values when text bounds are uninitialized)
        if (bounds.min.x >= bounds.max.x)
            return;

        var textTransform = sign.m_text.transform;
        var signTransform = sign.transform;

        // Convert TMP-local bounds to sign-local space
        var min = signTransform.InverseTransformPoint(
            textTransform.TransformPoint(bounds.min));
        var max = signTransform.InverseTransformPoint(
            textTransform.TransformPoint(bounds.max));

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

        return mesh;
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
