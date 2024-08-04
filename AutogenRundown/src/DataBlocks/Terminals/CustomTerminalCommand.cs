using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Terminals;

public record CustomTerminalCommand
{
    public string Command { get; set; } = "";

    public string CommandDesc { get; set; } = "";

    public List<TerminalOutput> PostCommandOutputs { get; set; } = new();

    public List<WardenObjectiveEvent> CommandEvents { get; set; } = new();

    public int SpecialCommandRule { get; set; } = 0;
}
