using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Tracing a route across the triangles.
///
/// The invariant every one of these is circling: a traced polyline lies on the surface, and every
/// segment of it lies inside a single flat triangle. Six rounds of sampling-and-snapping never got
/// there, because a sampled point cannot say which polygon it is on and so cannot tell a ramp from
/// a ledge. Tracing gets there by construction.
/// </summary>
[TestClass]
public class SurfaceTrace_Tests
{
    /// <summary>
    /// The property that matters, checked the way it actually fails: sample along each segment and
    /// compare the chord against the floor underneath it. A chord that dips below the surface is
    /// one the scan visibly travels through.
    /// </summary>
    private static void AssertStaysOnSurface(NavSurface surface, List<Vector3> points, string what)
    {
        const int samples = 8;

        for (var i = 1; i < points.Count; i++)
        {
            for (var sample = 0; sample <= samples; sample++)
            {
                var chord = Vector3.Lerp(points[i - 1], points[i], sample / (float)samples);

                Assert.IsTrue(
                    surface.TryLocate(chord, chord.y, out var onSurface),
                    $"{what}: segment {i - 1}→{i} leaves the mesh at " +
                    $"{SurfaceFixtures.Show(chord)}");

                Assert.AreEqual(
                    chord.y, onSurface.y, 1e-2f,
                    $"{what}: segment {i - 1}→{i} is {onSurface.y - chord.y:F3}m off the floor at " +
                    $"{SurfaceFixtures.Show(chord)} " +
                    $"({SurfaceFixtures.Show(points[i - 1])} → {SurfaceFixtures.Show(points[i])})");
            }
        }
    }

    [TestMethod]
    public void Test_Trace_FollowsFlatGround()
    {
        var surface = SurfaceFixtures.Flat();
        var traced = new List<Vector3>();

        Assert.IsTrue(surface.TryTrace(new Vector3(0f, 0f, 0f), new Vector3(20f, 0f, 0f), traced));

        Assert.IsTrue(traced.Count > 1, "Should have crossed several triangles");
        AssertStaysOnSurface(surface, traced, "Flat");

        var last = traced[traced.Count - 1];
        Assert.AreEqual(20f, last.x, 1e-2f, "Should have arrived at the destination");
    }

    [TestMethod]
    public void Test_Trace_FollowsARampRatherThanChordingIt()
    {
        // The original complaint: a chord from the bottom of a staircase to a point past the top
        // cuts through the slope. Tracing puts a vertex at each fold, so it cannot.
        var surface = SurfaceFixtures.Ramp();
        var traced = new List<Vector3>();

        Assert.IsTrue(surface.TryTrace(new Vector3(2f, 0f, 0f), new Vector3(16f, 3f, 0f), traced));

        AssertStaysOnSurface(surface, traced, "Ramp");

        // The folds at x = 5 and x = 10 are where a chord would cut the corner.
        Assert.IsTrue(
            traced.Any(p => Mathf.Abs(p.x - 5f) < 0.05f),
            $"No vertex at the foot of the ramp: {SurfaceFixtures.Show(traced)}");

        Assert.IsTrue(
            traced.Any(p => Mathf.Abs(p.x - 10f) < 0.05f),
            $"No vertex at the top of the ramp: {SurfaceFixtures.Show(traced)}");
    }

    [TestMethod]
    public void Test_Trace_StopsAtALedgeInsteadOfCrossingIt()
    {
        // Segment 21→22 from the ZONE_606 log: (-19.9, 0.0, …) → (-24.0, -6.0, …). Both endpoints
        // are on the mesh and the straight line between them is short, so a raycast-plus-slope
        // check waves it through. There is no floor in between.
        var surface = SurfaceFixtures.Ledge();
        var traced = new List<Vector3>();

        Assert.IsFalse(
            surface.TryTrace(new Vector3(2f, 0f, 0f), new Vector3(20f, -6f, 0f), traced),
            "Tracing across a 6m drop should fail, not produce a chord");

        foreach (var point in traced)
            Assert.IsTrue(
                point.x <= 10f + 1e-2f,
                $"Traced past the edge of the upper floor to {SurfaceFixtures.Show(point)}");
    }

    [TestMethod]
    public void Test_Trace_StopsAtACliffWithFloorDirectlyBelowIt()
    {
        // The version of the ledge with no gap to fall into. There is walkable floor immediately
        // past the edge, so "is there surface near here" answers yes and every proximity-based
        // check waves it through — this is why NavMesh.SamplePosition could never be made to work.
        // Only the height change between one triangle and the next distinguishes it.
        var surface = SurfaceFixtures.Cliff();
        var traced = new List<Vector3>();

        Assert.IsFalse(
            surface.TryTrace(new Vector3(2f, 0f, 0f), new Vector3(20f, -6f, 0f), traced),
            "Tracing off a 6m cliff should fail, not step down onto the floor below");

        foreach (var point in traced)
            Assert.IsTrue(
                point.y > -1f,
                $"Dropped off the cliff to {SurfaceFixtures.Show(point)}");
    }

    [TestMethod]
    public void Test_Trace_CrossesAStepSmallEnoughToBeBaked()
    {
        // The bake's agentClimb is 0.5m, so two adjacent walkable polygons may legitimately differ
        // by that much at their shared edge. Rejecting those would refuse to trace real staircases.
        var surface = SurfaceFixtures.Cliff(-0.4f);
        var traced = new List<Vector3>();

        Assert.IsTrue(
            surface.TryTrace(new Vector3(2f, 0f, 0f), new Vector3(20f, -0.4f, 0f), traced),
            "A 0.4m step is inside agentClimb and must still trace");

        Assert.IsTrue(
            traced.Any(p => p.y < -0.3f),
            $"Should have stepped down: {SurfaceFixtures.Show(traced)}");
    }

    [TestMethod]
    public void Test_Trace_NeverChangesFloor()
    {
        var surface = SurfaceFixtures.Stacked();
        var traced = new List<Vector3>();

        Assert.IsTrue(surface.TryTrace(new Vector3(0f, 0f, 0f), new Vector3(20f, 0f, 0f), traced));

        foreach (var point in traced)
            Assert.AreEqual(
                0f, point.y, 1e-3f,
                $"Dropped to the lower floor at {SurfaceFixtures.Show(point)}");
    }

    [TestMethod]
    public void Test_Trace_NeverChangesFloorGoingTheOtherWay()
    {
        var surface = SurfaceFixtures.Stacked();
        var traced = new List<Vector3>();

        Assert.IsTrue(surface.TryTrace(new Vector3(0f, -6f, 0f), new Vector3(20f, -6f, 0f), traced));

        foreach (var point in traced)
            Assert.AreEqual(-6f, point.y, 1e-3f, SurfaceFixtures.Show(point));
    }

    [TestMethod]
    public void Test_Trace_StopsAtTheEdgeOfAHole()
    {
        var surface = SurfaceFixtures.Hole();
        var traced = new List<Vector3>();

        Assert.IsFalse(
            surface.TryTrace(new Vector3(0f, 0f, 0f), new Vector3(20f, 0f, 0f), traced),
            "Should not cross the hole");

        if (traced.Count > 0)
            Assert.IsTrue(
                traced[traced.Count - 1].x <= 8f + 1e-2f,
                $"Traced into the hole, reaching {SurfaceFixtures.Show(traced[traced.Count - 1])}");
    }

    [TestMethod]
    public void Test_Trace_RoutesAroundAHoleWhenGivenTheWayRound()
    {
        // What the old walk could not do once its shared detour budget ran out. The pathfinder
        // knows the way round — every failing segment in the log reported PathComplete with 3 to 11
        // corners — so the trace only has to be told, and follow it.
        var surface = SurfaceFixtures.Hole();

        var corners = new List<Vector3> { new(0f, 0f, 0f), new(20f, 0f, 0f) };

        List<Vector3> Reroute(Vector3 from, Vector3 to)
            => new() { new Vector3(10f, 0f, 4f) };

        var traced = TravelPathGenerator.TraceSurface(corners, surface, Reroute);

        Assert.IsTrue(traced.Count > 2, "Should have traced a route");
        AssertStaysOnSurface(surface, traced, "Around the hole");

        Assert.IsTrue(
            traced.Any(p => p.z > 2f),
            "The route should have gone around the hole rather than through it");
    }

    [TestMethod]
    public void Test_TraceSurface_KeepsGoingWhenAStretchCannotBeTraced()
    {
        // With nothing to reroute through, the corner is emitted and the circuit continues. That
        // segment is wrong and gets logged, but the rest of the path is still usable — the old
        // behaviour silently teleported the cursor and emitted nothing at all.
        var surface = SurfaceFixtures.Ledge();

        var corners = new List<Vector3>
        {
            new(2f, 0f, 0f),
            new(20f, -6f, 0f),
            new(18f, -6f, 0f)
        };

        var traced = TravelPathGenerator.TraceSurface(corners, surface);

        Assert.IsTrue(traced.Count >= 2, "Should still have produced a path");

        Assert.IsTrue(
            traced.Any(p => p.y < -5f),
            $"Should have reached the lower floor: {SurfaceFixtures.Show(traced)}");
    }

    [TestMethod]
    public void Test_TraceSurface_ChainsCornersWithoutDuplicating()
    {
        var surface = SurfaceFixtures.Flat();

        var corners = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(10f, 0f, 0f),
            new(10f, 0f, 4f)
        };

        var traced = TravelPathGenerator.TraceSurface(corners, surface);

        AssertStaysOnSurface(surface, traced, "Chained corners");

        for (var i = 1; i < traced.Count; i++)
            Assert.IsTrue(
                (traced[i] - traced[i - 1]).magnitude > 1e-4f,
                $"Duplicate point at index {i}: {SurfaceFixtures.Show(traced[i])}");
    }

    [TestMethod]
    public void Test_Trace_HandlesADiagonalAcrossTheGrid()
    {
        // Axis-aligned traces can pass by crossing edges cleanly; a diagonal exercises the exit
        // maths against every edge orientation, including the quad diagonals.
        var surface = SurfaceFixtures.Ramp();
        var traced = new List<Vector3>();

        Assert.IsTrue(surface.TryTrace(new Vector3(-1f, 0f, -5f), new Vector3(19f, 3f, 5f), traced));

        AssertStaysOnSurface(surface, traced, "Diagonal");
    }

    [TestMethod]
    public void Test_Trace_ShortHopWithinOneTriangle()
    {
        var surface = SurfaceFixtures.Flat();
        var traced = new List<Vector3>();

        Assert.IsTrue(surface.TryTrace(new Vector3(5.1f, 0f, 0.1f), new Vector3(5.3f, 0f, 0.3f), traced));

        Assert.AreEqual(1, traced.Count, "A hop inside one triangle needs only its endpoint");
        Assert.AreEqual(5.3f, traced[0].x, 1e-3f);
    }
}
