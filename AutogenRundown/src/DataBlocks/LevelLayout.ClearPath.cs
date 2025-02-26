using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    public void BuildLayout_ClearPath(BuildDirector director, WardenObjective objective, ZoneNode start)
    {
        var prev = start;

        AddAlignedBossFight(start);

        for (var i = 1; i < director.ZoneCount; i++)
        {
            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
            var next = new ZoneNode(director.Bulkhead, zoneIndex);
            var nextZone = new Zone
            {
                Coverage = CoverageMinMax.GenNormalSize(),
                LightSettings = Lights.GenRandomLight(),
            };

            nextZone.RollFog(level);

            // This means it is the last zone
            if (i == director.ZoneCount - 1)
                next.Tags.Add("exit_elevator");

            level.Planner.Connect(prev, next);
            level.Planner.AddZone(next, nextZone);

            prev = next;
        }

        var last = prev;
        var lastZone = level.Planner.GetZone(last)!;

        var secondLast = (ZoneNode)level.Planner.GetBuildFrom(last)!;
        var secondLastZone = level.Planner.GetZone(secondLast)!;

        var subcomplex = GenSubComplex(level.Complex);

        // Some adjustments to try and reduce the chance of the exit geo not
        // spawning due to being trapped by a small penultimate zone
        secondLastZone.ZoneExpansion = ZoneExpansion.Expansional;
        secondLastZone.Coverage = new CoverageMinMax { Min = 35, Max = 45 };
        lastZone.StartPosition = ZoneEntranceBuildFrom.Furthest;

        // The final zone is the extraction zone
        lastZone.Coverage = new CoverageMinMax { Min = 50, Max = 50 };
        lastZone.SubComplex = subcomplex;
        lastZone.GenExitGeomorph(level.Complex);
    }
}
