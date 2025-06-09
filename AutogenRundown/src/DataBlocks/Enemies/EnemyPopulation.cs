using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Enemies;

public record EnemyPopulation : DataBlock
{
    [JsonIgnore]
    public static List<EnemyPopulationRole> Roles = new();

    public static void Setup()
    {
        // JArray array = JArray.Parse(EnemyPopulationRole.VanillaData);
        // var vanillaRoles = array.ToObject<List<EnemyPopulationRole>>();
        //
        // if (vanillaRoles == null)
        //     throw new JsonException("Failed to parse vanilla enemy population data");
        //
        // foreach (var popRole in vanillaRoles)
        // {
        //     Roles.Add(popRole);
        // }
    }

    public new static void SaveStatic()
    {
        #region Individual enemy populations
        // Easier mapping of enemy role, enemy, and point cost. This should map to vanilla.
        var fixedEnemies = new List<EnemyInfo>
        {
            EnemyInfo.Striker,
            EnemyInfo.StrikerGiant,
            EnemyInfo.Striker_Wave,
            EnemyInfo.Shooter,
            EnemyInfo.ShooterGiant,
            EnemyInfo.Shooter_Wave,
            EnemyInfo.Charger,
            EnemyInfo.ChargerGiant,
            EnemyInfo.Shadow,
            EnemyInfo.ShadowGiant,
            EnemyInfo.Hybrid,
            EnemyInfo.Hybrid_Hunter,
            EnemyInfo.NightmareShooter,
            EnemyInfo.NightmareStriker,
            EnemyInfo.NightmareStrikerGiant,
            EnemyInfo.Flyer,
            EnemyInfo.FlyerBig,
            EnemyInfo.Mother,
            EnemyInfo.Mother_Hunter,
            EnemyInfo.Mother_MiniBoss,
            EnemyInfo.PMother,
            EnemyInfo.PMother_Hunter,
            EnemyInfo.PMother_MiniBoss,
            EnemyInfo.BirtherChild,
            EnemyInfo.Tank,
            EnemyInfo.Tank_Hunter,
            EnemyInfo.Tank_MiniBoss,
            EnemyInfo.Pouncer,

            EnemyInfo.PouncerShadow,
            EnemyInfo.PouncerShadow_Sneak,
            EnemyInfo.HybridInfected,
            EnemyInfo.HybridInfected_Hunter,
            EnemyInfo.NightmareGiant,
        };

        foreach (var info in fixedEnemies)
        {
            // Add enemies into their own difficulty slots. This lets us short circuit the
            // random group selection process and have more control over spawning specific
            // groups of enemies.
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)info.Enemy,
                Enemy = info.Enemy,
                Cost = info.Points,
            });
        }
        #endregion

        #region Scouts

        var scouts = new List<(uint, Enemy)>
        {
            (EnemySpawningData.Scout.Difficulty,          Enemy.Scout),
            (EnemySpawningData.ScoutCharger.Difficulty,   Enemy.ScoutCharger),
            (EnemySpawningData.ScoutShadow.Difficulty,    Enemy.ScoutShadow),
            (EnemySpawningData.ScoutNightmare.Difficulty, Enemy.ScoutNightmare),
            (EnemySpawningData.ScoutZoomer.Difficulty,    Enemy.ScoutZoomer)
        };

        foreach (var (difficulty, enemy) in scouts)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = 5u,
                Difficulty = difficulty,
                Enemy = enemy,
                Cost = 5,
            });
        }

        #endregion

        #region Boss Aligned spawns
        // We add the bosses again but including the boss aligned spawn difficulty mask.
        // These are then set up again in EnemyGroup.cs to spawn with boss alignment on
        // geos that support it.
        foreach (var bossInfo in EnemyInfo.SpawnAlignedBosses)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)bossInfo.Role,
                Difficulty = (uint)AutogenDifficulty.BossAlignedSpawn | (uint)bossInfo.Enemy,
                Enemy = bossInfo.Enemy,
                Cost = bossInfo.Points,
            });
        }
        #endregion

        #region MegaMother spawn

        var megaMotherSpawnEnemies = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Striker_Wave, 2.0),
            (EnemyInfo.BirtherChild, 1.0)
        };

        foreach (var (info, points) in megaMotherSpawnEnemies)
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)EnemyRole.BirtherChild,
                Difficulty = (uint)AutogenDifficulty.MegaMotherSpawn,
                Enemy = info.Enemy,
                Cost = points,
            });

        #endregion

        #region Level Tier populations
        #region Tier-A
        /** ================  Tier A  ================
         * A-Tier is the easiest tier. It should be the most common tier and not have difficult
         * eneies. No Chargers / Shadows / Hybrids spawn here.
         * */
        var enemiesTierA = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Striker, 1.0),
            (EnemyInfo.Shooter, 1.0),
            (EnemyInfo.StrikerGiant, 0.5),
            (EnemyInfo.ShooterGiant, 0.25)
        };

        foreach (var (info, weight) in enemiesTierA)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)AutogenDifficulty.TierA,
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }
        #endregion

        #region Tier-B
        /** ================  Tier B  ================
         * A step up from A-Tier. Still no Chargers / Shadows / Hybrids spawn here.
         * */
        var enemiesTierB = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Striker, 1.0),
            (EnemyInfo.Shooter, 1.0),
            (EnemyInfo.StrikerGiant, 0.75),
            (EnemyInfo.ShooterGiant, 0.25)
        };

        foreach (var (info, weight) in enemiesTierB)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)AutogenDifficulty.TierB,
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }
        #endregion

        #region Tier-C
        /** ================  Tier C  ================
         * Standard difficulty. We can get regular chargers for this.
         * */
        var enemiesTierC = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Striker, 1.0),
            (EnemyInfo.Shooter, 1.0),
            (EnemyInfo.StrikerGiant, 0.75),
            (EnemyInfo.ShooterGiant, 0.5)
        };

        foreach (var (info, weight) in enemiesTierC)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)AutogenDifficulty.TierC,
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        // Add charger to C. C will only spawn regular chargers so no need to add giant.
        Roles.Add(new EnemyPopulationRole
        {
            Role = (uint)EnemyInfo.Charger.Role,
            Difficulty = (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Chargers),
            Enemy = EnemyInfo.Charger.Enemy,
            Cost = EnemyInfo.Charger.Points,
            Weight = 1.0
        });

        var nightmaresTierC = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.NightmareStriker, 1.0),
            (EnemyInfo.NightmareShooter, 1.0)
        };

        foreach (var (info, weight) in nightmaresTierC)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Nightmares),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }
        #endregion

        #region Tier-D
        /** ================  Tier D  ================
         * Difficult tier. We get all enemies except shadow giants on here.
         * */
        var enemiesTierD = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Striker, 1.0),
            (EnemyInfo.Shooter, 1.0),
            (EnemyInfo.StrikerGiant, 1.0),
            (EnemyInfo.ShooterGiant, 0.3),
        };

        foreach (var (info, weight) in enemiesTierD)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)AutogenDifficulty.TierD,
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        var chargersTierD = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Charger, 1.0),
            (EnemyInfo.ChargerGiant, 0.4),
        };

        foreach (var (info, weight) in chargersTierD)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Chargers),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        var shadowsTierD = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Shadow, 1.0),
            (EnemyInfo.ShadowGiant, 0.4)
        };

        foreach (var (info, weight) in shadowsTierD)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Shadows),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        var hybridsTierD = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Hybrid, 1.0),
        };

        foreach (var (info, weight) in hybridsTierD)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Hybrids),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        var nightmaresTierD = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.NightmareStriker, 1.0),
            (EnemyInfo.NightmareShooter, 1.0),
            (EnemyInfo.NightmareGiant, 0.2)
        };

        foreach (var (info, weight) in nightmaresTierD)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Nightmares),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }
        #endregion

        #region Tier-E
        /** ================  Tier E  ================
         * The hardest tier. All enemies are available to be spawned
         * */
        var enemiesTierE = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Striker, 1.0),
            (EnemyInfo.Shooter, 1.0),
            (EnemyInfo.StrikerGiant, 1.0),
            (EnemyInfo.ShooterGiant, 0.3),
        };

        foreach (var (info, weight) in enemiesTierE)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)AutogenDifficulty.TierE,
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        var chargersTierE = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Charger, 1.0),
            (EnemyInfo.ChargerGiant, 0.4),
        };

        foreach (var (info, weight) in chargersTierE)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Chargers),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        var shadowsTierE = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Shadow, 1.0),
            (EnemyInfo.ShadowGiant, 0.4)
        };

        foreach (var (info, weight) in shadowsTierE)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Shadows),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        var hybridsTierE = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.Hybrid, 1.0),
        };

        foreach (var (info, weight) in hybridsTierE)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Hybrids),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }

        var nightmaresTierE = new List<(EnemyInfo, double)>
        {
            (EnemyInfo.NightmareStriker, 1.0),
            (EnemyInfo.NightmareShooter, 1.0),
            (EnemyInfo.NightmareGiant, 0.2)
        };

        foreach (var (info, weight) in nightmaresTierE)
        {
            Roles.Add(new EnemyPopulationRole
            {
                Role = (uint)info.Role,
                Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Nightmares),
                Enemy = info.Enemy,
                Cost = info.Points,
                Weight = weight
            });
        }
        #endregion
        #endregion

        Bins.EnemyPopulations.AddBlock(new EnemyPopulation
        {
            Name = "DefaultPop_AutogenRundown",
            PersistentId = 1,
            RoleDatas = Roles
        });
    }

    public List<EnemyPopulationRole> RoleDatas { get; set; } = new();
}
