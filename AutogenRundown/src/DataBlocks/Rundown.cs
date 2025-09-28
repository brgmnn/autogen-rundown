using AutogenRundown.DataBlocks.Custom.AutogenRundown.LogArchives;
using AutogenRundown.DataBlocks.Logs;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public record Rundown : DataBlock
{
    public static readonly uint R_Daily = 1;
    public static readonly uint R_Weekly = 2;
    public static readonly uint R_Monthly = 3;
    public static readonly uint R_Seasonal = 4;

    public List<Level> TierA { get; set; } = new();
    public List<Level> TierB { get; set; } = new();
    public List<Level> TierC { get; set; } = new();
    public List<Level> TierD { get; set; } = new();
    public List<Level> TierE { get; set; } = new();

    [JsonIgnore]
    public int TierA_Count { get; set; } = Generator.Between(1, 2);
    [JsonIgnore]
    public int TierB_Count { get; set; } = Generator.Between(3, 4);
    [JsonIgnore]
    public int TierC_Count { get; set; } = Generator.Between(2, 4);
    [JsonIgnore]
    public int TierD_Count { get; set; } = Generator.Between(1, 2);
    [JsonIgnore]
    public int TierE_Count { get; set; } = Generator.Between(1, 2);

    /// <summary>
    /// If there are build seeds in this pool they will be used to set the build seed of the level being built.
    /// </summary>
    [JsonIgnore]
    public List<int> BuildSeedPool { get; set; } = new();

    /// <summary>
    /// Name of the rundown to be used
    /// </summary>
    [JsonIgnore]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Name of the rundown to be used
    /// </summary>
    [JsonIgnore]
    public string StoryTitle { get; set; } = string.Empty;


    // Values we just leave as is
    public bool NeverShowRundownTree = false;
    public int VanityItemLayerDropDataBlock = 0;

    #region Unlock requirements for levels
    public bool UseTierUnlockRequirements { get; set; } = false;

    // No restrictions on which levels can be accessed, so these are all defaults
    public ReqToReachTier ReqToReachTierB = new();
    public ReqToReachTier ReqToReachTierC = new();
    public ReqToReachTier ReqToReachTierD = new();
    public ReqToReachTier ReqToReachTierE = new();
    #endregion

    private double ScaleTierWidth(int levelCount)
        => levelCount switch
        {
            1 => 0.01,
            2 => 0.50,
            3 => 0.65,
            4 => 0.75,
            5 => 0.85,

            _ => 1.0
        };

    [JsonIgnore]
    public Color VisualsETier { get; set; } = Color.MenuVisuals;

    public JObject StorytellingData {
        get => JObject.FromObject(new
        {
            Title = StoryTitle,
            TextLog = 1268,
            TextLogPos = new
            {
                x = 0.0,
                y = 0.0,
                magnitude = 0.0,
                sqrMagnitude = 0.0
            },
            Visuals = new
            {
                ColorBackground = new Color { Alpha = 1.0f },
                TierAVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = ScaleTierWidth(TierA.Count),
                    ScaleYModifier = 0.3
                },
                TierBVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = ScaleTierWidth(TierB.Count),
                    ScaleYModifier = 0.3
                },
                TierCVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = ScaleTierWidth(TierC.Count),
                    ScaleYModifier = 0.3
                },
                TierDVisuals = new
                {
                    Color = Color.MenuVisuals,
                    Scale = ScaleTierWidth(TierD.Count),
                    ScaleYModifier = 0.3
                },
                TierEVisuals = new
                {
                    Color = VisualsETier,
                    Scale = ScaleTierWidth(TierE.Count),
                    ScaleYModifier = 0.3
                },
            },
            SurfaceIconPosition = new
            {
                x = 0.0,
                y = 0.0,
                magnitude = 0.0,
                sqrMagnitude = 0.0
            },
            SurfaceDescription = 1267,
            ExternalExpTitle = 3901084012
        });
        private set { }
    }

    [JsonIgnore]
    public string DisplaySeed { get; set; } = "";

    public void AddLevel(Level level)
    {
        switch (level.Tier)
        {
            case "A":
                TierA.Add(level);
                break;
            case "B":
                TierB.Add(level);
                break;
            case "C":
                TierC.Add(level);
                break;
            case "D":
                TierD.Add(level);
                break;
            case "E":
                TierE.Add(level);
                break;
        }
    }

    /// <summary>
    /// Builds a new Rundown
    /// </summary>
    /// <returns></returns>
    public static Rundown Build(Rundown rundown)
    {
        // Rundown.Name is used by LocalProgression for storing the progression data. Ensure
        // this is unique to guarantee we store progression between runs.
        rundown.Name = $"RndRundownSeed{Generator.Seed.Replace("_", "")}";
        rundown.DisplaySeed = Generator.DisplaySeed;

        return rundown;
    }

    /// <summary>
    /// Distributes D-Lock Block Decipherer logs across the rundowns levels
    ///
    /// 83 total levels in base
    /// 195 total logs
    /// ~2.35 logs per level
    ///
    /// Logs are distributed as follows in levels across all rundowns
    ///     0 logs: 4
    ///     1 logs: 21
    ///     2 logs: 30
    ///     3 logs: 16
    ///     4 logs: 6
    ///     5 logs: 2
    ///     6 logs: 4
    /// </summary>
    public void DistributeDLockLogs()
    {
        var logs = DLockDecipherer.AllLogs.Shuffle();
        var totallevels = TierA.Count + TierB.Count + TierC.Count + TierD.Count + TierE.Count;

        // 83 total levels in base
        // 195 total logs
        // ~2.35 logs per level

        const double total = 83;

        var levels = new List<Level>();
        levels.AddRange(TierA);
        levels.AddRange(TierB);
        levels.AddRange(TierC);
        levels.AddRange(TierD);
        levels.AddRange(TierE);

        Plugin.Logger.LogDebug("=== D-Lock Block Decipherer ===");

        foreach (var level in levels)
        {
            var terminals = new List<(Bulkhead, Zone, TerminalPlacement)>();
            var bulkheads =  new List<Bulkhead> { Bulkhead.Main, Bulkhead.Extreme, Bulkhead.Overload };

            // Logs are distributed as follows in levels across all rundowns. We weight number of
            // logs in each level to match the same distribution profile as the base game
            //     0 logs: 4
            //     1 logs: 21
            //     2 logs: 30
            //     3 logs: 16
            //     4 logs: 6
            //     5 logs: 2
            //     6 logs: 4
            var totalLogs = Generator.Select(new List<(double chance, int count)>
            {
                ( 4 / total, 0),
                (21 / total, 1),
                (30 / total, 2),
                (16 / total, 3),
                ( 6 / total, 4),
                ( 5 / total, 5),
                ( 6 / total, 6),
            });

            Plugin.Logger.LogDebug($"Placing {totalLogs} logs in \"{level.Tier}{level.Index} {level.Name}\", logs located at:");

            foreach (var bulkhead in bulkheads)
            {
                var layout = level.GetLevelLayout(bulkhead);

                if (layout == null)
                    continue;

                foreach (var zone in layout.Zones)
                    foreach (var terminal in zone.TerminalPlacements)
                        terminals.Add((bulkhead, zone, terminal));
            }

            var toPlace = terminals.Shuffle().Take(totalLogs);

            foreach (var (bulkhead, zone, terminal) in toPlace)
            {
                var lorelog = logs.PickRandom();

                if (lorelog != null)
                {
                    terminal.LogFiles.Add(lorelog);

                    level.LogArchives.Logs.Add(new Log
                    {
                        Bulkhead = bulkhead,
                        ZoneNumber = zone.LocalIndex,
                        FileName = lorelog.FileName
                    });

                    if (lorelog.AttachedAudioFile != Sound.None &&
                        terminal.StartingStateData.StartingState == TerminalState.Sleeping)
                    {
                        terminal.StartingStateData.StartingState = TerminalState.AudioLoopError;
                        Plugin.Logger.LogDebug($" -> {bulkhead}, ZONE_{zone.LocalIndex}, with audio");
                    }
                    else
                    {
                        Plugin.Logger.LogDebug($" -> {bulkhead}, ZONE_{zone.LocalIndex}");
                    }
                }
            }

            level.LogArchives.Save();
        }
    }

    [JsonProperty("name")]
    public new string BlockName
    {
        get => Name;
        private set { }
    }
}
