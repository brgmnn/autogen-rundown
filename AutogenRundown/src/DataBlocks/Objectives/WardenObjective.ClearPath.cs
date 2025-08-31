using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: ClearPath
 *
 *
 * Fairly straight forward objective, get to the end zone. Some additional enemies
 * at the end make this a more interesting experience.
 *
 * This objective can only be for Main given it ends the level on completion
 */
public partial record WardenObjective
{
    public void Build_ClearPath(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        // Find the exit zone
        var exit = level.Planner.GetZonesByTag(director.Bulkhead, "exit_elevator").First();
        var exitZoneNumber = layout.ZoneAliasStart + exit.ZoneNumber;

        MainObjective = new Text(() => $"Clear a path to the exit point in {Lore.Zone(layout.ZoneAliasStart + exit.ZoneNumber)}");
        GoToWinCondition_Elevator = "";
        GoToWinCondition_CustomGeo = $"Go to the forward exit point in {Lore.Zone(exitZoneNumber)}";

        dataLayer.ObjectiveData.WinCondition = WardenObjectiveWinCondition.GoToElevator;
    }
}
