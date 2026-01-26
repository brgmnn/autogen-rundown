using AutogenRundown.Patches.ZoneSensors;

namespace AutogenRundownTests.Patches.ZoneSensors;

[TestClass]
public class ZoneSensorMovementState_Tests
{
    #region CalculateBatchCount Tests

    [TestMethod]
    public void Test_CalculateBatchCount_ZeroSensors()
    {
        Assert.AreEqual(0, ZoneSensorMovementState.CalculateBatchCount(0));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_OneSensor()
    {
        Assert.AreEqual(1, ZoneSensorMovementState.CalculateBatchCount(1));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_32Sensors()
    {
        // Exactly one batch (32 is max per batch)
        Assert.AreEqual(1, ZoneSensorMovementState.CalculateBatchCount(32));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_33Sensors()
    {
        // 33 sensors requires 2 batches
        Assert.AreEqual(2, ZoneSensorMovementState.CalculateBatchCount(33));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_128Sensors()
    {
        // 128 sensors = 4 batches (128 / 32)
        Assert.AreEqual(4, ZoneSensorMovementState.CalculateBatchCount(128));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_BoundaryConditions()
    {
        for (int batches = 1; batches <= 4; batches++)
        {
            int maxForBatch = batches * 32;
            int minForNextBatch = maxForBatch + 1;

            Assert.AreEqual(batches, ZoneSensorMovementState.CalculateBatchCount(maxForBatch),
                $"Expected {batches} batches for {maxForBatch} sensors");

            Assert.AreEqual(batches + 1, ZoneSensorMovementState.CalculateBatchCount(minForNextBatch),
                $"Expected {batches + 1} batches for {minForNextBatch} sensors");
        }
    }

    #endregion

    #region Movement State Round-Trip Tests

    [TestMethod]
    public void Test_SetGetMovementState_RoundTrip()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1, BatchIndex = 0 };

        state.SetMovementState(0, 5, 3, true, 0.75f);
        var (waypointIndex, forward, progress) = state.GetMovementState(0);

        Assert.AreEqual(3, waypointIndex);
        Assert.IsTrue(forward);
        Assert.AreEqual(0.75f, progress, 1f / 255f); // Within byte precision
    }

    [TestMethod]
    public void Test_SetGetMovementState_AllParameters()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1, BatchIndex = 0 };

        // Test backward direction
        state.SetMovementState(0, 10, 7, false, 0.25f);
        var (waypointIndex, forward, progress) = state.GetMovementState(0);

        Assert.AreEqual(7, waypointIndex);
        Assert.IsFalse(forward);
        Assert.AreEqual(0.25f, progress, 1f / 255f);
    }

    [TestMethod]
    public void Test_SetGetMovementState_WaypointIndexClamping()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1 };

        // Max waypoint index that fits in a byte is 255
        state.SetMovementState(0, 0, 255, true, 0f);
        var (waypointIndex, _, _) = state.GetMovementState(0);
        Assert.AreEqual(255, waypointIndex);

        // Values > 255 should be clamped
        state.SetMovementState(0, 0, 300, true, 0f);
        (waypointIndex, _, _) = state.GetMovementState(0);
        Assert.AreEqual(255, waypointIndex);
    }

    #endregion

    #region Direction Mask Tests

    [TestMethod]
    public void Test_DirectionMask_Forward()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1 };

        state.SetMovementState(0, 0, 0, true, 0f);
        var (_, forward, _) = state.GetMovementState(0);

        Assert.IsTrue(forward);
        Assert.AreEqual(1u, state.DirectionMask);
    }

    [TestMethod]
    public void Test_DirectionMask_Backward()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1 };

        state.SetMovementState(0, 0, 0, false, 0f);
        var (_, forward, _) = state.GetMovementState(0);

        Assert.IsFalse(forward);
        Assert.AreEqual(0u, state.DirectionMask);
    }

    [TestMethod]
    public void Test_DirectionMask_Multiple()
    {
        var state = new ZoneSensorMovementState { SensorCount = 4 };

        // Set mixed directions: forward, backward, forward, backward
        state.SetMovementState(0, 0, 0, true, 0f);
        state.SetMovementState(1, 1, 0, false, 0f);
        state.SetMovementState(2, 2, 0, true, 0f);
        state.SetMovementState(3, 3, 0, false, 0f);

        // Verify each direction is independent
        Assert.IsTrue(state.GetMovementState(0).forward);
        Assert.IsFalse(state.GetMovementState(1).forward);
        Assert.IsTrue(state.GetMovementState(2).forward);
        Assert.IsFalse(state.GetMovementState(3).forward);

        // Verify raw mask: bits 0 and 2 set = 0b0101 = 5
        Assert.AreEqual(5u, state.DirectionMask);
    }

    [TestMethod]
    public void Test_DirectionMask_TogglePreserveOthers()
    {
        var state = new ZoneSensorMovementState { SensorCount = 3 };

        // Set all forward
        state.SetMovementState(0, 0, 0, true, 0f);
        state.SetMovementState(1, 1, 0, true, 0f);
        state.SetMovementState(2, 2, 0, true, 0f);

        Assert.AreEqual(7u, state.DirectionMask); // 0b111

        // Change middle one to backward
        state.SetMovementState(1, 1, 0, false, 0f);

        Assert.AreEqual(5u, state.DirectionMask); // 0b101

        // Verify others unchanged
        Assert.IsTrue(state.GetMovementState(0).forward);
        Assert.IsFalse(state.GetMovementState(1).forward);
        Assert.IsTrue(state.GetMovementState(2).forward);
    }

    [TestMethod]
    public void Test_DirectionMask_AllBits()
    {
        var state = new ZoneSensorMovementState { SensorCount = 32 };

        // Set all 32 entries to forward
        for (int i = 0; i < 32; i++)
        {
            state.SetMovementState(i, i, 0, true, 0f);
        }

        Assert.AreEqual(uint.MaxValue, state.DirectionMask);

        // Set all to backward
        for (int i = 0; i < 32; i++)
        {
            state.SetMovementState(i, i, 0, false, 0f);
        }

        Assert.AreEqual(0u, state.DirectionMask);
    }

    #endregion

    #region Progress Encoding Tests

    [TestMethod]
    public void Test_ProgressEncoding_Boundaries()
    {
        var state = new ZoneSensorMovementState { SensorCount = 3 };

        // 0.0 → should map to byte 0
        state.SetMovementState(0, 0, 0, true, 0.0f);
        Assert.AreEqual(0.0f, state.GetMovementState(0).progress, 0.001f);

        // 0.5 → should map to byte ~127/128
        state.SetMovementState(1, 1, 0, true, 0.5f);
        Assert.AreEqual(0.5f, state.GetMovementState(1).progress, 1f / 255f + 0.001f);

        // 1.0 → should map to byte 255
        state.SetMovementState(2, 2, 0, true, 1.0f);
        Assert.AreEqual(1.0f, state.GetMovementState(2).progress, 0.001f);
    }

    [TestMethod]
    public void Test_ProgressEncoding_Precision()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1 };

        // Test various progress values
        float[] testValues = { 0.0f, 0.1f, 0.25f, 0.333f, 0.5f, 0.666f, 0.75f, 0.9f, 1.0f };

        foreach (var value in testValues)
        {
            state.SetMovementState(0, 0, 0, true, value);
            var (_, _, progress) = state.GetMovementState(0);

            // Tolerance is 1/255 (~0.004) since we use byte precision
            Assert.AreEqual(value, progress, 1f / 255f + 0.001f, $"Failed for value {value}");
        }
    }

    [TestMethod]
    public void Test_ProgressEncoding_Clamping()
    {
        var state = new ZoneSensorMovementState { SensorCount = 2 };

        // Values < 0 should clamp to 0
        state.SetMovementState(0, 0, 0, true, -0.5f);
        Assert.AreEqual(0f, state.GetMovementState(0).progress, 0.001f);

        // Values > 1 should clamp to 1
        state.SetMovementState(1, 1, 0, true, 1.5f);
        Assert.AreEqual(1f, state.GetMovementState(1).progress, 0.001f);
    }

    #endregion

    #region GetGlobalSensorIndex Tests

    [TestMethod]
    public void Test_GetGlobalSensorIndex_Sparse()
    {
        var state = new ZoneSensorMovementState { SensorCount = 3 };

        // Store non-consecutive global indices
        state.SetMovementState(0, 5, 0, true, 0f);   // Entry 0 → global 5
        state.SetMovementState(1, 20, 0, true, 0f);  // Entry 1 → global 20
        state.SetMovementState(2, 127, 0, true, 0f); // Entry 2 → global 127

        // Should return stored global index, not entry index
        Assert.AreEqual(5, state.GetGlobalSensorIndex(0));
        Assert.AreEqual(20, state.GetGlobalSensorIndex(1));
        Assert.AreEqual(127, state.GetGlobalSensorIndex(2));
    }

    [TestMethod]
    public void Test_GetGlobalSensorIndex_InvalidEntry()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1 };

        // Invalid entries return -1
        Assert.AreEqual(-1, state.GetGlobalSensorIndex(-1));
        Assert.AreEqual(-1, state.GetGlobalSensorIndex(32));
    }

    #endregion

    #region HasMovementData Tests

    [TestMethod]
    public void Test_HasMovementData_True()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1 };
        Assert.IsTrue(state.HasMovementData);

        state.SensorCount = 32;
        Assert.IsTrue(state.HasMovementData);
    }

    [TestMethod]
    public void Test_HasMovementData_False()
    {
        var state = new ZoneSensorMovementState { SensorCount = 0 };
        Assert.IsFalse(state.HasMovementData);
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Test_DefaultConstructor()
    {
        var state = new ZoneSensorMovementState();

        Assert.AreEqual(0, state.BatchIndex);
        Assert.AreEqual(0, state.SensorCount);
        Assert.AreEqual(0, state.Padding);
        Assert.AreEqual(0u, state.DirectionMask);
    }

    #endregion

    #region Constants Tests

    [TestMethod]
    public void Test_MaxSensors_Constant()
    {
        Assert.AreEqual(32, ZoneSensorMovementState.MaxSensors);
    }

    #endregion

    #region Invalid Index Handling Tests

    [TestMethod]
    public void Test_GetMovementState_InvalidIndex()
    {
        var state = new ZoneSensorMovementState { SensorCount = 1 };

        // Invalid indices return default values
        var (waypointIndex, forward, progress) = state.GetMovementState(-1);
        Assert.AreEqual(0, waypointIndex);
        Assert.IsTrue(forward);
        Assert.AreEqual(0f, progress);

        (waypointIndex, forward, progress) = state.GetMovementState(32);
        Assert.AreEqual(0, waypointIndex);
        Assert.IsTrue(forward);
        Assert.AreEqual(0f, progress);
    }

    #endregion

    #region Full Capacity Tests

    [TestMethod]
    public void Test_FullCapacity_32Entries()
    {
        var state = new ZoneSensorMovementState { SensorCount = 32 };

        // Fill all 32 entries with different values
        for (int i = 0; i < 32; i++)
        {
            int globalIndex = i * 4;
            int waypointIndex = i % 20;
            bool forward = i % 3 != 0;
            float progress = i / 32f;

            state.SetMovementState(i, globalIndex, waypointIndex, forward, progress);
        }

        // Verify all 32 entries
        for (int i = 0; i < 32; i++)
        {
            int expectedGlobal = i * 4;
            int expectedWaypoint = i % 20;
            bool expectedForward = i % 3 != 0;
            float expectedProgress = i / 32f;

            Assert.AreEqual(expectedGlobal, state.GetGlobalSensorIndex(i), $"Entry {i} global index");

            var (waypointIndex, forward, progress) = state.GetMovementState(i);
            Assert.AreEqual(expectedWaypoint, waypointIndex, $"Entry {i} waypoint index");
            Assert.AreEqual(expectedForward, forward, $"Entry {i} direction");
            Assert.AreEqual(expectedProgress, progress, 1f / 255f + 0.001f, $"Entry {i} progress");
        }
    }

    #endregion
}
