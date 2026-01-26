using System.Runtime.InteropServices;
using AutogenRundown.Patches.ZoneSensors;

namespace AutogenRundownTests.Patches.ZoneSensors;

/// <summary>
/// Tests that verify state structs produce deterministic/identical results
/// regardless of which "side" (host/client) processes them.
/// </summary>
[TestClass]
public class StateConsistency_Tests
{
    #region Position State Determinism

    [TestMethod]
    public void Test_PositionState_MetadataDeterministic()
    {
        // Create two states with identical metadata inputs
        var state1 = new ZoneSensorPositionState
        {
            SensorCount = 5,
            BatchIndex = 2,
            TotalBatches = 3
        };

        var state2 = new ZoneSensorPositionState
        {
            SensorCount = 5,
            BatchIndex = 2,
            TotalBatches = 3
        };

        Assert.AreEqual(state1.SensorCount, state2.SensorCount);
        Assert.AreEqual(state1.BatchIndex, state2.BatchIndex);
        Assert.AreEqual(state1.TotalBatches, state2.TotalBatches);
    }

    [TestMethod]
    public void Test_PositionState_WaypointCountsDeterministic()
    {
        var state1 = new ZoneSensorPositionState { SensorCount = 16 };
        var state2 = new ZoneSensorPositionState { SensorCount = 16 };

        // Set same waypoint counts
        for (int i = 0; i < 16; i++)
        {
            state1.SetWaypointCount(i, i % 4);
            state2.SetWaypointCount(i, i % 4);
        }

        Assert.AreEqual(state1.WaypointCounts, state2.WaypointCounts);
    }

    [TestMethod]
    public void Test_PositionState_GlobalIndexDeterministic()
    {
        var state1 = new ZoneSensorPositionState { BatchIndex = 3 };
        var state2 = new ZoneSensorPositionState { BatchIndex = 3 };

        for (int i = 0; i < 16; i++)
        {
            Assert.AreEqual(state1.GetGlobalSensorIndex(i), state2.GetGlobalSensorIndex(i));
        }
    }

    #endregion

    #region Group State Determinism

    [TestMethod]
    public void Test_GroupState_Deterministic()
    {
        var state1 = new ZoneSensorGroupState(true);
        var state2 = new ZoneSensorGroupState(true);

        // Apply same operations
        state1.SetSensorDisabled(10);
        state1.SetSensorDisabled(50);
        state1.SetSensorTriggered(75);
        state1.SetSensorTriggered(100);

        state2.SetSensorDisabled(10);
        state2.SetSensorDisabled(50);
        state2.SetSensorTriggered(75);
        state2.SetSensorTriggered(100);

        // Compare all fields
        Assert.AreEqual(state1.Enabled, state2.Enabled);
        Assert.AreEqual(state1.SensorMask0, state2.SensorMask0);
        Assert.AreEqual(state1.SensorMask1, state2.SensorMask1);
        Assert.AreEqual(state1.SensorMask2, state2.SensorMask2);
        Assert.AreEqual(state1.SensorMask3, state2.SensorMask3);
        Assert.AreEqual(state1.TriggeredMask0, state2.TriggeredMask0);
        Assert.AreEqual(state1.TriggeredMask1, state2.TriggeredMask1);
        Assert.AreEqual(state1.TriggeredMask2, state2.TriggeredMask2);
        Assert.AreEqual(state1.TriggeredMask3, state2.TriggeredMask3);
    }

    [TestMethod]
    public void Test_GroupState_MaskFieldsDeterministic()
    {
        var state1 = new ZoneSensorGroupState(true);
        var state2 = new ZoneSensorGroupState(true);

        // Disable sensors in each field
        int[] indices = { 0, 31, 32, 63, 64, 95, 96, 127 };
        foreach (int i in indices)
        {
            state1.SetSensorDisabled(i);
            state2.SetSensorDisabled(i);
        }

        Assert.AreEqual(state1.SensorMask0, state2.SensorMask0);
        Assert.AreEqual(state1.SensorMask1, state2.SensorMask1);
        Assert.AreEqual(state1.SensorMask2, state2.SensorMask2);
        Assert.AreEqual(state1.SensorMask3, state2.SensorMask3);
    }

    [TestMethod]
    public void Test_GroupState_ApplyTriggeredDeterministic()
    {
        var state1 = new ZoneSensorGroupState(true);
        var state2 = new ZoneSensorGroupState(true);

        // Same trigger pattern
        state1.SetSensorTriggered(25);
        state1.SetSensorTriggered(50);
        state1.ApplyTriggeredMask();

        state2.SetSensorTriggered(25);
        state2.SetSensorTriggered(50);
        state2.ApplyTriggeredMask();

        Assert.AreEqual(state1.SensorMask0, state2.SensorMask0);
        Assert.AreEqual(state1.SensorMask1, state2.SensorMask1);
        Assert.AreEqual(state1.SensorMask2, state2.SensorMask2);
        Assert.AreEqual(state1.SensorMask3, state2.SensorMask3);
    }

    [TestMethod]
    public void Test_GroupState_IsSensorEnabledDeterministic()
    {
        var state1 = new ZoneSensorGroupState(true);
        var state2 = new ZoneSensorGroupState(true);

        // Apply same modifications
        state1.SetSensorDisabled(15);
        state1.SetSensorDisabled(45);
        state1.SetSensorDisabled(75);
        state1.SetSensorDisabled(105);

        state2.SetSensorDisabled(15);
        state2.SetSensorDisabled(45);
        state2.SetSensorDisabled(75);
        state2.SetSensorDisabled(105);

        // All 128 sensors should report same enabled state
        for (int i = 0; i < 128; i++)
        {
            Assert.AreEqual(state1.IsSensorEnabled(i), state2.IsSensorEnabled(i),
                $"Sensor {i} enabled state differs");
        }
    }

    #endregion

    #region Movement State Determinism

    [TestMethod]
    public void Test_MovementState_Deterministic()
    {
        var state1 = new ZoneSensorMovementState { SensorCount = 5, BatchIndex = 0 };
        var state2 = new ZoneSensorMovementState { SensorCount = 5, BatchIndex = 0 };

        // Same movement data
        state1.SetMovementState(0, 0, 3, true, 0.25f);
        state1.SetMovementState(1, 1, 7, false, 0.5f);
        state1.SetMovementState(2, 2, 1, true, 0.75f);
        state1.SetMovementState(3, 3, 10, false, 0.0f);
        state1.SetMovementState(4, 4, 5, true, 1.0f);

        state2.SetMovementState(0, 0, 3, true, 0.25f);
        state2.SetMovementState(1, 1, 7, false, 0.5f);
        state2.SetMovementState(2, 2, 1, true, 0.75f);
        state2.SetMovementState(3, 3, 10, false, 0.0f);
        state2.SetMovementState(4, 4, 5, true, 1.0f);

        // Compare DirectionMask
        Assert.AreEqual(state1.DirectionMask, state2.DirectionMask);

        // Compare all movement states
        for (int i = 0; i < 5; i++)
        {
            var (wp1, fwd1, prog1) = state1.GetMovementState(i);
            var (wp2, fwd2, prog2) = state2.GetMovementState(i);

            Assert.AreEqual(wp1, wp2, $"Entry {i} waypoint index");
            Assert.AreEqual(fwd1, fwd2, $"Entry {i} direction");
            Assert.AreEqual(prog1, prog2, 0.001f, $"Entry {i} progress");
        }
    }

    [TestMethod]
    public void Test_MovementState_DirectionMaskDeterministic()
    {
        var state1 = new ZoneSensorMovementState { SensorCount = 10 };
        var state2 = new ZoneSensorMovementState { SensorCount = 10 };

        // Alternating directions
        for (int i = 0; i < 10; i++)
        {
            state1.SetMovementState(i, i, 0, i % 2 == 0, 0.5f);
            state2.SetMovementState(i, i, 0, i % 2 == 0, 0.5f);
        }

        Assert.AreEqual(state1.DirectionMask, state2.DirectionMask);
    }

    [TestMethod]
    public void Test_MovementState_GlobalIndexDeterministic()
    {
        var state1 = new ZoneSensorMovementState { SensorCount = 5 };
        var state2 = new ZoneSensorMovementState { SensorCount = 5 };

        // Set same global indices
        state1.SetMovementState(0, 10, 0, true, 0f);
        state1.SetMovementState(1, 25, 0, true, 0f);
        state1.SetMovementState(2, 50, 0, true, 0f);
        state1.SetMovementState(3, 75, 0, true, 0f);
        state1.SetMovementState(4, 100, 0, true, 0f);

        state2.SetMovementState(0, 10, 0, true, 0f);
        state2.SetMovementState(1, 25, 0, true, 0f);
        state2.SetMovementState(2, 50, 0, true, 0f);
        state2.SetMovementState(3, 75, 0, true, 0f);
        state2.SetMovementState(4, 100, 0, true, 0f);

        for (int i = 0; i < 5; i++)
        {
            Assert.AreEqual(state1.GetGlobalSensorIndex(i), state2.GetGlobalSensorIndex(i));
        }
    }

    #endregion

    #region Waypoint State Determinism

    [TestMethod]
    public void Test_WaypointState_MetadataDeterministic()
    {
        var state1 = new ZoneSensorWaypointState
        {
            SensorIndex = 5,
            BatchIndex = 2,
            TotalBatches = 4,
            WaypointCount = 15,
            Speed = 3.5f
        };

        var state2 = new ZoneSensorWaypointState
        {
            SensorIndex = 5,
            BatchIndex = 2,
            TotalBatches = 4,
            WaypointCount = 15,
            Speed = 3.5f
        };

        Assert.AreEqual(state1.SensorIndex, state2.SensorIndex);
        Assert.AreEqual(state1.BatchIndex, state2.BatchIndex);
        Assert.AreEqual(state1.TotalBatches, state2.TotalBatches);
        Assert.AreEqual(state1.WaypointCount, state2.WaypointCount);
        Assert.AreEqual(state1.Speed, state2.Speed, 0.001f);
    }

    [TestMethod]
    public void Test_WaypointState_HasWaypointsDeterministic()
    {
        var state1 = new ZoneSensorWaypointState { WaypointCount = 10 };
        var state2 = new ZoneSensorWaypointState { WaypointCount = 10 };

        Assert.AreEqual(state1.HasWaypoints, state2.HasWaypoints);
        Assert.IsTrue(state1.HasWaypoints);

        state1.WaypointCount = 0;
        state2.WaypointCount = 0;

        Assert.AreEqual(state1.HasWaypoints, state2.HasWaypoints);
        Assert.IsFalse(state1.HasWaypoints);
    }

    [TestMethod]
    public void Test_WaypointState_CalculateBatchCountDeterministic()
    {
        // Same inputs should produce same batch counts
        for (int count = 0; count <= 160; count += 10)
        {
            int result1 = ZoneSensorWaypointState.CalculateBatchCount(count);
            int result2 = ZoneSensorWaypointState.CalculateBatchCount(count);
            Assert.AreEqual(result1, result2, $"Count {count}");
        }
    }

    #endregion

    #region Cross-Operation Consistency

    [TestMethod]
    public void Test_PositionState_WaypointCountRoundTrip()
    {
        var state = new ZoneSensorPositionState { SensorCount = 16 };

        // Set and get should return identical values
        for (int i = 0; i < 16; i++)
        {
            int original = i % 4;
            state.SetWaypointCount(i, original);
            int retrieved = state.GetWaypointCount(i);
            Assert.AreEqual(original, retrieved, $"Sensor {i}");
        }
    }

    [TestMethod]
    public void Test_MovementState_RoundTripConsistent()
    {
        var state = new ZoneSensorMovementState { SensorCount = 32 };

        // Set all 32 entries with various values
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

    [TestMethod]
    public void Test_GroupState_EnableDisableRoundTrip()
    {
        var state = new ZoneSensorGroupState(true);

        // Disable then re-enable specific sensors
        int[] testIndices = { 0, 31, 32, 63, 64, 95, 96, 127 };

        foreach (int i in testIndices)
        {
            Assert.IsTrue(state.IsSensorEnabled(i), $"Sensor {i} should start enabled");
            state.SetSensorDisabled(i);
            Assert.IsFalse(state.IsSensorEnabled(i), $"Sensor {i} should be disabled");
        }

        // Re-enable all
        state.SetAllEnabled();

        foreach (int i in testIndices)
        {
            Assert.IsTrue(state.IsSensorEnabled(i), $"Sensor {i} should be re-enabled");
        }
    }

    [TestMethod]
    public void Test_GroupState_TriggerApplyRoundTrip()
    {
        var state = new ZoneSensorGroupState(true);

        // Trigger and apply
        state.SetSensorTriggered(50);
        Assert.IsTrue(state.IsSensorTriggered(50));
        Assert.IsTrue(state.IsSensorEnabled(50)); // Still enabled until applied

        state.ApplyTriggeredMask();
        Assert.IsFalse(state.IsSensorEnabled(50)); // Now disabled

        // Clear triggered
        state.ClearTriggered();
        Assert.IsFalse(state.IsSensorTriggered(50));
        Assert.IsFalse(state.IsSensorEnabled(50)); // Still disabled (clear doesn't re-enable)
    }

    #endregion
}
