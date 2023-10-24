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
            // Easier mapping of enemy role, enemy, and point cost. This should map to vanilla.
            var fixedEnemies = new List<(EnemyRole, Enemy, double)>
            {
                (EnemyRole.Melee,  Enemy.Striker,      1.0),
                (EnemyRole.Melee,  Enemy.StrikerGiant, 4.0),
                (EnemyRole.Ranged, Enemy.Shooter,      1.0),
                (EnemyRole.Ranged, Enemy.ShooterGiant, 4.0),

                (EnemyRole.Lurker, Enemy.Charger,      2.0),
                (EnemyRole.Lurker, Enemy.ChargerGiant, 4.0),

                (EnemyRole.Melee,  Enemy.Shadow,       4.0),
                (EnemyRole.Melee,  Enemy.ShadowGiant,  4.0),

                (EnemyRole.PureSneak, Enemy.Hybrid,    3.0),
                (EnemyRole.Hunter,    Enemy.Hybrid,    4.0),

                (EnemyRole.PureSneak, Enemy.Mother,    10.0),
                (EnemyRole.Hunter,    Enemy.Mother,    10.0),
                (EnemyRole.MiniBoss,  Enemy.Mother,    10.0),
                (EnemyRole.PureSneak, Enemy.PMother,   10.0),
                (EnemyRole.Hunter,    Enemy.PMother,   10.0),
                (EnemyRole.Boss,      Enemy.PMother,   10.0),

                (EnemyRole.BirtherChild, Enemy.Baby,   1.0),

                (EnemyRole.PureSneak, Enemy.Tank,      10.0),
                (EnemyRole.Hunter,    Enemy.Tank,      10.0),
                (EnemyRole.MiniBoss,  Enemy.Tank,      10.0),

                (EnemyRole.Hunter,    Enemy.Pouncer,   1.0), // Weirdly Pouncer only is valued at 1pt
            };

            foreach (var (role, enemy, cost) in fixedEnemies)
            {
                // Add enemies into their own difficulty slots. This lets us short circuit the
                // random group selection process and have more control over spawning specific
                // groups of enemies.
                Roles.Add(new EnemyPopulationRole
                {
                    Role = (uint)role,
                    Difficulty = (uint)enemy,
                    Enemy = enemy,
                    Cost = cost,
                });
            }


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
