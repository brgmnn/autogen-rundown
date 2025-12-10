using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks.Objectives;

/**
 * Objective: HsuFindSample
 *
 *
 * Collect the HSU from within a storage zone
 */
public partial record WardenObjective
{
    public void Build_HsuFindSample(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Find <color=orange>[ITEM_SERIAL]</color> somewhere inside HSU Storage Zone");

        ActivateHSU_BringItemInElevator = true;
        GatherItemId = (uint)WardenObjectiveItem.HSU;
        StartPuzzle = ChainedPuzzle.FindOrPersist(ChainedPuzzle.TeamScan);

        // Place HSU's within the objective zone
        var zn = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "hsu_sample")!;
        var zoneIndex = zn.ZoneNumber;

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>
            {
                new()
                {
                    LocalIndex = zoneIndex,
                    Weights = ZonePlacementWeights.NotAtStart
                }
            });

        var zone = layout.Zones[zoneIndex];
        zone.HSUsInZone = level.Tier switch
        {
            "B" => DistributionAmount.SomeMore,
            "C" => DistributionAmount.Many,
            "D" => DistributionAmount.Alot,
            "E" => DistributionAmount.Tons,
            _ => DistributionAmount.Some
        };
        zone.Coverage = level.Tier switch
        {
            "A" => new CoverageMinMax { Min = 20, Max = 25 },
            "B" => new CoverageMinMax { Min = 25, Max = 30 },
            "C" => new CoverageMinMax { Min = 30, Max = 50 },
            "D" => new CoverageMinMax { Min = 50, Max = 75 },
            "E" => new CoverageMinMax { Min = 75, Max = 100 },
            _ => zone.Coverage
        };
        zone.HSUClustersInZone = level.Tier switch
        {
            "C" => 2,
            "D" => 3,
            "E" => 3,
            _ => 1
        };

        AddCompletedObjectiveWaves(level, director);
    }

    private void PostBuildIntel_HsuFindSample(Level level)
    {
        #region Warden Intel Messages
        /*
         * HSU Find Sample specific warden intel messages
         */
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
        {
            /*
             * All these intel messages end with the highlighted portion at the end
             */
            ">... [heavy breathing] Search for that HSU.\r\n>... Why do we need this sample?\r\n>... <size=200%><color=red>Just find it!</color></size>",
            ">... That console might show where it is.\r\n>... [typing] Checking logs now.\r\n>... <size=200%><color=red>We must find that HSU!</color></size>",
            ">... [scanner ping] Nothing in this zone.\r\n>... We should check through there.\r\n>... <size=200%><color=red>Don't forget the sample scan!</color></size>",
            // ">... Another locked door?\r\n>... [sigh] We need that key.\r\n>... <size=200%><color=red>HSU might be behind here.</color></size>",
            ">... Warden's orders are clear.\r\n>... We retrieve this sample.\r\n>... <size=200%><color=red>But why?</color></size>",
            ">... [metal clang]\r\n>... Is that an HSU hallway?\r\n>... <size=200%><color=red>Check each one carefully.</color></size>",
            ">... So many stasis units...\r\n>... Who are these people?\r\n>... <size=200%><color=red>Focus! Find ours now.</color></size>",
            ">... [beeping] That HSU panel is active.\r\n>... Let's do the bioscan.\r\n>... <size=200%><color=red>We need that sample code.</color></size>",
            ">... I'm worried about what's inside.\r\n>... Could this sample be infected?\r\n>... <size=200%><color=red>Either way, we proceed.</color></size>",
            ">... [quiet humming]\r\n>... This place creeps me out.\r\n>... <size=200%><color=red>But we need the HSU sample!</color></size>",
            ">... Keep scanning every row.\r\n>... We can't miss that unit.\r\n>... <size=200%><color=red>Time is against us!</color></size>",
            ">... [alarm beeping]\r\n>... That terminal might help.\r\n>... <size=200%><color=red>Check HSU logs immediately!</color></size>",
            ">... Did you see that label?\r\n>... Could be our target.\r\n>... <size=200%><color=red>Don't skip any. Verify everything!</color></size>",
            ">... [electrical buzz]\r\n>... This sample must be crucial.\r\n>... <size=200%><color=red>The Warden must have big plans.</color></size>",
            ">... Check behind that sealed door.\r\n>... We must hurry.\r\n>... <size=200%><color=red>HSU sample won't wait forever!</color></size>",
            ">... [rattling] Something moved over there.\r\n>... Keep scanning for HSUs.\r\n>... <size=200%><color=red>We can't fail this mission!</color></size>",
            ">... Another stasis tube.\r\n>... Is this the right occupant?\r\n>... <size=200%><color=red>Scan the ID carefully!</color></size>",
            // ">... [coughing] Stale air everywhere.\r\n>... Where's the HSU access panel?\r\n>... <size=200%><color=red>We can't see in this fog!</color></size>",
            ">... Why store people like this?\r\n>... Warden's methods are secret.\r\n>... <size=200%><color=red>We only follow orders.</color></size>",
            ">... [clank] Another security door.\r\n>... Let's override it.\r\n>... <size=200%><color=red>HSU data is behind there.</color></size>",
            ">... Checking HSU number again.\r\n>... [typing] It's not here.\r\n>... <size=200%><color=red>We must search deeper!</color></size>",
            ">... Someone else was here...\r\n>... [bloodstains] They didn't finish.\r\n>... <size=200%><color=red>Let's not end like them!</color></size>",
            ">... So many sealed containers...\r\n>... Could they all hold samples?\r\n>... <size=200%><color=red>Just find the right one.</color></size>",
            ">... [whispering] Do we trust the Warden?\r\n>... They want someone's DNA?\r\n>... <size=200%><color=red>Focus. Keep going!</color></size>",
            ">... The logs show a partial match.\r\n>... It's near that storage block.\r\n>... <size=200%><color=red>Check it now!</color></size>",
            ">... [scanner beep]\r\n>... This is the HSU zone.\r\n>... <size=200%><color=red>Proceed with caution!</color></size>",
            ">... It's so cold here.\r\n>... Maybe cryo systems still function.\r\n>... <size=200%><color=red>Don't let it distract you.</color></size>",
            ">... [metal door opens]\r\n>... Another row of HSUs.\r\n>... <size=200%><color=red>Scan them all. Quickly!</color></size>",
            ">... We can't fail this extraction.\r\n>... The sample is everything.\r\n>... <size=200%><color=red>Warden won't accept failure.</color></size>",
            ">... [machinery humming]\r\n>... Check behind that stasis tube.\r\n>... <size=200%><color=red>Could be our target HSU.</color></size>",
            ">... This occupant looks... fresh.\r\n>... Could this be the one?\r\n>... <size=200%><color=red>Scan ID tags now!</color></size>",
            ">... [warning beep]\r\n>... Something's wrong with this unit.\r\n>... <size=200%><color=red>Keep searching; that sample isn't here.</color></size>",
            ">... Are these people even alive?\r\n>... They've been here centuries?\r\n>... <size=200%><color=red>Don't ask. Just keep moving!</color></size>",
            ">... That label matches the logs.\r\n>... [keypad clicks]\r\n>... <size=200%><color=red>Open it. Get the sample!</color></size>",
            ">... I'm not sure it's safe.\r\n>... The stasis field might fail.\r\n>... <size=200%><color=red>Just grab the sample quickly.</color></size>",
            ">... [electronic whirring]\r\n>... The data spool is loading.\r\n>... <size=200%><color=red>We must extract that record!</color></size>",
            ">... That occupant has a missing limb.\r\n>... Did something break out?\r\n>... <size=200%><color=red>Stay sharp. It's not safe.</color></size>",
            ">... This tube shows critical errors.\r\n>... We can't scan it properly.\r\n>... <size=200%><color=red>Move on. Time is short!</color></size>",
            ">... There's a name tag here.\r\n>... Could that be our sample?\r\n>... <size=200%><color=red>Confirm with the terminal!</color></size>",
            ">... [quiet shuffle]\r\n>... My gut says we're close.\r\n>... <size=200%><color=red>Don't let your guard down!</color></size>",
            ">... We only have partial coordinates.\r\n>... That HSU could be anywhere.\r\n>... <size=200%><color=red>Stay determined. We'll find it!</color></size>",
            ">... [distant rumbling]\r\n>... The deeper we go, the worse.\r\n>... <size=200%><color=red>HSU must be near. Keep moving!</color></size>",
            ">... Think about it: who is inside?\r\n>... The Warden wants their sample?\r\n>... <size=200%><color=red>Stop speculating. Focus on tasks!</color></size>",
            ">... Another locked panel.\r\n>... Let's bypass it quickly.\r\n>... <size=200%><color=red>HSU data is inside. Hurry!</color></size>",
            ">... [low static]\r\n>... Why is this sample so important?\r\n>... <size=200%><color=red>Doesn't matter. Just find it!</color></size>",
            ">... We found an HSU archive.\r\n>... Must be thousands here.\r\n>... <size=200%><color=red>Look for the right occupant!</color></size>",
            ">... [scanner alert]\r\n>... Found something near that unit.\r\n>... <size=200%><color=red>Could be the sample container!</color></size>",
            ">... Hard to read these codes.\r\n>... The file is corrupted.\r\n>... <size=200%><color=red>Keep trying. No giving up!</color></size>",
            ">... This is the correct zone.\r\n>... The logs confirm it.\r\n>... <size=200%><color=red>Find that HSU ASAP!</color></size>",
            ">... [exhaling shakily]\r\n>... We almost have the sample.\r\n>... <size=200%><color=red>Secure it and extract now!</color></size>",

            /*
             * These intel messages have the highlighted portion at the start or middle
             *
             * Commented out messages should probably be used in other situations
             */
            ">... <size=200%><color=red>Keep your eyes open!</color></size>\r\n>... Another corridor of HSUs.\r\n>... The sample must be near.",
            // ">... <size=200%><color=red>Doors sealed behind us!</color></size>\r\n>... We can't turn back now.\r\n>... That HSU is forward.",
            ">... We're venturing deeper, <size=200%><color=red>follow the data logs</color></size>.\r\n>... The sample has to be here.",
            ">... <size=200%><color=red>This stasis unit is offline!</color></size>\r\n>... Let's see if the occupant has the sample.\r\n>... Carefully open it.",
            ">... Why does the Warden <size=200%><color=red>need this DNA</color></size> anyway?\r\n>... I'm not sure, but we must comply.",
            ">... <size=200%><color=red>Wait!</color></size>\r\n>... The HSU label matches our objective.\r\n>... Let's start extracting.",
            ">... There's a body inside.\r\n>... The scanner shows <size=200%><color=red>the needed sample</color></size> is intact.",
            ">... <size=200%><color=red>I've got movement!</color></size>\r\n>... Check that HSU quickly.\r\n>... We don't have much time.",
            ">... That console's flickering.\r\n>... <size=200%><color=red>Try the override now.</color></size>\r\n>... We need the data fast.",
            ">... We might be close.\r\n>... <size=200%><color=red>Check the occupant's ID</color></size> on this HSU.\r\n>... Cross-reference everything.",
            ">... <size=200%><color=red>Stop!</color></size>\r\n>... Did you hear something?\r\n>... We can't lose focus on the sample.",
            ">... Look at these notes.\r\n>... <size=200%><color=red>This subject is flagged high-priority.</color></size>\r\n>... Must be the one.",
            ">... It's so cold here.\r\n>... <size=200%><color=red>The occupant might still be alive.</color></size>\r\n>... Keep your guard up.",
            ">... <size=200%><color=red>Over here!</color></size>\r\n>... HSU shows partial power.\r\n>... Let's see if we can open it.",
            ">... Lights are flickering.\r\n>... <size=200%><color=red>We can't risk an alarm.</color></size>\r\n>... Work quickly on that terminal.",
            ">... Something is dripping from above.\r\n>... <size=200%><color=red>Stay alert and locate the sample.</color></size>\r\n>... Watch for ambushes.",
            ">... <size=200%><color=red>Another row of HSUs!</color></size>\r\n>... We search them all.\r\n>... The sample is here somewhere.",
            ">... Keep scanning the logs.\r\n>... <size=200%><color=red>If it's not here, we move on.</color></size>\r\n>... Time is ticking.",
            ">... The occupant's ID matches!\r\n>... <size=200%><color=red>Quick, scan for the sample!</color></size>\r\n>... We secure it and leave.",
            ">... <size=200%><color=red>Who were these people?</color></size>\r\n>... The Warden won't say.\r\n>... Let's just finish the job.",
            ">... Did you see that label?\r\n>... <size=200%><color=red>I think it's our HSU.</color></size>\r\n>... Let's confirm it.",
            ">... There must be a reason.\r\n>... <size=200%><color=red>Why this HSU specifically?</color></size>\r\n>... We won't get answers now.",
            ">... <size=200%><color=red>Watch the power levels!</color></size>\r\n>... Too low and stasis might fail.\r\n>... That sample is fragile.",
            ">... We keep going.\r\n>... <size=200%><color=red>The logs indicate a sub-level.</color></size>\r\n>... One more passage down.",
            ">... <size=200%><color=red>Mind the noise!</color></size>\r\n>... We don't want them waking.\r\n>... Keep your flashlights low.",
            ">... Warden said \"Retrieve or die.\" \r\n>... <size=200%><color=red>So we keep searching!</color></size>\r\n>... No matter what's down here.",
            ">... <size=200%><color=red>This place is huge.</color></size>\r\n>... How many HSUs are in here?\r\n>... We'll find ours eventually.",
            ">... The HSU panel is jammed.\r\n>... <size=200%><color=red>Try a manual override!</color></size>\r\n>... That sample won't wait forever.",
            ">... <size=200%><color=red>I've got a partial reading!</color></size>\r\n>... The occupant might still have what we need.\r\n>... Carefully approach.",
            ">... So many unknown subjects.\r\n>... <size=200%><color=red>We just need the right one.</color></size>\r\n>... Keep scanning labels.",
            ">... <size=200%><color=red>Hang on!</color></size>\r\n>... This HSU was flagged by prior teams.\r\n>... Let's see if it's intact.",
            ">... The stasis fluid is leaking.\r\n>... <size=200%><color=red>That occupant won't last long.</color></size>\r\n>... We better hurry.",
            ">... <size=200%><color=red>This ID is incomplete!</color></size>\r\n>... We have to guess.\r\n>... Check the next corridor.",
            ">... The logs reference a sample type.\r\n>... <size=200%><color=red>DNA extraction, maybe?</color></size>\r\n>... Let's confirm at the console.",
            ">... <size=200%><color=red>We found the HSU number!</color></size>\r\n>... It's in the next sector.\r\n>... We need to bypass that lock.",
            ">... I'm stepping over cables.\r\n>... <size=200%><color=red>Watch for power surges!</color></size>\r\n>... We can't lose the terminals.",
            ">... <size=200%><color=red>Another barrier?</color></size>\r\n>... Warden must be protecting this.\r\n>... Let's find a way through.",
            ">... There's a name scratched here.\r\n>... <size=200%><color=red>Must've been important.</color></size>\r\n>... Check the occupant's file.",
            ">... <size=200%><color=red>Keep your footsteps quiet!</color></size>\r\n>... These HSUs might alert something.\r\n>... No sudden noises.",
            ">... That panel's flickering again.\r\n>... <size=200%><color=red>Reset it to access the logs!</color></size>\r\n>... The sample is in there.",
            ">... <size=200%><color=red>These tubes are everywhere.</color></size>\r\n>... We'll have to split up.\r\n>... Remember the plan, and regroup.",
            ">... The occupant's readout is stable.\r\n>... <size=200%><color=red>Let's extract the data now.</color></size>\r\n>... Watch my back!",
            ">... <size=200%><color=red>Heads up!</color></size>\r\n>... We lost power again.\r\n>... This won't be easy.",
            ">... Something's locked in that case.\r\n>... <size=200%><color=red>Could be the sample drive!</color></size>\r\n>... Let's pry it open.",
            ">... <size=200%><color=red>This isn't the right occupant.</color></size>\r\n>... Keep looking.\r\n>... We'll find the match.",
            ">... My tracker shows HSU pods ahead.\r\n>... <size=200%><color=red>That must be the zone.</color></size>\r\n>... Eyes peeled, everyone.",
            ">... <size=200%><color=red>The ID code is garbled!</color></size>\r\n>... We'll have to guess.\r\n>... On to the next row.",
            ">... There's a faint reading here.\r\n>... <size=200%><color=red>Check if it matches our sample.</color></size>\r\n>... I'll run the scan.",
            ">... <size=200%><color=red>Are we sure it's safe?</color></size>\r\n>... Not at all.\r\n>... But we need that data.",
            ">... The logs show missing entries.\r\n>... <size=200%><color=red>Someone tampered with them!</color></size>\r\n>... Keep trying different terminals."
        }))!);
        #endregion
    }
}
