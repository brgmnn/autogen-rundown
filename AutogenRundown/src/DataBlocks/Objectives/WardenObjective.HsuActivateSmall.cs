using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/*
 * This objective is bringing a large item like Neonate or Data Sphere to a machine and then
 * optionally returning with it.
 *
 * Looks like we have a few options for what we could do with this:
 *  - Bring Item down in elevator -> insert into device
 *  - Bring Item down in elevator -> insert into device -> extract with it
 *  - No item in elevator -> find item -> insert into device
 *  - No item in elevator -> find item -> insert into device -> extract with it
 */
public partial record WardenObjective
{
    public void PreBuild_HsuActivateSmall(BuildDirector director, Level level)
    {
        var item = Generator.Pick(new List<Items.Item>
        {
            Items.Item.DataSphere,
            Items.Item.NeonateHsu_Stage1
        });

        // ActivateHSU_ItemFromStart = Items.Item.NeonateHsu_Stage1;
        // ActivateHSU_ItemAfterActivation = Items.Item.NeonateHsu_Stage2;

        ActivateHSU_ItemFromStart = item;
        ActivateHSU_ItemAfterActivation = item == Items.Item.NeonateHsu_Stage1 ? Items.Item.NeonateHsu_Stage2 : item;
        ActivateHSU_RequireItemAfterActivationInExitScan = true;
    }

    public void Build_HsuActivateSmall(BuildDirector director, Level level)
    {
        switch (ActivateHSU_ItemFromStart)
        {
            case Items.Item.DataSphere:
            {
                MainObjective = "Bring the Data sphere to [ITEM_SERIAL] to unlock its data encryption";
                SolveItem = $"Insert Data Sphere into {HsuActivateSmall_MachineName} [ITEM_SERIAL]";
                GoToWinCondition_Elevator = "Return the Data Sphere to the point of entrance in [EXTRACTION_ZONE]";
                GoToWinCondition_CustomGeo = "Bring the Data Sphere to the forward exit in [EXTRACTION_ZONE]";

                break;
            }
            case Items.Item.NeonateHsu_Stage1:
            {
                MainObjective = "Bring the Neonate to [ITEM_SERIAL] to reactivate it";
                SolveItem = $"Insert Neonate into {HsuActivateSmall_MachineName} [ITEM_SERIAL]";
                GoToWinCondition_Elevator = "Return the Neonate to the point of entrance in [EXTRACTION_ZONE]";
                GoToWinCondition_CustomGeo = "Bring the Neonate to the forward exit in [EXTRACTION_ZONE]";
                break;
            }
        }

        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = "Navigate to [ITEM_ZONE] and find [ITEM_SERIAL]";
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find [ITEM_SERIAL] inside [ITEM_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        ActivateHSU_BringItemInElevator = true;
        ActivateHSU_MarkItemInElevatorAsWardenObjective = false;
        ActivateHSU_StopEnemyWavesOnActivation = false;
        ActivateHSU_ObjectiveCompleteAfterInsertion = true; // true fixes the double item in exit scan bug
        ActivateHSU_RequireItemAfterActivationInExitScan = true;

        AddCompletedObjectiveWaves(level, director);
    }

    public void PostBuild_HsuActivateSmall(BuildDirector director, Level level)
    {
        #region Warden Intel Messages
        // Generic item messages
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 6), Generator.Draw(new List<string>
        {
            ">... [flicker of lights]\r\n>... The machine says 'processing'.\r\n>... <size=200%><color=red>Defend until it's done!</color></size>",
            ">... [quiet humming]\r\n>... Haul it faster!\r\n>... <size=200%><color=red>Bring it to the machine.</color></size>",
            ">... Let's hurry, carrying slows us down.\r\n>... <size=200%><color=red>Get it locked in the machine!</color></size>\r\n>... Then we can rearm.",
            ">... <size=200%><color=red>Brace yourselves!</color></size>\r\n>... Activating the system might be loud.\r\n>... Enemies will come running.",
            ">... [alarm beep]\r\n>... Processing could take a while.\r\n>... <size=200%><color=red>We hold this position!</color></size>",
            ">... <size=200%><color=red>Don't set it down yet!</color></size>\r\n>... Wait for the prompt.\r\n>... This machine is picky.",
            ">... [electronic whirring]\r\n>... The machine is warming up.\r\n>... <size=200%><color=red>Defend until it completes!</color></size>",
            ">... <size=200%><color=red>This cargo is top priority.</color></size>\r\n>... We can't proceed without it.\r\n>... Keep your eyes peeled.",
            ">... I hate carrying big targets...\r\n>... <size=200%><color=red>But the Warden demands it!</color></size>\r\n>... Let's get this done quickly.",
            ">... <size=200%><color=red>Hold on!</color></size>\r\n>... Something's triggered by the scanning.\r\n>... Keep the area clear.",
            ">... The readout shows 60%.\r\n>... Not done yet.\r\n>... <size=200%><color=red>Stay on guard!</color></size>",
            ">... <size=200%><color=red>We're almost there!</color></size>\r\n>... Machine's finishing up.\r\n>... Then we haul it back.",
            ">... <size=200%><color=red>Don't drop it!</color></size>\r\n>... We can't afford any damage.\r\n>... Keep a firm grip.",
            ">... <size=200%><color=red>Alright, set it here!</color></size>\r\n>... The machine will do the rest.\r\n>... We just stand guard now.",
            ">... The readout says 'Processing sample'.\r\n>... That can't be good...\r\n>... <size=200%><color=red>Just keep quiet and watch!</color></size>",
            ">... [buzzing panel]\r\n>... It's halfway done.\r\n>... <size=200%><color=red>Don't let enemies break the scanner!</color></size>",
            ">... [flashing lights]\r\n>... The machine draws a lot of power.\r\n>... <size=200%><color=red>Hurry, it's making a scene!</color></size>",
            ">... We can't open it ourselves.\r\n>... <size=200%><color=red>Only the Warden can unlock this!</color></size>\r\n>... So let's deliver.",
            ">... <size=200%><color=red>Don't jostle it!</color></size>\r\n>... The machine might fail.\r\n>... Then we do this all again.",
            ">... I'm entering the code.\r\n>... [terminal beeps]\r\n>... <size=200%><color=red>Stand by for activation!</color></size>",
            ">... [flicker of power]\r\n>... The device is halfway done.\r\n>... <size=200%><color=red>Stay close, it's vulnerable.</color></size>",
            ">... <size=200%><color=red>Machine beeped. It's finished!</color></size>\r\n>... Grab the cargo!\r\n>... We run this back, quick.",
            ">... There's a final scan we must do.\r\n>... That console needs input.\r\n>... <size=200%><color=red>Don't leave yet!</color></size>",
            ">... <size=200%><color=red>Cover me while I carry this!</color></size>\r\n>... My hands are full.\r\n>... Someone watch my flank.",
            ">... [terminal beep]\r\n>... The Warden's console says 'INSERT'.\r\n>... <size=200%><color=red>Let's slot it now!</color></size>",
            ">... This is it.\r\n>... The entire mission hinges on it.\r\n>... <size=200%><color=red>Protect it with your life!</color></size>",
            ">... [electronic whine]\r\n>... The station locked onto it.\r\n>... <size=200%><color=red>Time to let it run its cycle!</color></size>",
            ">... <size=200%><color=red>Grab it. Now move!</color></size>\r\n>... We finished the scan.\r\n>... Let's get out of here!",
            ">... [warning beep]\r\n>... The station's venting steam.\r\n>... <size=200%><color=red>Don't stand too close!</color></size>",
            ">... <size=200%><color=red>We have to escort it now.</color></size>\r\n>... The logs say 'final phase'.\r\n>... Keep your guard up.",

            // ">... <size=200%><color=red>Look at that label!</color></size>\r\n>... 'High-Security Cargo'?\r\n>... This won't be easy.",
        }))!);

        switch (ActivateHSU_ItemFromStart)
        {
            case Items.Item.DataSphere:
            {
                level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
                {
                    // Data Sphere
                    ">... It's heavier than it looks.\r\n>... Watch your step.\r\n>... <size=200%><color=red>We can't drop the data sphere!</color></size>",
                    ">... <size=200%><color=red>This data sphere is crucial.</color></size>\r\n>... The Warden demanded it.\r\n>... We better not disappoint them.",
                    ">... Keep an eye on corners.\r\n>... <size=200%><color=red>We can't lose the data sphere!</color></size>\r\n>... It's our only key forward.",
                    ">... [metallic clang]\r\n>... That's the intake port.\r\n>... <size=200%><color=red>Slot the data sphere in and hold tight!</color></size>",
                    ">... That sphere's locked tight.\r\n>... Must be something important.\r\n>... <size=200%><color=red>We deliver, no matter what.</color></size>",
                    ">... [soft hum]\r\n>... The data sphere's active.\r\n>... <size=200%><color=red>Plug it in. Start the process.</color></size>",
                    ">... [heavy footsteps]\r\n>... I'm slow with this data sphere.\r\n>... <size=200%><color=red>Cover me while I move!</color></size>",
                    ">... Data port's on the other side.\r\n>... <size=200%><color=red>Circle around and secure it!</color></size>\r\n>... Watch for sleepers!",
                    ">... [grunting]\r\n>... Why's this thing so heavy?\r\n>... <size=200%><color=red>The data contents must be critical.</color></size>",
                    ">... <size=200%><color=red>Check the console!</color></size>\r\n>... Does it say 'Complete'?\r\n>... Then get that sphere out of here.",
                    ">... <size=200%><color=red>Security alert triggered!</color></size>\r\n>... Processing must be drawing hostiles.\r\n>... Defend the data sphere!",
                }))!);
                break;
            }

            case Items.Item.NeonateHsu_Stage1:
            {
                level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
                {
                    ">... <size=200%><color=red>Careful!</color></size>\r\n>... That neonate might be fragile.\r\n>... Keep it upright in transit.",
                    ">... [strained breathing]\r\n>... Did Warden say what's inside?\r\n>... <size=200%><color=red>We just deliver. No questions.</color></size>",
                    ">... This little HSU requires a scan.\r\n>... Let's plug it into the station.\r\n>... <size=200%><color=red>Stay alert while it processes!</color></size>",
                    ">... <size=200%><color=red>Keep the neonate safe!</color></size>\r\n>... Enemies won't ignore it.\r\n>... We move as a unit.",
                    ">... The readout says 'Neonate Inside'.\r\n>... <size=200%><color=red>Whatever that means, don't break it!</color></size>\r\n>... Let's keep it stable.",
                    ">... [rapid beeping]\r\n>... The console warns of hostiles.\r\n>... <size=200%><color=red>We can't abandon the neonate now!</color></size>",
                    ">... They didn't tell us what this neonate is.\r\n>... <size=200%><color=red>Probably for some twisted experiment!</color></size>\r\n>... Let's just do our job.",
                    ">... <size=200%><color=red>Grab that neonate now!</color></size>\r\n>... We need to carry it back.\r\n>... The Warden wants it intact.",
                    ">... Something's inside, something moving\r\n>... <size=200%><color=red>Ignore it, just keep moving!</color></size>\r\n>... Don't think about it.",
                    ">... [quiet mechanical hum]\r\n>... Something's alive in there.\r\n>... <size=200%><color=red>Keep steady hands.</color></size>",
                    ">... The logs mention a 'small HSU'.\r\n>... <size=200%><color=red>We have no idea what's inside.</color></size>\r\n>... Let's not linger.",
                    ">... Are we sure it's stable?\r\n>... The machine's giving warnings.\r\n>... <size=200%><color=red>Too late to back out now!</color></size>",
                    ">... [grinding sound]\r\n>... Feels like it's unlocking.\r\n>... <size=200%><color=red>Keep your weapons ready!</color></size>",
                    ">... [drips of coolant]\r\n>... The neonate container is frosted over.\r\n>... <size=200%><color=red>Hope it keeps functioning!</color></size>",
                    ">... That item might be a blueprint.\r\n>... Or a living test subject.\r\n>... <size=200%><color=red>We won't question it.</color></size>",
                    ">... Wait, did that read 'Neonate'?\r\n>... This is definitely bigger than us.\r\n>... <size=200%><color=red>Just complete the handoff!</color></size>",
                }))!);
                break;
            }
        }
        #endregion
    }
}
