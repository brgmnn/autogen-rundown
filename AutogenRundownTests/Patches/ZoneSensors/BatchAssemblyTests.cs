using AutogenRundown.Patches.ZoneSensors;
using UnityEngine;

namespace AutogenRundownTests.Patches.ZoneSensors;

/// <summary>
/// Tests for batch assembly logic simulating late joiner scenarios.
/// These test the data structures without requiring network mocking or Vector3.
/// </summary>
[TestClass]
public class BatchAssembly_Tests
{
    #region Position Batch Metadata Tests

    [TestMethod]
    public void Test_PositionBatches_MetadataInOrder()
    {
        // Simulate receiving 3 batches for 48 sensors (16 + 16 + 16)
        var batches = new ZoneSensorPositionState[3];

        for (int b = 0; b < 3; b++)
        {
            batches[b] = new ZoneSensorPositionState
            {
                SensorCount = 16,
                BatchIndex = (byte)b,
                TotalBatches = 3
            };
        }

        // Verify metadata
        Assert.AreEqual(0, batches[0].BatchIndex);
        Assert.AreEqual(1, batches[1].BatchIndex);
        Assert.AreEqual(2, batches[2].BatchIndex);

        // Verify all know total batches
        foreach (var batch in batches)
        {
            Assert.AreEqual(3, batch.TotalBatches);
        }
    }

    [TestMethod]
    public void Test_PositionBatches_GlobalIndexCalculation()
    {
        var batches = new ZoneSensorPositionState[3];

        for (int b = 0; b < 3; b++)
        {
            batches[b] = new ZoneSensorPositionState
            {
                SensorCount = 16,
                BatchIndex = (byte)b,
                TotalBatches = 3
            };
        }

        // Verify global index calculation across batches
        Assert.AreEqual(0, batches[0].GetGlobalSensorIndex(0));   // Batch 0, local 0
        Assert.AreEqual(15, batches[0].GetGlobalSensorIndex(15)); // Batch 0, local 15
        Assert.AreEqual(16, batches[1].GetGlobalSensorIndex(0));  // Batch 1, local 0
        Assert.AreEqual(31, batches[1].GetGlobalSensorIndex(15)); // Batch 1, local 15
        Assert.AreEqual(32, batches[2].GetGlobalSensorIndex(0));  // Batch 2, local 0
        Assert.AreEqual(47, batches[2].GetGlobalSensorIndex(15)); // Batch 2, local 15
    }

    [TestMethod]
    public void Test_PositionBatches_PartialLastBatch()
    {
        // 20 sensors = 1 full batch (16) + 1 partial batch (4)
        int totalSensors = 20;
        int batchCount = ZoneSensorPositionState.CalculateBatchCount(totalSensors);
        Assert.AreEqual(2, batchCount);

        var batch0 = new ZoneSensorPositionState { SensorCount = 16, BatchIndex = 0, TotalBatches = 2 };
        var batch1 = new ZoneSensorPositionState { SensorCount = 4, BatchIndex = 1, TotalBatches = 2 };

        // Verify correct sensor counts
        Assert.AreEqual(16, batch0.SensorCount);
        Assert.AreEqual(4, batch1.SensorCount);

        // Verify global indices for partial batch
        Assert.AreEqual(16, batch1.GetGlobalSensorIndex(0));
        Assert.AreEqual(17, batch1.GetGlobalSensorIndex(1));
        Assert.AreEqual(18, batch1.GetGlobalSensorIndex(2));
        Assert.AreEqual(19, batch1.GetGlobalSensorIndex(3));
    }

    [TestMethod]
    public void Test_PositionBatches_AssembleOutOfOrder()
    {
        // Create 3 batches
        var batch0 = new ZoneSensorPositionState { SensorCount = 16, BatchIndex = 0, TotalBatches = 3 };
        var batch1 = new ZoneSensorPositionState { SensorCount = 16, BatchIndex = 1, TotalBatches = 3 };
        var batch2 = new ZoneSensorPositionState { SensorCount = 16, BatchIndex = 2, TotalBatches = 3 };

        // Simulate receiving out of order: 2, 0, 1
        var received = new ZoneSensorPositionState?[3];
        received[batch2.BatchIndex] = batch2;  // Receive batch 2 first
        received[batch0.BatchIndex] = batch0;  // Receive batch 0 second
        received[batch1.BatchIndex] = batch1;  // Receive batch 1 last

        // All batches should be present after receiving all
        Assert.IsTrue(received.All(b => b.HasValue));

        // Verify they're in correct order by BatchIndex
        Assert.AreEqual(0, received[0]!.Value.BatchIndex);
        Assert.AreEqual(1, received[1]!.Value.BatchIndex);
        Assert.AreEqual(2, received[2]!.Value.BatchIndex);
    }

    #endregion

    #region Waypoint Batch Metadata Tests

    [TestMethod]
    public void Test_WaypointBatch_SensorIndexPreserved()
    {
        var state = new ZoneSensorWaypointState
        {
            SensorIndex = 7,
            BatchIndex = 0,
            TotalBatches = 1,
            WaypointCount = 10,
            Speed = 2.5f
        };

        Assert.AreEqual(7, state.SensorIndex);
    }

    [TestMethod]
    public void Test_WaypointBatch_MultipleBatchesMetadata()
    {
        // Simulate 3 batches for a sensor with 50 waypoints (20 + 20 + 10)
        var batches = new ZoneSensorWaypointState[3];

        batches[0] = new ZoneSensorWaypointState
        {
            SensorIndex = 5,
            BatchIndex = 0,
            TotalBatches = 3,
            WaypointCount = 20,
            Speed = 3.0f
        };

        batches[1] = new ZoneSensorWaypointState
        {
            SensorIndex = 5,
            BatchIndex = 1,
            TotalBatches = 3,
            WaypointCount = 20,
            Speed = 0f  // Speed only in first batch
        };

        batches[2] = new ZoneSensorWaypointState
        {
            SensorIndex = 5,
            BatchIndex = 2,
            TotalBatches = 3,
            WaypointCount = 10,
            Speed = 0f
        };

        // Verify all have same sensor index
        foreach (var batch in batches)
        {
            Assert.AreEqual(5, batch.SensorIndex);
            Assert.AreEqual(3, batch.TotalBatches);
        }

        // Verify batch indices are sequential
        Assert.AreEqual(0, batches[0].BatchIndex);
        Assert.AreEqual(1, batches[1].BatchIndex);
        Assert.AreEqual(2, batches[2].BatchIndex);

        // Verify only first batch has speed
        Assert.AreEqual(3.0f, batches[0].Speed);
        Assert.AreEqual(0f, batches[1].Speed);
        Assert.AreEqual(0f, batches[2].Speed);

        // Verify total waypoint count
        int totalWaypoints = batches.Sum(b => b.WaypointCount);
        Assert.AreEqual(50, totalWaypoints);
    }

    [TestMethod]
    public void Test_WaypointBatch_CalculateBatchCount()
    {
        // Test batch count calculation for waypoints
        Assert.AreEqual(0, ZoneSensorWaypointState.CalculateBatchCount(0));
        Assert.AreEqual(1, ZoneSensorWaypointState.CalculateBatchCount(1));
        Assert.AreEqual(1, ZoneSensorWaypointState.CalculateBatchCount(20));
        Assert.AreEqual(2, ZoneSensorWaypointState.CalculateBatchCount(21));
        Assert.AreEqual(3, ZoneSensorWaypointState.CalculateBatchCount(45));
        Assert.AreEqual(3, ZoneSensorWaypointState.CalculateBatchCount(60));
        Assert.AreEqual(8, ZoneSensorWaypointState.CalculateBatchCount(160));
    }

    #endregion

    #region Movement State Sparse Storage Tests

    [TestMethod]
    public void Test_MovementState_SparseStorage()
    {
        var state = new ZoneSensorMovementState { SensorCount = 3, BatchIndex = 0 };

        // Store non-consecutive global indices
        state.SetMovementState(0, 5, 2, true, 0.1f);
        state.SetMovementState(1, 20, 4, false, 0.5f);
        state.SetMovementState(2, 100, 8, true, 0.9f);

        // Verify global indices are preserved
        Assert.AreEqual(5, state.GetGlobalSensorIndex(0));
        Assert.AreEqual(20, state.GetGlobalSensorIndex(1));
        Assert.AreEqual(100, state.GetGlobalSensorIndex(2));

        // Verify movement data is correct for each entry
        var (wp0, fwd0, prog0) = state.GetMovementState(0);
        Assert.AreEqual(2, wp0);
        Assert.IsTrue(fwd0);
        Assert.AreEqual(0.1f, prog0, 1f / 255f);

        var (wp1, fwd1, prog1) = state.GetMovementState(1);
        Assert.AreEqual(4, wp1);
        Assert.IsFalse(fwd1);
        Assert.AreEqual(0.5f, prog1, 1f / 255f);

        var (wp2, fwd2, prog2) = state.GetMovementState(2);
        Assert.AreEqual(8, wp2);
        Assert.IsTrue(fwd2);
        Assert.AreEqual(0.9f, prog2, 1f / 255f);
    }

    [TestMethod]
    public void Test_MovementState_MultipleBatches()
    {
        // 40 sensors need 2 batches (32 + 8)
        var batch0 = new ZoneSensorMovementState { SensorCount = 32, BatchIndex = 0 };
        var batch1 = new ZoneSensorMovementState { SensorCount = 8, BatchIndex = 1 };

        // Fill batch 0 with sensors 0-31
        for (int i = 0; i < 32; i++)
        {
            batch0.SetMovementState(i, i, i % 10, i % 2 == 0, i / 40f);
        }

        // Fill batch 1 with sensors 32-39
        for (int i = 0; i < 8; i++)
        {
            batch1.SetMovementState(i, 32 + i, (32 + i) % 10, (32 + i) % 2 == 0, (32 + i) / 40f);
        }

        // Verify batch 0
        for (int i = 0; i < 32; i++)
        {
            Assert.AreEqual(i, batch0.GetGlobalSensorIndex(i), $"Batch 0 entry {i}");
        }

        // Verify batch 1
        for (int i = 0; i < 8; i++)
        {
            Assert.AreEqual(32 + i, batch1.GetGlobalSensorIndex(i), $"Batch 1 entry {i}");
        }
    }

    #endregion

    #region Late Joiner Simulation Tests

    [TestMethod]
    public void Test_LateJoiner_ReceiveAllPositionBatches()
    {
        // Simulate: Host has 35 sensors, late joiner receives batches
        int totalSensors = 35;
        int totalBatches = ZoneSensorPositionState.CalculateBatchCount(totalSensors);

        Assert.AreEqual(3, totalBatches); // 16 + 16 + 3

        // Create batches as host would send them
        var batches = new ZoneSensorPositionState[totalBatches];
        for (int b = 0; b < totalBatches; b++)
        {
            int startIndex = b * 16;
            int count = Math.Min(16, totalSensors - startIndex);

            batches[b] = new ZoneSensorPositionState
            {
                SensorCount = (byte)count,
                BatchIndex = (byte)b,
                TotalBatches = (byte)totalBatches
            };
        }

        // Verify batch sensor counts
        Assert.AreEqual(16, batches[0].SensorCount);
        Assert.AreEqual(16, batches[1].SensorCount);
        Assert.AreEqual(3, batches[2].SensorCount);

        // Verify total sensor count from batches
        int totalFromBatches = batches.Sum(b => b.SensorCount);
        Assert.AreEqual(totalSensors, totalFromBatches);
    }

    [TestMethod]
    public void Test_LateJoiner_GroupStateConsistent()
    {
        // Host state: some sensors triggered and disabled
        var hostState = new ZoneSensorGroupState(true);
        hostState.SetSensorTriggered(5);
        hostState.SetSensorTriggered(50);
        hostState.ApplyTriggeredMask();

        // Late joiner receives exact state via CreateWithMasks
        var clientState = ZoneSensorGroupState.CreateWithMasks(
            hostState.Enabled,
            hostState.SensorMask0, hostState.SensorMask1, hostState.SensorMask2, hostState.SensorMask3,
            hostState.TriggeredMask0, hostState.TriggeredMask1, hostState.TriggeredMask2, hostState.TriggeredMask3
        );

        // Verify same enabled/disabled state
        for (int i = 0; i < 128; i++)
        {
            Assert.AreEqual(hostState.IsSensorEnabled(i), clientState.IsSensorEnabled(i), $"Sensor {i} enabled mismatch");
            Assert.AreEqual(hostState.IsSensorTriggered(i), clientState.IsSensorTriggered(i), $"Sensor {i} triggered mismatch");
        }
    }

    [TestMethod]
    public void Test_LateJoiner_WaypointCountsPreserved()
    {
        // Host sets waypoint counts for 16 sensors
        var hostBatch = new ZoneSensorPositionState
        {
            SensorCount = 16,
            BatchIndex = 0,
            TotalBatches = 1
        };

        for (int i = 0; i < 16; i++)
        {
            hostBatch.SetWaypointCount(i, i % 4);
        }

        // Late joiner receives batch (simulated by copying WaypointCounts)
        var clientBatch = new ZoneSensorPositionState
        {
            SensorCount = hostBatch.SensorCount,
            BatchIndex = hostBatch.BatchIndex,
            TotalBatches = hostBatch.TotalBatches,
            WaypointCounts = hostBatch.WaypointCounts
        };

        // Verify all waypoint counts match
        for (int i = 0; i < 16; i++)
        {
            Assert.AreEqual(hostBatch.GetWaypointCount(i), clientBatch.GetWaypointCount(i),
                $"Sensor {i} waypoint count mismatch");
        }
    }

    #endregion

    #region Position Batch Vector3 Assembly Tests

    [TestMethod]
    public void Test_PositionBatches_AssembleInOrder()
    {
        // Create 3 batches with actual Vector3 positions
        var batches = new ZoneSensorPositionState[3];

        for (int b = 0; b < 3; b++)
        {
            batches[b] = new ZoneSensorPositionState
            {
                SensorCount = 16,
                BatchIndex = (byte)b,
                TotalBatches = 3
            };

            // Set positions for each sensor in batch
            for (int s = 0; s < 16; s++)
            {
                int globalIndex = b * 16 + s;
                batches[b].SetPosition(s, new Vector3(globalIndex * 10f, globalIndex * 20f, globalIndex * 30f));
            }
        }

        // Assemble all positions into a single array (simulating late joiner reconstruction)
        var allPositions = new Vector3[48];
        foreach (var batch in batches)
        {
            for (int s = 0; s < batch.SensorCount; s++)
            {
                int globalIndex = batch.GetGlobalSensorIndex(s);
                allPositions[globalIndex] = batch.GetPosition(s);
            }
        }

        // Verify positions are in correct order
        for (int i = 0; i < 48; i++)
        {
            Assert.AreEqual(i * 10f, allPositions[i].x, 0.0001f, $"Position {i} X mismatch");
            Assert.AreEqual(i * 20f, allPositions[i].y, 0.0001f, $"Position {i} Y mismatch");
            Assert.AreEqual(i * 30f, allPositions[i].z, 0.0001f, $"Position {i} Z mismatch");
        }
    }

    [TestMethod]
    public void Test_PositionBatches_AssembleOutOfOrderWithPositions()
    {
        // Create batches but receive them out of order
        var batch0 = new ZoneSensorPositionState { SensorCount = 16, BatchIndex = 0, TotalBatches = 3 };
        var batch1 = new ZoneSensorPositionState { SensorCount = 16, BatchIndex = 1, TotalBatches = 3 };
        var batch2 = new ZoneSensorPositionState { SensorCount = 16, BatchIndex = 2, TotalBatches = 3 };

        // Set positions
        for (int s = 0; s < 16; s++)
        {
            batch0.SetPosition(s, new Vector3(s, 0, 0));
            batch1.SetPosition(s, new Vector3(s + 16, 0, 0));
            batch2.SetPosition(s, new Vector3(s + 32, 0, 0));
        }

        // Receive out of order: batch2, batch0, batch1
        var received = new ZoneSensorPositionState?[3];
        received[batch2.BatchIndex] = batch2;
        received[batch0.BatchIndex] = batch0;
        received[batch1.BatchIndex] = batch1;

        // Assemble in correct order using BatchIndex
        var allPositions = new Vector3[48];
        for (int b = 0; b < 3; b++)
        {
            var batch = received[b]!.Value;
            for (int s = 0; s < batch.SensorCount; s++)
            {
                int globalIndex = batch.GetGlobalSensorIndex(s);
                allPositions[globalIndex] = batch.GetPosition(s);
            }
        }

        // Verify all positions are correctly ordered
        for (int i = 0; i < 48; i++)
        {
            Assert.AreEqual((float)i, allPositions[i].x, 0.0001f, $"Position {i} X mismatch");
        }
    }

    #endregion

    #region Waypoint Batch Vector3 Assembly Tests

    [TestMethod]
    public void Test_WaypointBatches_ToArrayRoundTrip()
    {
        // Create a waypoint state with positions
        var state = new ZoneSensorWaypointState
        {
            SensorIndex = 3,
            BatchIndex = 0,
            TotalBatches = 1,
            WaypointCount = 10,
            Speed = 2.5f
        };

        // Set waypoints
        for (int i = 0; i < 10; i++)
        {
            state.SetWaypoint(i, new Vector3(i * 5f, i * 10f, i * 15f));
        }

        // Convert to array and verify
        var array = state.ToArray();
        Assert.AreEqual(10, array.Length);

        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual(i * 5f, array[i].x, 0.0001f, $"Waypoint {i} X mismatch");
            Assert.AreEqual(i * 10f, array[i].y, 0.0001f, $"Waypoint {i} Y mismatch");
            Assert.AreEqual(i * 15f, array[i].z, 0.0001f, $"Waypoint {i} Z mismatch");
        }
    }

    [TestMethod]
    public void Test_WaypointBatches_MultipleBatchesAssembly()
    {
        // Simulate 45 waypoints split across 3 batches (20 + 20 + 5)
        var waypoints = new Vector3[45];
        for (int i = 0; i < 45; i++)
        {
            waypoints[i] = new Vector3(i, i * 2, i * 3);
        }

        // Create batches manually (simulating what FromArrayBatched would do)
        var batch0 = new ZoneSensorWaypointState
        {
            SensorIndex = 0,
            BatchIndex = 0,
            TotalBatches = 3,
            WaypointCount = 20,
            Speed = 3.0f
        };

        var batch1 = new ZoneSensorWaypointState
        {
            SensorIndex = 0,
            BatchIndex = 1,
            TotalBatches = 3,
            WaypointCount = 20,
            Speed = 0f
        };

        var batch2 = new ZoneSensorWaypointState
        {
            SensorIndex = 0,
            BatchIndex = 2,
            TotalBatches = 3,
            WaypointCount = 5,
            Speed = 0f
        };

        // Set waypoints in each batch
        for (int i = 0; i < 20; i++)
        {
            batch0.SetWaypoint(i, waypoints[i]);
            batch1.SetWaypoint(i, waypoints[20 + i]);
        }
        for (int i = 0; i < 5; i++)
        {
            batch2.SetWaypoint(i, waypoints[40 + i]);
        }

        // Assemble waypoints from batches
        var assembled = new List<Vector3>();
        assembled.AddRange(batch0.ToArray());
        assembled.AddRange(batch1.ToArray());
        assembled.AddRange(batch2.ToArray());

        Assert.AreEqual(45, assembled.Count);

        // Verify all waypoints match original
        for (int i = 0; i < 45; i++)
        {
            Assert.AreEqual(waypoints[i].x, assembled[i].x, 0.0001f, $"Waypoint {i} X mismatch");
            Assert.AreEqual(waypoints[i].y, assembled[i].y, 0.0001f, $"Waypoint {i} Y mismatch");
            Assert.AreEqual(waypoints[i].z, assembled[i].z, 0.0001f, $"Waypoint {i} Z mismatch");
        }
    }

    #endregion
}
