using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;
using AutogenRundown.Patches;

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
                    Settings = WaveSettings.SingleWave_MiniBoss_16pts,
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

        #region Warden Intel Messages

        // Intel variables
        var verifyCode = Generator.Pick(level.Tier switch
        {
            "C" => TerminalUplink.FiveLetterWords,
            "D" => TerminalUplink.SixLetterWords,
            "E" => TerminalUplink.SevenLetterWords,
            _ => TerminalUplink.FourLetterWords
        })!.ToUpperInvariant();

        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
            ">... REACTOR_SHUTDOWN entered.\r\n>... <size=200%><color=red>Verification code on HUD!</color></size>\r\n>... [frantic typing]",
            ">... Got the verification!\r\n>... <size=200%><color=red>Bioscans starting!</color></size>\r\n>... [alarm blaring]",
            ">... Reactor's shutting down.\r\n>... Power's going to cut.\r\n>... <size=200%><color=red>Get ready!</color></size>",
            ">... [static]\r\n>... The reactor terminal.\r\n>... <size=200%><color=red>It's right there!</color></size>",
            ">... Shadows incoming!\r\n>... <size=200%><color=red>Tag them with Bio Tracker!</color></size>\r\n>... [gunfire]",
            ">... They won't stop spawning!\r\n>... Complete the bioscans!\r\n>... <size=200%><color=red>Move faster!</color></size>",
            ">... Power's severed.\r\n>... Everything's dark.\r\n>... <size=200%><color=red>Stay together!</color></size>",
            $">... [whispering] Enter the code.\r\n>... REACTOR_VERIFY {verifyCode}...\r\n>... <size=200%><color=red>They heard us!</color></size>",
            ">... Get in the bioscan!\r\n>... <size=200%><color=red>It's reversing!</color></size>\r\n>... Back in! Back in!",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Security system malfunction!</color></size>\r\n>... [gunfire intensifies]",
            ">... Sentries positioned?\r\n>... Two outside the reactor room.\r\n>... <size=200%><color=red>Here they come!</color></size>",
            ">... Shadows everywhere!\r\n>... <size=200%><color=red>Where's the Bio Tracker?!</color></size>\r\n>... [screaming]",
            ">... Verification complete.\r\n>... <size=200%><color=red>Now the real fight begins!</color></size>\r\n>... [distant roar]",
            ">... Another bioscan!\r\n>... How many more?!\r\n>... <size=200%><color=red>Just hold position!</color></size>",
            ">... [panting]\r\n>... We need to get back to extraction.\r\n>... <size=200%><color=red>They're blocking the path!</color></size>",
            $">... The code's on my HUD.\r\n>... {verifyCode}.\r\n>... <size=200%><color=red>Entering now!</color></size>",
            ">... C-Foam the doors!\r\n>... <size=200%><color=red>Slow them down!</color></size>\r\n>... [hammering]",
            ">... I'm out of heavy ammo!\r\n>... Conserve what you have.\r\n>... <size=200%><color=red>Long way to extraction!</color></size>",
            ">... Shadow sleepers.\r\n>... They're teleporting.\r\n>... <size=200%><color=red>Can't track them!</color></size>",
            ">... [static crackling]\r\n>... <size=200%><color=red>REACTOR_SHUTDOWN initiated!</color></size>\r\n>... [alarm blaring]",
            ">... Stand in the scan zone!\r\n>... Defend the position!\r\n>... <size=200%><color=red>Don't leave!</color></size>",
            $">... Code verification required.\r\n>... It's showing {verifyCode}.\r\n>... <size=200%><color=red>Type it in!</color></size>",
            ">... [heavy breathing]\r\n>... Bioscan complete.\r\n>... <size=200%><color=red>More coming!</color></size>",
            ">... The lights just went out!\r\n>... Power's severed.\r\n>... <size=200%><color=red>Just like we planned!</color></size>",
            ">... Terminal's right here.\r\n>... REACTOR_SHUTDOWN.\r\n>... <size=200%><color=red>No turning back now!</color></size>",
            ">... [screaming]\r\n>... <size=200%><color=red>Shadows in the reactor room!</color></size>\r\n>... [gunfire]",
            ">... Verification timer!\r\n>... No rush, we have time.\r\n>... <size=200%><color=red>Just type carefully!</color></size>",
            ">... Running back to extraction.\r\n>... <size=200%><color=red>They're everywhere!</color></size>\r\n>... Keep moving!",
            ">... Two people in the scan.\r\n>... Defenders cover us.\r\n>... <size=200%><color=red>Don't let them through!</color></size>",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... Can't shut it off!",
            ">... Reactor's offline.\r\n>... Mission complete.\r\n>... <size=200%><color=red>Now we just survive!</color></size>",
            $">... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Code entered!</color></size>\r\n>... [klaxon blaring]",
            ">... Sentry's targeting Shadows.\r\n>... Bio Tracker's working.\r\n>... <size=200%><color=red>Keep tagging!</color></size>",
            ">... [whispering] Bioscan sequence.\r\n>... How many?\r\n>... <size=200%><color=red>Too many!</color></size>",
            ">... Shadow just teleported!\r\n>... Where is it?!\r\n>... <size=200%><color=red>Behind you!</color></size>",
            ">... Pre-typed REACTOR_VERIFY.\r\n>... Waiting for the code.\r\n>... <size=200%><color=red>There it is!</color></size>",
            ">... [gunfire]\r\n>... Complete the scan!\r\n>... <size=200%><color=red>Don't move from the zone!</color></size>",
            ">... Power severed across the expedition.\r\n>... Everything's dark now.\r\n>... <size=200%><color=red>Use your flashlights!</color></size>",
            ">... They won't stop coming!\r\n>... <size=200%><color=red>That's the point!</color></size>\r\n>... Just run!",
            ">... Bioscan team ready?\r\n>... Ready.\r\n>... <size=200%><color=red>Initiating shutdown!</color></size>",
            ">... [panting]\r\n>... One more bioscan.\r\n>... <size=200%><color=red>Then we move!</color></size>",
            $">... Verification code is {verifyCode}.\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [typing frantically]",
            ">... Cleared the path beforehand.\r\n>... Smart.\r\n>... <size=200%><color=red>Won't last long!</color></size>",
            ">... Shadow teleported behind the sentry.\r\n>... <size=200%><color=red>Reposition!</color></size>\r\n>... [screaming]",
            ">... C-Foaming doors as we go.\r\n>... Slowing them down.\r\n>... <size=200%><color=red>Keep falling back!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Reactor Security System Malfunction!</color></size>\r\n>... [alarm wailing]",
            ">... Bioscans are done!\r\n>... They're still spawning!\r\n>... <size=200%><color=red>Run to extraction!</color></size>",
            ">... Located the reactor terminal.\r\n>... Ready to shut it down?\r\n>... <size=200%><color=red>Do it!</color></size>",
            ">... Heavy ammo depleted.\r\n>... Using rifle now.\r\n>... <size=200%><color=red>Melee if you have to!</color></size>",
            ">... [whispering] Don't wake them.\r\n>... Need to reach the reactor.\r\n>... <size=200%><color=red>Careful!</color></size>",
            ">... Verification entered correctly.\r\n>... <size=200%><color=red>Bioscans starting!</color></size>\r\n>... [alarm blaring]",
            ">... Shadows in the bioscan zone!\r\n>... <size=200%><color=red>Clear them out!</color></size>\r\n>... [gunfire]",
            ">... Rally at the next checkpoint!\r\n>... Keep together!\r\n>... <size=200%><color=red>Don't get separated!</color></size>",
            ">... The reactor's offline.\r\n>... Power's gone.\r\n>... <size=200%><color=red>Everything's spawning now!</color></size>",
            ">... [heavy breathing]\r\n>... <size=200%><color=red>Continuous spawning!</color></size>\r\n>... Until extraction!",
            ">... Got a sentry malfunction!\r\n>... Pick it up and reposition!\r\n>... <size=200%><color=red>Move!</color></size>",
            $">... Code's showing on HUD.\r\n>... {verifyCode}.\r\n>... <size=200%><color=red>Verifying now!</color></size>",
            ">... [alarm blaring]\r\n>... Another bioscan.\r\n>... <size=200%><color=red>Get in position!</color></size>",
            ">... Shadow sleepers everywhere.\r\n>... Bio Tracker essential.\r\n>... <size=200%><color=red>Tag every one!</color></size>",
            ">... C-Foam holding.\r\n>... For now.\r\n>... <size=200%><color=red>Keep moving back!</color></size>",
            ">... [static]\r\n>... REACTOR_SHUTDOWN command executed.\r\n>... <size=200%><color=red>Here we go!</color></size>",
            ">... Bioscan progress reversing!\r\n>... <size=200%><color=red>Back in the zone!</color></size>\r\n>... [panicked breathing]",
            ">... Sentries aren't tracking Shadows.\r\n>... <size=200%><color=red>Need Bio Tracker tags!</color></size>\r\n>... Tagging now!",
            ">... Extraction's so far.\r\n>... We'll make it.\r\n>... <size=200%><color=red>We have to!</color></size>",
            $">... [typing]\r\n>... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Verification complete!</color></size>",
            ">... Power just cut!\r\n>... Everything's dark!\r\n>... <size=200%><color=red>Flashlights on!</color></size>",
            ">... Multiple bioscans ahead.\r\n>... How many?\r\n>... <size=200%><color=red>Does it matter?!</color></size>",
            ">... Shadow just grabbed someone!\r\n>... <size=200%><color=red>Shoot it!</color></size>\r\n>... [screaming]",
            ">... Defender role assigned.\r\n>... Bioscan team's in position.\r\n>... <size=200%><color=red>Hold the line!</color></size>",
            ">... [panting]\r\n>... Almost to extraction.\r\n>... <size=200%><color=red>Final push!</color></size>",
            ">... Reactor terminal located.\r\n>... REACTOR_SHUTDOWN ready.\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... They're spawning faster!\r\n>... Complete the bioscans!\r\n>... <size=200%><color=red>Faster!</color></size>",
            ">... [alarm wailing]\r\n>... <size=200%><color=red>Can't deactivate this!</color></size>\r\n>... Just survive it!",
            ">... Organized retreat.\r\n>... Stay together.\r\n>... <size=200%><color=red>Watch your six!</color></size>",
            $">... Code verification: {verifyCode}.\r\n>... <size=200%><color=red>Entering now!</color></size>\r\n>... [typing rapidly]",
            ">... Bioscan complete!\r\n>... Another one starting!\r\n>... <size=200%><color=red>No breaks!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Reactor shutdown sequence active!</color></size>\r\n>... [klaxon]",
            ">... Shadows teleporting around us!\r\n>... Bio Tracker them!\r\n>... <size=200%><color=red>Tag everything!</color></size>",
            ">... Terminal's password locked!\r\n>... Found the password location.\r\n>... <size=200%><color=red>Going to get it!</color></size>",
            ">... Power severed.\r\n>... <size=200%><color=red>Lights are out!</color></size>\r\n>... Just as planned.",
            ">... [gunfire]\r\n>... <size=200%><color=red>Defend the bioscan!</color></size>\r\n>... Don't let them through!",
            ">... Running low on ammo.\r\n>... Ammo station ahead.\r\n>... <size=200%><color=red>Quick stop!</color></size>",
            $">... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Code entered!</color></size>\r\n>... [alarm blaring]",
            ">... Shadows everywhere!\r\n>... Can't see them without tags!\r\n>... <size=200%><color=red>Bio Tracker!</color></size>",
            ">... Two shotgun sentries.\r\n>... Outside the reactor room.\r\n>... <size=200%><color=red>Perfect position!</color></size>",
            ">... [panting]\r\n>... Bioscan sequence ongoing.\r\n>... <size=200%><color=red>Hold position!</color></size>",
            ">... C-Foam grenades ready.\r\n>... Foam every door.\r\n>... <size=200%><color=red>Slow them down!</color></size>",
            ">... They're still spawning!\r\n>... Until extraction.\r\n>... <size=200%><color=red>We knew this!</color></size>",
            ">... [typing]\r\n>... Verification code entered.\r\n>... <size=200%><color=red>Bioscans initiated!</color></size>",
            ">... Shadow teleported!\r\n>... <size=200%><color=red>Where'd it go?!</color></size>\r\n>... [screaming]",
            ">... Reactor's down.\r\n>... Mission's done.\r\n>... <size=200%><color=red>Now we just leave!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Security system malfunction detected!</color></size>\r\n>... [alarm wailing]",
            ">... Fall back to extraction!\r\n>... Fighting retreat!\r\n>... <size=200%><color=red>Move! Move!</color></size>",
            $">... Code's showing {verifyCode}.\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [frantic typing]",
            ">... Bioscan zone clear.\r\n>... Get in position.\r\n>... <size=200%><color=red>Starting scan!</color></size>",
            ">... [alarm blaring]\r\n>... Another wave!\r\n>... <size=200%><color=red>No end to this!</color></size>",
            ">... Terminal found.\r\n>... Entering shutdown command.\r\n>... <size=200%><color=red>REACTOR_SHUTDOWN!</color></size>",
            ">... Power's out everywhere.\r\n>... <size=200%><color=red>Dark all the way back!</color></size>\r\n>... Keep together!",
            ">... Shadows in the scan zone!\r\n>... Clear them!\r\n>... <size=200%><color=red>Now!</color></size>",
            ">... [gunfire]\r\n>... Sentry's down!\r\n>... <size=200%><color=red>Pick it up!</color></size>",
            $">... REACTOR_VERIFY {verifyCode}.\r\n>... Verification accepted.\r\n>... <size=200%><color=red>Here they come!</color></size>",
            ">... Bio Tracker essential!\r\n>... <size=200%><color=red>Tag the Shadows!</color></size>\r\n>... Sentries need targets!",
            ">... [panting]\r\n>... How many more bioscans?\r\n>... <size=200%><color=red>Just keep going!</color></size>",
            ">... C-Foam door behind us.\r\n>... Reinforcing.\r\n>... <size=200%><color=red>Buy us time!</color></size>",
            ">... They won't stop spawning!\r\n>... <size=200%><color=red>That's the alarm!</color></size>\r\n>... Can't shut it off!",
            ">... [static]\r\n>... Code verification required.\r\n>... <size=200%><color=red>It's on my HUD!</color></size>",
            ">... Bioscan progress at fifty percent.\r\n>... <size=200%><color=red>Don't leave the zone!</color></size>\r\n>... [gunfire]",
            ">... Shadow teleported again!\r\n>... Can't track them!\r\n>... <size=200%><color=red>Need Bio Tracker!</color></size>",
            ">... Reactor shutdown initiated.\r\n>... <size=200%><color=red>Power severing!</color></size>\r\n>... [electrical humming]",
            ">... Almost to extraction!\r\n>... Keep pushing!\r\n>... <size=200%><color=red>Don't stop!</color></size>",
            $">... [typing]\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [alarm blaring]",
            ">... Sentry positioned for bioscan.\r\n>... Covering main approach.\r\n>... <size=200%><color=red>Ready!</color></size>",
            ">... Verification timer's generous.\r\n>... Take your time.\r\n>... <size=200%><color=red>Just type it right!</color></size>",
            ">... [alarm wailing]\r\n>... Shadows spawning continuously!\r\n>... <size=200%><color=red>Tag and shoot!</color></size>",
            ">... Power's severed.\r\n>... Lights are gone.\r\n>... <size=200%><color=red>Visual contact lost!</color></size>",
            ">... Bioscan sequence nearly done.\r\n>... <size=200%><color=red>One more!</color></size>\r\n>... Then we run!",
            ">... [heavy breathing]\r\n>... Reactor terminal ahead.\r\n>... <size=200%><color=red>There it is!</color></size>",
            $">... Code's {verifyCode}.\r\n>... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... C-Foam holding the door.\r\n>... For now.\r\n>... <size=200%><color=red>Won't last!</color></size>",
            ">... [gunfire]\r\n>... <size=200%><color=red>Defend the bioscan team!</color></size>\r\n>... Don't let them reach!",
            ">... Shadow grabbed the defender!\r\n>... <size=200%><color=red>Shoot the Shadow!</color></size>\r\n>... [screaming]",
            ">... Terminal's unlocked.\r\n>... REACTOR_SHUTDOWN ready.\r\n>... <size=200%><color=red>Execute now!</color></size>",
            $">... [static]\r\n>... <size=200%><color=red>Verification code: {verifyCode}!</color></size>\r\n>... Entering!",
            ">... Bioscans complete!\r\n>... They're still spawning!\r\n>... <size=200%><color=red>Move to extraction!</color></size>",
            ">... Fighting retreat organized.\r\n>... Stay in formation.\r\n>... <size=200%><color=red>Watch the flanks!</color></size>",
            ">... [alarm blaring]\r\n>... Error alarm active!\r\n>... <size=200%><color=red>Can't deactivate!</color></size>",
            ">... Shadows everywhere!\r\n>... Bio Tracker working.\r\n>... <size=200%><color=red>Keep tagging!</color></size>",
            ">... Power's out.\r\n>... <size=200%><color=red>Everything's dark!</color></size>\r\n>... Use your lights!",
            ">... [panting]\r\n>... One bioscan left.\r\n>... <size=200%><color=red>Then we're done here!</color></size>",
            $">... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Code entered!</color></size>\r\n>... [klaxon blaring]",
            ">... Sentry's targeting correctly.\r\n>... Bio Tracker tags working.\r\n>... <size=200%><color=red>Good!</color></size>",
            ">... [typing frantically]\r\n>... Verification accepted.\r\n>... <size=200%><color=red>Bioscan starting!</color></size>",
            ">... Shadow teleported behind us!\r\n>... <size=200%><color=red>Turn around!</color></size>\r\n>... [gunfire]",
            ">... Reactor's offline.\r\n>... Objective complete.\r\n>... <size=200%><color=red>Now we survive!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>REACTOR_SHUTDOWN executed!</color></size>\r\n>... [alarm wailing]",
            ">... Multiple Shadows spawning!\r\n>... Tag them all!\r\n>... <size=200%><color=red>Bio Tracker!</color></size>",
            ">... Falling back through zones.\r\n>... C-Foam every door.\r\n>... <size=200%><color=red>Slow them down!</color></size>",
            $">... Code showing {verifyCode}.\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [typing]",
            ">... Bioscan at seventy percent.\r\n>... <size=200%><color=red>Stay in position!</color></size>\r\n>... Almost done!",
            ">... [alarm blaring]\r\n>... They keep coming!\r\n>... <size=200%><color=red>Just hold!</color></size>",
            ">... Terminal located.\r\n>... Ready to shut down.\r\n>... <size=200%><color=red>Do it!</color></size>",
            ">... Power severed everywhere.\r\n>... Lights out.\r\n>... <size=200%><color=red>Expected this!</color></size>",
            ">... [gunfire]\r\n>... Shadow in the bioscan!\r\n>... <size=200%><color=red>Kill it!</color></size>",
            $">... Verification code: {verifyCode}.\r\n>... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Entered!</color></size>",
            ">... Sentries positioned perfectly.\r\n>... Covering bioscan zones.\r\n>... <size=200%><color=red>Ready!</color></size>",
            ">... [panting]\r\n>... Running to extraction.\r\n>... <size=200%><color=red>Don't stop moving!</color></size>",
            ">... C-Foam deployed.\r\n>... Door reinforced.\r\n>... <size=200%><color=red>Won't hold long!</color></size>",
            ">... Shadows spawning continuously.\r\n>... <size=200%><color=red>Until we extract!</color></size>\r\n>... We knew this!",
            ">... [static]\r\n>... Bioscan sequence active.\r\n>... <size=200%><color=red>Hold position!</color></size>",
            ">... Shadow just vanished!\r\n>... Teleported!\r\n>... <size=200%><color=red>Where?!</color></size>",
            ">... Reactor shutdown complete.\r\n>... <size=200%><color=red>Now we leave!</color></size>\r\n>... Fighting retreat!",
            $">... [typing]\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [alarm blaring]",
            ">... Bio Tracker tagging Shadows.\r\n>... Sentries engaging.\r\n>... <size=200%><color=red>Working!</color></size>",
            ">... Power's out everywhere.\r\n>... Dark to extraction.\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... [alarm wailing]\r\n>... <size=200%><color=red>Another bioscan!</color></size>\r\n>... Get in the zone!",
            ">... Terminal's here.\r\n>... REACTOR_SHUTDOWN command.\r\n>... <size=200%><color=red>Executing!</color></size>",
            ">... Bioscan reversing!\r\n>... <size=200%><color=red>Back in!</color></size>\r\n>... [panicked shouting]",
            ">... Shadow spawns persistent.\r\n>... Won't stop.\r\n>... <size=200%><color=red>Just run!</color></size>",
            ">... [gunfire]\r\n>... Defending bioscan zone.\r\n>... <size=200%><color=red>Hold the line!</color></size>",
            $">... Code is {verifyCode}.\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [typing]",
            ">... Verification accepted.\r\n>... Bioscans starting.\r\n>... <size=200%><color=red>Here we go!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Reactor Security System Malfunction!</color></size>\r\n>... [klaxon]",
            ">... Almost to extraction!\r\n>... <size=200%><color=red>Keep moving!</color></size>\r\n>... Don't look back!",
            ">... Shadow teleported to bioscan!\r\n>... Clear it!\r\n>... <size=200%><color=red>Now!</color></size>",
            ">... C-Foam holding.\r\n>... Temporary.\r\n>... <size=200%><color=red>Keep falling back!</color></size>",
            ">... [panting]\r\n>... Power's severed.\r\n>... <size=200%><color=red>Lights are gone!</color></size>",
            $">... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Code entered!</color></size>\r\n>... [alarm blaring]",
            ">... Bioscan progress steady.\r\n>... Don't leave the zone.\r\n>... <size=200%><color=red>Stay!</color></size>",
            ">... Shadows everywhere!\r\n>... <size=200%><color=red>Tag with Bio Tracker!</color></size>\r\n>... [gunfire]",
            ">... Terminal found.\r\n>... Shutdown command ready.\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... [alarm wailing]\r\n>... Error alarm.\r\n>... <size=200%><color=red>Can't shut it off!</color></size>",
            ">... Fighting back to extraction.\r\n>... Stay together.\r\n>... <size=200%><color=red>Formation!</color></size>",
            $">... Code showing {verifyCode}.\r\n>... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... [static]\r\n>... Bioscan sequence initiated.\r\n>... <size=200%><color=red>Defend!</color></size>",
            ">... Sentries engaging Shadows.\r\n>... Bio Tracker tags active.\r\n>... <size=200%><color=red>Good!</color></size>",
            ">... Power severed.\r\n>... Everything's dark.\r\n>... <size=200%><color=red>As expected!</color></size>",
            ">... [gunfire]\r\n>... <size=200%><color=red>Shadow in the zone!</color></size>\r\n>... Kill it!",
            ">... Bioscans complete.\r\n>... Still spawning.\r\n>... <size=200%><color=red>Move out!</color></size>",
            ">... [typing frantically]\r\n>... Verification entered.\r\n>... <size=200%><color=red>Bioscans starting!</color></size>",
            ">... Shadow just grabbed someone!\r\n>... <size=200%><color=red>Shoot it!</color></size>\r\n>... [screaming]",
            ">... Reactor's down.\r\n>... Objective done.\r\n>... <size=200%><color=red>Extract now!</color></size>",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>REACTOR_SHUTDOWN active!</color></size>\r\n>... [klaxon]",
            ">... Falling back through zones.\r\n>... C-Foam doors.\r\n>... <size=200%><color=red>Slow them!</color></size>",
            $">... Code is {verifyCode}.\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [typing]",
            ">... Bioscan at ninety percent.\r\n>... <size=200%><color=red>Almost done!</color></size>\r\n>... Hold!",
            ">... [panting]\r\n>... They won't stop!\r\n>... <size=200%><color=red>Just keep moving!</color></size>",
            ">... Terminal located.\r\n>... REACTOR_SHUTDOWN.\r\n>... <size=200%><color=red>Executing now!</color></size>",
            ">... Power's out everywhere.\r\n>... <size=200%><color=red>Dark path back!</color></size>\r\n>... Stay together!",
            ">... Shadow teleported!\r\n>... Where'd it go?!\r\n>... <size=200%><color=red>Find it!</color></size>",
            ">... [gunfire]\r\n>... Defending the bioscan.\r\n>... <size=200%><color=red>Don't let them through!</color></size>",
            $">... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Code entered!</color></size>\r\n>... [alarm blaring]",
            ">... Bio Tracker essential.\r\n>... <size=200%><color=red>Tag all Shadows!</color></size>\r\n>... Sentries need it!",
            ">... [static]\r\n>... Verification accepted.\r\n>... <size=200%><color=red>Bioscans initiated!</color></size>",
            ">... C-Foam deployed on door.\r\n>... Reinforcing.\r\n>... <size=200%><color=red>Buy time!</color></size>",
            ">... Almost to extraction!\r\n>... Final push!\r\n>... <size=200%><color=red>Move!</color></size>",
            ">... [alarm wailing]\r\n>... Shadows spawning.\r\n>... <size=200%><color=red>Continuously!</color></size>",
            ">... Reactor shutdown complete.\r\n>... Mission done.\r\n>... <size=200%><color=red>Now we escape!</color></size>",
            $">... Code showing {verifyCode}.\r\n>... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... [typing]\r\n>... Verification done.\r\n>... <size=200%><color=red>Here they come!</color></size>",
            ">... Sentries positioned.\r\n>... Bioscan covered.\r\n>... <size=200%><color=red>Ready!</color></size>",
            ">... Power severed.\r\n>... <size=200%><color=red>Lights gone!</color></size>\r\n>... Flashlights on!",
            ">... [gunfire]\r\n>... Shadow in bioscan zone!\r\n>... <size=200%><color=red>Clear it!</color></size>",
            ">... Bioscan sequence ongoing.\r\n>... <size=200%><color=red>Hold position!</color></size>\r\n>... [alarm blaring]",
            ">... Shadow teleported again.\r\n>... Can't track.\r\n>... <size=200%><color=red>Bio Tracker!</color></size>",
            ">... Fighting retreat.\r\n>... Stay close.\r\n>... <size=200%><color=red>Don't separate!</color></size>",
            ">... [panting]\r\n>... Terminal's here.\r\n>... <size=200%><color=red>REACTOR_SHUTDOWN!</color></size>",
            $">... Code is {verifyCode}.\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [typing]",
            ">... Verification entered.\r\n>... Bioscans starting.\r\n>... <size=200%><color=red>Defend!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Security system malfunction!</color></size>\r\n>... [klaxon blaring]",
            ">... C-Foam the doors!\r\n>... Slowing them.\r\n>... <size=200%><color=red>Keep moving!</color></size>",
            ">... Shadows spawning continuously.\r\n>... Tag them all.\r\n>... <size=200%><color=red>Bio Tracker!</color></size>",
            ">... Power's out.\r\n>... Everything's dark.\r\n>... <size=200%><color=red>Expected!</color></size>",
            ">... [alarm blaring]\r\n>... Another bioscan.\r\n>... <size=200%><color=red>Get in!</color></size>",
            ">... Bioscan progress good.\r\n>... <size=200%><color=red>Don't leave zone!</color></size>\r\n>... [gunfire]",
            ">... Shadow grabbed defender!\r\n>... <size=200%><color=red>Shoot the Shadow!</color></size>\r\n>... [screaming]",
            $">... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Code entered!</color></size>\r\n>... [alarm wailing]",
            ">... Terminal found.\r\n>... Shutdown ready.\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... [typing frantically]\r\n>... Verification accepted.\r\n>... <size=200%><color=red>Bioscans active!</color></size>",
            ">... Reactor's offline.\r\n>... <size=200%><color=red>Now we run!</color></size>\r\n>... To extraction!",
            ">... Sentries engaging.\r\n>... Bio Tracker working.\r\n>... <size=200%><color=red>Good!</color></size>",
            ">... [panting]\r\n>... Almost there.\r\n>... <size=200%><color=red>Keep pushing!</color></size>",
            ">... C-Foam holding door.\r\n>... Won't last.\r\n>... <size=200%><color=red>Move!</color></size>",
            ">... Shadow teleported!\r\n>... <size=200%><color=red>Where is it?!</color></size>\r\n>... [gunfire]",
            $">... Code showing {verifyCode}.\r\n>... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... [static]\r\n>... Bioscan initiated.\r\n>... <size=200%><color=red>Defend the zone!</color></size>",
            ">... Power severed everywhere.\r\n>... Dark now.\r\n>... <size=200%><color=red>Use lights!</color></size>",
            ">... [alarm blaring]\r\n>... Error alarm active.\r\n>... <size=200%><color=red>Can't stop it!</color></size>",
            ">... Bioscans complete.\r\n>... Still spawning.\r\n>... <size=200%><color=red>Extract!</color></size>",
            ">... Fighting back through zones.\r\n>... C-Foam everything.\r\n>... <size=200%><color=red>Slow them!</color></size>",
            $">... [typing]\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [klaxon]",
            ">... Shadows everywhere.\r\n>... Tag them.\r\n>... <size=200%><color=red>Bio Tracker!</color></size>",
            ">... Terminal located.\r\n>... REACTOR_SHUTDOWN.\r\n>... <size=200%><color=red>Now!</color></size>",
            ">... Bioscan at fifty percent.\r\n>... <size=200%><color=red>Stay in zone!</color></size>\r\n>... Don't move!",
            ">... [gunfire]\r\n>... <size=200%><color=red>Shadow in the scan!</color></size>\r\n>... Kill it!",
            $">... Code is {verifyCode}.\r\n>... <size=200%><color=red>REACTOR_VERIFY {verifyCode}!</color></size>\r\n>... [typing]",
            ">... Verification done.\r\n>... Bioscans starting.\r\n>... <size=200%><color=red>Ready!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Reactor shutdown active!</color></size>\r\n>... [alarm wailing]",
            ">... Sentries positioned well.\r\n>... Covering approaches.\r\n>... <size=200%><color=red>Good!</color></size>",
            ">... Power's out.\r\n>... <size=200%><color=red>Dark everywhere!</color></size>\r\n>... Expected this!",
            ">... [panting]\r\n>... Extraction's close.\r\n>... <size=200%><color=red>Final push!</color></size>",
            ">... Shadow spawns continuous.\r\n>... Until extraction.\r\n>... <size=200%><color=red>We knew!</color></size>",
            ">... C-Foam deployed.\r\n>... Door reinforced.\r\n>... <size=200%><color=red>Temporary!</color></size>",
            ">... [alarm blaring]\r\n>... Another bioscan.\r\n>... <size=200%><color=red>Position!</color></size>",
            $">... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Code entered!</color></size>\r\n>... [klaxon blaring]",
            ">... Bioscan reversing.\r\n>... <size=200%><color=red>Back in!</color></size>\r\n>... [panicked breathing]",
            ">... Shadow teleported behind.\r\n>... Turn around!\r\n>... <size=200%><color=red>Shoot!</color></size>",
            ">... [typing frantically]\r\n>... Verification accepted.\r\n>... <size=200%><color=red>Bioscans go!</color></size>",
            ">... Terminal's here.\r\n>... Shutdown ready.\r\n>... <size=200%><color=red>Do it!</color></size>",
            ">... Reactor's down.\r\n>... <size=200%><color=red>Mission complete!</color></size>\r\n>... Now survive!",
            ">... [gunfire]\r\n>... Defend bioscan team.\r\n>... <size=200%><color=red>Hold line!</color></size>",
            $">... Code showing {verifyCode}.\r\n>... REACTOR_VERIFY {verifyCode}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... [static]\r\n>... Bioscan sequence active.\r\n>... <size=200%><color=red>Defend!</color></size>",
            ">... Bio Tracker tagging.\r\n>... Sentries engaging.\r\n>... <size=200%><color=red>Working!</color></size>",
            ">... Power severed.\r\n>... Lights gone.\r\n>... <size=200%><color=red>Flashlights!</color></size>",
            ">... [alarm wailing]\r\n>... Shadows spawning.\r\n>... <size=200%><color=red>Tag all!</color></size>",
            ">... Fighting retreat ongoing.\r\n>... Stay together.\r\n>... <size=200%><color=red>Formation!</color></size>",
            ">... [panting]\r\n>... Almost extracted.\r\n>... <size=200%><color=red>Don't stop!</color></size>",
        }))!);
        #endregion
    }
}
