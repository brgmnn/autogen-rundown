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

    private LineRenderer? lineRenderer;
    private LG_Sign? sign;
    private bool boundsResolved;

    public int ZoneNumber { get; private set; }

    public void Setup(LG_Sign sign, Color color, int zoneNumber)
    {
        this.sign = sign;
        ZoneNumber = zoneNumber;

        var borderGo = new GameObject("SignBorder_Line");
        borderGo.transform.SetParent(sign.m_text.transform, false);

        lineRenderer = borderGo.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        lineRenderer.startWidth = LineWidth;
        lineRenderer.endWidth = LineWidth;
        lineRenderer.numCornerVertices = 0;
        lineRenderer.numCapVertices = 0;
        lineRenderer.alignment = LineAlignment.TransformZ;

        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        lineRenderer.material = mat;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        SignBorderManager.Register(this);
    }

    public void SetColor(Color color)
    {
        if (lineRenderer == null)
            return;

        lineRenderer.material.color = color;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    public void SetVisible(bool visible)
    {
        if (lineRenderer != null)
            lineRenderer.enabled = visible;
    }

    private void LateUpdate()
    {
        if (boundsResolved || sign == null || lineRenderer == null)
            return;

        var bounds = sign.m_text.textBounds;

        if (bounds.size.sqrMagnitude < 0.001f)
            return;

        var min = bounds.min;
        var max = bounds.max;

        lineRenderer.SetPosition(0, new Vector3(min.x - Padding, min.y - Padding, ZOffset));
        lineRenderer.SetPosition(1, new Vector3(max.x + Padding, min.y - Padding, ZOffset));
        lineRenderer.SetPosition(2, new Vector3(max.x + Padding, max.y + Padding, ZOffset));
        lineRenderer.SetPosition(3, new Vector3(min.x - Padding, max.y + Padding, ZOffset));

        boundsResolved = true;
    }

    public void OnDestroy()
    {
        SignBorderManager.Unregister(this);

        if (lineRenderer != null)
            Destroy(lineRenderer.gameObject);
    }

    static SignBorder()
    {
        ClassInjector.RegisterTypeInIl2Cpp<SignBorder>();
    }
}
