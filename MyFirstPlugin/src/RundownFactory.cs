using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MyFirstPlugin.DataBlocks;


namespace MyFirstPlugin
{
    static internal class RundownFactory
    {
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
