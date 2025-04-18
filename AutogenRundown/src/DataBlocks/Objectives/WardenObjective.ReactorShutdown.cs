﻿using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

public partial record WardenObjective
{
    #region Custom Reactor Shutdown Alarm bases
    //
    // These are copies of the base versions of these alarms but the first team scan is switched
    // with a large red scan instead. This follows more what the base game does.
    //

    private static readonly ChainedPuzzle Reactor_AlarmClass4 = new()
    {
        PublicAlarmName = "Class IV Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ScanLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge
        },
    };

    private static readonly ChainedPuzzle Reactor_AlarmClass5 = new()
    {
        PublicAlarmName = "Class V Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ScanLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge
        },
    };

    private static readonly ChainedPuzzle Reactor_AlarmClass6 = new()
    {
        PublicAlarmName = "Class VI Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 15.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ScanLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge
        },
    };

    private static readonly ChainedPuzzle Reactor_AlarmClass4_Cluster = new()
    {
        PublicAlarmName = "Class IV Cluster Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 5.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ScanLarge,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
        },
    };

    private static readonly ChainedPuzzle Reactor_AlarmClass5_Cluster = new()
    {
        PublicAlarmName = "Class V Cluster Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 5.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ScanLarge,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
        },
    };

    private static readonly ChainedPuzzle Reactor_AlarmClass6_Cluster = new()
    {
        PublicAlarmName = "Class V Cluster Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 5.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ScanLarge,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.AllBig,
        },
    };

    private static readonly ChainedPuzzle Reactor_AlarmClass6_Mixed = new()
    {
        PublicAlarmName = "Class VI M Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 20.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ScanLarge,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.SustainedSmall,
        },
    };
    #endregion

    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    private void PreBuild_ReactorShutdown(BuildDirector director, Level level)
    {
        // We have to set the persistent ID later
        LayoutDefinitions = new LayoutDefinitions()
        {
            Name = $"{level.Tier}{level.Index}_{director.Bulkhead}",
            Type = ExtraObjectiveSetupType.ReactorShutdown,
        };
    }

    /// <summary>
    /// Reactor shutdown will result in the lights being off for the remainder of the
    /// level. Factor that as a difficulty modifier.
    ///
    /// TODO: we may want to remove the team scan at the start of our scans
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    private void Build_ReactorShutdown(BuildDirector director, Level level)
    {
        FindLocationInfo = "Make sure the Reactor is fully shut down before leaving";
        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_ToMainLayer = "Go back to the main objective and complete the expedition.";

        LightsOnFromBeginning = true;
        LightsOnDuringIntro = true;
        LightsOnWhenStartupComplete = false;

        StartPuzzle = ChainedPuzzle.FindOrPersist(ChainedPuzzle.TeamScan);

        // TODO: we might want a better way to determin the type of this
        var reactorDefinition = (ReactorShutdown)LayoutDefinitions!.Definitions.First();

        // Verification puzzle. Default to a team scan
        var puzzle = ChainedPuzzle.TeamScan;

        switch (director.Tier)
        {
            case "A":
            {
                var population = WavePopulation.Baseline;
                var surpriseBossEvents = new List<WardenObjectiveEvent>();

                // Surprise boss events
                var surpriseBossDelay = Generator.Between(50, 80);
                surpriseBossEvents = Generator.Pick(new List<List<WardenObjectiveEvent>>()
                {
                    new List<WardenObjectiveEvent>()
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    new List<WardenObjectiveEvent>()
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),
                })!;

                puzzle = Generator.Select(new List<(double, ChainedPuzzle)>
                {
                    (6.0, Reactor_AlarmClass4 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 20.0,
                        WantedDistanceBetweenPuzzleComponents = 10.0,
                    }),
                    (6.0, Reactor_AlarmClass4_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Normal,
                        WantedDistanceFromStartPos = 20.0,
                        WantedDistanceBetweenPuzzleComponents = 10.0,
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5)
                });

                // We only append the boss events if it's the boss scan
                if (puzzle == ChainedPuzzle.StealthScan5)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                    reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(surpriseBossEvents);
                    break; // No futher processing here
                }

                // Boss bait bait scan does still get the exit wave though
                if (puzzle == ChainedPuzzle.StealthScan4)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                }
                break;
            }

            case "B":
            {
                var population = WavePopulation.Baseline;

                if (level.Settings.HasShadows())
                    population = WavePopulation.Baseline_Shadows;
                else if (level.Settings.HasChargers())
                    population = WavePopulation.Baseline_Chargers;
                else if (level.Settings.HasNightmares())
                    population = WavePopulation.Baseline_Nightmare;

                var events = new List<WardenObjectiveEvent>();
                var surpriseBossEvents = new List<WardenObjectiveEvent>();

                // Chance of other bosses
                if (Generator.Flip(0.1))
                    events.AddSpawnWave(Generator.Pick(new List<GenericWave>
                    {
                        GenericWave.SingleTank,
                        GenericWave.SingleTankPotato
                    })!, Generator.Between(45, 70));

                // Surprise boss events
                var surpriseBossDelay = Generator.Between(50, 80);
                surpriseBossEvents = Generator.Pick(new List<List<WardenObjectiveEvent>>()
                {
                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncer, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncer, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),
                })!;

                puzzle = Generator.Select(new List<(double, ChainedPuzzle)>
                {
                    (6.0, Reactor_AlarmClass5 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 30.0
                    }),
                    (6.0, Reactor_AlarmClass5_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Normal,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 30.0
                    }),
                    (5.0, Reactor_AlarmClass6_Mixed with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Normal,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5)
                });

                // We only append the boss events if it's the boss scan
                if (puzzle == ChainedPuzzle.StealthScan5)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                    reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(surpriseBossEvents);
                    break; // No futher processing here
                }

                // Boss bait bait scan does still get the exit wave though
                if (puzzle == ChainedPuzzle.StealthScan4)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                }

                reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(events);
                break;
            }

            case "C":
            {
                var population = WavePopulation.Baseline_Hybrids;

                if (level.Settings.HasShadows())
                    population = WavePopulation.Baseline_Shadows;
                else if (level.Settings.HasChargers())
                    population = WavePopulation.Baseline_Chargers;
                else if (level.Settings.HasNightmares())
                    population = WavePopulation.Baseline_Nightmare;

                var events = new List<WardenObjectiveEvent>();
                var surpriseBossEvents = new List<WardenObjectiveEvent>();

                // Chance of pouncer 1
                if (Generator.Flip(0.2))
                    events.AddSpawnWave(GenericWave.SinglePouncer, Generator.Between(30, 40));

                // Chance of other bosses
                if (!events.Any() && Generator.Flip(0.1))
                    events.AddSpawnWave(Generator.Pick(new List<GenericWave>
                    {
                        GenericWave.SingleTank,
                        GenericWave.SingleTankPotato
                    })!, Generator.Between(45, 70));

                // Surprise boss events
                var surpriseBossDelay = Generator.Between(50, 80);
                surpriseBossEvents = Generator.Pick(new List<List<WardenObjectiveEvent>>()
                {
                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleMother, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncer, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncer, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncer, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),
                })!;

                puzzle = Generator.Select(new List<(double, ChainedPuzzle)>
                {
                    (6.0, Reactor_AlarmClass5 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (6.0, Reactor_AlarmClass5_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (5.0, Reactor_AlarmClass6_Mixed with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5)
                });

                // We only append the boss events if it's the boss scan
                if (puzzle == ChainedPuzzle.StealthScan5)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                    reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(surpriseBossEvents);
                    break; // No futher processing here
                }

                // Boss bait bait scan does still get the exit wave though
                if (puzzle == ChainedPuzzle.StealthScan4)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                }

                reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(events);
                break;
            }

            case "D":
            {
                var population = WavePopulation.Baseline_Hybrids;

                if (level.Settings.HasShadows())
                    population = WavePopulation.Baseline_Shadows;
                else if (level.Settings.HasChargers())
                    population = WavePopulation.Baseline_Chargers;
                else if (level.Settings.HasNightmares())
                    population = WavePopulation.Baseline_Nightmare;

                var events = new List<WardenObjectiveEvent>();
                var surpriseBossEvents = new List<WardenObjectiveEvent>();

                // Chance of pouncer 1
                if (Generator.Flip(0.3))
                    events.AddSpawnWave(GenericWave.SinglePouncer, Generator.Between(30, 40));

                // Chance of pouncer 2!
                if (Generator.Flip(0.04))
                    events.AddSpawnWave(GenericWave.SinglePouncer, Generator.Between(60, 75));

                // Chance of other bosses
                if (Generator.Flip(events.Any() ? 0.03 : 0.3))
                    events.AddSpawnWave(Generator.Pick(new List<GenericWave>
                    {
                        GenericWave.SingleTank,
                        GenericWave.SingleMother,
                        GenericWave.SingleTankPotato
                    })!, Generator.Between(45, 70));

                // Surprise boss events
                var surpriseBossDelay = Generator.Between(50, 80);
                surpriseBossEvents = Generator.Pick(new List<List<WardenObjectiveEvent>>()
                {
                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SinglePMother, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleMother, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleMother, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),
                })!;

                puzzle = Generator.Select(new List<(double, ChainedPuzzle)>
                {
                    (4.0, Reactor_AlarmClass5 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (4.0, Reactor_AlarmClass5_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (3.0, Reactor_AlarmClass6_Mixed with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),
                    // This may be extremely hard to do
                    (2.0, ChainedPuzzle.AlarmClass2_Surge with
                    {
                        WantedDistanceFromStartPos = 15.0,
                        WantedDistanceBetweenPuzzleComponents = 15.0
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5)
                });

                // We only append the boss events if it's the boss scan
                if (puzzle == ChainedPuzzle.StealthScan5)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                    reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(surpriseBossEvents);
                    break; // No futher processing here
                }

                // Boss bait bait scan does still get the exit wave though
                if (puzzle == ChainedPuzzle.StealthScan4)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                }

                reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(events);
                break;
            }

            case "E":
            {
                var population = WavePopulation.Baseline_Hybrids;

                if (level.Settings.HasShadows())
                    population = WavePopulation.Baseline_Shadows;
                else if (level.Settings.HasChargers())
                    population = WavePopulation.Baseline_Chargers;
                else if (level.Settings.HasNightmares())
                    population = WavePopulation.Baseline_Nightmare;

                var events = new List<WardenObjectiveEvent>();
                var surpriseBossEvents = new List<WardenObjectiveEvent>();

                // Chance of pouncer 1
                if (Generator.Flip(0.6))
                    events.AddSpawnWave(GenericWave.SinglePouncer, Generator.Between(30, 40));

                // Chance of pouncer 2!
                if (Generator.Flip(0.2))
                    events.AddSpawnWave(GenericWave.SinglePouncer, Generator.Between(60, 75));

                // Chance of other bosses
                if (Generator.Flip(events.Any() ? 0.1 : 0.6))
                    events.AddSpawnWave(Generator.Pick(new List<GenericWave>
                    {
                        GenericWave.SingleTank,
                        GenericWave.SingleMother,
                        GenericWave.SingleTankPotato
                    })!, Generator.Between(45, 70));

                // Surprise boss events
                var surpriseBossDelay = Generator.Between(50, 80);
                surpriseBossEvents = Generator.Pick(new List<List<WardenObjectiveEvent>>()
                {
                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SinglePMother, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleMother, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleMother, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncerShadow, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncerShadow, surpriseBossDelay + Generator.Between(10, 15))
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncerShadow, surpriseBossDelay + Generator.Between(10, 15))
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),
                })!;

                puzzle = Generator.Select(new List<(double, ChainedPuzzle)>
                {
                    (3.0, Reactor_AlarmClass6 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (3.0, Reactor_AlarmClass6_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (3.0, ChainedPuzzle.AlarmClass7_Mixed with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),
                    // This may be extremely hard to do
                    (2.0, ChainedPuzzle.AlarmClass3_Surge with
                    {
                        WantedDistanceFromStartPos = 20.0,
                        WantedDistanceBetweenPuzzleComponents = 15.0
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5),
                });

                // We only append the boss events if it's the boss scan
                if (puzzle == ChainedPuzzle.StealthScan5)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50,
                    };
                    reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(surpriseBossEvents);
                    break; // No futher processing here
                }

                // Boss bait bait scan does still get the exit wave though
                if (puzzle == ChainedPuzzle.StealthScan4)
                {
                    puzzle = puzzle with
                    {
                        WantedDistanceFromStartPos = 50,
                        WantedDistanceBetweenPuzzleComponents = 50
                    };
                }

                reactorDefinition.EventsOnShutdownPuzzleStarts.AddRange(events);
                break;
            }
        }

        reactorDefinition.PuzzleOnVerification = ChainedPuzzle.FindOrPersist(puzzle);

        // Add event to turn off the lights as not all the reactor geos correctly do this
        reactorDefinition.EventsOnActive.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.AllLightsOff,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                Delay = 0.0
            });

        // We have to force complete the objective in EOS
        reactorDefinition.EventsOnComplete.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ForceCompleteObjective,
                Layer = EventBuilder.GetLayerFromBulkhead(director.Bulkhead)
            });

        // Add varying exit alarms on completing the reactor. Some easy tiers have no alarm
        reactorDefinition.EventsOnComplete.AddGenericWave(
            (level.Tier, director.Bulkhead) switch
            {
                ("A", Bulkhead.Main) => GenericWave.Exit_Objective_Easy,
                ("A", Bulkhead.Extreme) => GenericWave.None,
                ("A", Bulkhead.Overload) => new GenericWave
                {
                    Settings = WaveSettings.SingleWave_MiniBoss_6pts,
                    Population = WavePopulation.OnlyHybrids
                },

                ("B", Bulkhead.Main) => GenericWave.Exit_Objective_Medium,
                ("B", Bulkhead.Extreme) => new GenericWave
                {
                    Settings = WaveSettings.SingleWave_MiniBoss_8pts,
                    Population = WavePopulation.OnlyHybrids
                },
                ("B", Bulkhead.Overload) => GenericWave.ErrorAlarm_Easy,

                ("C", Bulkhead.Main) => GenericWave.Exit_Objective_Medium,
                ("C", Bulkhead.Extreme) => new GenericWave
                {
                    Settings = WaveSettings.SingleWave_MiniBoss_12pts,
                    Population = WavePopulation.OnlyHybrids
                },
                ("C", Bulkhead.Overload) => GenericWave.ErrorAlarm_Normal,

                ("D", Bulkhead.Main) => GenericWave.Exit_Objective_Hard,
                ("D", Bulkhead.Extreme) => GenericWave.ErrorAlarm_Easy,
                ("D", Bulkhead.Overload) => GenericWave.ErrorAlarm_Normal,

                ("E", Bulkhead.Main) => GenericWave.Exit_Objective_VeryHard,
                ("E", Bulkhead.Extreme) => GenericWave.ErrorAlarm_Normal,
                ("E", Bulkhead.Overload) => GenericWave.ErrorAlarm_Hard,
            });
    }

    private void PostBuild_ReactorShutdown(BuildDirector _, Level level)
    {
        // EOS needs this to be set to empty to work
        Type = WardenObjectiveType.Empty;

        // This always needs to be the main level layout
        LayoutDefinitions!.MainLevelLayout = level.GetLevelLayout(Bulkhead.Main)!.PersistentId;
        LayoutDefinitions.Save();
    }
}
