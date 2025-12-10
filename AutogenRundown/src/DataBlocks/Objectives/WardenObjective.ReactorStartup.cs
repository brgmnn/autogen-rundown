using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.Reactor;
using AutogenRundown.Extensions;
using AutogenRundown.Patches;

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
    private void PreBuild_ReactorStartup(BuildDirector director, Level level)
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

            ("E", Bulkhead.Main,     Bulkhead.Main              ) => 10,
            ("E", Bulkhead.Main,     Bulkhead.PrisonerEfficiency) => 5,
            ("E", Bulkhead.Main,     _                          ) => Generator.Between(6, 7),
            ("E", _,                 Bulkhead.PrisonerEfficiency) => Generator.Between(3, 4),
            ("E", _,                 _                          ) => Generator.Between(4, 5),

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

            ("D", Bulkhead.Main, >= 9) => 5,
            ("D", Bulkhead.Main, >= 7) => 4,
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

        var minFogWaves = 0;
        var maxFogWaves = director.Tier switch
        {
            "D" => 2,
            "E" => 3,
            _ => 1
        };

        if (level.Settings.Modifiers.Contains(LevelModifiers.Fog))
        {
            minFogWaves = Generator.Between(0, 1);
            maxFogWaves += Generator.Select(new List<(double, int)>
            {
                (0.2, 0),
                (0.8, 1)
            });
        }
        else if (level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
        {
            minFogWaves = 1;
            maxFogWaves += Generator.Select(new List<(double, int)>
            {
                (0.2, 0),
                (0.6, 1),
                (0.2, 2)
            });
        }

        maxFogWaves = Math.Min(maxFogWaves, waveCount - 1);
        var fogWaveCount = Generator.Between(minFogWaves, Math.Max(minFogWaves, maxFogWaves));

        Plugin.Logger.LogDebug($"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead} -- " +
                               $"Fog Waves = {fogWaveCount} (Min = {minFogWaves}, Max = {maxFogWaves})");

        // Initialize the reactor Waves with the correct number of waves, these
        // will be updated as we go.
        for (var i = 0; i < waveCount; ++i)
            ReactorWaves.Add(new ReactorWave());

        #region Fog Waves
        /*
         * Fog waves will flood the level with fog on wave startup and then return the fog back
         * to the level default on finish.
         *
         * First wave is never a fog wave. The remaining waves are randomly assigned fog.
         */
        var candidateFogWaves = ReactorWaves.Skip(1).ToList();

        for (var i = 0; i < fogWaveCount - 1; i++)
        {
            var wave = Generator.Draw(candidateFogWaves);

            if (wave != null)
                wave.IsFogWave = true;
        }
        #endregion

        #region Fetch Waves
        /*
         * Fetch waves require going to a terminal to fetch the code
         *
         * First wave is never a fetch wave, last wave is always a fetch wave. The remaining
         * codes to be fetched are distributed randomly amongst the middle waves.
         */
        if (ReactorStartupGetCodes)
        {
            var candidateWaves = ReactorWaves.Skip(1).Take(ReactorWaves.Count - 2).ToList();

            // Always assign the last reactor wave to be a fetch wave
            ReactorWaves.Last().IsFetchWave = true;

            for (var i = 0; i < fetchCount - 1; i++)
            {
                var wave = Generator.Draw(candidateWaves);

                if (wave != null)
                    wave.IsFetchWave = true;
            }
        }
        #endregion
    }

    private void Build_ReactorStartup(BuildDirector director, Level level)
    {
        MainObjective = new Text("Find the main reactor for the floor and make sure it is back online.");
        FindLocationInfo = "Gather information about the location of the Reactor";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = new Text("Navigate to [ITEM_ZONE] and start the Reactor");
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find the reactor control panel and initiate the startup";
        SolveItem = "Make sure the Reactor is fully started before leaving";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
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
            wave.VerifyFail = 30;

            // Set the reactor waves
            wave.EnemyWaves = (director.Tier, ReactorWaves.Count, w + 1) switch
            {
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
                            ReactorEnemyWave.MiniBoss_12pts with
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
                            ReactorEnemyWave.MiniBoss_8pts with
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
                        (0.20, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeMedium,
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
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeHard with { Population = WavePopulation.Baseline_Infested },
                            ReactorEnemyWave.Baseline_Easy with
                            {
                                Population = WavePopulation.OnlyInfestedStrikers,
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
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeHard with { Population = WavePopulation.Baseline_Infested },
                            ReactorEnemyWave.Baseline_Medium with
                            {
                                Population = WavePopulation.OnlyInfestedStrikers,
                                SpawnTime = 25
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 60
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

                #region E-Tier
                #region First wave
                ("E", _, 1) =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.33, new List<ReactorEnemyWave> { ReactorEnemyWave.Baseline_SurgeVeryHard }),
                        (0.25, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard,
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                SpawnTime = 35
                            },
                        }),
                        (0.08, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 40
                            },
                            ReactorEnemyWave.SinglePouncer with { SpawnTime = 60, Duration = 0 },
                        }),
                        (0.25, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard,
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 40
                            },
                        }),
                        (0.08, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard,
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                SpawnTime = 40
                            },
                            ReactorEnemyWave.SinglePouncer with { SpawnTime = 60, Duration = 0 },
                        }),
                    }),
                #endregion

                #region First half waves
                ("E", var total, var waveNum) when waveNum <= total / 2 && waveNum > 1 =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new List<ReactorEnemyWave> { ReactorEnemyWave.Baseline_SurgeVeryHard }),
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 20
                            },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 65
                            }
                        }),
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeHard with { Population = WavePopulation.Baseline_Infested },
                            ReactorEnemyWave.Baseline_Medium with
                            {
                                Population = WavePopulation.OnlyInfestedStrikers,
                                SpawnTime = 35
                            }
                        }),
                        (level.Settings.HasChargers() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.Baseline_Chargers }
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.Baseline_Nightmare }
                        }),
                        (level.Settings.HasShadows() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeVeryHard with { Population = WavePopulation.Baseline_Shadows },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                SpawnTime = 45
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.Baseline_InfectedHybrids },
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 35
                            }
                        })
                    }),
                #endregion

                #region Second half waves
                ("E", var total, var waveNum) when waveNum > total / 2 && waveNum < total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeHard with { Population = WavePopulation.Baseline_Hybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 55
                            }
                        }),
                        (1.0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeHard with { Population = WavePopulation.Baseline_Infested },
                            ReactorEnemyWave.Baseline_Medium with
                            {
                                Population = WavePopulation.OnlyInfestedStrikers,
                                SpawnTime = 25
                            },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyHybrids,
                                SpawnTime = 55
                            }
                        }),
                        (level.Settings.HasChargers() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.OnlyChargers },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyChargers,
                                SpawnTime = 35
                            },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyChargers,
                                SpawnTime = 65,
                                Duration = 20
                            }
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.OnlyNightmares },
                            new()
                            {
                                Settings = WaveSettings.SingleWave_20pts,
                                Population = WavePopulation.OnlyFlyers,
                                SpawnTime = 20
                            },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyNightmareGiants,
                                SpawnTime = 35
                            }
                        }),
                        (level.Settings.HasShadows() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_VeryHard with { Population = WavePopulation.OnlyShadows },
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                SpawnTime = 20
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                AreaDistance = 1,
                                SpawnTime = 50
                            }
                        }),
                        (level.FogSettings.IsInfectious ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeHard with { Population = WavePopulation.Baseline_InfectedHybrids },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 15
                            },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 55
                            }
                        })
                    }),
                #endregion

                #region Last wave
                ("E", var total, var waveNum) when waveNum == total =>
                    Generator.Select(new List<(double, List<ReactorEnemyWave>)>
                    {
                        (0.35, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeVeryHard,
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
                            ReactorEnemyWave.Baseline_SurgeVeryHard,
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
                            ReactorEnemyWave.Baseline_SurgeVeryHard with { Population = WavePopulation.Baseline_Chargers },
                            ReactorEnemyWave.MiniBoss_12pts with
                            {
                                Population = WavePopulation.OnlyChargers,
                                SpawnTime = 25
                            },
                            ReactorEnemyWave.SingleTank with { SpawnTime = 45 }
                        }),
                        (level.Settings.HasNightmares() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeVeryHard with { Population = WavePopulation.Baseline_Nightmare },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyNightmareGiants,
                                SpawnTime = 15
                            },
                            new()
                            {
                                Settings = WaveSettings.SingleWave_28pts,
                                Population = WavePopulation.OnlyFlyers,
                                SpawnTime = 35
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyNightmareGiants,
                                SpawnTime = 65
                            },
                        }),
                        (level.Settings.HasShadows() ? 1.0 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeVeryHard with { Population = WavePopulation.Baseline_Shadows },
                            new()
                            {
                                Settings = WaveSettings.SingleWave_28pts,
                                Population = WavePopulation.OnlyShadows,
                                SpawnTime = 20
                            },
                            ReactorEnemyWave.MiniBoss_16pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                SpawnTime = 20
                            },
                            ReactorEnemyWave.MiniBoss_8pts with
                            {
                                Population = WavePopulation.OnlyShadows,
                                AreaDistance = 1,
                                SpawnTime = 50
                            },
                            ReactorEnemyWave.SingleShadowPouncer with { SpawnTime = 25 },
                            ReactorEnemyWave.SingleShadowPouncer with { SpawnTime = 65, Duration = 0 }
                        }),
                        (level.FogSettings.IsInfectious ? 0.5 : 0, new List<ReactorEnemyWave>
                        {
                            ReactorEnemyWave.Baseline_SurgeVeryHard with
                            {
                                Population = WavePopulation.Baseline_InfectedHybrids
                            },
                            ReactorEnemyWave.MiniBoss_24pts with
                            {
                                Population = WavePopulation.OnlyInfectedHybrids,
                                SpawnTime = 20
                            },
                            ReactorEnemyWave.SingleMother with { SpawnTime = 80 }
                        })
                    }),
                #endregion
                #endregion

                (_, _, _) => new List<ReactorEnemyWave> { ReactorEnemyWave.Baseline_Easy }
            };

            wave.RecalculateWaveSpawnTimes();

            var fog = level.FogSettings;
            var isInfectious = level.FogSettings.IsInfectious;

            if (wave.IsFogWave)
            {
                var inverted = fog.IsInverted;
                var tempFog = (isInfectious, inverted) switch
                {
                    (false, false) => Fog.Normal_Altitude_8,
                    (false, true ) => Fog.Inverted_Altitude_minus8,
                    (true,  false) => Fog.NormalInfectious_Altitude_8,
                    (true,  true ) => Fog.InvertedInfectious_Altitude_minus8
                };

                wave.Events
                    .AddSetFog(tempFog,wave.Warmup + 13.0, wave.Warmup + wave.Wave - 15.0)
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
        }

        Plugin.Logger.LogDebug($"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead} -- " +
                                 $"Reactor Waves: [" + ReactorWave.ListToString(ReactorWaves, ",\n  ") + "]");
    }

    private void PostBuildIntel_ReactorStartup(Level level)
    {
        #region Warden Intel Messages

        // Intel variables
        var verifyCode = Generator.Pick(level.Tier switch
        {
            "C" => TerminalUplink.FiveLetterWords,
            "D" => TerminalUplink.SixLetterWords,
            "E" => TerminalUplink.SevenLetterWords,
            _ => TerminalUplink.FourLetterWords
        })!.ToUpperInvariant();
        var waveCount = ReactorWaves.Count;
        var thirdWaves = Math.Min(Math.Max(1, waveCount / 3), waveCount - 1);
        var twoThirdsWaves = waveCount - thirdWaves;
        var halfWaves = Math.Min(Math.Max(1, waveCount / 2), waveCount - 1);
        var nextWave = thirdWaves + 1;

        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
        {
            ">... REACTOR_STARTUP entered.\r\n>... <size=200%><color=red>Here they come!</color></size>\r\n>... [gunfire]",
            ">... Wave four incoming!\r\n>... <size=200%><color=red>Where's the verification code?!</color></size>\r\n>... [frantic typing]",
            ">... The verification timer!\r\n>... <size=200%><color=red>We're out of time!</color></size>\r\n>... [alarm blaring]",
            ">... [breathing heavily] I'm out of ammo!\r\n>... <size=200%><color=red>Hold position!</color></size>\r\n>... They're everywhere!",
            $">... REACTOR_VERIFY {verifyCode}\r\n>... <size=200%><color=red>It worked!</color></size>\r\n>... Next wave in thirty...",
            $">... How many waves left?!\r\n>... <size=200%><color=red>{twoThirdsWaves.ToCardinal().ToTitleCase()}!</color></size>\r\n>... [groaning]",
            ">... Giants incoming!\r\n>... Focus fire!\r\n>... <size=200%><color=red>He's down!</color></size>",
            ">... Wave timer's running!\r\n>... <size=200%><color=red>Thirty seconds!</color></size>\r\n>... Almost done!",
            ">... [static]\r\n>... Reactor's starting up.\r\n>... <size=200%><color=red>Brace for contact!</color></size>",
            ">... Pre-type the command!\r\n>... <size=200%><color=red>REACTOR_VERIFY...</color></size>\r\n>... Ready!",
            ">... Missed the timer!\r\n>... Wave's resetting!\r\n>... <size=200%><color=red>Fuck!</color></size>",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave defense starting!</color></size>\r\n>... [gunfire]",
            $">... {waveCount.ToCardinal().ToTitleCase()} waves total.\r\n>... <size=200%><color=red>This'll take forever!</color></size>\r\n>... Stay focused!",
            ">... Chargers incoming!\r\n>... <size=200%><color=red>Kite them!</color></size>\r\n>... [footsteps]",
            ">... Warmup phase!\r\n>... <size=200%><color=red>Thirty seconds!</color></size>\r\n>... Get ready!",
            ">... Shooter Giants!\r\n>... Take cover!\r\n>... <size=200%><color=red>Behind the pillar!</color></size>",
            ">... [mechanical whirring]\r\n>... Reactor's powering up.\r\n>... <size=200%><color=red>Wave incoming!</color></size>",
            ">... Hybrids!\r\n>... <size=200%><color=red>Blood door!</color></size>\r\n>... Keep shooting!",
            $">... Verification in ten!\r\n>... <size=200%><color=red>Code ready?!</color></size>\r\n>... \"{verifyCode}\"!",
            ">... [coughing] Running low...\r\n>... <size=200%><color=red>Ammo crate!</color></size>\r\n>... Where?!",
            // ">... Terminal's locked!\r\n>... Hack it!\r\n>... <size=200%><color=red>No time!</color></size>",
            $">... Wave {nextWave.ToCardinal()} starting!\r\n>... <size=200%><color=red>Brace!</color></size>\r\n>... [alarm blaring]",
            ">... [breathing heavily] Last wave!\r\n>... <size=200%><color=red>Hold position!</color></size>\r\n>... [gunfire]",
            ">... Giant Strikers!\r\n>... <size=200%><color=red>Focus fire!</color></size>\r\n>... [gunfire intensifies]",
            ">... Verification timer's short!\r\n>... <size=200%><color=red>Twenty-five seconds!</color></size>\r\n>... Go now!",
            $">... [static]\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Verified!",
            $">... Wave {thirdWaves.ToCardinal()} complete.\r\n>... <size=200%><color=red>{twoThirdsWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [groaning]",
            ">... They're spawning behind us!\r\n>... <size=200%><color=red>Turn around!</color></size>\r\n>... [screaming]",
            ">... Reactor's at fifty percent!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [mechanical sounds]",
            ">... Tank incoming!\r\n>... <size=200%><color=red>Scatter!</color></size>\r\n>... [heavy footsteps]",
            ">... Verification failed!\r\n>... <size=200%><color=red>Wave reset!</color></size>\r\n>... Damn it!",
            ">... [alarm blaring]\r\n>... Wave timer!\r\n>... <size=200%><color=red>Sixty seconds!</color></size>",
            ">... [gunfire]\r\n>... <size=200%><color=red>Reloading!</color></size>\r\n>... Cover me!",
            $">... Wave {nextWave.ToCardinal()} starting.\r\n>... <size=200%><color=red>Here we go again!</color></size>\r\n>... [breathing heavily]",
            ">... Pre-typed the command!\r\n>... <size=200%><color=red>Just need the code!</color></size>\r\n>... Waiting!",
            ">... Mother spawning!\r\n>... <size=200%><color=red>Take her down!</color></size>\r\n>... [gunfire intensifies]",
            ">... [coughing] Need health!\r\n>... <size=200%><color=red>Health pack!</color></size>\r\n>... Where?!",
            ">... Verification timer running!\r\n>... <size=200%><color=red>Ten seconds!</color></size>\r\n>... Hurry!",
            $">... Wave {waveCount.ToCardinal()} incoming!\r\n>... <size=200%><color=red>Last one!</color></size>\r\n>... Almost there!",
            ">... Shadows incoming!\r\n>... They're teleporting!\r\n>... <size=200%><color=red>Watch your back!</color></size>",
            ">... [breathing heavily] Out of special ammo!\r\n>... <size=200%><color=red>Use your main!</color></size>\r\n>... [gunfire]",
            $">... Verification in five!\r\n>... <size=200%><color=red>Code?!</color></size>\r\n>... \"{verifyCode}\"!",
            ">... [mechanical whirring]\r\n>... <size=200%><color=red>Wave starting!</color></size>\r\n>... Get ready!",
            ">... Warmup timer!\r\n>... Thirty seconds!\r\n>... <size=200%><color=red>Check ammo!</color></size>",
            ">... Giants and Chargers!\r\n>... <size=200%><color=red>Prioritize Giants!</color></size>\r\n>... On it!",
            ">... Wave timer expired!\r\n>... <size=200%><color=red>Verify now!</color></size>\r\n>... [frantic typing]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Contact!</color></size>\r\n>... [gunfire]",
            // ">... Terminal's in Zone 24!\r\n>... <size=200%><color=red>That's deep!</color></size>\r\n>... Go solo!",
            $">... [panting] Wave {thirdWaves.ToCardinal()} done!\r\n>... <size=200%><color=red>{twoThirdsWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [groaning]",
            ">... Shooter wave!\r\n>... <size=200%><color=red>Take cover!</color></size>\r\n>... Behind the crate!",
            ">... Reactor's at seventy-five percent!\r\n>... <size=200%><color=red>One more wave!</color></size>\r\n>... Hold on!",
            ">... [static]\r\n>... <size=200%><color=red>Verification complete!</color></size>\r\n>... Next wave!",
            ">... Missed the code!\r\n>... <size=200%><color=red>Wave's resetting!</color></size>\r\n>... Again?!",
            ">... [breathing heavily] Need tool refill!\r\n>... <size=200%><color=red>Tool station!</color></size>\r\n>... Where?!",
            $">... Wave one complete!\r\n>... <size=200%><color=red>{(waveCount - 1).ToCardinal().ToTitleCase()} to go!</color></size>\r\n>... [panting]",
            $">... Verification code on HUD!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Typing!",
            ">... [gunfire intensifies]\r\n>... They're overwhelming us!\r\n>... <size=200%><color=red>Fall back!</color></size>",
            ">... Reactor defense starting!\r\n>... <size=200%><color=red>Brace for waves!</color></size>\r\n>... [alarm blaring]",
            ">... [coughing] Running low on health!\r\n>... <size=200%><color=red>Heal up!</color></size>\r\n>... No packs left!",
            ">... Wave timer!\r\n>... <size=200%><color=red>Ninety seconds!</color></size>\r\n>... Long one!",
            $">... Verification ready!\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Done!",
            ">... Tank and Giants!\r\n>... <size=200%><color=red>Focus the Tank!</color></size>\r\n>... [gunfire]",
            $">... Wave {waveCount.ToCardinal()} starting!\r\n>... This is it!\r\n>... <size=200%><color=red>Final wave!</color></size>",
            ">... Verification timer!\r\n>... <size=200%><color=red>Fifteen seconds!</color></size>\r\n>... Hurry up!",
            ">... [mechanical sounds]\r\n>... Reactor's progressing!\r\n>... <size=200%><color=red>Keep defending!</color></size>",
            ">... Charger wave!\r\n>... <size=200%><color=red>Hold them!</color></size>\r\n>... Keep moving!",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave defense!</color></size>\r\n>... [gunfire]",
            ">... Hybrid wave incoming!\r\n>... Blood door opening!\r\n>... <size=200%><color=red>They're fast!</color></size>",
            // ">... Wave five starting!\r\n>... <size=200%><color=red>Halfway there!</color></size>\r\n>... [breathing heavily]",
            ">... Verification failed!\r\n>... Timer expired!\r\n>... <size=200%><color=red>Wave reset!</color></size>",
            ">... [panting] Can't hold much longer!\r\n>... <size=200%><color=red>One more wave!</color></size>\r\n>... [gunfire]",
            ">... Reactor's at twenty-five percent!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [mechanical whirring]",
            ">... Wave timer expired!\r\n>... <size=200%><color=red>Verify!</color></size>\r\n>... [frantic typing]",
            ">... [gunfire]\r\n>... Giant down!\r\n>... <size=200%><color=red>Two more!</color></size>",
            ">... [breathing heavily] Out of mines!\r\n>... <size=200%><color=red>Use C-Foam!</color></size>\r\n>... On it!",
            $">... Wave {thirdWaves.ToCardinal()} starting!\r\n>... <size=200%><color=red>Here they come!</color></size>\r\n>... [alarm blaring]",
            $">... Verification in twenty!\r\n>... Code ready?\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Reactor progressing!</color></size>\r\n>... Next wave!",
            ">... Giant Shooters incoming!\r\n>... <size=200%><color=red>Take cover!</color></size>\r\n>... [gunfire]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave starting!</color></size>\r\n>... Brace!",
            ">... Verification complete!\r\n>... <size=200%><color=red>Next wave incoming!</color></size>\r\n>... [breathing heavily]",
            ">... [coughing] Fog wave!\r\n>... <size=200%><color=red>Can't see!</color></size>\r\n>... Use trackers!",
            ">... Wave timer!\r\n>... <size=200%><color=red>Two minutes!</color></size>\r\n>... Long wave!",
            ">... Tank spawning!\r\n>... <size=200%><color=red>Break line of sight!</color></size>\r\n>... [heavy footsteps]",
            $">... [breathing heavily] Terminal found!\r\n>... <size=200%><color=red>What's the code?!</color></size>\r\n>... \"{verifyCode}\"!",
            ">... Reactor's at ninety percent!\r\n>... <size=200%><color=red>Almost done!</color></size>\r\n>... One wave!",
            ">... Verification timer!\r\n>... <size=200%><color=red>Five seconds!</color></size>\r\n>... [frantic typing]",
            $">... Wave {twoThirdsWaves.ToCardinal()} complete!\r\n>... <size=200%><color=red>{thirdWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [panting]",
            // ">... Shadow wave!\r\n>... They're teleporting!\r\n>... <size=200%><color=red>Flashbangs!</color></size>",
            ">... [gunfire]\r\n>... <size=200%><color=red>Charger!</color></size>\r\n>... Behind you!",
            // ">... [mechanical whirring]\r\n>... <size=200%><color=red>Reactor online!</color></size>\r\n>... [cheering]",
            ">... Verification in thirty!\r\n>... Code?\r\n>... <size=200%><color=red>WAVE!</color></size>",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Final wave!</color></size>\r\n>... [gunfire intensifies]",
            $">... [breathing heavily] Wave {waveCount.ToCardinal()}!\r\n>... <size=200%><color=red>Last one!</color></size>\r\n>... Hold on!",
            ">... Warmup phase ending!\r\n>... <size=200%><color=red>Brace!</color></size>\r\n>... [alarm blaring]",
            // ">... [static]\r\n>... <size=200%><color=red>REACTOR_VERIFY complete!</color></size>\r\n>... Next!",
            ">... Flyers incoming!\r\n>... <size=200%><color=red>Look up!</color></size>\r\n>... [screeching]",
            $">... Code's on the HUD!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Typing!",
            ">... [coughing] Can't breathe!\r\n>... Fog's clearing!\r\n>... <size=200%><color=red>Hold on!</color></size>",
            $">... Wave {thirdWaves.ToCardinal()} complete!\r\n>... <size=200%><color=red>{twoThirdsWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [groaning]",
            $">... Verification ready!\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Done!",
            ">... [gunfire]\r\n>... <size=200%><color=red>Reload!</color></size>\r\n>... Covering!",
            $">... Wave {twoThirdsWaves.ToCardinal()} starting!\r\n>... <size=200%><color=red>{thirdWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [breathing heavily]",
            ">... Pouncer spawning!\r\n>... <size=200%><color=red>It's grabbing me!</color></size>\r\n>... [screaming]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave defense starting!</color></size>\r\n>... Here we go!",
            ">... Verification timer!\r\n>... <size=200%><color=red>Thirty seconds!</color></size>\r\n>... Get ready!",
            ">... [mechanical sounds]\r\n>... Reactor progressing!\r\n>... <size=200%><color=red>Wave complete!</color></size>",
            ">... Giant wave!\r\n>... <size=200%><color=red>Focus fire!</color></size>\r\n>... [gunfire intensifies]",
            $">... Code's \"{verifyCode}\"!\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Verified!",
            ">... [breathing heavily] Need backup!\r\n>... <size=200%><color=red>I'm out!</color></size>\r\n>... Keep shooting!",
            $">... Wave {halfWaves.ToCardinal()} complete!\r\n>... <size=200%><color=red>Halfway there!</color></size>\r\n>... [panting]",
            ">... Verification failed!\r\n>... <size=200%><color=red>Wave reset!</color></size>\r\n>... Not again!",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Contact!</color></size>\r\n>... [gunfire]",
            ">... Reactor's at sixty percent!\r\n>... <size=200%><color=red>Keep defending!</color></size>\r\n>... [mechanical whirring]",
            ">... Wave timer!\r\n>... <size=200%><color=red>Forty-five seconds!</color></size>\r\n>... Almost done!",
            ">... [gunfire]\r\n>... <size=200%><color=red>Hybrid down!</color></size>\r\n>... More coming!",
            $">... REACTOR_VERIFY {verifyCode}!\r\n>... <size=200%><color=red>Verified!</color></size>\r\n>... [mechanical sounds]",
            ">... [breathing heavily] Out of C-Foam!\r\n>... <size=200%><color=red>Use mines!</color></size>\r\n>... On it!",
            $">... Verification in ten!\r\n>... Code ready?\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
            $">... Wave {twoThirdsWaves.ToCardinal()} complete!\r\n>... <size=200%><color=red>{thirdWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [groaning]",
            ">... [static]\r\n>... <size=200%><color=red>Reactor advancing!</color></size>\r\n>... Next wave!",
            ">... Charger swarm!\r\n>... <size=200%><color=red>Shoot them!</color></size>\r\n>... Keep moving!",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave starting!</color></size>\r\n>... Brace!",
            ">... Verification complete!\r\n>... <size=200%><color=red>Next wave incoming!</color></size>\r\n>... [breathing heavily]",
            ">... [coughing] Fog wave!\r\n>... <size=200%><color=red>Can't see them!</color></size>\r\n>... Listen!",
            ">... Wave timer!\r\n>... <size=200%><color=red>Three minutes!</color></size>\r\n>... Long one!",
            ">... Mother spawning!\r\n>... <size=200%><color=red>Kill it fast!</color></size>\r\n>... [gunfire]",
            ">... Reactor's almost online!\r\n>... <size=200%><color=red>Final wave!</color></size>\r\n>... Hold on!",
            ">... Verification timer!\r\n>... <size=200%><color=red>Eight seconds!</color></size>\r\n>... [frantic typing]",
            $">... [static]\r\n>... Code's \"{verifyCode}\"!\r\n>... <size=200%><color=red>Typing!</color></size>",
            // ">... Wave ten complete!\r\n>... <size=200%><color=red>Reactor online!</color></size>\r\n>... [cheering]",
            ">... Nightmare wave!\r\n>... Flyers!\r\n>... <size=200%><color=red>Stand your ground!</color></size>",
            ">... [gunfire]\r\n>... <size=200%><color=red>Giant!</color></size>\r\n>... Focus fire!",
            // ">... [mechanical whirring]\r\n>... <size=200%><color=red>Reactor fully online!</color></size>\r\n>... We did it!",
            $">... Verification in fifteen!\r\n>... Code?\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Last wave!</color></size>\r\n>... [gunfire intensifies]",
            $">... [breathing heavily] Wave {twoThirdsWaves.ToCardinal()}!\r\n>... <size=200%><color=red>{thirdWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... Almost there!",
            ">... Warmup ending!\r\n>... <size=200%><color=red>Brace!</color></size>\r\n>... [alarm blaring]",
            ">... [static]\r\n>... <size=200%><color=red>Verification done!</color></size>\r\n>... Next wave!",
            ">... Shadow wave!\r\n>... <size=200%><color=red>They're everywhere!</color></size>\r\n>... [screeching sounds]",
            $">... Code's on HUD!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Typing!",
            ">... [coughing] Fog's thick!\r\n>... <size=200%><color=red>Push through!</color></size>\r\n>... [breathing heavily]",
            $">... Wave {thirdWaves.ToCardinal()} complete!\r\n>... <size=200%><color=red>{twoThirdsWaves.ToCardinal()} more!</color></size>\r\n>... [groaning]",
            $">... Verification ready!\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Done!",
            ">... [gunfire]\r\n>... <size=200%><color=red>Reloading!</color></size>\r\n>... Cover!",
            $">... Wave {twoThirdsWaves.ToCardinal()} starting!\r\n>... <size=200%><color=red>{thirdWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [breathing heavily]",
            ">... Tank incoming!\r\n>... <size=200%><color=red>Scatter!</color></size>\r\n>... [heavy footsteps]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave defense!</color></size>\r\n>... Here we go!",
            ">... Verification timer!\r\n>... <size=200%><color=red>Twenty seconds!</color></size>\r\n>... Hurry!",
            ">... [mechanical sounds]\r\n>... Reactor's progressing!\r\n>... <size=200%><color=red>Wave done!</color></size>",
            ">... Striker swarm!\r\n>... <size=200%><color=red>Focus fire!</color></size>\r\n>... [gunfire intensifies]",
            ">... [breathing heavily] Need ammo!\r\n>... <size=200%><color=red>Ammo crate!</color></size>\r\n>... Where?!",
            $">... Wave {(waveCount - 1).ToCardinal()} complete!\r\n>... <size=200%><color=red>One more!</color></size>\r\n>... [panting]",
            ">... Verification failed!\r\n>... <size=200%><color=red>Wave reset!</color></size>\r\n>... Fuck!",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Contact!</color></size>\r\n>... [gunfire]",
            ">... Reactor's at eighty percent!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [mechanical whirring]",
            ">... Wave timer!\r\n>... <size=200%><color=red>Ninety seconds!</color></size>\r\n>... Long wave!",
            ">... [gunfire]\r\n>... <size=200%><color=red>Shooter down!</color></size>\r\n>... More coming!",
            $">... REACTOR_VERIFY {verifyCode}!\r\n>... <size=200%><color=red>Verified!</color></size>\r\n>... [mechanical sounds]",
            ">... [breathing heavily] Out of tools!\r\n>... <size=200%><color=red>Use guns!</color></size>\r\n>... [gunfire]",
            $">... Wave {halfWaves.ToCardinal()} starting!\r\n>... <size=200%><color=red>Here they come!</color></size>\r\n>... [alarm blaring]",
            $">... Verification in twenty-five!\r\n>... Code ready?\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Reactor progressing!</color></size>\r\n>... Next wave!",
            ">... Giant Shooter wave!\r\n>... <size=200%><color=red>Take cover!</color></size>\r\n>... [gunfire]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave starting!</color></size>\r\n>... Brace!",
            ">... Verification complete!\r\n>... <size=200%><color=red>Next wave!</color></size>\r\n>... [breathing heavily]",
            ">... Pouncer spawning!\r\n>... <size=200%><color=red>Stay together!</color></size>\r\n>... [screeching]",
            ">... Reactor's near complete!\r\n>... <size=200%><color=red>One more wave!</color></size>\r\n>... Hold on!",
            ">... Verification timer!\r\n>... <size=200%><color=red>Twelve seconds!</color></size>\r\n>... [frantic typing]",
            $">... [static]\r\n>... Code's \"{verifyCode}\"!\r\n>... <size=200%><color=red>Typing!</color></size>",
            $">... Wave {(waveCount - 1).ToCardinal()} complete!\r\n>... <size=200%><color=red>Last one!</color></size>\r\n>... [panting]",
            ">... Berserker wave!\r\n>... They're fast!\r\n>... <size=200%><color=red>Stall them!</color></size>",
            ">... [gunfire]\r\n>... <size=200%><color=red>Charger!</color></size>\r\n>... Behind us!",
            // ">... [mechanical whirring]\r\n>... <size=200%><color=red>Reactor complete!</color></size>\r\n>... [cheering]",
            $">... Verification in five!\r\n>... Code?\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Final wave!</color></size>\r\n>... [gunfire intensifies]",
            $">... [breathing heavily] Wave {twoThirdsWaves.ToCardinal()}!\r\n>... <size=200%><color=red>{thirdWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... Keep going!",
            ">... Warmup phase!\r\n>... <size=200%><color=red>Get ready!</color></size>\r\n>... [alarm blaring]",
            ">... [static]\r\n>... <size=200%><color=red>Verification done!</color></size>\r\n>... Next!",
            ">... Infected Hybrids!\r\n>... <size=200%><color=red>They're fast!</color></size>\r\n>... [gunfire]",
            $">... Code's on HUD!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Typing!",
            ">... [coughing] Can't see!\r\n>... <size=200%><color=red>Fog's thick!</color></size>\r\n>... Use bio-tracker!",
            $">... Wave one complete!\r\n>... <size=200%><color=red>{waveCount.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [groaning]",
            $">... Verification ready!\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Done!",
            ">... [gunfire]\r\n>... <size=200%><color=red>Reload!</color></size>\r\n>... Covering!",
            $">... Wave {halfWaves.ToCardinal()} starting!\r\n>... <size=200%><color=red>Halfway!</color></size>\r\n>... [breathing heavily]",
            ">... Shadow Pouncer!\r\n>... <size=200%><color=red>It's teleporting!</color></size>\r\n>... [screaming]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave defense!</color></size>\r\n>... Here we go!",
            // ">... Verification timer!\r\n>... <size=200%><color=red>Seventeen minutes!</color></size>\r\n>... That's long!",
            ">... [mechanical sounds]\r\n>... Reactor's progressing!\r\n>... <size=200%><color=red>Wave done!</color></size>",
            ">... Striker wave!\r\n>... <size=200%><color=red>Focus fire!</color></size>\r\n>... [gunfire intensifies]",
            $">... Code's \"{verifyCode}\"!\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Verified!",
            ">... [breathing heavily] Need health!\r\n>... <size=200%><color=red>Health pack!</color></size>\r\n>... Here!",
            $">... Wave {twoThirdsWaves.ToCardinal()} complete!\r\n>... <size=200%><color=red>{thirdWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [panting]",
            ">... Verification missed!\r\n>... <size=200%><color=red>Wave reset!</color></size>\r\n>... Not again!",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Contact!</color></size>\r\n>... [gunfire]",
            ">... Reactor's at forty percent!\r\n>... <size=200%><color=red>Keep defending!</color></size>\r\n>... [mechanical whirring]",
            ">... Wave timer!\r\n>... <size=200%><color=red>Thirty seconds!</color></size>\r\n>... Almost done!",
            ">... [gunfire]\r\n>... <size=200%><color=red>Giant down!</color></size>\r\n>... More coming!",
            $">... REACTOR_VERIFY {verifyCode}!\r\n>... <size=200%><color=red>Verified!</color></size>\r\n>... [mechanical sounds]",
            ">... [breathing heavily] Out of ammo!\r\n>... <size=200%><color=red>Ammo refill!</color></size>\r\n>... Where?!",
            $">... Verification in twelve!\r\n>... Code ready?\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Reactor advancing!</color></size>\r\n>... Next wave!",
            $">... Wave {twoThirdsWaves.ToCardinal()} complete!\r\n>... <size=200%><color=red>{thirdWaves.ToCardinal().ToTitleCase()} more!</color></size>\r\n>... [groaning]",

            // ">... [coughing] Infectious fog!\r\n>... <size=200%><color=red>Disinfect!</color></size>\r\n>... Where?!",
            // ">... Wave timer!\r\n>... <size=200%><color=red>Four minutes!</color></size>\r\n>... This is long!",
        }))!);

        if (ReactorStartupGetCodes)
        {
            level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
            {
                ">... Terminal's in the next zone!\r\n>... <size=200%><color=red>Get the code!</color></size>\r\n>... [running footsteps]",
                $">... Code's in the log!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Get back here!",
                $">... Verification code?\r\n>... It's on the HUD!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
                ">... Can't reach the terminal!\r\n>... <size=200%><color=red>Clear a path!</color></size>\r\n>... [gunfire]",
                $">... Wave {thirdWaves.ToCardinal().ToTitleCase()} complete!\r\n>... <size=200%><color=red>Get the code!</color></size>\r\n>... On it!",
                ">... [panting] Found the terminal!\r\n>... <size=200%><color=red>What's the code?!</color></size>\r\n>... Checking logs!",
                ">... Code runner down!\r\n>... <size=200%><color=red>I'll get it!</color></size>\r\n>... Hurry!",
                ">... Code's not on HUD!\r\n>... <size=200%><color=red>Check the terminals!</color></size>\r\n>... Where?!",
                ">... Code zone unlocked!\r\n>... <size=200%><color=red>Get moving!</color></size>\r\n>... On my way!",
                ">... [panting] Can't find the terminal!\r\n>... Use PING!\r\n>... <size=200%><color=red>Found it!</color></size>",
                // ">... Code in Zone 18!\r\n>... <size=200%><color=red>That's across the map!</color></size>\r\n>... Run!",
                $">... Terminal shows the code!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Got it!",
                $">... [static]\r\n>... Code retrieved.\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>",
                ">... Code's in the next zone!\r\n>... Scout patrol!\r\n>... <size=200%><color=red>Stealth it!</color></size>",
                $">... Code runner, status?\r\n>... <size=200%><color=red>Got it! \"{verifyCode}\"!</color></size>\r\n>... Verifying!",
                ">... Code in the log file!\r\n>... <size=200%><color=red>LIST LOGS!</color></size>\r\n>... Found it!",
                ">... [static]\r\n>... Terminal located.\r\n>... <size=200%><color=red>Get the code!</color></size>",
                // ">... [breathing heavily] Code zone's alarmed!\r\n>... <size=200%><color=red>Stealth scan!</color></size>\r\n>... Trying!",
                $">... Code's \"{verifyCode}\"!\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Verified!",
                ">... Terminal's past the fog!\r\n>... <size=200%><color=red>Push through!</color></size>\r\n>... [coughing]",
                $">... [static]\r\n>... <size=200%><color=red>Code retrieved!</color></size>\r\n>... \"{verifyCode}\"!",
                // ">... Code zone's locked!\r\n>... <size=200%><color=red>Find the key!</color></size>\r\n>... Where?!",
                $">... REACTOR_VERIFY {verifyCode}!\r\n>... <size=200%><color=red>Verified!</color></size>\r\n>... [mechanical sounds]",
                ">... Terminal ahead!\r\n>... <size=200%><color=red>Get the code!</color></size>\r\n>... [running footsteps]",
                // ">... Code's in Zone 31!\r\n>... <size=200%><color=red>That's far!</color></size>\r\n>... Run fast!",
                $">... [static]\r\n>... Code's \"{verifyCode}\"!\r\n>... <size=200%><color=red>Verifying!</color></size>",
                ">... Code zone's scouted!\r\n>... <size=200%><color=red>Three Giants!</color></size>\r\n>... Clear them first!",
                $">... Terminal's hacked!\r\n>... <size=200%><color=red>Get the code!</color></size>\r\n>... \"{verifyCode}\"!",
                ">... Terminal zone unlocked!\r\n>... <size=200%><color=red>Get the code!</color></size>\r\n>... Moving!",
                $">... [static]\r\n>... Code located.\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
                ">... [static]\r\n>... <size=200%><color=red>Terminal located!</color></size>\r\n>... Get the code!",
                ">... Code zone's alarmed!\r\n>... <size=200%><color=red>Scan it!</color></size>\r\n>... No time!",
                ">... Terminal's in the hub!\r\n>... <size=200%><color=red>Get there!</color></size>\r\n>... [running footsteps]",
                $">... Code's in the log!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Got it!",
                $">... [breathing heavily] Code retrieved!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Verifying!",
                ">... Code zone's clear!\r\n>... <size=200%><color=red>Get the terminal!</color></size>\r\n>... On it!",
                ">... Terminal's locked!\r\n>... <size=200%><color=red>Hack it fast!</color></size>\r\n>... Working!",
                ">... Terminal zone open!\r\n>... <size=200%><color=red>Get the code!</color></size>\r\n>... Moving!",
                $">... [static]\r\n>... Code found.\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
                $">... Code's \"{verifyCode}\"!\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... Verified!",
                ">... [static]\r\n>... <size=200%><color=red>Terminal ahead!</color></size>\r\n>... Get it!",
                ">... Code zone's far!\r\n>... <size=200%><color=red>Run fast!</color></size>\r\n>... On it!",
                ">... Terminal's past the door!\r\n>... <size=200%><color=red>Scan it!</color></size>\r\n>... [bioscan sounds]",
                // ">... Code's in Zone 45!\r\n>... <size=200%><color=red>That's deep!</color></size>\r\n>... Go now!",
                $">... [breathing heavily] Code found!\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>\r\n>... Verifying!",
                ">... Code zone's scouted!\r\n>... <size=200%><color=red>Clear path!</color></size>\r\n>... Go now!",
                $">... Terminal's hacked!\r\n>... <size=200%><color=red>Code retrieved!</color></size>\r\n>... \"{verifyCode}\"!",
                ">... Terminal zone unlocked!\r\n>... <size=200%><color=red>Get it!</color></size>\r\n>... Moving!",
                $">... [static]\r\n>... Code located.\r\n>... <size=200%><color=red>\"{verifyCode}\"!</color></size>",
                ">... [static]\r\n>... <size=200%><color=red>Terminal found!</color></size>\r\n>... Get the code!",
                ">... Code zone's blood door!\r\n>... <size=200%><color=red>Kill them all!</color></size>\r\n>... [gunfire]",
                ">... Terminal's in storage!\r\n>... <size=200%><color=red>Get there!</color></size>\r\n>... [running footsteps]",
            }))!);
        }

        #endregion
    }
}
