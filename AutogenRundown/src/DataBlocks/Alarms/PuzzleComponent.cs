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

    public static readonly PuzzleComponent BulkheadMain      = new() { PuzzleType = PuzzleType.BulkheadMain };
    public static readonly PuzzleComponent BulkheadSecondary = new() { PuzzleType = PuzzleType.BulkheadSecondary };
    public static readonly PuzzleComponent BulkheadOverload  = new() { PuzzleType = PuzzleType.BulkheadOverload };

    // Unused?
    public static readonly PuzzleComponent ScanSmall = new()
    {
        PuzzleType = PuzzleType.Small
    };

    public static readonly PuzzleComponent ScanLarge = new()
    {
        PuzzleType = PuzzleType.Large
        // TODO: Add Duration measurement
    };

    public static readonly PuzzleComponent ClusterSmall = new()
    {
        PuzzleType = PuzzleType.ClusterSmall,
        Duration = 12
    };
    public static readonly PuzzleComponent ClusterLarge = new()
    {
        PuzzleType = PuzzleType.ClusterLarge,
        Duration = 15
    };
    public static readonly PuzzleComponent Cluster = new() { PuzzleType = PuzzleType.Cluster };

    /// <summary>
    /// Team scan orange component
    /// </summary>
    public static readonly PuzzleComponent AllBig = new()
    {
        PuzzleType = PuzzleType.AllBig,
        Duration = 8
    };

    public static readonly PuzzleComponent AllBig_GreenActive = new()
    {
        PuzzleType = PuzzleType.AllBig_GreenActive
    };

    /// <summary>
    /// Surge alarms orange team scan
    /// </summary>
    public static readonly PuzzleComponent AllLarge = new()
    {
        PuzzleType = PuzzleType.AllLarge,
        Duration = 15
    };

    // Unused right now
    public static readonly PuzzleComponent AllLarge_Slow = new()
    {
        PuzzleType = PuzzleType.AllLarge_Slow
    };

    public static readonly PuzzleComponent StealthBig_Cluster = new()
    {
        PuzzleType = PuzzleType.StealthBig_Cluster
    };

    public static readonly PuzzleComponent Sustained = new()
    {
        PuzzleType = PuzzleType.Sustained,
        Duration = 100
    };
    public static readonly PuzzleComponent SustainedSmall = new()
    {
        PuzzleType = PuzzleType.SustainedSmall,
        Duration = 12
    };
    public static readonly PuzzleComponent SustainedMedium = new()
    {
        PuzzleType = PuzzleType.SustainedMedium,
        Duration = 25
    };
    public static readonly PuzzleComponent SustainedHuge = new()
    {
        PuzzleType = PuzzleType.SustainedHuge,
        Duration = 120
    };
    public static readonly PuzzleComponent SustainedMegaHuge = new()
    {
        PuzzleType = PuzzleType.SustainedMegaHuge,
        Duration = 300
    };
    public static readonly PuzzleComponent SustainedBig_Cluster = new()
    {
        PuzzleType = PuzzleType.SustainedBig_Cluster,
        Duration = 36
    };

    public static readonly PuzzleComponent SustainedZone = new()
    {
        PuzzleType = PuzzleType.Sustained_Zone,
        Duration = 120
    };
}
