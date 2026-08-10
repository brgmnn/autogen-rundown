using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Tests for TravelPathGenerator.SubdivideSaggingSegments.
///
/// The real generator samples the NavMesh, but the sampler is injected so the recursion can be
/// driven by synthetic surfaces here — the test project only references UnityEngine.CoreModule,
/// not the AI module.
/// </summary>
[TestClass]
public class PathSubdivision_Tests
{
    /// <summary>
    /// Flat, then a 45 degree rise from x=0 to x=4, then flat at y=4.
    /// The crest at x=4 is a convex break: a chord spanning it passes below the surface.
    /// </summary>
    private static Vector3 Staircase(Vector3 p)
        => new(p.x, Mathf.Clamp(p.x, 0f, 4f), p.z);

    private static Vector3 Flat(Vector3 p) => new(p.x, 0f, p.z);

    private static float MaxSag(IReadOnlyList<Vector3> points, Func<Vector3, Vector3> sampler)
    {
        var worst = 0f;

        for (var i = 1; i < points.Count; i++)
        {
            var mid = (points[i - 1] + points[i]) * 0.5f;
            worst = Mathf.Max(worst, sampler(mid).y - mid.y);
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

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, new Vector3(0f, 0f, 0f), Flat);

        Assert.AreEqual(input.Count, result.Count);
    }

    [TestMethod]
    public void Test_Staircase_InsertsWaypointsAtCrest()
    {
        // A single chord from the bottom of the stairs to well out onto the upper floor.
        // This is the exact shape Unity's funnel algorithm produces for a straight stair run.
        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(8f, 4f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(
            input, new Vector3(0f, 0f, 0f), Staircase);

        Assert.IsTrue(
            result.Count > input.Count,
            $"Expected extra waypoints on the staircase, got {result.Count}");

        // At least one inserted point should land near the crest at x=4.
        Assert.IsTrue(
            result.Any(p => Mathf.Abs(p.x - 4f) < 1f),
            "Expected a waypoint near the crest at x=4");
    }

    [TestMethod]
    public void Test_Staircase_ResidualSagWithinTolerance()
    {
        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(8f, 4f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(
            input, new Vector3(0f, 0f, 0f), Staircase);

        var before = MaxSag(input, Staircase);
        var after = MaxSag(result, Staircase);

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
            new(0f, 4f, 0f),
            new(8f, 4f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(
            input, new Vector3(0f, 4f, 0f), Flat);

        Assert.AreEqual(input.Count, result.Count);
    }

    [TestMethod]
    public void Test_NoSegmentShorterThanMinimum()
    {
        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(8f, 4f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(
            input, new Vector3(0f, 0f, 0f), Staircase);

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
        // A sampler that always reports the surface far above the chord would recurse forever
        // without the depth bound. One segment may therefore gain at most 2^depth - 1 points.
        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(20f, 0f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(
            input, new Vector3(0f, 0f, 0f), p => new Vector3(p.x, p.y + 10f, p.z));

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

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, closing, Flat);

        Assert.AreNotEqual(closing, result[result.Count - 1]);
    }
}
