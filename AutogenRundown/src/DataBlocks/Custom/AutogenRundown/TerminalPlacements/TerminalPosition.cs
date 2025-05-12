namespace AutogenRundown.DataBlocks.Custom.AutogenRundown.TerminalPlacements;

public record TerminalPosition
{
    public string Layer { get; set; } = "MainLayer";

    public int LocalIndex { get; set; } = 0;

    public string Area { get; set; } = "A";

    public Vector3 Position { get; set; } = new();

    public Vector3 Rotation { get; set; } = new();
};
