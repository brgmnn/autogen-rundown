﻿using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.GeneratorData;
using BepInEx;

namespace AutogenRundown
{
    static public class RundownFactory
    {
        /// <summary>
        /// Checks if the Json Data directory already exists
        /// </summary>
        /// <returns></returns>
        static public bool JsonDataExists()
        {
            if (Generator.Seed == "")
                return false;

            var dir = Path.Combine(Paths.PluginPath, "MyFirstPlugin", "Datablocks", Generator.Seed);

            return Directory.Exists(dir);
        }

        /// <summary>
        /// Entrypoint to build a new rundown
        /// </summary>
        static public void Build()
        {
            Generator.Reload();
            Bins.Setup();

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
            var mainDirector = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = Complex.Mining,
                Complexity = Complexity.Low,
                Tier = "C",
                Objective = WardenObjectiveType.ReactorStartup,
            };
            mainDirector.GenPoints();

            var settings = new LevelSettings("C");
            settings.Modifiers.Add(LevelModifiers.Fog);

            var testLevel = Level.Build(
                new Level
                {
                    Tier = "C",
                    Name = "Reactor Startup",
                    Complex = Complex.Mining,
                    MainDirector = mainDirector,
                    Settings = settings,
                    Index = cMax + 1,
                    IsTest = true
                });
            rundown.AddLevel(testLevel);
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

            #region Test D Levels
            #if true
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
