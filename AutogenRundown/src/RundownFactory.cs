using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.GeneratorData;

namespace AutogenRundown;

public static class RundownFactory
{
    private static string DescriptionHeader(WardenObjectiveType objectiveType)
    {
        const string prefix = "<color=#444444>Objective Dispatch:</color> ";

        return objectiveType switch
        {
            WardenObjectiveType.HsuFindSample => prefix + "<color=orange>HSU Extraction</color>\n",
            WardenObjectiveType.ReactorStartup => prefix + "<color=orange>Reactor Startup</color>\n",
            WardenObjectiveType.ReactorShutdown => prefix + "<color=orange>Reactor Shutdown</color>\n",
            WardenObjectiveType.GatherSmallItems => prefix + "<color=orange>Gather Items</color>\n",
            WardenObjectiveType.ClearPath => prefix + "<color=orange>Sector Survey</color>\n",
            WardenObjectiveType.SpecialTerminalCommand => prefix + "<color=orange>Manual Terminal Override</color>\n",
            WardenObjectiveType.RetrieveBigItems => prefix + "<color=orange>Package Extraction</color>\n",
            WardenObjectiveType.PowerCellDistribution => prefix + "<color=orange>Power Cell Distribution</color>\n",
            WardenObjectiveType.TerminalUplink => prefix + "<color=orange>Network Uplink</color>\n",
            WardenObjectiveType.CentralGeneratorCluster => prefix + "<color=orange>Generator Cluster</color>\n",
            WardenObjectiveType.HsuActivateSmall => prefix + "<color=orange>Activate Item</color>\n",
            WardenObjectiveType.Survival => prefix + "<color=orange>Diversion Protocol</color>\n",
            WardenObjectiveType.GatherTerminal => prefix + "<color=orange>Extract Terminal Keys</color>\n",
            WardenObjectiveType.CorruptedTerminalUplink => prefix + "<color=orange>Dual Network Uplink</color>\n",
            WardenObjectiveType.TimedTerminalSequence => prefix + "<color=orange>Timed Sequence</color>\n",

            _ => ""
        };
    }

    /// <summary>
    /// Generates a rundown
    /// </summary>
    /// <param name="newRundown"></param>
    /// <param name="withFixed"></param>
    /// <param name="withUnlocks"></param>
    /// <returns></returns>
    public static Rundown BuildRundown(
        Rundown newRundown,
        bool withFixed = true,
        bool withUnlocks = true)
    {
        var rundown = Rundown.Build(newRundown);
        var levelNames = Words.NewLevelNamesPack();

        #region A-Tier Levels
        for (int i = 0; i < rundown.TierA_Count; i++)
        {
            var buildSeed = 1;

            if (rundown.BuildSeedPool.Count > 0)
            {
                buildSeed = rundown.BuildSeedPool[0];
                rundown.BuildSeedPool.RemoveAt(0);

                Generator.AdvanceSequence(buildSeed);
            }

            var level = Level.Build(
                new Level("A")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        #region Regular debugging level
        #if DEBUG
        // if (withFixed)
        // {
        //     var mainDirector = new BuildDirector
        //     {
        //         Bulkhead = Bulkhead.Main,
        //         Complex = Complex.Mining,
        //         Complexity = Complexity.Low,
        //         Tier = "D",
        //         Objective = WardenObjectiveType.HsuFindSample,
        //     };
        //     mainDirector.GenPoints();
        //
        //     var extremeDirector = new BuildDirector
        //     {
        //         Bulkhead = Bulkhead.Extreme,
        //         Complex = Complex.Mining,
        //         Complexity = Complexity.Low,
        //         Tier = "D",
        //         Objective = WardenObjectiveType.HsuFindSample,
        //     };
        //     extremeDirector.GenPoints();
        //
        //     var settings = new LevelSettings("D");
        //     settings.Modifiers.Add(LevelModifiers.Chargers);
        //
        //     var testLevel = Level.Build(
        //         new Level("D")
        //         {
        //             Tier = "D",
        //             Name = "Debug Test",
        //             Complex = Complex.Mining,
        //             MainDirector = mainDirector,
        //             // SecondaryDirector = extremeDirector,
        //             Settings = settings,
        //             Index = rundown.TierB_Count + 1,
        //             IsTest = true
        //         });
        //     rundown.AddLevel(testLevel);
        // }
        #endif
        #endregion

        #region Geomorph Debugging test level
        #if DEBUG
        #if false
        if (withFixed)
        {
            var settings = new LevelSettings("A");

            var testLevel = Level.Debug_BuildGeoTest(
                "Assets/Custom Geo's/Floodways_x_tile_4/floodway_x_tile_4.prefab",
                new Level("A")
                {
                    Tier = "A",
                    Name = "Debug Test",
                    Complex = Complex.Service,
                    Settings = settings,
                    Index = rundown.TierA_Count + 1,
                    IsTest = true
                }, 0);

            rundown.AddLevel(testLevel);
        }
        #endif
        #endif
        #endregion
        #endregion

        #region B-Tier Levels
        for (int i = 0; i < rundown.TierB_Count; i++)
        {
            var buildSeed = 1;

            if (rundown.BuildSeedPool.Count > 0)
            {
                buildSeed = rundown.BuildSeedPool[0];
                rundown.BuildSeedPool.RemoveAt(0);

                Generator.AdvanceSequence(buildSeed);
            }

            var level = Level.Build(
                new Level("B")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }
        #endregion

        #region C-Tier Levels
        for (int i = 0; i < rundown.TierC_Count; i++)
        {
            var buildSeed = 1;

            if (rundown.BuildSeedPool.Count > 0)
            {
                buildSeed = rundown.BuildSeedPool[0];
                rundown.BuildSeedPool.RemoveAt(0);

                Generator.AdvanceSequence(buildSeed);
            }

            var level = Level.Build(
                new Level("C")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        #region Test C Levels
        #if DEBUG
        if (withFixed)
        {
            const string tier = "D";
            const string title = "Cluster";
            const Complex complex = Complex.Service;

            var mainDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Complexity = Complexity.Low,
                Tier = tier,
                Objective = WardenObjectiveType.CentralGeneratorCluster,
            };
            mainDirector.GenPoints();

            var secondDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Extreme,
                Complex = complex,
                Complexity = Complexity.Low,
                Tier = tier,
                Objective = WardenObjectiveType.GatherSmallItems,
            };
            secondDirector.GenPoints();

            var thirdDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Overload,
                Complex = complex,
                Complexity = Complexity.Low,
                Tier = tier,
                Objective = WardenObjectiveType.GatherSmallItems,
            };
            thirdDirector.GenPoints();

            var testLevel = Level.Build(
                new Level(tier)
                {
                    Tier = tier,
                    Name = title,
                    Complex = complex,
                    MainDirector = mainDirector,
                    SecondaryDirector = secondDirector,
                    OverloadDirector = thirdDirector,
                    Settings = new LevelSettings(tier)
                    {
                        Bulkheads = Bulkhead.Main
                        // Bulkheads = Bulkhead.Main | Bulkhead.Extreme
                        // Bulkheads = Bulkhead.Main | Bulkhead.Overload
                        // Bulkheads = Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload
                    },
                    Index = rundown.TierC_Count + 1,
                    IsTest = true
                });
            rundown.AddLevel(testLevel);
        }
        #endif
        #endregion
        #endregion

        #region D-Tier Levels
        #region Survival Fixed
        // #if DEBUG
        #if true
        if (withFixed)
        {
            var objective = WardenObjectiveType.Survival;
            var mainDirector = new BuildDirector()
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Tech,
                Tier = "B",
                Objective = objective
            };
            mainDirector.GenPoints();

            var extremeDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Extreme,
                Complex = Complex.Tech,
                Complexity = Complexity.Low,
                Tier = "B",
                Objective = WardenObjectiveType.SpecialTerminalCommand,
            };
            extremeDirector.GenPoints();

            // var overloadDirector = new BuildDirector
            // {
            //     Bulkhead = Bulkhead.Overload,
            //     Complex = Complex.Tech,
            //     Complexity = Complexity.Low,
            //     Tier = "C",
            //     Objective = WardenObjectiveType.SpecialTerminalCommand,
            // };
            // overloadDirector.GenPoints();

            var settings = new LevelSettings("B");
            // settings.Modifiers.Add(LevelModifiers.Chargers);
            var description = new DataBlocks.Text(DescriptionHeader(objective) +
                                                  DataBlocks.WardenObjective.GenLevelDescription(objective));
            var level = Level.Build(
                new("B")
                {
                    Prefix = $"<color=orange>X</color><color=#444444>:</color>B",
                    Description = description.PersistentId,
                    Complex = Complex.Tech,
                    MainDirector = mainDirector,
                    SecondaryDirector = extremeDirector,
                    // OverloadDirector = overloadDirector,
                    Settings = settings,
                    Index = rundown.TierB_Count
                });

            rundown.AddLevel(level);
        }
        #endif
        // #endif
        #endregion

        // D levels
        for (int i = 1; i < rundown.TierD_Count + 1; i++)
        {
            var buildSeed = 1;

            if (rundown.BuildSeedPool.Count > 0)
            {
                buildSeed = rundown.BuildSeedPool[0];
                rundown.BuildSeedPool.RemoveAt(0);

                Generator.AdvanceSequence(buildSeed);
            }

            var level = Level.Build(
                new Level("D")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        #region Timed Terminal Sequence
        #if DEBUG
        #if false
        if (withFixed)
        {
            ///
            /// Timed Terminal Sequence mission. This is a hard coded mission of sequence stuff
            ///
            var objective = WardenObjectiveType.TimedTerminalSequence;
            var mainDirector = new BuildDirector()
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Mining,
                Tier = "D",
                Objective = objective
            };
            mainDirector.GenPoints();

            var settings = new LevelSettings("D");
            var description = new DataBlocks.Text(DescriptionHeader(objective) +
                                                  DataBlocks.WardenObjective.GenLevelDescription(objective));
            var level = Level.Build(
                new("D")
                {
                    Prefix = $"<color=orange>W</color><color=#444444>:</color>D",
                    Description = description.PersistentId,
                    Complex = Complex.Mining,
                    MainDirector = mainDirector,
                    Settings = settings,
                    Index = rundown.TierD_Count + 2
                });

            rundown.AddLevel(level);
        }
        #endif
        #endif
        #endregion

        #region Reactor startup
        #if DEBUG
        #if false
        if (withFixed)
        {
            ///
            /// Reactor startup mission. This will always spawn a reactor startup mission as
            /// the main objective. Side objectives can still spawn.
            ///
            var objective = WardenObjectiveType.ReactorStartup;
            var mainDirector = new BuildDirector()
            {
                Bulkhead = Bulkhead.Main,
                Tier = "D",
                Objective = objective
            };
            mainDirector.GenPoints();

            var settings = new LevelSettings("D");
            var description = new DataBlocks.Text(DescriptionHeader(objective) +
                                                  DataBlocks.WardenObjective.GenLevelDescription(objective));
            var level = Level.Build(
                new("D")
                {
                    Prefix = $"<color=orange>R</color><color=#444444>:</color>D",
                    Description = description.PersistentId,
                    MainDirector = mainDirector,
                    Settings = settings,
                    Index = rundown.TierD_Count + 3
                });

            rundown.AddLevel(level);
        }
        #endif
        #endif
        #endregion
        #endregion

        #region E-Tier Levels
        // E levels
        for (int i = 0; i < rundown.TierE_Count; i++)
        {
            var buildSeed = 1;

            if (rundown.BuildSeedPool.Count > 0)
            {
                buildSeed = rundown.BuildSeedPool[0];
                rundown.BuildSeedPool.RemoveAt(0);

                Generator.AdvanceSequence(buildSeed);
            }

            var level = Level.Build(
                new Level("E")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        #region Test E Levels
        #if DEBUG
        if (false && withFixed)
        {
            var mainDirectorE = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Mining,
                Complexity = Complexity.Low,
                Tier = "E",
                Objective = WardenObjectiveType.ReactorStartup,
            };
            mainDirectorE.GenPoints();

            var settingsE = new LevelSettings("E");
            settingsE.Modifiers.Add(LevelModifiers.ManyChargers);
            settingsE.Modifiers.Add(LevelModifiers.ManyShadows);

            var testLevelE = Level.Build(
                new Level("E")
                {
                    Tier = "E",
                    Name = "Terminal Command",
                    Complex = Complex.Tech,
                    MainDirector = mainDirectorE,
                    Settings = settingsE,
                    Index = rundown.TierE_Count + 1,
                    IsTest = true
                });
            rundown.AddLevel(testLevelE);
        }
        #endif
        #endregion
        #endregion

        // Return without unlocks if we don't want them
        if (!withUnlocks)
            return rundown;

        // Add progression requirements if unlocks are needed
        rundown.UseTierUnlockRequirements = true;

        rundown.ReqToReachTierB.MainSectors = Math.Max(1, rundown.TierA.Count - 1);
        rundown.ReqToReachTierC.MainSectors = Math.Max(0, rundown.ReqToReachTierB.MainSectors + rundown.TierB.Count - 1);
        rundown.ReqToReachTierD.MainSectors = Math.Max(0, rundown.ReqToReachTierC.MainSectors + rundown.TierC.Count - 1);
        rundown.ReqToReachTierE.MainSectors = Math.Max(0, rundown.ReqToReachTierD.MainSectors + rundown.TierD.Count - 1);

        return rundown;
    }

        /// <summary>
    /// Dedicated method for building the monthly rundown
    /// </summary>
    /// <param name="rundown"></param>
    /// <returns></returns>
    public static Rundown BuildSeasonalRundown(Rundown newRundown)
    {
        var rundown = Rundown.Build(newRundown);
        var levelNames = Words.NewLevelNamesPack();

        //
        // Use the seed pool to re-roll levels. Start by setting these at 1 and incrementing each time
        // there's a level lockup
        //
        // TODO: how would we cover this on longer time scales
        //
        var buildPools = new Dictionary<string, List<List<int>>>
        {
            {
                "2025_08", new List<List<int>>
                {
                    new() { 1, 1 },
                    new() { 1, 1 },
                    new() { 1, 2, 1, 1 },
                    new() { 1, 2, 1, 2 },
                    new() { 1, 2, 1 }
                }
            }
        };

        #region Cross-level specific setup

        buildPools.TryGetValue(Generator.Seed, out var buildSeeds);

        // Set a default list of build seeds for months we don't roll
        if (buildSeeds is null)
            buildSeeds = new List<List<int>>
            {
                /* Tier A */ new() { 1, 1 },
                /* Tier B */ new() { 1, 1, 1 },
                /* Tier C */ new() { 1, 1, 1, 1 },
                /* Tier D */ new() { 1, 1, 1, 1 },
                /* Tier E */ new() { 1, 1 }
            };

        // Define a rundown global pool of objectives for the main objective.
        // This is to help us high rolling one particular objective over
        // another. So we avoid having a rundown with only ReactorStartup
        var mainObjectivesPool = new List<(double, int, WardenObjectiveType)>
        {
            (1.0, 3, WardenObjectiveType.HsuFindSample),
            (1.0, 3, WardenObjectiveType.ReactorStartup),
            (1.0, 3, WardenObjectiveType.ReactorShutdown),
            (1.0, 2, WardenObjectiveType.GatherSmallItems),
            (1.0, 2, WardenObjectiveType.ClearPath),
            (1.0, 1, WardenObjectiveType.SpecialTerminalCommand),
            (1.0, 2, WardenObjectiveType.RetrieveBigItems),
            (1.0, 2, WardenObjectiveType.PowerCellDistribution),
            (1.0, 3, WardenObjectiveType.TerminalUplink),
            (1.0, 3, WardenObjectiveType.CentralGeneratorCluster),
            (1.0, 3, WardenObjectiveType.HsuActivateSmall),
            (1.0, 3, WardenObjectiveType.Survival),
            (1.0, 2, WardenObjectiveType.GatherTerminal),
            (1.0, 2, WardenObjectiveType.CorruptedTerminalUplink),
            (1.0, 2, WardenObjectiveType.TimedTerminalSequence),
        };
        var complexPool = new List<(double, int, Complex)>
        {
            (1.0, 10, Complex.Mining),
            (1.0, 10, Complex.Tech),
            (0.8, 10, Complex.Service)
        };

        rundown.TierA_Count = buildSeeds[0].Count;
        rundown.TierB_Count = buildSeeds[1].Count;
        rundown.TierC_Count = buildSeeds[2].Count;
        rundown.TierD_Count = buildSeeds[3].Count;
        rundown.TierE_Count = buildSeeds[4].Count;

        #endregion

        // --- A-Tier Levels ---
        for (var i = 0; i < rundown.TierA_Count; i++)
        {
            var buildSeed = buildSeeds[0][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "A",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("A")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // --- B-Tier Levels ---
        for (var i = 0; i < rundown.TierB_Count; i++)
        {
            var buildSeed = buildSeeds[1][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "B",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("B")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // --- C-Tier Levels ---
        for (var i = 0; i < rundown.TierC_Count; i++)
        {
            var buildSeed = buildSeeds[2][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "C",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("C")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // --- D-Tier Levels ---
        for (var i = 0; i < rundown.TierD_Count; i++)
        {
            var buildSeed = buildSeeds[3][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "D",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("D")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // --- E-Tier Levels ---
        for (var i = 0; i < rundown.TierE_Count; i++)
        {
            var buildSeed = buildSeeds[4][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "E",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("E")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // Add progression requirements if unlocks are needed
        // Only add this for the non-debug build
        #if !DEBUG
        rundown.UseTierUnlockRequirements = true;

        rundown.ReqToReachTierB.MainSectors = Math.Max(1, rundown.TierA.Count - 1);
        rundown.ReqToReachTierC.MainSectors = Math.Max(0, rundown.ReqToReachTierB.MainSectors + rundown.TierB.Count - 1);
        rundown.ReqToReachTierD.MainSectors = Math.Max(0, rundown.ReqToReachTierC.MainSectors + rundown.TierC.Count - 1);
        rundown.ReqToReachTierE.MainSectors = Math.Max(0, rundown.ReqToReachTierD.MainSectors + rundown.TierD.Count - 1);
        #endif

        rundown.VisualsETier = Color.MenuVisuals_SeasonalE;

        return rundown;
    }


    /// <summary>
    /// Dedicated method for building the monthly rundown
    /// </summary>
    /// <param name="rundown"></param>
    /// <returns></returns>
    public static Rundown BuildMonthlyRundown(Rundown newRundown)
    {
        var rundown = Rundown.Build(newRundown);
        var levelNames = Words.NewLevelNamesPack();

        //
        // Use the seed pool to re-roll levels. Start by setting these at 1 and incrementing each time
        // there's a level lockup
        //
        // TODO: how would we cover this on longer time scales
        //
        var buildPools = new Dictionary<string, List<List<int>>>
        {
            {
                "2025_07", new List<List<int>>
                {
                    new() { 1, 2 },
                    new() { 1, 1, 1 },
                    new() { 4, 1, 1, 1 },
                    new() { 2, 1, 1, 2 },
                    new() { 1, 1, 1 }
                }
            },
            {
                "2025_08", new List<List<int>>
                {
                    new() { 1, 1 },
                    new() { 1, 1 },
                    new() { 1, 2, 1, 1 },
                    new() { 1, 2, 1, 2 },
                    new() { 1, 2, 1 }
                }
            }
        };

        #region Cross-level specific setup

        buildPools.TryGetValue(Generator.Seed, out var buildSeeds);

        // Set a default list of build seeds for months we don't roll
        if (buildSeeds is null)
            buildSeeds = new List<List<int>>
            {
                /* Tier A */ new() { 2, 1 },
                /* Tier B */ new() { 1, 1, 1 },
                /* Tier C */ new() { 1, 1, 1, 1 },
                /* Tier D */ new() { 2, 1, 1, 1 },
                /* Tier E */ new() { 1, 1 }
            };

        // Define a rundown global pool of objectives for the main objective.
        // This is to help us high rolling one particular objective over
        // another. So we avoid having a rundown with only ReactorStartup
        var mainObjectivesPool = new List<(double, int, WardenObjectiveType)>
        {
            (1.0, 3, WardenObjectiveType.HsuFindSample),
            (1.0, 3, WardenObjectiveType.ReactorStartup),
            (1.0, 3, WardenObjectiveType.ReactorShutdown),
            (1.0, 2, WardenObjectiveType.GatherSmallItems),
            (1.0, 2, WardenObjectiveType.ClearPath),
            (1.0, 1, WardenObjectiveType.SpecialTerminalCommand),
            (1.0, 2, WardenObjectiveType.RetrieveBigItems),
            (1.0, 2, WardenObjectiveType.PowerCellDistribution),
            (1.0, 3, WardenObjectiveType.TerminalUplink),
            (1.0, 3, WardenObjectiveType.CentralGeneratorCluster),
            (1.0, 3, WardenObjectiveType.HsuActivateSmall),
            (1.0, 3, WardenObjectiveType.Survival),
            (1.0, 2, WardenObjectiveType.GatherTerminal),
            (1.0, 2, WardenObjectiveType.CorruptedTerminalUplink),
            (1.0, 2, WardenObjectiveType.TimedTerminalSequence),
        };
        var complexPool = new List<(double, int, Complex)>
        {
            (1.0, 10, Complex.Mining),
            (1.0, 10, Complex.Tech),
            (0.8, 10, Complex.Service)
        };

        rundown.TierA_Count = buildSeeds[0].Count;
        rundown.TierB_Count = buildSeeds[1].Count;
        rundown.TierC_Count = buildSeeds[2].Count;
        rundown.TierD_Count = buildSeeds[3].Count;
        rundown.TierE_Count = buildSeeds[4].Count;

        #endregion

        // --- A-Tier Levels ---
        for (var i = 0; i < rundown.TierA_Count; i++)
        {
            var buildSeed = buildSeeds[0][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "A",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("A")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // --- B-Tier Levels ---
        for (var i = 0; i < rundown.TierB_Count; i++)
        {
            var buildSeed = buildSeeds[1][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "B",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("B")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // --- C-Tier Levels ---
        for (var i = 0; i < rundown.TierC_Count; i++)
        {
            var buildSeed = buildSeeds[2][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "C",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("C")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // --- D-Tier Levels ---
        for (var i = 0; i < rundown.TierD_Count; i++)
        {
            var buildSeed = buildSeeds[3][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "D",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("D")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // --- E-Tier Levels ---
        for (var i = 0; i < rundown.TierE_Count; i++)
        {
            var buildSeed = buildSeeds[4][i];
            Generator.AdvanceSequence(buildSeed);

            var complex = Generator.DrawSelect(complexPool);
            var objective = Generator.DrawSelect(mainObjectivesPool);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = complex,
                Tier = "E",
                Objective = objective,
            };

            var level = Level.Build(
                new Level("E")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1,
                    Complex = complex,
                    MainDirector = director,
                    BuildSeed = buildSeed
                });

            rundown.AddLevel(level);
        }

        // Add progression requirements if unlocks are needed
        // Only add this for the non-debug build
        #if !DEBUG
        rundown.UseTierUnlockRequirements = true;

        rundown.ReqToReachTierB.MainSectors = Math.Max(1, rundown.TierA.Count - 1);
        rundown.ReqToReachTierC.MainSectors = Math.Max(0, rundown.ReqToReachTierB.MainSectors + rundown.TierB.Count - 1);
        rundown.ReqToReachTierD.MainSectors = Math.Max(0, rundown.ReqToReachTierC.MainSectors + rundown.TierC.Count - 1);
        rundown.ReqToReachTierE.MainSectors = Math.Max(0, rundown.ReqToReachTierD.MainSectors + rundown.TierD.Count - 1);
        #endif

        rundown.VisualsETier = Color.MenuVisuals_MonthlyE;

        return rundown;
    }

    /// <summary>
    /// Entrypoint to build a new rundown
    /// </summary>
    public static void Build(string dailySeed, string weeklySeed, string monthlySeed)
    {
        // TODO: Clean custom directory

        Bins.Setup();
        LayoutDefinitions.Setup();

        var gameSetup = new GameSetup()
        {
            PersistentId = 1,
            Name = "Default"
        };

        // Seasonal Rundown -- Rundown 8 replacement
        #region Seasonal Rundown
        #if false
        {
            // Reads or generates the seed
            Generator.SetSeasonSeed();
            Generator.Reload();

            var name = $"{Generator.Pick(Words.Adjectives)} {Generator.Pick(Words.NounsRundown)}";
            var seasonal = BuildSeasonalRundown(new Rundown
            {
                PersistentId = Rundown.R_Seasonal,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>SEASON {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}"
            });

            Bins.Rundowns.AddBlock(seasonal);
        }
        #endif
        #endregion

        // Monthly Rundown -- Rundown 4 replacement
        #region Monthly Rundown
        {
            ///
            /// The monthly rundown will get a bit more hand crafted work to ensure it loads. Specifically each month
            /// before the new month go through and verify that every level will at least load. Use the BuildSeed to
            /// re-roll any levels that do not load.
            ///
            /// It's not ideal as there needs to be a monthly release to ensure these levels work, but it should always
            /// mean they will load.
            ///

            // Set the monthly seed
            Generator.SetMonthSeed(monthlySeed);
            Generator.Reload();

            var name = Words.RundownNameMonthly();
            var monthly = BuildMonthlyRundown(new Rundown
            {
                PersistentId = Rundown.R_Monthly,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>MONTHLY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",
            });

            Bins.Rundowns.AddBlock(monthly);
        }
        #endregion

        // Weekly Rundown -- Rundown 5 replacement
        #region Weekly Rundown
        {
            // Set the weekly seed
            Generator.SetWeeklySeed(weeklySeed);
            Generator.Reload();

            var name = $"{Generator.Pick(Words.Adjectives)} {Generator.Pick(Words.NounsRundown)}";

            var withUnlocks = false;
            #if DEBUG
            withUnlocks = false;
            #endif

            var weekly = BuildRundown(new Rundown
            {
                PersistentId = Rundown.R_Weekly,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>WEEKLY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",
            }, false, withUnlocks);

            weekly.VisualsETier = Color.MenuVisuals_WeeklyE;

            Bins.Rundowns.AddBlock(weekly);
        }
        #endregion

        // Daily Rundown -- Rundown 7 replacement
        #region Daily Rundown
        {
            // Reads or generates the seed
            Generator.ReadOrSetSeed(dailySeed);
            Generator.Reload();

            var name = $"{Generator.Pick(Words.Adjectives)} {Generator.Pick(Words.NounsRundown)}";
            var daily = BuildRundown(new Rundown
            {
                PersistentId = Rundown.R_Daily,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>DAILY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",

                TierA_Count = 1,
                TierD_Count = 3,
                TierE_Count = 1
            }, true, false);

            daily.VisualsETier = Color.MenuVisuals_DailyE;

            Bins.Rundowns.AddBlock(daily);
        }
        #endregion

        // Only load the rundowns we want
        gameSetup.RundownIdsToLoad = new List<uint>
        {
            Rundown.R_Daily, // Rundown.R7,
            Rundown.R_Monthly, // Rundown.R4,
            Rundown.R_Weekly, // Rundown.R5,

            // // These are no-ops and will be disabled
            // Rundown.R_Daily,
            // Rundown.R_Daily,
            // Rundown.R_Daily,
            // Rundown.R_Daily,
            //
            // Rundown.R_Seasonal, // Rundown.R8,
        };
        Bins.GameSetups.AddBlock(gameSetup);

        // Configure any peer mods
        Peers.Configure();

        Bins.Save();

        EnemyCustomization.Ability.Save();
        EnemyCustomization.EnemyAbility.Save();
        EnemyCustomization.Model.Save();
        EnemyCustomization.Projectile.Save();
        GlobalConfig.Save();
    }
}

