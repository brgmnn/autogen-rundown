using AmorLib.Networking.StateReplicators;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Objectives;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Represents a group of sensors that share the same trigger events.
/// Manages the enabled/disabled state and visual updates for all sensors in the group.
/// </summary>
public class ZoneSensorGroup
{
    // Base ID for zone sensor replicators (avoid collisions with other mods)
    private const uint REPLICATOR_BASE_ID = 0x5A534E00; // "ZSN" prefix + group index

    public int GroupIndex { get; private set; }
    public bool Enabled { get; private set; } = true;
    public List<GameObject> Sensors { get; } = new();
    public List<WardenObjectiveEvent> EventsOnTrigger { get; set; } = new();

    public StateReplicator<ZoneSensorGroupState>? Replicator { get; private set; }

    private bool triggerEach;
    private uint currentSensorMask = uint.MaxValue;

    public void Initialize(int index, List<WardenObjectiveEvent> events, bool triggerEach)
    {
        GroupIndex = index;
        EventsOnTrigger = events;
        Enabled = true;
        this.triggerEach = triggerEach;
        currentSensorMask = uint.MaxValue;

        // Create network replicator with unique ID
        uint replicatorId = REPLICATOR_BASE_ID + (uint)index;
        Replicator = StateReplicator<ZoneSensorGroupState>.Create(
            replicatorId,
            new ZoneSensorGroupState(true, uint.MaxValue),
            LifeTimeType.Session  // Auto-cleanup on level end
        );

        if (Replicator != null)
        {
            Replicator.OnStateChanged += OnStateChanged;
        }
    }

    public void AddSensor(GameObject sensor)
    {
        Sensors.Add(sensor);
    }

    /// <summary>
    /// Sets the enabled state of the sensor group.
    /// Syncs to all clients via StateReplicator.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        // Reset mask when enabling (all sensors become active again)
        if (enabled)
            currentSensorMask = uint.MaxValue;

        // Update visuals immediately for responsive feel
        UpdateVisualsUnsynced(enabled, currentSensorMask);

        // Sync to clients (only master can broadcast)
        if (Replicator != null && Replicator.IsValid)
        {
            Replicator.SetState(new ZoneSensorGroupState(enabled, currentSensorMask));
        }
    }

    /// <summary>
    /// Disables a single sensor and syncs to all clients.
    /// Used when TriggerEach mode is enabled.
    /// Note: Maximum of 32 sensors per group due to bitmask limitation.
    /// Sensors beyond index 31 will be silently ignored.
    /// </summary>
    public void DisableSensor(int sensorIndex)
    {
        if (sensorIndex < 0 || sensorIndex >= Sensors.Count || sensorIndex >= 32)
            return;

        currentSensorMask &= ~(1u << sensorIndex);

        // Update local visual immediately
        var sensor = Sensors[sensorIndex];
        sensor?.SetActive(false);

        // Sync to all clients
        if (Replicator != null && Replicator.IsValid)
        {
            Replicator.SetState(new ZoneSensorGroupState(Enabled, currentSensorMask));
        }
    }

    /// <summary>
    /// Callback for network state changes.
    /// </summary>
    private void OnStateChanged(ZoneSensorGroupState oldState, ZoneSensorGroupState newState, bool isRecall)
    {
        currentSensorMask = newState.SensorMask;
        UpdateVisualsUnsynced(newState.Enabled, newState.SensorMask);
    }

    /// <summary>
    /// Updates the local visual state without network sync.
    /// </summary>
    private void UpdateVisualsUnsynced(bool enabled, uint sensorMask)
    {
        Enabled = enabled;

        for (int i = 0; i < Sensors.Count; i++)
        {
            var sensor = Sensors[i];
            if (sensor != null)
            {
                // Sensor is visible if group enabled AND individual sensor enabled
                bool sensorEnabled = enabled && ((sensorMask & (1u << i)) != 0);
                sensor.SetActive(sensorEnabled);
            }
        }
    }

    /// <summary>
    /// Cleans up all sensor objects.
    /// </summary>
    public void Cleanup()
    {
        // Unload replicator (though Session lifetime auto-cleans)
        Replicator?.Unload();
        Replicator = null;

        foreach (var sensor in Sensors)
        {
            if (sensor != null)
                UnityEngine.Object.Destroy(sensor);
        }
        Sensors.Clear();
    }
}
