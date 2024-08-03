using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout : DataBlock
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_ReactorShutdown(
            BuildDirector director,
            WardenObjective objective,
            ZoneNode? startish)
    {
        // There's a problem if we have no start zone
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;

        // Create some initial zones
        //var prev = level.Planner.GetExactZones(director.Bulkhead).First();
        var prev = start;

        // Don't create quite all the initial zones
        var preludeZoneCount = Generator.Random.Next(0, director.ZoneCount);

        for (int i = 0; i < preludeZoneCount; i++)
        {
            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
            var next = new ZoneNode(director.Bulkhead, zoneIndex);

            level.Planner.Connect(prev, next);
            level.Planner.AddZone(
                next,
                new Zone
                {
                    Coverage = CoverageMinMax.GenNormalSize(),
                    LightSettings = Lights.GenRandomLight(),
                });

            prev = next;
        }

        // Pick a random direction to expand the reactor
        var (startExpansion, zoneExpansion) = Generator.Pick(
            new List<(ZoneBuildExpansion, ZoneExpansion)>
            {
                (ZoneBuildExpansion.Left, ZoneExpansion.Left),
                (ZoneBuildExpansion.Right, ZoneExpansion.Right),
                (ZoneBuildExpansion.Forward, ZoneExpansion.Forward),
                (ZoneBuildExpansion.Backward, ZoneExpansion.Backward)
            });
        // Use the same light for both corridor and reactor
        var light = Lights.GenReactorLight();

        // Always generate a corridor of some kind (currently fixed) for the reactor zones.
        var corridorNode = new ZoneNode(director.Bulkhead, level.Planner.NextIndex(director.Bulkhead));
        var corridor = new Zone
        {
            LightSettings = light,
            StartPosition = ZoneEntranceBuildFrom.Furthest,
            StartExpansion = startExpansion,
            ZoneExpansion = zoneExpansion
        };
        corridor.GenReactorCorridorGeomorph(director.Complex);

        level.Planner.Connect(prev, corridorNode);
        level.Planner.AddZone(corridorNode, corridor);

        // Create the reactor zone
        var reactorNode = new ZoneNode(director.Bulkhead, level.Planner.NextIndex(director.Bulkhead));
        var reactor = new Zone
        {
            LightSettings = light,
            StartPosition = ZoneEntranceBuildFrom.Furthest,
            StartExpansion = startExpansion,
            ZoneExpansion = zoneExpansion,
            ForbidTerminalsInZone = true,
            AliasPrefix = "Reactor, ZONE"
        };
        reactor.GenReactorGeomorph(director.Complex);
        reactor.TerminalPlacements = new List<TerminalPlacement>();

        level.Planner.Connect(corridorNode, reactorNode);
        level.Planner.AddZone(reactorNode, reactor);
    }
}

