using AutogenRundown.DataBlocks.Custom.ZoneScan;

namespace AutogenRundown.DataBlocks.Alarms;

/// <summary>
/// Area and zone scan alarms, using the ZoneScan plugin (see <see cref="ZoneScans"/>).
///
/// Every component of these must be a zone scan component with the same area/zone mode:
/// ZoneScan applies to all scans of a chained puzzle.
/// </summary>
public static class ZoneScanAlarms
{
    /// <summary>
    /// Full team has to gather in the area (room) of each scan
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass2_AreaScan = new()
    {
        PublicAlarmName = "Class II Area Alarm",
        Settings = WaveSettings.Baseline_Easy,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 35.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan
        }
    };

    public static readonly ChainedPuzzle AlarmClass3_AreaScan = new()
    {
        PublicAlarmName = "Class III Area Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 35.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan,
            PuzzleComponent.AreaScan
        }
    };

    public static readonly ChainedPuzzle AlarmClass4_AreaScan = new()
    {
        PublicAlarmName = "Class IV Area Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 35.0,
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
        PublicAlarmName = "Class S I Area Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 18.0,
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
        PublicAlarmName = "Class S I Zone Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
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
