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
    public int GroupIndex { get; private set; }
    public bool Enabled { get; private set; } = true;
    public List<GameObject> Sensors { get; } = new();
    public List<WardenObjectiveEvent> EventsOnTrigger { get; set; } = new();

    // TODO: Add FloLib StateReplicator for network sync when available
    // public StateReplicator<ZoneSensorGroupState> StateReplicator { get; private set; }

    public void Initialize(int index, List<WardenObjectiveEvent> events)
    {
        GroupIndex = index;
        EventsOnTrigger = events;
        Enabled = true;

        // TODO: Initialize FloLib StateReplicator for multiplayer sync
        // When FloLib is added as a dependency:
        //
        // uint replicatorId = EOSNetworking.AllotReplicatorID();
        // StateReplicator = StateReplicator<ZoneSensorGroupState>.Create(
        //     replicatorId,
        //     new ZoneSensorGroupState { Enabled = true },
        //     LifeTimeType.Level
        // );
        // StateReplicator.OnStateChanged += OnStateChanged;
    }

    public void AddSensor(GameObject sensor)
    {
        Sensors.Add(sensor);
    }

    /// <summary>
    /// Sets the enabled state of the sensor group.
    /// In multiplayer, this would sync to all clients via StateReplicator.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        UpdateVisualsUnsynced(enabled);

        // TODO: For multiplayer sync with FloLib:
        // if (SNet.IsMaster)
        //     StateReplicator?.SetState(new ZoneSensorGroupState { Enabled = enabled });
    }

    /// <summary>
    /// Callback for network state changes (when FloLib is available).
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
        foreach (var sensor in Sensors)
        {
            if (sensor != null)
                UnityEngine.Object.Destroy(sensor);
        }
        Sensors.Clear();
    }
}
