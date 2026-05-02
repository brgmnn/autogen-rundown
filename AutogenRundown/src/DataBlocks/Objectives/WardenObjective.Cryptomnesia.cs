using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks.Objectives;

/// <summary>
/// Objective: Cryptomnesia
///
/// Modeled after R6D4. Gather tampered data cubes across multiple dimensions.
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
                Weights = ZonePlacementWeights.NotAtStart
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

        var forwardExtract = level.Planner
            .GetZones(branch: "forward_extract", dimension: DimensionIndex.Reality)
            .First();

        // Final warp back to Reality
        EventsOnActivate
            .AddDimensionWarp(DimensionIndex.Reality)
            .AddUnlockDoor(Bulkhead.Main, forwardExtract.ZoneNumber, delay: 1.0);

        OnActivateOnSolveItem = true;

        GatherMaxPerZone = (int)Math.Ceiling((double)GatherSpawnCount / Math.Max(1, placements.Count));

        Plugin.Logger.LogDebug($"Cryptomnesia: GatherMaxPerZone = {GatherMaxPerZone}");
        Plugin.Logger.LogDebug($"Cryptomnesia: Placements = [{Gather_PlacementNodes.Print()}]");

        Type = WardenObjectiveType.GatherSmallItems;

        AddCompletedObjectiveChallenge(level, director);
    }

    private void PostBuildIntel_Cryptomnesia(Level level)
    {
        var realityTheme = Cryptomnesia_SelectedThemes.FirstOrDefault();
        var themePool = ThemeIntelPool(realityTheme);

        // 70% theme-aware, 30% neutral — the elevator drop usually telegraphs the
        // Reality theme but Cryptomnesia's loop / cube / cognition motifs still
        // surface for variety.
        var pool = Generator.Flip(0.7) ? themePool : CryptomnesiaIntel_Neutral;

        level.ElevatorDropWardenIntel.Add(
            (Generator.Between(1, 10), Generator.Pick(pool)!));
    }

    private static List<string> ThemeIntelPool(CryptomnesiaTheme theme) => theme switch
    {
        CryptomnesiaTheme.ErrorAlarm   => CryptomnesiaIntel_ErrorAlarm,
        CryptomnesiaTheme.Giants       => CryptomnesiaIntel_Giants,
        CryptomnesiaTheme.Chargers     => CryptomnesiaIntel_Chargers,
        CryptomnesiaTheme.InfectionFog => CryptomnesiaIntel_InfectionFog,
        CryptomnesiaTheme.Shadows      => CryptomnesiaIntel_Shadows,
        CryptomnesiaTheme.Nightmares   => CryptomnesiaIntel_Nightmares,
        _                              => CryptomnesiaIntel_Neutral,
    };
}
