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
    /// 25 small clusters, blue when non-alarm orange otherwise.
    /// </summary>
    ClusterMega = 61,

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

    #region Travel (Moving) scans

    // Travel scan inventory:
    //
    // * 31 = 84s  - 1:24 duration - Regular with green activation when standing in it
    // * 24 = 100s - 1:40 duration - Regular travel, green, seems a little slower
    // * 38 = 100s - 1:40 duration - Double sized big green team travel scans
    // * 43 = 100s - 1:42 duration - Team green activation when standing, shorter version of 42
    //
    // * 22 = 202s - 3:22 duration - Regular looking travel scan
    // * 21 = 203s - 3:23 duration - Regular travel scan, exactly like 22 but green when active
    //
    // * 60 = 53s  - 0:53 duration - Solo regular green scan when standing in it (solo version of 31)
    // * 52 = 87s  - 1:27 duration - Small green solo travel scan
    // * 42 = 175s - 2:55 duration - Solo pure orange scan version of regular travel scan (solo version of 22)

    #region Solo travel
    /// <summary>
    /// ~50 seconds duration
    /// </summary>
    TravelSolo_Short = 60,

    /// <summary>
    /// ~90 seconds duration
    /// </summary>
    TravelSolo_Medium = 52,

    /// <summary>
    /// ~175 seconds duration
    /// </summary>
    TravelSolo_Long = 42,
    #endregion

    #region Short duration
    /// <summary>
    /// ~84 seconds duration
    /// Orange color
    /// </summary>
    TravelTeam_Short = 31,

    /// <summary>
    /// ~100 seconds duration
    /// Green color when active
    /// </summary>
    TravelTeam_Medium = 43,

    /// <summary>
    /// ~100 seconds duration
    /// Green color when active, slightly slower than others
    /// </summary>
    TravelTeam_MediumGreen = 24,
    #endregion

    #region Long duration
    /// <summary>
    /// ~200 seconds duration
    /// Orange color
    /// </summary>
    TravelTeam_Long = 22,

    /// <summary>
    /// ~200 seconds duration
    /// Exactly like TravelTeam_Long=22 but green when active
    /// </summary>
    TravelTeam_LongGreen = 21,
    #endregion

    #region Sustained travel
    /// <summary>
    /// Sustained scan with movement. Uses sustained prefab + runtime CP_BasicMovable injection.
    /// </summary>
    SustainedTravel = 100,
    #endregion

    /// <summary>
    /// Moving team scan (require all). Scan circle moves along a walking path.
    /// </summary>
    TravelTeam = 22,

    /// <summary>
    /// Moving solo-capable big scan. Scan circle moves along a walking path.
    /// </summary>
    TravelBig = 42,

    /// <summary>
    /// Moving solo-capable big scan variant A (slow). Scan circle moves along a walking path.
    /// </summary>
    TravelBig_A = 52,
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
