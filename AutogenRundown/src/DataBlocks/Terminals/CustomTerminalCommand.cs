using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Terminals;

public record CustomTerminalCommand
{
    public string Command { get; set; } = "";

    [JsonIgnore]
    public Text CommandDesc { get; set; } = Text.None;

    [JsonIgnore]
    private uint _commandDescId;

    [JsonProperty("CommandDesc")]
    public uint CommandDescId
    {
        get => CommandDesc.PersistentId != 0 ? CommandDesc.PersistentId : _commandDescId;
        set => _commandDescId = value;
    }

    public List<TerminalOutput> PostCommandOutputs { get; set; } = new();

    public List<WardenObjectiveEvent> CommandEvents { get; set; } = new();

    public CommandRule SpecialCommandRule { get; set; } = CommandRule.Normal;
}
