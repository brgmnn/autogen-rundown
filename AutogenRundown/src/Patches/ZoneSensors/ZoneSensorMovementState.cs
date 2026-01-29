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
    /// Global sensor indices for each entry in this batch.
    /// Allows sparse storage of only moving sensors.
    /// </summary>
    private fixed byte _globalIndices[MaxSensors];

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
    /// <param name="entryIndex">Entry index within this batch (0-31), packed consecutively</param>
    /// <param name="globalSensorIndex">Global sensor index within the group</param>
    /// <param name="waypointIndex">Current target waypoint index</param>
    /// <param name="forward">True if moving forward through waypoints</param>
    /// <param name="progress">Progress toward target (0.0-1.0)</param>
    public void SetMovementState(int entryIndex, int globalSensorIndex, int waypointIndex, bool forward, float progress)
    {
        if (entryIndex < 0 || entryIndex >= MaxSensors)
            return;

        _globalIndices[entryIndex] = (byte)globalSensorIndex;
        _waypointIndices[entryIndex] = (byte)Math.Clamp(waypointIndex, 0, 255);
        _progress[entryIndex] = (byte)(Math.Clamp(progress, 0f, 1f) * 255f);

        // Set or clear direction bit
        if (forward)
            DirectionMask |= (1u << entryIndex);
        else
            DirectionMask &= ~(1u << entryIndex);
    }

    /// <summary>
    /// Gets the movement state for an entry in this batch.
    /// </summary>
    /// <param name="entryIndex">Entry index within this batch (0-31)</param>
    /// <returns>Tuple of (waypointIndex, isForward, progress 0.0-1.0)</returns>
    public (int waypointIndex, bool forward, float progress) GetMovementState(int entryIndex)
    {
        if (entryIndex < 0 || entryIndex >= MaxSensors)
            return (0, true, 0f);

        int waypointIndex = _waypointIndices[entryIndex];
        bool forward = (DirectionMask & (1u << entryIndex)) != 0;
        float progress = _progress[entryIndex] / 255f;

        return (waypointIndex, forward, progress);
    }

    /// <summary>
    /// Checks if this state has valid movement data.
    /// </summary>
    public bool HasMovementData => SensorCount > 0;

    /// <summary>
    /// Gets the global sensor index for an entry in this batch.
    /// </summary>
    /// <param name="entryIndex">Entry index within this batch (0-31)</param>
    /// <returns>Global sensor index, or -1 if invalid</returns>
    public int GetGlobalSensorIndex(int entryIndex)
    {
        if (entryIndex < 0 || entryIndex >= MaxSensors)
            return -1;
        return _globalIndices[entryIndex];
    }

    /// <summary>
    /// Calculates the number of batches needed for a given total sensor count.
    /// </summary>
    public static int CalculateBatchCount(int totalSensors)
    {
        return (totalSensors + MaxSensors - 1) / MaxSensors;
    }
}
