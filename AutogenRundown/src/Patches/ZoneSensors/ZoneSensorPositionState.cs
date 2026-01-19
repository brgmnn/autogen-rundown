using System.Runtime.InteropServices;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Network replication state for sensor positions within a group.
/// Used to sync spawn positions from host to clients and late joiners.
///
/// Positions are stored in a fixed-size buffer. The state includes:
/// - SensorCount: Number of sensors in this group
/// - Positions: Packed X, Y, Z coordinates for each sensor
///
/// Maximum 16 sensors per group (struct size constraint).
/// Each position uses 3 floats (12 bytes), total buffer = 192 bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ZoneSensorPositionState
{
    /// <summary>
    /// Maximum number of sensors supported per position state.
    /// </summary>
    public const int MaxSensors = 16;

    /// <summary>
    /// Number of sensors with positions stored in this state.
    /// </summary>
    public byte SensorCount;

    /// <summary>
    /// Packed waypoint counts for each sensor (2 bits per sensor).
    /// Supports 0-3 additional waypoints per sensor (beyond spawn position).
    /// Bits 0-1: sensor 0, Bits 2-3: sensor 1, etc.
    /// </summary>
    public uint WaypointCounts;

    /// <summary>
    /// Fixed buffer for sensor positions: [x0, y0, z0, x1, y1, z1, ...]
    /// Each sensor's spawn position is stored as 3 consecutive floats.
    /// </summary>
    private fixed float _positions[MaxSensors * 3]; // 48 floats = 192 bytes

    public ZoneSensorPositionState()
    {
        SensorCount = 0;
        WaypointCounts = 0;
    }

    /// <summary>
    /// Sets the position for a sensor at the given index.
    /// </summary>
    public void SetPosition(int index, Vector3 position)
    {
        if (index < 0 || index >= MaxSensors)
            return;

        int offset = index * 3;
        _positions[offset] = position.x;
        _positions[offset + 1] = position.y;
        _positions[offset + 2] = position.z;
    }

    /// <summary>
    /// Gets the position for a sensor at the given index.
    /// </summary>
    public Vector3 GetPosition(int index)
    {
        if (index < 0 || index >= MaxSensors)
            return Vector3.zero;

        int offset = index * 3;
        return new Vector3(
            _positions[offset],
            _positions[offset + 1],
            _positions[offset + 2]);
    }

    /// <summary>
    /// Sets the waypoint count for a sensor (0-3 additional waypoints).
    /// </summary>
    public void SetWaypointCount(int sensorIndex, int count)
    {
        if (sensorIndex < 0 || sensorIndex >= MaxSensors)
            return;

        // Clamp to 0-3 (2 bits)
        count = Math.Clamp(count, 0, 3);

        int shift = sensorIndex * 2;
        uint mask = ~(3u << shift);
        WaypointCounts = (WaypointCounts & mask) | ((uint)count << shift);
    }

    /// <summary>
    /// Gets the waypoint count for a sensor (0-3 additional waypoints).
    /// </summary>
    public int GetWaypointCount(int sensorIndex)
    {
        if (sensorIndex < 0 || sensorIndex >= MaxSensors)
            return 0;

        int shift = sensorIndex * 2;
        return (int)((WaypointCounts >> shift) & 3);
    }

    /// <summary>
    /// Checks if the state has valid position data.
    /// </summary>
    public bool HasPositions => SensorCount > 0;
}
