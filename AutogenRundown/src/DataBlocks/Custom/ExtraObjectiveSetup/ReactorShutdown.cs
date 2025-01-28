using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class ReactorShutdown : Definition
{
    public bool LightsOnFromBeginning { get; set; } = true;

    #region Puzzle settings
    /// <summary>
    /// Puzzle that appears after entering REACTOR_SHUTDOWN. Usually this should just be left
    /// as the team scan.
    ///
    /// Can be set to 0, by which reactor shutdown will be initiated upon command being input.
    /// </summary>
    [JsonIgnore]
    public ChainedPuzzle PuzzleToActivate { get; set; } = ChainedPuzzle.TeamScan;

    public uint ChainedPuzzleToActive
    {
        get => PuzzleToActivate.PersistentId;
        private set { }
    }

    /// <summary>
    /// In the vanilla objective this is the `ChainedPuzzleMidObjective` property. This is the
    /// scan which appears after putting in the verification code and usually is the hard scan
    /// that happens while an alarm is on going.
    /// </summary>
    [JsonIgnore]
    public ChainedPuzzle PuzzleOnVerification { get; set; } = ChainedPuzzle.TeamScan;

    public uint ChainedPuzzleOnVerification
    {
        get => PuzzleOnVerification.PersistentId;
        private set { }
    }
    #endregion

    public List<WardenObjectiveEvent> EventsOnActive { get; set; } = new();

    public List<WardenObjectiveEvent> EventsOnShutdownPuzzleStarts { get; set; } = new();

    public List<WardenObjectiveEvent> EventsOnComplete { get; set; } = new();

    public bool PutVerificationCodeOnTerminal { get; set; } = false;

    public int InstanceIndex { get; set; } = 0;

    public JObject VerificationCodeTerminal = new()
    {
        ["DimensionIndex"] = "Reality",
        ["LayerType"] = "MainLayer",
        ["LocalIndex"] = 0,
        ["InstanceIndex"] = 0
    };

    [JsonIgnore]
    public PasswordData Password { get; set; } = new();

    public JObject ReactorTerminal
    {
        get => new()
        {
            ["LocalLogFiles"] = new JArray(),
            ["UniqueCommands"] = new JArray(),
            ["PasswordData"] = JObject.FromObject(Password)
        };
    }
}
