namespace AutogenRundown.DataBlocks;

public class ZonePlacementWeights
{
    public double Start { get; set; } = 0.0;

    public double Middle { get; set; } = 0.0;

    public double End { get; set; } = 0.0;

    public static ZonePlacementWeights GenRandom()
        => Generator.Pick(new List<ZonePlacementWeights>
        {
            EvenlyDistributed,
            AtStart,
            AtMiddle,
            AtEnd,
            NotAtStart,
            NotAtMiddle,
            NotAtEnd
        })!;

    public static readonly ZonePlacementWeights None
        = new() { Start = 0.0, Middle = 0.0, End = 0.0 };

    public static readonly ZonePlacementWeights EvenlyDistributed
        = new() { Start = 1000.0, Middle = 1000.0, End = 1000.0 };

    public static readonly ZonePlacementWeights AtStart
        = new() { Start = 1000.0, Middle = 0.0, End = 0.0 };

    public static readonly ZonePlacementWeights AtMiddle
        = new() { Start = 0.0, Middle = 1000.0, End = 0.0 };

    public static readonly ZonePlacementWeights AtEnd
        = new() { Start = 0.0, Middle = 0.0, End = 1000.0 };

    public static readonly ZonePlacementWeights NotAtStart
        = new() { Start = 0.0, Middle = 1000.0, End = 1000.0 };

    public static readonly ZonePlacementWeights NotAtMiddle
        = new() { Start = 1000.0, Middle = 0.0, End = 1000.0 };

    public static readonly ZonePlacementWeights NotAtEnd
        = new() { Start = 1000.0, Middle = 1000.0, End = 0.0 };
}
