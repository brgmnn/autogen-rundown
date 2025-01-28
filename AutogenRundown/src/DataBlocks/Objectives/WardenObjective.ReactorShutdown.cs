using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.Reactor;

namespace AutogenRundown.DataBlocks;

public partial record class WardenObjective : DataBlock
{
    public void PreBuild_ReactorShutdown(BuildDirector director, Level level)
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
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void Build_ReactorShutdown(BuildDirector director, Level level)
    {
        FindLocationInfo = "Make sure the Reactor is fully shut down before leaving";
        // SolveItem = "Make sure the Reactor is fully shut down before leaving";
        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_ToMainLayer = "Go back to the main objective and complete the expedition.";

        LightsOnFromBeginning = true;
        LightsOnDuringIntro = true;
        LightsOnWhenStartupComplete = false;

        StartPuzzle = ChainedPuzzle.FindOrPersist(ChainedPuzzle.TeamScan);

        // TODO: it doesn't spawn the extract scan?

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
                    (6.0, ChainedPuzzle.AlarmClass5 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 20.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0,
                    }),
                    (6.0, ChainedPuzzle.AlarmClass5_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Normal,
                        WantedDistanceFromStartPos = 20.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0,
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5),

                    // Tiny chance they get a free scan instead of the boss bait
                    (0.25, ChainedPuzzle.StealthScan4)
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

                reactorDefinition.EventsOnComplete.AddGenericWave(new GenericWave
                {
                    WaveSettings = director.Bulkhead switch
                    {
                        Bulkhead.Main => WaveSettings.Exit_Objective_Medium.PersistentId,
                        Bulkhead.Extreme => WaveSettings.Exit_Objective_Easy.PersistentId,
                        Bulkhead.Overload => WaveSettings.Exit_Objective_Easy.PersistentId,
                    },
                    WavePopulation = WavePopulation.Baseline.PersistentId,
                    TriggerAlarm = true
                });
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
                    (6.0, ChainedPuzzle.AlarmClass5 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 30.0
                    }),
                    (6.0, ChainedPuzzle.AlarmClass5_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Normal,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 30.0
                    }),
                    (5.0, ChainedPuzzle.AlarmClass6_Mixed with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Normal,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5),

                    // Tiny chance they get a free scan instead of the boss bait
                    (0.25, ChainedPuzzle.StealthScan4)
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
                reactorDefinition.EventsOnComplete.AddGenericWave(new GenericWave
                {
                    WaveSettings = director.Bulkhead switch
                    {
                        Bulkhead.Main => WaveSettings.Exit_Objective_Medium.PersistentId,
                        Bulkhead.Extreme => WaveSettings.Exit_Objective_Easy.PersistentId,
                        Bulkhead.Overload => WaveSettings.Exit_Objective_Easy.PersistentId,
                    },
                    WavePopulation = WavePopulation.Baseline.PersistentId,
                    TriggerAlarm = true
                });
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
                    (6.0, ChainedPuzzle.AlarmClass5 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (6.0, ChainedPuzzle.AlarmClass5_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (5.0, ChainedPuzzle.AlarmClass6_Mixed with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_Hard,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5),

                    // Tiny chance they get a free scan instead of the boss bait
                    (0.25, ChainedPuzzle.StealthScan4)
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
                reactorDefinition.EventsOnComplete.AddGenericWave(new GenericWave
                {
                    WaveSettings = director.Bulkhead switch
                    {
                        Bulkhead.Main => WaveSettings.Exit_Objective_Hard.PersistentId,
                        Bulkhead.Extreme => WaveSettings.Exit_Objective_Easy.PersistentId,
                        Bulkhead.Overload => WaveSettings.Exit_Objective_Medium.PersistentId,
                    },
                    WavePopulation = WavePopulation.Baseline.PersistentId,
                    TriggerAlarm = true
                });
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
                    (4.0, ChainedPuzzle.AlarmClass5 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (4.0, ChainedPuzzle.AlarmClass5_Cluster with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (3.0, ChainedPuzzle.AlarmClass6_Mixed with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 30.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),
                    // This may be extremely hard to do
                    (2.0, ChainedPuzzle.AlarmClass2_Surge with
                    {
                        WantedDistanceFromStartPos = 20.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5),

                    // Tiny chance they get a free scan instead of the boss bait
                    (0.25, ChainedPuzzle.StealthScan4)
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
                reactorDefinition.EventsOnComplete.AddGenericWave(new GenericWave
                {
                    WaveSettings = director.Bulkhead switch
                    {
                        Bulkhead.Main => WaveSettings.Exit_Objective_VeryHard.PersistentId,
                        Bulkhead.Extreme => WaveSettings.Exit_Objective_Easy.PersistentId,
                        Bulkhead.Overload => WaveSettings.Exit_Objective_Medium.PersistentId,
                    },
                    WavePopulation = WavePopulation.Baseline.PersistentId,
                    TriggerAlarm = true
                });
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
                        .AddGenericWave(GenericWave.SinglePMother, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleMother, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleMother, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleMother, surpriseBossDelay)
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTank, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncer, surpriseBossDelay + Generator.Between(10, 15))
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),

                    (new List<WardenObjectiveEvent>())
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SingleTankPotato, surpriseBossDelay)
                        .AddGenericWave(GenericWave.SinglePouncer, surpriseBossDelay + Generator.Between(10, 15))
                        .AddSound(Sound.Enemies_DistantLowRoar, surpriseBossDelay)
                        .AddMessage(":://WARNING - BIOMASS SIGNATURE", surpriseBossDelay - 2)
                        .ToList(),
                })!;

                puzzle = Generator.Select(new List<(double, ChainedPuzzle)>
                {
                    (3.0, ChainedPuzzle.AlarmClass6 with
                    {
                        Population = population,
                        Settings = WaveSettings.Baseline_VeryHard,
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 40.0
                    }),
                    (3.0, ChainedPuzzle.AlarmClass6_Cluster with
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
                        WantedDistanceFromStartPos = 40.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),
                    // This may be extremely hard to do
                    (2.0, ChainedPuzzle.AlarmClass3_Surge with
                    {
                        WantedDistanceFromStartPos = 20.0,
                        WantedDistanceBetweenPuzzleComponents = 20.0
                    }),

                    // This is the boss bait scan
                    (1.0, ChainedPuzzle.StealthScan5),

                    // Tiny chance they get a free scan instead of the boss bait
                    (0.25, ChainedPuzzle.StealthScan4)
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
                reactorDefinition.EventsOnComplete.AddGenericWave(new GenericWave
                {
                    WaveSettings = director.Bulkhead switch
                    {
                        Bulkhead.Main => WaveSettings.Baseline_Normal.PersistentId,
                        Bulkhead.Extreme => WaveSettings.Exit_Objective_Easy.PersistentId,
                        Bulkhead.Overload => WaveSettings.Exit_Objective_Medium.PersistentId,
                    },
                    WavePopulation = WavePopulation.Baseline.PersistentId,
                    TriggerAlarm = true
                });
                break;
            }
        }

        reactorDefinition.PuzzleOnVerification = ChainedPuzzle.FindOrPersist(puzzle);
    }

    public void PostBuild_ReactorShutdown(BuildDirector director, Level level)
    {
        // EOS needs this to be set to empty to work
        Type = WardenObjectiveType.Empty;

        LayoutDefinitions.MainLevelLayout = level.GetLevelLayout(director.Bulkhead)!.PersistentId;
        LayoutDefinitions.Save();
    }
}
