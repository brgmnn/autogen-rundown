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

        level.Planner.GetZones(director.Bulkhead, null).ForEach(node =>
        {
            var zone = level.Planner.GetZone(node)!;

            switch (level.Tier)
            {
                case "A":
                {
                    zone.AmmoPacks *= 2.0;
                    zone.ToolPacks *= 2.0;
                    zone.HealthPacks *= 2.0;
                    break;
                }

                case "B":
                {
                    zone.AmmoPacks *= 1.6;
                    zone.ToolPacks *= 1.6;
                    zone.HealthPacks *= 1.6;
                    break;
                }

                case "C":
                {
                    // TODO: change all of these so we can assign actual numbers of ammo / health / tool use
                    zone.AmmoPacks *= 1.4;
                    zone.ToolPacks *= 1.4;
                    zone.HealthPacks *= 1.4;
                    break;
                }

                case "D":
                {
                    zone.AmmoPacks *= 1.2;
                    zone.ToolPacks *= 1.2;
                    zone.HealthPacks *= 1.2;
                    break;
                }

                case "E":
                {
                    // No extra resources in E
                    break;
                }
            }
        });
    }
}
