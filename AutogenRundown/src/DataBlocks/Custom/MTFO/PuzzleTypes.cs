using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AutogenRundown.DataBlocks.Custom.MTFO;

/// <summary>
/// MTFO custom puzzle types, read once from Custom/PuzzleTypes.json when MTFO loads. Each scan
/// clones the puzzle component prefab of BaseScan, overrides its scanner and graphics settings
/// and registers it as a new puzzle component type under PersistentID.
/// </summary>
public record PuzzleTypes
{
    public List<CustomBioScan> Scans { get; set; } = new();

    /// <summary>
    /// Must be present: MTFO iterates it whenever the file exists.
    /// </summary>
    public List<object> Clusters { get; set; } = new();

    public void Save()
    {
        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}", "Custom");

        Directory.CreateDirectory(dir);

        File.WriteAllText(
            Path.Combine(dir, "PuzzleTypes.json"),
            JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PlayerRequirement
{
    None,
    All,
    Solo
}

[JsonConverter(typeof(StringEnumConverter))]
public enum RevealMode
{
    ScaleByDistance,
    ConstantTime,
    Instant
}

/// <summary>
/// Mirrors the game's eChainedPuzzleGraphicsColorMode
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum ColorMode
{
    NotSet,
    Waiting,
    Active,
    TimedOut,
    Alarm_Waiting,
    Alarm_Active,
    Alarm_TimedOut
}

public record CustomBioScan
{
    public uint PersistentID { get; init; }

    /// <summary>
    /// Puzzle component type whose prefab is cloned. Must have a CP_Bioscan_Core.
    /// </summary>
    public uint BaseScan { get; init; }

    public PlayerRequirement PlayerRequirement { get; init; } = PlayerRequirement.All;

    public double ScanRadius { get; init; }

    /// <summary>
    /// Scan progress per second, indexed by the number of players scanning minus one.
    /// </summary>
    public List<double> PlayersInScanMulti { get; init; } = new();

    public double ReduceSpeed { get; init; }

    public bool ReduceWhenNoPlayer { get; init; }

    public double RevealTime { get; init; }

    public RevealMode RevealMode { get; init; } = RevealMode.ScaleByDistance;

    public BioScanGraphics BioScanGraphics { get; init; } = new();
}

public record BioScanGraphics
{
    public bool HideScanText { get; init; }

    public string ScanText { get; init; } = "";

    /// <summary>
    /// Radius of the scan circle graphic. Independent of the scanner's ScanRadius.
    /// </summary>
    public double Radius { get; init; }

    /// <summary>
    /// MTFO replaces the prefab's colours with this list, so every mode the scan can enter
    /// needs an entry.
    /// </summary>
    public List<ColorByMode> ColorModeColor { get; init; } = new();
}

public record ColorByMode
{
    public ColorMode Mode { get; init; }

    public double R { get; init; }

    public double G { get; init; }

    public double B { get; init; }

    public double A { get; init; } = 1.0;
}
