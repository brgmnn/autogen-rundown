using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.Utils;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks
{
    public enum EnemyRoleDifficulty : uint
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

    public enum EnemyZoneDistribution
    {
        None = 0,
        ForceOne = 1,
        RelValue = 2
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
    ///     * Nightmare Striker: 2
    ///     * Nightmare Shooter: 2
    ///
    ///     * Scout: 5
    ///     * Charger Scout: 5
    ///     * Shadow Scout: 5
    ///
    ///     * Baby: 1
    ///
    ///     * Tank: 10
    ///     * TankPotato: 8
    ///     * Mother: 10
    ///     * P-Mother: 10
    ///
    ///     * Flyer: 1
    ///     * Boss Flyer: 10
    /// </summary>
    public record EnemySpawningData
    {
        /// <summary>
        /// Find this in the expedition balance block, EnemyPopulationPerZone.
        ///
        /// The actual base value here doesn't really matter, as we scale the RelValue to equal
        /// the number of points we want in that zone. All that matters is that this value equals
        /// the value in ExpeditionBalanceDataBlock.
        ///
        /// TODO: consider maintaining ExpeditionBalanceDataBlock.
        /// </summary>
        private const int POPULATION_PER_ZONE = 25;

        /// <summary>
        /// Works with our points system
        /// </summary>
        [JsonIgnore]
        public int Points { get; set; } = 25;

        /// <summary>
        /// Tags that we can annotate onto enemy spawning data.
        /// </summary>
        [JsonIgnore] public Tags Tags { get; set; } = new();

        #region Datablock settings
        /// <summary>
        /// What state the enemies are in
        /// </summary>
        public EnemyGroupType GroupType { get; set; } = EnemyGroupType.Hibernate;

        /// <summary>
        /// What difficulty of enemies to match in the enemy population data block
        /// </summary>
        public uint Difficulty { get; set; } = (uint)EnemyRoleDifficulty.Easy;

        /// <summary>
        /// How should the enemies be distributed. This is only ever between RelValue and ForceOne
        /// where force one spawns a single unit and rel value will use the calculated rel value
        /// which is ultimately based on points.
        /// </summary>
        public EnemyZoneDistribution Distribution { get; set; } = EnemyZoneDistribution.RelValue;

        /// <summary>
        ///
        /// </summary>
        public double DistributionValue
        {
            get => (double)Points / POPULATION_PER_ZONE;
        }
        #endregion

        #region Enemy Tiers
        public static EnemySpawningData TierA = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)AutogenDifficulty.TierA
        };

        public static EnemySpawningData TierB = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)AutogenDifficulty.TierB
        };

        public static EnemySpawningData TierC = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)AutogenDifficulty.TierC
        };

        public static EnemySpawningData TierD = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)AutogenDifficulty.TierD
        };

        public static EnemySpawningData TierE = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)AutogenDifficulty.TierE
        };
        #endregion

        #region Sleepers
        public static EnemySpawningData Striker = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)Enemy.Striker
        };

        public static EnemySpawningData Shooter = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)Enemy.Shooter
        };

        public static EnemySpawningData HybridInfected = new()
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = Enemy_New.HybridInfected.PersistentId
        };
        #endregion

        #region Scouts
        /// <summary>
        /// Regular scout
        /// </summary>
        public static EnemySpawningData Scout = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Scout,
            Difficulty = (uint)EnemyRoleDifficulty.Easy,
            Tags = new Tags("scout")
        };

        public static EnemySpawningData ScoutCharger = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Scout,
            Difficulty = (uint)EnemyRoleDifficulty.MiniBoss,
            Tags = new Tags("scout")
        };

        public static EnemySpawningData ScoutShadow = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Scout,
            Difficulty = (uint)EnemyRoleDifficulty.Boss,
            Tags = new Tags("scout")
        };

        public static EnemySpawningData ScoutNightmare = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Scout,
            Difficulty = (uint)EnemyRoleDifficulty.Biss,
            Tags = new Tags("scout")
        };

        public static EnemySpawningData ScoutZoomer = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Scout,
            Difficulty = (uint)EnemyRoleDifficulty.Tank,
            Tags = new Tags("scout")
        };
        #endregion

        #region Bosses
        public static EnemySpawningData Mother = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)Enemy.Mother,
            Tags = new Tags("boss")
        };

        public static EnemySpawningData PMother = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)Enemy.PMother,
            Tags = new Tags("boss")
        };

        // Technically not a boss but spawns with the mothers usually
        public static EnemySpawningData Baby = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)Enemy.Baby
        };

        public static EnemySpawningData Tank = new EnemySpawningData
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)Enemy.Tank,
            Tags = new Tags("boss")
        };

        public static readonly EnemySpawningData TankPotato = new()
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)Enemy.TankPotato,
            Points = 10,
            Tags = new Tags("boss")
        };

        public static readonly EnemySpawningData Pouncer = new()
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = (uint)Enemy.Pouncer,
            Points = 4,
            Tags = new Tags("boss")
        };

        public static readonly EnemySpawningData PouncerShadow = new()
        {
            GroupType = EnemyGroupType.Hibernate,
            Difficulty = Enemy_New.PouncerShadow.PersistentId,
            Points = 4,
            Tags = new Tags("boss")
        };
        #endregion

        #region Bosses (Spawn Aligned)
        /**
         * Mothers
         */
        public static readonly EnemySpawningData Mother_AlignedSpawn = new()
        {
            GroupType = EnemyGroupType.PureSneak,
            Difficulty = (uint)AutogenDifficulty.BossAlignedSpawn | (uint)Enemy.Mother,
            Points = 10,
            Tags = new Tags("boss")
        };

        public static readonly EnemySpawningData PMother_AlignedSpawn = new()
        {
            GroupType = EnemyGroupType.PureSneak,
            Difficulty = (uint)AutogenDifficulty.BossAlignedSpawn | (uint)Enemy.PMother,
            Points = 10,
            Tags = new Tags("boss")
        };

        // TODO: MegaMother appears to also spawn non-babies. It may be spawning enemies from the
        // levels normal enemy spawning options! Saw it spawn chargers
        public static readonly EnemySpawningData MegaMother_AlignedSpawn = new()
        {
            GroupType = EnemyGroupType.PureSneak,
            Difficulty = (uint)AutogenDifficulty.BossAlignedSpawn | (uint)Enemy.MegaMother,
            Points = 40,
            Tags = new Tags("boss")
        };

        /**
         * Tanks
         */
        public static readonly EnemySpawningData Tank_AlignedSpawn = new()
        {
            GroupType = EnemyGroupType.PureSneak,
            Difficulty = (uint)AutogenDifficulty.BossAlignedSpawn | (uint)Enemy.Tank,
            Points = 10,
            Tags = new Tags("boss")
        };

        public static readonly EnemySpawningData TankPotato_AlignedSpawn = new()
        {
            GroupType = EnemyGroupType.PureSneak,
            Difficulty = (uint)AutogenDifficulty.BossAlignedSpawn | (uint)Enemy.TankPotato,
            Points = 10,
            Tags = new Tags("boss")
        };

        /**
         * Pouncers
         */
        public static readonly EnemySpawningData Pouncer_AlignedSpawn = new()
        {
            GroupType = EnemyGroupType.PureSneak,
            Difficulty = (uint)AutogenDifficulty.BossAlignedSpawn | (uint)Enemy.Pouncer,
            Points = 4,
            Tags = new Tags("boss")
        };
        #endregion
    }
}
