using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    public partial record class LevelLayout : DataBlock
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

            level.Planner.Connect(start, corridorNode);
            level.Planner.AddZone(corridorNode, corridor);

            // Create the reactor zone
            var reactorNode = new ZoneNode(director.Bulkhead, level.Planner.NextIndex(director.Bulkhead));
            var reactor = new Zone
            {
                LightSettings = light,
                StartPosition = ZoneEntranceBuildFrom.Furthest,
                StartExpansion = startExpansion,
                ZoneExpansion = zoneExpansion,
                ForbidTerminalsInZone = true
            };
            reactor.GenReactorGeomorph(director.Complex);
            reactor.TerminalPlacements = new List<TerminalPlacement>();

            level.Planner.Connect(corridorNode, reactorNode);
            level.Planner.AddZone(reactorNode, reactor);

            return reactorNode;
        }

        /// <summary>
        /// Creates a simple reactor startup layout. These startups have no code fetching and
        /// thus are simpler and "easier". Note that we can make them harder with the waves
        /// being spawned.
        /// </summary>
        /// <param name="director"></param>
        /// <param name="objective"></param>
        /// <param name="start"></param>
        public void BuildLayout_ReactorStartup_Simple(BuildDirector director, WardenObjective objective, ZoneNode start)
        {
            // Place some zones before the reactor
            var preludeCount = level.Tier switch
            {
                "A" => 0,
                "B" => 1,
                "C" => Generator.Random.Next(1, 2),
                "D" => Generator.Random.Next(1, 3),
                "E" => Generator.Random.Next(2, 4),
                _ => 2
            };

            var last = BuildBranch(start, preludeCount);
            var reactor = BuildReactor(last);
        }
    }
}
