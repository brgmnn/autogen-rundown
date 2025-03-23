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

        var alarmSkipOnDone = new List<WardenObjectiveEvent>
        {
            new()
            {
                Type = WardenObjectiveEventType.StopEventLoop,
                Count = SecurityControlEventLoopIndex
            }
        };

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
                Duration = Survival_CalculateSkipTime(director, level),
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
        }

        #region Warden Intel Messages
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
            ">... [alarm blaring] Move it!\r\n>... The clock's ticking.\r\n>... <size=200%><color=red>We can't die here!</color></size>",
            ">... Get to that bulkhead.\r\n>... It only opens when time's up.\r\n>... <size=200%><color=red>Keep them off our backs!</color></size>",
            ">... <size=200%><color=red>Watch your flank!</color></size>\r\n>... Enemies keep pouring in.\r\n>... We hold until the door unlocks.",
            ">... [heavy gunfire]\r\n>... Ten more minutes of this?\r\n>... <size=200%><color=red>We must hold our ground!</color></size>",
            ">... <size=200%><color=red>That alarm won't stop!</color></size>\r\n>... It's drawing them from everywhere.\r\n>... Keep your ammo ready.",
            ">... We're pinned down.\r\n>... <size=200%><color=red>The door's not open yet!</color></size>\r\n>... Keep firing!",
            ">... [adrenaline pumping]\r\n>... Survive until the countdown hits zero.\r\n>... <size=200%><color=red>Then we make our escape!</color></size>",
            ">... <size=200%><color=red>Check our timer!</color></size>\r\n>... We can't hold out forever.\r\n>... Stay together, watch corners.",
            ">... [screeching foes]\r\n>... They're relentless in this alarm.\r\n>... <size=200%><color=red>No turning back now!</color></size>",
            ">... <size=200%><color=red>Almost there!</color></size>\r\n>... Once that lock releases, run!\r\n>... We're out of time!",
            ">... [footsteps] Keep moving!\r\n>... The clock doesn't slow.\r\n>... <size=200%><color=red>We fight till it's done!</color></size>",
            ">... Stay close, watch corners.\r\n>... This alarm never stops.\r\n>... <size=200%><color=red>We last or we fall.</color></size>",
            ">... [gunfire echoing]\r\n>... Incoming from the left!\r\n>... <size=200%><color=red>Hold position!</color></size>",
            ">... <size=200%><color=red>Eyes on the timer!</color></size>\r\n>... That door won't budge yet.\r\n>... Keep them at bay!",
            ">... Weapons hot, ammo low.\r\n>... The wave isn't slowing.\r\n>... <size=200%><color=red>We hold, no matter what!</color></size>",
            ">... [screeching in distance]\r\n>... They keep coming...\r\n>... <size=200%><color=red>Don't lose your nerve!</color></size>",
            ">... <size=200%><color=red>Stack at the gate!</color></size>\r\n>... It's still locked.\r\n>... Survive until it opens.",
            ">... [panting] Not much longer.\r\n>... We can almost see zero.\r\n>... <size=200%><color=red>Hang in there!</color></size>",
            ">... Blood everywhere...\r\n>... Maintain formation!\r\n>... <size=200%><color=red>No one splits off!</color></size>",
            ">... <size=200%><color=red>Ten seconds left?</color></size>\r\n>... Feels like an eternity.\r\n>... We won't fail now!",
            ">... [loud alarm] It's maddening.\r\n>... Keep your heads, stay sharp.\r\n>... <size=200%><color=red>Not much time left!</color></size>",
            ">... I'm almost out of bullets.\r\n>... <size=200%><color=red>Check crates, anything helps!</color></size>\r\n>... The door's still locked.",
            ">... [explosion nearby]\r\n>... That's not good...\r\n>... <size=200%><color=red>Stay down and hold!</color></size>",
            ">... <size=200%><color=red>Watch your six!</color></size>\r\n>... They're circling around.\r\n>... Reinforce our rear line!",
            ">... The countdown is half done.\r\n>... We can do this!\r\n>... <size=200%><color=red>Stay alive!</color></size>",
            ">... [static in comms]\r\n>... Anyone see the exit?\r\n>... <size=200%><color=red>It's still sealed shut!</color></size>",
            ">... <size=200%><color=red>Cover me!</color></size>\r\n>... I'm reloading now.\r\n>... Keep them off me!",
            ">... The lights are flickering.\r\n>... <size=200%><color=red>Don't panic!</color></size>\r\n>... We must outlast them.",
            ">... [low growl nearby]\r\n>... They're right on top of us.\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
            ">... <size=200%><color=red>Stay together!</color></size>\r\n>... This is no time to split.\r\n>... We won't survive alone.",
            ">... I'm down to my sidearm!\r\n>... <size=200%><color=red>Any ammo left?</color></size>\r\n>... We can't run dry now.",
            ">... Door still locked...\r\n>... [alarm rings louder]\r\n>... <size=200%><color=red>Brace for another wave!</color></size>",
            ">... <size=200%><color=red>They're in the vents!</color></size>\r\n>... Keep scanning above.\r\n>... We can't afford surprises.",
            ">... The timer is everything.\r\n>... Survive until it ends.\r\n>... <size=200%><color=red>Then we bolt!</color></size>",
            ">... [shell casings clatter]\r\n>... Keep your lines tight!\r\n>... <size=200%><color=red>Don't let them flank us!</color></size>",
            ">... <size=200%><color=red>Out of medkits?</color></size>\r\n>... We'll push through the pain.\r\n>... No choice now.",
            ">... They've broken the barricade!\r\n>... Fall back slowly.\r\n>... <size=200%><color=red>Don't lose ground entirely!</color></size>",
            ">... [heart pounding]\r\n>... The door might open soon.\r\n>... <size=200%><color=red>Just hold on!</color></size>",
            ">... <size=200%><color=red>We can't outrun them!</color></size>\r\n>... We'll stand and fight.\r\n>... Wait for that lock to release.",
            ">... Time left: unknown...\r\n>... Feels like forever.\r\n>... <size=200%><color=red>Stay focused!</color></size>",
            ">... [roaring intensifies]\r\n>... They sense we're cornered.\r\n>... <size=200%><color=red>Show no weakness!</color></size>",
            ">... <size=200%><color=red>Vent incoming!</color></size>\r\n>... Block it with foam!\r\n>... Anything to slow them down.",
            ">... The door is still locked.\r\n>... <size=200%><color=red>Keep the alarm from deafening you!</color></size>\r\n>... Communicate at all costs.",
            ">... [flares flicker]\r\n>... Darkness won't help us.\r\n>... <size=200%><color=red>Check your flashlights!</color></size>",
            ">... <size=200%><color=red>We've lost someone!</color></size>\r\n>... Keep pushing forward.\r\n>... Can't stop now.",
            ">... The wave receded?\r\n>... Another will come soon.\r\n>... <size=200%><color=red>Reload and regroup!</color></size>",
            ">... [spike in alarm pitch]\r\n>... It's intensifying again!\r\n>... <size=200%><color=red>Positions, hurry!</color></size>",
            ">... <size=200%><color=red>Lock's at 25%!</color></size>\r\n>... Still too long...\r\n>... We must endure.",
            ">... Keep the turrets operational.\r\n>... <size=200%><color=red>Every gun counts!</color></size>\r\n>... No second chances here.",
            ">... [door rattling]\r\n>... They're trying to break in.\r\n>... <size=200%><color=red>Stop them or we're done!</color></size>",
            ">... <size=200%><color=red>Stay in scanning range!</color></size>\r\n>... We can't miss any approach.\r\n>... Don't give them an opening.",
            ">... The corridor is blocked.\r\n>... <size=200%><color=red>Find another choke point!</color></size>\r\n>... Keep them funneled.",
            ">... [breathing heavily]\r\n>... The adrenaline won't last.\r\n>... <size=200%><color=red>We must outlast them!</color></size>",
            ">... <size=200%><color=red>That siren's drilling my head!</color></size>\r\n>... Keep calm, watch your aim.\r\n>... Panicking is death.",
            ">... Stay quiet, if possible.\r\n>... <size=200%><color=red>We can't lure more in!</color></size>\r\n>... Let them come to us.",
            ">... [slamming against door]\r\n>... It's holding for now.\r\n>... <size=200%><color=red>But not for long!</color></size>",
            ">... <size=200%><color=red>Time check!</color></size>\r\n>... Halfway to unlocking.\r\n>... Feels like an eternity.",
            ">... I'm bleeding bad...\r\n>... <size=200%><color=red>Medic, patch me up!</color></size>\r\n>... Hurry, next wave soon.",
            ">... [gun clicks empty]\r\n>... No ammo left.\r\n>... <size=200%><color=red>Grab a spare mag!</color></size>",
            ">... <size=200%><color=red>They're climbing the walls!</color></size>\r\n>... Use the high ground!\r\n>... Don't let them flank.",
            ">... Keep those mines placed.\r\n>... <size=200%><color=red>Every trap helps!</color></size>\r\n>... Save the big ones.",
            ">... [unrelenting alarm]\r\n>... This is insane...\r\n>... <size=200%><color=red>We must endure!</color></size>",
            ">... <size=200%><color=red>They found a side path!</color></size>\r\n>... Fall back, regroup.\r\n>... We hold them off again.",
            ">... I'm watching that timer.\r\n>... <size=200%><color=red>Not nearly done yet.</color></size>\r\n>... Reload, reload!",
            ">... [faint moaning]\r\n>... Another wave inbound.\r\n>... <size=200%><color=red>Steady yourselves!</color></size>",
            ">... <size=200%><color=red>We can't lose anyone else!</color></size>\r\n>... Stick together, cover each other.\r\n>... Survive this countdown.",
            ">... Smoke in the corridor...\r\n>... <size=200%><color=red>They might be hiding in it!</color></size>\r\n>... Stay alert!",
            ">... [metal scraping]\r\n>... The door tries to unlock.\r\n>... <size=200%><color=red>Still jammed!</color></size>",
            ">... <size=200%><color=red>Reinforce the barricade!</color></size>\r\n>... We'll wait out the timer.\r\n>... Don't let them break through!",
            ">... Keep eyes on that hallway.\r\n>... <size=200%><color=red>They'll rush again soon.</color></size>\r\n>... Prepare defenses.",
            ">... [yelling in distance]\r\n>... Another team didn't make it.\r\n>... <size=200%><color=red>We won't share their fate!</color></size>",
            ">... <size=200%><color=red>Close those shutters!</color></size>\r\n>... That might slow them.\r\n>... Just a bit longer.",
            ">... Check your gear.\r\n>... <size=200%><color=red>We can't fix it mid-fight!</color></size>\r\n>... Be ready for anything.",
            ">... [gunshot echoes]\r\n>... I'm running out.\r\n>... <size=200%><color=red>Scavenge if you can!</color></size>",
            ">... <size=200%><color=red>Look sharp!</color></size>\r\n>... They're using every opening.\r\n>... No safe spot left.",
            ">... The alarm is deafening...\r\n>... <size=200%><color=red>Focus on your target!</color></size>\r\n>... Don't let panic win.",
            ">... [heartbeat racing]\r\n>... Ten seconds to go.\r\n>... <size=200%><color=red>We can make it!</color></size>",
            ">... <size=200%><color=red>They're swarming again!</color></size>\r\n>... Keep behind cover.\r\n>... We hold them here.",
            ">... That door's so close to unlocking.\r\n>... <size=200%><color=red>Don't lose hope!</color></size>\r\n>... Just a bit more time.",
            ">... [comm static] Who's left?\r\n>... <size=200%><color=red>Stay in radio contact!</color></size>\r\n>... We can't split up now.",
            ">... <size=200%><color=red>Brace for final surge!</color></size>\r\n>... The timer's nearly zero.\r\n>... Everything rides on this!",
            ">... If we survive this...\r\n>... <size=200%><color=red>We walk right out!</color></size>\r\n>... Then we're free.",
            ">... [gasping for air]\r\n>... So many close calls.\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
            ">... <size=200%><color=red>Watch overhead vents!</color></size>\r\n>... They drop in from anywhere.\r\n>... Stay vigilant!",
            ">... My arms are shaking.\r\n>... <size=200%><color=red>We can't quit now!</color></size>\r\n>... Push through the pain.",
            ">... [sharp screech]\r\n>... That means they're near.\r\n>... <size=200%><color=red>Weapons up!</color></size>",
            ">... <size=200%><color=red>Door's still sealed.</color></size>\r\n>... Let's hold position.\r\n>... No running left to do.",
            ">... The alarm lights flicker.\r\n>... <size=200%><color=red>This might be the last wave!</color></size>\r\n>... Stay ready!",
            ">... [broken panel sparks]\r\n>... Power fluctuations again.\r\n>... <size=200%><color=red>Time won't pause for us!</color></size>",
            ">... <size=200%><color=red>No place to hide!</color></size>\r\n>... We stand or fall.\r\n>... Countdown or death.",
            ">... Check your health.\r\n>... <size=200%><color=red>We need everyone upright!</color></size>\r\n>... No survivors left behind.",
            ">... [shells hitting floor]\r\n>... Keep reloading, keep firing.\r\n>... <size=200%><color=red>Every second counts!</color></size>",
            ">... <size=200%><color=red>Five seconds!</color></size>\r\n>... Get near the exit!\r\n>... Run at zero!",
            ">... Another wave forming...\r\n>... <size=200%><color=red>This must be the final push!</color></size>\r\n>... Make it count.",
            ">... [frantic steps]\r\n>... I'm covering you!\r\n>... <size=200%><color=red>Don't look back!</color></size>",
            ">... <size=200%><color=red>Look at the lock!</color></size>\r\n>... It's nearly open!\r\n>... Brace for one last stand.",
            ">... Our last magazine...\r\n>... <size=200%><color=red>We must use it wisely!</color></size>\r\n>... No mistakes now.",
            ">... [echoing roars]\r\n>... Final countdown started!\r\n>... <size=200%><color=red>Hold them off!</color></size>",
            ">... <size=200%><color=red>Open fire!</color></size>\r\n>... Keep them at bay!\r\n>... Time's almost up.",
            ">... The alarm is at max.\r\n>... <size=200%><color=red>We run for the door soon!</color></size>\r\n>... No second chance.",
            ">... [gunfire crescendos]\r\n>... We see the lock disengaging.\r\n>... <size=200%><color=red>Go, go now!</color></size>",
            ">... We survived!\r\n>... Get through that exit!\r\n>... <size=200%><color=red>Wait, what's that?!</color></size>"
        }))!);
        #endregion
    }
}
