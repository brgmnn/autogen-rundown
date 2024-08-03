using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout : DataBlock
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_ReactorShutdown(
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

        // Don't create quite all the initial zones
        var preludeZoneCount = Generator.Random.Next(0, director.ZoneCount);

        // Create some initial zones
        var prev = BuildBranch(start, preludeZoneCount, "primary");

        BuildReactor(prev);
    }
}

