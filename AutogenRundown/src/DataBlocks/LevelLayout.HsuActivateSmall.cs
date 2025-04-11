using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    public void BuildLayout_HsuActivateSmall(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        var nodes = AddBranch_Forward(start, 2);

        var (end, endZone) = AddZone(nodes.Last(), new ZoneNode { Branch = "activate_item" });


        // Set up the end zone
        switch (level.Complex)
        {
            case Complex.Mining:
            {
                // Works with:
                //  - Data Sphere
                endZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_dead_end_HA_01.prefab";
                break;
            }

            case Complex.Tech:
            {
                // transformZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_Lab_dead_end_room_02_R8C2.prefab"; // Doesn't work?
                // transformZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_Lab_dead_end_room_02.prefab"; // Neonate I think, works with Data Sphere too but weird

                // Works with:
                //  - Data Sphere
                endZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_Lab_dead_end_room_01.prefab";
                break;
            }

            case Complex.Service:
            {
                // Works with:
                //  -
                endZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_I_01.prefab";
                break;
            }
        }

        layerData.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
        {
            new()
            {
                DimensionIndex = 0,
                LocalIndex = end.ZoneNumber
            }
        });
    }
}
