using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Builds an uplink terminal objective layout
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="start"></param>
    public void BuildLayout_TerminalUplink(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? start)
    {
        if (start == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        switch (objective.Uplink_NumberOfTerminals)
        {
            case 1:
            {
                var last = BuildBranch((ZoneNode)start,
                    GenNumZones(director.Tier, director.Bulkhead, SizeFactor.Large));
                BuildBranch(last, 1, "uplink_terminals");

                break;
            }

            case 2:
            {
                var last = BuildBranch((ZoneNode)start,
                    GenNumZones(director.Tier, director.Bulkhead, SizeFactor.Medium));
                BuildBranch(last, 2, "uplink_terminals");
                break;
            }

            case 3:
            {
                var last = BuildBranch((ZoneNode)start,
                    GenNumZones(director.Tier, director.Bulkhead, SizeFactor.Small));
                BuildBranch(last, 3, "uplink_terminals");
                break;
            }

            case 4:
            {
                var last = BuildBranch((ZoneNode)start,
                    GenNumZones(director.Tier, director.Bulkhead, SizeFactor.Small));
                BuildBranch(last, 4, "uplink_terminals");
                break;
            }

            default:
            {
                var last = BuildBranch((ZoneNode)start,
                    GenNumZones(director.Tier, director.Bulkhead, SizeFactor.Large));
                BuildBranch(last, 1, "uplink_terminals");
                break;
            }
        }
    }
}
