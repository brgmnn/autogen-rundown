using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.WorldEvents;

public class WorldEventChainedPuzzle
{
    [JsonIgnore]
    public ChainedPuzzle Puzzle { get; set; } = ChainedPuzzle.None;

    [JsonProperty("ChainedPuzzle")]
    public uint ChainedPuzzleId => Puzzle.PersistentId;

    public string WorldEventObjectFilter { get; set; } = "";

    public List<WardenObjectiveEvent> EventsOnScanDone { get; set; } = new();
}
