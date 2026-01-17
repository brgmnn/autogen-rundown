namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Network replication state for a sensor group.
/// Used with FloLib StateReplicator for multiplayer sync.
/// </summary>
public struct ZoneSensorGroupState
{
    public bool Enabled;
    public uint SensorMask;  // Each bit = sensor enabled (supports up to 32 sensors per group)

    public ZoneSensorGroupState()
    {
        Enabled = true;
        SensorMask = uint.MaxValue;  // All sensors enabled by default
    }

    public ZoneSensorGroupState(bool enabled)
    {
        Enabled = enabled;
        SensorMask = enabled ? uint.MaxValue : 0;
    }

    public ZoneSensorGroupState(bool enabled, uint sensorMask)
    {
        Enabled = enabled;
        SensorMask = sensorMask;
    }

    public ZoneSensorGroupState(ZoneSensorGroupState other)
    {
        Enabled = other.Enabled;
        SensorMask = other.SensorMask;
    }

    public bool IsSensorEnabled(int index) => (SensorMask & (1u << index)) != 0;
    public uint WithSensorDisabled(int index) => SensorMask & ~(1u << index);
}
