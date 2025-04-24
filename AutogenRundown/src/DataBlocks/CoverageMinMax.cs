using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public enum CoverageSize : int
{
    Nano = 25,
    Tiny = 35,
    Small = 50,
    Medium = 75,
    Large = 100,
    Huge = 125,
}

public class CoverageMinMax
{
    private static List<double> Sizes = new List<double>
    {
        (double)CoverageSize.Nano,
        (double)CoverageSize.Tiny,
        (double)CoverageSize.Small,
        (double)CoverageSize.Medium,
        (double)CoverageSize.Large,
        (double)CoverageSize.Huge
    };

    private System.Numerics.Vector2 vec = new() { X = 0.0f, Y = 0.0f };

    #region Input values
    /// <summary>
    /// Minimum size
    /// </summary>
    [JsonProperty("x")]
    public double Min { get => vec.X; set => vec.X = (float)value; }

    /// <summary>
    /// Maximum size
    /// </summary>
    [JsonProperty("y")]
    public double Max { get => vec.Y; set => vec.Y = (float)value; }

    #endregion

    #region Computed values

    [JsonProperty("normalized")]
    public JObject Normalized
    {
        get
        {
            var normal = System.Numerics.Vector2.Normalize(vec);

            return new JObject
            {
                ["x"] = normal.X,
                ["y"] = normal.Y,
                ["magnitude"] = 1.0,
                ["sqrMagnitude"] = 1.0
            };
        }
    }

    [JsonProperty("magnitude")]
    public double Magnitude { get => vec.Length(); }

    [JsonProperty("sqrMagnitude")]
    public double SqrMagnitude { get => vec.LengthSquared(); }

    #endregion

    public CoverageMinMax() { }

    public CoverageMinMax(double value)
    {
        Min = value;
        Max = value;
    }

    public static readonly CoverageMinMax Nano = new() { Min = (double)CoverageSize.Nano, Max = (double)CoverageSize.Nano };
    public static readonly CoverageMinMax Tiny = new() { Min = (double)CoverageSize.Tiny, Max = (double)CoverageSize.Tiny };
    public static readonly CoverageMinMax Small = new() { Min = (double)CoverageSize.Small, Max = (double)CoverageSize.Small };
    public static readonly CoverageMinMax Medium = new() { Min = (double)CoverageSize.Medium, Max = (double)CoverageSize.Medium };
    public static readonly CoverageMinMax Large = new() { Min = (double)CoverageSize.Large, Max = (double)CoverageSize.Large };
    public static readonly CoverageMinMax Huge = new() { Min = (double)CoverageSize.Huge, Max = (double)CoverageSize.Huge };

    /// <summary>
    /// Generates a randomly picked size
    /// </summary>
    /// <returns></returns>
    public static CoverageMinMax GenCoverage()
        => new CoverageMinMax { Min = Generator.Pick(Sizes), Max = Generator.Pick(Sizes) };


    #region Normal size generation
    /// <summary>
    /// Actual sizes are:
    ///     * 3  (Small, Tiny)
    ///     * 10 (Medium)
    ///     * 20 (Large)
    ///     * 32 (Huge)
    /// And presumably we need everything else to be a multiple of this.
    /// </summary>
    private static List<(double, (double, double))> NormalSizes = new()
    {
        // Small sizes
        (0.25, (10, 10)),
        (0.45, (10, 20)),

        // Medium
        (0.85, (20, 20)),
        (1.00, (20, 32)),
        (1.00, (32, 32)),
        (1.00, (32, 42)),

        // Large
        (1.00, (42, 42)),
        (0.55, (42, 64)),
        (0.45, (64, 64)),
    };

    /// <summary>
    /// Randomly selects two sizes from the NormalSizes list for Min and Max size. The Normal
    /// sizes list follows a somewhat normal distribution around the 25-30 size, which seems
    /// typical for the game.
    /// </summary>
    /// <returns></returns>
    public static CoverageMinMax GenNormalSize()
    {
        //var size = Generator.Pick(NormalSizes);
        var (min, max) = Generator.Select(NormalSizes);

        return new CoverageMinMax { Min = min, Max = max };
    }
    #endregion

    public static CoverageMinMax GenStartZoneSize()
    {
        var size = Generator.Pick(new List<double> { 25.0, 30.0, 35.0, 50.0 });

        return new CoverageMinMax { Min = size, Max = size + 20.0 };
    }

    public static CoverageMinMax GenSize(int zoneIndex)
        => zoneIndex == 0 ? GenStartZoneSize() : GenNormalSize();
}
