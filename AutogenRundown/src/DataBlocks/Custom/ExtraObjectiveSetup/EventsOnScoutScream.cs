using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

/// <summary>
/// Works analogously as the corresponding feature in EEC.
/// * Allows you to execute warden events when scout is alerted.
/// * Suppresses vanilla scout wave, to further customize scout waves, per zone, via SpawnEnemyWave events.
/// </summary>
public class EventsOnScoutScream : Definition
{
    /// <summary>
    /// If set to `true`, won't spawn expedition scout waves defined in:
    ///     'RundownDB' -> 'ScoutWaveSettings' + 'ScoutWavePopulation'
    ///
    /// Default value: `false`.
    ///
    /// I usually set it to `true` and instead use warden events to spawn scout
    /// waves, which grants me more flexibility.
    /// </summary>
    public bool SuppressVanillaScoutWave { get; set; } = false;

    [JsonProperty("EventsOnScoutScream")]
    public List<WardenObjectiveEvent> Events { get; set; } = new();
}
