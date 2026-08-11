using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Tests for TravelPathGenerator.SubdivideSaggingSegments.
///
/// The real generator samples the NavMesh, but the surface probe is injected so the recursion can
/// be driven by synthetic geometry here — the test project only references UnityEngine.CoreModule,
/// not the AI module.
/// </summary>
[TestClass]
public class PathSubdivision_Tests
{
    /// <summary>
    /// Flat, then a 45 degree rise from x=0 to x=4, then flat at y=4.
    /// The crest at x=4 is a convex break: a chord spanning it passes below the surface.
    /// </summary>
    private static FakeSurfaceProbe Staircase()
        => new(FakeSurfaceProbe.Stair(0f, 4f, 4f));

    private static FakeSurfaceProbe Flat(float y = 0f)
        => new(FakeSurfaceProbe.Flat(y));

    private static float SurfaceYAt(ISurfaceProbe probe, Vector3 point, float referenceY)
    {
        probe.TrySnap(point, referenceY, point.y, out var snapped);
        return snapped.y;
    }

    private static float MaxSag(IReadOnlyList<Vector3> points, ISurfaceProbe probe)
    {
        var worst = 0f;

        for (var i = 1; i < points.Count; i++)
        {
            var mid = (points[i - 1] + points[i]) * 0.5f;
            worst = Mathf.Max(worst, SurfaceYAt(probe, mid, points[i - 1].y) - mid.y);
        }

        return worst;
    }

    [TestMethod]
    public void Test_FlatSurface_InsertsNothing()
    {
        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(4f, 0f, 0f),
            new(8f, 0f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(
            input, new Vector3(0f, 0f, 0f), Flat());

        Assert.AreEqual(input.Count, result.Count);
    }

    /// <summary>
    /// A pair of waypoints straddling the crest at x=4, spaced as WalkSurface would emit them.
    ///
    /// Subdivision runs on walk output, not raw NavMesh corners, so its input is always short
    /// segments whose endpoints are already on the surface. It deliberately cannot repair a long
    /// chord that starts a floor away from the geometry — reaching that far is what let the old
    /// resampler snap onto the wrong floor in the first place.
    /// </summary>
    private static List<Vector3> CrestPair() => new()
    {
        new Vector3(3.2f, 3.2f, 0f),   // on the stair
        new Vector3(5.2f, 4f, 0f)      // out onto the flat above
    };

    [TestMethod]
    public void Test_Staircase_InsertsWaypointsAtCrest()
    {
        var input = CrestPair();

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], Staircase());

        Assert.IsTrue(
            result.Count > input.Count,
            $"Expected extra waypoints on the staircase, got {result.Count}");

        // The inserted point should land on the crest at x=4, at the full upper height.
        Assert.IsTrue(
            result.Any(p => Mathf.Abs(p.x - 4f) < 1f && Mathf.Abs(p.y - 4f) < 0.1f),
            "Expected a waypoint on the crest at x=4, y=4");
    }

    [TestMethod]
    public void Test_Staircase_ResidualSagWithinTolerance()
    {
        var probe = Staircase();
        var input = CrestPair();

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], probe);

        var before = MaxSag(input, probe);
        var after = MaxSag(result, probe);

        Assert.IsTrue(before > TravelScanRegistry.MaxChordSag, "Test fixture should sag to start with");
        Assert.IsTrue(
            after <= TravelScanRegistry.MaxChordSag,
            $"Residual sag {after} should be within {TravelScanRegistry.MaxChordSag} (was {before})");
    }

    [TestMethod]
    public void Test_SurfaceBelowChord_InsertsNothing()
    {
        // Concave break — the surface drops away under the chord. The scan floats slightly,
        // which is harmless, so subdivision must not fire.
        var input = new List<Vector3>
        {
            new(0f, 0.5f, 0f),
            new(8f, 0.5f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(
            input, new Vector3(0f, 0.5f, 0f), Flat());

        Assert.AreEqual(input.Count, result.Count);
    }

    [TestMethod]
    public void Test_NoSegmentShorterThanMinimum()
    {
        var input = CrestPair();

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], Staircase());

        for (var i = 1; i < result.Count; i++)
        {
            var length = (result[i] - result[i - 1]).magnitude;

            Assert.IsTrue(
                length >= TravelScanRegistry.MinSegmentLength,
                $"Segment {i} is {length}m, under the {TravelScanRegistry.MinSegmentLength}m minimum");
        }
    }

    [TestMethod]
    public void Test_RespectsMaxSubdivisionDepth()
    {
        // A probe that always reports the surface far above the chord would recurse forever
        // without the depth bound. One segment may therefore gain at most 2^depth - 1 points.
        var runaway = new FakeSurfaceProbe(p => p.y + 10f);

        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(
            input, new Vector3(0f, 0f, 0f), runaway);

        var maxInserted = (1 << TravelScanRegistry.MaxSubdivisionDepth) - 1;

        // input[0], plus at most maxInserted + 1 points for the one real segment, plus the
        // wrap-around segment back to the closing point.
        Assert.IsTrue(
            result.Count <= 1 + (maxInserted + 1) * 2,
            $"Subdivision produced {result.Count} points, beyond the depth bound");
    }

    [TestMethod]
    public void Test_ClosingPointNotAppended()
    {
        // GenerateLoop's contract: the source position is re-inserted at index 0 by the caller,
        // so it must never appear at the end of the returned list.
        var closing = new Vector3(0f, 0f, 0f);

        var input = new List<Vector3>
        {
            closing,
            new(4f, 0f, 0f),
            new(8f, 0f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, closing, Flat());

        Assert.AreNotEqual(closing, result[result.Count - 1]);
    }
}
