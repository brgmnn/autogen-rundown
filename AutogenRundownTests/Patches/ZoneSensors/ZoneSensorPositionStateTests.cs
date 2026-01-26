using AutogenRundown.Patches.ZoneSensors;
using UnityEngine;

namespace AutogenRundownTests.Patches.ZoneSensors;

[TestClass]
public class ZoneSensorPositionState_Tests
{
    #region CalculateBatchCount Tests

    [TestMethod]
    public void Test_CalculateBatchCount_ZeroSensors()
    {
        Assert.AreEqual(0, ZoneSensorPositionState.CalculateBatchCount(0));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_OneSensor()
    {
        Assert.AreEqual(1, ZoneSensorPositionState.CalculateBatchCount(1));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_SixteenSensors()
    {
        // Exactly one batch (16 is max per batch)
        Assert.AreEqual(1, ZoneSensorPositionState.CalculateBatchCount(16));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_SeventeenSensors()
    {
        // 17 sensors requires 2 batches
        Assert.AreEqual(2, ZoneSensorPositionState.CalculateBatchCount(17));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_128Sensors()
    {
        // 128 sensors = 8 batches (128 / 16)
        Assert.AreEqual(8, ZoneSensorPositionState.CalculateBatchCount(128));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_BoundaryConditions()
    {
        // Test exact boundaries between batch counts
        for (int batches = 1; batches <= 8; batches++)
        {
            int maxForBatch = batches * 16;
            int minForNextBatch = maxForBatch + 1;

            Assert.AreEqual(batches, ZoneSensorPositionState.CalculateBatchCount(maxForBatch),
                $"Expected {batches} batches for {maxForBatch} sensors");

            Assert.AreEqual(batches + 1, ZoneSensorPositionState.CalculateBatchCount(minForNextBatch),
                $"Expected {batches + 1} batches for {minForNextBatch} sensors");
        }
    }

    #endregion

    #region WaypointCount Tests

    [TestMethod]
    public void Test_WaypointCount_TwoBitPacking()
    {
        var state = new ZoneSensorPositionState();

        // Test all valid values (0-3)
        for (int value = 0; value <= 3; value++)
        {
            state.SetWaypointCount(0, value);
            Assert.AreEqual(value, state.GetWaypointCount(0), $"Failed for value {value}");
        }
    }

    [TestMethod]
    public void Test_WaypointCount_Clamping()
    {
        var state = new ZoneSensorPositionState();

        // Values > 3 should be clamped
        state.SetWaypointCount(0, 4);
        Assert.AreEqual(3, state.GetWaypointCount(0));

        state.SetWaypointCount(0, 100);
        Assert.AreEqual(3, state.GetWaypointCount(0));

        // Negative values should be clamped to 0
        state.SetWaypointCount(0, -1);
        Assert.AreEqual(0, state.GetWaypointCount(0));
    }

    [TestMethod]
    public void Test_WaypointCount_AllSensors()
    {
        var state = new ZoneSensorPositionState { SensorCount = 16, BatchIndex = 0, TotalBatches = 1 };

        // Set different waypoint counts for each sensor
        for (int i = 0; i < 16; i++)
        {
            state.SetWaypointCount(i, i % 4); // Values 0, 1, 2, 3, 0, 1, 2, 3, ...
        }

        // Verify all are independent
        for (int i = 0; i < 16; i++)
        {
            Assert.AreEqual(i % 4, state.GetWaypointCount(i), $"Sensor {i} waypoint count mismatch");
        }
    }

    [TestMethod]
    public void Test_WaypointCount_InvalidIndices()
    {
        var state = new ZoneSensorPositionState();

        // Invalid indices should return 0
        Assert.AreEqual(0, state.GetWaypointCount(-1));
        Assert.AreEqual(0, state.GetWaypointCount(16));
    }

    [TestMethod]
    public void Test_WaypointCount_BitIsolation()
    {
        var state = new ZoneSensorPositionState();

        // Set sensor 0 to 3 (binary 11)
        state.SetWaypointCount(0, 3);

        // Set sensor 1 to 1 (binary 01)
        state.SetWaypointCount(1, 1);

        // Verify sensor 0 is still 3 and sensor 1 is 1
        Assert.AreEqual(3, state.GetWaypointCount(0));
        Assert.AreEqual(1, state.GetWaypointCount(1));

        // Verify raw packing: bits should be ...0001_11 = 0x07
        // Sensor 0 uses bits 0-1 (value 3 = 0b11)
        // Sensor 1 uses bits 2-3 (value 1 = 0b01)
        uint expected = (1u << 2) | 3u; // 0b0111 = 7
        Assert.AreEqual(expected, state.WaypointCounts);
    }

    #endregion

    #region GetGlobalSensorIndex Tests

    [TestMethod]
    public void Test_GetGlobalSensorIndex_Batch0()
    {
        var state = new ZoneSensorPositionState { BatchIndex = 0 };

        // Batch 0, local 5 → global 5
        Assert.AreEqual(5, state.GetGlobalSensorIndex(5));
        Assert.AreEqual(0, state.GetGlobalSensorIndex(0));
        Assert.AreEqual(15, state.GetGlobalSensorIndex(15));
    }

    [TestMethod]
    public void Test_GetGlobalSensorIndex_Batch2()
    {
        var state = new ZoneSensorPositionState { BatchIndex = 2 };

        // Batch 2, local 3 → global 35 (2 * 16 + 3)
        Assert.AreEqual(35, state.GetGlobalSensorIndex(3));
        Assert.AreEqual(32, state.GetGlobalSensorIndex(0));
        Assert.AreEqual(47, state.GetGlobalSensorIndex(15));
    }

    [TestMethod]
    public void Test_GetGlobalSensorIndex_AllBatches()
    {
        // Test all 8 possible batches (for 128 sensors max)
        for (byte batchIndex = 0; batchIndex < 8; batchIndex++)
        {
            var state = new ZoneSensorPositionState { BatchIndex = batchIndex };

            for (int localIndex = 0; localIndex < 16; localIndex++)
            {
                int expected = batchIndex * 16 + localIndex;
                Assert.AreEqual(expected, state.GetGlobalSensorIndex(localIndex),
                    $"Batch {batchIndex}, local {localIndex}");
            }
        }
    }

    #endregion

    #region HasPositions Tests

    [TestMethod]
    public void Test_HasPositions_True()
    {
        var state = new ZoneSensorPositionState { SensorCount = 1 };
        Assert.IsTrue(state.HasPositions);

        state.SensorCount = 16;
        Assert.IsTrue(state.HasPositions);
    }

    [TestMethod]
    public void Test_HasPositions_False()
    {
        var state = new ZoneSensorPositionState { SensorCount = 0 };
        Assert.IsFalse(state.HasPositions);
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Test_DefaultConstructor()
    {
        var state = new ZoneSensorPositionState();

        Assert.AreEqual(0, state.SensorCount);
        Assert.AreEqual(0, state.BatchIndex);
        Assert.AreEqual(1, state.TotalBatches);
        Assert.AreEqual(0u, state.WaypointCounts);
    }

    #endregion

    #region Constants Tests

    [TestMethod]
    public void Test_MaxSensorsPerBatch_Constant()
    {
        Assert.AreEqual(16, ZoneSensorPositionState.MaxSensorsPerBatch);
    }

    #endregion

    #region Field Assignment Tests

    [TestMethod]
    public void Test_FieldAssignment_SensorCount()
    {
        var state = new ZoneSensorPositionState();
        state.SensorCount = 10;
        Assert.AreEqual(10, state.SensorCount);
    }

    [TestMethod]
    public void Test_FieldAssignment_BatchIndex()
    {
        var state = new ZoneSensorPositionState();
        state.BatchIndex = 5;
        Assert.AreEqual(5, state.BatchIndex);
    }

    [TestMethod]
    public void Test_FieldAssignment_TotalBatches()
    {
        var state = new ZoneSensorPositionState();
        state.TotalBatches = 8;
        Assert.AreEqual(8, state.TotalBatches);
    }

    #endregion

    #region SetPosition/GetPosition Tests

    [TestMethod]
    public void Test_SetGetPosition_RoundTrip()
    {
        var state = new ZoneSensorPositionState();
        var position = new Vector3(1.5f, 2.5f, 3.5f);

        state.SetPosition(0, position);
        var result = state.GetPosition(0);

        Assert.AreEqual(position.x, result.x, 0.0001f);
        Assert.AreEqual(position.y, result.y, 0.0001f);
        Assert.AreEqual(position.z, result.z, 0.0001f);
    }

    [TestMethod]
    public void Test_SetGetPosition_AllSlots()
    {
        var state = new ZoneSensorPositionState { SensorCount = 16 };

        // Set positions for all 16 slots
        for (int i = 0; i < 16; i++)
        {
            state.SetPosition(i, new Vector3(i * 10f, i * 20f, i * 30f));
        }

        // Verify all positions are independent and correct
        for (int i = 0; i < 16; i++)
        {
            var pos = state.GetPosition(i);
            Assert.AreEqual(i * 10f, pos.x, 0.0001f, $"Sensor {i} X mismatch");
            Assert.AreEqual(i * 20f, pos.y, 0.0001f, $"Sensor {i} Y mismatch");
            Assert.AreEqual(i * 30f, pos.z, 0.0001f, $"Sensor {i} Z mismatch");
        }
    }

    [TestMethod]
    public void Test_SetGetPosition_BoundaryIndices_Valid()
    {
        var state = new ZoneSensorPositionState();

        // First valid index
        state.SetPosition(0, new Vector3(1f, 2f, 3f));
        var pos0 = state.GetPosition(0);
        Assert.AreEqual(1f, pos0.x, 0.0001f);

        // Last valid index
        state.SetPosition(15, new Vector3(4f, 5f, 6f));
        var pos15 = state.GetPosition(15);
        Assert.AreEqual(4f, pos15.x, 0.0001f);
    }

    [TestMethod]
    public void Test_SetGetPosition_BoundaryIndices_Invalid()
    {
        var state = new ZoneSensorPositionState();

        // Invalid negative index returns zero
        var negativeResult = state.GetPosition(-1);
        Assert.AreEqual(0f, negativeResult.x);
        Assert.AreEqual(0f, negativeResult.y);
        Assert.AreEqual(0f, negativeResult.z);

        // Invalid index at 16 (out of bounds) returns zero
        var outOfBoundsResult = state.GetPosition(16);
        Assert.AreEqual(0f, outOfBoundsResult.x);
        Assert.AreEqual(0f, outOfBoundsResult.y);
        Assert.AreEqual(0f, outOfBoundsResult.z);

        // Setting invalid indices should be no-op (doesn't crash)
        state.SetPosition(-1, new Vector3(100f, 100f, 100f));
        state.SetPosition(16, new Vector3(100f, 100f, 100f));
    }

    [TestMethod]
    public void Test_SetGetPosition_NegativeCoordinates()
    {
        var state = new ZoneSensorPositionState();
        var position = new Vector3(-100.5f, -200.25f, -300.125f);

        state.SetPosition(5, position);
        var result = state.GetPosition(5);

        Assert.AreEqual(position.x, result.x, 0.0001f);
        Assert.AreEqual(position.y, result.y, 0.0001f);
        Assert.AreEqual(position.z, result.z, 0.0001f);
    }

    [TestMethod]
    public void Test_SetGetPosition_LargeCoordinates()
    {
        var state = new ZoneSensorPositionState();
        var position = new Vector3(10000.0f, 50000.0f, 99999.0f);

        state.SetPosition(8, position);
        var result = state.GetPosition(8);

        Assert.AreEqual(position.x, result.x, 0.01f);
        Assert.AreEqual(position.y, result.y, 0.01f);
        Assert.AreEqual(position.z, result.z, 0.01f);
    }

    #endregion
}
