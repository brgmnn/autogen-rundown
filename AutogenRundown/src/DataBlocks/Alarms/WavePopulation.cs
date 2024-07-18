using BepInEx;
using AutogenRundown.DataBlocks.Enemies;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Alarms
{
    public record WavePopulation : DataBlock
    {
        #region Properties
        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// Cost: 0.75
        /// </summary>
        public Enemy WaveRoleWeakling { get; set; }

        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// Cost: 1.0
        /// </summary>
        public Enemy WaveRoleStandard { get; set; }

        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// Cost: 1.0
        /// </summary>
        public Enemy WaveRoleSpecial { get; set; }

        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// Cost: 2.0
        /// </summary>
        public Enemy WaveRoleMiniBoss { get; set; }

        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        ///
        /// Allegedly doesn't work
        /// Cost: 2.0
        /// </summary>
        public Enemy WaveRoleBoss { get; set; }
        #endregion

        public WavePopulation(PidOffsets offsets = PidOffsets.WavePopulation)
            : base(Generator.GetPersistentId(offsets))
        { }

        public WavePopulation Persist()
        {
            Bins.WavePopulations.AddBlock(this);
            return this;
        }

        public static new void Setup()
        {
            var dir = Path.Combine(Paths.PluginPath, Plugin.Name);
            var path = Path.Combine(dir, $"GameData_SurvivalWavePopulationDataBlock_bin.json");
            var data = JObject.Parse(File.ReadAllText(path));

            if (data?["Blocks"] == null)
                throw new Exception("Failed to get 'Blocks' property");

            var blocks = data["Blocks"]!.ToObject<List<GameDataWavePopulation>>();

            if (blocks == null)
                throw new Exception("Failed to parse SurvivalWavePopulation");

            foreach (var block in blocks)
                Bins.WavePopulations.AddBlock(block);
        }

        public static new void SaveStatic()
        {
            Bins.WavePopulations.AddBlock(Baseline);
            Bins.WavePopulations.AddBlock(Baseline_Hybrids);
            Bins.WavePopulations.AddBlock(Baseline_Chargers);
            Bins.WavePopulations.AddBlock(Baseline_Nightmare);
            Bins.WavePopulations.AddBlock(Baseline_Shadows);

            Bins.WavePopulations.AddBlock(OnlyChargers);
            Bins.WavePopulations.AddBlock(OnlyHybrids);
            Bins.WavePopulations.AddBlock(OnlyShadows);
            Bins.WavePopulations.AddBlock(OnlyNightmares);

            // Shadows
            Bins.WavePopulations.AddBlock(Shadows_WithHybrids);

            // Single enemy
            Bins.WavePopulations.AddBlock(SingleEnemy_Mother);
            Bins.WavePopulations.AddBlock(SingleEnemy_PMother);
            Bins.WavePopulations.AddBlock(SingleEnemy_Tank);
            Bins.WavePopulations.AddBlock(SingleEnemy_TankPotato);
            Bins.WavePopulations.AddBlock(SingleEnemy_Pouncer);
        }

        #region Alarm waves
        #region Baseline waves
        /// <summary>
        /// Same as vanilla baseline
        /// </summary>
        public static WavePopulation Baseline = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Striker_Wave,
            WaveRoleSpecial = Enemy.Shooter_Wave,
            WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
            WaveRoleBoss = Enemy.ShooterGiant
        };

        public static WavePopulation Baseline_Hybrids = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Striker_Wave,
            WaveRoleSpecial = Enemy.Shooter_Wave,
            WaveRoleMiniBoss = Enemy.Hybrid,
            WaveRoleBoss = Enemy.Hybrid
        };

        public static WavePopulation Baseline_Chargers = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Striker_Wave,
            WaveRoleSpecial = Enemy.Charger,
            WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
            WaveRoleBoss = Enemy.ChargerGiant
        };

        public static WavePopulation Baseline_Nightmare = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Striker_Wave,
            WaveRoleSpecial = Enemy.NightmareStriker,
            WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
            WaveRoleBoss = Enemy.NightmareShooter
        };

        public static WavePopulation Baseline_Shadows = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Striker_Wave,
            WaveRoleSpecial = Enemy.Shadow,
            WaveRoleMiniBoss = Enemy.ShooterGiant,
            WaveRoleBoss = Enemy.ShadowGiant
        };
        #endregion

        public static WavePopulation OnlyChargers = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Charger,
            WaveRoleStandard = Enemy.Charger,
            WaveRoleMiniBoss = Enemy.ChargerGiant,
        };

        public static WavePopulation OnlyHybrids = new WavePopulation
        {
            WaveRoleSpecial = Enemy.Hybrid
        };

        public static WavePopulation OnlyShadows = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Shadow,
            WaveRoleMiniBoss = Enemy.ShadowGiant,
        };

        public static WavePopulation OnlyNightmares = new WavePopulation
        {
            WaveRoleStandard = Enemy.NightmareStriker,
            WaveRoleSpecial = Enemy.NightmareShooter,
            //WaveRoleMiniBoss = Enemy.ShooterGiant_Infected,
        };

        public static WavePopulation Shadows_WithHybrids = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Shadow,
            WaveRoleSpecial = Enemy.ShadowGiant,
            WaveRoleMiniBoss = Enemy.Hybrid
        };
        #endregion

        #region Specific enemies for custom waves
        public static WavePopulation Special_StrikerGiants = new WavePopulation
        {
            WaveRoleSpecial = Enemy.StrikerGiant_Wave
        };
        #endregion

        #region Single enemies
        public static WavePopulation SingleEnemy_Mother =     new WavePopulation { WaveRoleMiniBoss = Enemy.Mother };
        public static WavePopulation SingleEnemy_PMother =    new WavePopulation { WaveRoleMiniBoss = Enemy.PMother };

        public static WavePopulation SingleEnemy_Tank =       new WavePopulation { WaveRoleMiniBoss = Enemy.Tank };
        public static WavePopulation SingleEnemy_TankPotato = new WavePopulation { WaveRoleMiniBoss = Enemy.TankPotato };

        public static WavePopulation SingleEnemy_Pouncer =    new WavePopulation { WaveRoleMiniBoss = Enemy.Pouncer };
        #endregion
    }

    public record GameDataWavePopulation : WavePopulation
    {
        /// <summary>
        /// We explicitly want to not have PIDs set when loading data, they come with their own
        /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
        /// </summary>
        public GameDataWavePopulation() : base(PidOffsets.None)
        { }
    }
}
