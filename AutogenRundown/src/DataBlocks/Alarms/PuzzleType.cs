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

    #region Zone Sustain class scans

    /// <summary>
    /// R7C1 Monster zone alarm scan
    /// </summary>
    Sustained_Zone = 37,

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
