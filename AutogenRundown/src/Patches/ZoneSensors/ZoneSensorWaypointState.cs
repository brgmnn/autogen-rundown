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
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ZoneSensorWaypointState
{
    /// <summary>
    /// Maximum waypoints per sensor. NavMesh paths typically have 4-8 corners,
    /// but complex routes can have more. Limited to 20 to fit within
    /// StateReplicator's 256-byte payload limit (8 + 20*3*4 = 248 bytes).
    /// </summary>
    public const int MaxWaypointsPerSensor = 20;

    /// <summary>
    /// Global sensor index within the group (0-127).
    /// </summary>
    public byte SensorIndex;

    /// <summary>
    /// Number of waypoints stored for this sensor.
    /// </summary>
    public byte WaypointCount;

    /// <summary>
    /// Padding for alignment.
    /// </summary>
    public ushort Padding;

    /// <summary>
    /// Movement speed for this sensor.
    /// </summary>
    public float Speed;

    /// <summary>
    /// Fixed buffer for waypoint positions: [x0, y0, z0, x1, y1, z1, ...]
    /// 20 waypoints * 3 floats = 60 floats = 240 bytes
    /// </summary>
    private fixed float _waypoints[MaxWaypointsPerSensor * 3];

    public ZoneSensorWaypointState()
    {
        SensorIndex = 0;
        WaypointCount = 0;
        Padding = 0;
        Speed = 0f;
    }

    /// <summary>
    /// Sets the waypoint position at the given index.
    /// </summary>
    public void SetWaypoint(int index, Vector3 pos)
    {
        if (index < 0 || index >= MaxWaypointsPerSensor)
            return;

        int offset = index * 3;
        _waypoints[offset] = pos.x;
        _waypoints[offset + 1] = pos.y;
        _waypoints[offset + 2] = pos.z;
    }

    /// <summary>
    /// Gets the waypoint position at the given index.
    /// </summary>
    public Vector3 GetWaypoint(int index)
    {
        if (index < 0 || index >= MaxWaypointsPerSensor)
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
    /// Creates a waypoint state from an array of positions.
    /// </summary>
    public static ZoneSensorWaypointState FromArray(int sensorIndex, Vector3[] waypoints, float speed)
    {
        var state = new ZoneSensorWaypointState
        {
            SensorIndex = (byte)sensorIndex,
            WaypointCount = (byte)Math.Min(waypoints.Length, MaxWaypointsPerSensor),
            Speed = speed
        };

        for (int i = 0; i < state.WaypointCount; i++)
        {
            state.SetWaypoint(i, waypoints[i]);
        }

        return state;
    }
}
