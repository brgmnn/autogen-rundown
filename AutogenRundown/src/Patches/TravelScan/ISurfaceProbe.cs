using UnityEngine;
using UnityEngine.AI;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// The two NavMesh questions the path generator needs to ask. Abstracted so the geometry in
/// TravelPathGenerator can be unit tested against a synthetic multi-floor world — the test project
/// only references UnityEngine.CoreModule, not the AI module.
/// </summary>
public interface ISurfaceProbe
{
    /// <summary>
    /// Projects a point onto the walkable surface.
    ///
    /// The two heights play different roles, and both are needed:
    ///
    /// <paramref name="referenceY"/> is a height already known to be on the surface, and it
    /// constrains what may be *accepted* — a result further than MaxSurfaceSnapRise from it has
    /// crossed to another floor and is rejected.
    ///
    /// <paramref name="preferredY"/> is where the route is heading, and it biases where we
    /// *look*. Without it the probe always resolves to whichever surface is nearest the current
    /// height, so at the foot of a staircase it keeps picking the floor — the stair tread is
    /// always slightly further away — and the walk never starts climbing.
    ///
    /// Returns the point unchanged when no acceptable surface is found.
    /// </summary>
    Vector3 Snap(Vector3 point, float referenceY, float preferredY);

    /// <summary>
    /// True when you can walk in a straight line from <paramref name="a"/> to <paramref name="b"/>
    /// without leaving the walkable surface.
    ///
    /// This is the predicate the scan actually needs: CP_BasicMovable.DoMoveScanner lerps straight
    /// between consecutive positions, so a segment that fails this test is one the scan will
    /// travel through geometry. It is also the game's own test — LG_BuildNodeVolumeJobData's
    /// NodeLinksJob gates every AI graph link on a navmesh raycast coming back clear.
    /// </summary>
    bool IsWalkable(Vector3 a, Vector3 b);

    /// <summary>
    /// Nudges a waypoint away from the nearest surface edge so the scan bubble doesn't push
    /// players into walls or off ledges, returning the position unchanged when it can't be done
    /// safely.
    /// </summary>
    Vector3 PullFromEdge(Vector3 position, float minDistance);

    /// <summary>
    /// Interior turning points of a walkable route from <paramref name="from"/> to
    /// <paramref name="to"/> — the way around an obstruction.
    ///
    /// Returns false when no route exists, and also when the route is a straight line with
    /// nothing in between. That second case matters to callers that splice the result into a
    /// route they are walking: an empty splice would leave them facing the same blocked target
    /// with nothing changed, and they would loop.
    /// </summary>
    bool TryFindRoute(Vector3 from, Vector3 to, out List<Vector3> via);
}

/// <summary>
/// Geometry shared by the real probe and its test doubles.
/// </summary>
public static class SurfaceGeometry
{
    /// <summary>
    /// True when the height change from a to b is one the terrain could actually have.
    ///
    /// This is separate from the navmesh raycast because a raycast cannot see it.
    /// NavMesh.Raycast walks the *2D projection* of the mesh, so two waypoints stacked at nearly
    /// the same XZ on different floors look like a zero-length walk that never leaves the mesh —
    /// it reports them as connected. The game guards its own AI graph links the same way:
    /// LG_BuildNodeVolumeJobData.NodeLinksJob requires both a Y band and a clear raycast.
    /// </summary>
    public static bool IsSlopeWalkable(Vector3 a, Vector3 b)
    {
        var delta = b - a;
        var horizontal = new Vector2(delta.x, delta.z).magnitude;

        return Mathf.Abs(delta.y)
               <= horizontal * TravelScanRegistry.MaxSlopeRatio + TravelScanRegistry.MaxStepUp;
    }

    /// <summary>
    /// How far the chord between two waypoints may pass below the walkable surface, tightened by
    /// how steep that segment is.
    ///
    /// A flat chord dipping 0.25m is invisible against a scan several metres across. The same
    /// 0.25m on a staircase is a step nose punched through the floor — and stepped geometry sags
    /// by about half a step height, which sits right under a flat 0.25m threshold and slips
    /// through unnoticed.
    ///
    /// Both the generator and the debug overlay call this, so what gets subdivided and what gets
    /// drawn as over-tolerance cannot drift apart.
    /// </summary>
    public static float MaxSagFor(Vector3 a, Vector3 b)
    {
        var delta = b - a;
        var horizontal = new Vector2(delta.x, delta.z).magnitude;

        // A purely vertical segment has no meaningful slope ratio; treat it as the steepest the
        // terrain can be rather than dividing by ~zero.
        var slope = horizontal < 0.001f
            ? TravelScanRegistry.MaxSlopeRatio
            : Mathf.Abs(delta.y) / horizontal;

        return TravelScanRegistry.MaxChordSag
               / (1f + TravelScanRegistry.SagSlopeFactor * slope);
    }
}

/// <summary>
/// Production <see cref="ISurfaceProbe"/> backed by Unity's NavMesh.
/// </summary>
public sealed class NavMeshSurfaceProbe : ISurfaceProbe
{
    public static readonly NavMeshSurfaceProbe Instance = new();

    public Vector3 Snap(Vector3 point, float referenceY, float preferredY)
    {
        // SamplePosition only ever returns the single nearest surface, so the bias has to be
        // applied by moving the probe rather than by choosing between candidates. Clamping to
        // the accept band keeps the probe from reaching a floor the result would be rejected for.
        var probeY = Mathf.Clamp(
            preferredY,
            referenceY - TravelScanRegistry.MaxSurfaceSnapRise,
            referenceY + TravelScanRegistry.MaxSurfaceSnapRise);

        var probe = new Vector3(
            point.x,
            probeY + TravelScanRegistry.SurfaceSampleLift,
            point.z);

        if (!NavMesh.SamplePosition(
                probe,
                out var hit,
                TravelScanRegistry.SurfaceSampleRadius,
                TravelScanRegistry.WalkableAreaMask))
            return point;

        // A large correction relative to the reference means we found a different floor, not the
        // surface under this point. Stacked geometry makes that a real risk: the whole level is
        // baked into a single NavMeshData, so both floors of a stairwell are polygons at the
        // same XZ.
        if (Mathf.Abs(hit.position.y - referenceY) > TravelScanRegistry.MaxSurfaceSnapRise)
            return point;

        return hit.position;
    }

    public bool IsWalkable(Vector3 a, Vector3 b)
    {
        if (!SurfaceGeometry.IsSlopeWalkable(a, b))
            return false;

        return !NavMesh.Raycast(a, b, out _, TravelScanRegistry.WalkableAreaMask);
    }

    /// <summary>
    /// Every result is checked back against <paramref name="position"/>: this runs after the
    /// surface walk has already established the correct floor, so a shifted point that lands more
    /// than MaxSurfaceSnapRise away vertically has crossed to another one and is discarded. It is
    /// also verified walkable from the original — a pull that has to cross a gap is not one we can
    /// use, because the scan would lerp across that gap.
    /// </summary>
    public Vector3 PullFromEdge(Vector3 position, float minDistance)
    {
        var mask = TravelScanRegistry.WalkableAreaMask;

        if (!NavMesh.FindClosestEdge(position, out var hitNear, mask))
            return position;

        if (hitNear.distance >= minDistance)
            return position;

        // Measure distance to opposite wall via raycast in pull direction
        var maxProbe = minDistance * 3f;
        var rayTarget = position + hitNear.normal * maxProbe;
        float distToFar;

        if (NavMesh.Raycast(position, rayTarget, out var rayHit, mask))
            distToFar = rayHit.distance;
        else
            distToFar = maxProbe; // No opposite wall — wide open

        var corridorWidth = hitNear.distance + distToFar;

        if (corridorWidth >= minDistance * 2f)
        {
            // Wide enough for full pull
            var pullAmount = minDistance - hitNear.distance;
            var pulled = position + hitNear.normal * pullAmount;

            if (NavMesh.SamplePosition(pulled, out var sample, 0.5f, mask))
                return AcceptShift(position, sample.position);

            return position;
        }

        // Narrow corridor: center between the two walls
        var targetDist = corridorWidth / 2f;
        var shift = targetDist - hitNear.distance;
        var centered = position + hitNear.normal * shift;

        if (NavMesh.SamplePosition(centered, out var snap, 1f, mask))
            return AcceptShift(position, snap.position);

        return position;
    }

    private Vector3 AcceptShift(Vector3 original, Vector3 shifted)
    {
        if (Mathf.Abs(shifted.y - original.y) > TravelScanRegistry.MaxSurfaceSnapRise)
            return original;

        return IsWalkable(original, shifted) ? shifted : original;
    }

    public bool TryFindRoute(Vector3 from, Vector3 to, out List<Vector3> via)
    {
        via = new List<Vector3>();

        var path = new NavMeshPath();

        if (!NavMesh.CalculatePath(from, to, TravelScanRegistry.WalkableAreaMask, path)
            || path.status != NavMeshPathStatus.PathComplete)
            return false;

        var corners = path.corners;

        // Skip corners[0] (== from) and the last (== to); the caller already has both.
        for (var i = 1; i < corners.Length - 1; i++)
            via.Add(corners[i]);

        return via.Count > 0;
    }
}
