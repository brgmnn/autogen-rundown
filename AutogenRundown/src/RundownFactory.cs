using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.GeneratorData;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

                //WardenObjectiveType.Survival => prefix + "<color=orange>Distraction Protocol</color>\n",
                _ => ""
            };
        }

        // TODO: Break this out into a separate class for custom geos
        private static void WriteComplexResourceSet()
        {
            const string name = "GameData_ComplexResourceSetDataBlock_bin.json";

            var revision = CellBuildData.GetRevision();

            var from = Path.Combine(Paths.PluginPath, Plugin.Name, name);
            var destDir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}");
            var dest = Path.Combine(destDir, name);

            using var sourceFile = File.OpenText(from);
            using var reader = new JsonTextReader(sourceFile);

            var resourceSet = (JObject)JToken.ReadFrom(reader);

            if (resourceSet["Blocks"] == null)
            {
                Plugin.Logger.LogFatal("No complex resource set data blocks found");
                return;
            }

            var blocks = (JArray)resourceSet["Blocks"]!;

            var serviceBlock = blocks.OfType<JObject>()
                .First(block => (int?)block["persistentID"] == (int)Complex.Service);

            if (serviceBlock?["CustomGeomorphs_Objective_1x1"] == null)
            {
                Plugin.Logger.LogFatal("No Complex.Service resource block found");
                return;
            }

            var serviceObjectiveGeos = (JArray)serviceBlock["CustomGeomorphs_Objective_1x1"]!;

            serviceObjectiveGeos.Insert(0,
                new JObject
                {
                    ["Prefab"] = "Assets/Prefabs/Geomorph/Service/geo_floodways_FA_reactor_01.prefab",
                    ["SubComplex"] = 6,
                    ["Shard"] = "S1"
                });

            // Ensure the directory exists
            Directory.CreateDirectory(destDir);

            // write JSON directly to a file
            using var destFile = File.CreateText(dest);
            using var writer = new JsonTextWriter(destFile);

            resourceSet.WriteTo(writer);

            Plugin.Logger.LogWarning($"We got here ok!!! {from} -> {dest}");
        }

        /// <summary>
        /// Entrypoint to build a new rundown
        /// </summary>
        public static void Build()
        {
            Generator.Reload();
            Bins.Setup();

            WriteComplexResourceSet();

            // Rundown 8 replacement
            var rundown = Rundown.Build(new Rundown { PersistentId = Rundown.R7 });

            var levelNames = new List<string>(Words.NounsLevel);

            var aMax = Generator.Random.Next(1, 2);
            var bMax = Generator.Random.Next(3, 4);
            var cMax = Generator.Random.Next(2, 4);
            var dMax = Generator.Random.Next(2, 4);
            var eMax = Generator.Random.Next(1, 2);
            /*var aMax = 0;
            var bMax = 0;
            var cMax = 0;
            var dMax = 0;
            var eMax = 0;*/


            #region A-Tier Levels
            for (int i = 0; i < aMax; i++)
            {
                var level = Level.Build(
                    new Level
                    {
                        Tier = "A",
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
                    new Level
                    {
                        Tier = "B",
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
                    new Level
                    {
                        Tier = "C",
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
                    Objective = WardenObjectiveType.GatherSmallItems,
                };
                mainDirector.GenPoints();

                var settings = new LevelSettings("C");
                //settings.Modifiers.Add(LevelModifiers.Fog);

                var testLevel = Level.Build(
                    new Level
                    {
                        Tier = "C",
                        Name = "Gather Items",
                        Complex = Complex.Mining,
                        MainDirector = mainDirector,
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
            // D levels
            for (int i = 0; i < dMax; i++)
            {
                var level = Level.Build(
                    new Level
                    {
                        Tier = "D",
                        Name = Generator.Draw(levelNames)!,
                        Index = i + 1
                    });
                rundown.AddLevel(level);
            }

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
                    new()
                    {
                        Tier = "D",
                        Prefix = $"<color=orange>R</color><color=#444444>:</color>D",
                        Description = description.PersistentId,
                        MainDirector = mainDirector,
                        Settings = settings,
                        Index = dMax + 1
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
                    new Level
                    {
                        Tier = "E",
                        Name = Generator.Draw(levelNames)!,
                        Index = i + 1
                    });
                rundown.AddLevel(level);
            }

            #region Clear Path
            {
                var objective = WardenObjectiveType.ClearPath;
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
                        Prefix = $"<color=orange>P</color><color=#444444>:</color>E",
                        Description = description.PersistentId,
                        MainDirector = mainDirector,
                        Settings = settings,
                        Index = eMax + 1
                    });

                rundown.AddLevel(level);
            }
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
