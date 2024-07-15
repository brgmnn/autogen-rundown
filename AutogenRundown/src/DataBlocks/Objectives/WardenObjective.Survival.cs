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
    public void Survival_CalculateTime(BuildDirector director, Level level)
    {
        var nodes = level.Planner.GetZones(director.Bulkhead, null);

        foreach (var node in nodes)
        {
            var zone = level.Planner.GetZone(node)!;
            var total = zone.GetClearTimeEstimate();

            // Add the total zone time to the time to survive
            Survival_TimeToSurvive += total;

            Plugin.Logger.LogDebug($"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead} -- "
                                   + $"Survival: Zone {node.ZoneNumber} time budget: total={total}s");
        }
    }

    public void PreBuild_Survival(BuildDirector director, Level level) { }

    public void Build_Survival(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        var exitZone = level.Planner.GetZones(director.Bulkhead, "exit").First();
        var exitNumber = layout.ZoneAliasStart + exitZone.ZoneNumber;
        var exitZoneString = $"<color=orange>ZONE {exitNumber}</color>";

        MainObjective = $"Find a way to stay alive during Warden Protocol X:://FORLORN_DECOY, and make your way to {exitZoneString} for extraction";
        Survival_TimerTitle = "Time until allowed extraction:";
        Survival_TimerToActivateTitle = "<color=red>WARNING!</color> Warden Protocol <color=orange>X:://FORLORN_DECOY</color> will commence in: ";
        // Set these both as go forward as we always have an exit geo.
        GoToWinCondition_Elevator = $"Go to the forward exit point in {exitZoneString}";
        GoToWinCondition_CustomGeo = $"Go to the forward exit point in {exitZoneString}";

        // Put a relatively short exit scan time as we will hit them hard on times up
        ChainedPuzzleAtExitScanSpeedMultiplier = GenExitScanTime(30, 40);

        // Set the base times. Give 30s to begin and 30s base time on the time to survive
        Survival_TimeToActivate = 30.0;
        Survival_TimeToSurvive = 30.0;

        // Calculate and add the additional times
        Survival_CalculateTime(director, level);

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
        EventBuilder.AddUnlockDoor(
            EventsOnGotoWin,
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
}
