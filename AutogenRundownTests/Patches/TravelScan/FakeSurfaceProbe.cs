using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// A synthetic multi-floor world standing in for the NavMesh.
///
/// A "surface" is a height function over XZ that returns null where that surface doesn't exist.
/// Stacking two of them reproduces the situation that breaks real path generation: both floors of
/// a stairwell are candidates at the same XZ, and only the reference height distinguishes them.
/// </summary>
public sealed class FakeSurfaceProbe : ISurfaceProbe
{
    private readonly List<Func<Vector3, float?>> _surfaces;

    public FakeSurfaceProbe(params Func<Vector3, float?>[] surfaces)
    {
        _surfaces = surfaces.ToList();
    }

    /// <summary>A single flat surface at the given height, covering everything.</summary>
    public static Func<Vector3, float?> Flat(float y) => _ => y;

    /// <summary>Flat at <paramref name="y"/>, but only within an XZ rectangle.</summary>
    public static Func<Vector3, float?> Slab(float y, float minX, float maxX)
        => p => p.x >= minX && p.x <= maxX ? y : null;

    /// <summary>
    /// A 45 degree stair rising from y=0 at x=riseStart to y=height at x=riseEnd, flat either side.
    /// </summary>
    public static Func<Vector3, float?> Stair(float riseStart, float riseEnd, float height)
        => p =>
        {
            if (p.x <= riseStart) return 0f;
            if (p.x >= riseEnd) return height;
            return height * (p.x - riseStart) / (riseEnd - riseStart);
        };

    /// <summary>
    /// Mirrors NavMeshSurfaceProbe.Snap: probe from the reference height, take the nearest surface
    /// within the sample radius, and reject it if it is too far from the reference to be the same
    /// floor.
    /// </summary>
    public Vector3 Snap(Vector3 point, float referenceY, float preferredY)
    {
        var probeY = Mathf.Clamp(
                         preferredY,
                         referenceY - TravelScanRegistry.MaxSurfaceSnapRise,
                         referenceY + TravelScanRegistry.MaxSurfaceSnapRise)
                     + TravelScanRegistry.SurfaceSampleLift;

        float? best = null;
        var bestDistance = float.MaxValue;

        foreach (var surface in _surfaces)
        {
            var y = surface(point);
            if (y == null)
                continue;

            var distance = Mathf.Abs(y.Value - probeY);
            if (distance > TravelScanRegistry.SurfaceSampleRadius || distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = y;
        }

        if (best == null)
            return point;

        if (Mathf.Abs(best.Value - referenceY) > TravelScanRegistry.MaxSurfaceSnapRise)
            return point;

        return new Vector3(point.x, best.Value, point.z);
    }

    /// <summary>
    /// Mirrors NavMeshSurfaceProbe.IsWalkable: the slope must be one the terrain could have, and
    /// the straight line must stay on a surface for its whole length (which is what fails when a
    /// chord crosses a void).
    /// </summary>
    public bool IsWalkable(Vector3 a, Vector3 b)
    {
        if (!SurfaceGeometry.IsSlopeWalkable(a, b))
            return false;

        const int samples = 24;

        for (var i = 0; i <= samples; i++)
        {
            var point = Vector3.Lerp(a, b, i / (float)samples);
            var supported = false;

            foreach (var surface in _surfaces)
            {
                var y = surface(point);
                if (y != null && Mathf.Abs(y.Value - point.y) <= TravelScanRegistry.MaxSurfaceSnapRise)
                {
                    supported = true;
                    break;
                }
            }

            if (!supported)
                return false;
        }

        return true;
    }

    /// <summary>
    /// The synthetic world is unbounded in XZ, so there are no edges to pull away from. Keeping
    /// this a no-op also isolates the walk tests from edge-adjustment behaviour.
    /// </summary>
    public Vector3 PullFromEdge(Vector3 position, float minDistance) => position;
}
