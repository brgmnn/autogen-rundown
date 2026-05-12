using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown.AreaCounts;

public record AreaCountZone
{
    public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

    public int Layer => Bulkhead switch
    {
        Bulkhead.Main => 0,
        Bulkhead.Extreme => 1,
        Bulkhead.Overload => 2,
        _ => 0
    };

    public int ZoneNumber { get; set; } = 0;

    public DimensionIndex Dimension { get; set; } = DimensionIndex.Reality;

    public int Count { get; set; } = 2;
}
