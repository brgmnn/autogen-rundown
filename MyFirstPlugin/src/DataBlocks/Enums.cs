using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin.DataBlocks
{
    /// <summary>
    /// Which group of tiles this data will generate for
    /// </summary>
    enum Complex
    {
        Mining = 1,
        Tech = 3,
        Service = 27
    }

    /// <summary>
    /// Complex types
    /// </summary>
    enum SubComplex
    {
        // ComplexResourceData.Mining
        DigSite = 0,
        Refinery = 1,
        Storage = 2,
        // MiningReactor = 7,

        // ComplexResourceData.Tech
        DataCenter = 3,
        Lab = 4,

        // ComplexResourceData
        Floodways = 6,
        Gardens = 11,

        // Choose anything valid
        All = 5,
    }
}
