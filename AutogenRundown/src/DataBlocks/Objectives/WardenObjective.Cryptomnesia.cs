using AutogenRundown.DataBlocks.Enums;
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
            .Select(node => new ZonePlacementData
            {
                Dimension = node.Dimension,
                LocalIndex = node.ZoneNumber,
                Weights = ZonePlacementWeights.EvenlyDistributed
            }).ToList();
        dataLayer.ObjectiveData.ZonePlacementDatas.Add(placements);

        // Build the event chain from per-dimension enter/exit events.
        // Order: [Reality exit] → warp(Dim1) → [Dim1 enter] → break → [Dim1 exit] → warp(Dim2) → ...
        // Reality's enter events go to EventsOnElevatorLand instead.

        // Reality enter events → elevator landing
        if (Cryptomnesia_EnterEvents.TryGetValue(DimensionIndex.Reality, out var realityEnter))
            EventsOnElevatorLand.AddRange(realityEnter);

        // Reality exit events start the activate chain
        if (Cryptomnesia_ExitEvents.TryGetValue(DimensionIndex.Reality, out var realityExit))
            foreach (var e in realityExit)
                EventsOnActivate.Add(e);

        // Each dimension: warp → enter → break → exit
        foreach (var zoneNode in Gather_PlacementNodes.TakeLast(GatherSpawnCount - 1))
        {
            var dim = zoneNode.Dimension;

            EventsOnActivate.AddDimensionWarp(dim);

            if (Cryptomnesia_EnterEvents.TryGetValue(dim, out var enterEvents))
                foreach (var e in enterEvents)
                    EventsOnActivate.Add(e);

            EventsOnActivate.AddEventBreak();

            if (Cryptomnesia_ExitEvents.TryGetValue(dim, out var exitEvents))
                foreach (var e in exitEvents)
                    EventsOnActivate.Add(e);
        }

        // Final warp back to Reality
        EventsOnActivate.AddDimensionWarp(DimensionIndex.Reality);

        OnActivateOnSolveItem = true;

        GatherMaxPerZone = (int)Math.Ceiling((double)GatherSpawnCount / Math.Max(1, placements.Count));

        Plugin.Logger.LogDebug($"Cryptomnesia: GatherMaxPerZone = {GatherMaxPerZone}");
        Plugin.Logger.LogDebug($"Cryptomnesia: Placements = [{Gather_PlacementNodes.Print()}]");

        Type = WardenObjectiveType.GatherSmallItems;

        AddCompletedObjectiveChallenge(level, director);
    }

    private void PostBuildIntel_Cryptomnesia(Level level)
    {
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Pick(new List<string>
        {
            // Vanilla intel
            ">... We're... back...\r\n>... This will go quick then!\r\n>... No, <size=200%><color=red>wait-</color></size>\r\n>...[screeching, gunfire]",

            // Generated intel
            ">... The cubes are scattered across dimensions!\r\n>... <size=200%><color=red>Stay together!</color></size>\r\n>... [static]",
            ">... Reality is shifting...\r\n>... <size=200%><color=red>Find the data cubes!</color></size>\r\n>... [alarm blaring]",
            ">... Dimensional anomalies detected!\r\n>... <size=200%><color=red>Retrieve the cubes!</color></size>\r\n>... [creatures shrieking]",
        })!));
    }
}
