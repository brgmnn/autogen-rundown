using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin.DataBlocks
{
    internal class LevelLayout : DataBlock
    {
        public int ZoneAliasStart { get; set; }

        public List<Zone> Zones { get; set; } = new List<Zone>();

        public LevelLayout()
        {
            Type = "LevelLayoutDataBlock";
        }

        public static LevelLayout Build(Level level)
        {
            var layout = new LevelLayout
            {
                Name = $"{level.Tier}{level.Index} {level.Name}",
                ZoneAliasStart = Generator.Random.Next(5, 900)
            };

            int numZones = 1;

            for (int i = 0; i < numZones; i++)
            {
                var zone = new Zone
                {
                    LocalIndex = i,
                    SubComplex = SubComplex.All,
                    Coverage = CoverageMinMax.Small
                };

                layout.Zones.Add(zone);
            }

            return layout;
        }
    }
}
