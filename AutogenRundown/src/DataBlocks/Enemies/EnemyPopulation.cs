using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Enemies
{
    public record class EnemyPopulation : DataBlock
    {
        [JsonIgnore]
        public static List<EnemyRole> Roles = new List<EnemyRole>(); 

        public static new void Setup()
        {
            JArray array = JArray.Parse(EnemyRole.VanillaData);
            var vanillaGroups = array.ToObject<List<EnemyRole>>();

            foreach (var group in vanillaGroups)
            {
                Roles.Add(group);
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

        public List<EnemyRole> RoleDatas { get; set; } = new List<EnemyRole> { };
    }
}
