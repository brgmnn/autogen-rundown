using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

/// <summary>
/// Works analogously as the corresponding feature in EEC.
/// * Allows you to execute warden events when scout is alerted.
/// * Suppresses vanilla scout wave, to further customize scout waves, per zone, via SpawnEnemyWave events.
/// </summary>
public class EventsOnBossDeath : Definition
{
    /// <summary>
    /// Apply the events to hibernating bosses spawned in zone.
    ///
    /// Default: true
    /// </summary>
    public bool ApplyToHibernate { get; set; } = true;

    /// <summary>
    /// How many hibernate bosses would you like to apply the death events to.
    ///
    /// In most scenarios: Simply drop this field, or leave it to its default value,
    ///                    to make events simply applied to all hibernate bosses spawned in the zone,
    ///                    which should satisfy most level designs.
    ///
    /// This is an advanced option: Death events will be RANDOMLY applied to hibernate bosses,
    ///                             depending on their spawning order, which might lead to issues that are too long to write it down here.
    ///                             Take more care to this option, if you want to use it.
    ///
    /// Default value: 2147483647, (by which, means 'unlimited')
    /// </summary>
    public int ApplyToHibernateCount { get; set; } = 2147483647;

    /// <summary>
    /// Apply the events to wave bosses spawned in zone.
    /// This is an advanced option.
    ///
    /// Default: false
    /// </summary>
    public bool ApplyToWave { get; set; } = false;

    /// <summary>
    ///
    /// </summary>
    public int ApplyToWaveCount { get; set; } = 2147483647;

    /// <summary>
    /// Enemy persistent Ids to apply these to.
    /// </summary>
    [JsonProperty("BossIDs")]
    public List<uint> PersistentIds { get; set; } = new();

    /// <summary>
    /// Events to be executed
    /// </summary>
    [JsonProperty("EventsOnBossDeath")]
    public List<WardenObjectiveEvent> Events { get; set; } = new();
}
