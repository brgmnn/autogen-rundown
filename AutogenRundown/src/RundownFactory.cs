using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BepInEx;
using AutogenRundown.DataBlocks;


namespace AutogenRundown
{
    static internal class RundownFactory
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

            // Rundown 7 replacement
            var rundown = Rundown.Build(new Rundown { PersistentId = 31 });

            // A levels
            var levelA = Level.Build(
                new BuildDirector { Complexity = Complexity.Low, Credits = 200 },
                new Level { Tier = "A", Index = 1 });

            rundown.AddLevel(levelA);

            // B levels
            var levelB = Level.Build(
                new BuildDirector { Complexity = Complexity.Low, Credits = 400 },
                new Level { Tier = "B", Index = 1 });

            rundown.AddLevel(levelB);

            // C levels
            var levelC = Level.Build(
                new BuildDirector { Complexity = Complexity.Medium, Credits = 600 },
                new Level { Tier = "C", Index = 1 });

            rundown.AddLevel(levelC);

            Bins.Rundowns.AddBlock(rundown);

            Bins.Rundowns.AddBlock(new Rundown { PersistentId = 32, Name = "ALT Rundown 1.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = 33, Name = "ALT Rundown 2.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = 34, Name = "ALT Rundown 3.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = 37, Name = "ALT Rundown 4.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = 38, Name = "ALT Rundown 5.0" });
            Bins.Rundowns.AddBlock(new Rundown { PersistentId = 41, Name = "ALT Rundown 9.0" });

            Bins.Save();
        }
    }
}
