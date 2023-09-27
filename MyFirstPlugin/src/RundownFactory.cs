using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BepInEx;
using MyFirstPlugin.DataBlocks;


namespace MyFirstPlugin
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

            var rundown = Rundown.Build();

            var level = Level.Build(
                new BuildDirector 
                { 
                    Complexity = Complexity.Low, 
                    Credits = 200 
                },
                new Level
                {
                    Tier = "A",
                    Index = 1,
                });

            rundown.AddLevel(level);

            rundown.Save();
        }
    }
}
