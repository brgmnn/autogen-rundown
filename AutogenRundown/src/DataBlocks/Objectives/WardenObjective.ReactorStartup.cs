using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.Reactor;
using AutogenRundown.Extensions;

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
    public void PreBuild_ReactorStartup(BuildDirector director, Level level)
    {
        // Determine if we should make the users get codes
        ReactorStartupGetCodes = (director.Tier, director.Bulkhead, level.Settings.Bulkheads) switch
        {
            ("A", Bulkhead.Main,     Bulkhead.Main              ) => true,
            ("A", _,                 _                          ) => false,

            ("B", _,                 Bulkhead.PrisonerEfficiency) => false,
            ("B", Bulkhead.Main,     Bulkhead.Main              ) => true,
            ("B", Bulkhead.Overload, _                          ) => Generator.Flip(0.6),
            ("B", _,                 _                          ) => false,

            ("C", _,                 Bulkhead.PrisonerEfficiency) => false,
            ("C", Bulkhead.Main,     _                          ) => true,
            ("C", Bulkhead.Extreme,  _                          ) => false,
            ("C", Bulkhead.Overload, _                          ) => true,

            ("D", Bulkhead.Main,     _                          ) => true,
            ("D", _,                 Bulkhead.PrisonerEfficiency) => false,
            ("D", Bulkhead.Extreme,  _                          ) => Generator.Flip(0.4),
            ("D", Bulkhead.Overload, _                          ) => true,

            // Always have codes on E-tier.
            ("E", _, _) => true,

            _ => false,
        };

        var waveCount = (director.Tier, director.Bulkhead, level.Settings.Bulkheads) switch
        {
            ("A", Bulkhead.Main,     Bulkhead.Main              ) => 5,
            ("A", Bulkhead.Main,     Bulkhead.PrisonerEfficiency) => 3,
            ("A", Bulkhead.Main,     _                          ) => Generator.Between(4, 5),
            ("A", _,                 Bulkhead.PrisonerEfficiency) => 2,
            ("A", _,                 _                          ) => 3,

            ("B", Bulkhead.Main,     Bulkhead.Main              ) => Generator.Between(6, 7),
            ("B", Bulkhead.Main,     Bulkhead.PrisonerEfficiency) => 4,
            ("B", Bulkhead.Main,     _                          ) => Generator.Between(5, 6),
            ("B", _,                 Bulkhead.PrisonerEfficiency) => 3,
            ("B", Bulkhead.Overload, _                          ) => 4,
            ("B", _,                 _                          ) => 3,

            ("C", Bulkhead.Main,     Bulkhead.Main              ) => Generator.Between(7, 8),
            ("C", Bulkhead.Main,     Bulkhead.PrisonerEfficiency) => 5,
            ("C", Bulkhead.Main,     _                          ) => Generator.Between(6, 7),

            ("C", Bulkhead.Extreme,  Bulkhead.PrisonerEfficiency) => Generator.Between(3, 4),
            ("C", Bulkhead.Extreme,  _                          ) => Generator.Between(4, 5),
            ("C", Bulkhead.Overload, Bulkhead.PrisonerEfficiency) => 4,
            ("C", Bulkhead.Overload, _                          ) => 5,

            ("D", Bulkhead.Main,     Bulkhead.Main              ) => Generator.Between(9, 10),
            ("D", Bulkhead.Main,     Bulkhead.PrisonerEfficiency) => 5,
            ("D", Bulkhead.Main,     _                          ) => Generator.Between(7, 8),
            ("D", _,                 Bulkhead.PrisonerEfficiency) => 3,
            ("D", Bulkhead.Extreme,  _                          ) => Generator.Between(3, 4),
            ("D", Bulkhead.Overload, _                          ) => Generator.Between(4, 5),

            // ("E", Bulkhead.Main,     Bulkhead.Main              ) => 10,
            // ("E", Bulkhead.Main,     Bulkhead.PrisonerEfficiency) => 5,
            // ("E", Bulkhead.Main,     _                          ) => Generator.Between(6, 7),
            // ("E", _,                 Bulkhead.PrisonerEfficiency) => Generator.Between(3, 4),
            // ("E", _,                 _                          ) => Generator.Between(4, 5),
            ("E", _, _) => 5,

            _ => 1
        };

        var fetchCount = (director.Tier, director.Bulkhead, waveCount) switch
        {
            ("A", Bulkhead.Main, >= 4) => 2,

            ("B", Bulkhead.Main, >= 5) => 3,
            ("B", Bulkhead.Main,  < 5) => 2,
            ("B", _,             >= 4) => 2,
            ("B", _,              < 4) => 1,

            ("C", Bulkhead.Main, >= 7) => 4,
            ("C", Bulkhead.Main,  < 7) => 3,
            ("C", _,             >= 5) => 3,
            ("C", _,              < 5) => 2,

            ("D", Bulkhead.Main, >= 9) => 6,
            ("D", Bulkhead.Main, >= 7) => 5,
            ("D", Bulkhead.Main,  < 7) => 3,
            ("D", _,             >= 4) => 2,
            ("D", _,              < 4) => 1,

            ("E", Bulkhead.Main,   10) => 6,
            ("E", Bulkhead.Main, >= 6) => 4,
            ("E", Bulkhead.Main,  < 6) => 3,
            ("E", _,             >= 4) => 2,
            ("E", _,              < 4) => 1,

            _ => 1
        };

        // Initialize the reactor Waves with the correct number of waves, these
        // will be updated as we go.
        for (var i = 0; i < waveCount; ++i)
            ReactorWaves.Add(new ReactorWave());

        if (!ReactorStartupGetCodes) return;

        var candidateWaves = ReactorWaves.Skip(1).Take(ReactorWaves.Count - 2).ToList();

        // Always assign the last reactor wave to be a fetch wave
        ReactorWaves.Last().IsFetchWave = true;

        for (var i = 0; i < fetchCount - 1; i++)
        {
            var wave = Generator.Draw(candidateWaves);

            if (wave != null)
                wave.IsFetchWave = true;
        }

        Plugin.Logger.LogWarning($"What are my reactor waves: [" + string.Join(", ", ReactorWaves.Select(wave => $"Wave {{ IsFetchWave = {wave.IsFetchWave} }}")) + "]");
    }

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
            wave.EnemyWaves = (director.Tier, ReactorWaves.Count, w + 1) switch
            {
                // // First wave is always a softball wave
                // ("D", _, 0) => new() { ReactorEnemyWave.Baseline_Medium },
                // // ("E", 0) => new() { ReactorEnemyWave.Baseline_Hard },
                //
                // // TODO: BaselineMedium / Easy are far too easy alone
                //
                //
                // #region D-Tier
                // ("D", _, >= 1 and < 3) => Generator.Select(
                //     new List<(double, List<ReactorEnemyWave>)>
                //     {
                //         (2.0, new() { ReactorEnemyWave.Baseline_Hard }),
                //         (1.0, new()
                //         {
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 35 }
                //         }),
                //     }),
                // ("D", _, >= 3 and < 5) => Generator.Select(
                //     new List<(double, List<ReactorEnemyWave>)>
                //     {
                //         (1.0, new()
                //         {
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.OnlyHybrids_Medium with { SpawnTime = 30 }
                //         }),
                //         (1.0, new()
                //         {
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 35 }
                //         }),
                //     }),
                // ("D", _, >= 5 and < 7) => Generator.Select(
                //     new List<(double, List<ReactorEnemyWave>)>
                //     {
                //         (1.0, new()
                //         {
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 45 }
                //         }),
                //         (1.0, new()
                //         {
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 60 },
                //             ReactorEnemyWave.OnlyHybrids_Medium with { SpawnTime = 45 }
                //         }),
                //         (1.0, new()
                //         {
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.OnlyShadows_Hard with { SpawnTime = 30 }
                //         }),
                //         (1.0, new()
                //         {
                //             // TODO: Too easy
                //             ReactorEnemyWave.BaselineWithNightmare_Hard,
                //             ReactorEnemyWave.SingleTankPotato with { SpawnTime = 20 }
                //         }),
                //         (2.0, new()
                //         {
                //             ReactorEnemyWave.BaselineWithNightmare_Hard,
                //             ReactorEnemyWave.BaselineWithChargers_Hard with { SpawnTime = 45 }
                //         }),
                //     }),
                // ("D", _, >= 7) => Generator.Select(
                //     new List<(double, List<ReactorEnemyWave>)>
                //     {
                //         (4.0, new()
                //         {
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 20 }
                //         }),
                //         (4.0, new()
                //         {
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.BaselineWithNightmare_Hard with { SpawnTime = 45 },
                //             ReactorEnemyWave.OnlyShadows_Hard with { SpawnTime = 60 }
                //         }),
                //         (3.0, new()
                //         {
                //             // Good :+1:
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.OnlyHybrids_Medium with { SpawnTime = 55 },
                //             ReactorEnemyWave.SingleTankPotato with { SpawnTime = 20 }
                //         }),
                //         (1.0, new()
                //         {
                //             // TBD: is this good?
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.SingleMother with { SpawnTime = 60 },
                //         }),
                //         (1.0, new()
                //         {
                //             // TBD: Added another baseline hard. Might be quite hard
                //             ReactorEnemyWave.Baseline_Hard,
                //             ReactorEnemyWave.Baseline_Hard with { SpawnTime = 70 },
                //             ReactorEnemyWave.SingleTank with { SpawnTime = 60 },
                //         }),
                //     }),
                // #endregion

                #region A-Tier
                #region First wave
                ("A", _, 1) =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.33, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Easy,
                            ReactorEnemyWave.MiniBoss_6pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 40
                            }
                        }),
                        (0.66, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Easy,
                            ReactorEnemyWave.MiniBoss_6pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 40
                            }
                        }),
                    }),
                #endregion

                #region First half waves
                ("A", var total, var waveNum) when waveNum <= total / 2 && waveNum > 1 =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.40, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                        }),
                        (0.30, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Easy,
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 25
                            }
                        }),
                        (0.30, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Easy,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 45
                            }
                        }),
                    }),
                #endregion

                #region Second half waves
                ("A", var total, var waveNum) when waveNum > total / 2 && waveNum < total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.40, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                        }),
                        (0.30, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Easy,
                            ReactorEnemyWave.MiniBoss_6pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 25
                            }
                        }),
                        (0.20, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Easy,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 25
                            }
                        })
                    }),
                #endregion

                #region Last wave
                ("A", var total, var waveNum) when waveNum == total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.40, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_6pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 25
                            }
                        }),
                        (0.30, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 15
                            }
                        }),
                        (0.30, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.SingleTankPotato with { SpawnTime = 35 }
                        }),
                    }),
                #endregion
                #endregion

                #region B-Tier
                #region First wave
                ("B", _, 1) =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.33, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 35
                            }
                        }),
                        (0.66, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 40
                            }
                        }),
                    }),
                #endregion

                #region First half waves
                ("B", var total, var waveNum) when waveNum <= total / 2 && waveNum > 1 =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.10, new List<ReactorEnemyWave> { ReactorEnemyWave.Baseline_Hard }),
                        (0.30, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Easy,
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 35
                            },
                        }),
                        (0.25, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 25
                            }
                        }),
                        (0.25, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 45
                            }
                        }),
                        (0.10, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 30
                            },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 45
                            }
                        }),
                    }),
                #endregion

                #region Second half waves
                ("B", var total, var waveNum) when waveNum > total / 2 && waveNum < total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.10, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Easy,
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 35
                            },
                        }),
                        (0.25, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 45
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 0.10 : 0.25, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 20
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 0.15 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 20
                            }
                        }),
                        (0.40, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 30
                            },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 45
                            }
                        }),
                    }),
                #endregion

                #region Last wave
                ("B", var total, var waveNum) when waveNum == total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.35, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_6pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 10
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 30
                            },
                            ReactorEnemyWave.MiniBoss_6pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 45
                            }
                        }),
                        (0.45, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 10
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 45
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 0.15 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 15
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 32
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 0.05 : 0.20, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium,
                            ReactorEnemyWave.SingleTankPotato with { SpawnTime = 35 }
                        }),
                    }),
                #endregion
                #endregion

                #region C-Tier
                #region First wave
                ("C", _, 1) =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.33, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 35
                            }
                        }),
                        (0.66, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 40
                            }
                        }),
                    }),
                #endregion

                #region First half waves
                ("C", var total, var waveNum) when waveNum <= total / 2 && waveNum > 1 =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard
                        }),
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 35
                            }
                        }),
                        (level.Settings.HasChargers() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium with { Population = WavePopulation.Baseline_Chargers }
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium with { Population = WavePopulation.Baseline_Nightmare }
                        }),
                        (level.FogSettings.IsInfectious ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 35
                            }
                        })
                    }),
                #endregion

                #region Second half waves
                ("C", var total, var waveNum) when waveNum > total / 2 && waveNum < total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 35
                            }
                        }),
                        (level.Settings.HasChargers() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium with { Population = WavePopulation.OnlyChargers }
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium with { Population = WavePopulation.OnlyNightmares }
                        }),
                        (level.FogSettings.IsInfectious ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Medium with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 20
                            }
                        })
                    }),
                #endregion

                #region Last wave
                ("C", var total, var waveNum) when waveNum == total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.35, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 10
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 30
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 45
                            }
                        }),
                        (0.45, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 10
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 45
                            }
                        }),
                        (0.45, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 10
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 34
                            },
                            ReactorEnemyWave.SingleTankPotato with { SpawnTime = 55 }
                        }),
                        (level.Settings.HasChargers() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Chargers },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyChargers,
                                SpawnTime = 25
                            },
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Nightmare },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyNightmareGiants,
                                SpawnTime = 25
                            },
                        }),
                        (level.FogSettings.IsInfectious ? 0.5 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 15
                            }
                        })
                    }),
                #endregion
                #endregion

                #region D-Tier
                #region First wave
                ("D", _, 1) =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.33, new List<ReactorEnemyWave> { ReactorEnemyWave.Baseline_SurgeHard }),
                        (0.33, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 35
                            }
                        }),
                        (0.33, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 40
                            }
                        }),
                    }),
                #endregion

                #region First half waves
                ("D", var total, var waveNum) when waveNum <= total / 2 && waveNum > 1 =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new List<ReactorEnemyWave> { ReactorEnemyWave.Baseline_VeryHard }),
                        (1.0, new List<ReactorEnemyWave> { ReactorEnemyWave.Baseline_SurgeHard }),
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 35
                            }
                        }),
                        (level.Settings.HasChargers() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Chargers }
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Nightmare }
                        }),
                        (level.Settings.HasShadows() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.Baseline_Shadows },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                SpawnTime = 45
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 35
                            }
                        })
                    }),
                #endregion

                #region Second half waves
                ("D", var total, var waveNum) when waveNum > total / 2 && waveNum < total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new List<ReactorEnemyWave> { ReactorEnemyWave.Baseline_SurgeHard }),
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 35
                            }
                        }),
                        (level.Settings.HasChargers() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.OnlyChargers }
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.OnlyNightmares }
                        }),
                        (level.Settings.HasShadows() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.OnlyShadows },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                SpawnTime = 45
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                AreaDistance = 1,
                                SpawnTime = 60
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.Baseline_InfectedHybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 35
                            }
                        })
                    }),
                #endregion

                #region Last wave
                ("D", var total, var waveNum) when waveNum == total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.35, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard,
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 10
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 45
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 65
                            }
                        }),
                        (0.45, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 15
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 40
                            },
                            ReactorEnemyWave.SingleTankPotato with { SpawnTime = 65 }
                        }),
                        (level.Settings.HasChargers() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Chargers },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyChargers,
                                SpawnTime = 25
                            },
                            ReactorEnemyWave.SingleTank with { SpawnTime = 65 }
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Nightmare },
                            new()
                            {
                                Settings = WaveSettings.SingleWave_20pts,
                                Population = WavePopulation.OnlyFlyers,
                                SpawnTime = 20
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyNightmareGiants,
                                SpawnTime = 45
                            },
                        }),
                        (level.Settings.HasShadows() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_Hard with { Population = WavePopulation.Baseline_Shadows },
                            new()
                            {
                                Settings = WaveSettings.SingleWave_28pts,
                                Population = WavePopulation.OnlyShadows,
                                SpawnTime = 20
                            },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                SpawnTime = 45
                            },
                            ReactorEnemyWave.SingleShadowPouncer with { SpawnTime = 35 },
                            ReactorEnemyWave.SingleShadowPouncer with { SpawnTime = 65, Duration = 0 }
                        }),
                        (level.FogSettings.IsInfectious ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with
                            {
                                Population = WavePopulation.Baseline_InfectedHybrids
                            },
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 25
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 0.5 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with
                            {
                                Population = WavePopulation.Baseline_InfectedHybrids
                            },
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 25
                            },
                            ReactorEnemyWave.SingleMother with { SpawnTime = 80 }
                        })
                    }),
                #endregion
                #endregion

                #region E-Tier (TODO)
                #region First wave
                ("E", _, 1) => Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                {
                    // (0.33, new() { ReactorEnemyWave.Baseline_SurgeVeryHard }),
                    // (0.33, new()
                    // {
                    //     ReactorEnemyWave.Baseline_SurgeHard,
                    //     ReactorEnemyWave.MiniBoss_12pts with { Population = WavePopulation.OnlyHybrids, SpawnTime = 70 }
                    // }),
                    (0.33, new()
                    {
                        // ReactorEnemyWave.Baseline_SurgeHard,
                        // ReactorEnemyWave.MiniBoss_12pts with { Population = WavePopulation.OnlyInfectedHybrids, SpawnTime = 70 }

                        ReactorEnemyWave.MiniBoss_12pts with { Population = WavePopulation.OnlyInfectedHybrids, Duration = 20 }
                    })
                }),
                #endregion

                #region First half waves
                ("E", var total, var waveNum) when waveNum <= total / 2 && waveNum > 1 => Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                {
                    (0.4, new()
                    {
                        ReactorEnemyWave.OnlyChargers_Hard with { Duration = 20 }
                        // ReactorEnemyWave.Baseline_Hard,
                        // ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 20 }
                    }),
                    // (0.4, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.OnlyShadows_Hard  with { SpawnTime = 10 }
                    // }),
                    // (0.2, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.SingleMother with { SpawnTime = 45 },
                    //     ReactorEnemyWave.Baseline_Medium with { SpawnTime = 90 }
                    // }),
                    // (0.2, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.SingleTank with { SpawnTime = 60 },
                    //     ReactorEnemyWave.Baseline_Medium with { SpawnTime = 75 }
                    // }),
                }),
                #endregion

                #region Second half waves
                ("E", var total, var waveNum) when waveNum > total / 2 && waveNum < total => Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                {
                    (0.4, new()
                    {
                        ReactorEnemyWave.OnlyShadows_Easy with { Duration = 20 }
                        // ReactorEnemyWave.Baseline_Hard,
                        // ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 20 }
                    }),
                    // (0.4, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.OnlyShadows_Hard  with { SpawnTime = 10 }
                    // }),
                    // (0.2, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.SingleMother with { SpawnTime = 45 },
                    //     ReactorEnemyWave.Baseline_Medium with { SpawnTime = 90 }
                    // }),
                    // (0.2, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.SingleTank with { SpawnTime = 60 },
                    //     ReactorEnemyWave.Baseline_Medium with { SpawnTime = 75 }
                    // }),
                }),
                #endregion

                #region Last wave
                ("E", var total, var waveNum) when waveNum == total => Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                {
                    // (0.4, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.OnlyChargers_Hard with { SpawnTime = 20 }
                    // }),
                    // (0.4, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.OnlyShadows_Hard  with { SpawnTime = 10 }
                    // }),
                    // (0.2, new()
                    // {
                    //     ReactorEnemyWave.Baseline_Hard,
                    //     ReactorEnemyWave.SingleMother with { SpawnTime = 45 },
                    //     ReactorEnemyWave.Baseline_Medium with { SpawnTime = 90 }
                    // }),
                    (0.2, new()
                    {
                        ReactorEnemyWave.SingleTank with { Duration = 20 }
                        // ReactorEnemyWave.Baseline_Hard,
                        // ReactorEnemyWave.SingleTank with { SpawnTime = 60 },
                        // ReactorEnemyWave.Baseline_Medium with { SpawnTime = 75 }
                    }),
                }),
                #endregion
                #endregion

                (_, _, _) => new() { ReactorEnemyWave.Baseline_Easy }
            };

            wave.RecalculateWaveSpawnTimes();

            var fog = level.FogSettings;
            var isInfectious = level.FogSettings.Infection > 0.01;

            // Add wave fog flood for funsies
            // TODO: don't hard code this, use the random generator
            if (director.Tier == "C" && (w == 1) ||
                director.Tier == "D" && (w == 0 || w == ReactorWaves.Count - 1) ||
                director.Tier == "E" && (w == 1 || w == ReactorWaves.Count - 1))
            {
                wave.Events
                    .AddSetFog(
                        isInfectious ? Fog.FullFog_Infectious : Fog.FullFog,
                        wave.Warmup + 13.0,
                        wave.Warmup + wave.Wave - 15.0)
                    .AddSetFog(
                        fog,
                        wave.Warmup + wave.Wave + 12.0,
                        21.0,
                        "VENTILATION SYSTEM REBOOTED - SYSTEMS ONLINE");
            }

            // Calculate how many points of enemies will be spawned in total.
            reactorWavePoints += wave.EnemyWaves.Sum(enemyWave
                => (int)enemyWave.Settings.PopulationPointsTotal);
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

        // Grant an extra 30% of the min ammo required to clear all waves. This is for missed
        // shots. Given there's enough tool to clear potentially 35% of the enemy points, combined
        // this should be a good amount to clear everything
        entrance.AmmoPacks += 1.3 * ammoPacks * 0.33;
        reactor.AmmoPacks += 1.3 * ammoPacks * 0.66;

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

        // TODO: Clean up the old resourcing code
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

        var fetchWaves = ReactorWaves.Where(wave => wave.IsFetchWave).ToList();

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
