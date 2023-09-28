namespace MyFirstPlugin.DataBlocks
{
    internal class ZonePlacementData
    {
        public int DimensionIndex { get; set; } = 0;

        public int LocalIndex { get; set; } = 0;

        public ZonePlacementWeights Weights { get; set; } = new ZonePlacementWeights();
    }
}
