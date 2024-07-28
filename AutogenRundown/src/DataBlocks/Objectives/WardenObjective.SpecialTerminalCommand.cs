using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: PowerCellDistribution
 *
 *
 * Prisoners have to go and enter a command on a terminal somewhere in the zone. The objective
 * itself is quite simple, but the real fun will come from what the effects are of the terminal
 * command. Base game has lights off as an effect.
 *
 ***************************************************************************************************
 *      TODO List
 *
 * TODO: It would be nice to add special commands other than just lights off that do other modifiers.
 *       Such as fog, error alarm, etc.
 *
 *       Ideas:
 *          1. Spawn boss
 *          2. Flood with fog
 *              a. Flood with fog slowly
 *              b. Instantly flood
 *          3. Trigger error alarm
 *          4. Trigger unit wave
 */
public partial record class WardenObjective : DataBlock
{
    public string SpecialTerminalCommand_GetCommand()
        => SpecialTerminalCommand_Type switch
        {
            SpecialCommand.LightsOff => "REROUTE_POWER",

            SpecialCommand.FillWithFog => Generator.Pick(new List<string>
                {
                    "FLUSH_VENTS"
                })!,

            _ => "UNKNOWN_COMMAND"
        };

    public void PreBuild_SpecialTerminalCommand(BuildDirector director, Level level)
    {
        SpecialTerminalCommand_Type = director.Tier switch
        {
            _ => Generator.Select(new List<(double, SpecialCommand)>
                {
                    (1.0, SpecialCommand.LightsOff),
                    (1.0, SpecialCommand.FillWithFog)
                })
        };
    }

    public void Build_SpecialTerminalCommand(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = "Find Computer terminal [ITEM_SERIAL] and input the backdoor command [SPECIAL_COMMAND]";
        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = "Navigate to [ITEM_ZONE] and find [ITEM_SERIAL]";
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find [ITEM_SERIAL] somewhere inside [ITEM_ZONE]";
        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ITEM_SERIAL]";
        SolveItem = "Proceed to input the backdoor command [SPECIAL_COMMAND] in [ITEM_SERIAL]";

        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        // Place the terminal in the last zone
        var node = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "find_items")!;
        var zoneIndex = node.ZoneNumber;

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>()
            {
                new ZonePlacementData
                {
                    LocalIndex = zoneIndex,
                    Weights = ZonePlacementWeights.NotAtStart
                }
            });

        /**
         * Base game commands:
         *  - REROUTE_POWER
         *  - OVERRIDE_LOCKDOWN
         *  - DISABLE_LIFE_SUPPORT
         *  - FLOODGATE_OPEN_S156F
         *  - SCCS_LOCAL_TEMP_SET_294_GRAD_INV_FALSE
         *  - DEACTIVATE_FILTERS_S65T
         *  - SPFC_REACTOR_A42_LINE_7_TRUE
         *
         *  - RECYCLE_GROUP_7A42
         *  - DRAIN_COOLING_TANKS
         *  - LOWER_RODS
         */
        switch (SpecialTerminalCommand_Type)
        {
            ///
            /// This is quite an easy command. We just turn the lights off.
            ///
            case SpecialCommand.LightsOff:
            {
                SpecialTerminalCommand = "REROUTE_POWER";
                SpecialTerminalCommandDesc = "Reroute power coupling to sector that has been powered down.";
                EventBuilder.AddLightsOff(EventsOnActivate, 9.0);
                break;
            }

            ///
            /// Fairly annoying command to get. Fills the level with non-infectious fog.
            ///
            case SpecialCommand.FillWithFog:
            {
                SpecialTerminalCommand = "FLUSH_VENTS";
                SpecialTerminalCommandDesc = "Divert atmospheric control system to initiate environmental maintenance procedures.";

                // Add a fog turbine in the first zone.
                var firstNode = level.Planner.GetZones(director.Bulkhead, null).First()!;
                var firstZone = level.Planner.GetZone(firstNode)!;
                firstZone.BigPickupDistributionInZone = BigPickupDistribution.FogTurbine.PersistentId;

                EventBuilder.AddFillFog(EventsOnActivate, 11.0);

                break;
            }
        }

        // Add scans
        ChainedPuzzleToActive = ChainedPuzzle.TeamScan.PersistentId;
        ChainedPuzzleAtExit = ChainedPuzzle.ExitAlarm.PersistentId;

        // Add exit wave if this is the main bulkhead
        if (director.Bulkhead.HasFlag(Bulkhead.Main))
            WavesOnGotoWin.Add(GenericWave.ExitTrickle); // TODO: not this, something else
    }
}
