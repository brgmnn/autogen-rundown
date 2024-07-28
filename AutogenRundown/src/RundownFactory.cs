using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.GeneratorData;

namespace AutogenRundown
{
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
        /// Entrypoint to build a new rundown
        /// </summary>
        public static void Build()
        {
            Generator.Reload();
            Bins.Setup();
            ComplexResourceSet.Setup();

            // Rundown 8 replacement
            var rundown = Rundown.Build(new Rundown { PersistentId = Rundown.R7 });

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
            #if false
            {
                var mainDirector = new BuildDirector
                {
                    Bulkhead = Bulkhead.Main,
                    Complex = Complex.Mining,
                    Complexity = Complexity.Low,
                    Tier = "C",
                    Objective = WardenObjectiveType.Survival,
                };
                mainDirector.GenPoints();

                var extremeDirector = new BuildDirector
                {
                    Bulkhead = Bulkhead.Extreme,
                    Complex = Complex.Mining,
                    Complexity = Complexity.Low,
                    Tier = "C",
                    Objective = WardenObjectiveType.PowerCellDistribution,
                };
                extremeDirector.GenPoints();

                var settings = new LevelSettings("C");
                //settings.Modifiers.Add(LevelModifiers.Fog);

                var testLevel = Level.Build(
                    new Level
                    {
                        Tier = "C",
                        Name = "Generator Cluster",
                        Complex = Complex.Mining,
                        MainDirector = mainDirector,
                        SecondaryDirector = extremeDirector,
                        Settings = settings,
                        Index = cMax + 1,
                        IsTest = true
                    });
                rundown.AddLevel(testLevel);
            }
            #endif
            #endregion
            #endregion

            #region D-Tier Levels
            #region Survival Fixed
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
            {
                ///
                /// Timed Terminal Sequence mission. This is a hard coded mission of sequence stuff
                ///
                var objective = WardenObjectiveType.TimedTerminalSequence;
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
                        Prefix = $"<color=orange>W</color><color=#444444>:</color>D",
                        Description = description.PersistentId,
                        MainDirector = mainDirector,
                        Settings = settings,
                        Index = dMax + 2
                    });

                rundown.AddLevel(level);
            }
            #endregion

            #region Reactor startup
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
            #if false
            {
                var objective = WardenObjectiveType.Survival;
                var mainDirector = new BuildDirector()
                {
                    Bulkhead = Bulkhead.Main,
                    Tier = "E",
                    Objective = objective
                };
                mainDirector.GenPoints();

                var settings = new LevelSettings("E");
                var description = new DataBlocks.Text(DescriptionHeader(objective) +
                                                      DataBlocks.WardenObjective.GenLevelDescription(objective));
                var level = Level.Build(
                    new()
                    {
                        Tier = "E",
                        Prefix = $"<color=orange>X</color><color=#444444>:</color>E",
                        Description = description.PersistentId,
                        MainDirector = mainDirector,
                        Settings = settings,
                        Index = eMax + 1
                    });

                rundown.AddLevel(level);
            }
            #endif
            #endregion

            #region Test E Levels
            #if false
            var mainDirectorE = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Mining,
                Complexity = Complexity.Low,
                Tier = "E",
                Objective = WardenObjectiveType.TerminalUplink,
            };
            mainDirectorE.GenPoints();

            var settingsE = new LevelSettings("E");
            settingsE.Modifiers.Add(LevelModifiers.ManyChargers);
            settingsE.Modifiers.Add(LevelModifiers.ManyShadows);

            var testLevelE = Level.Build(
                new Level
                {
                    Tier = "E",
                    Name = "Terminal Command",
                    Complex = Complex.Tech,
                    MainDirector = mainDirectorE,
                    Settings = settingsE,
                    Index = eMax + 1,
                    IsTest = true
                });
            rundown.AddLevel(testLevelE);
            #endif
            #endregion
            #endregion

            Bins.Rundowns.AddBlock(rundown);

            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R1, Name = "ALT Rundown 1.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R2, Name = "ALT Rundown 2.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R3, Name = "ALT Rundown 3.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R4, Name = "ALT Rundown 4.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R5, Name = "ALT Rundown 5.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R6, Name = "ALT Rundown 6.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R7, Name = "Rundown 7.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.R8, Name = "Rundown 8.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.Tutorial, Name = "Tutorial" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = Rundown.Geomorph, Name = "Geomorph" });

            Bins.Save();
        }
    }
}
