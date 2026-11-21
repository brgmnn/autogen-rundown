using AutogenRundown.DataBlocks.Custom.ZoneScan;

namespace AutogenRundown.DataBlocks.Alarms;

/// <summary>
/// Area and zone scan alarms, using the ZoneScan plugin (see <see cref="ZoneScans"/>).
///
/// Every component of these must be a zone scan component with the same area/zone mode:
/// ZoneScan applies to all scans of a chained puzzle.
///
/// They start at the door (no WantedDistanceFromStartPos, Zone.RollAlarms keeps it at 0) and
/// always show the HUD: players can be inside a large area/zone and far from the scan itself.
/// Multi-scan area alarms put each scan just inside the gate of a neighbouring area of the
/// zone that has no scan yet (see Patch_LG_NodeTools.TryPlaceAreaScans). The spacing is only
/// used if that placement fails.
/// </summary>
public static class ZoneScanAlarms
{
    /// <summary>
    /// Full team has to gather in the area (room) of each scan
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass2_AreaScan = new()
    {
        PublicAlarmName = "Class A II Alarm",
        Settings = WaveSettings.Baseline_Easy,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 15.0,
        OnlyShowHUDWhenPlayerIsClose = false,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan
        }
    };

    public static readonly ChainedPuzzle AlarmClass3_AreaScan = new()
    {
        PublicAlarmName = "Class A III Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 15.0,
        OnlyShowHUDWhenPlayerIsClose = false,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan
        }
    };

    public static readonly ChainedPuzzle AlarmClass4_AreaScan = new()
    {
        PublicAlarmName = "Class A IV Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 15.0,
        OnlyShowHUDWhenPlayerIsClose = false,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan
        }
    };

    /// <summary>
    /// Full team has to hold the area (room) of the scan, progress drains while anyone is out
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass1_AreaScan_Sustained = new()
    {
        PublicAlarmName = "Class S A I Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        OnlyShowHUDWhenPlayerIsClose = false,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AreaScan_Sustained
        }
    };

    /// <summary>
    /// Full team has to hold the whole zone of the scan, progress drains while anyone is out
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass1_ZoneScan_Sustained = new()
    {
        PublicAlarmName = "Class S Z Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        OnlyShowHUDWhenPlayerIsClose = false,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ZoneScan_Sustained
        }
    };

    /// <summary>
    /// Entries added to <see cref="ChainedPuzzle.BuildPack"/>. Same (chance, count, puzzle)
    /// weighting as the rest of the pack.
    /// </summary>
    public static List<(double chance, int count, ChainedPuzzle puzzle)> BuildPack(string tier)
        => tier switch
        {
            "A" => new()
            {
                (0.5, 1, AlarmClass2_AreaScan),
            },

            "B" => new()
            {
                (0.5, 1, AlarmClass2_AreaScan),
                (0.6, 1, AlarmClass3_AreaScan),
            },

            "C" => new()
            {
                (0.5, 1, AlarmClass3_AreaScan),
                (0.6, 1, AlarmClass4_AreaScan),
                (0.6, 1, AlarmClass1_AreaScan_Sustained),
            },

            "D" => new()
            {
                (0.6, 1, AlarmClass4_AreaScan),
                (0.8, 1, AlarmClass1_AreaScan_Sustained),
                (0.5, 1, AlarmClass1_ZoneScan_Sustained),
            },

            "E" => new()
            {
                (0.7, 1, AlarmClass4_AreaScan),
                (0.8, 1, AlarmClass1_AreaScan_Sustained),
                (0.7, 1, AlarmClass1_ZoneScan_Sustained),
            },

            _ => new()
        };
}
