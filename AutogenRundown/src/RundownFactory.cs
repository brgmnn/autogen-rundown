using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.GeneratorData;

namespace AutogenRundown;

public static class RundownFactory
{
    static string DescriptionHeader(WardenObjectiveType objectiveType)
    {
        var prefix = "<color=#444444>Objective Dispatch:</color> ";

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
            WardenObjectiveType.Survival => prefix + "<color=orange>Diversion Protocol</color>\n",
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
        #if true
        if (withFixed)
        {
            var settings = new LevelSettings("A");

            var testLevel = Level.Debug_BuildGeoTest(
                "Assets/Bundles/RLC_Tech/geo_64x64_tech_datacenter_I_RLC_01.prefab",
                new Level("A")
                {
                    Tier = "A",
                    Name = "Debug Test",
                    Complex = Complex.Tech,
                    Settings = settings,
                    Index = rundown.TierA_Count + 1,
                    IsTest = true
                }, 3);

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
            var mainDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Mining,
                Complexity = Complexity.Low,
                Tier = "C",
                Objective = WardenObjectiveType.ClearPath,
            };
            mainDirector.GenPoints();

            // var extremeDirector = new BuildDirector
            // {
            //     Bulkhead = Bulkhead.Extreme,
            //     Complex = Complex.Mining,
            //     Complexity = Complexity.Low,
            //     Tier = "C",
            //     Objective = WardenObjectiveType.PowerCellDistribution,
            // };
            // extremeDirector.GenPoints();

            var settings = new LevelSettings("C");
            //settings.Modifiers.Add(LevelModifiers.Fog);

            var testLevel = Level.Build(
                new Level("C")
                {
                    Tier = "C",
                    Name = "Clear Path",
                    Complex = Complex.Mining,
                    MainDirector = mainDirector,
                    // SecondaryDirector = extremeDirector,
                    Settings = settings,
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
        #if DEBUG
        #if true
        if (withFixed)
        {
            var objective = WardenObjectiveType.Survival;
            var mainDirector = new BuildDirector()
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Tech,
                Tier = "D",
                Objective = objective
            };
            mainDirector.GenPoints();

            var extremeDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Extreme,
                Complex = Complex.Tech,
                Complexity = Complexity.Low,
                Tier = "C",
                Objective = WardenObjectiveType.SpecialTerminalCommand,
            };
            extremeDirector.GenPoints();

            var overloadDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Overload,
                Complex = Complex.Tech,
                Complexity = Complexity.Low,
                Tier = "C",
                Objective = WardenObjectiveType.SpecialTerminalCommand,
            };
            overloadDirector.GenPoints();

            var settings = new LevelSettings("D");
            settings.Modifiers.Add(LevelModifiers.Chargers);
            var description = new DataBlocks.Text(DescriptionHeader(objective) +
                                                  DataBlocks.WardenObjective.GenLevelDescription(objective));
            var level = Level.Build(
                new("D")
                {
                    Prefix = $"<color=orange>X</color><color=#444444>:</color>D",
                    Description = description.PersistentId,
                    Complex = Complex.Tech,
                    MainDirector = mainDirector,
                    SecondaryDirector = extremeDirector,
                    OverloadDirector = overloadDirector,
                    Settings = settings,
                    Index = 1
                });

            rundown.AddLevel(level);
        }
        #endif
        #endif
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
        // if (false && withFixed)
        // {
        //     var mainDirectorE = new BuildDirector
        //     {
        //         Bulkhead = Bulkhead.Main,
        //         Complex = Complex.Mining,
        //         Complexity = Complexity.Low,
        //         Tier = "E",
        //         Objective = WardenObjectiveType.TerminalUplink,
        //     };
        //     mainDirectorE.GenPoints();
        //
        //     var settingsE = new LevelSettings("E");
        //     settingsE.Modifiers.Add(LevelModifiers.ManyChargers);
        //     settingsE.Modifiers.Add(LevelModifiers.ManyShadows);
        //
        //     var testLevelE = Level.Build(
        //         new Level
        //         {
        //             Tier = "E",
        //             Name = "Terminal Command",
        //             Complex = Complex.Tech,
        //             MainDirector = mainDirectorE,
        //             Settings = settingsE,
        //             Index = eMax + 1,
        //             IsTest = true
        //         });
        //     rundown.AddLevel(testLevelE);
        // }
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
    /// Entrypoint to build a new rundown
    /// </summary>
    public static void Build(string dailySeed)
    {
        Bins.Setup();
        ComplexResourceSet.Setup();
        LayoutDefinitions.Setup();

        var gameSetup = new GameSetup()
        {
            PersistentId = 1,
            Name = "Default"
        };

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
            Generator.SetMonthSeed();
            Generator.Reload();

            var name = Words.RundownNameMonthly();

            ///
            /// Use the seed pool to re-roll levels. Start by setting these at 1 and incrementing each time
            /// there's a level lockup
            /// TODO: Clear Path objectives seem to fail to place the exit zone often
            /// "2025_01" - January  2025:
            /// "2025_02" - February 2025:
            ///
            var buildPools = new Dictionary<string, List<int>>
            {
                {
                    "2025_02", new()
                    {
                        1, 1,       // Tier A - y,y
                        1, 1, 1,    // Tier B - y,y,y
                        1, 1, 1, 1, // Tier C - y,y,y,y
                        1, 1, 1, 1, // Tier D - y,y,y,y
                        1, 1, 1     // Tier E - y,y,y
                    }
                },
                {
                    "2025_03", new()
                    {
                        1,          // Tier A - y
                        1, 1, 1,    // Tier B - y,y,y
                        1, 1, 1, 1, // Tier C - y,y,y,y
                        1, 1, 2,    // Tier D - y,y,y
                        1, 1, 3     // Tier E - y,y,y
                    }
                }
            };

            buildPools.TryGetValue(Generator.Seed, out var buildSeeds);

            var withUnlocks = true;
            #if DEBUG
            withUnlocks = false;
            #endif

            var monthly = BuildRundown(new Rundown
            {
                PersistentId = Rundown.R_Monthly,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>MONTHLY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",

                TierA_Count = Generator.Seed == "2025_03" ? 1 : 2,
                TierB_Count = 3,
                TierC_Count = 4,
                TierD_Count = Generator.Seed == "2025_03" ? 3 : 4,
                TierE_Count = 3,

                ///
                /// Use the seed pool to re-roll levels. Start by setting these at 1 and incrementing each time
                /// there's a level lockup
                ///
                BuildSeedPool = buildSeeds ?? new List<int>()
            }, false, withUnlocks);

            monthly.VisualsETier = Color.MenuVisuals_MonthlyE;

            Bins.Rundowns.AddBlock(monthly);
        }
        #endregion

        // Weekly Rundown -- Rundown 5 replacement
        #region Weekly Rundown
        {
            // Set the monthly seed
            Generator.SetWeeklySeed();
            Generator.Reload();

            var name = $"{Generator.Pick(Words.Adjectives)} {Generator.Pick(Words.NounsRundown)}";

            var withUnlocks = true;
            #if DEBUG
            withUnlocks = false;
            #endif

            var weekly = BuildRundown(new Rundown
            {
                PersistentId = Rundown.R_Weekly,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>WEEKLY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",
            }, false, withUnlocks);

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
                TierD_Count = 2,
                TierE_Count = Generator.Between(0, 2),
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
        };
        Bins.GameSetups.AddBlock(gameSetup);

        Bins.Save();

        // Write the rundown local progression config
        LocalProgression.WriteConfig();
    }
}

