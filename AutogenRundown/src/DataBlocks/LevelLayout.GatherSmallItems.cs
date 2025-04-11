using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="_"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_GatherSmallItems(BuildDirector director, WardenObjective _, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;

        AddBranch(start, director.ZoneCount, "find_items");
    }
}
