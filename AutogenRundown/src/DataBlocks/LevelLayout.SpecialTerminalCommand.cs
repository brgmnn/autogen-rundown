using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AutogenRundown.TerminalPlacements;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Adds a special terminal command layout
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="start"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_SpecialTerminalCommand(
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

        // We have a special flow for KingOfTheHill
        if (objective.SpecialTerminalCommand_Type == SpecialCommand.KingOfTheHill)
        {
            // var hill = AddBranch_Forward(start, 2, "special_terminal").Last();
            var hill = AddBranch(start, 4, "special_terminal", (node, zone) =>
            {
                // var direction = level.Settings.GetDirections(director.Bulkhead).Forward;
                // zone.ZoneExpansion = direction;
                zone.ZoneExpansion = ZoneExpansion.Backward;
                zone.SetStartExpansionFromExpansion();
            }).Last();
            var hillZone = planner.GetZone(hill)!;

            hillZone.GenKingOfTheHillGeomorph(level, director);
            hillZone.TerminalPlacements = new List<TerminalPlacement>
            {
                new() { PlacementWeights = ZonePlacementWeights.AtEnd }
            };

            hillZone.Altitude = new Altitude { AllowedZoneAltitude = Height.OnlyHigh };

            // level.TerminalPlacements.Placements.Add(new TerminalPosition
            // {
            //     Layer = director.Bulkhead switch
            //     {
            //         Bulkhead.Main => "MainLayer",
            //         Bulkhead.Extreme => "SecondaryLayer",
            //         Bulkhead.Overload => "ThirdLayer",
            //     },
            //     LocalIndex = hill.ZoneNumber,
            //     Geomorph = hillZone.CustomGeomorph,
            //     Position = new Vector3 { X = 0, Y = 0, Z = 0 },
            //     Rotation = new Vector3 { X = 0, Y = 180, Z = 0 },
            // });

            var spawnZoneCount = 3;

            // Build the spawn zones
            // Uses a customized version of build branch because we want more control over creating the zone
            for (var num = 0; num < spawnZoneCount; num++)
            {
                const string branch = "hill_spawn";
                var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                var node = new ZoneNode(director.Bulkhead, zoneIndex, branch, 0);
                node.Tags.Add("no_enemies");

                var zone = new Zone(level.Tier)
                {
                    LightSettings = Lights.GenRandomLight(),
                };
                zone.RollFog(level);
                zone.GenDeadEndGeomorph(director.Complex);

                // No terminals needed in the spawn zones
                zone.TerminalPlacements = new List<TerminalPlacement>();

                // No alarm needed on the door and have it be locked
                zone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
                zone.Alarm = ChainedPuzzle.SkipZone;

                // Add event to open the door on activation
                objective.EventsOnActivate.AddOpenDoor(director.Bulkhead, node.ZoneNumber);

                level.Planner.Connect(hill, node);
                level.Planner.AddZone(node, zone);
            }

            return;
        }

        // If this is true, then we have a very limited objective that needs to be completed _fast_.
        if (level.MainDirector.Objective == WardenObjectiveType.Survival &&
            director.Bulkhead.HasFlag(Bulkhead.Overload))
        {
            // Mark the first zone to be where the terminal will spawn
            planner.UpdateNode(start with { Branch = "special_terminal" });

            // Largeish zone size
            var zone = planner.GetZone(start)!;
            zone.Coverage = new() { Min = 32, Max = 64 };

            return;
        }

        // Normal generation for this
        var terminal = BuildBranch(start, director.ZoneCount, "special_terminal");

        // 55% chance to attempt to lock the end zone with a key puzzle
        if (Generator.Flip(0.55))
            AddKeyedPuzzle(terminal, "special_terminal", director.Bulkhead == Bulkhead.Main ? 2 : 1);

        planner.UpdateNode(terminal with { Tags = terminal.Tags.Extend("bulkhead_candidate") });
    }
}
