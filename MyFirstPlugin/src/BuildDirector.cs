using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin
{
    enum Complexity
    {
        Low,
        Medium, 
        High
    }

    enum MissionSize
    { 
        Low, 
        Medium, 
        High
    }

    internal class BuildDirector
    {
        public int Credits { get; set; } = 0;

        public Complexity Complexity { get; set; } = Complexity.Medium;
    }
}
