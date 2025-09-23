using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Logs;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    private void BuildLayout_GatherTerminal_AlphaSix(
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

        var (portal, portalZone) = AddZone_Forward(start);

        portalZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_portal_HA_01.prefab";

        var (mwp, mwpZone) = BuildChallenge_Small(portal);

        mwpZone.GenMatterWaveProjectorGeomorph(level.Complex);
        mwpZone.BigPickupDistributionInZone = BigPickupDistribution.MatterWaveProjector.PersistentId;

        mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer);

        // portalZone.EventsOnPortalWarp.AddSpawnWave(
        //     new GenericWave
        //     {
        //         Population = WavePopulation.OnlyNightmareGiants,
        //         Settings = WaveSettings.Error_Hard
        //     },
        //     delay: 10.0);

        portalZone.EventsOnPortalWarp.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SpawnEnemyWave,
                Delay = 10.0,
                SoundId = Sound.Enemies_DistantLowRoar,
                EnemyWaveData = new GenericWave
                {
                    Population = WavePopulation.OnlyNightmareGiants,
                    Settings = WaveSettings.Error_Hard
                },
                Dimension = DimensionIndex.Dimension1
            });

        var dimension = new Dimension
        {
            Data = Dimensions.DimensionData.AlphaThree_Shaft with
            {
                StaticTerminalPlacements = new List<TerminalPlacement>
                {
                    new()
                    {
                        LogFiles = new List<LogFile>
                        {
                            DLockDecipherer.R1B1_Z40
                        }
                    }
                }
            }
        };
        dimension.FindOrPersist();

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
        BuildLayout_GatherTerminal_AlphaSix(director, objective, startish);

        return;

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
                        objective.PlacementNodes.Add(start);

                        var nodes = AddBranch_Forward(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),

                    // start -> Hub -> end1,end2
                    (0.75, () =>
                    {
                        startZone.GenCorridorGeomorph(level.Complex);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });

                        AddForwardExtractStart(hub);

                        if (Generator.Flip(0.4))
                            hubZone.GenTGeomorph(level.Complex);
                        else
                            hubZone.GenHubGeomorph(level.Complex);

                        SetGatherTerminal(hub.ZoneNumber);
                        objective.PlacementNodes.Add(hub);

                        var (end1, end1Zone) = AddZone(hub);
                        end1Zone.GenDeadEndGeomorph(level.Complex);

                        var (end2, end2Zone) = AddZone(hub);
                        end2Zone.GenDeadEndGeomorph(level.Complex);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                        objective.PlacementNodes.Add(end1);
                        objective.PlacementNodes.Add(end2);
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
                        objective.PlacementNodes.Add(hub);

                        var (end1, end1Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_1" });
                        end1Zone.GenDeadEndGeomorph(level.Complex);

                        var (end2, end2Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_2" });
                        end2Zone.GenDeadEndGeomorph(level.Complex);

                        var (end3, _) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_3" });
                        AddForwardExtractStart(end3);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                        SetGatherTerminal(end3.ZoneNumber);

                        objective.PlacementNodes.Add(end1);
                        objective.PlacementNodes.Add(end2);
                        objective.PlacementNodes.Add(end3);
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

                var nodes = AddBranch(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                {
                    SetGatherTerminal(node.ZoneNumber);
                    objective.PlacementNodes.Add(node);
                });

                AddForwardExtractStart(nodes.Last());
                break;
            }
        }

        startZone.TerminalPlacements.First().LogFiles.Add(new LogFile
        {
            FileName = $"DEC_KEY_INVENTORY-{Generator.ShortHexHash()}",
            FileContent = new Text(() =>
            {
                var zones = string.Join(
                    ", ",
                    objective.PlacementNodes.Select(node => Intel.Zone(node, planner, underscore: true)));

                // TODO: deal with how we would gather 5. It will be longer than 43 chars
                return $"-------------------------------------------\n" +
                       $"          Data redundancy system          \n\n" +
                       $"Backup decryption keys stored in mirror\n" +
                       $"terminal array. Terminal storage zones:\n\n" +
                       $"  {zones}\n\n" +
                       $"-------------------------------------------";
            })
        });
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
