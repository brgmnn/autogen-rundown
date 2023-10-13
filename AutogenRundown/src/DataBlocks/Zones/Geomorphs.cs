using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutogenRundown.DataBlocks.Zones
{
    internal class Geomorphs
    {
        public static List<(SubComplex, string)> Mining_I_Tile = new List<(SubComplex, string)>
        {
            (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_05.prefab"),
            (SubComplex.DigSite,  "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_I_HA_01.prefab")
        };

        /*public static (SubComplex, string) GenGeomorph(Complex complex)
        {
            return 
        }*/
    }
}
