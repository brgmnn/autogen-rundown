using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.ZoneData
{
    internal class BloodDoor
    {
        [JsonProperty("HasActiveEnemyWave")]
        public bool Enabled { get; set; } = false;

        public UInt32 EnemyGroupInfrontOfDoor { get; set; } = 0;

        public UInt32 EnemyGroupInArea { get; set; } = 0;

        public int EnemyGroupsInArea { get; set; } = 0;

        static public BloodDoor None = new BloodDoor { Enabled = false };

        static public BloodDoor Easy = new BloodDoor
        {
            Enabled = true,
            EnemyGroupInfrontOfDoor = 76,
            EnemyGroupInArea = 76,
            EnemyGroupsInArea = 1
        };
    }
}
