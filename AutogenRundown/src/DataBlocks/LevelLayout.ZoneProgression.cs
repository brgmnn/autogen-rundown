using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    public partial record class LevelLayout : DataBlock
    {
        /// <summary>
        /// Adds a key puzzle to enter the zone we have selected
        /// </summary>
        public void AddKeyedPuzzle(ZoneNode lockedNode)
        {
            var lockedZone = level.Planner.GetZone(lockedNode);
            var openZones = level.Planner
                .GetOpenZones(director.Bulkhead, lockedNode.Branch)
                .Where(node => node.ZoneNumber < lockedNode.ZoneNumber).ToList();
            var keyZoneNumber = 0;

            if (openZones.Any())
            {
                // We are able to construct a branch to store the key

                // Determin the key branch length
                var branchLength = director.Tier switch
                {
                    "A" => 1,
                    "B" => 1,
                    "C" => Generator.Select(new List<(double, int)>
                    {
                        (0.75, 1),
                        (0.25, 2)
                    }),
                    "D" => Generator.Select(new List<(double, int)>
                    {
                        (0.60, 1),
                        (0.37, 2),
                        (0.03, 3)
                    }),
                    "E" => Generator.Select(new List<(double, int)>
                    {
                        (0.50, 1),
                        (0.40, 2),
                        (0.10, 3)
                    }),

                    _ => 1
                };

                var branchBase = Generator.Pick(openZones);
                var branchIndex = 1;

                // Find a valid branch name
                while (level.Planner.GetZones(Bulkhead.All, $"key_{branchIndex}").Any())
                    branchIndex++;

                var branch = $"key_{branchIndex}";
                var last = BuildBranch(branchBase, branchLength, branch);

                var branchFirstNode = level.Planner.GetZones(director.Bulkhead, branch).First();
                var firstZone = level.Planner.GetZone(branchFirstNode);
                var branchBaseZone = level.Planner.GetZone(branchBase);

                // Try and set the first zone branch to actually pick a direction that can work
                firstZone.SetExpansionAsBranchOfZone(branchBaseZone,
                    Generator.Pick(new List<Direction>
                        { Direction.Left, Direction.Right }));

                keyZoneNumber = last.ZoneNumber;
            }
            else
            {
                // Bad luck, this key is a freebie and we just place in one of the preceeding zones
                // Pick one of the zones before the door
                keyZoneNumber = Generator.Pick(
                    level.Planner
                        .GetZones(director.Bulkhead, lockedNode.Branch)
                        .Where(node => node.ZoneNumber < lockedNode.ZoneNumber)
                    ).ZoneNumber;
            }

            // Place the key for the locked zone
            lockedZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
            {
                PuzzleType = ProgressionPuzzleType.Keycard,
                ZonePlacementData = new List<ZonePlacementData>
                {
                    new ZonePlacementData
                    {
                        LocalIndex = keyZoneNumber
                    }
                }
            };
        }

        /// <summary>
        /// Attempts to add some keyed doors
        /// </summary>
        public void RollKeyedDoors()
        {
            // Chance not to add any keyed doors
            if (Generator.Flip(0.6))
                return;

            // How many locked zones to add
            var locked = director.Tier switch
            {
                "A" => 1,
                "B" => 1,
                "C" => 1,
                "D" => 1,
                "E" => 1,

                _ => 0,
            };

            for (int i=0; i<locked; i++)
            {
                var candidates = level.Planner.GetZones(director.Bulkhead)
                    .Where(node => node.ZoneNumber > 0);
                var lockedZoneNode = Generator.Pick(candidates);

                if (candidates.Any())
                    AddKeyedPuzzle(lockedZoneNode);
            }
        }
    }
}
