using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: PowerCellDistribution
 *
 *
 * Prisoners have to go and enter a command on a terminal somewhere in the zone. The objective
 * itself is quite simple, but the real fun will come from what the effects are of the terminal
 * command. Base game has lights off as an effect.
 *
 */
public partial record WardenObjective
{
    /// <summary>
    /// We need to select the type of special terminal command that will be used here
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void PreBuild_ReachKdsDeep(BuildDirector director, Level level)
    {
        SpecialTerminalCommand_Type = director.Tier switch
        {
            "A" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.50, SpecialCommand.LightsOff),
                (0.07, SpecialCommand.FillWithFog),
                (0.43, SpecialCommand.KingOfTheHill)
            }),
            "B" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.30, SpecialCommand.LightsOff),
                (0.10, SpecialCommand.FillWithFog),
                (0.10, SpecialCommand.ErrorAlarm),
                (0.50, SpecialCommand.KingOfTheHill)
            }),
            "C" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.30, SpecialCommand.LightsOff),
                (0.10, SpecialCommand.FillWithFog),
                (0.15, SpecialCommand.ErrorAlarm),
                (0.45, SpecialCommand.KingOfTheHill)
            }),
            "D" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.25, SpecialCommand.LightsOff),
                (0.15, SpecialCommand.FillWithFog),
                (0.20, SpecialCommand.ErrorAlarm),
                (0.40, SpecialCommand.KingOfTheHill)
            }),
            "E" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.05, SpecialCommand.LightsOff),
                (0.15, SpecialCommand.FillWithFog),
                (0.30, SpecialCommand.ErrorAlarm),
                (0.50, SpecialCommand.KingOfTheHill)
            }),
        };
    }

    public void Build_ReachKdsDeep(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Find Computer terminal [ITEM_SERIAL] and input the backdoor command [SPECIAL_COMMAND]");
        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = new Text("Navigate to [ITEM_ZONE] and find [ITEM_SERIAL]");
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find [ITEM_SERIAL] somewhere inside [ITEM_ZONE]";
        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ITEM_SERIAL]";
        SolveItem = "Proceed to input the backdoor command [SPECIAL_COMMAND] in [ITEM_SERIAL]";

        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        dataLayer.ObjectiveData.WinCondition = WardenObjectiveWinCondition.GoToExitGeo;

        // Set type to empty
        Type = WardenObjectiveType.Empty;
    }
}
