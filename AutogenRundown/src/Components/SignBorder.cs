using AutogenRundown.Managers;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Components;

internal class SignBorder : MonoBehaviour
{
    private const float Padding = 0.15f;
    private const float LineWidth = 0.02f;
    private const float ZOffset = -0.01f;

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

        // Borrow shader from sign's own renderer (guaranteed to work in pipeline)
        var existingRenderer = sign.GetComponentInChildren<Renderer>();
        if (existingRenderer != null && existingRenderer.material != null)
        {
            material = new Material(existingRenderer.material.shader);
        }
        else
        {
            var shader = Shader.Find("Sprites/Default")
                ?? Shader.Find("UI/Default")
                ?? Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Plugin.Logger.LogError("SignBorder: No shader found");
                return;
            }
            material = new Material(shader);
        }

        material.SetColor("_EmissionColor", color);
        meshRenderer.material = material;

        Plugin.Logger.LogDebug($"SignBorder: Setup complete, shader={material.shader.name}, layer={borderGo.layer}");

        SignBorderManager.Register(this);
    }

    public void SetColor(Color color)
    {
        if (material == null)
            return;

        material.SetColor("_EmissionColor", color);
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
        var min = bounds.min;
        var max = bounds.max;

        // Reject sentinel/inverted bounds (TMP returns huge inverted values when uninitialized)
        if (min.x >= max.x)
            return;

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
