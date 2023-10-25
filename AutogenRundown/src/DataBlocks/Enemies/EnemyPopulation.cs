using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Enemies
{
    public record class EnemyPopulation : DataBlock
    {
        [JsonIgnore]
        public static List<EnemyPopulationRole> Roles = new List<EnemyPopulationRole>(); 

        public static new void Setup()
        {
            JArray array = JArray.Parse(EnemyPopulationRole.VanillaData);
            var vanillaRoles = array.ToObject<List<EnemyPopulationRole>>();

            foreach (var popRole in vanillaRoles)
            {
                Roles.Add(popRole);
            }
        }

        public static new void SaveStatic()
        {
            #region Individual enemy populations
            // Easier mapping of enemy role, enemy, and point cost. This should map to vanilla.
            var fixedEnemies = new List<EnemyInfo>
            {
                EnemyInfo.Striker,
                EnemyInfo.StrikerGiant,
                EnemyInfo.Shooter,
                EnemyInfo.ShooterGiant,
                EnemyInfo.Charger,
                EnemyInfo.ChargerGiant,
                EnemyInfo.Shadow,
                EnemyInfo.ShadowGiant,
                EnemyInfo.Hybrid,
                EnemyInfo.Hybrid_Hunter,
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

            #region Level Tier populations
            // ================  Tier A  ================
            var enemiesTierA = new List<(EnemyInfo, double)>
            {
                (EnemyInfo.Striker, 1.0),
                (EnemyInfo.Shooter, 1.0),
                (EnemyInfo.StrikerGiant, 0.5),
                (EnemyInfo.ShooterGiant, 0.5)
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

            // ================  Tier B  ================
            var enemiesTierB = new List<(EnemyInfo, double)>
            {
                (EnemyInfo.Striker, 1.0),
                (EnemyInfo.Shooter, 1.0),
                (EnemyInfo.StrikerGiant, 0.75),
                (EnemyInfo.ShooterGiant, 0.5)
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

            // ================  Tier C  ================
            var enemiesTierC = new List<(EnemyInfo, double)>
            {
                (EnemyInfo.Striker, 1.0),
                (EnemyInfo.Shooter, 1.0),
                (EnemyInfo.StrikerGiant, 0.75),
                (EnemyInfo.ShooterGiant, 0.5),
                (EnemyInfo.Charger, 1.0),
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

            // ================  Tier D  ================
            var enemiesTierD = new List<(EnemyInfo, double)>
            {
                (EnemyInfo.Striker, 1.0),
                (EnemyInfo.Shooter, 1.0),
                (EnemyInfo.StrikerGiant, 0.75),
                (EnemyInfo.ShooterGiant, 0.5)
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

            // ================  Tier E  ================
            var enemiesTierE = new List<(EnemyInfo, double)>
            {
                (EnemyInfo.Striker, 1.0),
                (EnemyInfo.Shooter, 1.0),
                (EnemyInfo.StrikerGiant, 0.75),
                (EnemyInfo.ShooterGiant, 0.5)
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
            #endregion

            Bins.EnemyPopulations.AddBlock(new EnemyPopulation
            {
                Name = "DefaultPop_AutogenRundown",
                PersistentId = 1,
                RoleDatas = Roles
            });
        }

        public List<EnemyPopulationRole> RoleDatas { get; set; } = new List<EnemyPopulationRole> { };
    }
}
