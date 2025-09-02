using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    private void BuildLayout_GatherTerminal_WarpJump(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        var (portal, portalZone) = AddZone(start);

        portalZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_portal_HA_01.prefab";

        portalZone.BigPickupDistributionInZone = BigPickupDistribution.MatterWaveProjector.PersistentId;




        var dimension = new Dimension
        {
            Data = new Dimensions.DimensionData
            {
                // DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Static_01.prefab",
                DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_R6A2.prefab",
                DimensionResourceSetID = 47,
                IsOutside = true,
                IsStaticDimension = true,
                // StaticTerminalPlacements = []
            },
            PersistentId = 2,
        };
        // dimension.FindOrPersist();
        dimension.Persist();

        level.DimensionDatas.Add(new Levels.DimensionData
        {
            Dimension = DimensionIndex.Dimension1,
            Data = dimension
        });

        var dataLayer = level.GetObjectiveLayerData(director.Bulkhead);

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
        {
            new()
            {
                Dimension = DimensionIndex.Dimension1,
                LocalIndex = 0,
                Weights = ZonePlacementWeights.EvenlyDistributed
            }
        });
    }

    private void BuildLayout_GatherTerminal(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        // BuildLayout_GatherTerminal_WarpJump(director, objective, startish);
        //
        // return;

        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        switch (level.Tier, director.Bulkhead)
        {
            // These all have 3 spawn count
            case ("B", Bulkhead.Main):
            case ("D", Bulkhead.Overload):
            case ("E", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight line
                    (0.25, () =>
                    {
                        SetGatherTerminal(start.ZoneNumber);
                        AddBranch_Forward(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                        });
                    }),

                    // start -> Hub -> end1,end2
                    (0.75, () =>
                    {
                        startZone.GenCorridorGeomorph(level.Complex);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 2 });

                        if (Generator.Flip(0.4))
                            hubZone.GenTGeomorph(level.Complex);
                        else
                            hubZone.GenHubGeomorph(level.Complex);

                        SetGatherTerminal(hub.ZoneNumber);

                        var (end1, end1Zone) = AddZone(hub);
                        end1Zone.GenDeadEndGeomorph(level.Complex);

                        var (end2, end2Zone) = AddZone(hub);
                        end2Zone.GenDeadEndGeomorph(level.Complex);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                    }),
                });
                break;
            }

            // These all have 4 spawn count
            case ("C", Bulkhead.Main):
            case ("D", Bulkhead.Main):
            case ("E", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // start -> Hub -> end1,end2,end3
                    (1.0, () =>
                    {
                        startZone.GenCorridorGeomorph(level.Complex);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(level.Complex);

                        SetGatherTerminal(hub.ZoneNumber);

                        var (end1, end1Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_1" });
                        end1Zone.GenDeadEndGeomorph(level.Complex);

                        var (end2, end2Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_2" });
                        end2Zone.GenDeadEndGeomorph(level.Complex);

                        var (end3, end3Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_3" });
                        end3Zone.GenDeadEndGeomorph(level.Complex);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                        SetGatherTerminal(end3.ZoneNumber);
                    }),
                });
                break;
            }

            // TODO: set up 6 spawn
            // Spawns 6
            // case ("E", Bulkhead.Main):
            // {
            //     Generator.SelectRun(new List<(double, Action)>
            //     {
            //     });
            //     break;
            // }

            // Most of the smaller levels will use this default linear branch
            default:
            {
                SetGatherTerminal(start.ZoneNumber);
                AddBranch(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                {
                    SetGatherTerminal(node.ZoneNumber);
                });
                break;
            }
        }

        return;
    }

    // Helper function to wrap adding the zone placement data
    private void SetGatherTerminal(int zoneNumber)
    {
        var dataLayer = level.GetObjectiveLayerData(director.Bulkhead);

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
        {
            new()
            {
                LocalIndex = zoneNumber,
                Weights = ZonePlacementWeights.EvenlyDistributed
            }
        });
    }
}
