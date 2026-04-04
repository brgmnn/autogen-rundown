using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks.Objectives;

/// <summary>
/// Objective: Cryptomnesia
///
/// Modeled after R6D4. Gather tampered data cubes across multiple dimensions.
/// For now this is a basic GatherSmallItems variant with 1 item in Reality.
/// </summary>
public partial record WardenObjective
{
    public void PreBuild_Cryptomnesia(BuildDirector director, Level level)
    {
        director.DisableStartingArea = true;

        GatherRequiredCount = 4;
        GatherSpawnCount = 4;
    }

    public void Build_Cryptomnesia(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        GatherItemId = (uint)WardenObjectiveItem.DataCubeTampered;

        MainObjective = new Text("Gather [COUNT_REQUIRED] Tampered Data Cubes and bring them to the extraction point.");
        FindLocationInfo = new Text("Look for Tampered Data Cubes in the complex");
        FindLocationInfoHelp = new Text("Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]");

        var placements = Gather_PlacementNodes
            .Select(node => new ZonePlacementData()
            {
                LocalIndex = node.ZoneNumber,
                Weights = ZonePlacementWeights.EvenlyDistributed
            }).ToList();
        dataLayer.ObjectiveData.ZonePlacementDatas.Add(placements);

        GatherMaxPerZone = (int)Math.Ceiling((double)GatherSpawnCount / Math.Max(1, placements.Count));

        Plugin.Logger.LogDebug($"Cryptomnesia: GatherMaxPerZone = {GatherMaxPerZone}");
        Plugin.Logger.LogDebug($"Cryptomnesia: Placements = [{Gather_PlacementNodes.Print()}]");

        AddCompletedObjectiveChallenge(level, director);
    }

    private void PostBuildIntel_Cryptomnesia(Level level)
    {
        level.ElevatorDropWardenIntel.Add((0, Generator.Pick(new List<string>
        {
            ">... The cubes are scattered across dimensions!\r\n>... <size=200%><color=red>Stay together!</color></size>\r\n>... [static]",
            ">... Reality is shifting...\r\n>... <size=200%><color=red>Find the data cubes!</color></size>\r\n>... [alarm blaring]",
            ">... Dimensional anomalies detected!\r\n>... <size=200%><color=red>Retrieve the cubes!</color></size>\r\n>... [creatures shrieking]",
        })!));
    }
}
