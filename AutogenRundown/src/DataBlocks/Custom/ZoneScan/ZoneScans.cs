using AutogenRundown.DataBlocks.Alarms;
using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ZoneScan;

/// <summary>
/// A scan component that the ZoneScan plugin (randomguy0753-ZoneScan) turns into an area or
/// zone scan.
/// </summary>
public record ZoneScanType
{
    public PuzzleComponent Component { get; init; } = new();

    /// <summary>
    /// Vanilla puzzle component whose prefab is copied (sounds, HUD, sustained behaviour).
    /// </summary>
    public PuzzleType BaseScan { get; init; }

    /// <summary>
    /// True: progress drains while the team is not all inside.
    /// </summary>
    public bool Sustained { get; init; }

    /// <summary>
    /// True: the whole zone the scan is in counts. False: only the area (room) it is in.
    /// </summary>
    public bool UseZone { get; init; }

    /// <summary>
    /// True: keep the base scan's colours. False: use the zone scan colours.
    /// </summary>
    public bool BaseColors { get; init; }

    public string ScanText { get; init; } = "";
}

/// <summary>
/// Area and zone scans, using the ZoneScan plugin.
///
/// ZoneScan replaces the radius check of every bioscan in the chained puzzles listed in
/// Custom/ZoneScanConfig/ZoneScanSettings.jsonc (read on managers setup, after we generate) with
/// "players in the scan's area" or "players in the scan's zone", and outlines that area on its
/// doors and open dividers. The vanilla scan circle stays, so the components are our own puzzle
/// component types with a zero radius circle, registered by Patch_ZoneScanPuzzleTypes.
///
/// ZoneScan works per chained puzzle, not per component: every scan of a listed chained puzzle
/// becomes an area/zone scan. Chained puzzles must therefore use only components from
/// <see cref="Types"/>, all with the same <see cref="ZoneScanType.UseZone"/>.
/// </summary>
public static class ZoneScans
{
    public static readonly List<ZoneScanType> Types = new()
    {
        new ZoneScanType
        {
            Component = PuzzleComponent.AreaScan,
            BaseScan = PuzzleType.AllBig,
            BaseColors = true,
            ScanText = "AREA SCAN - FULL TEAM"
        },
        new ZoneScanType
        {
            Component = PuzzleComponent.AreaScan_Sustained,
            BaseScan = PuzzleType.Sustained,
            Sustained = true,
            ScanText = "SUSTAINED AREA SCAN - FULL TEAM"
        },
        new ZoneScanType
        {
            Component = PuzzleComponent.ZoneScan_Sustained,
            BaseScan = PuzzleType.Sustained,
            Sustained = true,
            UseZone = true,
            ScanText = "SUSTAINED ZONE SCAN - FULL TEAM"
        },
        new ZoneScanType
        {
            Component = PuzzleComponent.ZoneScan_Sustained_Hill,
            BaseScan = PuzzleType.Sustained,
            Sustained = true,
            UseZone = true,
            ScanText = "SUSTAINED ZONE SCAN - FULL TEAM"
        },
    };

    public static ZoneScanType? Find(PuzzleComponent component)
        => Types.Find(type => type.Component.Equals(component));

    /// <summary>
    /// Whether every scan of the chained puzzle is a zone scan component
    /// </summary>
    public static bool IsZoneScanAlarm(ChainedPuzzle puzzle)
        => puzzle.Puzzle.Count > 0 && puzzle.Puzzle.All(component => Find(component) is not null);

    /// <summary>
    /// One entry per chained puzzle made of zone scan components.
    /// </summary>
    public static ZoneScanSettings BuildSettings(IEnumerable<ChainedPuzzle> puzzles)
    {
        var settings = new ZoneScanSettings();

        foreach (var puzzle in puzzles)
        {
            if (!IsZoneScanAlarm(puzzle))
                continue;

            var types = puzzle.Puzzle.Select(Find).ToList();

            settings.AllowedPersistentIDs.Add(new ZoneScanEntry
            {
                ID = puzzle.PersistentId,
                UseZoneInsteadOfNode = types.Any(type => type!.UseZone)
            });
        }

        return settings;
    }

    /// <summary>
    /// Writes ZoneScan's config. Call after the chained puzzles are saved, they need their
    /// persistent IDs.
    /// </summary>
    public static void Save()
    {
        var settings = BuildSettings(Bins.ChainedPuzzles.Blocks);

        settings.Save();

        Plugin.Logger.LogDebug(
            $"ZoneScan: {settings.AllowedPersistentIDs.Count} chained puzzles use area/zone scans");
    }
}

/// <summary>
/// ZoneScan's config. ZoneScan reads it with case sensitive System.Text.Json, so property names
/// must match exactly.
/// </summary>
public record ZoneScanSettings
{
    public List<ZoneScanEntry> AllowedPersistentIDs { get; set; } = new();

    public void Save()
    {
        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(
            Paths.BepInExRootPath,
            "GameData",
            $"{revision}",
            "Custom",
            "ZoneScanConfig");

        Directory.CreateDirectory(dir);

        File.WriteAllText(
            Path.Combine(dir, "ZoneScanSettings.jsonc"),
            JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}

public record ZoneScanEntry
{
    /// <summary>
    /// Chained puzzle persistent ID
    /// </summary>
    public uint ID { get; init; }

    public bool UseZoneInsteadOfNode { get; init; }

    /// <summary>
    /// Outline the scan boundary on the doors and open dividers leading out of the area/zone
    /// </summary>
    public bool AutoGenScanGraphic { get; init; } = true;

    public bool IgnoreClosedSecDoor { get; init; } = false;

    public bool IgnoreDivider { get; init; } = false;

    public bool UseSmallDividerGraphic { get; init; } = false;

    public List<object> CustomAlignGraphic { get; init; } = new();
}
