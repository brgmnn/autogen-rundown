using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms;

/// <summary>
/// Scan types. Scan sizes go from:
///
/// Small -> Big -> Large
/// </summary>
public enum PuzzleType : uint
{
    // Single red circles (blue when non-alarm)
    #region Individual scans
    /// <summary>
    /// Single large circle: red when alarmed, blue (stealth) otherwise.
    /// </summary>
    Large = 2,

    /// <summary>
    /// Single small circle: red when alarmed, blue (stealth) otherwise.
    /// Usually not used much.
    /// </summary>
    Small = 3,
    #endregion

    #region Cluster: Multiple individual circles
    /// <summary>
    /// 8 small clusters, blue when non-alarm orange otherwise.
    /// </summary>
    Cluster = 9,

    /// <summary>
    /// 4 small circles, work like PuzzleType.Small
    /// </summary>
    ClusterSmall = 4,

    /// <summary>
    /// 2 large PuzzleType.Large circles
    /// </summary>
    ClusterLarge = 5,
    #endregion

    #region Full team scans (orange)
    // Require All teammates (orange)
    AllBig = 6,
    AllBig_GreenActive = 29, // Same as 27, 28

    /// <summary>
    /// Large orange team scan, blue when scanning with no alarm.
    /// </summary>
    AllLarge = 8,
    AllBig_BlueActive = 25, // Like AllBig, but blue when scanning. Name: SecurityScan_Big_RequireAll_R6A1_Narrative

    AllLarge_Slow = 20, // Name: SecurityScan_D10_RequireAll
    #endregion

    // Stealth scans
    StealthBig_Cluster = 15,

    #region S-Class scans
    /**
     * S-Class scans
     */
    SustainedSmall = 14,

    /// <summary>
    /// This looks like the canonical sustained scan used in many levels.
    ///
    /// 100s duration with 4 players
    /// </summary>
    Sustained = 13,

    /// <summary>
    /// Solo: 100s duration
    /// 4 players: 25s duration
    ///
    /// Seems to be scaled based on players
    /// </summary>
    SustainedMedium = 32,
    SustainedHuge = 18,
    SustainedMegaHuge = 17,
    SustainedBig_Cluster = 16,
    #endregion

    // Checkpoints
    Checkpoint = 19,

    // Expedition Exit
    ExpeditionExit = 7,

    // Bulkhead
    BulkheadMain = 10,
    BulkheadSecondary = 11,
    BulkheadOverload = 12,
}

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