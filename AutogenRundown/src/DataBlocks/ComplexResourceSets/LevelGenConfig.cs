namespace AutogenRundown.DataBlocks.ComplexResourceSets;

public record LevelGenConfig
{
    public int GridSize { get; set; } = 40;

    public double CellDimension { get; set; } = 64.0;

    public double AltitudeOffset { get; set; } = 6.0;

    public int TransitionDirection { get; set; } = 0;

    public int LevelProgression { get; set; } = 0;
}
