using AutogenRundown.DataBlocks.Custom.AutogenRundown.TerminalPlacements;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record Zone
{
    /// <summary>
    /// Generate an exit tile
    /// </summary>
    /// <param name="complex"></param>
    public void GenExitGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
                (SubComplex, CustomGeomorph) = Generator.Pick(new List<(SubComplex, string)>
                {
                    (SubComplex.All,     "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_exit_01.prefab"),

                    // --- MOD Geomorphs ---
                    // DakGeos
                    (SubComplex.DigSite, "Assets/geo_64x64_dig_site_exit_dak_01.prefab"),

                    // Red_Leicester_Cheese
                    (SubComplex.Storage, "Assets/Bundles/RLC_Mining/geo_64x64_mining_storage_exit_hub_RLC_01.prefab"),

                    // SamDB
                    (SubComplex.Refinery, "Assets/Custom Geo's/Mining exit/Mining_exit_V1.prefab")
                });
                break;

            case Complex.Tech:
                (SubComplex, CustomGeomorph) = Generator.Pick(new List<(SubComplex, string)>
                {
                    (SubComplex.All,        "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_32x32_lab_exit_01.prefab"),
                    (SubComplex.DataCenter, "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_exit_01.prefab"),

                    // --- MOD Geomorphs ---
                    // SamDB
                    (SubComplex.Floodways, "Assets/Custom Geo's/Labs/lab_exit_V1/lab_exit_V1.prefab")
                });
                break;

            case Complex.Service:
                (SubComplex, CustomGeomorph) = Generator.Pick(new List<(SubComplex, string)>
                {
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/geo_32x32_floodways_exit_01.prefab"),
                    (SubComplex.Gardens,   "Assets/AssetPrefabs/Complex/Service/Geomorphs/geo_32x32_elevator_Gardens_exit_01.prefab"),

                    // --- MOD Geomorphs ---
                    // SamDB v1
                    (SubComplex.Floodways, "Assets/Custom Geo's/Floodways_exit/floodways_exit_tile.prefab"),

                    // SamDB v2
                    (SubComplex.Floodways, "Assets/SamdownGeos/Floodways Abberation Exit/Abberation_Exit_tile.prefab")
                });
                break;
        };

        Coverage = CoverageMinMax.Tiny;
    }

    /// <summary>
    /// Randomly picks a custom hub geomorph for the zone. Hub geomorphs can connect to new
    /// zones on all sides and can therefore be given a MaxConnections of 3 in the planner.
    /// See: https://docs.google.com/document/d/1iSYUASlQSaP6l7PD3HszsXSAxJ-wb8MAVwYxb9xW92c/edit
    /// </summary>
    /// <param name="complex"></param>
    public void GenHubGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // -- the problem
                    // Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_32x32_elevator_shaft_dig_site_04.prefab

                    (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 70 }),
                    (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_02.prefab", new CoverageMinMax { Min = 15, Max = 20 }),
                    (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_03.prefab", new CoverageMinMax { Min = 25, Max = 35 }),
                    (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_32x32_elevator_shaft_dig_site_04.prefab", new CoverageMinMax { Min = 20, Max = 40 }),

                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_01.prefab", new CoverageMinMax { Min = 25, Max = 35 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_02.prefab", new CoverageMinMax { Min = 50, Max = 60 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_03.prefab", new CoverageMinMax { Min = 20, Max = 40 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04.prefab", new CoverageMinMax { Min = 30, Max = 45 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_05.prefab", new CoverageMinMax { Min = 30, Max = 70 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_06.prefab", new CoverageMinMax { Min = 20, Max = 30 }),

                    (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 60 }),
                    (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_02.prefab", new CoverageMinMax { Min = 40, Max = 60 }),
                    (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_03.prefab", new CoverageMinMax { Min = 35, Max = 90 }),
                    (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_04.prefab", new CoverageMinMax { Min = 20, Max = 50 }),
                    (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_VS_01.prefab", new CoverageMinMax { Min = 25, Max = 35 }),

                    // --- MOD Geomorphs ---
                    // DakGeos
                    (SubComplex.DigSite, "assets/geo_64x64_mining_dig_site_t_dak_01.prefab", new CoverageMinMax { Min = 20, Max = 30 }),

                    // Red_Leicester_Cheese
                    (SubComplex.Refinery, "Assets/Bundles/RLC_Mining/geo_64x64_mining_refinery_X_RLC_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // SamDB v1
                    (SubComplex.DigSite, "Assets/Custom Geo's/Digsite/digsite_x_tile_1_V3/digsite_x_tile_1_V3.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.DigSite, "Assets/Custom Geo's/Digsite/Disite generator/Digsite_X_Tile_Generator.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // SamDB v2
                    (SubComplex.DigSite, "Assets/SamdownGeos/Refinery_X_tile_1/Refinery_X_Tile_1.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                });
                break;

            case Complex.Tech:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 60 }),
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_02.prefab", new CoverageMinMax { Min = 15, Max = 30 }),
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_hub_SF_01.prefab", new CoverageMinMax { Min = 35, Max = 55 }), // TODO: check its ok? Bugged in C2 Main 0.49.0
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_hub_JG_01.prefab", new CoverageMinMax { Min = 25, Max = 40 }),
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_node_transition_06_JG.prefab", new CoverageMinMax { Min = 32, Max = 45 }),

                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V2.prefab", new CoverageMinMax { Min = 25, Max = 40 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02.prefab", new CoverageMinMax { Min = 20, Max = 40 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02_V2.prefab", new CoverageMinMax { Min = 30, Max = 45 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_03.prefab", new CoverageMinMax { Min = 30, Max = 50 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_04_V3.prefab", new CoverageMinMax { Min = 25, Max = 35 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_SF_02.prefab", new CoverageMinMax { Min = 30, Max = 45 }),

                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.DataCenter, "Assets/geo_64x64_tech_data_center_hub_DS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // Red_Leicester_Cheese
                    // (SubComplex.Lab, "Assets/Bundles/RLC_Tech/geo_64x64_tech_lab_Hub_RLC_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }), // Disabled due to functioning NCR machine
                    (SubComplex.DataCenter, "Assets/Bundles/RLC_Tech/geo_64x64_tech_data_center_hub_JG_RLC_02_v3.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                });
                break;

            case Complex.Service:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_01.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_02.prefab", new CoverageMinMax { Min = 40, Max = 45 }),
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03.prefab", new CoverageMinMax { Min = 30, Max = 50 }),
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_02.prefab", new CoverageMinMax { Min = 30, Max = 50 }),

                    (SubComplex.Gardens, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_X_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Gardens, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_hub_SF_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_hub_DS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_hub_ds_02.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // SamDB v1
                    (SubComplex.Floodways, "Assets/Custom Geo's/Floodways_x_tile_1/floodways_x_tile_1.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Floodways, "Assets/Custom Geo's/Floodways_x_tile_2/floodways_x_tile_2.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Floodways, "Assets/Custom Geo's/Floodways_x_tile_3/floodways_x_tile_3.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Floodways, "Assets/Custom Geo's/Floodways_x_tile_5/Floodways_x_tile_5.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // SamDB v2
                    (SubComplex.Floodways, "Assets/SamdownGeos/Floodways Scaffolding HUB/Floodways_Scaffolding_HUB.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                });
                break;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="complex"></param>
    public void GenTGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // --- MOD Geomorphs ---
                    // DakGeos
                    (SubComplex.DigSite, "assets/geo_64x64_mining_dig_site_t_dak_01.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                });
                break;

            case Complex.Tech:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.DataCenter, "Assets/geo_64x64_tech_data_center_hub_DS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                });
                break;

            case Complex.Service:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_hub_DS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                });
                break;
        }
    }

    /// <summary>
    /// Set's the zone to a random custom I geomorph. I geomorphs can only connect to two new
    /// zones, one at either end and therefore should be given a MaxConnections of 1 in the
    /// planner.
    ///
    /// See: https://docs.google.com/document/d/1iSYUASlQSaP6l7PD3HszsXSAxJ-wb8MAVwYxb9xW92c/edit
    ///
    /// TODO: Add remaining I geomorphs.
    /// </summary>
    /// <param name="complex"></param>
    public void GenCorridorGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_I_HA_01.prefab", new CoverageMinMax { Min = 10, Max = 30 }),
                    (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_reactor_tunnel_I_HA_01.prefab", new CoverageMinMax { Min = 20, Max = 30 }),

                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_01.prefab", new CoverageMinMax { Min = 20, Max = 40 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_01_v2.prefab", new CoverageMinMax { Min = 10, Max = 20 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_02.prefab", new CoverageMinMax { Min = 12, Max = 20 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_03.prefab", new CoverageMinMax { Min = 10, Max = 30 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_04.prefab", new CoverageMinMax { Min = 40, Max = 70 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_05.prefab", new CoverageMinMax { Min = 15, Max = 20 }),
                    // Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_05_v2.prefab
                    // Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_05_v3.prefab
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_06.prefab", new CoverageMinMax { Min = 15, Max = 25 }),

                    (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_I_HA_01.prefab", new CoverageMinMax { Min = 10, Max = 20 }),

                    // --- MOD Geomorphs ---
                    // DakGeos
                    (SubComplex.DigSite, "assets/geo_64x64_dig_site_i_dak_01.prefab", new CoverageMinMax { Min = 20, Max = 30 }),

                    // Red_Leicester_Cheese
                    (SubComplex.Storage, "Assets/Bundles/RLC_Mining/geo_64x64_mining_storage_I_RLC_01.prefab", new CoverageMinMax { Min = 20, Max = 25 }),
                    (SubComplex.Refinery, "Assets/Bundles/RLC_Mining/geo_64x64_mining_refinery_I_RLC_01.prefab", new CoverageMinMax { Min = 20, Max = 25 }),

                    // SamDB
                    (SubComplex.DigSite, "Assets/Custom Geo's/Digsite/disite_i_tile_1_optimized/digsite_i_tile_1_V2.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                    (SubComplex.Refinery, "Assets/Custom Geo's/refinery/HA_1_i_tile/I_tile_V2.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                    (SubComplex.Refinery, "Assets/Custom Geo's/refinery bridge/refinery_i_tile_bridge.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                    (SubComplex.Storage, "Assets/Custom Geo's/Storage_neonate_room/neonate_storage.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                });
                break;

            case Complex.Tech:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_node_transition_02_JG.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_node_transition_03_JG.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_node_transition_06_JG.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // TODO: verify this asset path works
                    //(SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_node_transition_05_JG.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_I_HA_01.prefab", new CoverageMinMax { Min = 15, Max = 30 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_I_HA_02.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_I_HA_03.prefab", new CoverageMinMax { Min = 20, Max = 30 }),

                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.DataCenter, "Assets/geo_64x64_tech_data_center_I_tile_DS_1.prefab", new CoverageMinMax { Min = 20, Max = 30 }),

                    // FlowGeos
                    (SubComplex.DataCenter, "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_I_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // Red_Leicester_Cheese
                    // TODO: Probably disable this
                    // (SubComplex.Lab, "Assets/Bundles/RLC_Tech/geo_64x64_tech_lab_I_RLC_01.prefab", new CoverageMinMax { Min = 20, Max = 25 }),
                    // TODO: This tile seems to not have enough room to spawn items like cells and bulkhead keys
                    // (SubComplex.DataCenter, "Assets/Bundles/RLC_Tech/geo_64x64_tech_datacenter_I_RLC_01.prefab", new CoverageMinMax { Min = 20, Max = 25 }),

                    // SamDB
                    (SubComplex.Lab, "Assets/Custom Geo's/Labs/lab_i_tile/lab_i_tile_V1.prefab", new CoverageMinMax { Min = 20, Max = 30 }),

                    // The Doggy Doge
                    (SubComplex.DataCenter, "Assets/DogCustomGeos/Tilepack/DogGeos_Tech_Overpass.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                });
                break;

            case Complex.Service:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_I_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_I_HA_02.prefab", new CoverageMinMax { Min = 25, Max = 40 }),

                    (SubComplex.Gardens, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_I_01.prefab", new CoverageMinMax { Min = 20, Max = 25 }),

                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_i_bridge_ds_01.prefab", new CoverageMinMax { Min = 20, Max = 30 }),

                    // SamDB v1
                    (SubComplex.Floodways, "Assets/Custom Geo's/Floodways_I_tile_bridge/Floodways_I_tile_Bridge.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Floodways, "Assets/Custom Geo's/floodways_I_tile_1/floodways_i_tile_1.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // SamDB v2
                    (SubComplex.Floodways, "Assets/SamdownGeos/Floodways I Tile F1/Floodways_i_Tile_F1.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                });
                break;
        }
    }

    /// <summary>
    /// Places the matter wave projector pickup geomorph. DigSite has the actual geo from
    /// vanilla, Tech uses the data sphere/neonate needle geo, and service uses the huge
    /// pit geo.
    /// </summary>
    /// <param name="complex"></param>
    public void GenMatterWaveProjectorGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
                SubComplex = SubComplex.DigSite;
                CustomGeomorph = Generator.Flip() ?
                    "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_02.prefab" :
                    "Assets/SamdownGeos/T2D2 MWP room/MWP_Room.prefab";

                Coverage = new CoverageMinMax { Min = 20, Max = 20 };
                break;

            case Complex.Tech:
                SubComplex = SubComplex.Lab;
                CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_Lab_dead_end_room_02.prefab";
                Coverage = new CoverageMinMax { Min = 20, Max = 20 };
                break;

            case Complex.Service:
                SubComplex = SubComplex.Floodways;
                CustomGeomorph = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_01.prefab";
                Coverage = new CoverageMinMax { Min = 30, Max = 35 };
                break;
        }
    }

    /// <summary>
    /// Creates a reactor geomorph in the zone for use with reactor objectives.
    /// See: https://docs.google.com/document/d/1iSYUASlQSaP6l7PD3HszsXSAxJ-wb8MAVwYxb9xW92c/
    ///
    /// Note: Reactors can only be used in Mining/Tech complexes
    /// </summary>
    /// <param name="complex"></param>
    public void GenReactorGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
            {
                // reactor_open_HA_01 does not work for reactor shutdown without mods. EOS does allow this though.
                CustomGeomorph = Generator.Pick(new List<string>
                {
                    // "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_reactor_open_HA_01.prefab",
                    "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_reactor_HA_02.prefab"
                });
                SubComplex = SubComplex.Refinery;
                IgnoreRandomGeomorphRotation = true;
                Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
                break;
            }

            case Complex.Tech:
            {
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_reactor_HA_01.prefab", new CoverageMinMax { Min = 40, Max = 40 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_reactor_HA_02.prefab", new CoverageMinMax { Min = 40, Max = 40 }),

                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.DataCenter, "Assets/geo_64x64_data_center_reactor_DS_01.prefab", new CoverageMinMax { Min = 40, Max = 40 })
                });

                IgnoreRandomGeomorphRotation = true;
                break;
            }

            case Complex.Service:
            {
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // --- MOD Geomorphs ---
                    // floweria
                    (SubComplex.Floodways, "Assets/Prefabs/Geomorph/Service/geo_floodways_FA_reactor_01.prefab", new CoverageMinMax { Min = 40, Max = 40 }),

                    // donan3967
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_reactor_ds_02.prefab", new CoverageMinMax { Min = 40, Max = 40 })
                });

                IgnoreRandomGeomorphRotation = true;
                break;
            }
        };
    }

    /// <summary>
    /// Picks an appropriate geomorph for the corridor to a reactor
    ///
    /// TODO: improve this. Maybe just remove it entirely and use the regular corridor
    /// </summary>
    /// <param name="complex"></param>
    public void GenReactorCorridorGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
            {
                var (subcomplex, geomorph) = Generator.Pick(Geomorphs.Mining_I_Tile);
                CustomGeomorph = geomorph;
                SubComplex = subcomplex;

                Coverage = new CoverageMinMax { Min = 35.0, Max = 50.0 };
                break;
            }

            case Complex.Tech:
            {
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_I_HA_03_v2.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                    // --- MOD Geomorphs ---
                    // SamDB
                    (SubComplex.Lab, "Assets/Custom Geo's/Labs/lab_i_tile/lab_i_tile_V1.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                });
                break;
            }

            case Complex.Service:
            {
                GenCorridorGeomorph(complex);
                break;
            }
        }
    }

    /// <summary>
    /// Picks a generator cluster geo
    /// </summary>
    /// <param name="complex"></param>
    public void GenGeneratorClusterGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
            {
                var (sub, geo) = Generator.Pick(new List<(SubComplex, string)>
                {
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_07.prefab"),
                    (SubComplex.DigSite,  "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_01.prefab"),
                    (SubComplex.DigSite,  "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_02.prefab"),

                    // --- MOD Geomorphs ---
                    // SamDB
                    (SubComplex.DigSite, "Assets/Custom Geo's/Digsite/Disite generator/Digsite_X_Tile_Generator.prefab"),
                });

                CustomGeomorph = geo;
                SubComplex = sub;
                Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
                GeneratorClustersInZone = 1;

                break;
            }

            case Complex.Tech:
            {
                var (sub, geo) = Generator.Pick(new List<(SubComplex, string)>
                {
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02.prefab"),
                    // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02_V2.prefab"),
                    // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02_R5C2.prefab")

                    // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_LF_03_R8C2.prefab"),
                    //                  Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02.prefab
                });

                CustomGeomorph = geo;
                SubComplex = sub;
                Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
                GeneratorClustersInZone = 1;

                break;
            }

            case Complex.Service:
            {
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // Confirmed working
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_hub_ds_02_gen.prefab", CoverageMinMax.Medium_40),
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_I_HA_02.prefab", CoverageMinMax.Medium_40)

                    // Probably Broken
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_HA_08.prefab", CoverageMinMax.Medium_40),
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_HA_07.prefab", CoverageMinMax.Medium_40),
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_HA_06.prefab", CoverageMinMax.Medium_40),
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_HA_05_V2.prefab", CoverageMinMax.Medium_40),
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_HA_05.prefab", CoverageMinMax.Medium_40),
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_HA_02.prefab", CoverageMinMax.Medium_40)
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_SF_01.prefab", CoverageMinMax.Medium_40)
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_HA_04.prefab", CoverageMinMax.Medium_40),
                });

                GeneratorClustersInZone = 1;

                break;
            }
        }
    }

    /// <summary>
    /// Creates a Geomorph used as a primary objective point
    /// </summary>
    /// <param name="complex"></param>
    public void GenKingOfTheHillGeomorph(Level level, BuildDirector director)
    {
        switch (director.Complex)
        {
            case Complex.Mining:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // --- Validated and positioned spawns ---
                    // Tower uplink tile
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04_test.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    // Central tile, but it might roll to different things. We will see
                    (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_VS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04.prefab", new CoverageMinMax { Min = 30, Max = 45 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_06.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                });
                break;

            case Complex.Tech:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // --- Validated and positioned spawns ---
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_02.prefab", new CoverageMinMax { Min = 15, Max = 30 }),
                    // Very fun looking
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V3_LF.prefab", new CoverageMinMax { Min = 25, Max = 40 }),
                    // The "Monster" room
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_SF_02.prefab", new CoverageMinMax { Min = 30, Max = 45 }),
                    // Data center tower
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_hub_SF_01.prefab", new CoverageMinMax { Min = 35, Max = 55 }),
                    // The OG pillar room!
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V2.prefab", new CoverageMinMax { Min = 25, Max = 30 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_04_V3.prefab", new CoverageMinMax { Min = 25, Max = 35 }),
                });
                break;

            case Complex.Service:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // --- Validated and positioned spawns ---
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_01.prefab", new CoverageMinMax { Min = 50, Max = 60 }),
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03.prefab", new CoverageMinMax { Min = 30, Max = 50 }),
                    // Mega Nightmare Mother room
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03_V2.prefab", new CoverageMinMax { Min = 50, Max = 60 }),

                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_hub_DS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                });
                break;
        }

        // Position terminal
        // X -> +left/-right
        // Y -> +up/-down
        // Z -> +backward/-forward
        // Rotate Y -> -right/+left
        var (position, rotation) = CustomGeomorph switch
        {
            "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04_test.prefab" => (new Vector3 { X =  0.00, Y =  7.3, Z =   2.0 }, new Vector3 { Y = 180.0 }),
            "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_01.prefab" =>      (new Vector3 { X = -0.35, Y =  0.0, Z =   0.0 }, new Vector3 { Y = -90.0 }),
            "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_VS_01.prefab" =>      (new Vector3 { X =  0.00, Y =  0.0, Z =  -2.5 }, new Vector3 { Y = -15.0 }),
            "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04.prefab" =>      (new Vector3 { X =  0.00, Y =  0.0, Z =  -4.5 }, new Vector3 { Y = 180.0 }),
            "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_06.prefab" =>      (new Vector3 { X =  0.10, Y = -2.1, Z =  11.2 }, new Vector3 { Y = 180.0 }),

            // Tech
            "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V2.prefab" =>      (new Vector3 { X =  0.00, Y =  0.0, Z = 0.0 }, new Vector3 { Y = 180 }),
            "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_SF_02.prefab" =>         (new Vector3 { X =  0.00, Y = -2.1, Z = 2.0 }, new Vector3 { Y =   0 }),
            "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_02.prefab" =>       (new Vector3 { X = 10.00, Y = -4.0, Z = 3.5 }, new Vector3 { Y = 180 }),
            "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V3_LF.prefab" =>   (new Vector3 { X =  0.00, Y =  0.0, Z = 1.0 }, new Vector3 { Y =   0 }),
            "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_hub_SF_01.prefab" => (new Vector3 { X =  0.25, Y =  8.6, Z = 4.3 }, new Vector3 { Y = 180 }),
            "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_04_V3.prefab" =>      (new Vector3 { X = -3.80, Y =  0.0, Z = 6.0 }, new Vector3 { Y =  90 }),

            // Service
            "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_01.prefab" =>    (new Vector3 { X = -0.6, Y = -0.40, Z = -6.30 }, new Vector3 { Y = 170.0 }),
            "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03.prefab" =>    (new Vector3 { X =  0.0, Y =  0.00, Z = -0.20 }, new Vector3 { Y = 180.0 }),
            "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03_V2.prefab" => (new Vector3 { X = -0.4, Y =  0.00, Z =  3.95 }, new Vector3 { Y = 110.0 }),
            "Assets/geo_64x64_service_floodways_hub_DS_01.prefab" =>                                                       (new Vector3 { X =  0.6, Y =  3.85, Z =  2.15 }, new Vector3 { Y =   0.0 }),

            _ => (new Vector3(), new Vector3())
        };

        if (CustomGeomorph is not null)
            level.TerminalPlacements.Placements.Add(new TerminalPosition
            {
                Bulkhead = director.Bulkhead,
                LocalIndex = LocalIndex,
                Geomorph = CustomGeomorph ?? "",
                Position = position,
                Rotation = rotation
            });
    }

    /// <summary>
    /// Used for king of the hill or other areas
    ///
    /// Dead end geos
    /// </summary>
    /// <param name="complex"></param>
    public void GenDeadEndGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_dead_end_HA_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                    // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_dead_end_HA_01_R8B3.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                    // --- MOD Geomorphs ---
                    // Floweria
                    (SubComplex.Storage, "Assets/Prefabs/Geomorph/Mining/geo_storage_FA_dead_end_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                    // Red_Leicester_Cheese
                    (SubComplex.DigSite, "Assets/Bundles/RLC_Mining/geo_64x64_mining_digsite_dead_end_RLC_01.prefab", new CoverageMinMax { Min = 10, Max = 15 }),

                    // SamDB v2
                    (SubComplex.DataCenter, "Assets/SamdownGeos/Storage dead end spawn/DeadEnd_Storage.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                });
                break;

            case Complex.Tech:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_dead_end_HA_03_R7E1_03.prefab
                    (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_dead_end_HA_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_dead_end_HA_03.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                    // --- MOD Geomorphs ---
                    // Floweria
                    (SubComplex.Lab, "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_dead_end_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                    // SamDB v1
                    (SubComplex.Lab, "Assets/Custom Geo's/Labs/Labs dead end neonate cargo room/Labs_Neonate_cargo.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                    // SamDB v2
                    (SubComplex.DataCenter, "Assets/SamdownGeos/C2 Mainframe room/C2_Mainframe.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                });
                break;

            case Complex.Service:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_dead_end_HA_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_dead_end_HA_02.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                    // --- MOD Geomorphs ---
                    // donan3976
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_armory_DS_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                    // SamDB v1
                    (SubComplex.Floodways, "Assets/Custom Geo's/Floodways dead end spawn points/Floodways_Deadend_Spawn.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                    // SamDB v2
                    (SubComplex.Floodways, "Assets/GameObject/Floodways_Reactor_Cooling_Spawns.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                });
                break;
        }
    }

    /// <summary>
    /// Make this a boss spawn zone
    /// </summary>
    /// <param name="complex"></param>
    public void GenBossGeomorph(Complex complex)
    {
        switch (complex)
        {
            case Complex.Mining:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04.prefab", new CoverageMinMax { Min = 30, Max = 70 }),
                    (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_06.prefab", new CoverageMinMax { Min = 30, Max = 70 }),

                    // --- MOD Geomorphs ---
                });
                break;

            case Complex.Tech:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 60 }),
                    // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_04.prefab", new CoverageMinMax { Min = 30, Max = 60 }),
                    (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_R3D1.prefab", new CoverageMinMax { Min = 30, Max = 60 }),

                    // --- MOD Geomorphs ---
                });
                break;

            case Complex.Service:
                (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                {
                    // (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_01.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
                    (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03_V2.prefab", new CoverageMinMax { Min = 50, Max = 75 }),

                    // --- MOD Geomorphs ---
                    // donan3967
                    (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_boss_hub_DS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                });
                break;
        }
    }

    /// <summary>
    /// Sets a generic geomorph tile to be one of the generic garden geomorphs. This lets us
    /// still use these tiles without having them in the random pool. We can call this method
    /// only when we want to have the tile be a garden during level generation, instead of
    /// letting the level gen randomly assign it
    ///
    /// Does nothing if complex != Service.
    /// </summary>
    /// <param name="complex"></param>
    public void GenGardenGeomorph(Complex complex)
    {
        if (complex != Complex.Service)
            return;

        (CustomGeomorph, Coverage) = Generator.Pick(new List<(string, CoverageMinMax)>
        {
            ("Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_HA_01.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
            ("Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_HA_02.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
            ("Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_SF_01.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
            ("Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_AW_01.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
            ("Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_JG_01.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
            ("Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_JG_02.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
            ("Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_HA_03.prefab", new CoverageMinMax { Min = 50, Max = 75 })

            // --- MOD Geomorphs ---
        });
        SubComplex = SubComplex.Gardens;
    }

    /// <summary>
    /// Sets the zone to have a portal geomorph in it
    /// </summary>
    public void GenPortalGeomorph()
    {
        switch (level.Complex)
        {
            // This tile contains a possible path forward.
            case Complex.Mining:
                CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_portal_HA_01.prefab";
                break;

            // This tile is a dead end
            case Complex.Tech:
                CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_portal_HA_01.prefab";
                break;

            case Complex.Service:
            default:
                Plugin.Logger.LogError($"Attempted to build a portal geomorph for Service complex");
                break;
        }
    }
}
