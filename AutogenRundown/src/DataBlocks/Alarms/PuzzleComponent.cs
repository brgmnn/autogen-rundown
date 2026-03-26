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

    /// <summary>
    /// Single small circle: red when alarmed, blue (stealth) otherwise. Usually not used much.
    ///
    /// Unused?
    /// </summary>
    public static readonly PuzzleComponent ScanSmall = new()
    {
        PuzzleType = PuzzleType.Small
    };

    /// <summary>
    /// Single large circle: red when alarmed, blue (stealth) otherwise.
    /// </summary>
    public static readonly PuzzleComponent ScanLarge = new()
    {
        PuzzleType = PuzzleType.Large
        // TODO: Add Duration measurement
    };

    /// <summary>
    /// 4 small red circles
    /// </summary>
    public static readonly PuzzleComponent ClusterSmall = new()
    {
        PuzzleType = PuzzleType.ClusterSmall,
        Duration = 12
    };

    /// <summary>
    /// 2 large PuzzleType.Large circles
    /// </summary>
    public static readonly PuzzleComponent ClusterLarge = new()
    {
        PuzzleType = PuzzleType.ClusterLarge,
        Duration = 15
    };

    /// <summary>
    /// 8 small clusters, blue when non-alarm orange otherwise.
    /// </summary>
    public static readonly PuzzleComponent Cluster = new()
    {
        PuzzleType = PuzzleType.Cluster,
        Duration = 15
    };

    /// <summary>
    /// 25 small clusters, blue when non-alarm orange otherwise.
    /// </summary>
    public static readonly PuzzleComponent ClusterMega = new()
    {
        PuzzleType = PuzzleType.ClusterMega,
        Duration = 40
    };

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
        PuzzleType = PuzzleType.AllBig_GreenActive,
        Duration = 8
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
        Duration = 300 // Really 240? 4 mins
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

    #region Travel components

    #region Solo travel

    /// <summary>
    /// ~50 seconds duration
    /// </summary>
    public static readonly PuzzleComponent TravelSolo_Short = new()
    {
        PuzzleType = PuzzleType.TravelSolo_Short,
        Duration = 50,
    };

    /// <summary>
    /// ~90 seconds duration
    /// </summary>
    public static readonly PuzzleComponent TravelSolo_Medium = new()
    {
        PuzzleType = PuzzleType.TravelSolo_Medium,
        Duration = 90,
    };

    /// <summary>
    /// ~175 seconds duration
    /// </summary>
    public static readonly PuzzleComponent TravelSolo_Long = new()
    {
        PuzzleType = PuzzleType.TravelSolo_Long,
        Duration = 175,
    };

    #endregion

    #region Short duration

    /// <summary>
    /// ~84 seconds duration
    /// Orange color
    /// </summary>
    public static readonly PuzzleComponent TravelTeam_Short = new()
    {
        PuzzleType = PuzzleType.TravelTeam_Short,
        Duration = 84,
    };

    /// <summary>
    /// ~100 seconds duration
    /// Green color when active
    /// </summary>
    public static readonly PuzzleComponent TravelTeam_Medium = new()
    {
        PuzzleType = PuzzleType.TravelTeam_Medium,
        Duration = 100,
    };

    /// <summary>
    /// ~100 seconds duration
    /// Green color when active, slightly slower than others
    /// </summary>
    public static readonly PuzzleComponent TravelTeam_MediumGreen = new()
    {
        PuzzleType = PuzzleType.TravelTeam_MediumGreen,
        Duration = 100,
    };

    #endregion

    #region Long duration

    /// <summary>
    /// ~200 seconds duration
    /// Orange color
    /// </summary>
    public static readonly PuzzleComponent TravelTeam_Long = new()
    {
        PuzzleType = PuzzleType.TravelTeam_Long,
        Duration = 200,
    };

    /// <summary>
    /// ~200 seconds duration
    /// Exactly like TravelTeam_Long=22 but green when active
    /// </summary>
    public static readonly PuzzleComponent TravelTeam_LongGreen = new()
    {
        PuzzleType = PuzzleType.TravelTeam_LongGreen,
        Duration = 200,
    };

    #endregion

    /// <summary>
    /// Sustained travel scan (moving sustained). ~120s duration.
    /// </summary>
    public static readonly PuzzleComponent SustainedTravel = new()
    {
        PuzzleType = PuzzleType.SustainedTravel,
        Duration = 120
    };

    /// <summary>
    /// Travel team scan (require all, moving). ~45s duration.
    /// </summary>
    public static readonly PuzzleComponent TravelTeam = new()
    {
        PuzzleType = PuzzleType.TravelTeam,
        Duration = 45
    };

    /// <summary>
    /// Travel solo-capable big scan (moving). ~30s duration.
    /// </summary>
    public static readonly PuzzleComponent TravelBig = new()
    {
        PuzzleType = PuzzleType.TravelBig,
        Duration = 30
    };

    #endregion

    public override bool Equals(object? obj)
        => obj is PuzzleComponent other && PuzzleType == other.PuzzleType;

    public override int GetHashCode()
        => PuzzleType.GetHashCode();
}
