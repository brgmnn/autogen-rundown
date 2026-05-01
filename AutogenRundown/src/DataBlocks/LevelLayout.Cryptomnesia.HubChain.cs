using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// HubChain: A chain of 2-4 hub zones connected in sequence.
    /// Each zone uses GenHubGeomorph for large, open rooms.
    ///
    /// "hub_3", "side_2", "side_3a", "side_1"
    ///
    /// Layout map:
    ///
    ///   start
    ///   ├── side_1                            -> Dimension3 Cube
    ///   └── hub_2
    ///       ├── side_2                        -> Dimension1 Cube
    ///       ├── side_3a                       -> Dimension2 Cube
    ///       │   └── side_3b
    ///       └── hub_3                         -> Reality Cube
    ///           ├── side_4
    ///           └── forward_extract
    ///               └── extraction_elevator
    ///
    /// </summary>
    /// <returns>The last zone node (for data cube placement).</returns>
    private ZoneNode BuildCryptomnesia_HubChain(ZoneNode start)
    {
        #region Phase 1

        var hub1 = planner.UpdateNode(start with { MaxConnections = 3 });
        var hub1Zone = planner.GetZone(hub1)!;

        hub1 = level.GenHubGeomorph(hub1);

        var (side1, side1Zone) = AddZone_Side(hub1, new ZoneNode { Branch = "side_1" });

        side1Zone.Coverage = CoverageMinMax.Large_80;
        side1Zone.ZoneExpansion = ZoneExpansion.Expansional;

        #endregion


        #region Phase 2

        var (hub2, hub2Zone) = AddZone_Forward(hub1, new ZoneNode { Branch = "hub_2", MaxConnections = 3 });
        hub2 = level.GenHubGeomorph(hub2);

        var (side2, side2Zone) = AddZone_Left(hub2, new ZoneNode { Branch = "side_2", MaxConnections = 3 });
        side2 = level.GenHubGeomorph(side2);

        var (side3a, side3aZone) = AddZone_Right(hub2, new ZoneNode { Branch = "side_3a", MaxConnections = 1 });
        var (side3b, side3bZone) = AddZone_Right(side3a, new ZoneNode { Branch = "side_3b" });

        #endregion

        #region Phase 3

        var (hub3, hub3Zone) = AddZone_Forward(hub2, new ZoneNode { Branch = "hub_3", MaxConnections = 3 });
        hub3 = level.GenHubGeomorph(hub3);

        var (side4, side4Zone) = AddZone_Forward(hub3, new ZoneNode { Branch = "side_4", MaxConnections = 3 });

        #endregion

        #region Forward extract

        var (forwardExtract, forwardExtractZone) = AddZone_Left(
            hub3,
            new ZoneNode { Branch = "forward_extract", MaxConnections = 1 });

        forwardExtractZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
        forwardExtract = planner.AddTags(forwardExtract, CryptoTag_Extraction);

        if (Generator.Flip())
            forwardExtract = level.GenCorridorGeomorph(forwardExtract);
        else
        {
            forwardExtractZone.Coverage = CoverageMinMax.Medium_56;
            forwardExtractZone.ZoneExpansion = ZoneExpansion.Expansional;
        }

        var (extraction, extractionZone) = AddZone_Left(
            forwardExtract,
            new ZoneNode { Branch = "extraction_elevator", MaxConnections = 0 });

        extraction = level.GenExitGeomorph(extraction);

        extraction = planner.AddTags(extraction, CryptoTag_Extraction);

        level.ExtractionZone = extraction;

        #endregion

        return hub3;
    }

    #region HubChain challenge setup

    /// <summary>
    /// The secondary challenge types that can be rolled onto a HubChain cube door.
    /// </summary>
    private enum HubChainSecondary
    {
        None,
        Keycard,
        Generator,
        TerminalLock,
        Sensors,
        ShortError,
        MiniBoss,
        PlainAlarm,
    }

    /// <summary>
    /// Applies per-zone challenges to a HubChain dimension. Must be called AFTER
    /// ApplyCryptomnesiaTheme's enemy/visual setup (ie from inside ApplyCryptomnesiaTheme)
    /// so the Cryptomnesia_EnterEvents/ExitEvents for this dimension are already initialised.
    ///
    /// Challenges are only placed on the dimension's cube route (the path from hub_1 to
    /// the cube branch); off-route branches are left untouched so players aren't forced
    /// to traverse the whole map in every dimension. An optional single off-route detour
    /// zone may be selected to hold a progression puzzle's key for variety.
    /// </summary>
    public static void SetupHubChain(
        CryptomnesiaTheme theme,
        LevelLayout layout,
        Level level,
        BuildDirector director,
        WardenObjective objective)
    {
        var dim = layout.Dimension;
        var slot = (int)dim;

        if (slot >= objective.Cryptomnesia_CubeBranches.Count)
        {
            Plugin.Logger.LogWarning(
                $"HubChain: no cube branch for dim={dim} (slot {slot})");
            return;
        }

        var cubeBranch = objective.Cryptomnesia_CubeBranches[slot];
        var onRoute = RouteFor(cubeBranch);

        // 1. Hub alarms -- only on route zones
        if (onRoute.Contains("hub_2"))
            ApplyHubAlarm(level, director, dim, theme, "hub_2", weakerTier: false);
        if (onRoute.Contains("hub_3"))
            ApplyHubAlarm(level, director, dim, theme, "hub_3", weakerTier: true);

        // 2. Climactic challenge on the cube zone
        var cubeZone = ZoneByBranch(level, director, dim, cubeBranch);
        ApplyClimax(theme, level, director, objective, dim, cubeBranch, cubeZone);

        // 3. Secondary challenge -- one per dim, rolled from a theme-filtered pool
        var secondary = PickSecondary(theme, cubeBranch);
        var holderBranch = ApplySecondary(
            secondary, theme, layout, level, director, objective, dim, cubeBranch, cubeZone, onRoute);

        // 4. side_4 role -- supply by default; key-holder only in Reality (via ApplySecondary detour roll)
        ApplySide4(layout, level, dim, theme, cubeBranch, isHolder: holderBranch == "side_4");

        Plugin.Logger.LogDebug(
            $"HubChain: dim={dim} theme={theme} cube={cubeBranch} " +
            $"route=[{string.Join(",", onRoute)}] secondary={secondary} holder={holderBranch ?? "-"}");
    }

    /// <summary>Zones players MUST pass through from hub_1 to the cube branch.</summary>
    private static HashSet<string> RouteFor(string cubeBranch) => cubeBranch switch
    {
        "hub_3" => new HashSet<string> { "hub_1", "hub_2", "hub_3" },
        "side_1" => new HashSet<string> { "hub_1" },
        "side_2" => new HashSet<string> { "hub_1", "hub_2" },
        "side_3a" => new HashSet<string> { "hub_1", "hub_2" },
        _ => new HashSet<string> { "hub_1" }
    };

    private static Zone? ZoneByBranch(Level level, BuildDirector director, DimensionIndex dim, string branch)
    {
        var node = level.Planner.GetLastZone(director.Bulkhead, branch, dim);
        return node.HasValue ? level.Planner.GetZone(node.Value) : null;
    }

    #endregion

    #region HubChain: hub alarms

    private static void ApplyHubAlarm(
        Level level,
        BuildDirector director,
        DimensionIndex dim,
        CryptomnesiaTheme theme,
        string branch,
        bool weakerTier)
    {
        var zone = ZoneByBranch(level, director, dim, branch);
        if (zone == null) return;

        // 40% of hub doors get a hard travel scan instead of a door apex alarm.
        // Travel scans scale by tier, same as AddTravelScanAlarm does.
        var puzzle = Generator.Flip(0.4)
            ? BuildHardTravelScan(level.Tier, weakerTier, theme)
            : BuildApexAlarm(level.Tier, weakerTier, theme);

        zone.Alarm = ChainedPuzzle.FindOrPersist(puzzle);

        zone.AmmoPacks += 3.0;
        zone.ToolPacks += 2.0;
    }

    /// <summary>Themed apex alarm for hub entry doors. Mixes baseline strikers with theme-specific enemies.</summary>
    private static ChainedPuzzle BuildApexAlarm(string tier, bool weaker, CryptomnesiaTheme theme)
    {
        var basePuzzle = (tier, weaker) switch
        {
            ("A", false) => ChainedPuzzle.AlarmClass3_Mixed,
            ("A", true) => ChainedPuzzle.AlarmClass2,
            ("B", false) => ChainedPuzzle.AlarmClass4_Mixed,
            ("B", true) => ChainedPuzzle.AlarmClass3_Mixed,
            ("C", false) => ChainedPuzzle.AlarmClass5_Mixed,
            ("C", true) => ChainedPuzzle.AlarmClass4_Mixed,
            ("D", false) => ChainedPuzzle.AlarmClass6_Mixed,
            ("D", true) => ChainedPuzzle.AlarmClass5_Mixed,
            ("E", false) => ChainedPuzzle.AlarmClass7_Mixed,
            ("E", true) => ChainedPuzzle.AlarmClass6_Mixed,
            _ => weaker ? ChainedPuzzle.AlarmClass3_Mixed : ChainedPuzzle.AlarmClass4_Mixed
        };

        return basePuzzle with { Population = ThemedPopulation(theme, tier) };
    }

    /// <summary>Hard travel scan in the room as an alternative to an apex door alarm.</summary>
    private static ChainedPuzzle BuildHardTravelScan(string tier, bool weaker, CryptomnesiaTheme theme)
    {
        var scanOptions = (tier, weaker) switch
        {
            ("A", _) => new List<(double, PuzzleComponent)>
            {
                (0.66, PuzzleComponent.TravelTeam_Short),
                (0.34, PuzzleComponent.TravelTeam_MediumGreen),
            },
            ("B", false) => new List<(double, PuzzleComponent)>
            {
                (0.60, PuzzleComponent.TravelTeam_MediumGreen),
                (0.40, PuzzleComponent.TravelTeam_LongGreen),
            },
            ("B", true) => new List<(double, PuzzleComponent)>
            {
                (0.70, PuzzleComponent.TravelTeam_Short),
                (0.30, PuzzleComponent.TravelTeam_MediumGreen),
            },
            ("C", false) => new List<(double, PuzzleComponent)>
            {
                (0.50, PuzzleComponent.TravelTeam_MediumGreen),
                (0.50, PuzzleComponent.TravelTeam_LongGreen),
            },
            ("C", true) => new List<(double, PuzzleComponent)>
            {
                (0.60, PuzzleComponent.TravelTeam_MediumGreen),
                (0.40, PuzzleComponent.TravelTeam_LongGreen),
            },
            ("D", false) => new List<(double, PuzzleComponent)>
            {
                (0.40, PuzzleComponent.TravelTeam_LongGreen),
                (0.60, PuzzleComponent.SustainedTravel),
            },
            ("D", true) => new List<(double, PuzzleComponent)>
            {
                (0.70, PuzzleComponent.TravelTeam_LongGreen),
                (0.30, PuzzleComponent.SustainedTravel),
            },
            ("E", false) => new List<(double, PuzzleComponent)>
            {
                (0.30, PuzzleComponent.TravelTeam_LongGreen),
                (0.70, PuzzleComponent.SustainedTravel),
            },
            ("E", true) => new List<(double, PuzzleComponent)>
            {
                (0.60, PuzzleComponent.TravelTeam_LongGreen),
                (0.40, PuzzleComponent.SustainedTravel),
            },
            _ => new List<(double, PuzzleComponent)> { (1.0, PuzzleComponent.TravelTeam_MediumGreen) }
        };

        var scan = Generator.Select(scanOptions);
        var isSustained = scan == PuzzleComponent.SustainedTravel;

        return ChainedPuzzle.TravelAlarm_Team with
        {
            PublicAlarmName = isSustained ? "Class S T Alarm" : "Class T Alarm",
            Puzzle = new List<PuzzleComponent> { scan },
            Population = ThemedPopulation(theme, tier),
            Settings = (tier, weaker) switch
            {
                ("D" or "E", false) => WaveSettings.Baseline_Hard,
                _ => WaveSettings.Baseline_Normal
            }
        };
    }

    /// <summary>
    /// Picks a themed wave population. Per user guidance: "standards as normal wave strikers
    /// and the rest as themed enemies" -- i.e. mixed baseline + theme flavour, not pure-theme.
    /// Uses the hard variants for tiers D/E where available.
    /// </summary>
    private static WavePopulation ThemedPopulation(CryptomnesiaTheme theme, string tier)
    {
        var hard = tier is "D" or "E";
        return theme switch
        {
            CryptomnesiaTheme.Chargers => hard ? WavePopulation.Baseline_Chargers_Hard : WavePopulation.Baseline_Chargers,
            CryptomnesiaTheme.Shadows => WavePopulation.Baseline_Shadows,
            CryptomnesiaTheme.Nightmares => hard ? WavePopulation.Baseline_Nightmare_Hard : WavePopulation.Baseline_Nightmare,
            CryptomnesiaTheme.InfectionFog => WavePopulation.Baseline_Infested,
            // Giants theme pairs with baseline since there's no "Baseline_Giants" pool;
            // the dimension already has hibernate giants in every zone via the theme.
            _ => WavePopulation.Baseline
        };
    }

    #endregion

    #region HubChain: climax

    private static void ApplyClimax(
        CryptomnesiaTheme theme,
        Level level,
        BuildDirector director,
        WardenObjective objective,
        DimensionIndex dim,
        string cubeBranch,
        Zone? cubeZone)
    {
        if (cubeZone == null) return;

        switch (theme)
        {
            case CryptomnesiaTheme.Giants:
                AddScoutRoom(cubeZone, EnemySpawningData.Scout, count: 6);
                break;

            case CryptomnesiaTheme.Chargers:
                AddScoutRoom(cubeZone, EnemySpawningData.ScoutCharger, count: 6);
                break;

            case CryptomnesiaTheme.Shadows:
                AddScoutRoom(cubeZone, EnemySpawningData.ScoutShadow, count: 6);
                break;

            case CryptomnesiaTheme.Nightmares:
                // Mixed scout room: 1-2 nightmares + 4-5 zoomers (6 total) for pressure
                // without overloading the zone with full nightmare elites.
                var nightmareCount = Generator.Between(1, 2);
                AddScoutRoom(cubeZone, EnemySpawningData.ScoutNightmare, count: nightmareCount);
                AddScoutRoom(cubeZone, EnemySpawningData.ScoutZoomer, count: 6 - nightmareCount);
                break;

            case CryptomnesiaTheme.InfectionFog:
                // Extra dense spitters at the climax zone (theme already seeds baseline).
                cubeZone.StaticSpawnDataContainers.Add(new StaticSpawnDataContainer
                {
                    Count = 300,
                    DistributionWeightType = 0,
                    DistributionWeight = 1.0,
                    DistributionRandomBlend = 0.0,
                    DistributionResultPow = 2.0,
                    Unit = StaticSpawnUnit.Spitter,
                    FixedSeed = Generator.Between(10, 150)
                });
                cubeZone.DisinfectPacks += 2;
                break;

            case CryptomnesiaTheme.ErrorAlarm:
                // Theme already installs a dimension-wide ambient error alarm; no extra
                // climax needed. (Adding another sustained error wave here would stack.)
                break;
        }

        _ = level; _ = director; _ = objective; _ = dim; _ = cubeBranch;
    }

    private static void AddScoutRoom(Zone zone, EnemySpawningData scout, int count)
    {
        for (int i = 0; i < count; i++)
            zone.EnemySpawningInZone.Add(scout with
            {
                Distribution = EnemyZoneDistribution.ForceOne,
                Points = 25
            });
    }

    #endregion

    #region HubChain: secondary challenge

    private static HubChainSecondary PickSecondary(CryptomnesiaTheme theme, string cubeBranch)
    {
        var pool = new List<(double, HubChainSecondary)>
        {
            (1.0, HubChainSecondary.Keycard),
            (1.0, HubChainSecondary.Generator),
            (1.0, HubChainSecondary.TerminalLock),
            (0.8, HubChainSecondary.Sensors),
            (0.8, HubChainSecondary.MiniBoss),
            (0.6, HubChainSecondary.PlainAlarm),
            (0.3, HubChainSecondary.None),
        };

        // Short error alarm is only safe on non-ErrorAlarm themes. Also exclude it when
        // the cube zone is a scout-room climax (Giants/Chargers/Shadows/Nightmares) --
        // the error wave would trigger all scouts and defeat the stealth challenge.
        var isScoutClimax =
            theme is CryptomnesiaTheme.Giants
                 or CryptomnesiaTheme.Chargers
                 or CryptomnesiaTheme.Shadows
                 or CryptomnesiaTheme.Nightmares;

        if (theme != CryptomnesiaTheme.ErrorAlarm && !isScoutClimax)
            pool.Add((0.7, HubChainSecondary.ShortError));

        return Generator.Select(pool);
    }

    /// <summary>
    /// Applies the chosen secondary to the cube-zone entry. If a puzzle is chosen and an
    /// off-route detour is rolled, returns the detour branch ("side_4" / "side_1" / etc.)
    /// where the key was placed; otherwise returns null.
    /// </summary>
    private static string? ApplySecondary(
        HubChainSecondary secondary,
        CryptomnesiaTheme theme,
        LevelLayout layout,
        Level level,
        BuildDirector director,
        WardenObjective objective,
        DimensionIndex dim,
        string cubeBranch,
        Zone? cubeZone,
        HashSet<string> onRoute)
    {
        if (cubeZone == null || secondary == HubChainSecondary.None)
            return null;

        switch (secondary)
        {
            case HubChainSecondary.Keycard:
                return ApplyPuzzleDoor(
                    ProgressionPuzzleType.Keycard, layout, level, director,
                    dim, cubeBranch, cubeZone, onRoute);

            case HubChainSecondary.Generator:
                return ApplyPuzzleDoor(
                    ProgressionPuzzleType.Generator, layout, level, director,
                    dim, cubeBranch, cubeZone, onRoute);

            case HubChainSecondary.TerminalLock:
                return ApplyTerminalLock(
                    layout, level, director, dim, cubeBranch, onRoute);

            case HubChainSecondary.Sensors:
                // Place sensors in an upstream route zone (not the cube zone itself so
                // players don't fire them while doing the cube scan).
                var sensorBranch = PickUpstream(onRoute, cubeBranch);
                var sensorNode = level.Planner.GetLastZone(director.Bulkhead, sensorBranch, dim);
                if (sensorNode.HasValue)
                    layout.AddSecuritySensors(sensorNode.Value);
                return null;

            case HubChainSecondary.ShortError:
                // Short error alarm on the cube door. The puzzle's own survival wave is
                // disabled on scan completion (DisableSurvivalWaveOnComplete = true) so it
                // self-terminates once the scan finishes -- no exit-event cleanup needed.
                cubeZone.Alarm = ChainedPuzzle.FindOrPersist(ChainedPuzzle.AlarmError_Baseline with
                {
                    PublicAlarmName = "Class ://ERROR! (short)",
                    Puzzle = new List<PuzzleComponent> { PuzzleComponent.ScanSmall },
                    Settings = WaveSettings.Error_Easy,
                    DisableSurvivalWaveOnComplete = true
                });
                return null;

            case HubChainSecondary.MiniBoss:
                cubeZone.EventsOnDoorScanStart.AddSpawnWave(new GenericWave
                {
                    Settings = WaveSettings.SingleMiniBoss,
                    Population = BossPopulationFor(theme),
                    TriggerAlarm = false
                }, delay: Generator.Between(10, 25));
                return null;

            case HubChainSecondary.PlainAlarm:
                var plain = level.Tier switch
                {
                    "A" or "B" => ChainedPuzzle.AlarmClass3_Mixed,
                    "C" => ChainedPuzzle.AlarmClass4_Mixed,
                    _ => ChainedPuzzle.AlarmClass5_Mixed
                };
                cubeZone.Alarm = ChainedPuzzle.FindOrPersist(plain);
                return null;
        }

        return null;
    }

    /// <summary>
    /// Wires a keycard / generator door onto the cube zone and picks a holder zone for
    /// the key/cell. On-route holders are preferred; with a small probability an off-route
    /// detour zone is selected instead (at most one detour per dim).
    /// </summary>
    private static string? ApplyPuzzleDoor(
        ProgressionPuzzleType type,
        LevelLayout layout,
        Level level,
        BuildDirector director,
        DimensionIndex dim,
        string cubeBranch,
        Zone cubeZone,
        HashSet<string> onRoute)
    {
        var (holderBranch, holderNode) = PickHolder(level, director, dim, cubeBranch, onRoute);
        if (holderBranch == null || !holderNode.HasValue)
            return null;

        cubeZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
        {
            PuzzleType = type,
            PlacementCount = 1,
            ZonePlacementData = new List<ZonePlacementData>
            {
                new()
                {
                    Dimension = dim,
                    LocalIndex = holderNode.Value.ZoneNumber,
                    Weights = ZonePlacementWeights.EvenlyDistributed
                }
            }
        };

        level.Planner.AddTags(holderNode.Value, CryptoTag_KeyHolder);

        _ = layout;
        return holderBranch;
    }

    /// <summary>
    /// Wires a terminal-activation door onto the cube zone. The terminal that unlocks
    /// the door is placed in a separate reachable zone (same holder-selection logic as
    /// keycard/generator). Uses AddTerminalUnlockPuzzle to install the ACTIVATE_DOOR
    /// terminal command.
    /// </summary>
    private static string? ApplyTerminalLock(
        LevelLayout layout,
        Level level,
        BuildDirector director,
        DimensionIndex dim,
        string cubeBranch,
        HashSet<string> onRoute)
    {
        var cubeNode = level.Planner.GetLastZone(director.Bulkhead, cubeBranch, dim);
        if (!cubeNode.HasValue) return null;

        var (holderBranch, holderNode) = PickHolder(level, director, dim, cubeBranch, onRoute);
        if (holderBranch == null || !holderNode.HasValue) return null;

        layout.AddTerminalUnlockPuzzle(cubeNode.Value, holderNode.Value);
        level.Planner.AddTags(holderNode.Value, CryptoTag_KeyHolder);
        return holderBranch;
    }

    /// <summary>
    /// Shared holder-zone picker for keycard / generator / terminal puzzles. Prefers an
    /// on-route branch (so the player passes it on the way to the cube); with ~35% chance
    /// rolls a single off-route detour branch instead, never exceeding one detour per dim.
    /// </summary>
    private static (string? branch, ZoneNode? node) PickHolder(
        Level level,
        BuildDirector director,
        DimensionIndex dim,
        string cubeBranch,
        HashSet<string> onRoute)
    {
        var onRouteHolders = onRoute.Where(b => b != cubeBranch && b != "hub_1").ToList();
        var detourOptions = DetourOptionsFor(cubeBranch, dim);

        string holderBranch;
        if (onRouteHolders.Count > 0 && Generator.Flip(0.65))
        {
            holderBranch = Generator.Pick(onRouteHolders)!;
        }
        else if (detourOptions.Count > 0)
        {
            holderBranch = Generator.Pick(detourOptions)!;
        }
        else if (onRouteHolders.Count > 0)
        {
            holderBranch = Generator.Pick(onRouteHolders)!;
        }
        else
        {
            holderBranch = "hub_1";
        }

        var node = level.Planner.GetLastZone(director.Bulkhead, holderBranch, dim);
        return node.HasValue ? (holderBranch, node) : (null, null);
    }

    /// <summary>
    /// Up to one off-route detour branch the player may be asked to visit for a key/cell.
    /// Excludes zones that would force clearing too many extra alarms for that dim.
    /// </summary>
    private static List<string> DetourOptionsFor(string cubeBranch, DimensionIndex dim) => cubeBranch switch
    {
        // Reality: side_4 is on-route-adjacent (sibling off hub_3). Other sides are 1-zone detours.
        "hub_3" => new List<string> { "side_4", "side_1", "side_2", "side_3a" },

        // Cube in side_2: side_1 is ~1 zone off hub_1, side_3a is ~1 zone off hub_2 -- both cheap detours.
        "side_2" => new List<string> { "side_1", "side_3a" },

        // Cube in side_3a: mirror of side_2.
        "side_3a" => new List<string> { "side_1", "side_2" },

        // Cube in side_1: any detour past hub_1 forces clearing hub_2's entry alarm, so keep empty.
        _ => new List<string>()
    };

    /// <summary>Upstream on-route branch (not the cube) used to host sensors.</summary>
    private static string PickUpstream(HashSet<string> onRoute, string cubeBranch)
    {
        var upstream = onRoute.Where(b => b != cubeBranch).ToList();
        if (upstream.Count == 0) return cubeBranch; // degenerate: put sensors on cube zone
        // Prefer hub_2 as a large arena; fall back to hub_3 or hub_1.
        if (upstream.Contains("hub_2")) return "hub_2";
        if (upstream.Contains("hub_3")) return "hub_3";
        return upstream[0];
    }

    private static WavePopulation BossPopulationFor(CryptomnesiaTheme theme) => theme switch
    {
        CryptomnesiaTheme.Shadows => WavePopulation.SingleEnemy_PouncerShadow,
        CryptomnesiaTheme.InfectionFog => WavePopulation.SingleEnemy_Mother,
        CryptomnesiaTheme.ErrorAlarm => WavePopulation.SingleEnemy_Pouncer,
        _ => WavePopulation.SingleEnemy_Tank
    };

    #endregion

    #region HubChain: side_4 role

    private static void ApplySide4(
        LevelLayout layout,
        Level level,
        DimensionIndex dim,
        CryptomnesiaTheme theme,
        string cubeBranch,
        bool isHolder)
    {
        // If side_4 was already selected as the key-holder via ApplySecondary's detour
        // roll, don't stack supply on top -- the key placement is the whole story.
        if (isHolder) return;

        var side4Node = level.Planner.GetLastZone(Bulkhead.Main, "side_4", dim);
        if (!side4Node.HasValue) return;

        var zone = level.Planner.GetZone(side4Node.Value);
        if (zone == null) return;

        // Optional supply: packs appropriate to the theme. Keep modest so it doesn't
        // outshine the cube-route resources.
        zone.AmmoPacks += 2.0;
        zone.ToolPacks += 1.0;
        zone.HealthPacks += 1.0;

        if (theme == CryptomnesiaTheme.InfectionFog)
            zone.DisinfectPacks += 3;

        _ = layout; _ = cubeBranch;
    }

    #endregion
}
