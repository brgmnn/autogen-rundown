using System.Runtime.InteropServices;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Network replication state for sensor movement progress.
/// Used for periodic sync of moving sensor positions to keep clients in sync.
///
/// Each batch supports up to 32 sensors with:
/// - Current waypoint index (1 byte per sensor)
/// - Movement direction (1 bit per sensor, packed into uint)
/// - Progress toward target waypoint (1 byte per sensor, 0-255 = 0.0-1.0)
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ZoneSensorMovementState
{
    /// <summary>
    /// Maximum sensors per movement state batch.
    /// </summary>
    public const int MaxSensors = 32;

    /// <summary>
    /// Index of this batch (0-based).
    /// </summary>
    public byte BatchIndex;

    /// <summary>
    /// Number of sensors with movement data in this batch.
    /// </summary>
    public byte SensorCount;

    /// <summary>
    /// Padding for alignment.
    /// </summary>
    public ushort Padding;

    /// <summary>
    /// Direction mask: 1 bit per sensor (1 = moving forward, 0 = moving backward).
    /// </summary>
    public uint DirectionMask;

    /// <summary>
    /// Current waypoint target index for each sensor.
    /// </summary>
    private fixed byte _waypointIndices[MaxSensors];

    /// <summary>
    /// Progress toward target waypoint for each sensor (0-255 maps to 0.0-1.0).
    /// </summary>
    private fixed byte _progress[MaxSensors];

    public ZoneSensorMovementState()
    {
        BatchIndex = 0;
        SensorCount = 0;
        Padding = 0;
        DirectionMask = 0;
    }

    /// <summary>
    /// Sets the movement state for a sensor in this batch.
    /// </summary>
    /// <param name="index">Sensor index within this batch (0-31)</param>
    /// <param name="waypointIndex">Current target waypoint index</param>
    /// <param name="forward">True if moving forward through waypoints</param>
    /// <param name="progress">Progress toward target (0.0-1.0)</param>
    public void SetMovementState(int index, int waypointIndex, bool forward, float progress)
    {
        if (index < 0 || index >= MaxSensors)
            return;

        _waypointIndices[index] = (byte)Math.Clamp(waypointIndex, 0, 255);
        _progress[index] = (byte)(Math.Clamp(progress, 0f, 1f) * 255f);

        // Set or clear direction bit
        if (forward)
            DirectionMask |= (1u << index);
        else
            DirectionMask &= ~(1u << index);
    }

    /// <summary>
    /// Gets the movement state for a sensor in this batch.
    /// </summary>
    /// <param name="index">Sensor index within this batch (0-31)</param>
    /// <returns>Tuple of (waypointIndex, isForward, progress 0.0-1.0)</returns>
    public (int waypointIndex, bool forward, float progress) GetMovementState(int index)
    {
        if (index < 0 || index >= MaxSensors)
            return (0, true, 0f);

        int waypointIndex = _waypointIndices[index];
        bool forward = (DirectionMask & (1u << index)) != 0;
        float progress = _progress[index] / 255f;

        return (waypointIndex, forward, progress);
    }

    /// <summary>
    /// Checks if this state has valid movement data.
    /// </summary>
    public bool HasMovementData => SensorCount > 0;

    /// <summary>
    /// Calculates the global sensor index from a local index within this batch.
    /// </summary>
    public int GetGlobalSensorIndex(int localIndex)
    {
        return BatchIndex * MaxSensors + localIndex;
    }

    /// <summary>
    /// Calculates the number of batches needed for a given total sensor count.
    /// </summary>
    public static int CalculateBatchCount(int totalSensors)
    {
        return (totalSensors + MaxSensors - 1) / MaxSensors;
    }
}
