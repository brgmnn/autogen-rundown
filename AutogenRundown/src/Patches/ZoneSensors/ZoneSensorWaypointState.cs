using System.Runtime.InteropServices;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Network replication state for sensor waypoints.
/// Syncs the actual NavMesh path waypoints (not just the random seed positions)
/// to ensure late joiners see sensors at correct positions.
///
/// This solves the issue where NavMesh.CalculatePath() can produce different
/// corner points on different clients even with the same input positions.
///
/// Supports batching for paths with more than 20 waypoints.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ZoneSensorWaypointState
{
    /// <summary>
    /// Maximum waypoints per batch. Limited to 20 to fit within
    /// StateReplicator's 256-byte payload limit (8 + 20*3*4 = 248 bytes).
    /// </summary>
    public const int MaxWaypointsPerBatch = 20;

    /// <summary>
    /// Maximum batches per sensor. Supports up to 160 waypoints (8 * 20).
    /// </summary>
    public const int MaxBatchesPerSensor = 8;

    /// <summary>
    /// Global sensor index within the group (0-127).
    /// </summary>
    public byte SensorIndex;

    /// <summary>
    /// Batch index for this sensor (0-based).
    /// </summary>
    public byte BatchIndex;

    /// <summary>
    /// Total number of batches for this sensor.
    /// </summary>
    public byte TotalBatches;

    /// <summary>
    /// Number of waypoints in THIS batch.
    /// </summary>
    public byte WaypointCount;

    /// <summary>
    /// Movement speed for this sensor. Only meaningful in batch 0.
    /// </summary>
    public float Speed;

    /// <summary>
    /// Fixed buffer for waypoint positions: [x0, y0, z0, x1, y1, z1, ...]
    /// 20 waypoints * 3 floats = 60 floats = 240 bytes
    /// Total struct: 8 header + 240 data = 248 bytes
    /// </summary>
    private fixed float _waypoints[MaxWaypointsPerBatch * 3];

    public ZoneSensorWaypointState()
    {
        SensorIndex = 0;
        BatchIndex = 0;
        TotalBatches = 0;
        WaypointCount = 0;
        Speed = 0f;
    }

    /// <summary>
    /// Sets the waypoint position at the given index within this batch.
    /// </summary>
    public void SetWaypoint(int index, Vector3 pos)
    {
        if (index < 0 || index >= MaxWaypointsPerBatch)
            return;

        int offset = index * 3;
        _waypoints[offset] = pos.x;
        _waypoints[offset + 1] = pos.y;
        _waypoints[offset + 2] = pos.z;
    }

    /// <summary>
    /// Gets the waypoint position at the given index within this batch.
    /// </summary>
    public Vector3 GetWaypoint(int index)
    {
        if (index < 0 || index >= MaxWaypointsPerBatch)
            return Vector3.zero;

        int offset = index * 3;
        return new Vector3(
            _waypoints[offset],
            _waypoints[offset + 1],
            _waypoints[offset + 2]);
    }

    /// <summary>
    /// Checks if this state has valid waypoint data.
    /// </summary>
    public bool HasWaypoints => WaypointCount > 0;

    /// <summary>
    /// Extracts all waypoints as an array.
    /// </summary>
    public Vector3[] ToArray()
    {
        var result = new Vector3[WaypointCount];
        for (int i = 0; i < WaypointCount; i++)
        {
            result[i] = GetWaypoint(i);
        }
        return result;
    }

    /// <summary>
    /// Creates a waypoint state from an array of positions (single batch, for backward compatibility).
    /// Use FromArrayBatched() for paths that may exceed MaxWaypointsPerBatch.
    /// </summary>
    public static ZoneSensorWaypointState FromArray(int sensorIndex, Vector3[] waypoints, float speed)
    {
        if (waypoints.Length > MaxWaypointsPerBatch)
            Plugin.Logger.LogWarning($"ZoneSensorWaypointState: Waypoint count {waypoints.Length} exceeds max {MaxWaypointsPerBatch}, use FromArrayBatched() instead");

        var state = new ZoneSensorWaypointState
        {
            SensorIndex = (byte)sensorIndex,
            BatchIndex = 0,
            TotalBatches = 1,
            WaypointCount = (byte)Math.Min(waypoints.Length, MaxWaypointsPerBatch),
            Speed = speed
        };

        for (int i = 0; i < state.WaypointCount; i++)
        {
            state.SetWaypoint(i, waypoints[i]);
        }

        return state;
    }

    /// <summary>
    /// Creates multiple waypoint state batches from an array of positions.
    /// Splits waypoints across batches of MaxWaypointsPerBatch.
    /// </summary>
    public static List<ZoneSensorWaypointState> FromArrayBatched(int sensorIndex, Vector3[] waypoints, float speed)
    {
        var batches = new List<ZoneSensorWaypointState>();
        int totalBatches = CalculateBatchCount(waypoints.Length);

        // Clamp to max supported batches
        if (totalBatches > MaxBatchesPerSensor)
        {
            Plugin.Logger.LogWarning($"ZoneSensorWaypointState: Waypoint count {waypoints.Length} requires {totalBatches} batches, clamping to {MaxBatchesPerSensor} (max {MaxBatchesPerSensor * MaxWaypointsPerBatch} waypoints)");
            totalBatches = MaxBatchesPerSensor;
        }

        int waypointIndex = 0;
        for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            int waypointsInBatch = Math.Min(MaxWaypointsPerBatch, waypoints.Length - waypointIndex);

            var state = new ZoneSensorWaypointState
            {
                SensorIndex = (byte)sensorIndex,
                BatchIndex = (byte)batchIndex,
                TotalBatches = (byte)totalBatches,
                WaypointCount = (byte)waypointsInBatch,
                Speed = batchIndex == 0 ? speed : 0f  // Speed only in first batch
            };

            for (int i = 0; i < waypointsInBatch; i++)
            {
                state.SetWaypoint(i, waypoints[waypointIndex + i]);
            }

            batches.Add(state);
            waypointIndex += waypointsInBatch;
        }

        return batches;
    }

    /// <summary>
    /// Calculates the number of batches needed for a given waypoint count.
    /// </summary>
    public static int CalculateBatchCount(int waypointCount)
    {
        if (waypointCount <= 0)
            return 0;
        return (waypointCount + MaxWaypointsPerBatch - 1) / MaxWaypointsPerBatch;
    }
}
