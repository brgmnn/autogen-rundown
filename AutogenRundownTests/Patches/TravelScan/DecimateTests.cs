using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Thinning a trace down to waypoints.
///
/// The trace is exact but dense, and the scan only needs enough points that the straight lerps
/// between them stay on the floor. The rule is the whole of it: a point may go only if the path
/// without it still follows the path with it. Every previous version of this thinned on spacing
/// alone, which is how a stair nose got deleted and a chord ended up through the floor.
/// </summary>
[TestClass]
public class Decimate_Tests
{
    private const float Tolerance = TravelScanRegistry.MaxTraceDeviation;
    private const float MaxSpacing = TravelScanRegistry.StepDistance;

    private static List<Vector3> AlongX(float from, float to, float step, Func<float, float> height)
    {
        var points = new List<Vector3>();
        var count = Mathf.RoundToInt((to - from) / step);

        for (var i = 0; i <= count; i++)
        {
            var x = from + i * step;
            points.Add(new Vector3(x, height(x), 0f));
        }

        return points;
    }

    private static void AssertSpacing(List<Vector3> points)
    {
        for (var i = 1; i < points.Count; i++)
        {
            var length = (points[i] - points[i - 1]).magnitude;

            Assert.IsTrue(
                length <= MaxSpacing + 1e-3f,
                $"Segment {i - 1}→{i} is {length:F2}m, over the {MaxSpacing}m maximum");

            Assert.IsTrue(
                length >= TravelScanRegistry.MinSegmentLength - 1e-4f,
                $"Segment {i - 1}→{i} is {length:F3}m, a degenerate segment");
        }
    }

    [TestMethod]
    public void Test_Decimate_ThinsAFlatRun()
    {
        var traced = AlongX(0f, 20f, 0.25f, _ => 0f);

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        Assert.IsTrue(
            result.Count < traced.Count / 2,
            $"Flat ground should thin out substantially, got {result.Count} of {traced.Count}");

        AssertSpacing(result);
    }

    [TestMethod]
    public void Test_Decimate_KeepsAFold()
    {
        // A ramp with a sharp break at x = 10. Dropping the break is what puts a chord through the
        // top of a staircase.
        var traced = AlongX(0f, 20f, 0.25f, x => x <= 10f ? 0f : (x - 10f) * 1.2f);

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        Assert.IsTrue(
            result.Any(p => Mathf.Abs(p.x - 10f) < 0.26f),
            $"The fold at x=10 was thinned away: {SurfaceFixtures.Show(result)}");
    }

    [TestMethod]
    public void Test_Decimate_NeverDeviatesFromWhatItThinned()
    {
        // The guarantee. Anything the output strays from is geometry the scan will cut through.
        var traced = AlongX(0f, 20f, 0.2f, x => Mathf.Sin(x * 0.7f) * 1.5f);

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        var deviation = SurfaceFixtures.MaxDeviation(traced, result);

        // Points dropped as degenerate sit within MinSegmentLength of one that was kept, so they
        // can be that much further from the result without the path having moved.
        Assert.IsTrue(
            deviation <= Tolerance + TravelScanRegistry.MinSegmentLength + 1e-3f,
            $"Thinned path strays {deviation:F3}m from the trace, over the {Tolerance}m tolerance");

        AssertSpacing(result);
    }

    [TestMethod]
    public void Test_Decimate_SplitsALongFlatRun()
    {
        // Nothing to keep on curvature grounds, so only the spacing bound stands between this and
        // one 20m chord.
        var traced = AlongX(0f, 20f, 0.5f, _ => 0f);

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        Assert.IsTrue(result.Count >= 11, $"Expected at least 11 waypoints, got {result.Count}");

        AssertSpacing(result);
    }

    [TestMethod]
    public void Test_Decimate_SplitsAlongTheTraceNotTheChord()
    {
        // Splitting a long span by lerping the decimated chord would place waypoints in mid-air
        // over a curve. Every emitted point has to lie on the traced polyline — either one of its
        // points, or somewhere on a segment between two of them, which is flat ground by
        // construction.
        var traced = AlongX(0f, 20f, 0.25f, x => Mathf.Sin(x * 0.3f) * 2f);

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        foreach (var point in result)
            Assert.IsTrue(
                SurfaceFixtures.MaxDeviation(new List<Vector3> { point }, traced) < 1e-3f,
                $"{SurfaceFixtures.Show(point)} is not on the trace");
    }

    [TestMethod]
    public void Test_Decimate_DropsDegeneratePoints()
    {
        // DoMoveScanner divides by segment length and the reverse patch bails below 0.001m.
        var traced = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(0.001f, 0f, 0f),
            new(0.002f, 0f, 0f),
            new(1f, 0f, 0f),
            new(2f, 0f, 0f)
        };

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        AssertSpacing(result);
    }

    [TestMethod]
    public void Test_Decimate_LeavesShortInputAlone()
    {
        var traced = new List<Vector3> { new(0f, 0f, 0f), new(1f, 0f, 0f) };

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void Test_Decimate_KeepsTheEndpoints()
    {
        var traced = AlongX(0f, 20f, 0.25f, _ => 0f);

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        Assert.AreEqual(traced[0].x, result[0].x, 1e-3f, "Lost the first point");
        Assert.AreEqual(
            traced[traced.Count - 1].x, result[result.Count - 1].x, 1e-3f, "Lost the last point");
    }

    /// <summary>
    /// End to end on the geometry that started this: trace a ramp, thin it, and check the result
    /// still lies on the floor. This is the whole pipeline's contract in one test.
    /// </summary>
    [TestMethod]
    public void Test_TraceThenDecimate_StaysOnTheRamp()
    {
        var surface = SurfaceFixtures.Ramp();

        var corners = new List<Vector3> { new(-1f, 0f, 0f), new(20f, 3f, 0f) };
        var traced = TravelPathGenerator.TraceSurface(corners, surface);

        var result = TravelPathGenerator.Decimate(traced, Tolerance, MaxSpacing);

        AssertSpacing(result);

        for (var i = 1; i < result.Count; i++)
        {
            for (var sample = 0; sample <= 8; sample++)
            {
                var chord = Vector3.Lerp(result[i - 1], result[i], sample / 8f);

                Assert.IsTrue(
                    surface.TryLocate(chord, chord.y, out var onSurface),
                    $"Segment {i - 1}→{i} leaves the mesh at {SurfaceFixtures.Show(chord)}");

                Assert.IsTrue(
                    onSurface.y - chord.y <= Tolerance + 1e-3f,
                    $"Segment {i - 1}→{i} passes {onSurface.y - chord.y:F3}m below the floor at " +
                    $"{SurfaceFixtures.Show(chord)}");
            }
        }
    }
}
