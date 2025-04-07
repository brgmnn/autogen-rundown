using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Light;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    /// <summary>
    /// eZoneBuildFromExpansionType
    ///
    /// What direction to try and place the entrance door for this zone
    ///
    /// Direction is global and forward is looking from the drop elevator
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonebuildfromexpansiontype
    /// </summary>
    public enum ZoneBuildExpansion
    {
        Random = 0,
        Forward = 1,
        Backward = 2,
        Right = 3,
        Left = 4
    }

    /// <summary>
    /// eZoneExpansionType
    ///
    /// What direction to build the zone towards
    ///
    /// Direction is global and forward is looking from the drop elevator
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezoneexpansiontype
    /// </summary>
    public enum ZoneExpansion
    {
        Random = 0,
        Collapsed = 1,
        Expansional = 2,
        Forward = 3,
        Backward = 4,
        Right = 5,
        Left = 6,
        DirectionalRandom = 7
    }

    // ZoneExpansion = ZoneExpansion.Right;
    // StartExpansion = ZoneBuildExpansion.Right;
    // StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;

    // "StartPosition": 0,
    // "StartExpansion": 0,
    // "ZoneExpansion": 0,

    /// <summary>
    /// Where in the source zone to try to make the entrance to this zone.
    /// Note that a valid gate may not generate around the set source position/area.
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonebuildfromtype
    /// </summary>
    public enum ZoneEntranceBuildFrom
    {
        Random = 0,
        Start = 1,
        AverageCenter = 2,
        Furthest = 3,
        BetweenStartAndFurthest = 4,
        IndexWeight = 5
    }

    /// <summary>
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonedistributionamount
    /// </summary>
    public enum DistributionAmount
    {
        None = 0,

        /// <summary>
        /// Count = 2
        /// </summary>
        Pair = 1,

        /// <summary>
        /// Count = 5
        /// </summary>
        Few = 2,

        /// <summary>
        /// Count = 10
        /// </summary>
        Some = 3,

        /// <summary>
        /// Count = 15
        /// </summary>
        SomeMore = 4,

        /// <summary>
        /// Count = 20
        /// </summary>
        Many = 5,

        /// <summary>
        /// Count = 30
        /// </summary>
        Alot = 6,

        /// <summary>
        /// Count = 50
        /// </summary>
        Tons = 7
    }

    /// <summary>
    ///
    /// </summary>
    public record Zone : DataBlock
    {
        #region Direction manipulation
        /// <summary>
        /// Sets this zones expansion and build directions to be a branching direction of the parent zone
        ///
        /// It's quite an annoying function as we must enumerate how each rotation works for a
        /// given direction
        /// </summary>
        /// <param name="zone"></param>
        public void SetExpansionAsBranchOfZone(Zone zone, Direction leftOrRight)
        {
            switch (zone.ZoneExpansion)
            {
                case ZoneExpansion.Forward:
                    {
                        if (leftOrRight == Direction.Left)
                        {
                            ZoneExpansion = ZoneExpansion.Left;
                            StartExpansion = ZoneBuildExpansion.Left;

                            // TODO: Better heuristic for what to do with this
                            StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                        }
                        else
                        {
                            ZoneExpansion = ZoneExpansion.Right;
                            StartExpansion = ZoneBuildExpansion.Right;
                            StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                        }

                        break;
                    }

                case ZoneExpansion.Left:
                    {
                        if (leftOrRight == Direction.Left)
                        {
                            ZoneExpansion = ZoneExpansion.Backward;
                            StartExpansion = ZoneBuildExpansion.Backward;
                            StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                        }
                        else
                        {
                            ZoneExpansion = ZoneExpansion.Forward;
                            StartExpansion = ZoneBuildExpansion.Forward;
                            StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                        }

                        break;
                    }

                case ZoneExpansion.Right:
                    {
                        if (leftOrRight == Direction.Left)
                        {
                            ZoneExpansion = ZoneExpansion.Forward;
                            StartExpansion = ZoneBuildExpansion.Forward;
                            StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                        }
                        else
                        {
                            ZoneExpansion = ZoneExpansion.Backward;
                            StartExpansion = ZoneBuildExpansion.Backward;
                            StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                        }

                        break;
                    }

                case ZoneExpansion.Backward:
                    {
                        if (leftOrRight == Direction.Left)
                        {
                            ZoneExpansion = ZoneExpansion.Right;
                            StartExpansion = ZoneBuildExpansion.Right;
                        }
                        else
                        {
                            ZoneExpansion = ZoneExpansion.Left;
                            StartExpansion = ZoneBuildExpansion.Left;
                        }

                        break;
                    }
            }
        }

        public void SetStartExpansionFromExpansion()
        {
            StartExpansion = ZoneExpansion switch
            {
                ZoneExpansion.Forward => ZoneBuildExpansion.Forward,
                ZoneExpansion.Backward => ZoneBuildExpansion.Backward,
                ZoneExpansion.Left => ZoneBuildExpansion.Left,
                ZoneExpansion.Right => ZoneBuildExpansion.Right,
                _ => StartExpansion
            };
        }
        #endregion

        #region Custom geomorph settings
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
                        (SubComplex.Storage, "Assets/Bundles/RLC_Mining/geo_64x64_mining_storage_exit_hub_RLC_01.prefab")
                    });
                    break;

                case Complex.Tech:
                    (SubComplex, CustomGeomorph) = Generator.Pick(new List<(SubComplex, string)>
                    {
                        (SubComplex.All,        "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_32x32_lab_exit_01.prefab"),
                        (SubComplex.DataCenter, "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_exit_01.prefab")
                    });
                    break;

                case Complex.Service:
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Service/Geomorphs/geo_32x32_floodways_exit_01.prefab";
                    SubComplex = SubComplex.Floodways;
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
                        (SubComplex.Lab, "Assets/Bundles/RLC_Tech/geo_64x64_tech_lab_Hub_RLC_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                        (SubComplex.DataCenter, "Assets/Bundles/RLC_Tech/geo_64x64_tech_data_center_hub_JG_RLC_02_v3.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    });
                    break;

                case Complex.Service:
                    (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                    {
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_01.prefab", new CoverageMinMax { Min = 50, Max = 75 }),
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_02.prefab", new CoverageMinMax { Min = 40, Max = 45 }),
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03.prefab", new CoverageMinMax { Min = 30, Max = 50 }),

                        // TODO: Remove this perhaps, it's quite hard in bulkhead
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_02.prefab", new CoverageMinMax { Min = 30, Max = 50 }),

                        (SubComplex.Gardens, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_X_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                        // --- MOD Geomorphs ---
                        // donan3967
                        (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_hub_DS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                        (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_hub_ds_02.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
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
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_02.prefab";
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
                    CustomGeomorph = "Assets/Prefabs/Geomorph/Service/geo_floodways_FA_reactor_01.prefab";
                    SubComplex = SubComplex.Floodways;
                    IgnoreRandomGeomorphRotation = true;
                    Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
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
                        var (subcomplex, geomorph) = Generator.Pick(Zones.Geomorphs.Mining_I_Tile);
                        CustomGeomorph = geomorph;
                        SubComplex = subcomplex;

                        Coverage = new CoverageMinMax { Min = 35.0, Max = 50.0 };
                        break;
                    }

                case Complex.Tech:
                    {
                        CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_I_HA_03_v2.prefab";
                        SubComplex = SubComplex.Lab;
                        Coverage = new CoverageMinMax { Min = 30.0, Max = 40.0 };
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
        /// It seems this is fairly broken. The reliability of getting the generator cluster does not seem good.
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
                            // Succss rolls:   1
                            // Failure rolls:  1
                            //
                            // Bad zone spawn, tiny room: 2 -- (zone was tiny, seemed to really badly spawn. Bad spawn direction?)
                            (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_07.prefab"),

                            //(SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_01.prefab"),
                            //(SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_02.prefab"),
                        });

                        CustomGeomorph = geo;
                        SubComplex = sub;
                        Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
                        GeneratorClustersInZone = 1;

                        // Unclear how much these matter but re-rolling for the generator seems common
                        SubSeed = 24;
                        MarkerSubSeed = 3;
                        LightsSubSeed = 1;

                        break;
                    }

                case Complex.Tech:
                    {
                        var (sub, geo) = Generator.Pick(new List<(SubComplex, string)>
                        {
                            // Succss rolls:  2
                            // Failure rolls: 4
                            (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02.prefab"),
                            //(SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02_V2.prefab"),

                            //(SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02_R5C2.prefab")
                            //                  Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02.prefab

                        });

                        CustomGeomorph = geo;
                        SubComplex = sub;
                        Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
                        GeneratorClustersInZone = 1;

                        // Unclear how much these matter but re-rolling for the generator seems common
                        SubSeed = 24;
                        MarkerSubSeed = 1;
                        LightsSubSeed = 1;

                        break;
                    }
            }
        }

        /// <summary>
        /// Creates a Geomorph used as a primary objective point
        /// </summary>
        /// <param name="complex"></param>
        public void GenKingOfTheHillGeomorph(Complex complex)
        {
            switch (complex)
            {
                case Complex.Mining:
                    (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                    {
                        // // Doesn't place the terminal right. It always places it off to the side which causes the scan
                        // // to be very goofy
                        // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                        // No good either
                        // (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_02.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                        // // Tower uplink tile
                        (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04_test.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                        // Standard good hubs for this
                        (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                        (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_VS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),

                        // TODO: check these or other tiles for good candidates for "The Hill"
                        // (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 70 }),
                        // (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_02.prefab", new CoverageMinMax { Min = 15, Max = 20 }),
                        // (SubComplex.DigSite, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_HA_03.prefab", new CoverageMinMax { Min = 25, Max = 35 }),
                        //
                        // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_01.prefab", new CoverageMinMax { Min = 25, Max = 35 }),
                        // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_02.prefab", new CoverageMinMax { Min = 50, Max = 60 }),
                        // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_03.prefab", new CoverageMinMax { Min = 20, Max = 40 }),
                        // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04.prefab", new CoverageMinMax { Min = 30, Max = 45 }),
                        // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_05.prefab", new CoverageMinMax { Min = 30, Max = 70 }),
                        // (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_06.prefab", new CoverageMinMax { Min = 20, Max = 30 }),
                    });
                    break;

                case Complex.Tech:
                    (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                    {
                        // --- Bad ---
                        // (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 60 }),

                        // --- Maybe (terminal spawn seems no good) ---
                        // (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_02.prefab", new CoverageMinMax { Min = 15, Max = 30 }),
                        // Very fun looking
                        // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V3_LF.prefab", new CoverageMinMax { Min = 25, Max = 40 }),
                        // The "Monster" room
                        // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_SF_02.prefab", new CoverageMinMax { Min = 30, Max = 45 }),

                        // --- Missing from resource ---
                        // (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_hub_SF_01.prefab", new CoverageMinMax { Min = 35, Max = 55 }),


                        // --- Good ---
                        // The OG pillar room!
                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V2.prefab", new CoverageMinMax { Min = 25, Max = 30 }),

                        // TODO: check
                        // (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_hub_JG_01.prefab", new CoverageMinMax { Min = 25, Max = 40 }),
                        // (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_node_transition_06_JG.prefab", new CoverageMinMax { Min = 32, Max = 45 }),
                        //
                        // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_03.prefab", new CoverageMinMax { Min = 30, Max = 50 }),
                        // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_04_V3.prefab", new CoverageMinMax { Min = 25, Max = 35 }),
                        // (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_SF_02.prefab", new CoverageMinMax { Min = 30, Max = 45 }),
                    });
                    break;

                case Complex.Service:
                    (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                    {
                        // --- Good ---
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_01.prefab", new CoverageMinMax { Min = 50, Max = 60 }),
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03.prefab", new CoverageMinMax { Min = 30, Max = 50 }),

                        // Mega Nightmare Mother room
                        // TODO: do we actually want it for this? Seems cool
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03_V2.prefab", new CoverageMinMax { Min = 50, Max = 60 }),
                    });
                    break;
            }
        }

        /// <summary>
        /// Used for king of the hill or other areas
        ///
        /// Dead end geos
        /// </summary>
        /// <param name="complex"></param>
        public void GenKingOfTheHillSpawnGeomorph(Complex complex)
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
                    });
                    break;

                case Complex.Service:
                    (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                    {
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_dead_end_HA_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_dead_end_HA_02.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
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
                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_04.prefab", new CoverageMinMax { Min = 30, Max = 60 }),

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
        #endregion

        #region Alarms
        private void AlarmModifier_LightsOff()
        {
            EventsOnDoorScanStart.AddLightsOff(3.0);
            EventsOnDoorScanDone.AddLightsOn(1.0);
        }

        /// <summary>
        /// Ideas for alarm modifications
        ///
        ///     * Lights off during alarm
        ///         * Lights flashing during alarm
        ///         * Lights change during alarm?
        ///     * Fog rising during alarm
        ///         * Fog lowering from ceiling
        ///         * Fog clears in fog level during alarm
        ///     * Spawn custom enemies, tank / mother / pouncer etc.
        ///     * Turn on the security error scans from R8?
        ///
        /// </summary>
        /// <param name="puzzlePack"></param>
        /// <param name="wavePopulationPack"></param>
        /// <param name="waveSettingsPack"></param>
        public void RollAlarms(
            Level level,
            LevelLayout layout,
            ICollection<(double, int, ChainedPuzzle)> puzzlePack,
            ICollection<(double, int, WavePopulation)> wavePopulationPack,
            ICollection<(double, int, WaveSettings)> waveSettingsPack)
        {
            if (LocalIndex == 0 || Alarm == ChainedPuzzle.SkipZone || Alarm != ChainedPuzzle.None)
                return;

            // Grab a random puzzle from the puzzle pack
            var puzzle = Generator.DrawSelect(puzzlePack);

            if (puzzle == null)
                return;

            // TODO: Randomize things like travel distance here
            // We only copy the population settings in if we have an actual alarm here
            if (puzzle.TriggerAlarmOnActivate && !puzzle.FixedAlarm)
            {
                var population = Generator.DrawSelect(wavePopulationPack)!;
                var settings = Generator.DrawSelect(waveSettingsPack)!;

                // Rescale settings by difficulty factor
                if (!population.DifficultyFactor.ApproxEqual(1.0))
                {
                    settings = settings.ScaleDownFor(population.DifficultyFactor).FindOrPersist();
                    Plugin.Logger.LogDebug($"Rescaling wave settings! difficulty={population.DifficultyFactor}, settings={settings}");
                }

                puzzle = puzzle with { Population = population, Settings = settings };
                Plugin.Logger.LogDebug($"Zone {LocalIndex} alarm(rolled): {puzzle}");
            }
            else
            {
                Plugin.Logger.LogDebug($"Zone {LocalIndex} alarm(fixed): {puzzle}");
            }

            if (puzzle.Comment != null)
                Plugin.Logger.LogWarning($"Zone {LocalIndex} alarm(secret): EventsOnDoorScanStart={puzzle.EventsOnDoorScanStart.Count} EventsOnDoorOpen={puzzle.EventsOnOpenDoor.Count}");

            EventsOnApproachDoor.AddRange(puzzle.EventsOnApproachDoor);
            EventsOnUnlockDoor.AddRange(puzzle.EventsOnUnlockDoor);
            EventsOnOpenDoor.AddRange(puzzle.EventsOnOpenDoor);
            EventsOnDoorScanStart.AddRange(puzzle.EventsOnDoorScanStart);
            EventsOnDoorScanDone.AddRange(puzzle.EventsOnDoorScanDone);

            var parent = level.Planner.GetBuildFrom(new ZoneNode { Bulkhead = Bulkhead.Extreme, ZoneNumber = LocalIndex });

            // Add custom events on alarms
            if (!puzzle.FixedAlarm)
            {
                switch (level.Tier)
                {
                    case "B":
                    {
                        // Small chance to disable lights during the alarm
                        if (Generator.Flip(0.08))
                            AlarmModifier_LightsOff();

                        if (Generator.Flip(0.005))
                            EventsOnApproachDoor.AddSpawnWave(
                                GenericWave.SinglePouncer,
                                Generator.Between(1, 6));

                        break;
                    }

                    case "C":
                    {
                        // Small chance to disable lights during the alarm
                        if (Generator.Flip(0.1))
                            AlarmModifier_LightsOff();

                        // Tiny chance for bait door approach pouncer
                        if (Generator.Flip(0.01))
                            EventsOnApproachDoor.AddSpawnWave(
                                GenericWave.SinglePouncer,
                                Generator.Between(1, 6));

                        break;
                    }

                    case "D":
                    {
                        // Small chance to disable lights during the alarm
                        if (Generator.Flip(0.12))
                        {
                            AlarmModifier_LightsOff();

                            // Shadow pouncer with the lights off!
                            if (Generator.Flip(0.1))
                                EventsOnDoorScanStart.AddSpawnWave(
                                    GenericWave.SinglePouncerShadow,
                                    Generator.Between(4, 16));
                        }

                        // Tiny chance for bait door approach pouncer
                        if (Generator.Flip(0.01))
                            EventsOnApproachDoor.AddSpawnWave(
                                Generator.Flip() ?
                                    GenericWave.SinglePouncerShadow :
                                    GenericWave.SinglePouncer,
                                Generator.Between(1, 6));

                        // "Fun" single enemies on start wave
                        if (Generator.Flip(0.05))
                            EventsOnDoorScanStart.AddSpawnWave(GenericWave.SingleTank, Generator.Between(1, 27));
                        break;
                    }

                    case "E":
                    {
                        // Small chance to disable lights during the alarm
                        if (Generator.Flip(0.3))
                        {
                            AlarmModifier_LightsOff();

                            // Shadow pouncer with the lights off!
                            if (Generator.Flip(0.1))
                                EventsOnDoorScanStart.AddSpawnWave(
                                    GenericWave.SinglePouncerShadow,
                                    Generator.Between(4, 16));
                        }

                        // Tiny chance to be pouncered when opening the door or
                        // approaching the door
                        if (Generator.Flip(0.02))
                            EventsOnOpenDoor.AddSpawnWave(
                                GenericWave.SinglePouncerShadow,
                                Generator.Between(4, 16));
                        else if (Generator.Flip(0.01))
                            EventsOnApproachDoor.AddSpawnWave(
                                Generator.Flip(0.7) ?
                                    GenericWave.SinglePouncerShadow :
                                    GenericWave.SinglePouncer,
                                Generator.Between(1, 6));

                        // Single enemies on wave start
                        if (Generator.Flip(0.05))
                            EventsOnDoorScanStart.AddSpawnWave(GenericWave.SingleTank, Generator.Between(5, 27));
                        else if (Generator.Flip(0.03))
                            EventsOnDoorScanStart.AddSpawnWave(GenericWave.SingleTankPotato, Generator.Between(5, 27));
                        break;
                    }
                }
            }

            if (Bins.ChainedPuzzles.Contains(puzzle))
                Plugin.Logger.LogInfo($"Zone {LocalIndex} alarm reassigned: {puzzle}");

            Alarm = ChainedPuzzle.FindOrPersist(puzzle);
        }
        #endregion

        #region Fog
        /// <summary>
        /// Set's the zone to be out of fog (based on the level settings).
        /// </summary>
        /// <param name="level"></param>
        public void SetOutOfFog(Level level)
        {
            Height? altitude = Generator.Pick(level.FogSettings.NoFogLevels);

            if (altitude != null)
            {
                Altitude = new Altitude { AllowedZoneAltitude = (Height)altitude };
                InFog = false;
            }
        }

        /// <summary>
        /// Set's the zone to be in fog (based on the level settings).
        /// </summary>
        /// <param name="level"></param>
        public void SetInFog(Level level)
        {
            Height? altitude = Generator.Pick(level.FogSettings.FogLevels);

            if (altitude != null)
            {
                Altitude = new Altitude { AllowedZoneAltitude = (Height)altitude };
                InFog = true;

                // Always add fog repellers into the fog zone
                ConsumableDistributionInZone = ConsumableDistribution.Baseline_FogRepellers.PersistentId;
            }
        }

        /// <summary>
        /// Checks the level settings for fog and optionally sets the altitude
        /// </summary>
        public void RollFog(Level level)
        {
            // If we don't have fog enabled then return early.
            if (level.Settings.Modifiers.Contains(LevelModifiers.NoFog))
                return;

            double chance;

            if (level.Settings.Modifiers.Contains(LevelModifiers.Fog))
                chance = 0.5;
            else if (level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
                chance = 0.75;
            else
                return;

            if (Generator.Flip(chance))
                SetInFog(level);
            else
                SetOutOfFog(level);
        }
        #endregion

        #region Clear Time Calculation
        // Clear time is the time taken to clear and get through the zone.
        // Most components have a scale factor associated with them to allow fine-tuning of time
        // grants to give as accurate an estimate as possible.
        //
        // Time estimates are supposed to be as accurate as possible for a team to quickly and
        // efficiently clear a zone.

        private const double clearTimeFactor_AlarmsTime     = 1.20; // Time factor for duration of each scan
        private const double clearTimeFactor_AlarmsTraverse = 1.20; // Distance factor for walking between scans
        private const double clearTimeFactor_AreaCoverage   = 1.30; // Map area factor for size of zone
        private const double clearTimeFactor_EnemyPoints    = 2.40; // Points factor for each enemy point
        private const double clearTimeFactor_Bosses         = 1.00; // Per boss factor

        /// <summary>
        /// Estimates the time (in seconds) to clear a zone based on it's Area coverage.
        /// </summary>
        /// <returns></returns>
        public double ClearTime_AreaCoverage()
            => Coverage.Max * clearTimeFactor_AreaCoverage;

        /// <summary>
        /// Time based on door alarms
        /// We sum for the component durations, the distance from start pos, and the distance
        /// between the alarm components
        /// </summary>
        /// <returns></returns>
        public double ClearTime_Alarm()
            => 10.0 + Alarm.Puzzle.Sum(component => component.Duration)
                        * clearTimeFactor_AlarmsTime
                    + Alarm.WantedDistanceFromStartPos
                        * clearTimeFactor_AlarmsTraverse
                    + (Alarm.Puzzle.Count - 1)
                        * Alarm.WantedDistanceBetweenPuzzleComponents
                        * clearTimeFactor_AlarmsTraverse;

        /// <summary>
        /// Estimates the time (in seconds) to clear a zone only of the enemies marked as bosses.
        /// </summary>
        /// <returns></returns>
        public double ClearTime_Bosses()
            => EnemySpawningInZone
                .Where(spawn => spawn.Tags.Contains("boss"))
                .Sum(spawn =>
                    (Enemy)spawn.Difficulty switch
                    {
                        Enemy.Mother => 60.0 * (spawn.Points / 10.0),
                        Enemy.PMother => 75.0 * (spawn.Points / 10.0),
                        Enemy.Tank => 45.0 * (spawn.Points / 10.0),
                        Enemy.TankPotato => 30.0 * (spawn.Points / 10.0),

                        // Some unknown enemy, we won't add time for unknowns
                        _ => 0.0
                    })
                * clearTimeFactor_Bosses;

        /// <summary>
        /// TODO: this needs to be improved
        /// </summary>
        /// <returns></returns>
        public double ClearTime_BloodDoor()
            => BloodDoor != BloodDoor.None ? 20.0 : 0.0;

        /// <summary>
        /// Estimates the time (in seconds) to clear a zones hibernating enemies
        ///
        /// Note that this will add points based on bosses, but there's a separate extra
        /// factor for clearing bosses due to their points not being very high
        /// </summary>
        /// <returns></returns>
        public double ClearTime_Enemies()
            => EnemySpawningInZone.Sum(spawn => spawn.Points)
               * clearTimeFactor_EnemyPoints;

        /// <summary>
        /// Estimate the time it would take to clear this zone. This tries to give a close estimate
        /// for expedient clearing of the area. There are scale factors to adjust the times which
        /// is helpful for granting more/less time based on level tier. Knowing this value is very
        /// helpful for a number of objectives where players need to race against the clock to
        /// achieve some objective. This function lets us grant the players an amount of time that
        /// is difficult but still possible.
        /// </summary>
        /// <returns>Estimated time to clear the zone and any alarms to _enter_ the zone</returns>
        public double GetClearTimeEstimate()
        {
            const double factorAlarms = 1.20;
            const double factorBoss = 1.0;
            const double factorCoverage = 1.30;
            const double factorEnemyPoints = 2.40;

            // Add time based on the zone size
            var timeCoverage = Coverage.Max * factorCoverage;

            // Time based on enemy points in zones
            var timeEnemyPoints = EnemySpawningInZone
                .Sum(spawn => spawn.Points) * factorEnemyPoints;

            // Find and add extra time for bosses. These are generally quite hard to deal with.
            var timeBosses = EnemySpawningInZone
                .Where(spawn => spawn.Tags.Contains("boss"))
                .Sum(spawn =>
                    (Enemy)spawn.Difficulty switch
                    {
                        Enemy.Mother => 60.0 * (spawn.Points / 10.0),
                        Enemy.PMother => 75.0 * (spawn.Points / 10.0),
                        Enemy.Tank => 45.0 * (spawn.Points / 10.0),
                        Enemy.TankPotato => 30.0 * (spawn.Points / 10.0),

                        // Some unknown enemy, we won't add time for unknowns
                        _ => 0.0
                    });
            timeBosses *= factorBoss;

            // Time based on door alarms
            // We sum for the component durations, the distance from start pos, and the distance
            // between the alarm components
            var timeAlarms = 10.0 + Alarm.Puzzle.Sum(component => component.Duration)
                                + Alarm.WantedDistanceFromStartPos
                                + (Alarm.Puzzle.Count - 1) * Alarm.WantedDistanceBetweenPuzzleComponents;
            timeAlarms *= factorAlarms;

            // Give +20s for a blood door.
            // TODO: adjust based on spawns in the blood door.
            var timeBloodDoor = 0.0;
            var timeBloodDoorBoss = 0.0;

            if (BloodDoor != BloodDoor.None)
            {
                timeBloodDoor = 20.0;

                // We need to add extra time for mothers. BloodDoor_BossMother is 1x mother
                // TODO: I'd prefer to do a full enemy count like we do for the whole zone.
                if (BloodDoor.EnemyGroupInfrontOfDoor == (uint)VanillaEnemyGroup.BloodDoor_BossMother)
                    timeBloodDoorBoss += 60.0;
                if (BloodDoor.EnemyGroupInArea == (uint)VanillaEnemyGroup.BloodDoor_BossMother)
                    timeBloodDoorBoss += 60.0;
            }

            // Sum the values
            var total = timeCoverage
                        + timeEnemyPoints
                        + timeBosses
                        + timeAlarms
                        + timeBloodDoor
                        + timeBloodDoorBoss;

            Plugin.Logger.LogDebug($"Zone {LocalIndex} time budget: total={total}s -- "
                                   + $"alarms={timeAlarms}s coverage={timeCoverage}s "
                                   + $"enemies={timeEnemyPoints}s "
                                   + $"bosses={timeBosses}s "
                                   + $"blood_doors={timeBloodDoor}s "
                                   + $"blood_door_bosses={timeBloodDoorBoss}s");
            return total;
        }
        #endregion

        #region Internal plugin properties
        /// <summary>
        /// Flags whether the zone is in fog or not.
        /// </summary>
        [JsonIgnore]
        public bool InFog { get; set; } = false;

        /// <summary>
        /// Factor used to multiply the enemy spawning points in this zone.
        /// </summary>
        [JsonIgnore]
        public double EnemyPointsMultiplier { get; set; } = 1.0;
        #endregion

        #region Unused by us properties
        public int AliasOverride = -1;
        #endregion

        #region Seed properties
        public int SubSeed { get; set; } = 24;
        public int MarkerSubSeed { get; set; } = 3;
        public int LightsSubSeed { get; set; } = 1;
        #endregion

        /// <summary>
        /// Zone number offset from the level layout
        /// </summary>
        public int LocalIndex { get; set; } = 0;

        /// <summary>
        /// Which zone to build this zone from
        /// </summary>
        public int BuildFromLocalIndex { get; set; } = 0;

        #region Zone Prefix
        public bool OverrideAliasPrefix
        {
            get => AliasPrefix != string.Empty;
            private set { }
        }

        public string AliasPrefixOverride
        {
            get => AliasPrefix;
            private set { }
        }

        /// <summary>
        /// What Zone Alias prefix to use. Leave this empty for the default `ZONE` prefix
        /// </summary>
        [JsonIgnore]
        public string AliasPrefix { get; set; } = string.Empty;

        public string AliasPrefixShortOverride { get; set; } = "";
        #endregion

        /// <summary>
        /// Which tileset to use
        /// </summary>
        public SubComplex SubComplex { get; set; } = SubComplex.All;

        [JsonProperty("CoverageMinMax")]
        public CoverageMinMax Coverage { get; set; } = CoverageMinMax.Medium;

        /// <summary>
        /// Where in the source zone to make the entrance for this zone
        /// </summary>
        public ZoneEntranceBuildFrom StartPosition { get; set; } = ZoneEntranceBuildFrom.Random;

        /// <summary>
        /// What direction to try and place the door for this zone
        /// </summary>
        public ZoneBuildExpansion StartExpansion { get; set; } = ZoneBuildExpansion.Random;

        /// <summary>
        /// What direction to build this zone towards
        /// </summary>
        public ZoneExpansion ZoneExpansion { get; set; } = ZoneExpansion.Random;

        /// <summary>
        /// If we specify a custom geomorph it goes here
        /// </summary>
        public string? CustomGeomorph { get; set; } = null;

        public bool IgnoreRandomGeomorphRotation { get; set; } = false;

        /// <summary>
        /// Which Light to select
        /// </summary>
        public Lights.Light LightSettings { get; set; } = Lights.Light.AlmostWhite_1;

        /// <summary>
        /// What allowed altitiudes the level can have
        /// </summary>
        [JsonProperty("AltitudeData")]
        public Altitude Altitude { get; set; } = new();

        #region Events
        public List<WardenObjectiveEvent> EventsOnEnter { get; set; } = new();
        public List<WardenObjectiveEvent> EventsOnPortalWarp { get; set; } = new();
        public List<WardenObjectiveEvent> EventsOnApproachDoor { get; set; } = new();
        public List<WardenObjectiveEvent> EventsOnUnlockDoor { get; set; } = new();
        public List<WardenObjectiveEvent> EventsOnOpenDoor { get; set; } = new();
        public List<WardenObjectiveEvent> EventsOnDoorScanStart { get; set; } = new();
        public List<WardenObjectiveEvent> EventsOnDoorScanDone { get; set; } = new();
        #endregion

        #region Puzzle settings
        public ProgressionPuzzle ProgressionPuzzleToEnter { get; set; } = new();

        /// <summary>
        /// Reference to the alarm we are using for the chained puzzle
        /// </summary>
        [JsonIgnore]
        public ChainedPuzzle Alarm { get; set; } = ChainedPuzzle.None;

        /// <summary>
        /// Which security scan to use to enter
        /// </summary>
        public uint ChainedPuzzleToEnter
        {
            get => Alarm.PersistentId;
            private set { }
        }
        #endregion

        #region Security door
        public bool IsCheckpointDoor { get; set; } = false;

        public bool PlayScannerVoiceAudio { get; set; } = false;

        public bool SkipAutomaticProgressionObjective { get; set; } = false;

        public SecurityGate SecurityGateToEnter { get; set; } = SecurityGate.Security;

        public bool UseStaticBioscanPointsInZone { get; set; } = false;
        #endregion

        #region Error alarms
        /// <summary>
        /// Whether the alarm (typically an ://ERROR! Alarm) of this zone is turned off on a terminal.
        /// </summary>
        public bool TurnOffAlarmOnTerminal { get; set; } = false;

        /// <summary>
        /// Terminal data used when TurnOffAlarmOnTerminal is set to true.
        /// </summary>
        public TerminalZoneSelection TerminalPuzzleZone { get; set; } = new TerminalZoneSelection();

        /// <summary>
        /// Events to trigger when disable alarm terminal command is called.
        /// </summary>
        public List<WardenObjectiveEvent> EventsOnTerminalDeactivateAlarm = new List<WardenObjectiveEvent>();
        #endregion

        #region Enemies
        /// <summary>
        ///
        /// </summary>
        [JsonProperty("ActiveEnemyWave")]
        public BloodDoor BloodDoor { get; set; } = BloodDoor.None;

        public List<EnemySpawningData> EnemySpawningInZone { get; set; } = new();
        #endregion

        #region Objective settings
        public int HSUClustersInZone { get; set; } = 0;

        /// <summary>
        /// Used for distribute cells as well as generator puzzles
        /// </summary>
        public List<FunctionPlacementData> PowerGeneratorPlacements { get; set; } = new List<FunctionPlacementData>();
        #endregion

        #region Respawn settings
        /// <summary>
        /// Whether the enemies respawn
        /// </summary>
        public bool EnemyRespawning { get; set; } = false;

        public bool EnemyRespawnRequireOtherZone { get; set; } = true;

        public int EnemyRespawnRoomDistance { get; set; } = 2;

        public double EnemyRespawnTimeInterval { get; set; } = 10.0;

        public double EnemyRespawnCountMultiplier { get; set; } = 1.0;

        public JArray EnemyRespawnExcludeList = new JArray();
        #endregion

        #region Static Spawns (Spitters, Mother Sacks etc)
        /// <summary>
        ///
        /// </summary>
        public List<StaticSpawnDataContainer> StaticSpawnDataContainers { get; set; } = new();
        #endregion

        public int CorpseClustersInZone { get; set; } = 0;
        public int ResourceContainerClustersInZone { get; set; } = 0;
        public int GeneratorClustersInZone { get; set; } = 0;
        public DistributionAmount CorpsesInZone { get; set; } = DistributionAmount.None;
        public DistributionAmount GroundSpawnersInZone { get; set; } = DistributionAmount.Some;
        public DistributionAmount HSUsInZone { get; set; } = DistributionAmount.None;
        public DistributionAmount DeconUnitsInZone { get; set; } = DistributionAmount.None;
        public bool AllowSmallPickupsAllocation { get; set; } = true;
        public bool AllowResourceContainerAllocation { get; set; } = true;
        public bool ForceBigPickupsAllocation { get; set; } = false;
        public uint BigPickupDistributionInZone { get; set; } = 0;

        public List<TerminalPlacement> TerminalPlacements { get; set; } = new();

        public bool ForbidTerminalsInZone { get; set; } = false;
        public JArray DisinfectionStationPlacements { get; set; } = new JArray();
        public JArray DumbwaiterPlacements { get; set; } = new JArray();

        #region Resources and items
        /// <summary>
        /// Consumables include glowsticks, c-foam grenades, other slot 5 consumable items.
        /// </summary>
        public uint ConsumableDistributionInZone { get; set; }
            = ConsumableDistribution.Baseline.PersistentId;

        /// <summary>
        /// How many health pack uses are in this zone. Default = 5
        /// </summary>
        [JsonProperty("HealthMulti")]
        public double HealthPacks { get; set; } = 5;

        public Placement HealthPlacement { get; set; } = new();

        /// <summary>
        /// How many ammo pack uses are in this zone. Default = 4
        /// </summary>
        [JsonProperty("WeaponAmmoMulti")]
        public double AmmoPacks { get; set; } = 4;

        public Placement WeaponAmmoPlacement { get; set; } = new();

        /// <summary>
        /// How many tool pack uses are in this zone. Default = 3.5
        /// </summary>
        [JsonProperty("ToolAmmoMulti")]
        public double ToolPacks { get; set; } = 3.5;

        public Placement ToolAmmoPlacement { get; set; } = new();

        /// <summary>
        /// How many disinfect pack uses are in this zone. Default = 0 (5 in base game)
        /// </summary>
        [JsonProperty("DisinfectionMulti")]
        public double DisinfectPacks { get; set; } = 0; // Default is 5 in base game

        public Placement DisinfectionPlacement { get; set; } = new();

        /// <summary>
        /// Takes an input function and applies it to each of the three major resource multiples.
        /// </summary>
        /// <param name="transformer"></param>
        public void SetMainResourceMulti(Func<double, double> transformer)
        {
            HealthPacks = transformer(HealthPacks);
            ToolPacks = transformer(ToolPacks);
            AmmoPacks = transformer(AmmoPacks);
        }
        #endregion

        public Zone()
        {
            // Always ensure a terminal is placed in the zone
            TerminalPlacements.Add(new TerminalPlacement());
        }
    }
}
