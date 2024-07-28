using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout : DataBlock
{
    public void BuildLayout_TimedTerminalSequence(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode start)
    {
        // Set entrance zone to corridor
        var entranceZone = level.Planner.GetZone(start)!;
        entranceZone.GenCorridorGeomorph(director.Complex);
        entranceZone.RollFog(level);
        start.MaxConnections = 1;
        level.Planner.UpdateNode(start);

        // Create hub zone
        var hubIndex = level.Planner.NextIndex(director.Bulkhead);
        var hub = new ZoneNode(director.Bulkhead, hubIndex);
        hub.MaxConnections = 3;

        var zone = new Zone { LightSettings = Lights.GenRandomLight() };
        zone.GenHubGeomorph(director.Complex);
        zone.RollFog(level);

        level.Planner.Connect(start, hub);
        level.Planner.AddZone(hub, zone);

        // Assign objective data
    }
}
