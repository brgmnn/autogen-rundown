namespace AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;

public record InfectPlayer
{
    public List<int> PlayerFilter { get; set; } = new() { 0, 1, 2, 3 };

    public bool FullTeamOverflow { get; set; } = true;

    public double InfectionAmount { get; set; } = 0.0;

    public bool InfectOverTime { get; set; } = false;

    public double Interval { get; set; } = 1.0;

    public bool UseZone { get; set; } = false;
}
