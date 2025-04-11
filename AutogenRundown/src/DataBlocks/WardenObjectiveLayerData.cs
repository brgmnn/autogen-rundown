namespace AutogenRundown.DataBlocks;

public enum WardenObjectiveWinCondition
{
    GoToElevator = 0,
    GoToExitGeo = 1,
}

public class WardenObjectiveLayerData
{
    public uint DataBlockId { get; set; }

    public WardenObjectiveWinCondition WinCondition { get; set; }
        = WardenObjectiveWinCondition.GoToElevator;

    public List<List<ZonePlacementData>> ZonePlacementDatas { get; set; } = new();
}
