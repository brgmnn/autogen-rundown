using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutogenRundown.DataBlocks.Enemies;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks
{
    /// <summary>
    /// Hibernate is asleep, Hunter is for blood doors. There are others but they are broken
    /// </summary>
    enum EnemyGroupType : uint
    {
        /// <summary>
        /// Most spawned enemies should be hibernate
        /// </summary>
        Hibernate = 0,

        /// <summary>
        /// Usually for bosses that should be sneaked past.
        /// </summary>
        PureSneak = 1,

        /// <summary>
        /// Seems to be mostly Chargers
        /// </summary>
        Detect = 2,

        /// <summary>
        /// Scouts only
        /// </summary>
        Scout = 3,
        Awake = 5,

        /// <summary>
        /// Use only for blood doors
        /// </summary>
        Hunter = 6,
    }

    enum EnemyRoleDifficulty : uint
    {
        /// <summary>
        /// - Strikers (Hibernate)
        /// - Shooter (Hibernate)
        /// - Baby
        /// </summary>
        Easy = 0,

        /// <summary>
        /// - Strikers Hibernate
        /// - Shooter Hibernate
        /// </summary>
        Medium = 1,

        /// <summary>
        /// - Strikers Hibernate
        /// - Shooter Hibernate
        /// - Mother
        /// </summary>
        Hard = 2,

        /// <summary>
        /// - Mother
        /// - Big Mother
        /// </summary>
        MiniBoss = 3,

        /// <summary>
        /// - Big Mother
        /// </summary>
        Boss = 4,

        /// <summary>
        /// - Tank
        /// </summary>
        Tank = 5,

        /// <summary>
        /// - Shadow
        /// - Pouncer
        /// </summary>
        Biss = 6,

        /// <summary>
        /// - Baby
        /// - Pouncer
        /// </summary>
        Buss = 7
    }

    enum EnemyZoneDistribution
    {
        None = 0,
        ForceOne = 1,
        RelValue = 2
    }

    /// <summary>
    /// Not needed unless we generate our own pop blocks
    /// </summary>
    enum EnemyRoleDistribution
    {
        None = 0,
        ForceOne = 1,
        Rel25 = 2,
        Rel50 = 3,
        Rel75 = 4,
        Rel100 = 5,
        Rel15 = 6,
        Rel10 = 7,
        Rel05 = 8
    }

    /// <summary>
    /// https://gtfo-modding.gitbook.io/wiki/reference/nested-types/enemyspawningdata
    /// 
    /// With RelValue scoring system:
    ///     1. Randomly select EnemyGroup matching GroupType and Difficulty.
    ///     2. Deduct EnemyGroup.MaxScore from the current Distribution score.
    ///         * Note that this must be for Distribution * POPULATION_PER_ZONE.
    ///         * PopScore = Distribution * POPULATION_PER_ZONE.
    ///     3. For each role, match an EnemyPopulation using EnemyGroup.Role and 
    ///        EnemyGroup.Difficulty. Selection is random (using weights).
    ///     4. Spawn selected enemies from EnemyPopulation, count = MaxScore / Cost
    ///     5. Repeat: until PopScore <= 3
    /// 
    /// Score base spawning system:
    ///     number of enemies = EnemyGroup.MaxScore * Role.Distribution / EnemyPopulation.Cost
    ///
    /// Enemy Points:
    ///     * Striker: 1
    ///     * Shooter: 1
    ///     
    ///     * Giant Striker: 4
    ///     * Giant Shooter: 4
    ///     * Hybrid: 4 (3pts in Difficulty 6)
    ///     
    ///     * Shadow: 1
    ///     * Shadow Giant: 4
    ///
    ///     * Scout: 5
    ///     * Charger Scout: 5
    ///     * Shadow Scout: 5
    ///
    ///     * Baby: 1
    ///     
    ///     * Tank: 10
    ///     * Mother: 10
    ///     * P-Mother: 10
    ///
    ///     * Flyer: 1
    ///     * Boss Flyer: 10
    /// </summary>
    internal record class EnemySpawningData
    {
        /// <summary>
        /// Find this in the expedition balance block, EnemyPopulationPerZone.
        /// </summary>
        public static int POPULATION_PER_ZONE = 25;

        /// <summary>
        /// What state the enemies are in
        /// </summary>
        public EnemyGroupType GroupType { get; set; } = EnemyGroupType.Hibernate;

        /// <summary>
        /// What difficulty of enemies to match in the enemy population data block
        /// </summary>
        public EnemyRoleDifficulty Difficulty { get; set; } = EnemyRoleDifficulty.Easy;


        public EnemyZoneDistribution Distribution { get; set; } = EnemyZoneDistribution.RelValue;

        /// <summary>
        /// Works with our points system
        /// </summary>
        [JsonIgnore]
        public int Points { get; set; } = 25;

        /// <summary>
        /// 
        /// </summary>
        public double DistributionValue
        {
            get => (double)Points / POPULATION_PER_ZONE;
        }

        /// <summary>
        /// Regular scout
        /// </summary>
        public static EnemySpawningData Scout = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Scout,
            Difficulty = EnemyRoleDifficulty.Easy
        };

        public static EnemySpawningData ScoutCharger = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Scout,
            Difficulty = EnemyRoleDifficulty.MiniBoss
        };

        public static EnemySpawningData ScoutShadow = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Scout,
            Difficulty = EnemyRoleDifficulty.Boss
        };
    }
}
