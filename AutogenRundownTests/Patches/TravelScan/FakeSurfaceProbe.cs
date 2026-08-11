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

    /// <summary>
    /// How many times Snap has been called. The walk is supposed to notice a blocked sub-step
    /// immediately rather than spinning against it, and a call count is how that gets asserted.
    /// </summary>
    public int SnapCalls { get; private set; }

    /// <summary>
    /// Interior waypoints handed back by <see cref="TryFindRoute"/>. Empty means "no way around",
    /// which is the honest default — the synthetic world has no pathfinder.
    /// </summary>
    public List<Vector3> Route { get; set; } = new();

    /// <summary>A single flat surface at the given height, covering everything.</summary>
    public static Func<Vector3, float?> Flat(float y) => _ => y;

    /// <summary>
    /// Flat at <paramref name="y"/> everywhere except a rectangular hole, which is unsupported.
    /// A probe aimed into the hole returns the nearest supported point — which is *behind* the
    /// target, back toward the cursor. That is exactly how the real NavMesh behaves at a wall,
    /// and it is what makes the walk stall.
    /// </summary>
    public static Func<Vector3, float?> FlatWithHole(
        float y, float minX, float maxX, float minZ, float maxZ)
        => p => p.x >= minX && p.x <= maxX && p.z >= minZ && p.z <= maxZ ? null : y;

    /// <summary>Flat at <paramref name="y"/>, but only within an XZ rectangle.</summary>
    public static Func<Vector3, float?> Slab(float y, float minX, float maxX)
        => p => p.x >= minX && p.x <= maxX ? y : null;

    /// <summary>
    /// A ramp of the given slope climbing toward x=0, meeting a flat landing at y=0 there.
    ///
    /// This is the shape that defeats sag subdivision: the crest at x=0 is a convex break, so a
    /// chord straddling it dips below the surface by an amount that grows with the slope.
    /// </summary>
    public static Func<Vector3, float?> Crest(float slope)
        => p => p.x >= 0f ? 0f : p.x * slope;

    /// <summary>
    /// A real staircase: discrete treads of depth <paramref name="tread"/> separated by risers of
    /// height <paramref name="rise"/>, climbing from x=0.
    ///
    /// This is what a steep modded stair actually looks like to the navmesh — a run of small
    /// convex breaks rather than one smooth ramp. Each nose sags a chord that spans it, and the
    /// breaks come far more often than a single crest does.
    /// </summary>
    public static Func<Vector3, float?> Steps(float tread, float rise, int count)
        => p =>
        {
            if (p.x <= 0f) return 0f;

            var index = Mathf.Min(Mathf.FloorToInt(p.x / tread), count);
            return index * rise;
        };

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
    /// Mirrors NavMeshSurfaceProbe.Snap: probe from the reference height biased toward the
    /// preferred one, take the nearest surface within the sample radius, and reject it if it is
    /// too far from the reference to be the same floor.
    ///
    /// Like NavMesh.SamplePosition, the search is in 3D — if nothing is supported directly at the
    /// point's XZ, the nearest supported point *around* it is returned. That is what pulls a
    /// cursor backwards when it steps toward a wall.
    /// </summary>
    public bool TrySnap(Vector3 point, float referenceY, float preferredY, out Vector3 snapped)
    {
        SnapCalls++;

        var biasedY = Mathf.Clamp(
            preferredY,
            referenceY - TravelScanRegistry.MaxSurfaceSnapRise,
            referenceY + TravelScanRegistry.MaxSurfaceSnapRise);

        if (TrySnapFrom(point, biasedY, referenceY, out snapped))
            return true;

        // Mirrors the real probe: the bias can aim out of range of the surface underfoot, so fall
        // back to probing from the reference height.
        if (!Mathf.Approximately(biasedY, referenceY)
            && TrySnapFrom(point, referenceY, referenceY, out snapped))
            return true;

        snapped = point;
        return false;
    }

    private bool TrySnapFrom(Vector3 point, float probeHeight, float referenceY, out Vector3 snapped)
    {
        snapped = point;

        var probeY = probeHeight + TravelScanRegistry.SurfaceSampleLift;

        if (TrySampleAt(point, probeY, referenceY, out var direct))
        {
            snapped = direct;
            return true;
        }

        // Nothing under the point itself — search outward for the nearest supported spot, the way
        // SamplePosition does.
        var best = point;
        var bestDistance = float.MaxValue;

        for (var ring = 1; ring <= 4; ring++)
        {
            var radius = TravelScanRegistry.SurfaceSampleRadius * ring / 4f;

            for (var step = 0; step < 8; step++)
            {
                var angle = step * Mathf.PI * 2f / 8f;
                var candidate = point + new Vector3(
                    Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                if (!TrySampleAt(candidate, probeY, referenceY, out var sampled))
                    continue;

                var distance = (sampled - point).magnitude;
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = sampled;
            }

            if (bestDistance < float.MaxValue)
                break;
        }

        if (bestDistance == float.MaxValue)
            return false;

        snapped = best;
        return true;
    }

    private bool TrySampleAt(Vector3 point, float probeY, float referenceY, out Vector3 sampled)
    {
        sampled = point;

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
            return false;

        if (Mathf.Abs(best.Value - referenceY) > TravelScanRegistry.MaxSurfaceSnapRise)
            return false;

        sampled = new Vector3(point.x, best.Value, point.z);
        return true;
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
    /// Where each waypoint would be nudged to get clear of an edge. Null (the default) means no
    /// edges anywhere, which keeps the walk tests isolated from edge-adjustment behaviour; set it
    /// to reproduce a specific pull.
    /// </summary>
    public Func<Vector3, Vector3>? Pull { get; set; }

    public Vector3 PullFromEdge(Vector3 position, float minDistance)
        => Pull?.Invoke(position) ?? position;

    /// <summary>
    /// Returns whatever <see cref="Route"/> was set to. Empty by default — the synthetic world has
    /// no pathfinder, so "no way around" is the honest answer unless a test says otherwise.
    /// </summary>
    public bool TryFindRoute(Vector3 from, Vector3 to, out List<Vector3> via)
    {
        via = new List<Vector3>(Route);
        return via.Count > 0;
    }
}
