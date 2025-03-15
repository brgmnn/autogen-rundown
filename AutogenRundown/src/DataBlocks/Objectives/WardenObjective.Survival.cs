using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;

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

    /// <summary>
    /// Calcualtes the time granted to do all the zones with the force open doors command
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public double Survival_CalculateSkipTime(BuildDirector director, Level level)
    {
        var nodes = level.Planner.GetZones(director.Bulkhead, "survival_arena");

        // remove the first zone, as they will (should) have cleared it already
        nodes.RemoveAt(0);

        // Add the exit zone which we want to include
        var exit = level.Planner.GetZones(director.Bulkhead, "exit").First();
        nodes.Add(exit);

        // Give a fixed buffer for getting together and extracting
        var total = 15.0;

        foreach (var node in nodes)
        {
            var zone = level.Planner.GetZone(node)!;
            var zoneTotal = zone.ClearTime_AreaCoverage() +
                            zone.ClearTime_Enemies() +
                            zone.ClearTime_Bosses() +
                            zone.ClearTime_BloodDoor();

            // Add the total zone time to the time to survive
            total += zoneTotal;

            Plugin.Logger.LogDebug($"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead} -- "
                                   + $"Survival: Zone {node.ZoneNumber} skip time budget: total={zoneTotal}s");
        }

        return total;
    }

    public void Build_Survival(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        var exitZone = level.Planner.GetZones(director.Bulkhead, "exit").First();
        var exitNumber = layout.ZoneAliasStart + exitZone.ZoneNumber;
        var exitZoneString = Lore.Zone(exitNumber);

        MainObjective = $"Find a way to stay alive during Warden Protocol DECOY, and make your way to {exitZoneString} for extraction";
        Survival_TimerTitle = "Time until allowed extraction:";
        Survival_TimerToActivateTitle = "<color=red>WARNING!</color> Warden Protocol <color=orange>DECOY</color> will commence in: ";
        // Set these both as go forward as we always have an exit geo.
        GoToWinCondition_Elevator = $"Go to the forward exit point in {exitZoneString}";
        GoToWinCondition_CustomGeo = $"Go to the forward exit point in {exitZoneString}";

        // Put a relatively short exit scan time as we will hit them hard on times up
        ChainedPuzzleAtExitScanSpeedMultiplier = GenExitScanTime(30, 40);

        //================== AWO version ================
        // var onProgress = new List<ProgressEvent>()
        // {
        //     new ProgressEvent()
        //     {
        //         Progress = 0.75,
        //         Events = new List<WardenObjectiveEvent>().AddMessage("EventsOnProgress fired at 75%").ToList()
        //     }
        // };

        // Start of survival events
        var onStart = new List<WardenObjectiveEvent>();

        onStart.Add(new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                Trigger = WardenObjectiveEventTrigger.None,
                SoundId = Sound.Alarms_Error_AmbientLoop
            });
        onStart
            .AddGenericWave(GenericWave.Survival_ErrorAlarm)
            .AddMessage(":://WARNING - SECTOR ALARM ACTIVATED");

        // Finish events
        var onDone = new List<WardenObjectiveEvent>();

        onDone
            .AddUnlockDoor(
                director.Bulkhead,
                exitZone.ZoneNumber,
                $"Extraction zone {exitZoneString} unlocked",
                WardenObjectiveEventTrigger.None)
            .AddGenericWave(GenericWave.Survival_Impossible_TankPotato, 4.0)
            .AddSound(Sound.TankRoar, 7.0)
            .AddMessage(":://CRITICAL ERROR - LARGE B!OM4SS Ð!$_†URß@И¢€");

        var mainCountdown = new WardenObjectiveEvent
        {
            Type = WardenObjectiveEventType.Countdown,
            Delay = 0.0,
            Duration = Survival_CalculateTime(director, level),
            Countdown = new WardenObjectiveEventCountdown
            {
                TitleText = "Time until allowed extraction:",
                TimerColor = "red",
                EventsOnDone = onDone
            }
        };

        // Ensure we add the main countdown event
        onStart.Add(mainCountdown);

        var setupCountdown = new WardenObjectiveEvent
        {
            Type = WardenObjectiveEventType.Countdown,
            Delay = 3.5,
            Duration = 60.0,
            Countdown = new WardenObjectiveEventCountdown
            {
                TitleText = "<color=red>WARNING!</color> Warden Protocol <color=orange>DECOY</color> will commence in:",
                TimerColor = "#ffaa00",
                EventsOnDone = onStart
            }
        };

        EventsOnElevatorLand.Add(setupCountdown);

        #region Side objective

        var alarmSkipOnDone = new List<WardenObjectiveEvent>();
        alarmSkipOnDone.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.StopEventLoop,
                Count = SecurityControlEventLoopIndex
            });

        alarmSkipOnDone
            .AddStopLoop(SecurityControlEventLoopIndex)
            .AddAllLightsOff(1.0, WardenObjectiveEventTrigger.None)
            .AddSound(Sound.LightsOff, 0.0, WardenObjectiveEventTrigger.None)
            .AddMessage("://CRITICAL FAILURE - SECURITY SYSTEMS OFFLINE", 0.0)
            .AddGenericWave(GenericWave.Survival_Impossible_TankPotato, 9.0)
            .AddSound(Sound.TankRoar, 12.0)
            .AddMessage(":://CRITICAL ERROR - LARGE B!OM4SS Ð!$_†URß@И¢€", 5.0);

        var duration = 0.0;

        SecurityControlEvents.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.Countdown,
                Delay = 0.0,
                // Duration = Survival_CalculateSkipTime(director, level),
                Duration = 60,
                Countdown = new WardenObjectiveEventCountdown
                {
                    TitleText = "<color=red>SECURITY OVERRIDE!</color>: Time until full security shutdown:",
                    TimerColor = "red",
                    EventsOnDone = alarmSkipOnDone
                }
            });
        #endregion
    }

    /// <summary>
    /// Adds an event to grant additional time when opening the extreme bulkhead door
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void PostBuild_Survival(BuildDirector director, Level level)
    {
        // We change it to be a Clear Path
        // This should allow the core part of the objective which is the same as clear path work,
        // but let's us use the AWO timer instead of the base game which is more flexible for
        // adjusting the timer.
        Type = WardenObjectiveType.ClearPath;

        // Add additional time for clearing extreme
        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Extreme))
        {
            var extremeClearTime = Survival_CalculateTime(level.SecondaryDirector, level);

            var node = level.Planner.GetZones(Bulkhead.Extreme, null).First();
            var zone = level.Planner.GetZone(node)!;

            zone.EventsOnApproachDoor
                .AddMessage("SECONDARY OBJECTIVES PRIORITIZED, EXTENDS LOCKDOWN TIME ON OPEN", 5.0);

            zone.EventsOnOpenDoor
                .AddAdjustTimer(extremeClearTime)
                .AddMessage("LOCKDOWN TIME EXTENDED");

            // TODO: do we even need this? I think no in the current setup.
            // // Lock extreme for skip security control events as it grants extra time.
            // SecurityControlEvents.AddLockExtreme();
        }

        // Add additional time for clearing overload
        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Overload))
        {
            var overloadClearTime = Survival_CalculateTime(level.OverloadDirector, level);

            var node = level.Planner.GetZones(Bulkhead.Overload, null).First();
            var zone = level.Planner.GetZone(node)!;

            zone.EventsOnApproachDoor
                .AddMessage("OVERLOAD OBJECTIVES PRIORITIZED, EXTENDS LOCKDOWN TIME ON OPEN", 5.0);

            zone.EventsOnOpenDoor
                .AddAdjustTimer(overloadClearTime)
                .AddMessage("LOCKDOWN TIME EXTENDED");

            // TODO: do we even need this? I think no in the current setup.
            // // Lock extreme for skip security control events as it grants extra time.
            // SecurityControlEvents.AddLockOverload();
        }
    }
}
