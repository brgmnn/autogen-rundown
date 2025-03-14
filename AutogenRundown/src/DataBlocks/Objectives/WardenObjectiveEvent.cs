using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Enemies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/**
 *
 * TODO: Check through all these events and see if any others (Than Open/Unlock Doors) require the .Layer property
 * set. It seems to be used for selecting Main/Extreme/Overload
 */
namespace AutogenRundown.DataBlocks.Objectives;

public record ProgressEvent
{
    public double Progress { get; set; } = 0.0;

    public List<WardenObjectiveEvent> Events { get; set; } = new();
}

public record WardenObjectiveEventCountdown
{
    public string TitleText { get; set; } = "";

    public string TimerColor { get; set; } = "red";

    public List<ProgressEvent> EventsOnProgress { get; set; } = new();

    public List<WardenObjectiveEvent> EventsOnDone { get; set; } = new();
}

public record class WardenObjectiveEvent
{
    public WardenObjectiveEventType Type { get; set; } = WardenObjectiveEventType.None;

    public JObject Condition = new JObject
    {
        ["ConditionIndex"] = -1,
        ["IsTrue"] = false
    };

    public WardenObjectiveEventTrigger Trigger { get; set; } = WardenObjectiveEventTrigger.None;

    /// <summary>
    /// Persistent Id for chain puzzle
    /// </summary>
    public uint ChainPuzzle { get; set; } = 0;

    public bool UseStaticBioscanPoints { get; set; } = false;

    /// <summary>
    /// What Zone this event applies to. Typically for doors to be opened etc.
    ///
    /// NOTE: Make sure you also set the appropriate `Layer` property for the event.
    /// </summary>
    public int LocalIndex { get; set; } = 0;

    /// <summary>
    /// Which layer this event applies to. For opening / unlocking doors this is used to determin whether it's in
    /// Main / Extreme / Overload.
    /// </summary>
    public int Layer { get; set; } = 0;

    public double Delay { get; set; } = 0.0;

    public double Duration { get; set; } = 0.0;

    public bool ClearDimension { get; set; } = false;

    public string WardenIntel { get; set; } = "";

    public string CustomSubObjectiveHeader { get; set; } = "";

    public string CustomSubObjective { get; set; } = "";

    #region Sound settings
    [JsonProperty("SoundID")]
    public Sound SoundId { get; set; } = Sound.None;
    #endregion

    #region Fog settings
    public uint FogSetting { get; set; } = 0;

    public double FogTransitionDuration { get; set; } = 2.0;
    #endregion

    #region Enemy settings
    public GenericWave EnemyWaveData { get; set; } = new GenericWave();

    [JsonProperty("EnemyID")]
    public uint EnemyId { get; set; } = 0;

    public Vector3 Position { get; set; } = new Vector3();

    /// <summary>
    /// Vanilla: Enemy wave data count
    ///     - OR -
    /// EOSExt_SecuritySensor: Sensor group index
    /// </summary>
    public int Count { get; set; } = 0;

    /// <summary>
    /// Vanilla: Enemy wave data count
    ///     - OR -
    /// EOSExt_SecuritySensor: enable / disable sensor
    /// </summary>
    public bool Enabled { get; set; } = false;
    #endregion

    #region Terminal
    public int TerminalCommand = 0;
    public int TerminalCommandRule = 0;
    #endregion

    #region Not implemented yet
    public int DimensionIndex = 0;
    public uint SoundSubtitle = 0;
    public uint DialogueID = 0;
    #endregion

    #region ====== MODS: AWO AdvancedWardenObjective ======

    /// <summary>
    /// Used in conjunction with EventType.SetLightDataInZone (10016) to update the lights in a
    /// zone. Accepts the following structure as per the wiki
    ///
    /// ```json
    /// {
    ///   "Type": 10016,
    ///   "DimensionIndex": 0,
    ///   "Layer": 0,
    ///   "LocalIndex": 0,
    ///   "SetZoneLight": {
    ///     "Type": "SetZoneLightData", // Accepted enums: RevertToOriginal, SetZoneLightData
    ///     "LightDataID": 20,
    ///     "TransitionDuration": 1.0,
    ///     "Seed": 0
    ///   }
    /// }
    /// ```
    /// </summary>
    public SetZoneLight SetZoneLight { get; set; } = new();

    /// <summary>
    ///
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public EventLoop? EventLoop { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public WardenObjectiveEventCountdown Countdown { get; set; } = new();
    #endregion
}
