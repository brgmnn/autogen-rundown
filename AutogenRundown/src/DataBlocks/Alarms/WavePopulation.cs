using AutogenRundown.DataBlocks.Enemies;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Alarms
{
    public record class WavePopulation : DataBlock
    {
        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// </summary>
        public Enemy WaveRoleWeakling { get; set; }

        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// </summary>
        public Enemy WaveRoleStandard { get; set; }

        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// </summary>
        public Enemy WaveRoleSpecial { get; set; }

        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// </summary>
        public Enemy WaveRoleMiniBoss { get; set; }

        /// <summary>
        /// EnemyDataBlock PersistentID of eEnemyType for this population
        /// </summary>
        public Enemy WaveRoleBoss { get; set; }

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
            JArray array = JArray.Parse(GameDataWavePopulation.VanillaData);
            var waves = array.ToObject<List<GameDataWavePopulation>>();

            if (waves == null)
                throw new Exception("Failed to parse vanilla wave population data");

            foreach (var wave in waves)
            {
                Bins.WavePopulations.AddBlock(wave);
            }
        }

        public static new void SaveStatic()
        {
            Bins.WavePopulations.AddBlock(Baseline);
            Bins.WavePopulations.AddBlock(Baseline_Hybrid);

            Bins.WavePopulations.AddBlock(OnlyChargers);
            Bins.WavePopulations.AddBlock(OnlyShadows);

            Bins.WavePopulations.AddBlock(SingleEnemy_Mother);
            Bins.WavePopulations.AddBlock(SingleEnemy_PMother);
            Bins.WavePopulations.AddBlock(SingleEnemy_Tank);
            Bins.WavePopulations.AddBlock(SingleEnemy_Pouncer);
        }

        #region Alarm waves
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

        public static WavePopulation Baseline_Hybrid = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Striker_Wave,
            WaveRoleSpecial = Enemy.Shooter_Wave,
            WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
            WaveRoleBoss = Enemy.Hybrid
        };

        public static WavePopulation OnlyChargers = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Charger,
            WaveRoleStandard = Enemy.Charger,
            WaveRoleSpecial = Enemy.ChargerGiant,
        };

        public static WavePopulation OnlyShadows = new WavePopulation
        {
            WaveRoleWeakling = Enemy.Shadow,
            WaveRoleStandard = Enemy.Shadow,
            WaveRoleSpecial = Enemy.ShadowGiant,
        };
        #endregion

        #region Single enemies
        public static WavePopulation SingleEnemy_Mother = new WavePopulation { WaveRoleMiniBoss = Enemy.Mother };
        public static WavePopulation SingleEnemy_PMother = new WavePopulation { WaveRoleMiniBoss = Enemy.PMother };
        public static WavePopulation SingleEnemy_Tank = new WavePopulation { WaveRoleMiniBoss = Enemy.Tank };

        public static WavePopulation SingleEnemy_Pouncer = new WavePopulation { WaveRoleMiniBoss = Enemy.Pouncer };
        #endregion
    }

    public record class GameDataWavePopulation : WavePopulation
    {
        /// <summary>
        /// We explicitly want to not have PIDs set when loading data, they come with their own
        /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
        /// </summary>
        public GameDataWavePopulation() : base(PidOffsets.None)
        { }

        public const string VanillaData = @"[
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 18,
      ""name"": ""Baseline"",
      ""internalEnabled"": true,
      ""persistentID"": 1
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 11,
      ""WaveRoleSpecial"": 16,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 33,
      ""name"": ""Baseline_Hybrids R1D1"",
      ""internalEnabled"": true,
      ""persistentID"": 54
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 33,
      ""name"": ""Baseline Rapid Shooter R2D2"",
      ""internalEnabled"": true,
      ""persistentID"": 52
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 36,
      ""WaveRoleBoss"": 36,
      ""name"": ""Baseline MB-Birther"",
      ""internalEnabled"": true,
      ""persistentID"": 12
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 33,
      ""name"": ""Baseline MB-Hybrid"",
      ""internalEnabled"": true,
      ""persistentID"": 8
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 13,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 39,
      ""name"": ""Baseline M-Hybrid No Shooter"",
      ""internalEnabled"": true,
      ""persistentID"": 11
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 13,
      ""WaveRoleMiniBoss"": 13,
      ""WaveRoleBoss"": 33,
      ""name"": ""Baseline Rapid R2E1"",
      ""internalEnabled"": true,
      ""persistentID"": 48
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 30,
      ""WaveRoleBoss"": 18,
      ""name"": ""Baseline M-Bullrush"",
      ""internalEnabled"": true,
      ""persistentID"": 23
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 21,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 18,
      ""name"": ""Baseline Sp-Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 29
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 16,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 39,
      ""name"": ""Modified Sp-StrikerBig"",
      ""internalEnabled"": true,
      ""persistentID"": 21
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 42,
      ""WaveRoleSpecial"": 13,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 11,
      ""name"": ""Modified S-Flyer"",
      ""internalEnabled"": true,
      ""persistentID"": 27
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 42,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 11,
      ""name"": ""Modified S-Flyer_V2"",
      ""internalEnabled"": true,
      ""persistentID"": 30
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 42,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 11,
      ""name"": ""Modified S-Flyer_V3"",
      ""internalEnabled"": true,
      ""persistentID"": 41
    },
    {
      ""WaveRoleWeakling"": 42,
      ""WaveRoleStandard"": 42,
      ""WaveRoleSpecial"": 45,
      ""WaveRoleMiniBoss"": 30,
      ""WaveRoleBoss"": 30,
      ""name"": ""Modified S-Flyer_V4"",
      ""internalEnabled"": true,
      ""persistentID"": 42
    },
    {
      ""WaveRoleWeakling"": 11,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 16,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 39,
      ""name"": ""Modified Sp-Hybrid"",
      ""internalEnabled"": true,
      ""persistentID"": 9
    },
    {
      ""WaveRoleWeakling"": 11,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 33,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 39,
      ""name"": ""Modified Sp-Hybrid_R6C1"",
      ""internalEnabled"": true,
      ""persistentID"": 69
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 33,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 18,
      ""name"": ""Modified Sp-Hybrid_R5C3"",
      ""internalEnabled"": true,
      ""persistentID"": 55
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 33,
      ""WaveRoleMiniBoss"": 11,
      ""WaveRoleBoss"": 11,
      ""name"": ""Modified Rapid Shooter R2E1"",
      ""internalEnabled"": true,
      ""persistentID"": 49
    },
    {
      ""WaveRoleWeakling"": 16,
      ""WaveRoleStandard"": 16,
      ""WaveRoleSpecial"": 16,
      ""WaveRoleMiniBoss"": 18,
      ""WaveRoleBoss"": 29,
      ""name"": ""BigsAndBosses"",
      ""internalEnabled"": true,
      ""persistentID"": 22
    },
    {
      ""WaveRoleWeakling"": 16,
      ""WaveRoleStandard"": 16,
      ""WaveRoleSpecial"": 33,
      ""WaveRoleMiniBoss"": 18,
      ""WaveRoleBoss"": 29,
      ""name"": ""BigsAndBosses_v2"",
      ""internalEnabled"": true,
      ""persistentID"": 33
    },
    {
      ""WaveRoleWeakling"": 16,
      ""WaveRoleStandard"": 33,
      ""WaveRoleSpecial"": 33,
      ""WaveRoleMiniBoss"": 18,
      ""WaveRoleBoss"": 29,
      ""name"": ""BigsAndBosses S-Hybrid"",
      ""internalEnabled"": true,
      ""persistentID"": 20
    },
    {
      ""WaveRoleWeakling"": 16,
      ""WaveRoleStandard"": 16,
      ""WaveRoleSpecial"": 16,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 29,
      ""name"": ""BigsAndBosses M-Hybrid"",
      ""internalEnabled"": true,
      ""persistentID"": 25
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 16,
      ""WaveRoleSpecial"": 13,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 16,
      ""name"": ""StrikerBigs"",
      ""internalEnabled"": true,
      ""persistentID"": 4
    },
    {
      ""WaveRoleWeakling"": 29,
      ""WaveRoleStandard"": 29,
      ""WaveRoleSpecial"": 29,
      ""WaveRoleMiniBoss"": 29,
      ""WaveRoleBoss"": 29,
      ""name"": ""Tank"",
      ""internalEnabled"": true,
      ""persistentID"": 16
    },
    {
      ""WaveRoleWeakling"": 36,
      ""WaveRoleStandard"": 36,
      ""WaveRoleSpecial"": 36,
      ""WaveRoleMiniBoss"": 36,
      ""WaveRoleBoss"": 36,
      ""name"": ""Birther"",
      ""internalEnabled"": true,
      ""persistentID"": 31
    },
    {
      ""WaveRoleWeakling"": 30,
      ""WaveRoleStandard"": 30,
      ""WaveRoleSpecial"": 30,
      ""WaveRoleMiniBoss"": 39,
      ""WaveRoleBoss"": 39,
      ""name"": ""Bullrush"",
      ""internalEnabled"": true,
      ""persistentID"": 5
    },
    {
      ""WaveRoleWeakling"": 30,
      ""WaveRoleStandard"": 30,
      ""WaveRoleSpecial"": 42,
      ""WaveRoleMiniBoss"": 42,
      ""WaveRoleBoss"": 42,
      ""name"": ""BullrushAndFlyers"",
      ""internalEnabled"": true,
      ""persistentID"": 59
    },
    {
      ""WaveRoleWeakling"": 30,
      ""WaveRoleStandard"": 30,
      ""WaveRoleSpecial"": 30,
      ""WaveRoleMiniBoss"": 30,
      ""WaveRoleBoss"": 30,
      ""name"": ""Bullrush_Only"",
      ""internalEnabled"": true,
      ""persistentID"": 47
    },
    {
      ""WaveRoleWeakling"": 39,
      ""WaveRoleStandard"": 39,
      ""WaveRoleSpecial"": 39,
      ""WaveRoleMiniBoss"": 39,
      ""WaveRoleBoss"": 39,
      ""name"": ""BullrushBigs"",
      ""internalEnabled"": true,
      ""persistentID"": 15
    },
    {
      ""WaveRoleWeakling"": 30,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 39,
      ""WaveRoleBoss"": 16,
      ""name"": ""Bullrush_mix"",
      ""internalEnabled"": true,
      ""persistentID"": 17
    },
    {
      ""WaveRoleWeakling"": 30,
      ""WaveRoleStandard"": 42,
      ""WaveRoleSpecial"": 30,
      ""WaveRoleMiniBoss"": 42,
      ""WaveRoleBoss"": 16,
      ""name"": ""Bullrush_mix6"",
      ""internalEnabled"": true,
      ""persistentID"": 36
    },
    {
      ""WaveRoleWeakling"": 30,
      ""WaveRoleStandard"": 42,
      ""WaveRoleSpecial"": 39,
      ""WaveRoleMiniBoss"": 42,
      ""WaveRoleBoss"": 16,
      ""name"": ""Bullrush_mix7"",
      ""internalEnabled"": true,
      ""persistentID"": 40
    },
    {
      ""WaveRoleWeakling"": 30,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 30,
      ""WaveRoleMiniBoss"": 39,
      ""WaveRoleBoss"": 33,
      ""name"": ""Bullrush_mix2"",
      ""internalEnabled"": true,
      ""persistentID"": 19
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 30,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 39,
      ""name"": ""Bullrush_mix3"",
      ""internalEnabled"": true,
      ""persistentID"": 24
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 30,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 30,
      ""WaveRoleBoss"": 39,
      ""name"": ""Bullrush_mix4"",
      ""internalEnabled"": true,
      ""persistentID"": 13
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 30,
      ""WaveRoleMiniBoss"": 30,
      ""WaveRoleBoss"": 16,
      ""name"": ""Bullrush_mix5"",
      ""internalEnabled"": true,
      ""persistentID"": 14
    },
    {
      ""WaveRoleWeakling"": 30,
      ""WaveRoleStandard"": 16,
      ""WaveRoleSpecial"": 16,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 16,
      ""name"": ""BigsAndHybrid"",
      ""internalEnabled"": true,
      ""persistentID"": 28
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 31,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 39,
      ""name"": ""Baseline Reactor"",
      ""internalEnabled"": true,
      ""persistentID"": 6
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 31,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 33,
      ""name"": ""Rundown002_D2_Baseline_Reactor"",
      ""internalEnabled"": true,
      ""persistentID"": 46
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 31,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 18,
      ""name"": ""Rundown001_C1_Baseline_Reactor"",
      ""internalEnabled"": true,
      ""persistentID"": 53
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 31,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 18,
      ""name"": ""Baseline_Reactor_Kind - Rundown001 C1"",
      ""internalEnabled"": true,
      ""persistentID"": 50
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 31,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 16,
      ""name"": ""Baseline Reactor M-Hybrid"",
      ""internalEnabled"": true,
      ""persistentID"": 10
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 38,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 36,
      ""WaveRoleBoss"": 16,
      ""name"": ""Baseline Reactor S-Child"",
      ""internalEnabled"": true,
      ""persistentID"": 44
    },
    {
      ""WaveRoleWeakling"": 11,
      ""WaveRoleStandard"": 11,
      ""WaveRoleSpecial"": 33,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 18,
      ""name"": ""Shooters"",
      ""internalEnabled"": true,
      ""persistentID"": 3
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 21,
      ""WaveRoleSpecial"": 21,
      ""WaveRoleMiniBoss"": 35,
      ""WaveRoleBoss"": 35,
      ""name"": ""Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 7
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 21,
      ""WaveRoleSpecial"": 21,
      ""WaveRoleMiniBoss"": 21,
      ""WaveRoleBoss"": 21,
      ""name"": ""Shadows_Only"",
      ""internalEnabled"": true,
      ""persistentID"": 51
    },
    {
      ""WaveRoleWeakling"": 35,
      ""WaveRoleStandard"": 35,
      ""WaveRoleSpecial"": 35,
      ""WaveRoleMiniBoss"": 35,
      ""WaveRoleBoss"": 35,
      ""name"": ""Shadows_BigsOnly"",
      ""internalEnabled"": true,
      ""persistentID"": 38
    },
    {
      ""WaveRoleWeakling"": 21,
      ""WaveRoleStandard"": 21,
      ""WaveRoleSpecial"": 42,
      ""WaveRoleMiniBoss"": 35,
      ""WaveRoleBoss"": 35,
      ""name"": ""Shadows_Sp-Flyer"",
      ""internalEnabled"": true,
      ""persistentID"": 32
    },
    {
      ""WaveRoleWeakling"": 42,
      ""WaveRoleStandard"": 42,
      ""WaveRoleSpecial"": 42,
      ""WaveRoleMiniBoss"": 42,
      ""WaveRoleBoss"": 42,
      ""name"": ""Flyers"",
      ""internalEnabled"": true,
      ""persistentID"": 35
    },
    {
      ""WaveRoleWeakling"": 45,
      ""WaveRoleStandard"": 45,
      ""WaveRoleSpecial"": 45,
      ""WaveRoleMiniBoss"": 45,
      ""WaveRoleBoss"": 45,
      ""name"": ""Flyers_Big"",
      ""internalEnabled"": true,
      ""persistentID"": 37
    },
    {
      ""WaveRoleWeakling"": 42,
      ""WaveRoleStandard"": 42,
      ""WaveRoleSpecial"": 42,
      ""WaveRoleMiniBoss"": 45,
      ""WaveRoleBoss"": 45,
      ""name"": ""Flyers Mb-Flyer Big"",
      ""internalEnabled"": true,
      ""persistentID"": 43
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 11,
      ""WaveRoleMiniBoss"": 46,
      ""WaveRoleBoss"": 16,
      ""name"": ""Wave_Pouncer"",
      ""internalEnabled"": true,
      ""persistentID"": 39
    },
    {
      ""WaveRoleWeakling"": 46,
      ""WaveRoleStandard"": 46,
      ""WaveRoleSpecial"": 46,
      ""WaveRoleMiniBoss"": 46,
      ""WaveRoleBoss"": 46,
      ""name"": ""Pouncer"",
      ""internalEnabled"": true,
      ""persistentID"": 56
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 11,
      ""WaveRoleSpecial"": 16,
      ""WaveRoleMiniBoss"": 33,
      ""WaveRoleBoss"": 16,
      ""name"": ""Wave_Pouncer_Combo"",
      ""internalEnabled"": true,
      ""persistentID"": 45
    },
    {
      ""WaveRoleWeakling"": 13,
      ""WaveRoleStandard"": 13,
      ""WaveRoleSpecial"": 13,
      ""WaveRoleMiniBoss"": 16,
      ""WaveRoleBoss"": 16,
      ""name"": ""Strikers"",
      ""internalEnabled"": true,
      ""persistentID"": 18
    },
    {
      ""WaveRoleWeakling"": 42,
      ""WaveRoleStandard"": 42,
      ""WaveRoleSpecial"": 42,
      ""WaveRoleMiniBoss"": 42,
      ""WaveRoleBoss"": 42,
      ""name"": ""Boss_FlyerSpawn"",
      ""internalEnabled"": true,
      ""persistentID"": 26
    },
    {
      ""WaveRoleWeakling"": 44,
      ""WaveRoleStandard"": 44,
      ""WaveRoleSpecial"": 44,
      ""WaveRoleMiniBoss"": 44,
      ""WaveRoleBoss"": 44,
      ""name"": ""Boss_Squid"",
      ""internalEnabled"": true,
      ""persistentID"": 34
    }]";
    }
}
