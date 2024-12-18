using AutogenRundown.DataBlocks;
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

    public static Rundown BuildRundown(Rundown newRundown, bool withFixed = true)
    {
        var rundown = Rundown.Build(newRundown);

        var levelNames = Words.NewLevelNamesPack();

        var aMax = Generator.Between(1, 2);
        var bMax = Generator.Between(3, 4);
        var cMax = Generator.Between(2, 4);
        var dMax = Generator.Between(1, 2);
        var eMax = Generator.Between(1, 4);

        #region A-Tier Levels
        for (int i = 0; i < aMax; i++)
        {
            var level = Level.Build(
                new Level("A")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1
                });
            rundown.AddLevel(level);
        }
        #endregion

        #region B-Tier Levels
        for (int i = 0; i < bMax; i++)
        {
            var level = Level.Build(
                new Level("B")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1
                });
            rundown.AddLevel(level);
        }
        #endregion

        #region C-Tier Levels
        for (int i = 0; i < cMax; i++)
        {
            var level = Level.Build(
                new Level("C")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1
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
                    Index = cMax + 1,
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
        for (int i = 1; i < dMax + 1; i++)
        {
            var level = Level.Build(
                new Level("D")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1
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
                    Index = dMax + 2
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
                    Index = dMax + 3
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
        for (int i = 0; i < eMax; i++)
        {
            var level = Level.Build(
                new Level("E")
                {
                    Name = Generator.Draw(levelNames)!,
                    Index = i + 1
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

        return rundown;
    }

    /// <summary>
    /// Entrypoint to build a new rundown
    /// </summary>
    public static void Build(string dailySeed)
    {
        Bins.Setup();
        ComplexResourceSet.Setup();

        var gameSetup = new GameSetup()
        {
            PersistentId = 1,
            Name = "Default"
        };

        // Monthly Rundown -- Rundown 4 replacement
        #region Monthly Rundown
        {
            // Set the monthly seed
            Generator.SetMonthSeed();
            Generator.Reload();

            var name = $"{Generator.Pick(Words.Adjectives)} {Generator.Pick(Words.NounsRundown)}";

            var monthly = BuildRundown(new Rundown
            {
                PersistentId = Rundown.R_Monthly,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>MONTHLY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",
            }, false);

            // Add progression requirements for the monthly
            monthly.ReqToReachTierB.MainSectors = Math.Max(0, monthly.TierA.Count - 1);
            monthly.ReqToReachTierC.MainSectors = Math.Max(0, monthly.TierA.Count + monthly.TierB.Count - 1);
            monthly.ReqToReachTierD.MainSectors =
                Math.Max(0, monthly.TierA.Count + monthly.TierB.Count + monthly.TierC.Count - 1);
            monthly.ReqToReachTierE.MainSectors =
                Math.Max(0, monthly.TierA.Count + monthly.TierB.Count + monthly.TierC.Count + monthly.TierD.Count - 1);

            Bins.Rundowns.AddBlock(monthly);
        }
        #endregion

        // Weekly Rundown -- Rundown 5 replacement
        #region Monthly Rundown
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

            // Add progression requirements for the monthly
            weekly.ReqToReachTierB.MainSectors = Math.Max(0, weekly.TierA.Count - 1);
            weekly.ReqToReachTierC.MainSectors = Math.Max(0, weekly.TierA.Count + weekly.TierB.Count - 1);
            weekly.ReqToReachTierD.MainSectors =
                Math.Max(0, weekly.TierA.Count + weekly.TierB.Count + weekly.TierC.Count - 1);
            weekly.ReqToReachTierE.MainSectors =
                Math.Max(0, weekly.TierA.Count + weekly.TierB.Count + weekly.TierC.Count + weekly.TierD.Count - 1);

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
                // PersistentId = Rundown.R7,
                PersistentId = Rundown.R_Daily,
                Title = $"{name.ToUpper()}",
                StoryTitle = $"<color=green>RND://</color>DAILY {Generator.DisplaySeed}\r\nTITLE: {name.ToUpper()}",
            });

            Bins.Rundowns.AddBlock(daily);
        }
        #endregion

        Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R1, Name = "ALT Rundown 1.0" });
        Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R2, Name = "ALT Rundown 2.0" });
        Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R3, Name = "ALT Rundown 3.0" });
        // Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R4, Name = "ALT Rundown 4.0" });
        // Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R5, Name = "ALT Rundown 5.0" });
        Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R6, Name = "ALT Rundown 6.0" });
        //Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R7, Name = "Rundown 7.0" });

        Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R8, Name = "Rundown 8.0" });
        Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.Tutorial, Name = "Tutorial" });
        Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.Geomorph, Name = "Geomorph" });

        // Can we use LocalProgression EnableNoBoosterUsedProgressionForRundown to disable
        // boosters being used?

        gameSetup.RundownIdsToLoad = new List<uint>
        {
            Rundown.R1,
            Rundown.R_Daily, // Rundown.R7,
            Rundown.R2,
            Rundown.R3,
            Rundown.R_Monthly, // Rundown.R4,
            Rundown.R_Weekly, // Rundown.R5,
            Rundown.R6,
            Rundown.R8,
        };
        Bins.GameSetups.AddBlock(gameSetup);

        Bins.Save();
    }
}

