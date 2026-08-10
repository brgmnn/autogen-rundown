using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Tests for coverage: the walk must not silently lose stretches of route.
///
/// This is the class that reproduces the reported bug. A wall between two corners made the surface
/// probe pull the cursor back off its line every sub-step, so the walk spun in place until its
/// iteration guard, abandoned the corner, and emitted nothing further — leaving one huge chord
/// across the zone.
/// </summary>
[TestClass]
public class WalkCoverage_Tests
{
    /// <summary>
    /// A flat floor with a wall across the middle of the direct route. Walking straight from
    /// x=0 to x=20 along z=0 runs into it; the way around is via z=10.
    /// </summary>
    private static FakeSurfaceProbe Walled()
        => new(FakeSurfaceProbe.FlatWithHole(0f, 8f, 12f, -5f, 5f));

    private static string Show(Vector3 p) => $"({p.x:F2}, {p.y:F2}, {p.z:F2})";

    private static float PathLength(IReadOnlyList<Vector3> points)
    {
        var total = 0f;

        for (var i = 1; i < points.Count; i++)
            total += (points[i] - points[i - 1]).magnitude;

        return total;
    }

    [TestMethod]
    public void Test_BlockedWalk_StallsImmediatelyNotAfterThousandsOfProbes()
    {
        var probe = Walled();

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)   // straight through the wall
        };

        TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);

        // ~16 sub-steps to reach the wall, plus a handful of overhead. The old code spun 4096
        // times against the obstruction before giving up.
        Assert.IsTrue(
            probe.SnapCalls < 100,
            $"Walk probed {probe.SnapCalls} times against a wall; it should notice immediately");
    }

    [TestMethod]
    public void Test_BlockedWalk_StillReachesTheCornerViaDetour()
    {
        var probe = Walled();

        // The pathfinder's way around the wall
        probe.Route = new List<Vector3>
        {
            new(10f, 0f, 10f)
        };

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)
        };

        var result = TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);

        Assert.IsTrue(result.Count > 2, $"Expected a walked detour, got {result.Count} waypoints");

        // It should have gone around, i.e. visited the far side of the wall
        Assert.IsTrue(
            result.Any(p => p.z > 5f),
            "Walk should have detoured around the wall rather than stopping at it");

        Assert.IsTrue(
            result.Any(p => p.x > 12f),
            "Walk should have reached the far side of the wall");
    }

    [TestMethod]
    public void Test_UnobstructedWalk_CoversTheWholeRoute()
    {
        // The assertion that would have caught the reported bug: a clear route must be covered
        // end to end, not truncated part way.
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(40f, 0f, 0f),
            new(40f, 0f, 40f)
        };

        var result = TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);

        var routeLength = PathLength(corners);
        var walkedLength = PathLength(result);

        Assert.IsTrue(
            walkedLength > routeLength * 0.9f,
            $"Walk covered only {walkedLength:F1}m of a {routeLength:F1}m route");
    }

    [TestMethod]
    public void Test_Walk_NeverEmitsAnOverLongSegment()
    {
        var probe = Walled();
        probe.Route = new List<Vector3> { new(10f, 0f, 10f) };

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)
        };

        var walked = TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);
        var result = TravelPathGenerator.RepairUnwalkableSegments(walked, corners[0], probe);

        for (var i = 1; i < result.Count; i++)
        {
            var length = (result[i] - result[i - 1]).magnitude;

            Assert.IsTrue(
                length <= TravelScanRegistry.MaxSegmentLength,
                $"Segment {i - 1}→{i} is {length:F1}m, over the " +
                $"{TravelScanRegistry.MaxSegmentLength}m cap: " +
                $"{Show(result[i - 1])} → {Show(result[i])}");
        }
    }

    [TestMethod]
    public void Test_Repair_ReWalksAnOverLongButWalkableSegment()
    {
        // A long straight chord across open floor passes every walkability test — it is a
        // perfectly valid line. It is still wrong, because it means the walk skipped the route.
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(30f, 0f, 0f)
        };

        Assert.IsTrue(probe.IsWalkable(points[0], points[1]), "Fixture chord should be walkable");

        var result = TravelPathGenerator.RepairUnwalkableSegments(points, points[0], probe);

        Assert.IsTrue(
            result.Count > points.Count,
            $"Expected the 30m chord to be re-walked, got {result.Count} waypoints");

        for (var i = 1; i < result.Count; i++)
            Assert.IsTrue(
                (result[i] - result[i - 1]).magnitude <= TravelScanRegistry.MaxSegmentLength,
                $"Segment {i - 1}→{i} is still over the cap");
    }

    [TestMethod]
    public void Test_Repair_NoSegmentShorterThanMinimum()
    {
        // Re-walking must not leave degenerate hops: DoMoveScanner divides by segment length and
        // ReverseMovement bails out below 0.001m.
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(30f, 0f, 0f),
            new(30.2f, 0f, 0f)
        };

        var result = TravelPathGenerator.RepairUnwalkableSegments(points, points[0], probe);

        for (var i = 1; i < result.Count; i++)
            Assert.IsTrue(
                (result[i] - result[i - 1]).magnitude >= TravelScanRegistry.MinSegmentLength,
                $"Segment {i - 1}→{i} is under the {TravelScanRegistry.MinSegmentLength}m minimum");
    }

    [TestMethod]
    public void Test_Repair_TerminatesWhenNoRouteExists()
    {
        // No detour available and the chord is over-long: repair must still terminate and leave
        // the segment rather than looping.
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(100f, 0f, 0f)
        };

        var result = TravelPathGenerator.RepairUnwalkableSegments(points, points[0], probe);

        Assert.IsTrue(result.Count >= 2, "Repair should keep both endpoints");
        Assert.IsTrue(result.Count < 200, $"Repair produced {result.Count} points, suspiciously many");
    }
}
