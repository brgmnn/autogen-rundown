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
        #if true
        if (withFixed)
        {
            var mainDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Tech,
                Complexity = Complexity.Low,
                Tier = "B",
                Objective = WardenObjectiveType.HsuFindSample,
            };
            mainDirector.GenPoints();

            var extremeDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Extreme,
                Complex = Complex.Tech,
                Complexity = Complexity.Low,
                Tier = "B",
                Objective = WardenObjectiveType.HsuFindSample,
            };
            extremeDirector.GenPoints();

            var settings = new LevelSettings("B");
            settings.Modifiers.Add(LevelModifiers.Shadows);

            var testLevel = Level.Build(
                new Level("B")
                {
                    Tier = "B",
                    Name = "Debug Test",
                    Complex = Complex.Tech,
                    MainDirector = mainDirector,
                    // SecondaryDirector = extremeDirector,
                    Settings = settings,
                    Index = rundown.TierB_Count + 1,
                    IsTest = true
                });
            rundown.AddLevel(testLevel);
        }
        #endif
        #endregion

        #region Geomorph Debugging test level
        #if false
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
        if (true && withFixed)
        {
            var mainDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Mining,
                Complexity = Complexity.Low,
                Tier = "C",
                Objective = WardenObjectiveType.TerminalUplink,
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
                    Name = "Terminal Uplink",
                    Complex = Complex.Mining,
                    MainDirector = mainDirector,
                    // SecondaryDirector = extremeDirector,
                    Settings = settings,
                    Index = rundown.TierC_Count + 1,
                    IsTest = true
                });
            rundown.AddLevel(testLevel);
        }
        #endregion
        #endregion

        #region D-Tier Levels
        #region Survival Fixed
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
                    Settings = settings,
                    Index = 1
                });

            rundown.AddLevel(level);
        }
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
        #endregion

        #region Reactor startup
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
        #endregion

        #region Test D Levels
        #if false
        var mainDirectorD = new BuildDirector
        {
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
            Complexity = Complexity.Low,
            Tier = "D",
            Objective = WardenObjectiveType.ReactorStartup,
        };
        mainDirectorD.GenPoints();

        var settingsD = new LevelSettings("D");
        // settingsD.Modifiers.Add(LevelModifiers.ManyShadows);

        var testLevelD = Level.Build(
            new Level
            {
                Tier = "D",
                Name = "Reactor Startup",
                Complex = Complex.Mining,
                MainDirector = mainDirectorD,
                Settings = settingsD,
                Index = dMax + 1,
                IsTest = true
            });
        rundown.AddLevel(testLevelD);
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

        #region Survival
        // if (false && withFixed)
        // {
        //     var objective = WardenObjectiveType.Survival;
        //     var mainDirector = new BuildDirector()
        //     {
        //         Bulkhead = Bulkhead.Main,
        //         Tier = "E",
        //         Objective = objective
        //     };
        //     mainDirector.GenPoints();
        //
        //     var settings = new LevelSettings("E");
        //     var description = new DataBlocks.Text(DescriptionHeader(objective) +
        //                                           DataBlocks.WardenObjective.GenLevelDescription(objective));
        //     var level = Level.Build(
        //         new()
        //         {
        //             Tier = "E",
        //             Prefix = $"<color=orange>X</color><color=#444444>:</color>E",
        //             Description = description.PersistentId,
        //             MainDirector = mainDirector,
        //             Settings = settings,
        //             Index = eMax + 1
        //         });
        //
        //     rundown.AddLevel(level);
        // }
        #endregion

        #region Test E Levels
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
        #endregion
        #endregion

        // Add progression requirements if unlocks are needed
        if (withUnlocks)
        {
            rundown.UseTierUnlockRequirements = true;

            rundown.ReqToReachTierB.MainSectors = Math.Max(0, rundown.TierA.Count - 1);
            rundown.ReqToReachTierC.MainSectors = Math.Max(0, rundown.ReqToReachTierB.MainSectors + rundown.TierB.Count - 1);
            rundown.ReqToReachTierD.MainSectors = Math.Max(0, rundown.ReqToReachTierC.MainSectors + rundown.TierC.Count - 1);
            rundown.ReqToReachTierE.MainSectors = Math.Max(0, rundown.ReqToReachTierD.MainSectors + rundown.TierD.Count - 1);
        }

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
            var buildPools = new Dictionary<string, List<int>>()
            {
                {
                    "2025_02", new()
                    {
                        1, 2,       // Tier A - y,y
                        1, 1, 1,    // Tier B - y,y,y
                        1, 1, 1, 2, // Tier C - y,y,y,y
                        1, 1, 1, 1, // Tier D - y,y,y,y
                        1, 1, 1     // Tier E - y,y,y
                    }
                }
            };

            buildPools.TryGetValue(Generator.Seed, out var buildSeeds);

            var monthly = BuildRundown(new Rundown
            {
                PersistentId = Rundown.R_Monthly,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>MONTHLY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",

                TierA_Count = Generator.Seed == "2025_02" ? 2 : 1,
                TierB_Count = 3,
                TierC_Count = 4,
                TierD_Count = 4,
                TierE_Count = Generator.Seed == "2025_02" ? 3 : 2,

                ///
                /// Use the seed pool to re-roll levels. Start by setting these at 1 and incrementing each time
                /// there's a level lockup
                ///
                BuildSeedPool = buildSeeds ?? new List<int>()
            }, false);

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

            var weekly = BuildRundown(new Rundown
            {
                PersistentId = Rundown.R_Weekly,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>WEEKLY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",
            }, false);

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

                TierD_Count = 1,
                TierE_Count = Generator.Between(1, 4),
            }, true, false);

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

