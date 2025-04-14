namespace AutogenRundown.DataBlocks.Zones;

public class FunctionPlacementData
{
    public ZonePlacementWeights PlacementWeights { get; set; } = ZonePlacementWeights.EvenlyDistributed;

    public uint AreaSeedOffset { get; set; } = 0;

    public uint MarkerSeedOffset { get; set; } = 0;
}
