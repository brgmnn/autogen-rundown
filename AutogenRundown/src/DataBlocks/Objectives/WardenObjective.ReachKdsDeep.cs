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
        director.DisableStartingArea = true;

        SubType = Generator.Pick(new List<WardenObjectiveSubType>
        {
            WardenObjectiveSubType.ErrorAlarmChase
        });

        KdsDeepUnit = Generator.Between(1, 9);
    }

    public void Build_ReachKdsDeep(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Reach and enter KDS Deep");
        // FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";

        dataLayer.ObjectiveData.WinCondition = WardenObjectiveWinCondition.GoToExitGeo;

        switch (SubType)
        {
            case WardenObjectiveSubType.ErrorAlarmChase:
            {
                EventsOnElevatorLand
                    .AddSound(Sound.R8E1_ErrorAlarm, 2.0, WardenObjectiveEventTrigger.None)
                    .AddSound(
                        Sound.R8E1_GargantaWarning,
                        subtitle: 442824023,
                        delay: 7.0,
                        trigger: WardenObjectiveEventTrigger.None)
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = (WaveSettings.Error_VeryHard with
                            {
                                FilterType = PopulationFilterType.Include,
                                PopulationFilter = new List<Enemies.EnemyType>
                                {
                                    Enemies.EnemyType.Standard,
                                    Enemies.EnemyType.MiniBoss
                                },
                                OverrideWaveSpawnType = true,
                                SurvivalWaveSpawnType = Enemies.SurvivalWaveSpawnType.FromElevatorDirection,
                            }).FindOrPersist(),
                            Population = WavePopulation.Baseline_Hybrids,
                            SpawnDelay = 0.0,
                            TriggerAlarm = true
                        }, 30.0);
                break;
            }
        }

        if (level.HasOverload)
        {
            var overload = level.Planner.GetBulkheadFirstZone(Bulkhead.Overload);

            if (overload != null)
            {
                var overloadZone = level.Planner.GetZone((ZoneNode)overload);

                if (overloadZone != null)
                {
                    Plugin.Logger.LogDebug($"Cycling fog");
                    overloadZone.EventsOnOpenDoor.AddCyclingFog(
                        Fog.FullFog,
                        level.FogSettings,
                        2);
                }
            }
        }

        // Set type to empty
        Type = WardenObjectiveType.Empty;
    }

    private void PostBuildIntel_ReachKdsDeep(Level level)
    {
        #region Warden Intel Messages
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
        {
            // Mission Start / ErrorAlarmChase (30 messages)
            ">... [error alarm blaring]\r\n>... What's that sound?\r\n>... <size=200%><color=red>Something's coming!</color></size>",
            ">... Garganta...\r\n>... <size=200%><color=red>It knows we're here!</color></size>\r\n>... [distant roar]",
            ">... [alarm wailing]\r\n>... From the elevator!\r\n>... <size=200%><color=red>They're spawning behind us!</color></size>",
            ">... We just landed!\r\n>... <size=200%><color=red>No time to prepare!</color></size>\r\n>... [frantic footsteps]",
            ">... The warning...\r\n>... <size=200%><color=red>Garganta is here!</color></size>\r\n>... We need to move!",
            ">... [static crackling]\r\n>... Error alarm?\r\n>... <size=200%><color=red>Already?!</color></size>",
            ">... Thirty seconds!\r\n>... <size=200%><color=red>Wave incoming from elevator!</color></size>\r\n>... Get ready!",
            ">... No safe zone!\r\n>... <size=200%><color=red>Run forward!</color></size>\r\n>... Don't stop!",
            ">... [mechanical grinding]\r\n>... The elevator shaft...\r\n>... <size=200%><color=red>They're climbing up!</color></size>",
            ">... <size=200%><color=red>Garganta warning!</color></size>\r\n>... What does that mean?\r\n>... Nothing good...",
            ">... We're not alone.\r\n>... <size=200%><color=red>The facility knows!</color></size>\r\n>... Automated defenses!",
            ">... [alarm continues]\r\n>... <size=200%><color=red>It won't stop!</color></size>\r\n>... Can't turn it off!",
            ">... From behind!\r\n>... <size=200%><color=red>Hybrids from the elevator!</color></size>\r\n>... Keep moving!",
            ">... No time!\r\n>... <size=200%><color=red>They're already here!</color></size>\r\n>... [gunfire erupts]",
            ">... The warning said Garganta...\r\n>... What is that?\r\n>... <size=200%><color=red>Something ancient!</color></size>",
            ">... [whispering] Don't look back.\r\n>... <size=200%><color=red>Just run!</color></size>\r\n>... They're gaining!",
            ">... Error alarm chase!\r\n>... <size=200%><color=red>Waves behind us!</color></size>\r\n>... Forward! Go!",
            ">... This facility...\r\n>... <size=200%><color=red>It's awake!</color></size>\r\n>... Everything's active!",
            ">... [breathing heavily]\r\n>... Can't catch our breath.\r\n>... <size=200%><color=red>No stopping!</color></size>",
            ">... The alarm!\r\n>... It's drawing them!\r\n>... <size=200%><color=red>Move faster!</color></size>",
            ">... Automated response!\r\n>... <size=200%><color=red>Defenses activated!</color></size>\r\n>... We triggered everything!",
            ">... [distant shrieking]\r\n>... <size=200%><color=red>Garganta...</color></size>\r\n>... It's down here somewhere.",
            ">... No preparation time!\r\n>... <size=200%><color=red>Hit the ground running!</color></size>\r\n>... Literally!",
            ">... They knew we were coming!\r\n>... <size=200%><color=red>It's a trap!</color></size>\r\n>... No... automated.",
            ">... [alarm echoing]\r\n>... Throughout the whole facility!\r\n>... <size=200%><color=red>Everywhere!</color></size>",
            ">... Elevator wave at thirty!\r\n>... <size=200%><color=red>I count twenty contacts!</color></size>\r\n>... More!",
            ">... Can't defend here!\r\n>... <size=200%><color=red>The starting zone is compromised!</color></size>\r\n>... Push forward!",
            ">... [static burst]\r\n>... Garganta warning...\r\n>... <size=200%><color=red>We shouldn't be here!</color></size>",
            ">... No rest.\r\n>... <size=200%><color=red>No mercy!</color></size>\r\n>... From the moment we land!",
            ">... The whole facility knows!\r\n>... <size=200%><color=red>Alarms everywhere!</color></size>\r\n>... [blaring continues]",

            // Early Corridor Traverse (25 messages)
            ">... These corridors...\r\n>... <size=200%><color=red>Too quiet!</color></size>\r\n>... Where are they?",
            ">... Limited ammo here.\r\n>... Conserve it!\r\n>... <size=200%><color=red>We'll need every round!</color></size>",
            ">... [whispering] Shadows!\r\n>... <size=200%><color=red>At least twenty!</color></size>\r\n>... Get down!",
            ">... Pouncer!\r\n>... <size=200%><color=red>It's hunting us!</color></size>\r\n>... Watch the ceiling!",
            ">... No resources in these corridors.\r\n>... <size=200%><color=red>We're running on empty!</color></size>\r\n>... Keep pushing.",
            ">... [distant footsteps]\r\n>... Something's ahead.\r\n>... <size=200%><color=red>Shadow wave!</color></size>",
            ">... The lights...\r\n>... They're dimming.\r\n>... <size=200%><color=red>Power failure?</color></size>",
            ">... Wave at forty percent!\r\n>... <size=200%><color=red>Shadows incoming!</color></size>\r\n>... Brace!",
            ">... These halls go on forever.\r\n>... <size=200%><color=red>Where's the hub?</color></size>\r\n>... [exhausted breathing]",
            ">... Pouncer scout detected!\r\n>... <size=200%><color=red>Don't let it grab anyone!</color></size>\r\n>... Focus fire!",
            ">... Save your ammo.\r\n>... <size=200%><color=red>We don't have much!</color></size>\r\n>... Melee when possible.",
            ">... [shuffling sounds]\r\n>... Sleepers ahead?\r\n>... <size=200%><color=red>No... Shadows!</color></size>",
            ">... The corridor splits.\r\n>... Which way?\r\n>... <size=200%><color=red>Both look bad!</color></size>",
            ">... Wave spawn!\r\n>... <size=200%><color=red>Twenty shadow contacts!</color></size>\r\n>... Here they come!",
            ">... No health packs.\r\n>... <size=200%><color=red>No tools!</color></size>\r\n>... These corridors are stripped.",
            ">... [scuttling]\r\n>... Ceiling movement!\r\n>... <size=200%><color=red>Pouncer above!</color></size>",
            ">... Giant Shooters!\r\n>... <size=200%><color=red>Six of them?!</color></size>\r\n>... Take cover!",
            ">... Eighty-five percent traverse.\r\n>... <size=200%><color=red>Giants spawning!</color></size>\r\n>... Suppress them!",
            ">... [heavy footfalls]\r\n>... Those aren't Strikers...\r\n>... <size=200%><color=red>Giants!</color></size>",
            ">... We're exposed here!\r\n>... <size=200%><color=red>No cover!</color></size>\r\n>... Keep moving!",
            ">... Pouncer Shadow!\r\n>... <size=200%><color=red>High tier variant!</color></size>\r\n>... Ninety percent mark!",
            ">... These narrow corridors...\r\n>... Perfect for them.\r\n>... <size=200%><color=red>Terrible for us!</color></size>",
            ">... [distant snarling]\r\n>... More coming.\r\n>... <size=200%><color=red>Always more!</color></size>",
            ">... The walls are closing in.\r\n>... <size=200%><color=red>Claustrophobic!</color></size>\r\n>... Stay focused!",
            ">... Shadow teleport!\r\n>... Behind us!\r\n>... <size=200%><color=red>Turn around!</color></size>",

            // Hub Puzzles (20 messages)
            ">... Hub zone ahead!\r\n>... <size=200%><color=red>Resources!</color></size>\r\n>... Finally!",
            ">... Keycard puzzle.\r\n>... <size=200%><color=red>Find the card!</color></size>\r\n>... Split up!",
            ">... Generator power-up.\r\n>... <size=200%><color=red>We need cells!</color></size>\r\n>... Check the boxes!",
            ">... Terminal unlock required.\r\n>... [typing frantically]\r\n>... <size=200%><color=red>What's the command?!</color></size>",
            ">... Stock up here!\r\n>... <size=200%><color=red>Take everything!</color></size>\r\n>... We won't get another chance!",
            ">... Health packs!\r\n>... Tools!\r\n>... <size=200%><color=red>Ammo distribution three-point-oh!</color></size>",
            ">... [searching frantically]\r\n>... Where's the keycard?\r\n>... <size=200%><color=red>Check every corner!</color></size>",
            ">... Generators need power.\r\n>... <size=200%><color=red>Two cells minimum!</color></size>\r\n>... Find them!",
            ">... The terminal's locked.\r\n>... <size=200%><color=red>Access denied!</color></size>\r\n>... Try another command!",
            ">... This is a dig site hub.\r\n>... <size=200%><color=red>Multiple connections!</color></size>\r\n>... Which path?",
            ">... [whispering] Take your time here.\r\n>... <size=200%><color=red>Load up!</color></size>\r\n>... Next section is brutal.",
            ">... Found the keycard!\r\n>... <size=200%><color=red>Door's unlocking!</color></size>\r\n>... Move out!",
            ">... Generators online!\r\n>... Security door opening.\r\n>... <size=200%><color=red>Let's go!</color></size>",
            ">... [terminal beeping]\r\n>... Access granted!\r\n>... <size=200%><color=red>Puzzle solved!</color></size>",
            ">... Team coordination!\r\n>... <size=200%><color=red>One searches, others defend!</color></size>\r\n>... Standard protocol!",
            ">... This hub has three exits.\r\n>... <size=200%><color=red>Which one leads deeper?</color></size>\r\n>... Check the terminal.",
            ">... Random puzzle type.\r\n>... Could be any of three.\r\n>... <size=200%><color=red>Adapt!</color></size>",
            ">... [boxes opening]\r\n>... Ammunition!\r\n>... <size=200%><color=red>Tools and meds!</color></size>",
            ">... Storage hub ahead.\r\n>... Heavy resources!\r\n>... <size=200%><color=red>Jackpot!</color></size>",
            ">... Don't rush the puzzle.\r\n>... <size=200%><color=red>Mistakes cost time!</color></size>\r\n>... Do it right.",

            // Mid-Challenge Paths - Terminal Path (20 messages)
            ">... Security terminal located!\r\n>... <size=200%><color=red>It's in the dead-end zone!</color></size>\r\n>... Moving to it!",
            ">... [typing] KDS-DEEP...\r\n>... <size=200%><color=red>DEACTIVATE_DEFENSE!</color></size>\r\n>... [enter key]",
            ">... Authenticating with BIOCOM...\r\n>... <size=200%><color=red>Come on...</color></size>\r\n>... [spinner loading]",
            ">... Terminal ID confirmed.\r\n>... Operative credentials...\r\n>... <size=200%><color=red>Accepted!</color></size>",
            ">... Deactivating defense grid!\r\n>... <size=200%><color=red>KDS Deep systems...</color></size>\r\n>... Going offline!",
            ">... Defense grid inactive!\r\n>... <size=200%><color=red>Admin door unlocking!</color></size>\r\n>... Eleven seconds!",
            ">... [alarm chirping]\r\n>... Shadow wave spawning!\r\n>... <size=200%><color=red>Twenty-five second delay!</color></size>",
            ">... Proceed to KDS Deep.\r\n>... <size=200%><color=red>Objective updated!</color></size>\r\n>... Let's move!",
            ">... The security hub...\r\n>... Three connections.\r\n>... <size=200%><color=red>Heavy resources!</color></size>",
            ">... Terminal path chosen.\r\n>... <size=200%><color=red>Controlled timing!</color></size>\r\n>... We pick when to proceed.",
            ">... [terminal hum]\r\n>... Access codes required.\r\n>... <size=200%><color=red>Execute the command!</color></size>",
            ">... KDS Defense grid...\r\n>... <size=200%><color=red>Shutting down!</color></size>\r\n>... We did it!",
            ">... Admin-locked door!\r\n>... <size=200%><color=red>Countdown to unlock!</color></size>\r\n>... Clear the area!",
            ">... Shadow spawn in three...\r\n>... Two...\r\n>... <size=200%><color=red>One!</color></size>",
            ">... [gunfire]\r\n>... Shadows everywhere!\r\n>... <size=200%><color=red>Command triggered them!</color></size>",
            ">... Door's unlocking!\r\n>... <size=200%><color=red>Eleven seconds up!</color></size>\r\n>... Move through!",
            ">... Terminal unlocked it.\r\n>... Better than fighting.\r\n>... <size=200%><color=red>Smart choice!</color></size>",
            ">... Security zone clear!\r\n>... <size=200%><color=red>Advancing to KDS Deep!</color></size>\r\n>... Stay sharp!",
            ">... The terminal worked!\r\n>... <size=200%><color=red>Defense grid offline!</color></size>\r\n>... Path is clear!",
            ">... [beeping confirmation]\r\n>... Command executed.\r\n>... <size=200%><color=red>Systems responding!</color></size>",

            // Mid-Challenge Paths - Holdout Path (20 messages)
            ">... Time-locked door!\r\n>... <size=200%><color=red>Security scan required!</color></size>\r\n>... Get in position!",
            ">... Scan alarm!\r\n>... Class Two for C-tier!\r\n>... <size=200%><color=red>Forty-five seconds!</color></size>",
            ">... [alarm blaring]\r\n>... Class Three!\r\n>... <size=200%><color=red>Sixty seconds of hell!</color></size>",
            ">... Class Four scan!\r\n>... <size=200%><color=red>Ninety seconds!</color></size>\r\n>... E-tier difficulty!",
            ">... Boss spawn in fifteen!\r\n>... <size=200%><color=red>Tank incoming!</color></size>\r\n>... Prepare burst damage!",
            ">... [heavy breathing]\r\n>... Thirty seconds in...\r\n>... <size=200%><color=red>Mother spawning!</color></size>",
            ">... Mixed wave!\r\n>... Forty percent standard!\r\n>... <size=200%><color=red>Thirty chargers!</color></size>",
            ">... Nightmare variants!\r\n>... <size=200%><color=red>Thirty percent!</color></size>\r\n>... They're everywhere!",
            ">... Hold the scan!\r\n>... <size=200%><color=red>Don't leave the zone!</color></size>\r\n>... Almost there!",
            ">... Tank boss!\r\n>... Back tumors!\r\n>... <size=200%><color=red>Weak point!</color></size>",
            ">... [roaring]\r\n>... Mother!\r\n>... <size=200%><color=red>Kill the occiput!</color></size>",
            ">... Countdown!\r\n>... <size=200%><color=red>Twenty seconds left!</color></size>\r\n>... Hold position!",
            ">... Lockdown complete!\r\n>... <size=200%><color=red>Door unlocking!</color></size>\r\n>... We survived!",
            ">... [gasping]\r\n>... That was close.\r\n>... <size=200%><color=red>Too close!</color></size>",
            ">... Holdout path.\r\n>... <size=200%><color=red>More resources this way!</color></size>\r\n>... Worth the fight!",
            ">... Boss at twenty seconds!\r\n>... <size=200%><color=red>Get ready!</color></size>\r\n>... Here it comes!",
            ">... Scan percentage rising!\r\n>... <size=200%><color=red>Eighty percent!</color></size>\r\n>... Almost done!",
            ">... [countdown beeping]\r\n>... Ten!\r\n>... <size=200%><color=red>Nine!</color></size>",
            ">... Security lockdown!\r\n>... <size=200%><color=red>Timed scan alarm!</color></size>\r\n>... Defend!",
            ">... Staging hub!\r\n>... Heavy resources!\r\n>... <size=200%><color=red>Then hell breaks loose!</color></size>",

            // Final Approach (25 messages)
            ">... Final corridors ahead.\r\n>... <size=200%><color=red>Last stretch!</color></size>\r\n>... Limited resources.",
            ">... Tank Pouncer!\r\n>... <size=200%><color=red>Twenty second delay!</color></size>\r\n>... Door just opened!",
            ">... Giant Shooters spawning!\r\n>... <size=200%><color=red>Two-minute delay!</color></size>\r\n>... We have time!",
            ">... Pouncer Shadow!\r\n>... Three minutes!\r\n>... <size=200%><color=red>Don't wait for it!</color></size>",
            ">... Final hub zone!\r\n>... <size=200%><color=red>Last resources!</color></size>\r\n>... Take everything!",
            ">... [whispering] This is it.\r\n>... Final stockpile.\r\n>... <size=200%><color=red>No more after this!</color></size>",
            ">... Health four-point-oh!\r\n>... <size=200%><color=red>Tools eight!</color></size>\r\n>... Ammo eight!",
            ">... General hub puzzle.\r\n>... <size=200%><color=red>Last checkpoint!</color></size>\r\n>... Make it count!",
            ">... Prepare for the finale.\r\n>... <size=200%><color=red>No extraction ahead!</color></size>\r\n>... Just survival.",
            ">... [taking deep breath]\r\n>... Load up.\r\n>... <size=200%><color=red>Every tool matters!</color></size>",
            ">... These corridors feel wrong.\r\n>... Too quiet.\r\n>... <size=200%><color=red>Storm before the calm!</color></size>",
            ">... Minimal ammo distribution.\r\n>... <size=200%><color=red>Only two-point-oh!</color></size>\r\n>... Conserve!",
            ">... Tank, Pouncer hunting!\r\n>... <size=200%><color=red>It's tracking us!</color></size>\r\n>... Move fast!",
            ">... Don't waste resources here!\r\n>... <size=200%><color=red>Delayed spawns!</color></size>\r\n>... We'll be gone!",
            ">... Final hub reached.\r\n>... <size=200%><color=red>Discuss strategy!</color></size>\r\n>... Survival roles!",
            ">... Assign positions now!\r\n>... <size=200%><color=red>Defensive formation!</color></size>\r\n>... Who holds where?",
            ">... Full ammo.\r\n>... Full tools.\r\n>... <size=200%><color=red>This is it!</color></size>",
            ">... [checking gear]\r\n>... Mines?\r\n>... <size=200%><color=red>Check!</color></size>",
            ">... C-foam ready?\r\n>... Sentries deployed?\r\n>... <size=200%><color=red>All set!</color></size>",
            ">... The path ahead...\r\n>... <size=200%><color=red>Leads to KDS Deep!</color></size>\r\n>... No turning back.",
            ">... [nervous breathing]\r\n>... We ready?\r\n>... <size=200%><color=red>As we'll ever be!</color></size>",
            ">... Push through these corridors!\r\n>... <size=200%><color=red>Final approach!</color></size>\r\n>... Almost there!",
            ">... Three connections here.\r\n>... Random puzzle.\r\n>... <size=200%><color=red>Last one!</color></size>",
            ">... No going back now.\r\n>... <size=200%><color=red>Forward only!</color></size>\r\n>... To KDS Deep!",
            ">... [equipment rattling]\r\n>... Everyone ready?\r\n>... <size=200%><color=red>Let's finish this!</color></size>",

            // Reactor Explosion (50 messages)
            ">... [distant rumbling]\r\n>... What's that sound?\r\n>... <size=200%><color=red>From ahead...</color></size>",
            ">... Machinery blow!\r\n>... <size=200%><color=red>The reactor!</color></size>\r\n>... Something's failing!",
            ">... [mechanical groaning]\r\n>... KDS Deep...\r\n>... <size=200%><color=red>The reactor's going!</color></size>",
            ">... Seventeen seconds!\r\n>... <size=200%><color=red>What's shaking!</color></size>\r\n>... [violent tremor]",
            ">... The ground!\r\n>... <size=200%><color=red>Earthquake!</color></size>\r\n>... It's not an earthquake!",
            ">... [violent shaking]\r\n>... <size=200%><color=red>Reactor explosion!</color></size>\r\n>... Ahead of us!",
            ">... The lights!\r\n>... <size=200%><color=red>They're going out!</color></size>\r\n>... [darkness descends]",
            ">... [complete darkness]\r\n>... I can't see!\r\n>... <size=200%><color=red>Anything!</color></size>",
            ">... Flashlights!\r\n>... <size=200%><color=red>Now!</color></size>\r\n>... Turn them on!",
            ">... [fumbling]\r\n>... Where's my light?\r\n>... <size=200%><color=red>I'm blind!</color></size>",
            ">... Power failure!\r\n>... <size=200%><color=red>System-wide!</color></size>\r\n>... [alarm wailing]",
            ">... [powerdown sound]\r\n>... Everything's dying!\r\n>... <size=200%><color=red>Total blackout!</color></size>",
            ">... Stay together!\r\n>... <size=200%><color=red>Don't move!</color></size>\r\n>... Call out positions!",
            ">... [whispering] I'm here!\r\n>... Where are you?\r\n>... <size=200%><color=red>I can't see you!</color></size>",
            ">... Don't panic!\r\n>... <size=200%><color=red>Auxiliary power coming!</color></size>\r\n>... Wait for it!",
            ">... How long in darkness?\r\n>... <size=200%><color=red>Four seconds!</color></size>\r\n>... Feels like forever!",
            ">... [lights humming]\r\n>... Auxiliary power!\r\n>... <size=200%><color=red>Activating!</color></size>",
            ">... Red lighting!\r\n>... <size=200%><color=red>Emergency systems!</color></size>\r\n>... We can see!",
            ">... [loud poweron]\r\n>... Backup power online!\r\n>... <size=200%><color=red>Thank god!</color></size>",
            ">... The reactor ahead...\r\n>... <size=200%><color=red>It's already failed!</color></size>\r\n>... We're too late!",
            ">... This isn't behind us.\r\n>... <size=200%><color=red>It's ahead!</color></size>\r\n>... The explosion's forward!",
            ">... [realization]\r\n>... KDS Deep...\r\n>... <size=200%><color=red>Already destroyed!</color></size>",
            ">... The zones we're in!\r\n>... <size=200%><color=red>Losing power!</color></size>\r\n>... Not behind us!",
            ">... Corridor One dark!\r\n>... <size=200%><color=red>Corridor Two dark!</color></size>\r\n>... We're in them!",
            ">... [breathing heavily]\r\n>... Navigate blind!\r\n>... <size=200%><color=red>Four seconds!</color></size>",
            ">... Emergency lighting only.\r\n>... <size=200%><color=red>Red glow!</color></size>\r\n>... Dim as hell!",
            ">... The reactor failed ahead!\r\n>... <size=200%><color=red>That means...</color></size>\r\n>... No extraction!",
            ">... [static crackling]\r\n>... Systems failing everywhere!\r\n>... <size=200%><color=red>Catastrophic!</color></size>",
            ">... Auxiliary power's all we have!\r\n>... <size=200%><color=red>Main reactor's gone!</color></size>\r\n>... Keep moving!",
            ">... The facility's dying!\r\n>... <size=200%><color=red>All around us!</color></size>\r\n>... We're inside it!",
            ">... [distant explosion]\r\n>... KDS Deep reactor!\r\n>... <size=200%><color=red>Meltdown!</color></size>",
            ">... Don't sprint forward!\r\n>... <size=200%><color=red>We're IN the explosion!</color></size>\r\n>... Not fleeing it!",
            ">... Wait for lights!\r\n>... <size=200%><color=red>Don't run blind!</color></size>\r\n>... You'll get lost!",
            ">... Team positions!\r\n>... <size=200%><color=red>Call out!</color></size>\r\n>... [voices overlapping]",
            ">... The earth's shaking!\r\n>... <size=200%><color=red>What's happening!</color></size>\r\n>... It's ahead of us!",
            ">... [mechanical screaming]\r\n>... Reactor core breach!\r\n>... <size=200%><color=red>Critical failure!</color></size>",
            ">... Progressive shutdown!\r\n>... <size=200%><color=red>Zone by zone!</color></size>\r\n>... Cascading failure!",
            ">... Emergency systems engaging!\r\n>... <size=200%><color=red>Auxiliary only!</color></size>\r\n>... Limited power!",
            ">... [red light flickering]\r\n>... This is all we get.\r\n>... <size=200%><color=red>Emergency lighting!</color></size>",
            ">... Can barely see!\r\n>... <size=200%><color=red>Shadows everywhere!</color></size>\r\n>... Stay close!",
            ">... The explosion happened!\r\n>... <size=200%><color=red>We weren't fast enough!</color></size>\r\n>... Too late...",
            ">... Reactor's dead ahead.\r\n>... <size=200%><color=red>That means KDS Deep...</color></size>\r\n>... Destroyed.",
            ">... [powerup whine]\r\n>... Backup systems active.\r\n>... <size=200%><color=red>That's all we have!</color></size>",
            ">... Navigate by red light.\r\n>... <size=200%><color=red>Auxiliary power only!</color></size>\r\n>... Move carefully!",
            ">... The darkness...\r\n>... Four seconds of hell.\r\n>... <size=200%><color=red>Felt like hours!</color></size>",
            ">... [equipment clattering]\r\n>... Everyone still here?\r\n>... <size=200%><color=red>Sound off!</color></size>",
            ">... KDS Deep's gone.\r\n>... <size=200%><color=red>We know it now!</color></size>\r\n>... Still going forward.",
            ">... What's left ahead?\r\n>... <size=200%><color=red>After an explosion like that?</color></size>\r\n>... Nothing good.",
            ">... [mechanical death rattle]\r\n>... Reactor's finished.\r\n>... <size=200%><color=red>Systems critical!</color></size>",

            // Arriving at Destroyed KDS Deep (35 messages)
            ">... [gasping]\r\n>... KDS Deep...\r\n>... <size=200%><color=red>It's a crater!</color></size>",
            ">... The exit zone!\r\n>... <size=200%><color=red>Completely destroyed!</color></size>\r\n>... What happened here?",
            ">... [static]\r\n>... No elevator.\r\n>... <size=200%><color=red>No extraction!</color></size>",
            ">... Where's the elevator?\r\n>... <size=200%><color=red>Gone!</color></size>\r\n>... Destroyed in the blast!",
            ">... Emergency power only!\r\n>... <size=200%><color=red>Where are the lights?</color></size>\r\n>... Auxiliary lighting!",
            ">... The HSU storage...\r\n>... <size=200%><color=red>Devastated!</color></size>\r\n>... [debris falling]",
            ">... We're too late.\r\n>... <size=200%><color=red>We arrived too late!</color></size>\r\n>... It's already gone!",
            ">... [whispering] The crater...\r\n>... Reactor explosion did this.\r\n>... <size=200%><color=red>Massive!</color></size>",
            ">... No way out!\r\n>... <size=200%><color=red>Extraction destroyed!</color></size>\r\n>... We're trapped!",
            ">... The facility's dead.\r\n>... <size=200%><color=red>Everything's on backup!</color></size>\r\n>... Barely functioning!",
            ">... [dramatic music]\r\n>... This is KDS Deep?\r\n>... <size=200%><color=red>Or what's left of it!</color></size>",
            // ">... Custom exit geomorph.\r\n>... <size=200%><color=red>R8E1 destroyed variant!</color></size>\r\n>... Crater layout!",
            ">... The destruction...\r\n>... <size=200%><color=red>It's catastrophic!</color></size>\r\n>... Nothing survived intact!",
            ">... We fought through everything!\r\n>... <size=200%><color=red>For this?!</color></size>\r\n>... A crater!",
            ">... [kicking debris]\r\n>... No extraction.\r\n>... <size=200%><color=red>What now?</color></size>",
            ">... The Inner is down there.\r\n>... Somewhere.\r\n>... <size=200%><color=red>Beneath the crater!</color></size>",
            // ">... Garganta...\r\n>... This is its home.\r\n>... <size=200%><color=red>KDS Deep crater!</color></size>",
            // ">... Chicxulub impact site!\r\n>... <size=200%><color=red>Ancient crater!</color></size>\r\n>... Now this!",
            ">... The HSUs were here.\r\n>... <size=200%><color=red>All destroyed!</color></size>\r\n>... Everything's gone!",
            ">... [wind howling]\r\n>... Through the crater.\r\n>... <size=200%><color=red>It's gone!</color></size>",
            ">... No extraction.\r\n>... <size=200%><color=red>This can't be right!</color></size>\r\n>... What's the plan?",
            ">... Team scan required.\r\n>... <size=200%><color=red>Entry point scan!</color></size>\r\n>... What happens then?",
            ">... The zone alias...\r\n>... KDS Deep, ZONE.\r\n>... <size=200%><color=red>KDS Deep!</color></size>",
            ">... We made it to KDS Deep.\r\n>... <size=200%><color=red>But there's nothing left!</color></size>\r\n>... Just ruins!",
            ">... [distant groaning metal]\r\n>... Structure's unstable!\r\n>... <size=200%><color=red>Could collapse!</color></size>",
            ">... The reactor crater...\r\n>... We're standing in it.\r\n>... <size=200%><color=red>Ground zero!</color></size>",
            // ">... Destroyed geomorph!\r\n>... <size=200%><color=red>Custom R8E1 variant!</color></size>\r\n>... Matches the lore!",
            ">... This was our objective.\r\n>... <size=200%><color=red>Reach KDS Deep!</color></size>\r\n>... Mission accomplished?",
            ">... We reached it.\r\n>... <size=200%><color=red>But at what cost?</color></size>\r\n>... [looking around]",
            ">... The devastation...\r\n>... [whispering] Total.\r\n>... <size=200%><color=red>Absolute!</color></size>",
            ">... Emergency power flickering.\r\n>... <size=200%><color=red>Barely holding!</color></size>\r\n>... How long will it last?",
            ">... [debris shifting]\r\n>... Something's moving!\r\n>... <size=200%><color=red>In the crater!</color></size>",
            ">... The exit zone is gone.\r\n>... <size=200%><color=red>Only this crater remains!</color></size>\r\n>... We're stranded!",
            ">... KDS Deep reactor failure.\r\n>... <size=200%><color=red>Complete meltdown!</color></size>\r\n>... We're in the aftermath!",
            ">... [static burst]\r\n>... No signals out.\r\n>... <size=200%><color=red>Communications dead!</color></size>",

            // Survival Encounter (50 messages)
            ">... [scanning]\r\n>... Exit scan complete!\r\n>... <size=200%><color=red>What now?!</color></size>",
            ">... WinOnDeath timer!\r\n>... <size=200%><color=red>Starting now!</color></size>\r\n>... Event Type twenty-six!",
            ">... Six seconds!\r\n>... <size=200%><color=red>SURVIVE!</color></size>\r\n>... [message appears]",
            ">... Survive?\r\n>... <size=200%><color=red>That's the objective?!</color></size>\r\n>... Just survive!",
            // ">... Thirty seconds!\r\n>... <size=200%><color=red>A-tier duration!</color></size>\r\n>... Hold out!",
            ">... Forty seconds to survive!\r\n>... <size=200%><color=red>Still?!</color></size>\r\n>... We can do this!",
            ">... Fifty seconds!\r\n>... <size=200%><color=red>That's too long!</color></size>\r\n>... Defensive positions!",
            ">... Seventy seconds!\r\n>... <size=200%><color=red>Seventy?!</color></size>\r\n>... This is going to hurt!",
            ">... Ninety seconds!\r\n>... <size=200%><color=red>We won't make it!</color></size>\r\n>... [gulp]",
            ">... [gunfire erupts]\r\n>... Eight second wave!\r\n>... <size=200%><color=red>Error_VeryHard!</color></size>",
            ">... Baseline population!\r\n>... <size=200%><color=red>They're everywhere!</color></size>\r\n>... Focus fire!",
            ">... Twenty-five percent timer!\r\n>... <size=200%><color=red>Pouncer Shadow!</color></size>\r\n>... I hear them coming!",
            ">... Sixty-six percent!\r\n>... <size=200%><color=red>Infected Hybrids!</color></size>\r\n>... It's a big wave!",
            ">... Twenty percent.\r\n>... <size=200%><color=red>Pouncer incoming!</color></size>\r\n>... Watch out!",
            ">... Fifty-five percent!\r\n>... <size=200%><color=red>Chargers wave!</color></size>\r\n>... Hold them!",
            ">... Eighty-five percent E!\r\n>... <size=200%><color=red>Giant Shooters!</color></size>\r\n>... Take cover!",
            ">... [explosions]\r\n>... Use the mines!\r\n>... <size=200%><color=red>All of them!</color></size>",
            ">... C-foam!\r\n>... <size=200%><color=red>Reinforce positions!</color></size>\r\n>... Block the lanes!",
            ">... Sentries deployed!\r\n>... <size=200%><color=red>Covering sectors!</color></size>\r\n>... Good positioning!",
            ">... Defensive formation!\r\n>... <size=200%><color=red>Watch all angles!</color></size>\r\n>... They're surrounding us!",
            ">... [reloading frantically]\r\n>... Ammo check!\r\n>... <size=200%><color=red>Running low!</color></size>",
            ">... Don't need to kill them all!\r\n>... <size=200%><color=red>Just survive!</color></size>\r\n>... The timer!",
            ">... [breathing heavily]\r\n>... How long left?\r\n>... <size=200%><color=red>Check the timer!</color></size>",
            ">... Halfway there!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... Don't give up!",
            ">... [screaming]\r\n>... He's down!\r\n>... <size=200%><color=red>Get him up!</color></size>",
            ">... Get them up!\r\n>... <size=200%><color=red>Quickly!</color></size>\r\n>... Timer's still running!",
            ">... Overwhelming numbers!\r\n>... <size=200%><color=red>Just stay alive!</color></size>\r\n>... That's all we need!",
            ">... Resource depletion!\r\n>... <size=200%><color=red>Everything's running out!</color></size>\r\n>... Make it count!",
            ">... [countdown beeping]\r\n>... Twenty seconds left!\r\n>... <size=200%><color=red>Almost there!</color></size>",
            ">... Ten seconds!\r\n>... <size=200%><color=red>Hold the line!</color></size>\r\n>... Just a bit more!",
            ">... Five seconds!\r\n>... Four!\r\n>... <size=200%><color=red>Three!</color></size>",
            // ">... [timer expires]\r\n>... <size=200%><color=red>Survival complete!</color></size>\r\n>... We did it!",
            // ">... Instant win!\r\n>... <size=200%><color=red>WinOnDeath activated!</color></size>\r\n>... Mission success!",
            ">... [static]\r\n>... WARDEN SECURITY SYSTEMS DISABLED!\r\n>... <size=200%><color=red>It's hopeless!</color></size>",
            // ">... We don't die!\r\n>... <size=200%><color=red>WinOnDeath is misleading!</color></size>\r\n>... Instant success!",
            // ">... The timer hit zero!\r\n>... <size=200%><color=red>Mission complete!</color></size>\r\n>... Automatically!",
            // ">... [dramatic music fades]\r\n>... Resources Expended screen!\r\n>... <size=200%><color=red>Custom victory!</color></size>",
            // ">... No extraction needed!\r\n>... <size=200%><color=red>Survival WAS the goal!</color></size>\r\n>... We won!",
            ">... Tank coming!\r\n>... <size=200%><color=red>What now?!</color></size>\r\n>... Timer's done!",
            ">... Mother spawning!\r\n>... Oh god!\r\n>... <size=200%><color=red>Doesn't matter!</color></size>",
            // ">... [victory screen]\r\n>... Resources Expended!\r\n>... <size=200%><color=red>R8E1 Valiant style!</color></size>",
            // ">... We survived!\r\n>... <size=200%><color=red>That's all we needed!</color></size>\r\n>... No extraction!",
            // ">... Stranded but alive!\r\n>... <size=200%><color=red>Victory!</color></size>\r\n>... Mission accomplished!",
            // ">... The cost...\r\n>... <size=200%><color=red>Resources Expended!</color></size>\r\n>... But we made it!",
            // ">... Timer-based win!\r\n>... <size=200%><color=red>Unique victory condition!</color></size>\r\n>... GoToExitGeo!",
            // ">... [exhausted]\r\n>... We held out.\r\n>... <size=200%><color=red>That's enough!</color></size>",
            // ">... No elevator escape.\r\n>... <size=200%><color=red>Just survival!</color></size>\r\n>... And we survived!",
            // ">... The odds were impossible!\r\n>... <size=200%><color=red>But we did it!</color></size>\r\n>... Somehow!",
            // ">... [silence]\r\n>... It's over.\r\n>... <size=200%><color=red>We're alive!</color></size>",

            // General Combat / Panic (15 messages)
            ">... [clicking empty]\r\n>... I'm out!\r\n>... <size=200%><color=red>No ammo left!</color></size>",
            ">... Health critical!\r\n>... <size=200%><color=red>Someone heal me!</color></size>\r\n>... [coughing]",
            ">... [thud]\r\n>... Player down!\r\n>... <size=200%><color=red>Need revive!</color></size>",
            ">... Get them up now!\r\n>... <size=200%><color=red>We can't lose anyone!</color></size>\r\n>... Hurry!",
            ">... Too many!\r\n>... <size=200%><color=red>We're outnumbered!</color></size>\r\n>... Fall back!",
            ">... [gasping]\r\n>... Can't keep this up!\r\n>... <size=200%><color=red>Overwhelming!</color></size>",
            ">... They just keep coming!\r\n>... <size=200%><color=red>Endless waves!</color></size>\r\n>... No end!",
            ">... [screaming]\r\n>... Behind you!\r\n>... <size=200%><color=red>Turn around!</color></size>",
            ">... Surrounded!\r\n>... <size=200%><color=red>Three sixty coverage!</color></size>\r\n>... Watch everywhere!",
            ">... We can't win this!\r\n>... <size=200%><color=red>Yes we can!</color></size>\r\n>... Just survive!",
            ">... [weapon jamming]\r\n>... No!\r\n>... <size=200%><color=red>Not now!</color></size>",
            ">... Resources depleted!\r\n>... <size=200%><color=red>Everything's gone!</color></size>\r\n>... Melee only!",
            ">... Last player standing!\r\n>... <size=200%><color=red>Come on!</color></size>\r\n>... Get up!",
            ">... [frantic footsteps]\r\n>... Running!\r\n>... <size=200%><color=red>They're right behind!</color></size>",
            ">... This is it!\r\n>... <size=200%><color=red>Make every shot count!</color></size>\r\n>... [gunfire]",

            // Lore / Atmosphere (10 messages)
            // ">... Garganta is real.\r\n>... <size=200%><color=red>Down here somewhere!</color></size>\r\n>... The ancient one.",
            // ">... The Inner spacecraft.\r\n>... <size=200%><color=red>Beneath KDS Deep!</color></size>\r\n>... Collector technology!",
            // ">... Chicxulub crater!\r\n>... <size=200%><color=red>Dinosaur extinction!</color></size>\r\n>... And now this!",
            ">... NAM-V virus originated here.\r\n>... <size=200%><color=red>KDS Deep!</color></size>\r\n>... Source of everything!",
            // ">... The Collectors' ship.\r\n>... <size=200%><color=red>The Inner!</color></size>\r\n>... Ancient alien craft!",
            ">... Garganta's voice.\r\n>... We heard the warning.\r\n>... <size=200%><color=red>It knows!</color></size>",
            // ">... KDS facility history.\r\n>... <size=200%><color=red>All ends here!</color></size>\r\n>... In crater ruins!",
            // ">... The Yucatan Peninsula.\r\n>... <size=200%><color=red>Mexico!</color></size>\r\n>... Deepest point!",
            // ">... Kovac Defense Services.\r\n>... <size=200%><color=red>Their greatest failure!</color></size>\r\n>... KDS Deep lost!",
            // ">... [whispering] The Inner calls.\r\n>... <size=200%><color=red>Garganta answers!</color></size>\r\n>... We're between them!"
        }))!);
        #endregion
    }
}
