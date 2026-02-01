using System.Runtime.InteropServices;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Network replication state for sensor positions within a group.
/// Used to sync spawn positions from host to clients and late joiners.
///
/// Positions are stored in a fixed-size buffer. The state includes:
/// - SensorCount: Number of sensors in this batch
/// - BatchIndex: Which batch this is (0, 1, 2, ...)
/// - TotalBatches: Total number of batches for this group
/// - Positions: Packed X, Y, Z coordinates for each sensor
///
/// Maximum 16 sensors per batch (struct size constraint of 256 bytes).
/// Each position uses 3 floats (12 bytes), total buffer = 192 bytes.
/// Multiple batches allow groups with more than 16 sensors.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ZoneSensorPositionState
{
    /// <summary>
    /// Maximum number of sensors supported per position state batch.
    /// </summary>
    public const int MaxSensorsPerBatch = 16;

    /// <summary>
    /// Number of sensors with positions stored in this batch.
    /// </summary>
    public byte SensorCount;

    /// <summary>
    /// Index of this batch (0-based). Used to calculate global sensor indices.
    /// </summary>
    public byte BatchIndex;

    /// <summary>
    /// Total number of batches for this sensor group.
    /// Clients wait until all batches are received before spawning.
    /// </summary>
    public byte TotalBatches;

    /// <summary>
    /// Packed waypoint counts for each sensor (2 bits per sensor).
    /// Supports 0-3 additional waypoints per sensor (beyond spawn position).
    /// Bits 0-1: sensor 0, Bits 2-3: sensor 1, etc.
    /// Note: This is informational only - actual waypoints are synced via WaypointReplicators.
    /// </summary>
    public uint WaypointCounts;

    /// <summary>
    /// Fixed buffer for sensor positions: [x0, y0, z0, x1, y1, z1, ...]
    /// Each sensor's spawn position is stored as 3 consecutive floats.
    /// </summary>
    private fixed float _positions[MaxSensorsPerBatch * 3]; // 48 floats = 192 bytes

    public ZoneSensorPositionState()
    {
        SensorCount = 0;
        BatchIndex = 0;
        TotalBatches = 1;
        WaypointCounts = 0;
    }

    /// <summary>
    /// Sets the position for a sensor at the given index (within this batch).
    /// </summary>
    public void SetPosition(int index, Vector3 position)
    {
        if (index < 0 || index >= MaxSensorsPerBatch)
            return;

        var offset = index * 3;
        _positions[offset] = position.x;
        _positions[offset + 1] = position.y;
        _positions[offset + 2] = position.z;
    }

    /// <summary>
    /// Gets the position for a sensor at the given index (within this batch).
    /// </summary>
    public Vector3 GetPosition(int index)
    {
        if (index < 0 || index >= MaxSensorsPerBatch)
            return Vector3.zero;

        var offset = index * 3;
        return new Vector3(
            _positions[offset],
            _positions[offset + 1],
            _positions[offset + 2]);
    }

    /// <summary>
    /// Sets the waypoint count for a sensor (0-3 additional waypoints).
    /// Index is within this batch.
    /// </summary>
    public void SetWaypointCount(int sensorIndex, int count)
    {
        if (sensorIndex < 0 || sensorIndex >= MaxSensorsPerBatch)
            return;

        // Clamp to 0-3 (2 bits)
        count = Math.Clamp(count, 0, 3);

        var shift = sensorIndex * 2;
        var mask = ~(3u << shift);
        WaypointCounts = (WaypointCounts & mask) | ((uint)count << shift);
    }

    /// <summary>
    /// Gets the waypoint count for a sensor (0-3 additional waypoints).
    /// Index is within this batch.
    /// </summary>
    public int GetWaypointCount(int sensorIndex)
    {
        if (sensorIndex < 0 || sensorIndex >= MaxSensorsPerBatch)
            return 0;

        var shift = sensorIndex * 2;
        return (int)((WaypointCounts >> shift) & 3);
    }

    /// <summary>
    /// Checks if the state has valid position data.
    /// </summary>
    public bool HasPositions => SensorCount > 0;

    /// <summary>
    /// Calculates the global sensor index from a local index within this batch.
    /// </summary>
    public int GetGlobalSensorIndex(int localIndex)
    {
        return BatchIndex * MaxSensorsPerBatch + localIndex;
    }

    /// <summary>
    /// Calculates the number of batches needed for a given total sensor count.
    /// </summary>
    public static int CalculateBatchCount(int totalSensors)
    {
        return (totalSensors + MaxSensorsPerBatch - 1) / MaxSensorsPerBatch;
    }
}
