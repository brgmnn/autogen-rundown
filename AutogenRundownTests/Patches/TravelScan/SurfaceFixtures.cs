using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Builds triangle meshes to trace against.
///
/// These feed the real <see cref="NavSurface"/> rather than a stand-in, so what the tests exercise
/// is the production geometry. Only NavSurface.Build — the one method that calls Unity — is out of
/// reach here, and it does nothing but copy arrays out of the triangulation.
///
/// Quads are emitted with their own four vertices and never welded, deliberately. Unity is not
/// obliged to weld across the seams between 64m bake tiles either, so tracing has to cope with a
/// mesh whose triangles only meet geometrically. If the trace ever grew an adjacency table these
/// fixtures would start failing, which is the point.
/// </summary>
internal sealed class MeshBuilder
{
    private readonly List<Vector3> vertices = new();
    private readonly List<int> indices = new();

    /// <summary>
    /// Adds a grid of quads over an XZ rectangle. <paramref name="height"/> supplies Y at each
    /// corner; <paramref name="present"/> can leave cells out to make holes and gaps.
    /// </summary>
    internal MeshBuilder AddGrid(
        float minX, float maxX, float minZ, float maxZ, float cell,
        Func<float, float, float> height,
        Func<float, float, bool>? present = null)
    {
        var countX = Mathf.RoundToInt((maxX - minX) / cell);
        var countZ = Mathf.RoundToInt((maxZ - minZ) / cell);

        for (var ix = 0; ix < countX; ix++)
        for (var iz = 0; iz < countZ; iz++)
        {
            var x = minX + ix * cell;
            var z = minZ + iz * cell;

            if (present != null && !present(x + cell * 0.5f, z + cell * 0.5f))
                continue;

            var first = vertices.Count;

            vertices.Add(new Vector3(x, height(x, z), z));
            vertices.Add(new Vector3(x + cell, height(x + cell, z), z));
            vertices.Add(new Vector3(x + cell, height(x + cell, z + cell), z + cell));
            vertices.Add(new Vector3(x, height(x, z + cell), z + cell));

            indices.Add(first);
            indices.Add(first + 1);
            indices.Add(first + 2);

            indices.Add(first);
            indices.Add(first + 2);
            indices.Add(first + 3);
        }

        return this;
    }

    internal NavSurface Build() => new(vertices.ToArray(), indices.ToArray());
}

internal static class SurfaceFixtures
{
    /// <summary>Featureless floor at y = 0, x in [-2, 22], z in [-6, 6].</summary>
    internal static NavSurface Flat()
        => new MeshBuilder().AddGrid(-2f, 22f, -6f, 6f, 1f, (_, _) => 0f).Build();

    /// <summary>
    /// Flat, then a ramp climbing 3m over x in [5, 10], then flat again — the shape the navmesh
    /// bakes over a staircase, as the in-game overlay shows over catwalk_stairs_a.
    /// </summary>
    internal static NavSurface Ramp()
        => new MeshBuilder()
            .AddGrid(-2f, 22f, -6f, 6f, 1f, (x, _) => RampHeight(x))
            .Build();

    internal static float RampHeight(float x)
    {
        if (x <= 5f) return 0f;
        if (x >= 10f) return 3f;

        return (x - 5f) * 0.6f;
    }

    /// <summary>
    /// Two floors 6m apart with a genuine gap between them: upper for x in [-2, 10], lower for
    /// x in [12, 22], nothing across x in [10, 12].
    ///
    /// This is the geometry behind segment 21→22 in ZONE_606 — (-19.9, 0.0, 124.1) to
    /// (-24.0, -6.0, 124.5) — which the old walk chorded straight across.
    /// </summary>
    internal static NavSurface Ledge()
        => new MeshBuilder()
            .AddGrid(
                -2f, 22f, -6f, 6f, 1f,
                (x, _) => x <= 10f ? 0f : -6f,
                (x, _) => x < 10f || x > 12f)
            .Build();

    /// <summary>
    /// Two floors 6m apart meeting edge to edge with no gap between them: upper for x in [-2, 10],
    /// lower for x in [10, 22].
    ///
    /// This is the harder half of the ledge, and the one the geometry in ZONE_606 actually is.
    /// Where <see cref="Ledge"/> has a gap that nothing can be found in, here there is a perfectly
    /// good floor immediately past the edge — just six metres down. Every check that works by
    /// asking "is there surface near here" says yes.
    /// </summary>
    /// <remarks>
    /// Two separate grids meeting at x = 10, not one grid with a step in its height function —
    /// a single grid spanning the boundary would emit a quad sloping from 0 to -6 over one metre,
    /// which is continuous geometry the trace should and would follow.
    /// </remarks>
    internal static NavSurface Cliff() => Cliff(-6f);

    internal static NavSurface Cliff(float lowerY)
        => new MeshBuilder()
            .AddGrid(-2f, 10f, -6f, 6f, 1f, (_, _) => 0f)
            .AddGrid(10f, 22f, -6f, 6f, 1f, (_, _) => lowerY)
            .Build();

    /// <summary>
    /// Two complete floors stacked at the same XZ, 6m apart. NavMesh.Raycast walks the 2D
    /// projection and cannot see the difference; locating by height can.
    /// </summary>
    internal static NavSurface Stacked()
        => new MeshBuilder()
            .AddGrid(-2f, 22f, -6f, 6f, 1f, (_, _) => 0f)
            .AddGrid(-2f, 22f, -6f, 6f, 1f, (_, _) => -6f)
            .Build();

    /// <summary>Flat floor with a 4m square missing from x in [8, 12], z in [-2, 2].</summary>
    internal static NavSurface Hole()
        => new MeshBuilder()
            .AddGrid(
                -2f, 22f, -6f, 6f, 1f,
                (_, _) => 0f,
                (x, z) => x < 8f || x > 12f || z < -2f || z > 2f)
            .Build();

    internal static string Show(Vector3 p) => $"({p.x:F2}, {p.y:F2}, {p.z:F2})";

    internal static string Show(IEnumerable<Vector3> points)
        => string.Join(" ", points.Select(Show));

    /// <summary>
    /// Largest distance from any point of <paramref name="from"/> to the polyline
    /// <paramref name="to"/>. Used to check a thinned path still follows what it was thinned from.
    /// </summary>
    internal static float MaxDeviation(List<Vector3> from, List<Vector3> to)
    {
        var worst = 0f;

        foreach (var point in from)
        {
            var nearest = float.MaxValue;

            for (var i = 1; i < to.Count; i++)
                nearest = Mathf.Min(nearest, DistanceToSegment(point, to[i - 1], to[i]));

            worst = Mathf.Max(worst, nearest);
        }

        return worst;
    }

    private static float DistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var lengthSqr = ab.sqrMagnitude;

        if (lengthSqr < 1e-8f)
            return (point - a).magnitude;

        var t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSqr);

        return (point - (a + ab * t)).magnitude;
    }
}
