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

    public void Initialize(int index, List<WardenObjectiveEvent> events)
    {
        GroupIndex = index;
        EventsOnTrigger = events;
        Enabled = true;

        // Create network replicator with unique ID
        uint replicatorId = REPLICATOR_BASE_ID + (uint)index;
        Replicator = StateReplicator<ZoneSensorGroupState>.Create(
            replicatorId,
            new ZoneSensorGroupState(true),
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
        // Update visuals immediately for responsive feel
        UpdateVisualsUnsynced(enabled);

        // Sync to clients (only master can broadcast)
        if (Replicator != null && Replicator.IsValid)
        {
            Replicator.SetState(new ZoneSensorGroupState(enabled));
        }
    }

    /// <summary>
    /// Callback for network state changes.
    /// </summary>
    private void OnStateChanged(ZoneSensorGroupState oldState, ZoneSensorGroupState newState, bool isRecall)
    {
        UpdateVisualsUnsynced(newState.Enabled);
    }

    /// <summary>
    /// Updates the local visual state without network sync.
    /// </summary>
    private void UpdateVisualsUnsynced(bool enabled)
    {
        Enabled = enabled;

        foreach (var sensor in Sensors)
        {
            if (sensor != null)
                sensor.SetActive(enabled);
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
