using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Custom.ZoneSensors;

/// <summary>
/// Defines zone-based security sensors that are placed automatically
/// within a zone using the game's navigation mesh for valid positions.
/// </summary>
public class ZoneSensorDefinition : Definition
{
    /// <summary>
    /// Unique identifier for this sensor definition.
    /// Used by events to target this specific sensor group.
    /// Auto-assigned via Generator.GetPersistentId() if not explicitly set in JSON.
    /// </summary>
    public int Id { get; set; } = (int)Generator.GetPersistentId();

    /// <summary>
    /// Groups of sensors to place in this zone.
    /// Each group can have different settings (radius, color, count).
    /// </summary>
    public List<ZoneSensorGroupDefinition> SensorGroups { get; set; } = new();

    /// <summary>
    /// Events to execute when any sensor in this definition is triggered.
    /// Common events include spawning enemies, playing sounds, or toggling sensors.
    /// </summary>
    public List<WardenObjectiveEvent> EventsOnTrigger { get; set; } = new();
}
