using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
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
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_exit_01.prefab";
                    SubComplex = SubComplex.All;
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
                        (SubComplex.Storage, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_VS_01.prefab", new CoverageMinMax { Min = 25, Max = 35 })
                    });
                    break;

                case Complex.Tech:
                    (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                    {
                        (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 60 }),
                        (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_destroyed_HA_02.prefab", new CoverageMinMax { Min = 15, Max = 30 }),
                        (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_hub_SF_01.prefab", new CoverageMinMax { Min = 35, Max = 55 }),
                        (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_hub_JG_01.prefab", new CoverageMinMax { Min = 25, Max = 40 }),
                        (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_node_transition_06_JG.prefab", new CoverageMinMax { Min = 32, Max = 45 }),

                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V2.prefab", new CoverageMinMax { Min = 25, Max = 40 }),
                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02.prefab", new CoverageMinMax { Min = 20, Max = 40 }),
                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_02_V2.prefab", new CoverageMinMax { Min = 30, Max = 45 }),
                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_03.prefab", new CoverageMinMax { Min = 30, Max = 50 }),
                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_04_V3.prefab", new CoverageMinMax { Min = 25, Max = 35 }),
                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_SF_02.prefab", new CoverageMinMax { Min = 30, Max = 45 }),
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
                                              //Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_02.prefab

                        (SubComplex.Gardens, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_X_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
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

                        // --- Custom Geo mods
                        // FlowGeos
                        (SubComplex.DataCenter, "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_I_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                    });
                    break;

                case Complex.Service:
                    (SubComplex, CustomGeomorph, Coverage) = Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
                    {
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_I_HA_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_I_HA_02.prefab", new CoverageMinMax { Min = 25, Max = 40 }),

                        (SubComplex.Gardens, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_I_01.prefab", new CoverageMinMax { Min = 20, Max = 25 }),
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
                    // reactor_open_HA_01 does not work for reactor shutdown
                    //               "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_reactor_open_HA_01.prefab"
                    //               "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_reactor_HA_02.prefab"
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_reactor_HA_02.prefab";
                    SubComplex = SubComplex.Refinery;
                    IgnoreRandomGeomorphRotation = true;
                    Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
                    break;
                }

                case Complex.Tech:
                {
                    //               "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_reactor_HA_01.prefab"
                    //               "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_reactor_HA_02.prefab"
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_reactor_HA_01.prefab";
                    SubComplex = SubComplex.Lab;
                    IgnoreRandomGeomorphRotation = true;
                    Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
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
        #endregion

        #region Enemies
        public class WeightedDifficulty : Generator.ISelectable
        {
            public double Weight { get; set; }

            public List<EnemyRoleDifficulty> Difficulties { get; set; } = new List<EnemyRoleDifficulty>();
        }

        /// <summary>
        /// Generate enemies for the zone
        /// </summary>
        /// <param name="director"></param>
        public void GenEnemies(BuildDirector director)
        {
            var points = director.GetPoints(this);

            switch (director.Tier)
            {
                // Easiest tier, we want the enemies to be relatively easy
                case "A":
                    {
                        var selected = Generator.Select(
                            new List<WeightedDifficulty>
                            {
                                new() { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy } },
                                new() { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy, EnemyRoleDifficulty.Medium } },
                            });

                        foreach (var difficulty in selected.Difficulties)
                        {
                            EnemySpawningInZone.Add(
                                new EnemySpawningData
                                {
                                    GroupType = EnemyGroupType.Hibernate,
                                    Difficulty = (uint)difficulty,
                                    Points = points / selected.Difficulties.Count,
                                });
                        }

                        break;
                    }

                case "B":
                    {
                        var selected = Generator.Select(
                            new List<WeightedDifficulty>
                            {
                                new() { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Easy } },
                                new() { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy, EnemyRoleDifficulty.Medium } },
                                new() { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy, EnemyRoleDifficulty.Hard } }
                            });

                        foreach (var difficulty in selected.Difficulties)
                        {
                            EnemySpawningInZone.Add(
                                new EnemySpawningData
                                {
                                    GroupType = EnemyGroupType.Hibernate,
                                    Difficulty = (uint)difficulty,
                                    Points = points / selected.Difficulties.Count,
                                });
                        }

                        break;
                    }

                // C-tier is the normal difficulty benchmark.
                case "C":
                    {
                        var selected = Generator.Select(
                            new List<WeightedDifficulty>
                            {
                                new() { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy, EnemyRoleDifficulty.Medium } },
                                new() { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Medium } },
                                new() { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Medium, EnemyRoleDifficulty.Hard } },
                                new() { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Hard } },
                            });

                        foreach (var difficulty in selected.Difficulties)
                        {
                            EnemySpawningInZone.Add(
                                new EnemySpawningData
                                {
                                    GroupType = EnemyGroupType.Hibernate,
                                    Difficulty = (uint)difficulty,
                                    Points = points / selected.Difficulties.Count,
                                });
                        }

                        break;
                    }

                default:
                    break;
            }


        }
        #endregion

        #region Alarms
        public void RollAlarms(
            ICollection<(double, int, ChainedPuzzle)> puzzlePack,
            ICollection<(double, int, WavePopulation)> wavePopulationPack,
            ICollection<(double, int, WaveSettings)> waveSettingsPack)
        {
            if (LocalIndex == 0 || Alarm == ChainedPuzzle.SkipZone)
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

                puzzle = puzzle with { Population = population, Settings = settings };
                Alarm = puzzle;
            }
            else
                Alarm = puzzle;

            Plugin.Logger.LogDebug($"Zone {LocalIndex} alarm: {puzzle}");

            EventsOnApproachDoor.AddRange(puzzle.EventsOnApproachDoor);
            EventsOnUnlockDoor.AddRange(puzzle.EventsOnUnlockDoor);
            EventsOnOpenDoor.AddRange(puzzle.EventsOnOpenDoor);
            EventsOnDoorScanStart.AddRange(puzzle.EventsOnDoorScanStart);
            EventsOnDoorScanDone.AddRange(puzzle.EventsOnDoorScanDone);

            puzzle.Persist();
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

        #region Time Calculation
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
            var factorAlarms = 1.20;
            var factorBoss = 1.0;
            var factorCoverage = 1.30;
            var factorEnemyPoints = 1.20;

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
        public bool OverrideAliasPrefix = false;
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
        public ProgressionPuzzle ProgressionPuzzleToEnter { get; set; } = new ProgressionPuzzle();

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

        public List<TerminalPlacement> TerminalPlacements { get; set; } = new List<TerminalPlacement>();

        public bool ForbidTerminalsInZone { get; set; } = false;
        public JArray DisinfectionStationPlacements { get; set; } = new JArray();
        public JArray DumbwaiterPlacements { get; set; } = new JArray();

        #region Resources and items
        /// <summary>
        /// Consumables include glowsticks, c-foam grenades, other slot 5 consumable items.
        /// </summary>
        public uint ConsumableDistributionInZone { get; set; }
            = ConsumableDistribution.Baseline.PersistentId;

        public double HealthMulti { get; set; } = 1.0;

        public Placement HealthPlacement { get; set; } = new Placement();

        public double WeaponAmmoMulti { get; set; } = 1.0;

        public Placement WeaponAmmoPlacement { get; set; } = new Placement();

        public double ToolAmmoMulti { get; set; } = 1.0;

        public Placement ToolAmmoPlacement { get; set; } = new Placement();

        public double DisinfectionMulti { get; set; } = 0.0;

        public Placement DisinfectionPlacement { get; set; } = new Placement();
        #endregion

        public JArray StaticSpawnDataContainers { get; set; } = new JArray();

        public Zone()
        {
            // Always ensure a terminal is placed in the zone
            TerminalPlacements.Add(new TerminalPlacement());
        }
    }
}
