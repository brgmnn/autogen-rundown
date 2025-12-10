using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks.Objectives;

public partial record WardenObjective
{
    public void PreBuild_TimedTerminalSequence(BuildDirector director, Level level)
    {
        // TODO: change these?
        TimedTerminalSequence_NumberOfRounds = 3;
        TimedTerminalSequence_NumberOfTerminals = 3;
    }

    /// <summary>
    /// Aparently you can set the terminal it picks using the WorldEventFilter?
    ///
    /// If true, it would mean we could control which terminals are used.
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void Build_TimedTerminalSequence(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Find [ITEM_SERIAL] and initiate timed sequence protocol.");
        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";

        TimedTerminalSequence_TimeForConfirmation = 10.0;
        //TimedTerminalSequence_TimeForConfirmation = 120.0; // DEBUG

        TimedTerminalSequence_TimePerRound = 60.0;

        // Calculate round time based on max time to clear a zone
        for (int i = 0; i < TimedTerminalSequence_NumberOfTerminals; i++)
        {
            var nodes = level.Planner.GetZones(director.Bulkhead, $"timed_terminal_{i}");
            var total = 0.0;

            foreach (var node in nodes)
            {
                var zone = level.Planner.GetZone(node)!;
                total += zone.GetClearTimeEstimate();
            }

            // We will just set the time per round to the max time we would need to clear the
            // hardest zone
            TimedTerminalSequence_TimePerRound = Math.Max(TimedTerminalSequence_TimePerRound, total);
        }

        ///
        /// Build the event pool of things that can be triggered on sequence events
        ///
        var eventPool = new List<(double, int, ICollection<WardenObjectiveEvent>)>
        {
            // Base error alarm
            (1.0, 1, new List<WardenObjectiveEvent>()
                    .AddMessage($"{Intel.Error}! Alarm Active", 7.0)
                    .AddSpawnWave(new GenericWave
                    {
                        Population = WavePopulation.Baseline,
                        Settings = WaveSettings.Error_Easy,
                    }, 8.0)),

            // Some padding waves to always have something
            (0.5, 3, new List<WardenObjectiveEvent>()
                    .AddSpawnWave(new GenericWave
                    {
                        Population = WavePopulation.Baseline,
                        Settings = WaveSettings.Finite_35pts_Hard
                    }, 15.0))
        };

        var roundTwoEvents = new List<(double, int, ICollection<WardenObjectiveEvent>)>
        {
            // Tank single spawn
            (1.0, 2, new List<WardenObjectiveEvent>()
                    .AddMessage($"{Intel.Error} LARGE BIOMASS DETECTED", 12.0)
                    .AddSpawnWave(GenericWave.SingleTank, 11.0)),

            // Single Potato
            (0.5, 2, new List<WardenObjectiveEvent>()
                    .AddMessage($"{Intel.Error} LARGE BIOMASS DETECTED", 12.0)
                    .AddSpawnWave(GenericWave.SingleTankPotato, 18.0)),

            // Single Mother
            (0.1, 3, new List<WardenObjectiveEvent>()
                    .AddMessage($"{Intel.Error} LARGE BIOMASS DETECTED", 12.0)
                    .AddSpawnWave(GenericWave.SingleMother, 22.0))
        };

        var extraEvents = new List<(double, int, ICollection<WardenObjectiveEvent>)>
        {
            // Nothing
            (1.0, 3, new List<WardenObjectiveEvent>()),

            // Lights off
            (0.5, 1, new List<WardenObjectiveEvent>().AddLightsOff(20.0)),

            // Fog flood
            (0.1, 1, new List<WardenObjectiveEvent>().AddFillFog(
                5, 1800, $"{Intel.Warning} - VENTILATION SYSTEM ON BACKUP POWER"))
        };

        // Add waves etc. on each round
        for (var round = 0; round < TimedTerminalSequence_NumberOfRounds; round++)
        {
            var onStart = Generator.DrawSelect(eventPool).ToList();
            TimedTerminalSequence_EventsOnSequenceDone.Add(new());
            TimedTerminalSequence_EventsOnSequenceFail.Add(new());

            // Unlock all the terminal zone doors on round start
            if (round == 0)
            {
                for (var t = 0; t < TimedTerminalSequence_NumberOfTerminals; t++)
                {
                    var first = level.Planner.GetZones(director.Bulkhead, $"timed_terminal_{t}").First();
                    EventBuilder.AddUnlockDoor(onStart, director.Bulkhead, first.ZoneNumber);
                }
            }

            // Add some more events for post round 0
            if (round > 0)
                eventPool.AddRange(roundTwoEvents);

            // Potentially add some bonus events
            onStart.AddRange(Generator.DrawSelect(extraEvents));

            TimedTerminalSequence_EventsOnSequenceStart.Add(onStart);
        }

        var turnOff = level.Planner.GetZones(director.Bulkhead, "timed_terminal_error_off");

        // Add event to unlock error alarm disable (if we have the timed terminal error off zone)
        if (turnOff.Any())
            EventsOnGotoWin.Add(new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.UnlockSecurityDoor,
                    LocalIndex = turnOff.First().ZoneNumber
                });
    }

    private void PostBuildIntel_TimedTerminalSequence(Level level)
    {
        #region Warden Intel Messages
        // Intel variables

        var verificationRounds = TimedTerminalSequence_NumberOfRounds.ToCardinal();
        var terminalCount = TimedTerminalSequence_NumberOfTerminals.ToCardinal();
        var midRound = 1.ToCardinal();

        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
        {
            // Timer Pressure & 10-Second Sprint (Static)
            ">... Ten seconds!\r\n>... <size=200%><color=red>I can't make it!</color></size>\r\n>... [frantic footsteps]",
            ">... [panting]\r\n>... Nine seconds left!\r\n>... <size=200%><color=red>Where's the central?!</color></size>",
            ">... Eight seconds!\r\n>... <size=200%><color=red>Get out of the way!</color></size>\r\n>... [gunfire]",
            ">... The timer!\r\n>... <size=200%><color=red>Seven seconds!</color></size>\r\n>... Run faster!",
            ">... Six!\r\n>... Almost there!\r\n>... <size=200%><color=red>Move!</color></size>",
            ">... Five seconds!\r\n>... <size=200%><color=red>Clear the path!</color></size>\r\n>... [screaming]",
            ">... Four!\r\n>... <size=200%><color=red>I'm not gonna make it!</color></size>\r\n>... Keep running!",
            ">... Three seconds!\r\n>... [panting]\r\n>... <size=200%><color=red>Almost!</color></size>",
            ">... Two!\r\n>... <size=200%><color=red>There!</color></size>\r\n>... [typing frantically]",
            ">... One second!\r\n>... <size=200%><color=red>CONFIRM!</color></size>\r\n>... [typing]",
            ">... <size=200%><color=red>Time's up!</color></size>\r\n>... No!\r\n>... [alarm blaring]",
            ">... We're out of time!\r\n>... <size=200%><color=red>Sequence failed!</color></size>\r\n>... [static]",
            ">... Ten seconds isn't enough!\r\n>... <size=200%><color=red>They're too far apart!</color></size>\r\n>... Impossible!",
            ">... Sprint faster!\r\n>... <size=200%><color=red>Drop everything!</color></size>\r\n>... Lighter!",
            ">... [gasping]\r\n>... Can't... breathe...\r\n>... <size=200%><color=red>Keep going!</color></size>",
            ">... How much time?\r\n>... <size=200%><color=red>Seconds!</color></size>\r\n>... [frantic running]",
            ">... The countdown!\r\n>... <size=200%><color=red>Go go go!</color></size>\r\n>... [panting]",
            ">... Too slow!\r\n>... <size=200%><color=red>Faster!</color></size>\r\n>... [screaming]",
            ">... Timer started!\r\n>... Ten seconds to get back!\r\n>... <size=200%><color=red>That's impossible!</color></size>",
            ">... [heavy breathing]\r\n>... <size=200%><color=red>Out of stamina!</color></size>\r\n>... Keep moving!",

            // Terminal Search Chaos (Static)
            ">... Which terminal?!\r\n>... <size=200%><color=red>I don't know!</color></size>\r\n>... Check them all!",
            ">... Not this one!\r\n>... <size=200%><color=red>Keep searching!</color></size>\r\n>... [running]",
            ">... Found it!\r\n>... Where?!\r\n>... <size=200%><color=red>Zone ahead!</color></size>",
            ">... Wrong terminal!\r\n>... <size=200%><color=red>Try the next one!</color></size>\r\n>... Hurry!",
            ">... It's not here!\r\n>... <size=200%><color=red>Keep looking!</color></size>\r\n>... Time's running out!",
            ">... This one?\r\n>... No!\r\n>... <size=200%><color=red>Different zone!</color></size>",
            ">... Parallel terminal located!\r\n>... <size=200%><color=red>Move now!</color></size>\r\n>... [sprinting]",
            ">... Where is it?!\r\n>... <size=200%><color=red>Search faster!</color></size>\r\n>... [frantic footsteps]",
            ">... Not the right one!\r\n>... How many left?\r\n>... <size=200%><color=red>Too many!</color></size>",
            ">... Split up!\r\n>... <size=200%><color=red>Check every terminal!</color></size>\r\n>... Go!",
            ">... Random every time!\r\n>... <size=200%><color=red>Could be anywhere!</color></size>\r\n>... Keep searching!",
            ">... Terminal's different!\r\n>... It randomized!\r\n>... <size=200%><color=red>Search again!</color></size>",
            ">... Where's the parallel?!\r\n>... <size=200%><color=red>I'm checking!</color></size>\r\n>... Faster!",
            ">... Is it this one?\r\n>... Try it!\r\n>... <size=200%><color=red>No match!</color></size>",
            ">... Found the parallel!\r\n>... <size=200%><color=red>Going in!</color></size>\r\n>... [typing]",
            ">... How far is it?\r\n>... <size=200%><color=red>Too far!</color></size>\r\n>... We'll never make it back!",
            ">... Terminal distances!\r\n>... <size=200%><color=red>They're spread out!</color></size>\r\n>... This is bad!",
            ">... Not enough time to search!\r\n>... <size=200%><color=red>Keep trying!</color></size>\r\n>... [panting]",
            ">... Which zone?!\r\n>... <size=200%><color=red>I don't know!</color></size>\r\n>... [running]",
            ">... Terminal's moving?\r\n>... No, it's random!\r\n>... <size=200%><color=red>Different each time!</color></size>",

            // Command Execution (Static)
            ">... INIT_TIMED_CONNECTION!\r\n>... <size=200%><color=red>Sequence started!</color></size>\r\n>... [alarm blaring]",
            ">... Typing INIT...\r\n>... [typing]\r\n>... <size=200%><color=red>Here they come!</color></size>",
            ">... Command entered!\r\n>... <size=200%><color=red>Timer's running!</color></size>\r\n>... Go!",
            ">... VERIFY now!\r\n>... <size=200%><color=red>Typing!</color></size>\r\n>... [frantic typing]",
            ">... VERIFY_TIMED_CONNECTION!\r\n>... <size=200%><color=red>Ten second timer!</color></size>\r\n>... Run!",
            ">... Verification complete!\r\n>... <size=200%><color=red>Sprint back!</color></size>\r\n>... [running]",
            ">... CONFIRM!\r\n>... [typing]\r\n>... <size=200%><color=red>Did it work?!</color></size>",
            ">... CONFIRM_TIMED_CONNECTION!\r\n>... <size=200%><color=red>Sequence complete!</color></size>\r\n>... [relief]",
            ">... Wrong command!\r\n>... <size=200%><color=red>VERIFY not CONFIRM!</color></size>\r\n>... [panic]",
            ">... Command failed!\r\n>... <size=200%><color=red>Try again!</color></size>\r\n>... [typing]",
            ">... What's the command?!\r\n>... INIT!\r\n>... <size=200%><color=red>INIT_TIMED_CONNECTION!</color></size>",
            ">... Typing...\r\n>... <size=200%><color=red>Faster!</color></size>\r\n>... [frantic typing]",
            ">... Tab complete!\r\n>... Good!\r\n>... <size=200%><color=red>Enter!</color></size>",
            ">... Command syntax?\r\n>... <size=200%><color=red>Just type VERIFY!</color></size>\r\n>... [typing]",
            ">... [typing]\r\n>... Wrong terminal!\r\n>... <size=200%><color=red>Find the parallel!</color></size>",
            ">... CONFIRM at central!\r\n>... <size=200%><color=red>Not here!</color></size>\r\n>... Wrong one!",
            ">... Pre-type it!\r\n>... <size=200%><color=red>Ready to enter!</color></size>\r\n>... Hit it!",
            ">... Command ready!\r\n>... On your mark!\r\n>... <size=200%><color=red>Now!</color></size>",
            ">... [typing]\r\n>... <size=200%><color=red>Entered!</color></size>\r\n>... Timer started!",
            ">... Typo!\r\n>... <size=200%><color=red>Fix it!</color></size>\r\n>... No time!",

            // Team Coordination (Static)
            ">... Runner ready?\r\n>... <size=200%><color=red>Go!</color></size>\r\n>... [sprinting]",
            ">... Scouts split up!\r\n>... <size=200%><color=red>Search all terminals!</color></size>\r\n>... Roger!",
            ">... Defenders hold here!\r\n>... <size=200%><color=red>Keep them off the runner!</color></size>\r\n>... [gunfire]",
            ">... Clear the path!\r\n>... <size=200%><color=red>Runner incoming!</color></size>\r\n>... Move!",
            ">... Who's running?\r\n>... Fastest player!\r\n>... <size=200%><color=red>You!</color></size>",
            ">... Team positions?\r\n>... <size=200%><color=red>Spread out!</color></size>\r\n>... Search pattern!",
            ">... On my mark!\r\n>... Ready!\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... Cover the runner!\r\n>... <size=200%><color=red>Enemies incoming!</color></size>\r\n>... [gunfire]",
            ">... Stay together!\r\n>... <size=200%><color=red>No, split up!</color></size>\r\n>... Search faster!",
            ">... Who's at central?\r\n>... <size=200%><color=red>Nobody!</color></size>\r\n>... Get there!",
            ">... Callouts!\r\n>... <size=200%><color=red>Found it!</color></size>\r\n>... Where?!",
            ">... Team split?\r\n>... <size=200%><color=red>Yes!</color></size>\r\n>... Search coverage!",
            ">... Fastest route?\r\n>... <size=200%><color=red>Through there!</color></size>\r\n>... Go!",
            ">... Defend or search?\r\n>... <size=200%><color=red>Both!</color></size>\r\n>... Split roles!",
            ">... Communication!\r\n>... <size=200%><color=red>Call it out!</color></size>\r\n>... Found parallel!",
            ">... Regroup!\r\n>... <size=200%><color=red>No time!</color></size>\r\n>... Keep searching!",
            ">... Where's the team?\r\n>... Scattered!\r\n>... <size=200%><color=red>Get organized!</color></size>",
            ">... Role assignments!\r\n>... <size=200%><color=red>You run, I defend!</color></size>\r\n>... Roger!",
            ">... Lost contact!\r\n>... <size=200%><color=red>Anyone?!</color></size>\r\n>... [static]",
            ">... Coordination failing!\r\n>... <size=200%><color=red>Stick to the plan!</color></size>\r\n>... [chaos]",

            // Enemy Combat During Sequence (Static)
            ">... Spawns started!\r\n>... <size=200%><color=red>They're everywhere!</color></size>\r\n>... [gunfire]",
            ">... Continuous waves!\r\n>... <size=200%><color=red>They won't stop!</color></size>\r\n>... [screaming]",
            ">... Ignore them!\r\n>... <size=200%><color=red>Just run!</color></size>\r\n>... [sprinting]",
            ">... Giant!\r\n>... <size=200%><color=red>Keep moving!</color></size>\r\n>... Don't engage!",
            ">... Striker wave!\r\n>... <size=200%><color=red>Push through!</color></size>\r\n>... [gunfire]",
            ">... Low on ammo!\r\n>... <size=200%><color=red>Conserve!</color></size>\r\n>... Melee!",
            ">... They're blocking the path!\r\n>... <size=200%><color=red>Clear them!</color></size>\r\n>... [gunfire]",
            ">... Shooter nest!\r\n>... <size=200%><color=red>Suppress fire!</color></size>\r\n>... [gunfire]",
            ">... Behind us!\r\n>... <size=200%><color=red>Keep running!</color></size>\r\n>... Don't stop!",
            ">... Swarmed!\r\n>... <size=200%><color=red>Fight through!</color></size>\r\n>... [screaming]",
            ">... Charger!\r\n>... <size=200%><color=red>Dodge!</color></size>\r\n>... [impact]",
            ">... Tank spotted!\r\n>... <size=200%><color=red>Run around it!</color></size>\r\n>... No time to fight!",
            ">... Spawns won't stop!\r\n>... <size=200%><color=red>Until we complete it!</color></size>\r\n>... Hurry!",
            ">... Defending runner!\r\n>... <size=200%><color=red>Keep them busy!</color></size>\r\n>... [gunfire]",
            ">... Path's blocked!\r\n>... <size=200%><color=red>Clear it!</color></size>\r\n>... [gunfire]",
            ">... Enemy at central!\r\n>... <size=200%><color=red>Kill it!</color></size>\r\n>... [gunfire]",
            ">... Out of resources!\r\n>... <size=200%><color=red>Just complete it!</color></size>\r\n>... Almost there!",
            ">... Taking damage!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [panting]",
            ">... Can't fight them all!\r\n>... <size=200%><color=red>Don't try!</color></size>\r\n>... Run!",
            ">... Overwhelmed!\r\n>... <size=200%><color=red>Focus on sequence!</color></size>\r\n>... [chaos]",

            // Sequence Failures & Restarts (Static)
            ">... Failed!\r\n>... <size=200%><color=red>Start over!</color></size>\r\n>... [frustrated]",
            ">... Sequence reset!\r\n>... <size=200%><color=red>Try again!</color></size>\r\n>... Different terminal this time!",
            ">... Didn't make it!\r\n>... <size=200%><color=red>Restart!</color></size>\r\n>... [panting]",
            ">... Timer expired!\r\n>... <size=200%><color=red>Back to the start!</color></size>\r\n>... [groaning]",
            ">... Failed verification!\r\n>... <size=200%><color=red>Again!</color></size>\r\n>... [typing]",
            ">... Sequence error!\r\n>... <size=200%><color=red>Restart from INIT!</color></size>\r\n>... [frustrated]",
            ">... Third attempt!\r\n>... <size=200%><color=red>Make it count!</color></size>\r\n>... Go!",
            ">... How many tries?\r\n>... Too many!\r\n>... <size=200%><color=red>One more!</color></size>",
            ">... Failed again!\r\n>... <size=200%><color=red>We can't do this!</color></size>\r\n>... Try anyway!",
            ">... Reset!\r\n>... <size=200%><color=red>New parallel terminal!</color></size>\r\n>... Search!",
            ">... Sequence aborted!\r\n>... <size=200%><color=red>Start over!</color></size>\r\n>... [groaning]",
            ">... Retry!\r\n>... <size=200%><color=red>Same plan!</color></size>\r\n>... Different terminal!",
            ">... Another failure!\r\n>... <size=200%><color=red>Keep trying!</color></size>\r\n>... [panting]",
            ">... Restart sequence!\r\n>... <size=200%><color=red>From the top!</color></size>\r\n>... INIT again!",
            ">... Failed confirmation!\r\n>... <size=200%><color=red>One more time!</color></size>\r\n>... [typing]",
            ">... Sequence incomplete!\r\n>... <size=200%><color=red>Retry!</color></size>\r\n>... [frustrated]",
            ">... Start over!\r\n>... <size=200%><color=red>Again!</color></size>\r\n>... [groaning]",
            ">... Timeout!\r\n>... <size=200%><color=red>Back to INIT!</color></size>\r\n>... [frustrated]",
            ">... Failed at confirm!\r\n>... <size=200%><color=red>So close!</color></size>\r\n>... Try again!",
            ">... Aborting!\r\n>... <size=200%><color=red>Restart!</color></size>\r\n>... [static]",

            // General Panic & Stress (Static)
            ">... This is impossible!\r\n>... <size=200%><color=red>Keep trying!</color></size>\r\n>... [panting]",
            ">... Can't do this!\r\n>... <size=200%><color=red>We have to!</color></size>\r\n>... [screaming]",
            ">... Too fast!\r\n>... <size=200%><color=red>Faster!</color></size>\r\n>... [running]",
            ">... Not enough time!\r\n>... <size=200%><color=red>Move!</color></size>\r\n>... [sprinting]",
            ">... [hyperventilating]\r\n>... Calm down!\r\n>... <size=200%><color=red>Focus!</color></size>",
            ">... We're gonna die!\r\n>... <size=200%><color=red>Not yet!</color></size>\r\n>... Keep moving!",
            ">... Can't think!\r\n>... <size=200%><color=red>Just run!</color></size>\r\n>... [panting]",
            ">... Losing it!\r\n>... <size=200%><color=red>Hold together!</color></size>\r\n>... [groaning]",
            ">... No way out!\r\n>... <size=200%><color=red>Complete the sequence!</color></size>\r\n>... Trying!",
            ">... [screaming]\r\n>... <size=200%><color=red>Get it together!</color></size>\r\n>... [panting]",
            ">... Too much pressure!\r\n>... <size=200%><color=red>Deal with it!</color></size>\r\n>... [groaning]",
            ">... Breaking down!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [crying]",
            ">... Can't breathe!\r\n>... <size=200%><color=red>Run anyway!</color></size>\r\n>... [gasping]",
            ">... Hands shaking!\r\n>... <size=200%><color=red>Type it!</color></size>\r\n>... [typing]",
            ">... Vision blurring!\r\n>... <size=200%><color=red>Almost there!</color></size>\r\n>... [panting]",
            ">... Heart pounding!\r\n>... <size=200%><color=red>Keep moving!</color></size>\r\n>... [running]",
            ">... [whimpering]\r\n>... <size=200%><color=red>Don't stop!</color></size>\r\n>... [panting]",
            ">... Legs giving out!\r\n>... <size=200%><color=red>Push through!</color></size>\r\n>... [groaning]",
            ">... Can't... go... on...\r\n>... <size=200%><color=red>Yes you can!</color></size>\r\n>... [panting]",
            ">... [sobbing]\r\n>... <size=200%><color=red>Focus!</color></size>\r\n>... [sniffling]",

            // Timed Mechanics (Static)
            ">... Three twenty to find it!\r\n>... <size=200%><color=red>Then ten seconds back!</color></size>\r\n>... Insane!",
            ">... First timer running!\r\n>... <size=200%><color=red>Search!</color></size>\r\n>... [running]",
            ">... Timer override!\r\n>... Ten seconds now!\r\n>... <size=200%><color=red>Run!</color></size>",
            ">... Two timers!\r\n>... <size=200%><color=red>First one's generous!</color></size>\r\n>... Second one's brutal!",
            ">... How much time left?\r\n>... <size=200%><color=red>Minutes!</color></size>\r\n>... Keep searching!",
            ">... Time running out!\r\n>... <size=200%><color=red>Haven't found it yet!</color></size>\r\n>... Faster!",
            ">... Timer started!\r\n>... <size=200%><color=red>Clock's ticking!</color></size>\r\n>... Go!",
            ">... Time check!\r\n>... <size=200%><color=red>Under a minute!</color></size>\r\n>... Hurry!",
            ">... Countdown!\r\n>... <size=200%><color=red>Seconds!</color></size>\r\n>... [panic]",
            ">... Timer's critical!\r\n>... <size=200%><color=red>Almost zero!</color></size>\r\n>... [sprinting]",
            ">... First phase done!\r\n>... <size=200%><color=red>Ten second phase!</color></size>\r\n>... Sprint!",
            ">... Time constraint!\r\n>... <size=200%><color=red>Too tight!</color></size>\r\n>... Try anyway!",
            ">... Racing the clock!\r\n>... <size=200%><color=red>Losing!</color></size>\r\n>... [panting]",
            ">... Timer active!\r\n>... <size=200%><color=red>Move faster!</color></size>\r\n>... [running]",
            ">... Time limit!\r\n>... <size=200%><color=red>Unreasonable!</color></size>\r\n>... Do it anyway!",

            // Dynamic Messages - Using verificationRounds variable (50 messages)
            $">... {verificationRounds} rounds total!\r\n>... <size=200%><color=red>This is just the start!</color></size>\r\n>... [groaning]",
            $">... How many rounds?\r\n>... <size=200%><color=red>{verificationRounds}!</color></size>\r\n>... We'll never make it!",
            $">... {verificationRounds} times!\r\n>... <size=200%><color=red>We have to do this {verificationRounds} times?!</color></size>\r\n>... [panicking]",
            $">... Round one of {verificationRounds}!\r\n>... <size=200%><color=red>Long way to go!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} rounds ahead!\r\n>... <size=200%><color=red>Kill me now!</color></size>\r\n>... [frustrated]",
            $">... Only {verificationRounds} rounds!\r\n>... <size=200%><color=red>ONLY?!</color></size>\r\n>... That's too many!",
            $">... {verificationRounds} full sequences!\r\n>... <size=200%><color=red>Each with timers!</color></size>\r\n>... Impossible!",
            $">... We need {verificationRounds} completions!\r\n>... <size=200%><color=red>One's hard enough!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} rounds required!\r\n>... <size=200%><color=red>This is torture!</color></size>\r\n>... [panting]",
            $">... All {verificationRounds} rounds!\r\n>... <size=200%><color=red>Every single one!</color></size>\r\n>... No shortcuts!",
            $">... {verificationRounds} rounds to go!\r\n>... <size=200%><color=red>Each one harder!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} separate runs!\r\n>... <size=200%><color=red>Spawns each time!</color></size>\r\n>... [gunfire]",
            $">... Round count: {verificationRounds}!\r\n>... <size=200%><color=red>Can we do it?!</color></size>\r\n>... Have to try!",
            $">... {verificationRounds} rounds designed!\r\n>... <size=200%><color=red>Who designed this?!</color></size>\r\n>... Sadist!",
            $">... Warden wants {verificationRounds} rounds!\r\n>... <size=200%><color=red>Of course it does!</color></size>\r\n>... [frustrated]",
            $">... {verificationRounds} complete sequences!\r\n>... <size=200%><color=red>Getting harder each time!</color></size>\r\n>... [panting]",
            $">... {verificationRounds} rounds minimum!\r\n>... <size=200%><color=red>Could be more?!</color></size>\r\n>... No!",
            $">... Protocol requires {verificationRounds} rounds!\r\n>... <size=200%><color=red>Protocol's insane!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} verification rounds!\r\n>... <size=200%><color=red>All timed!</color></size>\r\n>... [panic]",
            $">... Must complete {verificationRounds}!\r\n>... <size=200%><color=red>No failures!</color></size>\r\n>... Pressure's on!",
            $">... {verificationRounds} rounds straight!\r\n>... <size=200%><color=red>No breaks!</color></size>\r\n>... Continuous spawns!",
            $">... Still {verificationRounds} to do!\r\n>... <size=200%><color=red>Haven't started!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} rounds configured!\r\n>... <size=200%><color=red>Warden special!</color></size>\r\n>... [frustrated]",
            $">... Need {verificationRounds} successes!\r\n>... <size=200%><color=red>One failure resets!</color></size>\r\n>... Careful!",
            $">... {verificationRounds} rounds programmed!\r\n>... <size=200%><color=red>Each one timed!</color></size>\r\n>... [panic]",
            $">... Count's {verificationRounds}!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... Do it anyway!",
            $">... {verificationRounds} full runs!\r\n>... <size=200%><color=red>With spawns!</color></size>\r\n>... [gunfire]",
            $">... Rounds total: {verificationRounds}!\r\n>... <size=200%><color=red>Brace yourselves!</color></size>\r\n>... [groaning]",
            $">... All {verificationRounds} required!\r\n>... <size=200%><color=red>No skipping!</color></size>\r\n>... [frustrated]",
            $">... {verificationRounds} round protocol!\r\n>... <size=200%><color=red>Brutal!</color></size>\r\n>... [panting]",
            $">... {verificationRounds} rounds set!\r\n>... <size=200%><color=red>Here we go!</color></size>\r\n>... [typing]",
            $">... Objective needs {verificationRounds}!\r\n>... <size=200%><color=red>Prepare yourselves!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} rounds locked in!\r\n>... <size=200%><color=red>No changing it!</color></size>\r\n>... [frustrated]",
            $">... System shows {verificationRounds} rounds!\r\n>... <size=200%><color=red>Not a mistake!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} iterations!\r\n>... <size=200%><color=red>Each one counts!</color></size>\r\n>... Focus!",
            $">... {verificationRounds} round sequence!\r\n>... <size=200%><color=red>Extended protocol!</color></size>\r\n>... [panting]",
            $">... Doing {verificationRounds} rounds!\r\n>... <size=200%><color=red>No choice!</color></size>\r\n>... [typing]",
            $">... {verificationRounds} rounds required!\r\n>... <size=200%><color=red>Warden's orders!</color></size>\r\n>... [frustrated]",
            $">... Round requirement: {verificationRounds}!\r\n>... <size=200%><color=red>That's final!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} rounds minimum!\r\n>... <size=200%><color=red>Could fail more!</color></size>\r\n>... [panic]",
            $">... We've got {verificationRounds} rounds!\r\n>... <size=200%><color=red>Make them count!</color></size>\r\n>... Go!",
            $">... Total rounds: {verificationRounds}!\r\n>... <size=200%><color=red>Let's do this!</color></size>\r\n>... [typing]",
            $">... {verificationRounds} complete runs needed!\r\n>... <size=200%><color=red>Each perfect!</color></size>\r\n>... [panting]",
            $">... Verification count: {verificationRounds}!\r\n>... <size=200%><color=red>Buckle up!</color></size>\r\n>... [groaning]",
            $">... {verificationRounds} rounds programmed!\r\n>... <size=200%><color=red>Protocol active!</color></size>\r\n>... [typing]",
            $">... Must do all {verificationRounds}!\r\n>... <size=200%><color=red>Every round!</color></size>\r\n>... No exceptions!",
            $">... {verificationRounds} rounds configured!\r\n>... <size=200%><color=red>System locked!</color></size>\r\n>... [frustrated]",
            $">... Round configuration: {verificationRounds}!\r\n>... <size=200%><color=red>Ready or not!</color></size>\r\n>... [panting]",
            $">... {verificationRounds} timed rounds!\r\n>... <size=200%><color=red>All critical!</color></size>\r\n>... Focus!",
            $">... Sequence length: {verificationRounds}!\r\n>... <size=200%><color=red>Extended!</color></size>\r\n>... [groaning]",

            // Dynamic Messages - Using terminalCount variable (50 messages)
            $">... {terminalCount} terminals!\r\n>... <size=200%><color=red>Which one?!</color></size>\r\n>... Check them all!",
            $">... {terminalCount} possible locations!\r\n>... <size=200%><color=red>Search every one!</color></size>\r\n>... Split up!",
            $">... How many terminals?\r\n>... <size=200%><color=red>{terminalCount}!</color></size>\r\n>... Too many!",
            $">... {terminalCount} terminals to check!\r\n>... <size=200%><color=red>Not enough time!</color></size>\r\n>... Try anyway!",
            $">... Could be any of {terminalCount}!\r\n>... <size=200%><color=red>Random each time!</color></size>\r\n>... [running]",
            $">... {terminalCount} terminal spread!\r\n>... <size=200%><color=red>Wide coverage!</color></size>\r\n>... Search pattern!",
            $">... Only {terminalCount} terminals!\r\n>... <size=200%><color=red>Still random!</color></size>\r\n>... [frustrated]",
            $">... {terminalCount} possibilities!\r\n>... <size=200%><color=red>One's correct!</color></size>\r\n>... Find it!",
            $">... Terminal count: {terminalCount}!\r\n>... <size=200%><color=red>Divide and search!</color></size>\r\n>... Go!",
            $">... We have {terminalCount} terminals!\r\n>... <size=200%><color=red>Check systematically!</color></size>\r\n>... [running]",
            $">... {terminalCount} terminal layout!\r\n>... <size=200%><color=red>Memorize locations!</color></size>\r\n>... Hurry!",
            $">... Split {terminalCount} ways!\r\n>... <size=200%><color=red>Cover all of them!</color></size>\r\n>... Search!",
            $">... {terminalCount} terminals available!\r\n>... <size=200%><color=red>One's parallel!</color></size>\r\n>... Which one?!",
            $">... {terminalCount} to search!\r\n>... <size=200%><color=red>Systematic approach!</color></size>\r\n>... Pattern!",
            $">... One of {terminalCount}!\r\n>... <size=200%><color=red>Find the right one!</color></size>\r\n>... [running]",
            $">... {terminalCount} terminal protocol!\r\n>... <size=200%><color=red>One central, rest random!</color></size>\r\n>... Search!",
            $">... Terminal spread: {terminalCount}!\r\n>... <size=200%><color=red>Check distances!</color></size>\r\n>... Plan routes!",
            $">... {terminalCount} terminals configured!\r\n>... <size=200%><color=red>Random selection!</color></size>\r\n>... [frustrated]",
            $">... Count's {terminalCount}!\r\n>... <size=200%><color=red>Plus central!</color></size>\r\n>... Check all!",
            $">... {terminalCount} possible parallels!\r\n>... <size=200%><color=red>Only one works!</color></size>\r\n>... Find it!",
            $">... We've got {terminalCount} terminals!\r\n>... <size=200%><color=red>Eliminate wrong ones!</color></size>\r\n>... [running]",
            $">... {terminalCount} terminals total!\r\n>... <size=200%><color=red>Search grid!</color></size>\r\n>... Pattern!",
            $">... {terminalCount} locations!\r\n>... <size=200%><color=red>One's correct!</color></size>\r\n>... Hurry!",
            $">... Terminal configuration: {terminalCount}!\r\n>... <size=200%><color=red>Scout them all!</color></size>\r\n>... Go!",
            $">... {terminalCount} terminals set!\r\n>... <size=200%><color=red>Which one's parallel?!</color></size>\r\n>... Search!",
            $">... One out of {terminalCount}!\r\n>... <size=200%><color=red>Odds aren't great!</color></size>\r\n>... Try all!",
            $">... {terminalCount} terminal system!\r\n>... <size=200%><color=red>Parallel's random!</color></size>\r\n>... [running]",
            $">... {terminalCount} to check!\r\n>... <size=200%><color=red>Not this one!</color></size>\r\n>... Next!",
            $">... {terminalCount} terminals active!\r\n>... <size=200%><color=red>Find parallel!</color></size>\r\n>... [sprinting]",
            $">... System has {terminalCount}!\r\n>... <size=200%><color=red>Random each attempt!</color></size>\r\n>... Search again!",
            $">... {terminalCount} terminal network!\r\n>... <size=200%><color=red>One's the key!</color></size>\r\n>... Find it!",
            $">... All {terminalCount} terminals!\r\n>... <size=200%><color=red>Check every one!</color></size>\r\n>... [running]",
            $">... {terminalCount} possibilities exist!\r\n>... <size=200%><color=red>Narrow it down!</color></size>\r\n>... Faster!",
            $">... Terminal pool: {terminalCount}!\r\n>... <size=200%><color=red>One's selected!</color></size>\r\n>... Which?!",
            $">... {terminalCount} in the area!\r\n>... <size=200%><color=red>Search zone by zone!</color></size>\r\n>... Go!",
            $">... We need to check {terminalCount}!\r\n>... <size=200%><color=red>Time's limited!</color></size>\r\n>... Hurry!",
            $">... {terminalCount} terminals online!\r\n>... <size=200%><color=red>One's correct!</color></size>\r\n>... [running]",
            $">... One of {terminalCount} works!\r\n>... <size=200%><color=red>Test them!</color></size>\r\n>... [typing]",
            $">... {terminalCount} terminal zone!\r\n>... <size=200%><color=red>Cover all areas!</color></size>\r\n>... Search!",
            $">... Count: {terminalCount} terminals!\r\n>... <size=200%><color=red>Systematic!</color></size>\r\n>... Don't miss any!",
            $">... {terminalCount} terminals installed!\r\n>... <size=200%><color=red>Random parallel!</color></size>\r\n>... Find it!",
            $">... Pool of {terminalCount}!\r\n>... <size=200%><color=red>Narrow down fast!</color></size>\r\n>... [running]",
            $">... {terminalCount} terminals mapped!\r\n>... <size=200%><color=red>Check your assignments!</color></size>\r\n>... Go!",
            $">... {terminalCount} in play!\r\n>... <size=200%><color=red>One's the target!</color></size>\r\n>... Search!",
            $">... {terminalCount} terminal array!\r\n>... <size=200%><color=red>Parallel's hidden!</color></size>\r\n>... Find it!",
            $">... We're checking {terminalCount}!\r\n>... <size=200%><color=red>Split the work!</color></size>\r\n>... [running]",
            $">... {terminalCount} nodes active!\r\n>... <size=200%><color=red>One's parallel!</color></size>\r\n>... Which one?!",
            $">... {terminalCount} terminals ready!\r\n>... <size=200%><color=red>Search begins!</color></size>\r\n>... Go!",
            $">... Array size: {terminalCount}!\r\n>... <size=200%><color=red>Find the match!</color></size>\r\n>... [running]",
            $">... {terminalCount} available!\r\n>... <size=200%><color=red>One's right!</color></size>\r\n>... Test all!",

            // Dynamic Messages - Using midRound variable with verificationRounds (25 messages)
            $">... Round {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [panting]",
            $">... {midRound} down!\r\n>... <size=200%><color=red>{verificationRounds} total!</color></size>\r\n>... More to go!",
            $">... Round {midRound} complete!\r\n>... Still need {verificationRounds}!\r\n>... <size=200%><color=red>Not done!</color></size>",
            $">... We're on round {midRound}!\r\n>... <size=200%><color=red>{verificationRounds} required!</color></size>\r\n>... Keep pushing!",
            $">... {midRound} of {verificationRounds} done!\r\n>... <size=200%><color=red>Halfway there!</color></size>\r\n>... [panting]",
            $">... Round {midRound}!\r\n>... <size=200%><color=red>Out of {verificationRounds}!</color></size>\r\n>... Progress!",
            $">... {midRound} rounds in!\r\n>... <size=200%><color=red>{verificationRounds} to finish!</color></size>\r\n>... [groaning]",
            $">... Round count: {midRound}!\r\n>... Total: {verificationRounds}!\r\n>... <size=200%><color=red>Keep moving!</color></size>",
            $">... We're at {midRound}!\r\n>... <size=200%><color=red>Need {verificationRounds}!</color></size>\r\n>... Continue!",
            $">... {midRound} completed!\r\n>... <size=200%><color=red>{verificationRounds} required!</color></size>\r\n>... [panting]",
            $">... Round {midRound} active!\r\n>... Of {verificationRounds} total!\r\n>... <size=200%><color=red>Focus!</color></size>",
            $">... {midRound} down, more to go!\r\n>... <size=200%><color=red>{verificationRounds} rounds total!</color></size>\r\n>... [groaning]",
            $">... On round {midRound}!\r\n>... <size=200%><color=red>Out of {verificationRounds}!</color></size>\r\n>... [panting]",
            $">... Progress: {midRound}!\r\n>... Goal: {verificationRounds}!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {midRound} rounds done!\r\n>... <size=200%><color=red>Need {verificationRounds}!</color></size>\r\n>... Continue!",
            $">... Round {midRound} starting!\r\n>... {verificationRounds} total rounds!\r\n>... <size=200%><color=red>Here we go!</color></size>",
            $">... We're {midRound} in!\r\n>... <size=200%><color=red>{verificationRounds} to complete!</color></size>\r\n>... [panting]",
            $">... Round {midRound} underway!\r\n>... Of {verificationRounds}!\r\n>... <size=200%><color=red>Don't stop!</color></size>",
            $">... {midRound} finished!\r\n>... <size=200%><color=red>{verificationRounds} needed!</color></size>\r\n>... [groaning]",
            $">... Current: round {midRound}!\r\n>... Target: {verificationRounds}!\r\n>... <size=200%><color=red>Keep pushing!</color></size>",
            $">... Round {midRound} active!\r\n>... <size=200%><color=red>Out of {verificationRounds}!</color></size>\r\n>... Focus!",
            $">... {midRound} rounds complete!\r\n>... {verificationRounds} required!\r\n>... <size=200%><color=red>Continue!</color></size>",
            $">... We're on {midRound}!\r\n>... <size=200%><color=red>Need {verificationRounds} total!</color></size>\r\n>... [panting]",
            $">... Round {midRound} now!\r\n>... Of {verificationRounds}!\r\n>... <size=200%><color=red>Keep it up!</color></size>",
            $">... {midRound} down!\r\n>... <size=200%><color=red>{verificationRounds} to go!</color></size>\r\n>... [groaning]",

            // Dynamic Messages - Complex combinations (25 messages)
            $">... {terminalCount} terminals!\r\n>... <size=200%><color=red>{verificationRounds} rounds!</color></size>\r\n>... This is hell!",
            $">... Round {midRound}!\r\n>... {terminalCount} terminals to check!\r\n>... <size=200%><color=red>Again!</color></size>",
            $">... {verificationRounds} rounds!\r\n>... <size=200%><color=red>{terminalCount} terminals each!</color></size>\r\n>... [groaning]",
            $">... {midRound} of {verificationRounds}!\r\n>... {terminalCount} terminal search!\r\n>... <size=200%><color=red>Hurry!</color></size>",
            $">... {terminalCount} terminals!\r\n>... Round {midRound}!\r\n>... <size=200%><color=red>{verificationRounds} total!</color></size>",
            $">... Round {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>{terminalCount} to search!</color></size>\r\n>... [running]",
            $">... {verificationRounds} rounds configured!\r\n>... <size=200%><color=red>{terminalCount} terminals!</color></size>\r\n>... Brutal!",
            $">... {terminalCount} terminal check!\r\n>... Round {midRound}!\r\n>... <size=200%><color=red>Of {verificationRounds}!</color></size>",
            $">... We're at {midRound}!\r\n>... {terminalCount} terminals!\r\n>... <size=200%><color=red>{verificationRounds} rounds!</color></size>",
            $">... {verificationRounds} rounds!\r\n>... {terminalCount} terminals!\r\n>... <size=200%><color=red>Too much!</color></size>",
            $">... Round {midRound}!\r\n>... <size=200%><color=red>{terminalCount} terminals again!</color></size>\r\n>... Every round!",
            $">... {terminalCount} possibilities!\r\n>... Round {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>Search!</color></size>",
            $">... {midRound} down!\r\n>... <size=200%><color=red>{terminalCount} terminals!</color></size>\r\n>... {verificationRounds} total!",
            $">... Terminal count: {terminalCount}!\r\n>... Round: {midRound}!\r\n>... <size=200%><color=red>Total: {verificationRounds}!</color></size>",
            $">... {verificationRounds} rounds planned!\r\n>... {terminalCount} terminals!\r\n>... <size=200%><color=red>Round {midRound} now!</color></size>",
            $">... {terminalCount} to check!\r\n>... <size=200%><color=red>Round {midRound} of {verificationRounds}!</color></size>\r\n>... Go!",
            $">... Round {midRound}!\r\n>... {verificationRounds} total!\r\n>... <size=200%><color=red>{terminalCount} searches!</color></size>",
            $">... {terminalCount} terminals!\r\n>... <size=200%><color=red>{midRound} of {verificationRounds}!</color></size>\r\n>... [panting]",
            $">... We're on {midRound}!\r\n>... {terminalCount} terminals!\r\n>... <size=200%><color=red>{verificationRounds} rounds!</color></size>",
            $">... {verificationRounds} rounds!\r\n>... <size=200%><color=red>Round {midRound}!</color></size>\r\n>... {terminalCount} terminals!",
            $">... {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>{terminalCount} terminal pool!</color></size>\r\n>... Search!",
            $">... {terminalCount} terminals configured!\r\n>... <size=200%><color=red>Round {midRound}!</color></size>\r\n>... {verificationRounds} total!",
            $">... Round {midRound} active!\r\n>... {terminalCount} to search!\r\n>... <size=200%><color=red>{verificationRounds} needed!</color></size>",
            $">... {verificationRounds} verification rounds!\r\n>... {terminalCount} terminal array!\r\n>... <size=200%><color=red>Round {midRound}!</color></size>",
            $">... {midRound} complete!\r\n>... {terminalCount} terminals!\r\n>... <size=200%><color=red>{verificationRounds} required!</color></size>",
        }))!);
        #endregion
    }
}
