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

    /// <summary>
    /// Optional. Asset path of the prefab to use as the FIRST tile of this
    /// zone. When non-null, Patch_ForceMinAreaCount intercepts the first
    /// GetGeomorphTile call for this zone and substitutes this prefab.
    /// Subsequent tiles come from the normal SubComplex pool.
    /// </summary>
    public string? Geomorph { get; set; } = null;
}
