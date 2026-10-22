using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Joining a traced route into the circuit CP_BasicMovable walks.
///
/// The game wraps the loop from the last waypoint to the first, and cannot survive a segment there
/// shorter than one frame of travel: the lerp overshoots two waypoints in one step and indexes past
/// the end of ScanPositions. So the loop must start at the scan itself and no segment of it — the
/// closing one included — may be degenerate.
/// </summary>
[TestClass]
public class CloseLoop_Tests
{
    private static void AssertEverySegmentIsReal(List<Vector3> loop)
    {
        for (var i = 0; i < loop.Count; i++)
        {
            var next = (i + 1) % loop.Count;
            var length = (loop[next] - loop[i]).magnitude;

            Assert.IsTrue(
                length >= TravelScanRegistry.MinSegmentLength,
                $"Segment {i}→{next} is {length:F3}m: {SurfaceFixtures.Show(loop)}");
        }
    }

    [TestMethod]
    public void Test_CloseLoop_StartsAtTheScanWithoutDuplicatingItAtEitherEnd()
    {
        // The route as GenerateLoop produces it: it leaves from, and arrives back at, the scan's
        // position as placed on the floor — 6cm below where the scan actually sits, which is the
        // "Loop closing distance: 0.06m" in the logs.
        var scan = new Vector3(0f, 0.06f, 0f);

        var route = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(2f, 0f, 0f),
            new(2f, 0f, 2f),
            new(0f, 0f, 2f),
            new(0f, 0f, 0f)
        };

        var loop = TravelPathGenerator.CloseLoop(route, scan);

        Assert.AreEqual(scan, loop[0], "The loop must start where the scan is");
        Assert.AreEqual(route.Count - 1, loop.Count, "Only the two copies of the scan's position go");
        AssertEverySegmentIsReal(loop);
    }

    [TestMethod]
    public void Test_CloseLoop_DropsPointsThatWouldMakeADegenerateSegment()
    {
        // Raw pathfinder corners are not decimated, so nothing upstream stops two of them landing
        // on top of each other or on top of the scan.
        var scan = Vector3.zero;

        var route = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(0.01f, 0f, 0f),
            new(2f, 0f, 0f),
            new(2f, 0f, 0.001f),
            new(2f, 0f, 2f),
            new(0f, 0f, 0.02f),
            new(0f, 0f, 0f)
        };

        var loop = TravelPathGenerator.CloseLoop(route, scan);

        Assert.AreEqual(3, loop.Count, SurfaceFixtures.Show(loop));
        AssertEverySegmentIsReal(loop);
    }
}
