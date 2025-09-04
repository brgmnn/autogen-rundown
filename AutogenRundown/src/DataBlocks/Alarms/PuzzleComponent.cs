using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms;

public class PuzzleComponent
{
    public PuzzleType PuzzleType { get; set; } = 0;

    /// <summary>
    /// Primarily used by Reactor startup, we use this to tag how long it would take to
    /// complete this puzzle component. This is then used to calculate the total time
    /// for reactor verify waves where the code is on a terminal behind doors with scans.
    /// </summary>
    [JsonIgnore]
    public double Duration { get; set; } = 10;

    public static readonly PuzzleComponent BulkheadMain = new PuzzleComponent { PuzzleType = PuzzleType.BulkheadMain };
    public static readonly PuzzleComponent BulkheadSecondary = new PuzzleComponent { PuzzleType = PuzzleType.BulkheadSecondary };
    public static readonly PuzzleComponent BulkheadOverload = new PuzzleComponent { PuzzleType = PuzzleType.BulkheadOverload };

    public static readonly PuzzleComponent ScanSmall = new PuzzleComponent { PuzzleType = PuzzleType.Small };
    public static readonly PuzzleComponent ScanLarge = new PuzzleComponent { PuzzleType = PuzzleType.Large };

    public static readonly PuzzleComponent ClusterSmall = new PuzzleComponent
    {
        PuzzleType = PuzzleType.ClusterSmall,
        Duration = 12
    };
    public static readonly PuzzleComponent ClusterLarge = new PuzzleComponent
    {
        PuzzleType = PuzzleType.ClusterLarge,
        Duration = 15
    };
    public static readonly PuzzleComponent Cluster = new PuzzleComponent { PuzzleType = PuzzleType.Cluster };

    public static readonly PuzzleComponent AllBig = new PuzzleComponent { PuzzleType = PuzzleType.AllBig };
    public static readonly PuzzleComponent AllBig_GreenActive = new PuzzleComponent { PuzzleType = PuzzleType.AllBig_GreenActive };
    public static readonly PuzzleComponent AllLarge = new PuzzleComponent { PuzzleType = PuzzleType.AllLarge };
    public static readonly PuzzleComponent AllLarge_Slow = new PuzzleComponent { PuzzleType = PuzzleType.AllLarge_Slow };

    public static readonly PuzzleComponent StealthBig_Cluster = new PuzzleComponent { PuzzleType = PuzzleType.StealthBig_Cluster };

    public static readonly PuzzleComponent Sustained = new PuzzleComponent
    {
        PuzzleType = PuzzleType.Sustained,
        Duration = 100
    };
    public static readonly PuzzleComponent SustainedSmall = new PuzzleComponent
    {
        PuzzleType = PuzzleType.SustainedSmall,
        Duration = 12
    };
    public static readonly PuzzleComponent SustainedMedium = new PuzzleComponent
    {
        PuzzleType = PuzzleType.SustainedMedium,
        Duration = 25
    };
    public static readonly PuzzleComponent SustainedHuge = new PuzzleComponent
    {
        PuzzleType = PuzzleType.SustainedHuge,
        Duration = 120
    };
    public static readonly PuzzleComponent SustainedMegaHuge = new PuzzleComponent
    {
        PuzzleType = PuzzleType.SustainedMegaHuge,
        Duration = 300
    };
    public static readonly PuzzleComponent SustainedBig_Cluster = new PuzzleComponent
    {
        PuzzleType = PuzzleType.SustainedBig_Cluster,
        Duration = 36
    };
}
