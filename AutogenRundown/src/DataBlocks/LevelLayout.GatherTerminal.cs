using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    private void BuildLayout_GatherTerminal(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
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
