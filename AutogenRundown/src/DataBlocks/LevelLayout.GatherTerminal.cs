using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    public void BuildLayout_GatherTerminal(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        // Helper function to wrap adding the zone placement data
        void SetGatherTerminal(int zoneNumber)
        {
            layerData.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
            {
                new()
                {
                    DimensionIndex = 0,
                    LocalIndex = zoneNumber,
                    Weights = ZonePlacementWeights.EvenlyDistributed
                }
            });
        }

        switch (level.Tier, director.Bulkhead)
        {
            // case ("B", Bulkhead.Main):
            // {
            //     break;
            // }

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
    }
}
