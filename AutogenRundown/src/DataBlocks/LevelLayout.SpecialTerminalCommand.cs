﻿using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout : DataBlock
{
    /// <summary>
    /// 
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

        // If this is true, then we have a very limited objective that needs to be completed _fast_.
        if (level.MainDirector.Objective == WardenObjectiveType.Survival &&
            director.Bulkhead.HasFlag(Bulkhead.Overload))
        {
            // Mark the first zone to be where the terminal will spawn
            //start.Branch = "find_items";
            planner.UpdateNode(start with { Branch = "find_items" });

            // Largeish zone size
            var zone = planner.GetZone(start)!;
            zone.Coverage = new() { Min = 32, Max = 64 };

            return;
        }

        // Normal generation for this
        BuildBranch(start, director.ZoneCount, "find_items");
    }
}
