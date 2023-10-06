using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BepInEx;
using AutogenRundown.DataBlocks;
using AutogenRundown.GeneratorData;

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

            var levelNames = new List<string>(Words.NounsLevel);

            var bMax = Generator.Random.Next(2, 4);
            var cMax = Generator.Random.Next(2, 4);
            var dMax = Generator.Random.Next(1, 3);
            var eMax = Generator.Random.Next(0, 1);

            //var bMax = 4;
            //var cMax = 3;
            //var dMax = 3;
            //var eMax = 1;


            // A levels, always generate atleast 1
            var levelA = Level.Build(
                new Level 
                { 
                    Tier = "A", 
                    Name = Generator.Draw(levelNames),
                    Index = 1 
                });
            rundown.AddLevel(levelA);

            /*
            // B levels
            for (int i = 0; i < bMax; i++)
            {
                var level = Level.Build(
                    new Level 
                    { 
                        Tier = "B",
                        Name = Generator.Draw(levelNames),
                        Index = i + 1 
                    });
                rundown.AddLevel(level);
            }


            // C levels
            for (int i = 0; i < cMax; i++)
            {
                var level = Level.Build(
                    new Level 
                    { 
                        Tier = "C",
                        Name = Generator.Draw(levelNames),
                        Index = i + 1 
                    });
                rundown.AddLevel(level);
            }


            // D levels
            for (int i = 0; i < dMax; i++)
            {
                var level = Level.Build(
                    new Level 
                    { 
                        Tier = "D",
                        Name = Generator.Draw(levelNames),
                        Index = i + 1 
                    });
                rundown.AddLevel(level);
            }


            // E levels
            for (int i = 0; i < eMax; i++)
            {
                var level = Level.Build(
                    new Level 
                    { 
                        Tier = "E",
                        Name = Generator.Draw(levelNames),
                        Index = i + 1 
                    });
                rundown.AddLevel(level);
            }
            */

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
