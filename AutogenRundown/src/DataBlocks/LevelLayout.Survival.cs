using AutogenRundown.DataBlocks.Alarms;
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
    public void BuildLayout_Survival(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? start)
    {
        if (start == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var prev = BuildBranch((ZoneNode)start, director.ZoneCount, "survival_arena");

        // We need an exit zone as prisoners have to run to the exit
        var exitIndex = level.Planner.NextIndex(director.Bulkhead);
        var exitNode = new ZoneNode(director.Bulkhead, exitIndex, "exit");
        var exitZone = new Zone
        {
            Coverage = CoverageMinMax.Tiny,
            LightSettings = Lights.GenRandomLight()
        };

        exitZone.GenExitGeomorph(director.Complex);

        // Exit scan will be HARD
        exitZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
        exitZone.Alarm = ChainedPuzzle.SkipZone;

        level.Planner.Connect(prev, exitNode);
        level.Planner.AddZone(exitNode, exitZone);
    }
}
