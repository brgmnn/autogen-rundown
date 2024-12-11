using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    public partial record LevelLayout : DataBlock
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
            var corridorNode = start;
            corridorNode.Branch = "reactor_area";
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
                "reactor_area");
            reactorNode.Tags.Add("reactor");

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

            // Add fog repellers in case of fog
            reactor.ConsumableDistributionInZone = ConsumableDistribution.Reactor_FogRepellers.PersistentId;

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
        public void BuildLayout_ReactorStartup_Simple(
            BuildDirector director,
            WardenObjective objective,
            ZoneNode start)
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

        /// <summary>
        /// Creates a fetch codes reactor layout. These are a lot more complex and require
        /// branches with terminals to fetch codes from.
        /// </summary>
        public void BuildLayout_ReactorStartup_FetchCodes(
            BuildDirector director,
            WardenObjective objective,
            ZoneNode start)
        {
            var reactor = BuildReactor(start);

            var fetchCount = director.Tier switch
            {
                "A" => 1,
                "B" => 2,
                "C" => Generator.Random.Next(3, 4),
                "D" => Generator.Random.Next(3, 5),
                "E" => Generator.Random.Next(4, 6),
                _ => 1
            };
            var (branchMin, branchMax) = (director.Tier, fetchCount) switch
            {
                ("A", _) => (1, 1),
                ("B", _) => (1, 2),

                ("C", 4) => (1, 2),
                ("C", _) => (1, 3),

                ("D", 5) => (1, 2),
                ("D", 4) => (1, 3),
                ("D", _) => (2, 3),

                ("E", 6) => (1, 2),
                ("E", 5) => (1, 3),
                ("E", 4) => (2, 3),
                ("E", _) => (1, 3),

                (_, _) => (1, 1)
            };
            var openChance = director.Tier switch
            {
                "A" => 1.0,
                "B" => 0.5,
                "C" => 0.4,
                "D" => 0.3,
                "E" => 0.2,
                _ => 1.0
            };

            objective.ReactorStartup_FetchWaves = fetchCount;
            var fetchWaves = objective.ReactorWaves
                .TakeLast(fetchCount)
                .ToList();

            for (var b = 0; b < fetchCount; b++)
            {
                var branch = $"reactor_code_{b}";

                var baseNode = b < 3 ? reactor : (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, $"reactor_code_{b - 3}")!;

                var last = BuildBranch(baseNode, Generator.Random.Next(branchMin, branchMax), branch);
                var branchNodes = level.Planner.GetZones(director.Bulkhead, branch);

                var lastZone = level.Planner.GetZone(last)!;
                var firstZone = level.Planner.GetZone(branchNodes.First())!;

                // Set terminal zone size to be large. This is to help avoid soft locking the level
                // when placing terminals
                lastZone.Coverage = CoverageMinMax.Large;

                // Add some extra terminals for confusion. All at the back.
                lastZone.TerminalPlacements = new List<TerminalPlacement>();
                var terminalCount = Generator.Random.Next(2, 3);

                for (int i = 0; i < terminalCount; i++)
                    lastZone.TerminalPlacements.Add(
                        new TerminalPlacement
                        {
                            PlacementWeights = ZonePlacementWeights.NotAtStart
                        });

                // Lock the entrance zone
                firstZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                // Set this zone to have the code for the right fetch wave
                var wave = fetchWaves[b];
                wave.VerifyInOtherZone = true;
                wave.ZoneForVerification = last.ZoneNumber;

                // Add an event to open/unlock the door when the wave defense is over (OnMid trigger)
                if (Generator.Flip(openChance))
                {
                    // TODO: The Zone number appears to not work now (E-level)
                    EventBuilder.AddOpenDoor(
                        wave.Events,
                        director.Bulkhead,
                        firstZone.LocalIndex,
                        $"Door to [ZONE_{firstZone.LocalIndex}] opened by startup sequence",
                        WardenObjectiveEventTrigger.OnMid,
                        8.0);

                    // Do not add an alarm to this zone as the door will be opened for the players.
                    firstZone.Alarm = ChainedPuzzle.SkipZone;
                }
                else
                    EventBuilder.AddUnlockDoor(
                        wave.Events,
                        director.Bulkhead,
                        firstZone.LocalIndex,
                        $"Door to [ZONE_{firstZone.LocalIndex}] unlocked by startup sequence",
                        WardenObjectiveEventTrigger.OnMid,
                        8.0);
            }
        }
    }
}
