using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.Reactor;
using AutogenRundown.DataBlocks.ZoneData;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: ReactorStartup
 *
 *
 * Find and start up a reactor, fighting waves and optionally getting codes from zones.
 *
 * Note that when spawning waves, waves with capped total points should be used to
 * ensure the waves end when the team has finished fighting all of the enemies.
 *
 * This is quite a complicated objective to create with _a lot_ of balancing required to get it in
 * a fun place.
 *
 ***************************************************************************************************
 *      TODO List
 *
 *  - Exit Alarms?
 */
public partial record WardenObjective
{
    public void Build_ReactorStartup(BuildDirector director, Level level)
    {
        MainObjective = "Find the main reactor for the floor and make sure it is back online.";
        FindLocationInfo = "Gather information about the location of the Reactor";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = "Navigate to [ITEM_ZONE] and start the Reactor";
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find the reactor control panel and initiate the startup";
        SolveItem = "Make sure the Reactor is fully started before leaving";
        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        StartPuzzle = ChainedPuzzle.FindOrPersist(ChainedPuzzle.TeamScan);

        // Setup reactor waves. We should not override these so that level layout
        // can assign codes to some of these waves.

        var reactorWavePoints = 0;

        for (var w = 0; w < ReactorWaves.Count; w++)
        {
            var wave = ReactorWaves[w];
            wave.Warmup = 30.0;
            wave.WarmupFail = 30.0;
            wave.Verify = 30.0;
            wave.VerifyFail = 30; // TODO: check how other levels do it?

            // Set the reactor waves
            wave.EnemyWaves = (director.Tier, w) switch
            {
                // First wave is always a softball wave
                ("D", 0) => new() { ReactorEnemyWave.Baseline_Medium },
                ("E", 0) => new() { ReactorEnemyWave.Baseline_Hard },
                (_, 0) => new() { ReactorEnemyWave.Baseline_Easy },

                // TODO: BaselineMedium / Easy are far too easy alone

                // A-Tier
                // Should be relatively easy so the rest of the waves are medium.
                ("A", >= 1) => new() { ReactorEnemyWave.Baseline_Medium },

                // B-Tier
                //
                ("B", >= 1 and < 4) => new() { ReactorEnemyWave.Baseline_Medium },
                ("B", >= 4) => new() { ReactorEnemyWave.Baseline_Hard },

                #region C-Tier
                ("C", >= 1 and < 3) => new() { ReactorEnemyWave.Baseline_Medium },
                ("C", >= 3 and < 5) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyHybrids_Medium with { SpawnTime = 20 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 45 }
                        }),
                    }),
                ("C", >= 5) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 45 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Easy with { SpawnTime = 30 },
                            ReactorEnemyWave.SinglePouncer with { SpawnTime = 45 }
                        }),
                    }),
                #endregion

                #region D-Tier
                ("D", >= 1 and < 3) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (2.0, new() { ReactorEnemyWave.Baseline_Hard }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 35 }
                        }),
                    }),
                ("D", >= 3 and < 5) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyHybrids_Medium with { SpawnTime = 30 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 35 }
                        }),
                    }),
                ("D", >= 5 and < 7) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 45 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 60 },
                            ReactorEnemyWave.OnlyHybrids_Medium with { SpawnTime = 45 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyShadows_Hard with { SpawnTime = 30 }
                        }),
                        (1.0, new()
                        {
                            // TODO: Too easy
                            ReactorEnemyWave.BaselineWithNightmare_Hard,
                            ReactorEnemyWave.SingleTankPotato with { SpawnTime = 20 }
                        }),
                        (2.0, new()
                        {
                            ReactorEnemyWave.BaselineWithNightmare_Hard,
                            ReactorEnemyWave.BaselineWithChargers_Hard with { SpawnTime = 45 }
                        }),
                    }),
                ("D", >= 7) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (4.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 20 }
                        }),
                        (4.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.BaselineWithNightmare_Hard with { SpawnTime = 45 },
                            ReactorEnemyWave.OnlyShadows_Hard with { SpawnTime = 60 }
                        }),
                        (3.0, new()
                        {
                            // Good :+1:
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyHybrids_Medium with { SpawnTime = 55 },
                            ReactorEnemyWave.SingleTankPotato with { SpawnTime = 20 }
                        }),
                        (1.0, new()
                        {
                            // TBD: is this good?
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.SingleMother with { SpawnTime = 60 },
                        }),
                        (1.0, new()
                        {
                            // TBD: Added another baseline hard. Might be quite hard
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.Baseline_Hard with { SpawnTime = 70 },
                            ReactorEnemyWave.SingleTank with { SpawnTime = 60 },
                        }),
                    }),
                #endregion

                #region E-Tier
                ("E", >= 1 and < 4) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (2.0, new() { ReactorEnemyWave.Baseline_Hard }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 30 }
                        }),
                    }),
                ("E", >= 4 and < 8) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new() { ReactorEnemyWave.Baseline_Hard }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 30 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyShadows_Hard with { SpawnTime = 25 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.Baseline_Medium with { SpawnTime = 45 },
                            ReactorEnemyWave.Baseline_Medium with { SpawnTime = 75 }
                        }),
                    }),
                ("E", >= 8 and < 10) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (6.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 20 }
                        }),
                        (3.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyShadows_Hard with { SpawnTime = 10 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.SingleTank with { SpawnTime = 70 },
                            ReactorEnemyWave.Baseline_Medium with { SpawnTime = 60 }
                        }),
                    }),
                ("E", >= 10) => Generator.Select(
                    new List<(double, List<ReactorEnemyWave>)>
                    {
                        (6.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 20 }
                        }),
                        (3.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.OnlyShadows_Hard  with { SpawnTime = 10 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.SingleMother with { SpawnTime = 45 },
                            ReactorEnemyWave.Baseline_Medium with { SpawnTime = 90 }
                        }),
                        (1.0, new()
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.SingleTank with { SpawnTime = 60 },
                            ReactorEnemyWave.Baseline_Medium with { SpawnTime = 75 }
                        }),
                    }),
                #endregion

                (_, _) => new() { ReactorEnemyWave.Baseline_Easy }
            };

            wave.RecalculateWaveSpawnTimes();

            // Add wave fog flood for funsies
            // TODO: don't hard code this in every D-tier wave
            if (director.Tier == "D" && (w == 0 || w == ReactorWaves.Count - 1))
            {
                EventBuilder.AddFillFog(
                    wave.Events,
                    wave.Warmup + 13.0,
                    wave.Warmup + wave.Wave - 15.0);
                EventBuilder.AddClearFog(
                    wave.Events,
                    wave.Warmup + wave.Wave + 12.0,
                    20.0);
            }

            // Calculate how many points of enemies will be spawned in total.
            reactorWavePoints += wave.EnemyWaves.Sum(enemyWave
                => (int)(Bins.WaveSettings.Find(enemyWave.WaveSettings)?.PopulationPointsTotal ?? 0));
        }

        // Spread resources to do the waves within the reactor area
        var entrance = level.Planner.GetZone(
                level.Planner.GetZones(director.Bulkhead, "reactor_entrance").First())!;
        var reactor = level.Planner.GetZone(
            level.Planner.GetZones(director.Bulkhead, "reactor").First())!;

        var baseResourcesMulti = reactorWavePoints / 35.0;

        // Approximately how many health points per point of enemies do we expect to get.
        // Smalls are 20 health
        const double healthPerPoint = 30.0;

        // Median refill damage for main weapons
        const double mainMaxDamagePerRefill = 139.0;

        // median refill damage for special weapons
        const double specialMaxDamagePerRefill = 270.0;

        // Quite imbalanced, shotgun and hel auto only do 176 and 106 per refill. But instead we
        // balance this towards the Burst and Sniper sentries which do 325 and 322 per refill.
        const double toolMaxDamagePerRefill = 322.0;

        // Min ammo packs required to clear _all_ points
        var ammoPacks = reactorWavePoints * healthPerPoint /
                        (mainMaxDamagePerRefill + specialMaxDamagePerRefill);

        // Min tool packs required to clear _all_ points
        var toolPacks = reactorWavePoints * healthPerPoint / toolMaxDamagePerRefill;

        // Grant an extra 20% of the min ammo required to clear all waves. This is for missed
        // shots. Given there's enough tool to clear potentially 35% of the enemy points, combined
        // this should be a good amount to clear everything
        entrance.AmmoPacks += 1.2 * ammoPacks * 0.33;
        reactor.AmmoPacks += 1.2 * ammoPacks * 0.66;

        // Flat health per wave, in this case 3 uses per wave
        entrance.HealthPacks += 1 * ReactorWaves.Count;
        reactor.HealthPacks += 2 * ReactorWaves.Count;

        // We don't give enough tool to clear everything, enough to clear 35% of the points
        entrance.ToolPacks += 0.35 * toolPacks * 0.33;
        reactor.ToolPacks += 0.35 * toolPacks * 0.66;

        Plugin.Logger.LogDebug($"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead} -- "
                + $"ReactorStartup: {ReactorWaves.Count} waves, {reactorWavePoints}pts of enemies, assigned {baseResourcesMulti} resources "
                + $"New ammo metric = {ammoPacks}, new tool metric = {toolPacks} "
                + $"ammo packs = {entrance.AmmoPacks + reactor.AmmoPacks}, "
                + $"tool packs = {entrance.ToolPacks + reactor.ToolPacks}, "
                + $"health packs = {entrance.HealthPacks + reactor.HealthPacks}, ");

        // Multipliers to adjust the verify time
        // In general this is quite sensitive and the calculations actually get
        // quite close to a good number for a fun and challenging time. So the
        // scale factors don't need to be wildly different per tier.
        var (alarmMultiplier, coverageMultiplier, enemyMultiplier) = director.Tier switch
        {
            "A" => (1.40, 1.60, 1.60),
            "B" => (1.20, 1.50, 1.50),
            "C" => (1.10, 1.30, 1.30),
            "D" => (1.20, 1.25, 1.25), // Play testing around D-tier
            "E" => (1.15, 1.20, 1.20),

            _ => (1.4, 1.4, 1.4)
        };

        var fetchWaves = ReactorWaves.TakeLast(ReactorStartup_FetchWaves).ToList();

        // Adjust verify time for reactor waves that require fetching codes.
        for (var b = 0; b < fetchWaves.Count; b++)
        {
            var wave = fetchWaves[b];
            var branch = $"reactor_code_{b}";

            var branchNodes = level.Planner.GetZones(director.Bulkhead, branch);
            var branchZones = branchNodes.Select(node => level.Planner.GetZone(node))
                                         .Where(zone => zone != null)
                                         .OfType<Zone>()
                                         .ToList();

            foreach (var zone in branchZones)
                wave.Verify += zone.GetClearTimeEstimate();

            // // First add time based on the total area of the zones to be traversed
            // var coverageSum = branchZones.Sum(zone => zone.Coverage.Max);
            //
            // // Second, add time based on the total enemy points across the zones
            // var enemyPointsSum = branchZones.Sum(zone => zone.EnemySpawningInZone.Sum(spawn => spawn.Points));
            //
            // // Next, add time based on door alarms. Factor the scan time for each scan, time for the first scan
            // // to appear, and time between each scan to reach that next scan.
            // var alarmSum = branchZones.Sum(
            //     zone => 10 + zone.Alarm.Puzzle.Sum(component => component.Duration)
            //                + zone.Alarm.WantedDistanceFromStartPos
            //                + (zone.Alarm.Puzzle.Count - 1) * zone.Alarm.WantedDistanceBetweenPuzzleComponents);
            //
            // // Finally, add time based on blood doors. Flat +20s per blood door to be opened.
            // var bloodSum = branchZones.Sum(zone => zone.BloodDoor != BloodDoor.None ? 20 : 0);
            //
            // wave.Verify += alarmSum * alarmMultiplier;
            // wave.Verify += bloodSum;
            // wave.Verify += coverageSum * coverageMultiplier;
            // wave.Verify += enemyPointsSum * enemyMultiplier;

            // Add some grace time for failures on large branches
            wave.VerifyFail = Math.Max(30, wave.Verify / 2);

            Plugin.Logger.LogDebug($"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead} -- "
                + $"ReactorStartup: Fetch wave {b} has {wave.Verify}s time");
        }
    }
}
