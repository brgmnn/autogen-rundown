namespace MyFirstPlugin.DataBlocks
{
    internal class BulkheadDoorPlacementData
    {
        public int LocalZoneIndex { get; set; } = 0;

        public ZonePlacementWeights PlacementWeights { get; set; } = new ZonePlacementWeights();

        public int AreaSeedOffset { get; set; } = 0;

        public int MarkerSeedOffset { get; set; } = 0;
    }
}
