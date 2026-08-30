using AutogenRundown.DataBlocks.Enemies;

namespace AutogenRundown.DataBlocks.Objectives;

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
                MainObjective = new Text("Bring the Data sphere to [ITEM_SERIAL] to unlock its data encryption");
                SolveItem = new Text($"Insert Data Sphere into {HsuActivateSmall_MachineName} [ITEM_SERIAL]");
                GoToWinCondition_Elevator = new Text(() =>
                    $"Return the Data Sphere to the point of entrance in {Intel.Zone(level.ExtractionZone, level.Planner)}");
                GoToWinCondition_CustomGeo = new Text(() =>
                    $"Bring the Data Sphere to the forward exit in {Intel.Zone(level.ExtractionZone, level.Planner)}");

                break;
            }
            case Items.Item.NeonateHsu_Stage1:
            {
                MainObjective = new Text("Bring the Neonate to [ITEM_SERIAL] to reactivate it");
                SolveItem = new Text($"Insert Neonate into {HsuActivateSmall_MachineName} [ITEM_SERIAL]");
                GoToWinCondition_Elevator = new Text(() =>
                    $"Return the Neonate to the point of entrance in {Intel.Zone(level.ExtractionZone, level.Planner)}");
                GoToWinCondition_CustomGeo = new Text(() =>
                    $"Bring the Neonate to the forward exit in {Intel.Zone(level.ExtractionZone, level.Planner)}");
                break;
            }
        }

        FindLocationInfo = new Text("Gather information about the location of [ITEM_SERIAL]");
        GoToZone = new Text("Navigate to [ITEM_ZONE] and find [ITEM_SERIAL]");
        GoToZoneHelp = new Text("Use information in the environment to find [ITEM_ZONE]");
        InZoneFindItem = new Text("Find [ITEM_SERIAL] inside [ITEM_ZONE]");
        GoToWinConditionHelp_Elevator = new Text("Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back");
        GoToWinConditionHelp_CustomGeo = new Text("Use the navigational beacon and the information in the surroundings to find the exit point");

        ActivateHSU_BringItemInElevator = true;
        ActivateHSU_MarkItemInElevatorAsWardenObjective = false;
        ActivateHSU_StopEnemyWavesOnActivation = false;
        ActivateHSU_ObjectiveCompleteAfterInsertion = true; // true fixes the double item in exit scan bug
        ActivateHSU_RequireItemAfterActivationInExitScan = true;

        AddCompletedObjectiveChallenge(level, director);
    }

    private void PostBuildIntel_HsuActivateSmall(Level level)
    {
        #region Warden Intel Messages
        // Generic item messages
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
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
            ">... I can barely jog with this.\r\n>... Then don't jog. Just don't stop.\r\n>... <size=200%><color=red>Something's already following us!</color></size>",
            ">... [dripping]\r\n>... <size=200%><color=red>That's the intake. Has to be.</color></size>\r\n>... It hasn't had power in a very long time.",
            ">... <size=200%><color=red>Take it! My arms are gone!</color></size>\r\n>... Set it down first, I can't grab it mid-run.\r\n>... [panting] Then hurry up...",
            ">... Eleven percent.\r\n>... <size=200%><color=red>Eleven?! We'll be dry before it's half done!</color></size>\r\n>... Count your rounds. Out loud.",
            ">... The console's asking me to confirm.\r\n>... Confirm what? It doesn't say what happens after.\r\n>... <size=200%><color=red>Just press it. We're not getting answers.</color></size>",
            ">... <size=200%><color=red>It's screaming into the dark!</color></size>\r\n>... Every one of them heard that.\r\n>... [distant shrieking]",
            ">... Cycle ended. Pull it.\r\n>... <size=200%><color=red>It's fused to the cradle!</color></size>\r\n>... [straining] Pull harder!",
            ">... Don't lean on the housing.\r\n>... Why, what happens?\r\n>... <size=200%><color=red>It stops, and then we start over.</color></size>",
            ">... <size=200%><color=red>How far back is the lift?</color></size>\r\n>... Further than we came. The doors sealed behind us.\r\n>... [swearing]",
            ">... [stumbling]\r\n>... <size=200%><color=red>I can't shoot while I'm holding this!</color></size>\r\n>... Then get behind me and keep walking.",
            ">... What does the Warden even want with it?\r\n>... Don't. Whatever the answer is, we're the ones carrying it.\r\n>... <size=200%><color=red>We're the delivery. That's all we are.</color></size>",
            ">... <size=200%><color=red>Trade with me. Now, before the corner.</color></size>\r\n>... Count of three. One... two...\r\n>... [grunting]",
            ">... Lid's open. It's waiting.\r\n>... <size=200%><color=red>Slot it and step back!</color></size>\r\n>... [clanking]",
            ">... Every terminal points the same direction.\r\n>... Deeper, then. Always deeper.\r\n>... <size=200%><color=red>It's waiting down there for us.</color></size>",
            ">... <size=200%><color=red>Forty percent and it's slowing down!</color></size>\r\n>... The power's browning out.\r\n>... We hold. There's nothing else to do.",
            ">... [hissing]\r\n>... <size=200%><color=red>It's venting something. Don't breathe near it!</color></size>\r\n>... Great. Of course it is.",
            ">... They're stacking up in the corridor.\r\n>... Let them come to the choke.\r\n>... <size=200%><color=red>Nobody steps away from the machine!</color></size>",
            ">... <size=200%><color=red>Get it out and run!</color></size>\r\n>... The cradle's still holding it!\r\n>... [wrenching]",
            ">... It got heavier after the machine finished.\r\n>... <size=200%><color=red>That's not possible.</color></size>\r\n>... Then you carry it and tell me I'm wrong.",
            ">... My legs are shaking.\r\n>... Hand it over then.\r\n>... <size=200%><color=red>No. If I stop, I won't start again.</color></size>",
            ">... <size=200%><color=red>Slow down, I'm falling behind!</color></size>\r\n>... We can't slow down.\r\n>... [panting]",
            ">... This room's full of empty cradles.\r\n>... <size=200%><color=red>Only one of them still has power.</color></size>\r\n>... Then that's the one.",
            ">... Somebody else take a turn.\r\n>... You've had it since the lift, nobody's arguing.\r\n>... <size=200%><color=red>Then move, before they wake!</color></size>",
            ">... <size=200%><color=red>Don't slot it until we're set!</color></size>\r\n>... Sentries out. Foam the near approach.\r\n>... Once it starts, it doesn't stop.",
            ">... [gunfire]\r\n>... <size=200%><color=red>Watch the vent line! They're dropping in!</color></size>\r\n>... Cover the cradle, not me!",
            ">... Readout jumped from twenty to nineteen.\r\n>... It's going backwards.\r\n>... <size=200%><color=red>Then we're here a lot longer.</color></size>",
            ">... <size=200%><color=red>Stop shooting past it!</color></size>\r\n>... One stray round in that housing and it's over.\r\n>... Then push them back further.",
            ">... Same corridor, other direction.\r\n>... <size=200%><color=red>Everything we walked past is awake now.</color></size>\r\n>... [snarling]",
            ">... It's warm now. It wasn't warm before.\r\n>... Wrap it and move.\r\n>... <size=200%><color=red>Don't look at it. Just move.</color></size>",
            ">... <size=200%><color=red>Why does it need us to carry it?</color></size>\r\n>... Because machines don't have hands down here.\r\n>... That's not what I asked.",
            ">... [thudding]\r\n>... <size=200%><color=red>You dropped it! Is it cracked?!</color></size>\r\n>... Shut up and check the seams.",
            ">... Nobody said it would weigh this much.\r\n>... Enough that you'll never outrun anything.\r\n>... <size=200%><color=red>Then don't let anything get close.</color></size>",
            ">... <size=200%><color=red>The map ends here. The machine doesn't.</color></size>\r\n>... So we go off the map.\r\n>... [breathing]",
            ">... Sixty-two percent.\r\n>... <size=200%><color=red>Stop reading it out. It's not going faster.</color></size>\r\n>... I need something to say.",
            ">... It wants a hand on the panel while it takes it.\r\n>... Of course it does.\r\n>... <size=200%><color=red>Somebody stand there and don't flinch!</color></size>",
            ">... <size=200%><color=red>They came for the noise, not for us!</color></size>\r\n>... Doesn't matter. We're standing in it.\r\n>... [gunfire]",
            ">... Green light. It's finished with it.\r\n>... <size=200%><color=red>Then take it before something else does!</color></size>\r\n>... [clattering]",
            ">... [whirring]\r\n>... The cradle's shuddering.\r\n>... <size=200%><color=red>If it fails, we did all this for nothing.</color></size>",
            ">... <size=200%><color=red>The way back is longer than the way in.</color></size>\r\n>... It always is.\r\n>... Nobody stops. Not for anything.",
            ">... Hands are full, I'm blind on the left.\r\n>... <size=200%><color=red>Then someone own the left!</color></size>\r\n>... [shuffling]",
            ">... Not for the complex. For whatever's under it.\r\n>... You don't know that.\r\n>... <size=200%><color=red>I know we're feeding something.</color></size>",
            ">... <size=200%><color=red>Set it down and get on the door!</color></size>\r\n>... I've got it, I've got it-\r\n>... You don't. Put it down.",
            ">... [beeping]\r\n>... <size=200%><color=red>It accepted it. It's locking down!</color></size>\r\n>... We're committed. Get to your corners.",
            ">... Rows of dead terminals. All of them dark.\r\n>... One's blinking at the back.\r\n>... <size=200%><color=red>Something down here still wants company.</color></size>",
            ">... <size=200%><color=red>How long is this cycle?!</color></size>\r\n>... Longer than we have ammunition for.\r\n>... Then make every round count.",
            ">... Housing's cracked. It's leaking.\r\n>... <size=200%><color=red>Is it still counting?!</color></size>\r\n>... [dripping]",
            ">... Second wave. Bigger.\r\n>... The readout hasn't even reached half.\r\n>... <size=200%><color=red>Nobody breaks the line!</color></size>",
            ">... <size=200%><color=red>It's changed. It's not the same.</color></size>\r\n>... Doesn't matter. It's still coming with us.\r\n>... [wheezing] I don't want to hold it.",
            ">... The manifest just calls it cargo.\r\n>... <size=200%><color=red>Nobody writes 'cargo' for something harmless.</color></size>\r\n>... Stop reading and start walking.",
            ">... Bulkhead's shut behind us.\r\n>... So we go the long way, carrying that.\r\n>... <size=200%><color=red>Through everything we left alive.</color></size>",
            ">... <size=200%><color=red>I'm the slowest thing in this complex right now.</color></size>\r\n>... Then we move at your speed.\r\n>... That's what scares me.",
            ">... It should be past this junction.\r\n>... <size=200%><color=red>Should be? You said you were sure.</color></size>\r\n>... I said it was the only guess we had.",
            ">... Pass it, pass it, pass it-\r\n>... [grunting]\r\n>... <size=200%><color=red>Both hands! BOTH HANDS!</color></size>",
            ">... <size=200%><color=red>In. It's in.</color></size>\r\n>... Then step back and let it work.\r\n>... [whirring]",
            ">... [howling]\r\n>... <size=200%><color=red>That one's not a striker!</color></size>\r\n>... Focus it down before it reaches the cradle!",
            ">... Eighty percent.\r\n>... Don't say it. The last twenty always costs the most.\r\n>... <size=200%><color=red>Reload while you still can!</color></size>",
            ">... <size=200%><color=red>Back off the machine, it's dumping heat!</color></size>\r\n>... My visor's fogging.\r\n>... [hissing]",
            ">... Halfway to the lift.\r\n>... <size=200%><color=red>Only halfway?</color></size>\r\n>... [panting] Keep counting corners, not distance.",
            ">... Cradle's open. Take it.\r\n>... You take it. I've had my turn.\r\n>... <size=200%><color=red>Somebody take it before they get here!</color></size>",
            ">... <size=200%><color=red>What if it's not supposed to leave?</color></size>\r\n>... Then someone should have told the Warden.\r\n>... I think the Warden knows.",
            ">... My grip's slipping.\r\n>... <size=200%><color=red>Do not drop it here!</color></size>\r\n>... Then get under it with me.",
            ">... [whispering] Don't run with it. They hear the footfalls.\r\n>... Walk it. Heel first.\r\n>... <size=200%><color=red>One of them just turned its head.</color></size>",
            ">... <size=200%><color=red>Follow the cables. They all feed one thing.</color></size>\r\n>... Whatever's at the end is still drawing power.\r\n>... After all this time...",
            ">... [ticking]\r\n>... <size=200%><color=red>Ninety-four. Nearly. Nearly!</color></size>\r\n>... Say nothing until it's done.",
            ">... Prompt's up. It knows what we're holding.\r\n>... How does it know?\r\n>... <size=200%><color=red>Don't ask. Just feed it.</color></size>",
            ">... <size=200%><color=red>I'm on my last magazine!</color></size>\r\n>... The cycle isn't finished.\r\n>... Then use the hammer and pray.",
            ">... Something's still moving inside it.\r\n>... <size=200%><color=red>It wasn't doing that before we came here!</color></size>\r\n>... Carry it anyway.",
            ">... Whole frame's rattling.\r\n>... [rattling]\r\n>... <size=200%><color=red>It was never built to run this long!</color></size>",
            ">... <size=200%><color=red>Don't set it down at the door!</color></size>\r\n>... The scan won't count it if it's not with us.\r\n>... Then it goes on my back.",
            ">... We hauled it down, we hauled it back.\r\n>... And we still don't know what we did.\r\n>... <size=200%><color=red>We'll find out. Everyone always does.</color></size>",

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
                    ">... My arms are shaking already.\r\n>... It's like carrying a dead man.\r\n>... <size=200%><color=red>Don't ask me to run with it!</color></size>",
                    ">... <size=200%><color=red>Whose turn is it?</color></size>\r\n>... I've hauled it since the last door.\r\n>... [panting] Somebody take it.",
                    ">... There's no seam on it. None.\r\n>... <size=200%><color=red>Whatever's inside was never meant to come out.</color></size>\r\n>... And we're the ones opening it.",
                    ">... Four of us for one sealed sphere.\r\n>... That's the trade they made.\r\n>... <size=200%><color=red>Nobody asked us about the trade.</color></size>",
                    ">... [scraping]\r\n>... <size=200%><color=red>The port's too tight, it won't seat!</color></size>\r\n>... Turn it. Turn it the other way.",
                    ">... <size=200%><color=red>It's in. Clamps have it.</color></size>\r\n>... Hands off the cradle.\r\n>... Now it starts making noise.",
                    ">... Listen to that thing work.\r\n>... It's grinding through the encryption.\r\n>... <size=200%><color=red>Every sleeper down here can hear it!</color></size>",
                    ">... [grinding]\r\n>... <size=200%><color=red>It's stuck on the same layer!</color></size>\r\n>... Then we're stuck here with it.",
                    ">... <size=200%><color=red>I asked what's on it. No answer.</color></size>\r\n>... You expected one?\r\n>... I expected something.",
                    ">... The Warden won't say a word about the contents.\r\n>... It never does.\r\n>... <size=200%><color=red>We bleed for a file we'll never read.</color></size>",
                    ">... Take it. Take it, my grip's gone.\r\n>... <size=200%><color=red>Both hands! BOTH HANDS!</color></size>\r\n>... [scrambling] I've got it. I've got it.",
                    ">... <size=200%><color=red>Did that dent it?!</color></size>\r\n>... It rolled. I only heard it roll.\r\n>... Pick it up and say nothing.",
                    ">... If the casing cracks, we threw it all away.\r\n>... So don't crack it.\r\n>... <size=200%><color=red>Then stop shoving me!</color></size>",
                    ">... I can't shoot with this in my arms.\r\n>... <size=200%><color=red>Then put it down and shoot!</color></size>\r\n>... And leave it out in the open? No.",
                    ">... <size=200%><color=red>[dragging]</color></size>\r\n>... Don't drag it. Lift it.\r\n>... You come lift it then.",
                    ">... Readout says decrypting.\r\n>... How long is that supposed to take?\r\n>... <size=200%><color=red>Longer than we have!</color></size>",
                    ">... [beeping]\r\n>... <size=200%><color=red>The counter went backwards!</color></size>\r\n>... Don't touch it. Don't touch anything.",
                    ">... <size=200%><color=red>It's warm. Why is it warm?</color></size>\r\n>... It's been in the cold since before us.\r\n>... Then something in there is still running.",
                    ">... Old research, that's all this is.\r\n>... Old research they buried a whole complex over.\r\n>... <size=200%><color=red>Lower your voice and walk.</color></size>",
                    ">... They're coming to the sound of the machine.\r\n>... <size=200%><color=red>We hold the doorway, not the room!</color></size>\r\n>... Somebody stay on the console.",
                    ">... <size=200%><color=red>Nobody leaves the intake!</color></size>\r\n>... If they break it, all of this was for nothing.\r\n>... [gunfire]",
                    ">... So we carry it back out again.\r\n>... Same weight. Less ammo.\r\n>... <size=200%><color=red>And now they know where we are.</color></size>",
                    ">... Sphere's out of the cradle.\r\n>... <size=200%><color=red>Move, before the next wave finds us!</color></size>\r\n>... Stay behind the carrier.",
                    ">... <size=200%><color=red>You drop it, you explain it.</color></size>\r\n>... To who?\r\n>... To whatever's listening.",
                    ">... [static]\r\n>... The label's scraped off. Someone wanted it gone.\r\n>... <size=200%><color=red>So we're hauling a secret.</color></size>",
                    ">... Stairs. Watch the stairs.\r\n>... <size=200%><color=red>Slow! If it goes down those steps we're done!</color></size>\r\n>... [strained breathing]",
                    ">... <size=200%><color=red>The console wants a key we don't have!</color></size>\r\n>... Then it chews through it the hard way.\r\n>... How long is the hard way?",
                    ">... It went quiet.\r\n>... That's either finished or dead.\r\n>... <size=200%><color=red>Nobody move until it says so.</color></size>",
                    ">... Someone died sealing this thing.\r\n>... <size=200%><color=red>And someone's dying to open it.</color></size>\r\n>... Just let it not be me.",
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
                    ">... It shifted. In my hands, it shifted.\r\n>... <size=200%><color=red>Things settle. Fluid moves. That's all it is.</color></size>\r\n>... That was not fluid.",
                    ">... The frost keeps growing back over the glass.\r\n>... I wiped it twice. It came back twice.\r\n>... <size=200%><color=red>Then stop wiping it and stop looking.</color></size>",
                    ">... <size=200%><color=red>It's leaking! There's a line running down my leg!</color></size>\r\n>... That's coolant. It's losing coolant.\r\n>... How long does it have?",
                    ">... Level it out! You're tipping it!\r\n>... I've got it, I've got it-\r\n>... <size=200%><color=red>Tip it again and it wakes up!</color></size>",
                    ">... If the cold goes, whatever's in there comes out.\r\n>... <size=200%><color=red>Then pray the cold holds.</color></size>\r\n>... I stopped praying at the elevator.",
                    ">... <size=200%><color=red>I'm not carrying a person.</color></size>\r\n>... It isn't a person. It's cargo.\r\n>... Then you carry the cargo.",
                    ">... We're handing something alive to that machine.\r\n>... We hand it over and we walk away.\r\n>... <size=200%><color=red>Nobody walks away from this.</color></size>",
                    ">... Don't think about the size of it.\r\n>... <size=200%><color=red>I said don't think about it!</color></size>\r\n>... [breathing] Just walk.",
                    ">... <size=200%><color=red>It tapped. From the inside.</color></size>\r\n>... [tapping]\r\n>... There. There it is again.",
                    ">... Put your hand flat against the shell.\r\n>... ...That's a heartbeat.\r\n>... <size=200%><color=red>Take your hand off it. Now.</color></size>",
                    ">... [hissing]\r\n>... <size=200%><color=red>The seal's venting! The cycle's started!</color></size>\r\n>... Get back from the cradle.",
                    ">... It's screaming. That thing is screaming.\r\n>... That's the resuscitator, that's only the machine-\r\n>... <size=200%><color=red>THAT IS NOT THE MACHINE!</color></size>",
                    ">... <size=200%><color=red>It stopped.</color></size>\r\n>... Stopped good, or stopped bad?\r\n>... [silence]",
                    ">... The stabiliser light keeps sinking to amber.\r\n>... <size=200%><color=red>Run and it goes red. So we don't run.</color></size>\r\n>... They're right behind us and we don't run?",
                    ">... <size=200%><color=red>Cradle's cold. Set it down slow.</color></size>\r\n>... The clamps have it.\r\n>... Step back. Everyone step back.",
                    ">... What kind of place keeps these on a shelf?\r\n>... A place that made a lot of them.\r\n>... <size=200%><color=red>Don't look at the other shelves.</color></size>",
                    ">... [scratching]\r\n>... <size=200%><color=red>That's not my glove on the casing!</color></size>\r\n>... Keep moving. Just keep moving.",
                    ">... <size=200%><color=red>It's small. That's the worst part.</color></size>\r\n>... Don't.\r\n>... It's so small.",
                    ">... I asked what it's for. The objective just changed.\r\n>... It doesn't explain itself.\r\n>... <size=200%><color=red>It only wants this thing breathing.</color></size>",
                    ">... The frost's gone soft down one side.\r\n>... <size=200%><color=red>It's thawing early! Get it to the machine!</color></size>\r\n>... [running]",
                    ">... Your turn. Hands under it, not on the glass.\r\n>... Why not the glass?\r\n>... <size=200%><color=red>Because it looks back through the glass.</color></size>",
                    ">... <size=200%><color=red>It's heavier now. I swear it's heavier.</color></size>\r\n>... Whatever they woke in there has grown.\r\n>... Just get it to the elevator.",
                    ">... It's making noise now. A lot of noise.\r\n>... <size=200%><color=red>Everything in the dark can hear that!</color></size>\r\n>... Then we go quiet, because it won't.",
                    ">... When this is over I want it out of my hands.\r\n>... You'll set it in the cradle and that's the end of it.\r\n>... <size=200%><color=red>It'll still know my face.</color></size>",
                }))!);
                break;
            }
        }
        #endregion
    }
}
