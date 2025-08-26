using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutogenRundown.DataBlocks.Enums;

namespace AutogenRundown.DataBlocks.Zones;

public class Geomorphs
{
    public static List<(SubComplex, string)> Mining_I_Tile = new List<(SubComplex, string)>
    {
        (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_05.prefab"),
        (SubComplex.DigSite,  "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_I_HA_01.prefab"),

        // SamDB
        (SubComplex.DigSite, "Assets/Custom Geo's/Digsite/disite_i_tile_1_optimized/digsite_i_tile_1_V2.prefab"),
        (SubComplex.Refinery, "Assets/Custom Geo's/refinery/HA_1_i_tile/I_tile_V2.prefab"),
        (SubComplex.Refinery, "Assets/Custom Geo's/refinery bridge/refinery_i_tile_bridge.prefab"),
    };

    /*public static (SubComplex, string) GenGeomorph(Complex complex)
    {
        return
    }*/
}
