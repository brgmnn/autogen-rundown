using UnityEngine;
using UnityEngine.AI;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// The seam the path generator sees. Production is <see cref="NavSurface"/> over the baked NavMesh;
/// tests supply synthetic triangle meshes.
/// </summary>
public interface INavSurface
{
    /// <summary>
    /// Places a point on the walkable surface, choosing the floor closest to <paramref name="preferredY"/>.
    /// </summary>
    bool TryLocate(Vector3 point, float preferredY, out Vector3 onSurface);

    /// <summary>
    /// Traces the surface from one point to another, appending a waypoint wherever the route
    /// crosses a triangle edge. Does not append <paramref name="from"/>; the last point appended is
    /// <paramref name="to"/> placed on the surface.
    ///
    /// Returns false — leaving whatever it managed to append in place — if the route runs off the
    /// mesh or changes floor.
    /// </summary>
    bool TryTrace(Vector3 from, Vector3 to, List<Vector3> output);
}

/// <summary>
/// The baked NavMesh as what it actually is: a soup of flat triangles.
///
/// Every other way of asking Unity about the navigation surface answers a slightly different
/// question than the one we need. NavMesh.SamplePosition returns the nearest surface within a
/// radius, which near a ledge is the floor below. NavMesh.Raycast walks the 2D projection, so two
/// points stacked on different floors read as connected. Neither knows which polygon you are on, so
/// neither can tell a ramp from a drop.
///
/// Tracing the triangles answers it directly. A route walked edge-to-edge across the mesh yields a
/// polyline where every segment lies inside a single flat triangle, and a flat triangle cannot be
/// passed through. That is the property the sampling approach could never quite reach, and it is
/// what stops the scan clipping through staircases.
///
/// The geometry is deliberately kept free of NavMesh calls so it can be tested against synthetic
/// meshes; <see cref="Build"/> is the only part that touches Unity.
/// </summary>
public sealed class NavSurface : INavSurface
{
    /// <summary>
    /// Tolerance for the barycentric containment test. Negative so a point sitting exactly on a
    /// shared edge is claimed by both triangles rather than falling between them.
    /// </summary>
    private const float ContainmentEpsilon = -1e-4f;

    private const float GeometryEpsilon = 1e-6f;

    private readonly Vector3[] vertices;
    private readonly int[] indices;
    private readonly Dictionary<long, List<int>> cells = new();

    public int TriangleCount => indices.Length / 3;

    /// <summary>
    /// Takes ownership of the arrays. <paramref name="indices"/> is three vertex indices per
    /// triangle, matching NavMeshTriangulation's layout.
    /// </summary>
    public NavSurface(Vector3[] vertices, int[] indices)
    {
        this.vertices = vertices;
        this.indices = indices;

        BuildSpatialIndex();
    }

    #region Spatial index

    private static int CellCoordinate(float value)
        => Mathf.FloorToInt(value / TravelScanRegistry.TriangleCellSize);

    private static long CellKey(int x, int z) => ((long)x << 32) ^ (uint)z;

    /// <summary>
    /// Buckets each triangle into every cell its XZ bounding box touches. Triangles are far smaller
    /// than a cell in practice, so most land in one or two.
    /// </summary>
    private void BuildSpatialIndex()
    {
        for (var triangle = 0; triangle < TriangleCount; triangle++)
        {
            var a = vertices[indices[triangle * 3]];
            var b = vertices[indices[triangle * 3 + 1]];
            var c = vertices[indices[triangle * 3 + 2]];

            var minX = CellCoordinate(Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
            var maxX = CellCoordinate(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
            var minZ = CellCoordinate(Mathf.Min(a.z, Mathf.Min(b.z, c.z)));
            var maxZ = CellCoordinate(Mathf.Max(a.z, Mathf.Max(b.z, c.z)));

            for (var x = minX; x <= maxX; x++)
            for (var z = minZ; z <= maxZ; z++)
            {
                var key = CellKey(x, z);

                if (!cells.TryGetValue(key, out var bucket))
                    cells[key] = bucket = new List<int>();

                bucket.Add(triangle);
            }
        }
    }

    #endregion

    #region Triangle maths

    /// <summary>
    /// Barycentric coordinates of an XZ point against a triangle's XZ projection. Returns false for
    /// a triangle with no XZ area — a wall, or a degenerate sliver.
    /// </summary>
    private bool TryBarycentric(int triangle, float x, float z, out float u, out float v, out float w)
    {
        u = v = w = 0f;

        var a = vertices[indices[triangle * 3]];
        var b = vertices[indices[triangle * 3 + 1]];
        var c = vertices[indices[triangle * 3 + 2]];

        var determinant = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);

        if (Mathf.Abs(determinant) < GeometryEpsilon)
            return false;

        u = ((b.z - c.z) * (x - c.x) + (c.x - b.x) * (z - c.z)) / determinant;
        v = ((c.z - a.z) * (x - c.x) + (a.x - c.x) * (z - c.z)) / determinant;
        w = 1f - u - v;

        return true;
    }

    /// <summary>
    /// Height of the triangle's plane at an XZ point, if the point is inside its XZ projection.
    /// </summary>
    internal bool TryHeightAt(int triangle, float x, float z, out float y)
    {
        y = 0f;

        if (!TryBarycentric(triangle, x, z, out var u, out var v, out var w))
            return false;

        if (u < ContainmentEpsilon || v < ContainmentEpsilon || w < ContainmentEpsilon)
            return false;

        y = u * vertices[indices[triangle * 3]].y
            + v * vertices[indices[triangle * 3 + 1]].y
            + w * vertices[indices[triangle * 3 + 2]].y;

        return true;
    }

    /// <summary>
    /// Height at an XZ point clamped into the triangle, plus how far outside it the point was.
    /// Used where a point is known to sit on or just past a boundary and float error may have put
    /// it fractionally on the wrong side.
    /// </summary>
    private bool TryClampedHeightAt(int triangle, float x, float z, out float y, out float distance)
    {
        y = 0f;
        distance = 0f;

        if (!TryBarycentric(triangle, x, z, out var u, out var v, out var w))
            return false;

        var a = vertices[indices[triangle * 3]];
        var b = vertices[indices[triangle * 3 + 1]];
        var c = vertices[indices[triangle * 3 + 2]];

        if (u >= ContainmentEpsilon && v >= ContainmentEpsilon && w >= ContainmentEpsilon)
        {
            y = u * a.y + v * b.y + w * c.y;
            return true;
        }

        // Outside: take the nearest point on the nearest edge, in XZ.
        var best = float.MaxValue;

        for (var edge = 0; edge < 3; edge++)
        {
            var p = vertices[indices[triangle * 3 + edge]];
            var q = vertices[indices[triangle * 3 + (edge + 1) % 3]];

            var dx = q.x - p.x;
            var dz = q.z - p.z;
            var lengthSqr = dx * dx + dz * dz;

            var t = lengthSqr < GeometryEpsilon
                ? 0f
                : Mathf.Clamp01(((x - p.x) * dx + (z - p.z) * dz) / lengthSqr);

            var nearestX = p.x + dx * t;
            var nearestZ = p.z + dz * t;
            var gap = (x - nearestX) * (x - nearestX) + (z - nearestZ) * (z - nearestZ);

            if (gap >= best)
                continue;

            best = gap;
            y = Mathf.Lerp(p.y, q.y, t);
        }

        distance = Mathf.Sqrt(best);

        return true;
    }

    #endregion

    #region Locating

    public bool TryLocate(Vector3 point, float preferredY, out Vector3 onSurface)
        => TryLocate(point, preferredY, out _, out onSurface);

    /// <summary>
    /// Finds the triangle under a point, preferring the floor nearest <paramref name="preferredY"/>.
    ///
    /// This is what removes the whole class of wrong-floor bugs. SamplePosition can only return the
    /// nearest surface and has to be coaxed toward the right one by moving the probe; here every
    /// candidate floor at that XZ is enumerated and the choice is explicit.
    /// </summary>
    internal bool TryLocate(Vector3 point, float preferredY, out int triangle, out Vector3 onSurface)
    {
        triangle = -1;
        onSurface = point;

        var best = float.MaxValue;

        if (cells.TryGetValue(CellKey(CellCoordinate(point.x), CellCoordinate(point.z)), out var bucket))
        {
            foreach (var candidate in bucket)
            {
                if (!TryHeightAt(candidate, point.x, point.z, out var y))
                    continue;

                var offset = Mathf.Abs(y - preferredY);

                if (offset >= best)
                    continue;

                best = offset;
                triangle = candidate;
                onSurface = new Vector3(point.x, y, point.z);
            }
        }

        return triangle >= 0 || TryLocateNearby(point, preferredY, out triangle, out onSurface);
    }

    /// <summary>
    /// Fallback for a point with no triangle directly underneath: search the surrounding cells and
    /// take the nearest surface within LocateRadius, still preferring the closest floor.
    ///
    /// Covers a CalculatePath corner sitting exactly on a mesh boundary, and a re-locate that lands
    /// in a sliver of a gap where two bake tiles meet.
    /// </summary>
    private bool TryLocateNearby(Vector3 point, float preferredY, out int triangle, out Vector3 onSurface)
    {
        triangle = -1;
        onSurface = point;

        var bestOffset = float.MaxValue;
        var bestDistance = float.MaxValue;

        var cellX = CellCoordinate(point.x);
        var cellZ = CellCoordinate(point.z);

        for (var x = cellX - 1; x <= cellX + 1; x++)
        for (var z = cellZ - 1; z <= cellZ + 1; z++)
        {
            if (!cells.TryGetValue(CellKey(x, z), out var bucket))
                continue;

            foreach (var candidate in bucket)
            {
                if (!TryClampedHeightAt(candidate, point.x, point.z, out var y, out var distance))
                    continue;

                if (distance > TravelScanRegistry.LocateRadius)
                    continue;

                var offset = Mathf.Abs(y - preferredY);

                // Floor first, proximity second. Floors are at least agentHeight apart, so the
                // offset separates them cleanly and distance only breaks ties within one floor.
                if (offset > bestOffset - 1e-3f && (offset > bestOffset + 1e-3f || distance >= bestDistance))
                    continue;

                bestOffset = offset;
                bestDistance = distance;
                triangle = candidate;
                onSurface = new Vector3(point.x, y, point.z);
            }
        }

        return triangle >= 0;
    }

    #endregion

    #region Tracing

    public bool TryTrace(Vector3 from, Vector3 to, List<Vector3> output)
    {
        if (!TryLocate(from, from.y, out _, out var start))
            return false;

        var direction = new Vector2(to.x - from.x, to.z - from.z);
        var total = direction.magnitude;

        if (total <= TravelScanRegistry.EdgeNudge)
        {
            if (!TryLocate(to, start.y, out var destination))
                return false;

            output.Add(destination);
            return true;
        }

        direction /= total;

        // Start the march a nudge along the route rather than at the start point itself.
        //
        // Every pathfinder corner sits on a triangle boundary, and a point on a shared edge belongs
        // to the triangles on both sides of it. Picking the one *behind* the route leaves a ray
        // with no forward exit at all, and the trace gives up before it has taken a step. Probing
        // ahead picks the triangle the route actually enters. The centimetre skipped is of no
        // consequence — the caller already holds the exact start point.
        var cursor = new Vector2(start.x, start.z) + direction * TravelScanRegistry.EdgeNudge;
        var travelled = TravelScanRegistry.EdgeNudge;

        if (!TryLocate(new Vector3(cursor.x, start.y, cursor.y), start.y, out var triangle, out var entry))
            return false;

        if (Mathf.Abs(entry.y - start.y) > TravelScanRegistry.MaxTraceStep)
            return false;

        var height = entry.y;

        // Each pass advances at least EdgeNudge, so this cannot spin; it is a backstop against
        // degenerate geometry rather than a work limit.
        var guard = Mathf.CeilToInt(total / TravelScanRegistry.EdgeNudge) + 64;

        while (guard-- > 0)
        {
            // Destination reached once it falls inside the triangle we are standing in.
            if (TryHeightAt(triangle, to.x, to.z, out var destinationY))
            {
                output.Add(new Vector3(to.x, destinationY, to.z));
                return true;
            }

            if (!TryExit(triangle, cursor, direction, out var exit, out var advance))
                return false;

            // Overshot without ever containing the destination — float error at a boundary. Place
            // it on the surface directly rather than marching past it.
            if (travelled + advance >= total)
            {
                if (!TryLocate(new Vector3(to.x, height, to.z), height, out var destination))
                    return false;

                output.Add(destination);
                return true;
            }

            if (!TryClampedHeightAt(triangle, exit.x, exit.y, out var exitY, out _))
                return false;

            output.Add(new Vector3(exit.x, exitY, exit.y));

            travelled += advance;

            // Step just past the edge and ask which triangle we are in now. Resolving by position
            // and height rather than by an adjacency table costs nothing and survives the seams
            // between 64m bake tiles, where Unity need not have welded the vertices.
            var probe = exit + direction * TravelScanRegistry.EdgeNudge;

            if (!TryLocate(new Vector3(probe.x, exitY, probe.y), exitY, out triangle, out var next))
                return false;

            // A jump larger than the bake's agentClimb is a different floor, not the next triangle
            // along. Stopping here is the whole point: this is the ledge the scan used to chord
            // straight across.
            if (Mathf.Abs(next.y - exitY) > TravelScanRegistry.MaxTraceStep)
                return false;

            cursor = probe;
            height = next.y;
            travelled += TravelScanRegistry.EdgeNudge;
        }

        return false;
    }

    /// <summary>
    /// Where a ray leaves a triangle, in XZ. Returns the nearest edge crossing strictly ahead of
    /// the cursor.
    /// </summary>
    private bool TryExit(int triangle, Vector2 cursor, Vector2 direction, out Vector2 exit, out float advance)
    {
        exit = cursor;
        advance = 0f;

        var best = float.MaxValue;

        for (var edge = 0; edge < 3; edge++)
        {
            var p = vertices[indices[triangle * 3 + edge]];
            var q = vertices[indices[triangle * 3 + (edge + 1) % 3]];

            var edgeX = q.x - p.x;
            var edgeZ = q.z - p.z;

            var denominator = direction.x * edgeZ - direction.y * edgeX;

            if (Mathf.Abs(denominator) < GeometryEpsilon)
                continue;

            var offsetX = p.x - cursor.x;
            var offsetZ = p.z - cursor.y;

            var distance = (offsetX * edgeZ - offsetZ * edgeX) / denominator;
            var along = (offsetX * direction.y - offsetZ * direction.x) / denominator;

            if (distance <= GeometryEpsilon || distance >= best)
                continue;

            if (along < -1e-4f || along > 1f + 1e-4f)
                continue;

            best = distance;
            exit = cursor + direction * distance;
        }

        if (best == float.MaxValue)
            return false;

        advance = best;

        return true;
    }

    #endregion

    #region Construction from the baked NavMesh

    /// <summary>
    /// Extracts the baked walkable surface. The game does this itself every level — MapDetails
    /// .Extract builds the in-game map mesh from the same call — so the API is known-live.
    ///
    /// Returns null rather than throwing if the triangulation is unavailable or empty; the caller
    /// falls back to unrefined pathfinder corners.
    /// </summary>
    public static NavSurface? Build()
    {
        try
        {
            var triangulation = NavMesh.CalculateTriangulation();

            // NavMeshTriangulation is non-blittable, so Il2CppInterop hands it over as a reference
            // wrapper around a boxed IL2CPP value type and its members as Il2CppStructArray. Copy
            // straight out to managed arrays: bulk copies beat per-element interop, and nothing
            // then holds an interop object across the rest of the build.
            var sourceVertices = triangulation?.vertices?.ToArray();
            var sourceIndices = triangulation?.indices?.ToArray();
            var sourceAreas = triangulation?.areas?.ToArray();

            if (sourceVertices == null || sourceIndices == null || sourceAreas == null)
            {
                Plugin.Logger.LogWarning("[NavSurface] NavMesh triangulation came back empty");
                return null;
            }

            var kept = new List<int>(sourceIndices.Length);

            for (var triangle = 0; triangle * 3 + 2 < sourceIndices.Length; triangle++)
            {
                // Area 0 is Walkable, matching WalkableAreaMask. Everything else — Jump and Ladder
                // off-mesh links especially — is surface the scan must not be dragged across.
                if (triangle >= sourceAreas.Length || sourceAreas[triangle] != 0)
                    continue;

                var a = sourceVertices[sourceIndices[triangle * 3]];
                var b = sourceVertices[sourceIndices[triangle * 3 + 1]];
                var c = sourceVertices[sourceIndices[triangle * 3 + 2]];

                // Walls and slivers have no XZ area; they can never be located or crossed, and
                // keeping them only slows the spatial index down.
                var area = (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);

                if (Mathf.Abs(area) < 1e-5f)
                    continue;

                kept.Add(sourceIndices[triangle * 3]);
                kept.Add(sourceIndices[triangle * 3 + 1]);
                kept.Add(sourceIndices[triangle * 3 + 2]);
            }

            if (kept.Count == 0)
            {
                Plugin.Logger.LogWarning("[NavSurface] Triangulation contained no walkable triangles");
                return null;
            }

            var surface = new NavSurface(sourceVertices, kept.ToArray());

            Plugin.Logger.LogDebug(
                $"[NavSurface] Extracted {surface.TriangleCount} walkable triangles " +
                $"from {sourceIndices.Length / 3} total, {sourceVertices.Length} vertices");

            return surface;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"[NavSurface] Could not extract the navmesh triangulation: {e.Message}");
            return null;
        }
    }

    #endregion
}
