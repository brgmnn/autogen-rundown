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
        // var startZone = planner.GetZone(start)!;
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        AddBranch(start, objective.GatherTerminal_SpawnCount, "primary", (node, zone) =>
        {
            layerData.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
            {
                new()
                {
                    DimensionIndex = 0,
                    LocalIndex = node.ZoneNumber,
                    Weights = ZonePlacementWeights.EvenlyDistributed
                }
            });
        });
    }
}
