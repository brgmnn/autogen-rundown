using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

/// <summary>
/// Locating a point on the walkable surface.
///
/// This is what replaced NavMesh.SamplePosition, which can only return the nearest surface within
/// a radius and therefore has to be coaxed toward the right floor by moving the probe around. Near
/// a ledge the nearest surface is the floor below, and every attempt to bias the probe was a
/// workaround for an API that cannot express the question.
/// </summary>
[TestClass]
public class NavSurface_Tests
{
    [TestMethod]
    public void Test_Locate_PlacesAPointOnFlatGround()
    {
        var surface = SurfaceFixtures.Flat();

        Assert.IsTrue(surface.TryLocate(new Vector3(5f, 3f, 0f), 0f, out var located));
        Assert.AreEqual(0f, located.y, 1e-3f, $"Located at {SurfaceFixtures.Show(located)}");
        Assert.AreEqual(5f, located.x, 1e-3f);
        Assert.AreEqual(0f, located.z, 1e-3f);
    }

    [TestMethod]
    public void Test_Locate_PicksTheFloorNearestPreferredY()
    {
        // The wrong-floor bug in one assertion. Both floors are directly under the same XZ, so
        // nothing about the point itself distinguishes them — only the height being asked for.
        var surface = SurfaceFixtures.Stacked();

        Assert.IsTrue(surface.TryLocate(new Vector3(5f, 0f, 0f), 0f, out var upper));
        Assert.AreEqual(0f, upper.y, 1e-3f, "Should have found the upper floor");

        Assert.IsTrue(surface.TryLocate(new Vector3(5f, 0f, 0f), -6f, out var lower));
        Assert.AreEqual(-6f, lower.y, 1e-3f, "Should have found the lower floor");
    }

    [TestMethod]
    public void Test_Locate_PrefersTheNearerFloorFromBetweenThem()
    {
        var surface = SurfaceFixtures.Stacked();

        Assert.IsTrue(surface.TryLocate(new Vector3(5f, 0f, 0f), -2f, out var located));
        Assert.AreEqual(0f, located.y, 1e-3f, "-2 is nearer the upper floor");

        Assert.IsTrue(surface.TryLocate(new Vector3(5f, 0f, 0f), -4f, out located));
        Assert.AreEqual(-6f, located.y, 1e-3f, "-4 is nearer the lower floor");
    }

    [TestMethod]
    public void Test_Locate_FollowsARamp()
    {
        var surface = SurfaceFixtures.Ramp();

        foreach (var x in new[] { 4f, 5f, 6f, 7.5f, 9f, 10f, 12f })
        {
            Assert.IsTrue(surface.TryLocate(new Vector3(x, 0f, 0f), SurfaceFixtures.RampHeight(x),
                out var located), $"Nothing under x={x}");

            Assert.AreEqual(
                SurfaceFixtures.RampHeight(x), located.y, 1e-3f,
                $"Wrong height at x={x}: {SurfaceFixtures.Show(located)}");
        }
    }

    [TestMethod]
    public void Test_Locate_FailsWellOffTheMesh()
    {
        var surface = SurfaceFixtures.Flat();

        Assert.IsFalse(
            surface.TryLocate(new Vector3(60f, 0f, 0f), 0f, out _),
            "A point far outside the mesh should not resolve to anything");
    }

    [TestMethod]
    public void Test_Locate_FailsInsideAHole()
    {
        var surface = SurfaceFixtures.Hole();

        Assert.IsFalse(
            surface.TryLocate(new Vector3(10f, 0f, 0f), 0f, out _),
            "The middle of the hole is more than LocateRadius from any triangle");
    }

    [TestMethod]
    public void Test_Locate_ToleratesAPointOnTheBoundary()
    {
        // CalculatePath puts corners exactly on mesh edges, so a point that is on the boundary
        // rather than strictly inside has to resolve rather than fall between two triangles.
        var surface = SurfaceFixtures.Flat();

        Assert.IsTrue(
            surface.TryLocate(new Vector3(22f, 0f, 0f), 0f, out var located),
            "A point on the outer edge should still locate");

        Assert.AreEqual(0f, located.y, 1e-3f, SurfaceFixtures.Show(located));
    }

    [TestMethod]
    public void Test_Locate_ToleratesAPointJustOutsideTheEdge()
    {
        var surface = SurfaceFixtures.Flat();

        Assert.IsTrue(
            surface.TryLocate(new Vector3(22.2f, 0f, 0f), 0f, out var located),
            "Within LocateRadius of the mesh should resolve");

        Assert.AreEqual(0f, located.y, 1e-3f, SurfaceFixtures.Show(located));
    }

    [TestMethod]
    public void Test_Build_DropsDegenerateAndKeepsTheRest()
    {
        var surface = SurfaceFixtures.Flat();

        // 24 x 12 cells, two triangles each.
        Assert.AreEqual(24 * 12 * 2, surface.TriangleCount);
    }
}
