using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.Patches.TravelScan;
using UnityEngine;

namespace AutogenRundownTests.Patches.TravelScan;

[TestClass]
public class TravelScanRegistry_Tests
{
    [TestMethod]
    public void Test_MovingPuzzleTypes_ContainsAllExpectedIds()
    {
        uint[] expected = { 22, 31, 38, 42, 43, 52, 60, 100 };

        foreach (var id in expected)
            Assert.IsTrue(
                TravelScanRegistry.MovingPuzzleTypes.Contains(id),
                $"MovingPuzzleTypes should contain {id}");
    }

    [TestMethod]
    public void Test_MovingPuzzleTypes_HasExactCount()
    {
        Assert.AreEqual(8, TravelScanRegistry.MovingPuzzleTypes.Count);
    }

    [TestMethod]
    public void Test_SustainedTravelTypes_ContainsOnly100()
    {
        Assert.AreEqual(1, TravelScanRegistry.SustainedTravelTypes.Count);
        Assert.IsTrue(TravelScanRegistry.SustainedTravelTypes.Contains(100));
    }

    [TestMethod]
    public void Test_SpeedConstants()
    {
        Assert.AreEqual(2.0f, TravelScanRegistry.SustainedTravelSpeed);
        Assert.AreEqual(1.0f, TravelScanRegistry.SustainedTravelReverseSpeed);
        Assert.AreEqual(2f, TravelScanRegistry.StepDistance);
        Assert.AreEqual(2f, TravelScanRegistry.EdgeDistance);
    }

    [TestMethod]
    public void Test_Clear_EmptiesAllCollections()
    {
        // Add dummy pointers
        TravelScanRegistry.SustainedTravelInstances.Add((nint)42);
        TravelScanRegistry.SustainedTravelMovables.Add((nint)99);

        TravelScanRegistry.Clear();

        Assert.AreEqual(0, TravelScanRegistry.SustainedTravelInstances.Count);
        Assert.AreEqual(0, TravelScanRegistry.SustainedTravelMovables.Count);
    }
}

[TestClass]
public class RemoveBunchedPoints_Tests
{
    [TestMethod]
    public void Test_WellSpacedPoints_NoRemoval()
    {
        var points = new List<Vector3>
        {
            new(0, 0, 0),
            new(3, 0, 0),
            new(6, 0, 0),
            new(9, 0, 0),
        };

        var result = TravelPathGenerator.RemoveBunchedPoints(points, 1.0f);

        Assert.AreEqual(4, result.Count);
    }

    [TestMethod]
    public void Test_BunchedPair_SecondRemoved()
    {
        var points = new List<Vector3>
        {
            new(0, 0, 0),
            new(5, 0, 0),
            new(5.5f, 0, 0), // 0.5m from previous → bunched (< 1m)
            new(10, 0, 0),
        };

        var result = TravelPathGenerator.RemoveBunchedPoints(points, 1.0f);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(new Vector3(0, 0, 0), result[0]);
        Assert.AreEqual(new Vector3(5, 0, 0), result[1]);
        Assert.AreEqual(new Vector3(10, 0, 0), result[2]);
    }

    [TestMethod]
    public void Test_MultipleConsecutiveBunched_AllRemovedExceptAnchor()
    {
        var points = new List<Vector3>
        {
            new(0, 0, 0),
            new(5, 0, 0),
            new(5.2f, 0, 0), // bunched
            new(5.4f, 0, 0), // still bunched (< 1m from anchor at 5,0,0)
            new(5.6f, 0, 0), // still bunched
            new(10, 0, 0),
        };

        var result = TravelPathGenerator.RemoveBunchedPoints(points, 1.0f);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(new Vector3(0, 0, 0), result[0]);
        Assert.AreEqual(new Vector3(5, 0, 0), result[1]);
        Assert.AreEqual(new Vector3(10, 0, 0), result[2]);
    }

    [TestMethod]
    public void Test_FewerThanThreePoints_ReturnedUnchanged()
    {
        var single = new List<Vector3> { new(1, 2, 3) };
        var pair = new List<Vector3> { new(0, 0, 0), new(0.1f, 0, 0) };

        Assert.AreEqual(1, TravelPathGenerator.RemoveBunchedPoints(single, 1.0f).Count);
        Assert.AreEqual(2, TravelPathGenerator.RemoveBunchedPoints(pair, 1.0f).Count);
    }

    [TestMethod]
    public void Test_AllPointsSameLocation_OnlyFirstSurvives()
    {
        var points = new List<Vector3>
        {
            new(5, 5, 5),
            new(5, 5, 5),
            new(5, 5, 5),
            new(5, 5, 5),
        };

        var result = TravelPathGenerator.RemoveBunchedPoints(points, 1.0f);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(new Vector3(5, 5, 5), result[0]);
    }

    [TestMethod]
    public void Test_PointsExactlyAtMinSpacing_Kept()
    {
        // minSpacing = 1.0, so sqrMagnitude threshold is 1.0
        // Distance of exactly 1.0 → sqrMagnitude = 1.0, NOT < 1.0 → kept
        var points = new List<Vector3>
        {
            new(0, 0, 0),
            new(1, 0, 0), // exactly 1.0m
            new(2, 0, 0), // exactly 1.0m
        };

        var result = TravelPathGenerator.RemoveBunchedPoints(points, 1.0f);

        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void Test_MixedSpacing()
    {
        var points = new List<Vector3>
        {
            new(0, 0, 0),   // kept (first)
            new(3, 0, 0),   // kept (3m from prev)
            new(3.3f, 0, 0), // removed (0.3m from prev)
            new(7, 0, 0),   // kept (3.7m from anchor at 3,0,0)
            new(7.1f, 0, 0), // removed (0.1m)
            new(7.2f, 0, 0), // removed (0.2m from anchor)
            new(12, 0, 0),  // kept (5m from anchor)
        };

        var result = TravelPathGenerator.RemoveBunchedPoints(points, 1.0f);

        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(new Vector3(0, 0, 0), result[0]);
        Assert.AreEqual(new Vector3(3, 0, 0), result[1]);
        Assert.AreEqual(new Vector3(7, 0, 0), result[2]);
        Assert.AreEqual(new Vector3(12, 0, 0), result[3]);
    }

    [TestMethod]
    public void Test_EmptyInput_ReturnsEmpty()
    {
        var result = TravelPathGenerator.RemoveBunchedPoints(new List<Vector3>(), 1.0f);
        Assert.AreEqual(0, result.Count);
    }
}

[TestClass]
public class ResamplePathRaw_Tests
{
    [TestMethod]
    public void Test_StraightLine_EvenlySpacedPoints()
    {
        // 10m line with step=2m → start + walked at 2,4,6,8,10 = 6 points
        var corners = new List<Vector3>
        {
            new(0, 0, 0),
            new(10, 0, 0),
        };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        Assert.AreEqual(6, result.Count);
        Assert.AreEqual(new Vector3(0, 0, 0), result[0]);
        Assert.AreEqual(new Vector3(2, 0, 0), result[1]);
        Assert.AreEqual(new Vector3(4, 0, 0), result[2]);
        Assert.AreEqual(new Vector3(6, 0, 0), result[3]);
        Assert.AreEqual(new Vector3(8, 0, 0), result[4]);
        Assert.AreEqual(new Vector3(10, 0, 0), result[5]);
    }

    [TestMethod]
    public void Test_RightAngleTurn_PointsFollowBothSegments()
    {
        // L-shaped path: 6m east then 6m north, step=2m
        // Seg 1 (6m): start + walked at 2,4,6 = 4 points. Remaining = 0.
        // Seg 2 (6m): walked at 2,4,6 = 3 points. Total = 7.
        var corners = new List<Vector3>
        {
            new(0, 0, 0),
            new(6, 0, 0),
            new(6, 0, 6),
        };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        Assert.AreEqual(7, result.Count);
        Assert.AreEqual(new Vector3(0, 0, 0), result[0]);
        Assert.AreEqual(new Vector3(2, 0, 0), result[1]);
        Assert.AreEqual(new Vector3(4, 0, 0), result[2]);
        Assert.AreEqual(new Vector3(6, 0, 0), result[3]);
        Assert.AreEqual(new Vector3(6, 0, 2), result[4]);
        Assert.AreEqual(new Vector3(6, 0, 4), result[5]);
        Assert.AreEqual(new Vector3(6, 0, 6), result[6]);
    }

    [TestMethod]
    public void Test_RemainingCarriesAcrossSegments()
    {
        // Two 3m segments with step=2m
        // Seg 1: start(0), walked at 2m. Remaining = 1m.
        // Seg 2: walked starts at 1 (remaining). 1+2=3 ≤ 3 → walked=3. Point at 3m from seg2 start.
        var corners = new List<Vector3>
        {
            new(0, 0, 0),
            new(3, 0, 0),
            new(3, 0, 3),
        };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(new Vector3(0, 0, 0), result[0]);
        Assert.AreEqual(new Vector3(2, 0, 0), result[1]);
        Assert.AreEqual(new Vector3(3, 0, 3), result[2]);
    }

    [TestMethod]
    public void Test_VeryShortPath_JustStartPoint()
    {
        // Path shorter than stepDistance
        var corners = new List<Vector3>
        {
            new(0, 0, 0),
            new(1, 0, 0), // 1m < 2m step
        };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(new Vector3(0, 0, 0), result[0]);
    }

    [TestMethod]
    public void Test_ZeroLengthSegment_Skipped()
    {
        // Zero-length segment is skipped, but 4m segment still produces points
        var corners = new List<Vector3>
        {
            new(0, 0, 0),
            new(0, 0, 0), // zero-length → skipped
            new(4, 0, 0), // 4m → walked at 2, 4
        };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(new Vector3(0, 0, 0), result[0]);
        Assert.AreEqual(new Vector3(2, 0, 0), result[1]);
        Assert.AreEqual(new Vector3(4, 0, 0), result[2]);
    }

    [TestMethod]
    public void Test_SinglePoint_ReturnsEmpty()
    {
        var corners = new List<Vector3> { new(5, 5, 5) };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Test_TwoIdenticalPoints_JustStart()
    {
        var corners = new List<Vector3>
        {
            new(5, 5, 5),
            new(5, 5, 5),
        };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        // Zero-length segment is skipped, but start point is always added
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void Test_LongPath_CorrectPointCount()
    {
        // 100m straight line with step=2m → start + walked at 2,4,...,100 = 51 points
        var corners = new List<Vector3>
        {
            new(0, 0, 0),
            new(100, 0, 0),
        };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        Assert.AreEqual(51, result.Count);
    }

    [TestMethod]
    public void Test_ExactMultiple_LastPointPlaced()
    {
        // 4m line with step=2m → start + walked at 2,4 = 3 points
        var corners = new List<Vector3>
        {
            new(0, 0, 0),
            new(4, 0, 0),
        };

        var result = TravelPathGenerator.ResamplePathRaw(corners, 2.0f);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(new Vector3(4, 0, 0), result[2]);
    }
}

[TestClass]
public class CalculateReverseLerp_Tests
{
    [TestMethod]
    public void Test_BasicDecrement()
    {
        // segmentDistance=2, reverseSpeed=1, timeDelta=0.5
        // delta = 0.5 * 1.0 / 2.0 = 0.25
        // newLerp = 5.0 - 0.25 = 4.75
        var result = Patch_SustainedTravelReverse.CalculateReverseLerp(5.0f, 0.5f, 1.0f, 2.0f);

        Assert.AreEqual(4.75f, result, 0.001f);
    }

    [TestMethod]
    public void Test_ClampAtZero()
    {
        // Large delta that would go negative
        var result = Patch_SustainedTravelReverse.CalculateReverseLerp(0.1f, 10.0f, 1.0f, 2.0f);

        Assert.AreEqual(0f, result);
    }

    [TestMethod]
    public void Test_ZeroSegmentDistance_NoChange()
    {
        var result = Patch_SustainedTravelReverse.CalculateReverseLerp(5.0f, 0.5f, 1.0f, 0.0f);

        Assert.AreEqual(5.0f, result);
    }

    [TestMethod]
    public void Test_ExactZeroResult()
    {
        // delta = 1.0 * 1.0 / 1.0 = 1.0
        // newLerp = 1.0 - 1.0 = 0.0
        var result = Patch_SustainedTravelReverse.CalculateReverseLerp(1.0f, 1.0f, 1.0f, 1.0f);

        Assert.AreEqual(0f, result);
    }

    [TestMethod]
    public void Test_LargeLerpSmallDelta()
    {
        // delta = 0.016 * 1.0 / 5.0 = 0.0032
        var result = Patch_SustainedTravelReverse.CalculateReverseLerp(50.0f, 0.016f, 1.0f, 5.0f);

        Assert.AreEqual(50.0f - 0.0032f, result, 0.0001f);
    }

    [TestMethod]
    public void Test_DefaultSpeeds()
    {
        // Using actual game constants: reverseSpeed=1.0, segmentDistance=2.0 (stepDistance)
        // At 60fps (delta=1/60): delta = (1/60) * 1.0 / 2.0 = 0.00833
        var result = Patch_SustainedTravelReverse.CalculateReverseLerp(
            10.0f, 1f / 60f, TravelScanRegistry.SustainedTravelReverseSpeed, TravelScanRegistry.StepDistance);

        Assert.AreEqual(10.0f - (1f / 60f * 1.0f / 2.0f), result, 0.0001f);
    }
}

[TestClass]
public class GetSegmentAndFraction_Tests
{
    [TestMethod]
    public void Test_IntegerLerp()
    {
        var (current, next, t) = Patch_SustainedTravelReverse.GetSegmentAndFraction(3.0f, 10);

        Assert.AreEqual(3, current);
        Assert.AreEqual(4, next);
        Assert.AreEqual(0f, t, 0.001f);
    }

    [TestMethod]
    public void Test_FractionalLerp()
    {
        var (current, next, t) = Patch_SustainedTravelReverse.GetSegmentAndFraction(3.7f, 10);

        Assert.AreEqual(3, current);
        Assert.AreEqual(4, next);
        Assert.AreEqual(0.7f, t, 0.001f);
    }

    [TestMethod]
    public void Test_WrapAtBoundary()
    {
        // Last segment: index 9, next wraps to 0
        var (current, next, t) = Patch_SustainedTravelReverse.GetSegmentAndFraction(9.5f, 10);

        Assert.AreEqual(9, current);
        Assert.AreEqual(0, next);
        Assert.AreEqual(0.5f, t, 0.001f);
    }

    [TestMethod]
    public void Test_ZeroLerp()
    {
        var (current, next, t) = Patch_SustainedTravelReverse.GetSegmentAndFraction(0.0f, 10);

        Assert.AreEqual(0, current);
        Assert.AreEqual(1, next);
        Assert.AreEqual(0f, t, 0.001f);
    }

    [TestMethod]
    public void Test_SmallAmountOfPositions()
    {
        // Only 2 positions: segment 0→1, wraps 1→0
        var (current, next, t) = Patch_SustainedTravelReverse.GetSegmentAndFraction(1.5f, 2);

        Assert.AreEqual(1, current);
        Assert.AreEqual(0, next);
        Assert.AreEqual(0.5f, t, 0.001f);
    }

    [TestMethod]
    public void Test_NearSegmentBoundary()
    {
        var (current, next, t) = Patch_SustainedTravelReverse.GetSegmentAndFraction(4.999f, 10);

        Assert.AreEqual(4, current);
        Assert.AreEqual(5, next);
        Assert.AreEqual(0.999f, t, 0.001f);
    }
}

[TestClass]
public class PuzzleComponent_TravelScan_Tests
{
    [TestMethod]
    public void Test_SustainedTravel_HasType100()
    {
        Assert.AreEqual((uint)PuzzleType.SustainedTravel, 100u);
        Assert.AreEqual(PuzzleType.SustainedTravel, PuzzleComponent.SustainedTravel.PuzzleType);
    }

    [TestMethod]
    public void Test_SustainedTravel_Duration120()
    {
        Assert.AreEqual(120.0, PuzzleComponent.SustainedTravel.Duration);
    }

    [TestMethod]
    public void Test_AllTravelComponents_HavePositiveDuration()
    {
        var travelComponents = new[]
        {
            PuzzleComponent.TravelSolo_Short,
            PuzzleComponent.TravelSolo_Medium,
            PuzzleComponent.TravelSolo_Long,
            PuzzleComponent.TravelTeam_Short,
            PuzzleComponent.TravelTeam_Medium,
            PuzzleComponent.TravelTeam_MediumGreen,
            PuzzleComponent.TravelTeam_Long,
            PuzzleComponent.TravelTeam_LongGreen,
            PuzzleComponent.SustainedTravel,
            PuzzleComponent.TravelTeam,
            PuzzleComponent.TravelBig,
        };

        foreach (var component in travelComponents)
            Assert.IsTrue(component.Duration > 0,
                $"Travel component with PuzzleType {component.PuzzleType} should have positive duration");
    }

    [TestMethod]
    public void Test_AllTravelTypes_AreInMovingPuzzleTypes()
    {
        var travelComponents = new[]
        {
            PuzzleComponent.TravelSolo_Short,
            PuzzleComponent.TravelSolo_Medium,
            PuzzleComponent.TravelSolo_Long,
            PuzzleComponent.TravelTeam_Short,
            PuzzleComponent.TravelTeam_Medium,
            PuzzleComponent.TravelTeam_Long,
            PuzzleComponent.SustainedTravel,
            PuzzleComponent.TravelTeam,
            PuzzleComponent.TravelBig,
        };

        foreach (var component in travelComponents)
            Assert.IsTrue(
                TravelScanRegistry.MovingPuzzleTypes.Contains((uint)component.PuzzleType),
                $"PuzzleType {component.PuzzleType} ({(uint)component.PuzzleType}) should be in MovingPuzzleTypes");
    }

    [TestMethod]
    public void Test_SustainedTravel_IsInSustainedTravelTypes()
    {
        Assert.IsTrue(TravelScanRegistry.SustainedTravelTypes.Contains(
            (uint)PuzzleComponent.SustainedTravel.PuzzleType));
    }
}
