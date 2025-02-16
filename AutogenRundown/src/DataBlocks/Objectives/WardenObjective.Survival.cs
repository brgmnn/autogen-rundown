using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: Survival
 *
 *
 * Fairly straight forward objective, get to the end zone. Some additional enemies
 * at the end make this a more interesting experience.
 *
 * This objective can only be for Main given it ends the level on completion
 *
 * TODO:
 *
 *  - Limit bosses in survival. Maybe no more than 1x tank, 2x mother
 *  - Check exit zone for 2024_07_13
 *  - Maybe only have error alarm in E-tier, it's quite difficult with that
 */
public partial record WardenObjective : DataBlock
{
    public double Survival_CalculateTime(BuildDirector director, Level level)
    {
        var nodes = level.Planner.GetZones(director.Bulkhead, null);
        var total = 0.0;

        foreach (var node in nodes)
        {
            var zone = level.Planner.GetZone(node)!;
            var zoneTotal = zone.GetClearTimeEstimate();

            // Add the total zone time to the time to survive
            total += zoneTotal;

            Plugin.Logger.LogDebug($"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead} -- "
                                   + $"Survival: Zone {node.ZoneNumber} time budget: total={zoneTotal}s");
        }

        return total;
    }

    public void PreBuild_Survival(BuildDirector director, Level level) { }

    public void Build_Survival(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        var exitZone = level.Planner.GetZones(director.Bulkhead, "exit").First();
        var exitNumber = layout.ZoneAliasStart + exitZone.ZoneNumber;
        var exitZoneString = $"<color=orange>ZONE {exitNumber}</color>";

        MainObjective = $"Find a way to stay alive during Warden Protocol DECOY, and make your way to {exitZoneString} for extraction";
        Survival_TimerTitle = "Time until allowed extraction:";
        Survival_TimerToActivateTitle = "<color=red>WARNING!</color> Warden Protocol <color=orange>DECOY</color> will commence in: ";
        // Set these both as go forward as we always have an exit geo.
        GoToWinCondition_Elevator = $"Go to the forward exit point in {exitZoneString}";
        GoToWinCondition_CustomGeo = $"Go to the forward exit point in {exitZoneString}";

        // Put a relatively short exit scan time as we will hit them hard on times up
        ChainedPuzzleAtExitScanSpeedMultiplier = GenExitScanTime(30, 40);

        // Set the base times. Give 60s to begin and calculate and add the additional times for survival
        Survival_TimeToActivate = 60.0;
        Survival_TimeToSurvive = Survival_CalculateTime(director, level);

        //==================== Events ====================
        EventsOnActivate.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                SoundId = Sound.Alarms_Error,
                Delay = 0.0
            });
        EventBuilder.AddSpawnEnemies(
            EventsOnActivate,
            GenericWave.Survival_ErrorAlarm,
            0.0,
            ":://WARNING - SECTOR ALARM ACTIVATED");

        // Unlock the exit
        // TODO: This didn't work again
        // Update: was it perhaps the layer number?
        EventBuilder.AddUnlockDoor(
            EventsOnGotoWin,
            director.Bulkhead,
            exitZone.ZoneNumber,
            $"Extraction zone {exitZoneString} unlocked");

        // Ending events
        // On end, start the tank
        EventBuilder.AddSpawnEnemies(
            EventsOnGotoWin,
            GenericWave.Survival_Impossible_TankPotato,
            4.0,
            ":://CRITICAL ERROR - LARGE B!OM4SS Ð!$_†URß@И¢€");
    }

    /// <summary>
    /// Adds an event to grant additional time when opening the extreme bulkhead door
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void PostBuild_Survival(BuildDirector director, Level level)
    {
        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Extreme))
        {
            ///
            /// For extreme we grant extra time to complete the objective
            ///
            var (extremeDataLayer, extremeLayout) = GetObjectiveLayerAndLayout(level.SecondaryDirector, level);

            var extremeTimeAdd = Survival_CalculateTime(level.SecondaryDirector, level);

            var node = level.Planner.GetZones(Bulkhead.Extreme, null).First();
            var zone = level.Planner.GetZone(node)!;

            EventBuilder.AddMessage(
                zone.EventsOnApproachDoor,
                "SECONDARY OBJECTIVES PRIORITIZED, EXTENDS LOCKDOWN TIME",
                5.0);

            // This sets a new survival timer?
            EventBuilder.SetSurvivalTimer(zone.EventsOnOpenDoor, extremeTimeAdd, "LOCKDOWN TIME EXTENDED");
        }

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Overload))
        {
            ///
            /// No additional time is granted for overload objectives in survival. They just
            /// have to be that hard.
            ///
            /// The overload objectives in survival are set to be much shorter, specifically to
            /// allow them to still be completed in time
            ///
            /// We could actually use the add to timer here now that it works correctly
            ///
            var node = level.Planner.GetZones(Bulkhead.Overload, null).First();
            var zone = level.Planner.GetZone(node)!;

            // Add extra resources to the overload zone to help reward doing it
            zone.AmmoPacks *= 2.0;
            zone.ToolPacks *= 2.0;
            zone.HealthPacks *= 2.0;

            EventBuilder.AddMessage(
                zone.EventsOnApproachDoor,
                "OVERLOAD OBJECTIVES NOT PRIORITIZED, NO ADDITIONAL TIME",
                5.0);
        }
    }
}
