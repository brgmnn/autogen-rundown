using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Tests for the surface walk and the walkability repair pass, against a synthetic two-floor
/// world — the geometry that produced the original bug.
///
/// Layout, looking along +X:
///
///     upper floor y=4                    ┌──────────────
///                              stair    ╱
///     lower floor y=0  ───────┴────────┘        (void under the upper floor)
///                      0      8       12       20
///
/// The mezzanine section (x in 12..20) has the upper floor at y=4 AND the lower floor at y=0
/// directly beneath it — exactly the stacked geometry that lets an unguarded probe pick the
/// wrong floor.
/// </summary>
[TestClass]
public class SurfaceWalk_Tests
{
    private const float UpperY = 4f;

    private static FakeSurfaceProbe TwoFloors()
        => new(
            // Lower floor spans the whole world
            FakeSurfaceProbe.Flat(0f),
            // Stair rises from x=8 to x=12, then the upper floor continues to x=20
            p => p.x < 8f ? null
                : p.x >= 12f ? UpperY
                : UpperY * (p.x - 8f) / 4f);

    private static bool OnUpperFloor(Vector3 p) => Mathf.Abs(p.y - UpperY) < 0.5f;
    private static bool OnLowerFloor(Vector3 p) => Mathf.Abs(p.y) < 0.5f;

    /// <summary>
    /// Vector3.ToString() reaches into UnityEngine.SharedInternalsModule, which isn't available
    /// outside the game — format coordinates by hand for assertion messages.
    /// </summary>
    private static string Show(Vector3 p) => $"({p.x:F2}, {p.y:F2}, {p.z:F2})";

    [TestMethod]
    public void Test_Walk_ClimbsStairWithoutJumpingFloors()
    {
        var probe = TwoFloors();

        // One raw corner pair, as the funnel algorithm would produce for a straight run:
        // bottom of the stairs to well out onto the upper floor.
        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, UpperY, 0f)
        };

        var result = TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);

        Assert.IsTrue(result.Count >= 2, $"Expected a walked path, got {result.Count} waypoints");

        // The walk must actually climb — a path that stalls at the bottom of the stair would
        // pass the no-floor-jumping assertions below for the wrong reason.
        Assert.IsTrue(
            OnUpperFloor(result[result.Count - 1]),
            $"Walk should finish on the upper floor, ended at {Show(result[result.Count - 1])}");

        Assert.IsTrue(
            result.Any(p => OnLowerFloor(p)),
            "Walk should start on the lower floor");

        // Every waypoint must sit on one floor or the other, never in between.
        foreach (var point in result)
            Assert.IsTrue(
                OnUpperFloor(point) || OnLowerFloor(point) || (point.x > 8f && point.x < 12f),
                $"Waypoint {Show(point)} is not on a floor or the stair");

        // And every consecutive pair must be connected.
        for (var i = 1; i < result.Count; i++)
            Assert.IsTrue(
                probe.IsWalkable(result[i - 1], result[i]),
                $"Segment {i - 1}→{i} is not walkable: {Show(result[i - 1])} → {Show(result[i])}");
    }

    [TestMethod]
    public void Test_Walk_NeverDropsBetweenFloors()
    {
        var probe = TwoFloors();

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, UpperY, 0f)
        };

        var result = TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);

        // The failure being guarded against: two adjacent waypoints on different floors, which
        // renders as the scan dropping vertically through the ceiling.
        for (var i = 1; i < result.Count; i++)
        {
            var dropsFloor = OnUpperFloor(result[i - 1]) && OnLowerFloor(result[i]);
            var climbsFloor = OnLowerFloor(result[i - 1]) && OnUpperFloor(result[i]);

            Assert.IsFalse(
                dropsFloor || climbsFloor,
                $"Waypoints {i - 1} and {i} are on different floors: " +
                $"{Show(result[i - 1])} → {Show(result[i])}");
        }
    }

    [TestMethod]
    public void Test_Walk_FlatGroundKeepsStepSpacing()
    {
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)
        };

        var result = TravelPathGenerator.WalkSurface(corners, TravelScanRegistry.StepDistance, probe);

        // On flat ground the walk should behave like plain resampling: ~20m / 2m steps.
        Assert.IsTrue(
            result.Count >= 9 && result.Count <= 12,
            $"Expected roughly 10 waypoints over 20m at 2m steps, got {result.Count}");

        for (var i = 1; i < result.Count; i++)
        {
            var spacing = (result[i] - result[i - 1]).magnitude;

            Assert.IsTrue(
                Mathf.Abs(spacing - TravelScanRegistry.StepDistance) < 0.5f,
                $"Segment {i} is {spacing}m, expected close to {TravelScanRegistry.StepDistance}m");
        }
    }

    [TestMethod]
    public void Test_IsSlopeWalkable_RejectsVerticalDrop()
    {
        // The case NavMesh.Raycast alone cannot detect: two points stacked at the same XZ.
        // The 2D-projected walk never leaves the mesh, so only the slope test catches it.
        var upper = new Vector3(15f, 4f, 0f);
        var lower = new Vector3(15.1f, 0f, 0f);

        Assert.IsFalse(SurfaceGeometry.IsSlopeWalkable(upper, lower));
    }

    [TestMethod]
    public void Test_IsSlopeWalkable_AcceptsStairSegment()
    {
        // A normal 2m step up a 45 degree stair must stay walkable.
        var a = new Vector3(0f, 0f, 0f);
        var b = new Vector3(1.41f, 1.41f, 0f);

        Assert.IsTrue(SurfaceGeometry.IsSlopeWalkable(a, b));
    }

    [TestMethod]
    public void Test_IsSlopeWalkable_AcceptsSmallStepUp()
    {
        // agentClimb allows a step-up over no horizontal distance at all.
        var a = new Vector3(0f, 0f, 0f);
        var b = new Vector3(0f, 0.4f, 0f);

        Assert.IsTrue(SurfaceGeometry.IsSlopeWalkable(a, b));
    }

    [TestMethod]
    public void Test_Repair_LeavesWalkablePathAlone()
    {
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(2f, 0f, 0f),
            new(4f, 0f, 0f)
        };

        var result = TravelPathGenerator.RepairUnwalkableSegments(
            points, new Vector3(0f, 0f, 0f), probe);

        CollectionAssert.AreEqual(points, result);
    }

    [TestMethod]
    public void Test_Repair_ClosingPointNotAppended()
    {
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));
        var closing = new Vector3(0f, 0f, 0f);

        var points = new List<Vector3>
        {
            closing,
            new(2f, 0f, 0f),
            new(4f, 0f, 0f)
        };

        var result = TravelPathGenerator.RepairUnwalkableSegments(points, closing, probe);

        Assert.AreNotEqual(closing, result[result.Count - 1]);
    }

    [TestMethod]
    public void Test_Probe_SnapPrefersReferenceFloor()
    {
        var probe = TwoFloors();

        // Standing on the upper floor over the mezzanine, both floors are candidates at this XZ.
        var onMezzanine = new Vector3(16f, UpperY, 0f);

        Assert.IsTrue(probe.TrySnap(onMezzanine, UpperY, UpperY, out var high));
        Assert.AreEqual(UpperY, high.y, 0.01f);
        Assert.IsTrue(probe.TrySnap(onMezzanine, 0f, 0f, out var low));
        Assert.AreEqual(0f, low.y, 0.01f);
    }

    [TestMethod]
    public void Test_Probe_SnapRejectsFarFloor()
    {
        var probe = TwoFloors();

        // A point on the upper floor probed with the upper floor as reference must never come
        // back on the lower one, even though the lower one exists directly beneath it.
        var onMezzanine = new Vector3(16f, UpperY, 0f);
        probe.TrySnap(onMezzanine, UpperY, UpperY, out var snapped);

        Assert.IsFalse(OnLowerFloor(snapped), $"Snapped onto the wrong floor: {Show(snapped)}");
    }
}
