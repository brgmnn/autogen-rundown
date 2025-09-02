using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Builds a forward extract zone
    /// </summary>
    public void BuildLayout_ForwardExtract(double chance = 0.5)
    {
        var mainObjective = level.GetObjective(Bulkhead.Main);

        // Skip objectives that already include forward extracts
        if (director.Objective is WardenObjectiveType.ClearPath or WardenObjectiveType.Survival)
            return;

        Plugin.Logger.LogInfo($"We made it!");

        // // Random chance for us to skip doing this all together
        // if (Generator.Flip(1.0 - chance))
        //     return;

        var start = level.ForwardExtractStartCandidates.Any()
            ? Generator.Select(level.ForwardExtractStartCandidates)
            : planner.GetLeafZones(Bulkhead.Main).PickRandom();

        var nodes = AddBranch(start, 2, "forward_extraction");

        var exit = nodes.Last();
        var exitZone = planner.GetZone(exit);

        level.ExtractionZone = exit;

        exitZone.GenExitGeomorph(level.Complex);

        // TODO: we should just set the messages to be the same
    }
}
