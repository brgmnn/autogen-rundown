﻿using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Light;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    public partial record class LevelLayout : DataBlock
    {
        #region Generator Powered Door
        /// <summary>
        ///
        /// </summary>
        /// <param name="lockedNode">
        ///     The ZoneNode who's entrance is to be locked behind the power generator
        /// </param>
        /// <param name="cellNode">The ZoneNode to place the power cell</param>
        public void AddGeneratorPuzzle(ZoneNode lockedNode, ZoneNode cellNode)
        {
            var lockedZone = level.Planner.GetZone(lockedNode);
            var cellZone = level.Planner.GetZone(cellNode);

            if (lockedZone == null || cellZone == null)
            {
                Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to missing zones: locked={lockedZone} cell={cellZone}");
                return;
            }

            // Place the key for the locked zone
            lockedZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
            {
                PuzzleType = ProgressionPuzzleType.Generator,
                PlacementCount = 0,
            };

            cellZone.ForceBigPickupsAllocation = true;
            // TODO: Change this so we can dynamically set distributions
            // For instance adding additional cells without needing to know what they are
            cellZone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;
            cellZone.LightSettings = Lights.Light.Pitch_black_1;

            var powerGenerator = new IndividualPowerGenerator()
            {
                Bulkhead = cellNode.Bulkhead,
                ZoneNumber = cellNode.ZoneNumber,
                EventsOnInsertCell = new()
                {
                    new WardenObjectiveEvent()
                    {
                        Type = WardenObjectiveEventType.SetLightDataInZone,
                        Trigger = WardenObjectiveEventTrigger.OnStart,
                        LocalIndex = cellZone.LocalIndex,
                        Layer = 0,
                        Delay = 2.5,
                        Duration = 0.1,
                        SetZoneLight = new()
                        {
                            LightSettings = LightSettings.AuxiliaryPower,
                            Duration = 0.1,
                            Seed = 1,
                        }
                    }
                }
            };
            powerGenerator.EventsOnInsertCell.AddSound(Sound.LightsOn_Vol1, 2.0);

            level.EOS_IndividualGenerator.Definitions.Add(powerGenerator);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lockedNode"></param>
        /// <param name="searchBranch"></param>
        /// <param name="generatorBranchLength"></param>
        public void AddGeneratorPuzzle(
            ZoneNode lockedNode,
            string? searchBranch = null,
            int generatorBranchLength = -1)
        {}

        #endregion

        #region Keyed Puzzles
        /// <summary>
        /// Adds a key puzzle to enter the zone we have selected
        ///
        /// Generally this should be added manually depending on the objective.
        /// </summary>
        public void AddKeyedPuzzle(
            ZoneNode lockedNode,
            string? searchBranch = null,
            int keyBranchLength = -1)
        {
            var lockedZone = level.Planner.GetZone(lockedNode);

            if (lockedZone == null)
                return;

            searchBranch ??= lockedNode.Branch;

            var openZones = level.Planner
                .GetOpenZones(director.Bulkhead, searchBranch)
                .Where(node => node.ZoneNumber < lockedNode.ZoneNumber).ToList();
            var keyZoneNumber = 0;

            if (openZones.Any())
            {
                // We are able to construct a branch to store the key

                // Determin the key branch length
                var branchLength = keyBranchLength > 0 ? keyBranchLength : director.Tier switch
                {
                    "A" => 1,
                    "B" => 1,
                    "C" => 1,
                    "D" => Generator.Select(new List<(double, int)>
                    {
                        (0.75, 1),
                        (0.25, 2)
                    }),
                    "E" => Generator.Select(new List<(double, int)>
                    {
                        (0.60, 1),
                        (0.35, 2),
                        (0.05, 3)
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
                    new() { LocalIndex = keyZoneNumber }
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
        #endregion
    }
}
