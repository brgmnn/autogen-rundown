using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Builds a reactor from the input zone node, and returns the reactor zone node.
    ///
    /// This will create a corridor connected to a reactor zone. Generally every reactor
    /// wants a corridor to reactor for fun wave defense.
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public ZoneNode BuildReactor(ZoneNode start)
    {
        var zoneExpansion = direction.Forward;
        var startExpansion = direction.Forward switch
        {
            ZoneExpansion.Forward  => ZoneBuildExpansion.Forward,
            ZoneExpansion.Left     => ZoneBuildExpansion.Left,
            ZoneExpansion.Right    => ZoneBuildExpansion.Right,
            ZoneExpansion.Backward => ZoneBuildExpansion.Backward,
            _ => ZoneBuildExpansion.Random
        };

        // Use the same light for both corridor and reactor
        var light = Lights.GenReactorLight();

        // Always generate a corridor of some kind (currently fixed) for the reactor zones.
        var corridorNode = planner.UpdateNode(start with { Branch = "reactor_entrance" });
        var corridor = planner.GetZone(corridorNode)!;

        corridor.LightSettings = light;
        corridor.StartExpansion = startExpansion;
        corridor.StartPosition = ZoneEntranceBuildFrom.Furthest;
        corridor.ZoneExpansion = zoneExpansion;

        corridor.GenReactorCorridorGeomorph(director.Complex);

        // Create the reactor zone
        var reactorNode = new ZoneNode(
            director.Bulkhead,
            level.Planner.NextIndex(director.Bulkhead),
            "reactor");
        reactorNode.Tags.Add("reactor");

        var reactor = new Zone(level)
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

        // Add fog repellers in case of fog
        reactor.ConsumableDistributionInZone = ConsumableDistribution.Reactor_FogRepellers.PersistentId;

        level.Planner.Connect(corridorNode, reactorNode);
        level.Planner.AddZone(reactorNode, reactor);

        return reactorNode;
    }
}
