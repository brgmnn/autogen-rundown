namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Network replication state for a sensor group.
/// Used with FloLib StateReplicator for multiplayer sync.
/// </summary>
public struct ZoneSensorGroupState
{
    public bool Enabled;

    public ZoneSensorGroupState()
    {
        Enabled = true;
    }

    public ZoneSensorGroupState(bool enabled)
    {
        Enabled = enabled;
    }

    public ZoneSensorGroupState(ZoneSensorGroupState other)
    {
        Enabled = other.Enabled;
    }
}
