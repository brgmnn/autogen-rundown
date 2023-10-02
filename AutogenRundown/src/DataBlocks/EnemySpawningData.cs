using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutogenRundown.DataBlocks
{
    /// <summary>
    /// Hibernate is asleep, Hunter is for blood doors. There are others but they are broken
    /// </summary>
    enum EnemyGroupType
    {
        Hibernate = 0,
        Hunter = 6,
    }

    enum EnemyRoleDifficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2,
        MiniBoss = 3,
        Boss = 4,
        MegaBoss = 5,
        Biss = 6,
        Buss = 7
    }

    enum EnemyZoneDistribution
    {
        None = 0,
        ForceOne = 1,
        RelValue = 2
    }

    /// <summary>
    /// https://gtfo-modding.gitbook.io/wiki/reference/nested-types/enemyspawningdata
    /// </summary>
    internal class EnemySpawningData
    {
        /// <summary>
        /// What state the enemies are in
        /// </summary>
        public EnemyGroupType GroupType { get; set; } = EnemyGroupType.Hibernate;

        /// <summary>
        /// What difficulty of enemies to match in the enemy population data block
        /// </summary>
        public EnemyRoleDifficulty Difficulty { get; set; } = EnemyRoleDifficulty.Easy;


        public EnemyZoneDistribution Distribution { get; set; } = EnemyZoneDistribution.RelValue;

        public double DistributionValue { get; set; } = 1.0;
    }
}
