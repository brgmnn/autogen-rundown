using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;

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
public partial record class WardenObjective : DataBlock
{
    public void Survival_CalculateTime(BuildDirector director, Level level)
    {
        var factorAlarms = 1.20;
        var factorCoverage = 1.20;
        var factorEnemyPoints = 1.20;

        var nodes = level.Planner.GetZones(director.Bulkhead, null);

        foreach (var node in nodes)
        {
            var zone = level.Planner.GetZone(node)!;

            // Add time based on the zone size
            var timeCoverage = zone.Coverage.Max * factorCoverage;

            // Time based on enemy points in zones
            var timeEnemyPoints = zone.EnemySpawningInZone.Sum(spawn => spawn.Points) * factorEnemyPoints;

            // Time based on door alarms
            // We sum for the component durations, the distance from start pos, and the distance
            // between the alarm components
            var timeAlarms = 10 + zone.Alarm.Puzzle.Sum(component => component.Duration)
                                + zone.Alarm.WantedDistanceFromStartPos
                                + (zone.Alarm.Puzzle.Count - 1) * zone.Alarm.WantedDistanceBetweenPuzzleComponents;
            timeAlarms *= factorAlarms;

            // Give +20s for a blood door.
            var timeBloodDoor = zone.BloodDoor != BloodDoor.None ? 20 : 0;

            // Sum the values
            var totalZoneTime = timeCoverage + timeEnemyPoints + timeAlarms + timeBloodDoor;

            // Add the total zone time to the time to survive
            Survival_TimeToSurvive += totalZoneTime;

            Plugin.Logger.LogDebug($"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead} -- "
                + $"Survival: Zone {node.ZoneNumber} time budget: total={totalZoneTime}s -- "
                + $"alarms={timeAlarms}s coverage={timeCoverage}s enemies={timeEnemyPoints}s "
                + $"blood_doors={timeBloodDoor}s");
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
