using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Tests for sag subdivision across a crest — a ramp meeting a landing, which is where a steep
/// staircase punches the scan through the floor.
///
/// The sag tolerance, MinSegmentLength and MaxSubdivisionDepth are only correct as a *set*: the
/// subdivision floor has to be small enough that the tightest adaptive tolerance is actually
/// reachable, or segments sit permanently over tolerance. These tests assert that relationship
/// rather than the individual numbers, so retuning any one of them fails loudly.
/// </summary>
[TestClass]
public class CrestSubdivision_Tests
{
    /// <summary>Slopes to check, spanning gentle ramp to steeper than the bake's 55° limit.</summary>
    private static readonly float[] Slopes = { 0.5f, 1.0f, 1.43f, 2.0f };

    private static string Show(Vector3 p) => $"({p.x:F2}, {p.y:F2}, {p.z:F2})";

    /// <summary>
    /// A ~2m segment straddling the crest at x=0, as WalkSurface would emit it.
    ///
    /// <paramref name="bias"/> shifts the crest away from the chord's midpoint. That matters more
    /// than it looks: subdivision splits at the *surface* point above the chord midpoint, so with
    /// a perfectly symmetric crest the very first split lands exactly on it and all sag vanishes
    /// in one step, no matter how coarse the subdivision floor is. Real geometry is not obliging,
    /// and an off-centre crest is what actually exercises the floor.
    /// </summary>
    private static List<Vector3> CrestPair(float slope, bool ascending, float bias = 0.55f)
    {
        // Horizontal span each side, sized so the 3D chord is about 2m
        var d = 2f / Mathf.Sqrt(4f + slope * slope);

        var low = new Vector3(-d * 2f * bias, -d * 2f * bias * slope, 0f);
        var high = new Vector3(d * 2f * (1f - bias), 0f, 0f);

        return ascending
            ? new List<Vector3> { low, high }
            : new List<Vector3> { high, low };
    }

    private static float SagOf(Vector3 a, Vector3 b, ISurfaceProbe probe)
    {
        var chordMid = (a + b) * 0.5f;
        probe.TrySnap(chordMid, Mathf.Max(a.y, b.y), chordMid.y, out var surfaceMid);

        return surfaceMid.y - chordMid.y;
    }

    [TestMethod]
    public void Test_MaxSagFor_IsMonotonicInSlope()
    {
        var previous = float.MaxValue;

        foreach (var slope in Slopes)
        {
            var a = new Vector3(0f, 0f, 0f);
            var b = new Vector3(1f, slope, 0f);
            var limit = SurfaceGeometry.MaxSagFor(a, b);

            Assert.IsTrue(limit < previous, $"Slope {slope} gave a looser limit than the one before");
            Assert.IsTrue(limit > 0f, $"Slope {slope} gave a non-positive limit");

            previous = limit;
        }
    }

    [TestMethod]
    public void Test_MaxSagFor_FlatUsesTheFullTolerance()
    {
        var limit = SurfaceGeometry.MaxSagFor(new Vector3(0f, 0f, 0f), new Vector3(2f, 0f, 0f));

        Assert.AreEqual(TravelScanRegistry.MaxChordSag, limit, 0.001f);
    }

    [TestMethod]
    public void Test_Crest_ResidualSagWithinAdaptiveLimit()
    {
        // The reachability invariant. If MinSegmentLength or MaxSubdivisionDepth is ever raised
        // without loosening the tolerance, subdivision will bottom out still over tolerance and
        // the segment would be drawn yellow forever — this is what catches that.
        foreach (var slope in Slopes)
        {
            var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Crest(slope));
            var input = CrestPair(slope, ascending: true);

            var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], probe);

            for (var i = 1; i < result.Count; i++)
            {
                var sag = SagOf(result[i - 1], result[i], probe);
                var limit = SurfaceGeometry.MaxSagFor(result[i - 1], result[i]);

                Assert.IsTrue(
                    sag <= limit + 0.001f,
                    $"Slope {slope}: segment {i - 1}→{i} sags {sag:F3}m against a {limit:F3}m " +
                    $"limit — subdivision cannot reach its own tolerance. " +
                    $"{Show(result[i - 1])} → {Show(result[i])}");
            }
        }
    }

    [TestMethod]
    public void Test_Crest_SubdividesWhenAscending()
    {
        // The regression test for the reported bug. Anchoring the sag probe to the segment start
        // meant an ascending crest read as having no sag at all, because the crest sat more than
        // MaxSurfaceSnapRise above the low end and the probe result was rejected as a floor jump.
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Crest(1.43f));
        var input = CrestPair(1.43f, ascending: true);

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], probe);

        Assert.IsTrue(
            result.Count > input.Count,
            $"Ascending a 55° crest should subdivide, got {result.Count} waypoints");
    }

    [TestMethod]
    public void Test_Crest_AscendingAndDescendingAgree()
    {
        // Direction symmetry. The same physical crest must be treated the same way whichever way
        // the path crosses it.
        foreach (var slope in Slopes)
        {
            var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Crest(slope));

            var up = CrestPair(slope, ascending: true);
            var down = CrestPair(slope, ascending: false);

            var upResult = TravelPathGenerator.SubdivideSaggingSegments(up, up[0], probe);
            var downResult = TravelPathGenerator.SubdivideSaggingSegments(down, down[0], probe);

            Assert.AreEqual(
                upResult.Count,
                downResult.Count,
                $"Slope {slope}: ascending gave {upResult.Count} waypoints, " +
                $"descending {downResult.Count}");
        }
    }

    [TestMethod]
    public void Test_Crest_RespectsMinSegmentLength()
    {
        foreach (var slope in Slopes)
        {
            var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Crest(slope));
            var input = CrestPair(slope, ascending: true);

            var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], probe);

            for (var i = 1; i < result.Count; i++)
                Assert.IsTrue(
                    (result[i] - result[i - 1]).magnitude >= TravelScanRegistry.MinSegmentLength,
                    $"Slope {slope}: segment {i - 1}→{i} is under the minimum length");
        }
    }

    [TestMethod]
    public void Test_SteppedStair_ResidualSagWithinAdaptiveLimit()
    {
        // A steep stair is not one crest, it is a run of small convex breaks — and they come far
        // more often than subdivision's floor is used to. This is the case that actually pins
        // MinSegmentLength: the floor has to be fine enough to place a waypoint per nose.
        const float tread = 0.25f;
        const float rise = 0.30f;   // slope 1.2, steeper than most vanilla stairs

        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Steps(tread, rise, 12));

        // One 2m-ish segment spanning several steps
        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(1.6f, 6f * rise, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], probe);

        for (var i = 1; i < result.Count; i++)
        {
            var sag = SagOf(result[i - 1], result[i], probe);
            var limit = SurfaceGeometry.MaxSagFor(result[i - 1], result[i]);

            Assert.IsTrue(
                sag <= limit + 0.001f,
                $"Stepped stair: segment {i - 1}→{i} sags {sag:F3}m against a {limit:F3}m limit. " +
                $"{Show(result[i - 1])} → {Show(result[i])}");
        }
    }

    [TestMethod]
    public void Test_SteppedStair_RespectsMinSegmentLength()
    {
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Steps(0.25f, 0.30f, 12));

        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(1.6f, 1.8f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], probe);

        for (var i = 1; i < result.Count; i++)
            Assert.IsTrue(
                (result[i] - result[i - 1]).magnitude >= TravelScanRegistry.MinSegmentLength,
                $"Segment {i - 1}→{i} is under the minimum length");
    }

    [TestMethod]
    public void Test_FlatGround_GainsNothing()
    {
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Flat(0f));

        var input = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(2f, 0f, 0f),
            new(4f, 0f, 0f)
        };

        var result = TravelPathGenerator.SubdivideSaggingSegments(input, input[0], probe);

        Assert.AreEqual(input.Count, result.Count);
    }

    [TestMethod]
    public void Test_Refinement_Settles()
    {
        // Repair and subdivision feed each other; the loop in GenerateLoop must reach a fixed
        // point. Run the same sequence here and assert it stops changing.
        var probe = new FakeSurfaceProbe(FakeSurfaceProbe.Crest(1.43f));

        var positions = new List<Vector3>
        {
            new(-8f, -8f * 1.43f, 0f),
            new(-4f, -4f * 1.43f, 0f),
            new(0f, 0f, 0f),
            new(6f, 0f, 0f)
        };

        var closing = positions[0];
        var passes = 0;

        for (; passes < TravelScanRegistry.MaxRefinementPasses; passes++)
        {
            var before = positions.Count;

            positions = TravelPathGenerator.RepairUnwalkableSegments(positions, closing, probe);
            positions = TravelPathGenerator.SubdivideSaggingSegments(positions, closing, probe);

            if (positions.Count == before)
                break;
        }

        Assert.IsTrue(
            passes < TravelScanRegistry.MaxRefinementPasses,
            $"Refinement did not settle within {TravelScanRegistry.MaxRefinementPasses} passes");

        for (var i = 1; i < positions.Count; i++)
        {
            var length = (positions[i] - positions[i - 1]).magnitude;

            Assert.IsTrue(
                length >= TravelScanRegistry.MinSegmentLength,
                $"Segment {i - 1}→{i} is {length:F3}m, under the minimum");

            Assert.IsTrue(
                length <= TravelScanRegistry.MaxSegmentLength,
                $"Segment {i - 1}→{i} is {length:F3}m, over the maximum");
        }
    }
}
