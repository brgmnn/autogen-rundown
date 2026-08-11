using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Tests that no stage can open a gap wider than MaxSegmentLength.
///
/// The failure these guard against is indirect and was missed twice. Edge-pulling bunched two
/// waypoints together; the next pass deleted one of them as degenerate; deletions accumulated
/// because the comparison point never advanced; and the resulting gap spanned a hole in the mesh,
/// producing a single 7.3m chord the scan slid straight across. Every link in that chain looked
/// locally reasonable.
/// </summary>
[TestClass]
public class SegmentBounds_Tests
{
    private static string Show(Vector3 p) => $"({p.x:F2}, {p.y:F2}, {p.z:F2})";

    private static FakeSurfaceProbe Flat() => new(FakeSurfaceProbe.Flat(0f));

    private static void AssertWithinBounds(IReadOnlyList<Vector3> points, string what)
    {
        for (var i = 1; i < points.Count; i++)
        {
            var length = (points[i] - points[i - 1]).magnitude;

            Assert.IsTrue(
                length <= TravelScanRegistry.MaxSegmentLength,
                $"{what}: segment {i - 1}→{i} is {length:F2}m, over the " +
                $"{TravelScanRegistry.MaxSegmentLength}m maximum. " +
                $"{Show(points[i - 1])} → {Show(points[i])}");
        }
    }

    [TestMethod]
    public void Test_EdgePull_RejectsShiftsThatBunchNeighbours()
    {
        // Pulling every waypoint hard in the same direction as its neighbour collapses the spacing.
        // Bounding only the upper end let this through, and the deletion cascade did the rest.
        var probe = Flat();
        probe.Pull = p => p + new Vector3(1.9f, 0f, 0f);

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(2f, 0f, 0f),
            new(4f, 0f, 0f),
            new(6f, 0f, 0f)
        };

        var result = TravelPathGenerator.PullFromEdges(points, points[0], probe, out var rejected);

        Assert.IsTrue(rejected > 0, "Pulls that bunch neighbours should be rejected");

        for (var i = 1; i < result.Count; i++)
        {
            var length = (result[i] - result[i - 1]).magnitude;

            Assert.IsTrue(
                length >= TravelScanRegistry.MinSegmentLength,
                $"Edge pull bunched segment {i - 1}→{i} to {length:F2}m");
        }
    }

    [TestMethod]
    public void Test_RemoveBunched_KeepsACloseWaypointWhenDroppingWouldOpenAGap()
    {
        // WalkSurface thins its output at StepDistance/2 (1m). The middle waypoint here is inside
        // that spacing, so on spacing grounds alone it would go — but the next one is 6.5m beyond,
        // and removing the only thing in between leaves a chord over the maximum.
        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(0.5f, 0f, 0f),
            new(6.5f, 0f, 0f)
        };

        var result = TravelPathGenerator.RemoveBunchedPoints(points, 1f);

        Assert.AreEqual(3, result.Count, "The close waypoint should have been kept");
        AssertWithinBounds(result, "After thinning");
    }

    [TestMethod]
    public void Test_RemoveBunched_StillThinsWhenItIsSafe()
    {
        // Guard against over-keeping: with the far point within reach, the close one should go.
        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(0.5f, 0f, 0f),
            new(3f, 0f, 0f)
        };

        var result = TravelPathGenerator.RemoveBunchedPoints(points, 1f);

        Assert.AreEqual(2, result.Count, "The close waypoint should have been thinned out");
    }

    [TestMethod]
    public void Test_RemoveBunched_KeepsAWaypointRatherThanOpeningAGap()
    {
        // Three waypoints where the middle one is close to the first but far from the last.
        // Dropping it on spacing grounds alone would leave a 7m gap.
        var probe = Flat();

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(0.2f, 0f, 0f),
            new(7f, 0f, 0f)
        };

        var result = TravelPathGenerator.RepairUnwalkableSegments(points, points[0], probe);

        AssertWithinBounds(result, "After repair");
    }

    [TestMethod]
    public void Test_Repair_ManyBunchedWaypointsDoNotCollapseIntoOneChord()
    {
        // The shape of the real failure: a run of tightly spaced waypoints followed by distance.
        // Deleting them all is what produced the long chord.
        var probe = Flat();

        var points = new List<Vector3> { new(0f, 0f, 0f) };

        for (var i = 1; i <= 10; i++)
            points.Add(new Vector3(i * 0.2f, 0f, 0f));

        points.Add(new Vector3(12f, 0f, 0f));

        var result = TravelPathGenerator.RepairUnwalkableSegments(points, points[0], probe);

        AssertWithinBounds(result, "After repair");
    }

    [TestMethod]
    public void Test_Repair_OutputIsNeverWorseThanInput()
    {
        // Repair splices a re-walk; if that re-walk gave up partway it contains a jump. Taking it
        // would trade one bad segment for several.
        var probe = new FakeSurfaceProbe(
            FakeSurfaceProbe.FlatWithHole(0f, 4f, 10f, -50f, 50f));

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(14f, 0f, 0f)   // straight across the hole, over-long and unwalkable
        };

        var worstBefore = 0f;
        for (var i = 1; i < points.Count; i++)
            worstBefore = Mathf.Max(worstBefore, (points[i] - points[i - 1]).magnitude);

        var result = TravelPathGenerator.RepairUnwalkableSegments(points, points[0], probe);

        var worstAfter = 0f;
        for (var i = 1; i < result.Count; i++)
            worstAfter = Mathf.Max(worstAfter, (result[i] - result[i - 1]).magnitude);

        Assert.IsTrue(
            worstAfter <= worstBefore + 0.001f,
            $"Repair made the worst segment longer: {worstBefore:F2}m → {worstAfter:F2}m");
    }

    [TestMethod]
    public void Test_Repair_LeavesAGoodPathAlone()
    {
        var probe = Flat();

        var points = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(2f, 0f, 0f),
            new(4f, 0f, 0f)
        };

        var result = TravelPathGenerator.RepairUnwalkableSegments(points, points[0], probe);

        CollectionAssert.AreEqual(points, result);
    }

    [TestMethod]
    public void Test_FullPipeline_KeepsEverySegmentInBounds()
    {
        // Walk, pull and repair together, over geometry with a hole in it — the combination is
        // where the invariant kept slipping.
        var probe = new FakeSurfaceProbe(
            FakeSurfaceProbe.FlatWithHole(0f, 8f, 11f, -3f, 3f));

        probe.Route = new List<Vector3> { new(9.5f, 0f, 6f) };
        probe.Pull = p => p + new Vector3(0f, 0f, 1.5f);

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)
        };

        var walked = TravelPathGenerator.WalkSurface(
            corners, TravelScanRegistry.StepDistance, probe);

        var pulled = TravelPathGenerator.PullFromEdges(walked, corners[0], probe, out _);
        var repaired = TravelPathGenerator.RepairUnwalkableSegments(pulled, corners[0], probe);

        AssertWithinBounds(repaired, "After the full pipeline");

        for (var i = 1; i < repaired.Count; i++)
            Assert.IsTrue(
                (repaired[i] - repaired[i - 1]).magnitude >= TravelScanRegistry.MinSegmentLength,
                $"Segment {i - 1}→{i} is under the minimum length");
    }
}
