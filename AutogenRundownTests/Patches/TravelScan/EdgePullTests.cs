using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Tests for edge-pulling and for the probe reporting failure.
///
/// Both existed as silent defects: waypoints were pulled clear of edges one at a time with no
/// regard for their neighbours, and the probe returned the input unchanged when it could not find
/// the surface. Between them, two waypoints 2m apart ended up 6m apart with a hole in the mesh
/// between them, and a run of waypoints drifted under the floor while every check reported success.
/// </summary>
[TestClass]
public class EdgePull_Tests
{
    private static string Show(Vector3 p) => $"({p.x:F2}, {p.y:F2}, {p.z:F2})";

    /// <summary>
    /// Flat floor with a narrow slot from x=-0.5 to x=0.5. Waypoints either side of it are on the
    /// floor, but the straight line between them is not walkable.
    /// </summary>
    private static FakeSurfaceProbe Slotted()
        => new(FakeSurfaceProbe.FlatWithHole(0f, -0.5f, 0.5f, -50f, 50f));

    /// <summary>
    /// Flat floor with a hole wider than twice SurfaceSampleRadius, so a point in the middle is
    /// genuinely out of reach. A narrower hole would still snap to its rim — which is correct,
    /// and is what NavMesh.SamplePosition does too.
    /// </summary>
    private static FakeSurfaceProbe WideHole()
        => new(FakeSurfaceProbe.FlatWithHole(0f, -2f, 2f, -50f, 50f));

    [TestMethod]
    public void Test_TrySnap_ReportsFailureOverAHole()
    {
        var probe = WideHole();

        Assert.IsFalse(
            probe.TrySnap(new Vector3(0f, 0f, 0f), 0f, 0f, out _),
            "A point well inside the hole should report no surface");

        Assert.IsTrue(
            probe.TrySnap(new Vector3(5f, 0f, 0f), 0f, 0f, out _),
            "A point on the floor should snap");
    }

    [TestMethod]
    public void Test_TrySnap_SnapsToTheRimOfASmallHole()
    {
        // Documents the boundary: a hole narrower than the sample radius is not a failure, the
        // nearest surface is simply its rim. That mirrors NavMesh.SamplePosition exactly.
        var probe = Slotted();

        Assert.IsTrue(probe.TrySnap(new Vector3(0f, 0f, 0f), 0f, 0f, out _));
    }

    [TestMethod]
    public void Test_TrySnap_ReportsFailureOnAnotherFloor()
    {
        // Only the upper floor exists here; probing from far below must not accept it.
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(10f));

        Assert.IsFalse(probe.TrySnap(new Vector3(0f, 0f, 0f), 0f, 0f, out _));
    }

    [TestMethod]
    public void Test_EdgePull_RejectsShiftsThatStraddleAHole()
    {
        // The exact reported failure. Two waypoints 2m apart either side of a hole, each pulled
        // 2m directly away from it — the pull direction is the edge normal, so neighbours on
        // opposite sides get pushed apart. Accepting both turns a 2m step into a 6m one with the
        // hole squarely in the middle.
        var probe = Slotted();
        probe.Pull = p => p.x < 0f
            ? p + new Vector3(-TravelScanRegistry.EdgeDistance, 0f, 0f)
            : p + new Vector3(TravelScanRegistry.EdgeDistance, 0f, 0f);

        var points = new List<Vector3>
        {
            new(-1f, 0f, 0f),
            new(1f, 0f, 0f)
        };

        var result = TravelPathGenerator.PullFromEdges(points, points[0], probe, out var rejected);

        Assert.IsTrue(rejected > 0, "At least one pull should have been rejected");

        var span = (result[1] - result[0]).magnitude;

        Assert.IsTrue(
            span <= TravelScanRegistry.MaxSegmentLength,
            $"Edge pull stretched the segment to {span:F2}m: {Show(result[0])} → {Show(result[1])}");
    }

    [TestMethod]
    public void Test_EdgePull_NeverMakesASegmentWorse()
    {
        // A segment that was already unwalkable in the input stays the repair pass's problem.
        // What edge-pulling must never do is break one that was fine.
        var probe = Slotted();
        probe.Pull = p => p.x < 0f
            ? p + new Vector3(-TravelScanRegistry.EdgeDistance, 0f, 0f)
            : p + new Vector3(TravelScanRegistry.EdgeDistance, 0f, 0f);

        var points = new List<Vector3>
        {
            new(-3f, 0f, 0f),
            new(-1f, 0f, 0f),
            new(1f, 0f, 0f),
            new(3f, 0f, 0f)
        };

        var wasWalkable = new bool[points.Count];
        for (var i = 1; i < points.Count; i++)
            wasWalkable[i] = probe.IsWalkable(points[i - 1], points[i]);

        var result = TravelPathGenerator.PullFromEdges(points, points[0], probe, out _);

        for (var i = 1; i < result.Count; i++)
        {
            if (!wasWalkable[i])
                continue;

            Assert.IsTrue(
                probe.IsWalkable(result[i - 1], result[i]),
                $"Edge pull broke segment {i - 1}→{i}, which was walkable before: " +
                $"{Show(result[i - 1])} → {Show(result[i])}");

            Assert.IsTrue(
                (result[i] - result[i - 1]).magnitude <= TravelScanRegistry.MaxSegmentLength,
                $"Edge pull stretched segment {i - 1}→{i} past the maximum");
        }
    }

    [TestMethod]
    public void Test_EdgePull_StillAcceptsHarmlessShifts()
    {
        // Guard against over-rejecting: on open floor a small sideways nudge keeps the chain
        // perfectly walkable and should be taken.
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));
        probe.Pull = p => p + new Vector3(0f, 0f, 0.5f);

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(2f, 0f, 0f),
            new(4f, 0f, 0f)
        };

        var result = TravelPathGenerator.PullFromEdges(points, points[0], probe, out var rejected);

        Assert.AreEqual(0, rejected, "No pull should have been rejected on open floor");

        for (var i = 0; i < result.Count; i++)
            Assert.AreEqual(0.5f, result[i].z, 0.001f, $"Waypoint {i} should have been nudged");
    }

    [TestMethod]
    public void Test_EdgePull_NoPullIsANoOp()
    {
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(2f, 0f, 0f)
        };

        var result = TravelPathGenerator.PullFromEdges(points, points[0], probe, out var rejected);

        Assert.AreEqual(0, rejected);
        CollectionAssert.AreEqual(points, result);
    }

    [TestMethod]
    public void Test_Walk_NeverEmitsAnOffMeshWaypoint()
    {
        // The direct regression for the under-floor drift. The probe used to return the raw target
        // when it found nothing, so the cursor advanced into thin air — and because the next probe
        // referenced that drifted height, it kept going.
        var probe = new FakeSurfaceProbe(
            FakeSurfaceProbe.FlatWithHole(0f, 4f, 10f, -50f, 50f));

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)   // straight across the hole
        };

        var result = TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);

        foreach (var point in result)
            Assert.IsTrue(
                probe.TrySnap(point, point.y, point.y, out _),
                $"Walk emitted a waypoint that is not on the surface: {Show(point)}");
    }

    [TestMethod]
    public void Test_Walk_BlockedSnapTakesTheDetour()
    {
        var probe = new FakeSurfaceProbe(
            FakeSurfaceProbe.FlatWithHole(0f, 4f, 10f, -5f, 5f));

        // A way around the hole
        probe.Route = new List<Vector3> { new(7f, 0f, 10f) };

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)
        };

        var result = TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);

        Assert.IsTrue(
            result.Any(p => p.z > 5f),
            "Walk should have detoured around the hole rather than stopping at it");

        foreach (var point in result)
            Assert.IsTrue(
                probe.TrySnap(point, point.y, point.y, out _),
                $"Detoured walk emitted an off-surface waypoint: {Show(point)}");
    }
}
