namespace MyFirstPlugin.DataBlocks
{
    enum WardenObjectiveWinCondition
    {
        GoToElevator = 0,
        GoToExitGeo = 1,
    }

    internal class WardenObjectiveLayerData
    {
        public UInt32 DataBlockId { get; set; }

        public WardenObjectiveWinCondition WinCondition { get; set; }
            = WardenObjectiveWinCondition.GoToElevator;

        public List<List<ZonePlacementData>> ZonePlacementDatas { get; set; }
            = new List<List<ZonePlacementData>>();
    }
}
