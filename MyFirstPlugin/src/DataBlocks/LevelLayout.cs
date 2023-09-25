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
    }
}
