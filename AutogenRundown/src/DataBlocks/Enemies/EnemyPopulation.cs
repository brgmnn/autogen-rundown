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
