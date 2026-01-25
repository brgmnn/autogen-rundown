using System.Runtime.InteropServices;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Network replication state for a sensor group.
/// Used with FloLib StateReplicator for multiplayer sync.
/// Supports up to 128 sensors per group using 4 × 32-bit fields.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ZoneSensorGroupState
{
    public bool Enabled;

    // 128-bit sensor mask split into 4 × 32-bit fields
    public uint SensorMask0;    // Sensors 0-31
    public uint SensorMask1;    // Sensors 32-63
    public uint SensorMask2;    // Sensors 64-95
    public uint SensorMask3;    // Sensors 96-127

    // 128-bit triggered mask split into 4 × 32-bit fields
    public uint TriggeredMask0; // Sensors 0-31
    public uint TriggeredMask1; // Sensors 32-63
    public uint TriggeredMask2; // Sensors 64-95
    public uint TriggeredMask3; // Sensors 96-127

    public ZoneSensorGroupState()
    {
        Enabled = true;
        SensorMask0 = SensorMask1 = SensorMask2 = SensorMask3 = uint.MaxValue;
        TriggeredMask0 = TriggeredMask1 = TriggeredMask2 = TriggeredMask3 = 0;
    }

    public ZoneSensorGroupState(bool enabled)
    {
        Enabled = enabled;
        var mask = enabled ? uint.MaxValue : 0u;
        SensorMask0 = SensorMask1 = SensorMask2 = SensorMask3 = mask;
        TriggeredMask0 = TriggeredMask1 = TriggeredMask2 = TriggeredMask3 = 0;
    }

    public static ZoneSensorGroupState CreateWithMasks(
        bool enabled,
        uint sensorMask0, uint sensorMask1, uint sensorMask2, uint sensorMask3,
        uint triggeredMask0, uint triggeredMask1, uint triggeredMask2, uint triggeredMask3)
    {
        return new ZoneSensorGroupState
        {
            Enabled = enabled,
            SensorMask0 = sensorMask0,
            SensorMask1 = sensorMask1,
            SensorMask2 = sensorMask2,
            SensorMask3 = sensorMask3,
            TriggeredMask0 = triggeredMask0,
            TriggeredMask1 = triggeredMask1,
            TriggeredMask2 = triggeredMask2,
            TriggeredMask3 = triggeredMask3
        };
    }

    public readonly bool IsSensorEnabled(int index)
    {
        if (index < 0 || index >= 128) return false;
        int field = index / 32;
        int bit = index % 32;
        return field switch
        {
            0 => (SensorMask0 & (1u << bit)) != 0,
            1 => (SensorMask1 & (1u << bit)) != 0,
            2 => (SensorMask2 & (1u << bit)) != 0,
            3 => (SensorMask3 & (1u << bit)) != 0,
            _ => false
        };
    }

    public readonly bool IsSensorTriggered(int index)
    {
        if (index < 0 || index >= 128) return false;
        int field = index / 32;
        int bit = index % 32;
        return field switch
        {
            0 => (TriggeredMask0 & (1u << bit)) != 0,
            1 => (TriggeredMask1 & (1u << bit)) != 0,
            2 => (TriggeredMask2 & (1u << bit)) != 0,
            3 => (TriggeredMask3 & (1u << bit)) != 0,
            _ => false
        };
    }

    public void SetSensorDisabled(int index)
    {
        if (index < 0 || index >= 128) return;
        int field = index / 32;
        int bit = index % 32;
        switch (field)
        {
            case 0: SensorMask0 &= ~(1u << bit); break;
            case 1: SensorMask1 &= ~(1u << bit); break;
            case 2: SensorMask2 &= ~(1u << bit); break;
            case 3: SensorMask3 &= ~(1u << bit); break;
        }
    }

    public void SetSensorTriggered(int index)
    {
        if (index < 0 || index >= 128) return;
        int field = index / 32;
        int bit = index % 32;
        switch (field)
        {
            case 0: TriggeredMask0 |= (1u << bit); break;
            case 1: TriggeredMask1 |= (1u << bit); break;
            case 2: TriggeredMask2 |= (1u << bit); break;
            case 3: TriggeredMask3 |= (1u << bit); break;
        }
    }

    public void SetAllEnabled()
    {
        SensorMask0 = SensorMask1 = SensorMask2 = SensorMask3 = uint.MaxValue;
    }

    public void SetAllDisabled()
    {
        SensorMask0 = SensorMask1 = SensorMask2 = SensorMask3 = 0;
    }

    public void ClearTriggered()
    {
        TriggeredMask0 = TriggeredMask1 = TriggeredMask2 = TriggeredMask3 = 0;
    }

    public void ApplyTriggeredMask()
    {
        SensorMask0 &= ~TriggeredMask0;
        SensorMask1 &= ~TriggeredMask1;
        SensorMask2 &= ~TriggeredMask2;
        SensorMask3 &= ~TriggeredMask3;
    }
}
