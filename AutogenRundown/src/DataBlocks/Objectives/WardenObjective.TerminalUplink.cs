using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;
using AutogenRundown.Patches;
using BepInEx.Logging;

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
public partial record WardenObjective
{
    public void PreBuild_TerminalUplink(BuildDirector director, Level level)
    {
        Uplink_NumberOfTerminals = (level.Tier, director.Bulkhead) switch
        {
            ("A", _) => 1,

            ("B", _) => Generator.Between(1, 2),

            ("C", Bulkhead.Main) => Generator.Between(1, 3),
            ("C", _) => Generator.Between(1, 2),

            ("D", Bulkhead.Main) => Generator.Between(1, 3),
            ("D", _) => Generator.Between(1, 3),

            ("E", Bulkhead.Main) => Generator.Between(2, 4),
            ("E", _) => Generator.Between(1, 3),

            (_, _) => 1
        };
        Uplink_NumberOfVerificationRounds = (level.Tier, Uplink_NumberOfTerminals) switch
        {
            ("A", _) => 3,

            ("B", _) => Generator.Between(3, 4),

            ("C", 1) => Generator.Between(4, 6),
            ("C", 2) => Generator.Between(4, 5),
            ("C", 3) => Generator.Between(3, 4),

            ("D", 1) => Generator.Between(5, 6),
            ("D", 2) => Generator.Between(4, 6),
            ("D", 3) => 4,

            ("E", 1) => Generator.Between(8, 12),
            ("E", 2) => Generator.Between(5, 6),
            ("E", 3) => 5,
            ("E", 4) => 5,

            (_, _) => 1,
        };
    }

    public void Build_TerminalUplink(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Find the <u>Uplink Terminals</u> [ALL_ITEMS] and establish an external uplink from each terminal");
        FindLocationInfo = "Gather information about the location of [ALL_ITEMS]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        SolveItem = "Use [ITEM_SERIAL] to create an uplink to [UPLINK_ADDRESS]";
        SolveItemHelp = "Use the UPLINK_CONNECT command to establish the connection";

        GoToWinCondition_Elevator = new Text(() => $"Neural Imprinting Protocols retrieved. Return to the point of entrance in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        // Require a team scan to start the uplink
        StartPuzzle = ChainedPuzzle.TeamScan;

        Uplink_WaveSpawnType = SurvivalWaveSpawnType.InSuppliedCourseNodeZone;

        var wave = level.Tier switch
        {
            "A" => GenericWave.Uplink_Easy,
            "B" => GenericWave.Uplink_Easy,
            _ => GenericWave.Uplink_Medium,
        };

        WavesOnActivate.Add(wave);

        // TODO: Generate proper zones, one for each uplink terminal
        var zones = level.Planner.GetZones(director.Bulkhead, "uplink_terminals")
                                 .TakeLast(Uplink_NumberOfTerminals);

        foreach (var zone in zones)
            dataLayer.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
            {
                new()
                {
                    LocalIndex = zone.ZoneNumber,
                    Weights = ZonePlacementWeights.NotAtStart
                }
            });

        #region Warden Intel Messages

        // Intel variables
        var terminalCount = Uplink_NumberOfTerminals.ToCardinal();
        var verificationRounds = Uplink_NumberOfVerificationRounds.ToCardinal();
        var midRound = Math.Min(Math.Max(1, Uplink_NumberOfVerificationRounds / 2), Uplink_NumberOfVerificationRounds - 1).ToCardinal();
        var verifyCode = Generator.Pick(level.Tier switch
        {
            "C" => TerminalUplink.FiveLetterWords,
            "D" => TerminalUplink.SixLetterWords,
            "E" => TerminalUplink.SevenLetterWords,
            _ => TerminalUplink.FourLetterWords
        })!.ToUpperInvariant();
        var hudCodesCount = (level.Tier switch
        {
            "C" => 8,
            "D" => 12,
            "E" => 15,
            _ => 6,
        }).ToCardinal();
        var correctHudCode = (level.Tier switch
        {
            "A" or "B" => Generator.Pick(new List<string>
            {
                "X01", "X02", "X03", "X04", "X05", "X06", "X07", "X08", "X09",
                "Y01", "Y02", "Y03", "Y04", "Y05", "Y06", "Y07", "Y08", "Y09",
                "Z01", "Z02", "Z03", "Z04", "Z05", "Z06", "Z07", "Z08", "Z09"
            }),
            _ => Generator.Pick(new List<string>
            {
                "A01", "A02", "A03", "A04", "A05", "A06", "A07", "A08", "A09", "A10", "A11", "A12",
                "B01", "B02", "B03", "B04", "B05", "B06", "B07", "B08", "B09", "B10", "B11", "B12",
                "C01", "C02", "C03", "C04", "C05", "C06", "C07", "C08", "C09", "C10", "C11", "C12",
                "W01", "W02", "W03", "W04", "W05", "W06", "W07", "W08", "W09", "W10", "W11", "W12",
                "X01", "X02", "X03", "X04", "X05", "X06", "X07", "X08", "X09", "X10", "X11", "X12",
                "Y01", "Y02", "Y03", "Y04", "Y05", "Y06", "Y07", "Y08", "Y09", "Y10", "Y11", "Y12",
                "Z01", "Z02", "Z03", "Z04", "Z05", "Z06", "Z07", "Z08", "Z09", "Z10", "Z11", "Z12"
            })
        })!;

        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
            // Code matching & verification panic (with variables)
            $">... Which code is it?!\r\n>... <size=200%><color=red>{correctHudCode}!</color></size>\r\n>... [typing frantically]",
            $">... The verification code is {verifyCode}.\r\n>... <size=200%><color=red>Enter it now!</color></size>\r\n>... [alarm blaring]",
            $">... Stage reset!\r\n>... We were on round {midRound}!\r\n>... <size=200%><color=red>Start over!</color></size>",
            $">... UPLINK_VERIFY {verifyCode}!\r\n>... <size=200%><color=red>That's the wrong code!</color></size>\r\n>... [groaning]",
            $">... {correctHudCode}... where is it?!\r\n>... <size=200%><color=red>I can't find it!</color></size>\r\n>... Check the HUD!",
            $">... Round {midRound} of what?!\r\n>... Just keep verifying!\r\n>... <size=200%><color=red>I can't do this!</color></size>",
            $">... It's {correctHudCode}... no wait...\r\n>... <size=200%><color=red>Stage failed!</color></size>\r\n>... [screaming]",
            $">... Type {verifyCode} exactly!\r\n>... <size=200%><color=red>Exactly!</color></size>\r\n>... [gunfire]",
            $">... How many stages left?!\r\n>... We're on {midRound}!\r\n>... <size=200%><color=red>That's too many!</color></size>",
            $">... The code is {verifyCode}!\r\n>... Are you sure?!\r\n>... <size=200%><color=red>Just type it!</color></size>",
            $">... {correctHudCode} matches {verifyCode}.\r\n>... <size=200%><color=red>Entering now!</color></size>\r\n>... [alarm intensifies]",
            $">... Wrong verification!\r\n>... It wasn't {verifyCode}?!\r\n>... <size=200%><color=red>We're back to stage one!</color></size>",
            $">... Check {correctHudCode} on the HUD!\r\n>... <size=200%><color=red>What's the password?!</color></size>\r\n>... [frantic typing]",
            $">... {verificationRounds.ToTitleCase()} rounds of this shit?!\r\n>... <size=200%><color=red>Focus!</color></size>\r\n>... [breathing heavily]",
            $">... Is it {verifyCode} or not?!\r\n>... <size=200%><color=red>Yes! Enter it!</color></size>\r\n>... [creatures shrieking]",
            $">... Stage {midRound} complete!\r\n>... <size=200%><color=red>Another one incoming!</color></size>\r\n>... [exhausted sigh]",
            $">... {correctHudCode}... I see it!\r\n>... <size=200%><color=red>Verify with {verifyCode}!</color></size>\r\n>... [typing]",
            $">... This is round {midRound}?!\r\n>... <size=200%><color=red>How many more?!</color></size>\r\n>... [gunfire]",
            $">... The terminal says {correctHudCode}.\r\n>... Then it's {verifyCode}!\r\n>... <size=200%><color=red>Hurry!</color></size>",
            $">... UPLINK_VERIFY {verifyCode}...\r\n>... <size=200%><color=red>Command accepted!</color></size>\r\n>... [alarm blaring]",
            $">... We failed stage {midRound}!\r\n>... <size=200%><color=red>Again!</color></size>\r\n>... [sobbing]",
            $">... Look for {correctHudCode}!\r\n>... <size=200%><color=red>Where is it?!</color></size>\r\n>... [static]",
            $">... {verifyCode.ToTitleCase()}... {verifyCode}...\r\n>... <size=200%><color=red>Don't forget it!</color></size>\r\n>... [creatures approaching]",
            $">... Only {verificationRounds} stages!\r\n>... <size=200%><color=red>Only?!</color></size>\r\n>... [nervous laughter]",
            $">... {correctHudCode} is the one!\r\n>... Verifying with {verifyCode}!\r\n>... <size=200%><color=red>It reset!</color></size>",
            $">... Stage {midRound} failed!\r\n>... <size=200%><color=red>Back to the start!</color></size>\r\n>... No no no...",
            $">... I typed {verifyCode}!\r\n>... <size=200%><color=red>It's wrong!</color></size>\r\n>... [panicking]",
            $">... {correctHudCode} on the terminal!\r\n>... <size=200%><color=red>Find it on the HUD!</color></size>\r\n>... [fumbling]",
            $">... We're at {midRound}!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [alarm blaring]",
            $">... It's {verifyCode}! I'm sure!\r\n>... <size=200%><color=red>Then verify!</color></size>\r\n>... [typing frantically]",
            $">... {correctHudCode}... match it!\r\n>... <size=200%><color=red>What's the password?!</color></size>\r\n>... {verifyCode.ToTitleCase()}!",
            $">... Round {midRound} down!\r\n>... <size=200%><color=red>How many left?!</color></size>\r\n>... [heavy breathing]",
            $">... The code is... {verifyCode}?\r\n>... <size=200%><color=red>You're guessing!</color></size>\r\n>... [creatures roaring]",
            $">... {correctHudCode} matches...\r\n>... <size=200%><color=red>{verifyCode}!</color></size>\r\n>... [entering command]",
            $">... We've done {midRound} already!\r\n>... <size=200%><color=red>More to go!</color></size>\r\n>... [groaning]",
            $">... Type {verifyCode} now!\r\n>... <size=200%><color=red>I'm trying!</color></size>\r\n>... [gunfire]",
            $">... {correctHudCode} is showing!\r\n>... <size=200%><color=red>Verify it!</color></size>\r\n>... [typing]",
            $">... Stage {midRound}...\r\n>... <size=200%><color=red>Reset again!</color></size>\r\n>... [screaming]",
            $">... Is the code {verifyCode}?!\r\n>... <size=200%><color=red>Yes! Type it!</color></size>\r\n>... [alarm]",
            $">... {correctHudCode} on screen!\r\n>... Password is {verifyCode}!\r\n>... <size=200%><color=red>Entering!</color></size>",
            $">... {verificationRounds.ToTitleCase()} rounds total!\r\n>... <size=200%><color=red>We'll never make it!</color></size>\r\n>... [crying]",
            $">... UPLINK_VERIFY {verifyCode}!\r\n>... <size=200%><color=red>Wrong again!</color></size>\r\n>... [creatures attacking]",
            $">... Find {correctHudCode}!\r\n>... <size=200%><color=red>I can't see it!</color></size>\r\n>... [gunfire]",
            $">... This is {midRound}?!\r\n>... <size=200%><color=red>Yes!</color></size>\r\n>... [exhausted]",
            $">... {verifyCode.ToTitleCase()}... got it!\r\n>... <size=200%><color=red>Enter it!</color></size>\r\n>... [typing frantically]",
            $">... {correctHudCode} is there!\r\n>... <size=200%><color=red>Match it quick!</color></size>\r\n>... [alarm blaring]",
            $">... Round {midRound} complete!\r\n>... <size=200%><color=red>Next stage!</color></size>\r\n>... [panting]",
            $">... The password is {verifyCode}!\r\n>... <size=200%><color=red>Verify now!</color></size>\r\n>... [creatures shrieking]",
            $">... {correctHudCode}...\r\n>... <size=200%><color=red>Where's that code?!</color></size>\r\n>... Right there! {verifyCode}!",
            $">... We're on stage {verificationRounds}!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [breathing hard]",
            $">... Type {verifyCode} exactly!\r\n>... <size=200%><color=red>I am!</color></size>\r\n>... [stage failed]",
            $">... {correctHudCode} shows...\r\n>... Password {verifyCode}!\r\n>... <size=200%><color=red>Verified!</color></size>",
            $">... {midRound.ToTitleCase()} stages left!\r\n>... <size=200%><color=red>That's impossible!</color></size>\r\n>... [gunfire]",
            $">... It's {verifyCode}!\r\n>... <size=200%><color=red>You sure?!</color></size>\r\n>... [typing]",
            $">... {correctHudCode} on terminal!\r\n>... <size=200%><color=red>Find the match!</color></size>\r\n>... [panicking]",
            $">... Stage {midRound} done!\r\n>... <size=200%><color=red>Another one?!</color></size>\r\n>... [groaning]",
            $">... UPLINK_VERIFY {verifyCode}...\r\n>... <size=200%><color=red>Failed!</color></size>\r\n>... [screaming]",
            $">... {correctHudCode} is it!\r\n>... <size=200%><color=red>Verify with {verifyCode}!</color></size>\r\n>... [alarm]",
            $">... We've done {midRound}!\r\n>... <size=200%><color=red>How many more?!</color></size>\r\n>... [exhausted sigh]",
            $">... The code... {verifyCode}!\r\n>... <size=200%><color=red>Enter it!</color></size>\r\n>... [typing frantically]",
            $">... {correctHudCode} matches...\r\n>... <size=200%><color=red>{verifyCode}!</color></size>\r\n>... [creatures approaching]",
            $">... Round {midRound}!\r\n>... <size=200%><color=red>Keep verifying!</color></size>\r\n>... [heavy breathing]",
            $">... Type {verifyCode} now!\r\n>... <size=200%><color=red>Stage reset!</color></size>\r\n>... [sobbing]",
            $">... {correctHudCode}...\r\n>... <size=200%><color=red>What's the password?!</color></size>\r\n>... {verifyCode.ToTitleCase()}!",
            $">... We're at {midRound}!\r\n>... <size=200%><color=red>Too many stages!</color></size>\r\n>... [alarm blaring]",
            $">... Password is {verifyCode}!\r\n>... <size=200%><color=red>Verify it!</color></size>\r\n>... [gunfire]",
            $">... {correctHudCode} on screen!\r\n>... <size=200%><color=red>Match it!</color></size>\r\n>... [typing]",
            $">... Stage {midRound} failed!\r\n>... <size=200%><color=red>Again!</color></size>\r\n>... No...",
            $">... It's {verifyCode}!\r\n>... <size=200%><color=red>Are you certain?!</color></size>\r\n>... [frantic]",
            $">... {correctHudCode} shows!\r\n>... Password {verifyCode}!\r\n>... <size=200%><color=red>Entering!</color></size>",
            $">... {verificationRounds.ToTitleCase()} rounds!\r\n>... <size=200%><color=red>That's madness!</color></size>\r\n>... [breathing heavily]",
            $">... UPLINK_VERIFY {verifyCode}!\r\n>... <size=200%><color=red>Accepted!</color></size>\r\n>... [alarm blaring]",
            $">... Find {correctHudCode}!\r\n>... <size=200%><color=red>I see it!</color></size>\r\n>... {verifyCode.ToTitleCase()}!",
            $">... We're on {midRound}!\r\n>... <size=200%><color=red>Focus!</color></size>\r\n>... [creatures roaring]",
            $">... The code is {verifyCode}!\r\n>... <size=200%><color=red>Type it!</color></size>\r\n>... [typing frantically]",
            $">... {correctHudCode} is there!\r\n>... <size=200%><color=red>Verify!</color></size>\r\n>... [alarm]",
            $">... Round {midRound} complete!\r\n>... <size=200%><color=red>More stages!</color></size>\r\n>... [exhausted]",

            // HUD code confusion (with variables)
            $">... There's {hudCodesCount} codes on the HUD!\r\n>... <size=200%><color=red>Which one?!</color></size>\r\n>... [panicking]",
            $">... {hudCodesCount.ToTitleCase()} different codes!\r\n>... <size=200%><color=red>I can't read them all!</color></size>\r\n>... [gunfire]",
            $">... Find {correctHudCode} in {hudCodesCount} codes!\r\n>... <size=200%><color=red>Impossible!</color></size>\r\n>... [alarm blaring]",
            $">... The HUD shows {hudCodesCount} codes!\r\n>... <size=200%><color=red>Which is right?!</color></size>\r\n>... [breathing hard]",
            $">... {hudCodesCount.ToTitleCase()} codes?!\r\n>... Look for {correctHudCode}!\r\n>... <size=200%><color=red>I can't find it!</color></size>",
            $">... Scrolling through {hudCodesCount} codes!\r\n>... <size=200%><color=red>Hurry!</color></size>\r\n>... [creatures approaching]",
            $">... {correctHudCode} is buried in {hudCodesCount} codes!\r\n>... <size=200%><color=red>Find it!</color></size>\r\n>... [frantic searching]",
            $">... {hudCodesCount.ToTitleCase()} codes on screen!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [crying]",
            $">... Which of these {hudCodesCount} is it?!\r\n>... <size=200%><color=red>{correctHudCode}!</color></size>\r\n>... [typing]",
            $">... {hudCodesCount.ToTitleCase()} codes to check!\r\n>... <size=200%><color=red>No time!</color></size>\r\n>... [alarm]",
            $">... The HUD has {hudCodesCount} codes!\r\n>... Find {correctHudCode}!\r\n>... <size=200%><color=red>I'm looking!</color></size>",
            $">... {hudCodesCount.ToTitleCase()} possibilities!\r\n>... <size=200%><color=red>Pick the right one!</color></size>\r\n>... [gunfire]",
            $">... Searching {hudCodesCount} codes!\r\n>... <size=200%><color=red>It's {correctHudCode}!</color></size>\r\n>... [typing frantically]",
            $">... {hudCodesCount.ToTitleCase()} codes displayed!\r\n>... <size=200%><color=red>Which one matches?!</color></size>\r\n>... [panicking]",
            $">... There's {hudCodesCount} of them!\r\n>... Look for {correctHudCode}!\r\n>... <size=200%><color=red>Found it!</color></size>",
            $">... {hudCodesCount.ToTitleCase()} codes to sort through!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [breathing heavily]",
            $">... {correctHudCode} is somewhere in {hudCodesCount}!\r\n>... <size=200%><color=red>Keep searching!</color></size>\r\n>... [alarm blaring]",
            $">... The HUD shows {hudCodesCount}!\r\n>... <size=200%><color=red>I can't read them fast enough!</color></size>\r\n>... [creatures shrieking]",
            $">... {hudCodesCount.ToTitleCase()} different options!\r\n>... <size=200%><color=red>Find {correctHudCode}!</color></size>\r\n>... [frantic]",
            $">... Checking {hudCodesCount} codes!\r\n>... <size=200%><color=red>Hurry up!</color></size>\r\n>... [gunfire]",
            $">... {hudCodesCount.ToTitleCase()} codes on the HUD!\r\n>... {correctHudCode} is there!\r\n>... <size=200%><color=red>Where?!</color></size>",
            $">... Too many codes! {hudCodesCount}!\r\n>... <size=200%><color=red>Just pick one!</color></size>\r\n>... [sobbing]",
            $">... {hudCodesCount.ToTitleCase()} to choose from!\r\n>... <size=200%><color=red>It's {correctHudCode}!</color></size>\r\n>... [typing]",
            $">... The HUD lists {hudCodesCount}!\r\n>... <size=200%><color=red>Find the match!</color></size>\r\n>... [alarm]",
            $">... {hudCodesCount.ToTitleCase()} codes displayed!\r\n>... Look for {correctHudCode}!\r\n>... <size=200%><color=red>I see it!</color></size>",
            $">... Scrolling {hudCodesCount} codes!\r\n>... <size=200%><color=red>No time for this!</color></size>\r\n>... [creatures approaching]",
            $">... {correctHudCode} among {hudCodesCount}!\r\n>... <size=200%><color=red>Find it quick!</color></size>\r\n>... [breathing hard]",
            $">... {hudCodesCount.ToTitleCase()} codes?!\r\n>... <size=200%><color=red>That's too many!</color></size>\r\n>... [panicking]",
            $">... Which {hudCodesCount} code?!\r\n>... <size=200%><color=red>{correctHudCode}!</color></size>\r\n>... [typing frantically]",
            $">... The HUD has {hudCodesCount}!\r\n>... <size=200%><color=red>I can't find {correctHudCode}!</color></size>\r\n>... [alarm blaring]",
            $">... {hudCodesCount.ToTitleCase()} possibilities!\r\n>... <size=200%><color=red>Check them all!</color></size>\r\n>... [gunfire]",
            $">... Looking through {hudCodesCount}!\r\n>... <size=200%><color=red>There! {correctHudCode}!</color></size>\r\n>... [typing]",
            $">... {hudCodesCount.ToTitleCase()} codes on screen!\r\n>... Find {correctHudCode}!\r\n>... <size=200%><color=red>Searching!</color></size>",
            $">... {correctHudCode} is in {hudCodesCount} codes!\r\n>... <size=200%><color=red>Where?!</color></size>\r\n>... [frantic searching]",
            $">... The HUD shows {hudCodesCount}!\r\n>... <size=200%><color=red>Too many options!</color></size>\r\n>... [breathing heavily]",
            $">... {hudCodesCount.ToTitleCase()} codes to check!\r\n>... <size=200%><color=red>Find it!</color></size>\r\n>... [alarm]",
            $">... Which of {hudCodesCount} codes?!\r\n>... <size=200%><color=red>It's {correctHudCode}!</color></size>\r\n>... [typing]",
            $">... {hudCodesCount.ToTitleCase()} different codes!\r\n>... <size=200%><color=red>I can't see {correctHudCode}!</color></size>\r\n>... [creatures roaring]",
            $">... Sorting {hudCodesCount} codes!\r\n>... <size=200%><color=red>No time!</color></size>\r\n>... [panicking]",
            $">... {hudCodesCount.ToTitleCase()} on the HUD!\r\n>... Look for {correctHudCode}!\r\n>... <size=200%><color=red>Found it!</color></size>",
            $">... {correctHudCode} among {hudCodesCount} codes!\r\n>... <size=200%><color=red>Verify it!</color></size>\r\n>... [typing frantically]",
            $">... The HUD lists {hudCodesCount}!\r\n>... <size=200%><color=red>Which is correct?!</color></size>\r\n>... [alarm blaring]",
            $">... {hudCodesCount.ToTitleCase()} codes showing!\r\n>... <size=200%><color=red>Find {correctHudCode}!</color></size>\r\n>... [gunfire]",
            $">... {hudCodesCount.ToTitleCase()} to choose from!\r\n>... <size=200%><color=red>Pick one!</color></size>\r\n>... [breathing hard]",
            $">... Looking at {hudCodesCount} codes!\r\n>... {correctHudCode} is there!\r\n>... <size=200%><color=red>Where?!</color></size>",
            $">... {hudCodesCount.ToTitleCase()} codes on display!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [crying]",
            $">... Find {correctHudCode} in {hudCodesCount}!\r\n>... <size=200%><color=red>Searching!</color></size>\r\n>... [alarm]",
            $">... {hudCodesCount.ToTitleCase()} codes listed!\r\n>... <size=200%><color=red>Which one?!</color></size>\r\n>... [creatures approaching]",
            $">... The HUD shows {hudCodesCount}!\r\n>... <size=200%><color=red>I see {correctHudCode}!</color></size>\r\n>... [typing]",
            $">... {hudCodesCount.ToTitleCase()} different ones!\r\n>... <size=200%><color=red>Find the right code!</color></size>\r\n>... [frantic]",
            $">... Checking {hudCodesCount} codes!\r\n>... {correctHudCode} is it!\r\n>... <size=200%><color=red>Verify!</color></size>",
            $">... {hudCodesCount.ToTitleCase()} codes on the HUD!\r\n>... <size=200%><color=red>I can't find it!</color></size>\r\n>... [panicking]",
            $">... {correctHudCode} somewhere in {hudCodesCount}!\r\n>... <size=200%><color=red>Keep looking!</color></size>\r\n>... [alarm blaring]",
            $">... The HUD has {hudCodesCount}!\r\n>... <size=200%><color=red>Too many codes!</color></size>\r\n>... [gunfire]",
            $">... {hudCodesCount.ToTitleCase()} possibilities!\r\n>... Find {correctHudCode}!\r\n>... <size=200%><color=red>There it is!</color></size>",
            $">... Scrolling {hudCodesCount} codes!\r\n>... <size=200%><color=red>Hurry!</color></size>\r\n>... [typing frantically]",
            $">... {hudCodesCount.ToTitleCase()} codes to sort!\r\n>... <size=200%><color=red>Which is {correctHudCode}?!</color></size>\r\n>... [breathing heavily]",
            $">... {correctHudCode} is one of {hudCodesCount}!\r\n>... <size=200%><color=red>Find it!</color></size>\r\n>... [alarm]",
            $">... {hudCodesCount.ToTitleCase()} codes displayed!\r\n>... <size=200%><color=red>I can't tell which!</color></size>\r\n>... [creatures shrieking]",
            $">... The HUD shows {hudCodesCount}!\r\n>... Look for {correctHudCode}!\r\n>... <size=200%><color=red>Searching!</color></size>",

            // Multiple terminals stress (with variables)
            $">... {terminalCount.ToTitleCase()} terminals to uplink!\r\n>... <size=200%><color=red>That's too many!</color></size>\r\n>... [exhausted sigh]",
            $">... We've done {terminalCount} already!\r\n>... <size=200%><color=red>How many more?!</color></size>\r\n>... [breathing hard]",
            $">... {terminalCount.ToTitleCase()} uplinks?!\r\n>... <size=200%><color=red>We'll never make it!</color></size>\r\n>... [crying]",
            $">... This is terminal {terminalCount}!\r\n>... <size=200%><color=red>Last one!</color></size>\r\n>... [alarm blaring]",
            $">... {terminalCount.ToTitleCase()} terminals total!\r\n>... <size=200%><color=red>I'm exhausted!</color></size>\r\n>... [panting]",
            $">... We need {terminalCount} uplinks!\r\n>... <size=200%><color=red>Split up!</color></size>\r\n>... [gunfire]",
            $">... {terminalCount.ToTitleCase()} more to go!\r\n>... <size=200%><color=red>I can't do this!</color></size>\r\n>... [sobbing]",
            $">... Terminal {terminalCount} is next!\r\n>... <size=200%><color=red>Keep moving!</color></size>\r\n>... [heavy breathing]",
            $">... {terminalCount.ToTitleCase()} uplinks required!\r\n>... <size=200%><color=red>That's insane!</color></size>\r\n>... [alarm]",
            $">... We finished {terminalCount}!\r\n>... <size=200%><color=red>More?!</color></size>\r\n>... [groaning]",
            $">... {terminalCount.ToTitleCase()} terminals!\r\n>... <size=200%><color=red>I'm out of ammo!</color></size>\r\n>... [creatures approaching]",
            $">... This is the {terminalCount} terminal!\r\n>... <size=200%><color=red>Almost done!</color></size>\r\n>... [typing frantically]",
            $">... {terminalCount.ToTitleCase()} uplinks to establish!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [panicking]",
            $">... We've hit {terminalCount} terminals!\r\n>... <size=200%><color=red>How many left?!</color></size>\r\n>... [alarm blaring]",
            $">... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>I'm done!</color></size>\r\n>... [exhausted]",
            $">... Terminal {terminalCount} complete!\r\n>... <size=200%><color=red>Next one!</color></size>\r\n>... [breathing heavily]",
            $">... {terminalCount.ToTitleCase()} terminals?!\r\n>... <size=200%><color=red>Split the team!</color></size>\r\n>... [gunfire]",
            $">... We're on terminal {terminalCount}!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [panting]",
            $">... {terminalCount.ToTitleCase()} uplinks needed!\r\n>... <size=200%><color=red>That's impossible!</color></size>\r\n>... [crying]",
            $">... Finished {terminalCount} terminals!\r\n>... <size=200%><color=red>More to go!</color></size>\r\n>... [alarm]",
            $">... {terminalCount.ToTitleCase()} terminals total!\r\n>... <size=200%><color=red>I can't!</color></size>\r\n>... [sobbing]",
            $">... Terminal {terminalCount} is active!\r\n>... <size=200%><color=red>Next!</color></size>\r\n>... [creatures roaring]",
            $">... {terminalCount.ToTitleCase()} uplinks?!\r\n>... <size=200%><color=red>We're all gonna die!</color></size>\r\n>... [breathing hard]",
            $">... We've done {terminalCount}!\r\n>... <size=200%><color=red>Another one?!</color></size>\r\n>... [groaning]",
            $">... {terminalCount.ToTitleCase()} terminals required!\r\n>... <size=200%><color=red>Too much!</color></size>\r\n>... [alarm blaring]",
            $">... This is {terminalCount}!\r\n>... <size=200%><color=red>Last terminal!</color></size>\r\n>... [typing frantically]",
            $">... {terminalCount.ToTitleCase()} total uplinks!\r\n>... <size=200%><color=red>I'm exhausted!</color></size>\r\n>... [panting]",
            $">... Terminal {terminalCount} complete!\r\n>... <size=200%><color=red>How many more?!</color></size>\r\n>... [gunfire]",
            $">... {terminalCount.ToTitleCase()} terminals to go!\r\n>... <size=200%><color=red>We won't make it!</color></size>\r\n>... [crying]",
            $">... We're at {terminalCount} terminals!\r\n>... <size=200%><color=red>Keep moving!</color></size>\r\n>... [alarm]",
            $">... {terminalCount.ToTitleCase()} uplinks needed!\r\n>... <size=200%><color=red>Split up!</color></size>\r\n>... [creatures approaching]",
            $">... Finished terminal {terminalCount}!\r\n>... <size=200%><color=red>Next one!</color></size>\r\n>... [breathing heavily]",
            $">... {terminalCount.ToTitleCase()} terminals?!\r\n>... <size=200%><color=red>That's madness!</color></size>\r\n>... [sobbing]",
            $">... We've completed {terminalCount}!\r\n>... <size=200%><color=red>More?!</color></size>\r\n>... [exhausted sigh]",
            $">... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>I can't do this!</color></size>\r\n>... [alarm blaring]",
            $">... Terminal {terminalCount} is done!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [gunfire]",
            $">... {terminalCount.ToTitleCase()} uplinks to establish!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [panting]",
            $">... We're on {terminalCount}!\r\n>... <size=200%><color=red>Almost there!</color></size>\r\n>... [typing frantically]",
            $">... {terminalCount.ToTitleCase()} terminals required!\r\n>... <size=200%><color=red>I'm done!</color></size>\r\n>... [crying]",
            $">... Completed {terminalCount} terminals!\r\n>... <size=200%><color=red>Another?!</color></size>\r\n>... [alarm]",

            // Verification round pressure (with variables)
            $">... {midRound.ToTitleCase()} more rounds!\r\n>... <size=200%><color=red>I can't!</color></size>\r\n>... [sobbing]",
            $">... We're on round {midRound}!\r\n>... <size=200%><color=red>How many total?!</color></size>\r\n>... [alarm blaring]",
            $">... {midRound.ToTitleCase()} stages left!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [breathing hard]",
            $">... Round {midRound} failed!\r\n>... <size=200%><color=red>Back to start!</color></size>\r\n>... [screaming]",
            $">... {verificationRounds.ToTitleCase()} verifications?!\r\n>... <size=200%><color=red>That's impossible!</color></size>\r\n>... [crying]",
            $">... We're at stage {midRound}!\r\n>... <size=200%><color=red>Keep verifying!</color></size>\r\n>... [gunfire]",
            $">... {verificationRounds.ToTitleCase()} rounds total!\r\n>... <size=200%><color=red>We'll never finish!</color></size>\r\n>... [panting]",
            $">... Round {midRound} complete!\r\n>... <size=200%><color=red>Next stage!</color></size>\r\n>... [exhausted]",
            $">... {midRound.ToTitleCase()} more to go!\r\n>... <size=200%><color=red>I'm done!</color></size>\r\n>... [alarm]",
            $">... We're on {midRound}!\r\n>... <size=200%><color=red>Focus!</color></size>\r\n>... [creatures approaching]",
            $">... {verificationRounds.ToTitleCase()} stages?!\r\n>... <size=200%><color=red>That's madness!</color></size>\r\n>... [sobbing]",
            $">... Round {midRound} reset!\r\n>... <size=200%><color=red>Start over!</color></size>\r\n>... [groaning]",
            $">... {midRound.ToTitleCase()} verifications left!\r\n>... <size=200%><color=red>Too much!</color></size>\r\n>... [breathing heavily]",
            $">... We finished {midRound}!\r\n>... <size=200%><color=red>More rounds?!</color></size>\r\n>... [alarm blaring]",
            $">... {verificationRounds.ToTitleCase()} total rounds!\r\n>... <size=200%><color=red>I can't do this!</color></size>\r\n>... [crying]",
            $">... Stage {midRound} is next!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [gunfire]",
            $">... {midRound.ToTitleCase()} more verifications!\r\n>... <size=200%><color=red>We're exhausted!</color></size>\r\n>... [panting]",
            $">... Round {midRound} is active!\r\n>... <size=200%><color=red>Verify now!</color></size>\r\n>... [typing frantically]",
            $">... {verificationRounds.ToTitleCase()} stages required!\r\n>... <size=200%><color=red>That's too many!</color></size>\r\n>... [alarm]",
            $">... We're at {midRound}!\r\n>... <size=200%><color=red>Almost done!</color></size>\r\n>... [creatures roaring]",
            $">... {midRound.ToTitleCase()} rounds left!\r\n>... <size=200%><color=red>I'm out of ammo!</color></size>\r\n>... [breathing hard]",
            $">... Round {midRound} down!\r\n>... <size=200%><color=red>Next one!</color></size>\r\n>... [exhausted sigh]",
            $">... {verificationRounds.ToTitleCase()} verifications?!\r\n>... <size=200%><color=red>We'll die first!</color></size>\r\n>... [sobbing]",
            $">... We've hit round {midRound}!\r\n>... <size=200%><color=red>Keep verifying!</color></size>\r\n>... [alarm blaring]",
            $">... {verificationRounds.ToTitleCase()} stages total!\r\n>... <size=200%><color=red>I'm done!</color></size>\r\n>... [crying]",
            $">... Round {midRound} failed!\r\n>... <size=200%><color=red>Reset!</color></size>\r\n>... [screaming]",
            $">... {midRound.ToTitleCase()} more rounds!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [gunfire]",
            $">... We're on stage {midRound}!\r\n>... <size=200%><color=red>Focus!</color></size>\r\n>... [panting]",
            $">... {midRound.ToTitleCase()} verifications left!\r\n>... <size=200%><color=red>That's impossible!</color></size>\r\n>... [alarm]",
            $">... Round {midRound} complete!\r\n>... <size=200%><color=red>Another stage!</color></size>\r\n>... [breathing heavily]",
            $">... {verificationRounds.ToTitleCase()} total stages!\r\n>... <size=200%><color=red>We won't make it!</color></size>\r\n>... [sobbing]",
            $">... We're at {midRound}!\r\n>... <size=200%><color=red>Keep going!</color></size>\r\n>... [creatures approaching]",
            $">... {verificationRounds.ToTitleCase()} rounds required!\r\n>... <size=200%><color=red>I can't!</color></size>\r\n>... [crying]",
            $">... Round {midRound} is next!\r\n>... <size=200%><color=red>Verify!</color></size>\r\n>... [typing frantically]",
            $">... {midRound.ToTitleCase()} more to go!\r\n>... <size=200%><color=red>I'm exhausted!</color></size>\r\n>... [alarm blaring]",
            $">... We finished {midRound}!\r\n>... <size=200%><color=red>More?!</color></size>\r\n>... [groaning]",
            $">... {midRound.ToTitleCase()} stages left!\r\n>... <size=200%><color=red>Too much!</color></size>\r\n>... [gunfire]",
            $">... Round {midRound} reset!\r\n>... <size=200%><color=red>Start over!</color></size>\r\n>... [breathing hard]",
            $">... {verificationRounds.ToTitleCase()} verifications?!\r\n>... <size=200%><color=red>That's madness!</color></size>\r\n>... [panting]",
            $">... We're on {midRound}!\r\n>... <size=200%><color=red>Almost there!</color></size>\r\n>... [exhausted]",
            $">... {verificationRounds.ToTitleCase()} total rounds!\r\n>... <size=200%><color=red>I'm done!</color></size>\r\n>... [alarm]",
            $">... Round {midRound} is active!\r\n>... <size=200%><color=red>Keep verifying!</color></size>\r\n>... [creatures roaring]",
            $">... {midRound.ToTitleCase()} more stages!\r\n>... <size=200%><color=red>We'll never finish!</color></size>\r\n>... [crying]",
            $">... We're at stage {midRound}!\r\n>... <size=200%><color=red>Focus!</color></size>\r\n>... [sobbing]",
            $">... {midRound.ToTitleCase()} rounds left!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [alarm blaring]",
            $">... Round {midRound} complete!\r\n>... <size=200%><color=red>Next!</color></size>\r\n>... [breathing heavily]",

            // Combat during uplink (no variables)
            ">... [gunfire]\r\n>... <size=200%><color=red>I can't see the terminal!</color></size>\r\n>... [creatures shrieking]",
            ">... Someone cover me!\r\n>... <size=200%><color=red>I'm typing!</color></size>\r\n>... [alarm blaring]",
            ">... They're on the terminal!\r\n>... <size=200%><color=red>Get them off!</color></size>\r\n>... [gunfire]",
            ">... I can't read the codes!\r\n>... <size=200%><color=red>Too much blood on the screen!</color></size>\r\n>... [screaming]",
            ">... [typing frantically]\r\n>... <size=200%><color=red>Giant incoming!</color></size>\r\n>... [roaring]",
            ">... Cover the terminal!\r\n>... <size=200%><color=red>I'm trying to type!</color></size>\r\n>... [creatures attacking]",
            ">... [gunfire]\r\n>... Can't see the screen!\r\n>... <size=200%><color=red>Move!</color></size>",
            ">... They're everywhere!\r\n>... <size=200%><color=red>Just finish the uplink!</color></size>\r\n>... [alarm]",
            ">... [typing]\r\n>... <size=200%><color=red>Behind you!</color></size>\r\n>... [creatures shrieking]",
            ">... I need more time!\r\n>... <size=200%><color=red>We don't have time!</color></size>\r\n>... [gunfire]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Keep them back!</color></size>\r\n>... [breathing heavily]",
            ">... Someone defend this terminal!\r\n>... <size=200%><color=red>I'm out of ammo!</color></size>\r\n>... [creatures approaching]",
            ">... [typing frantically]\r\n>... They're breaking through!\r\n>... <size=200%><color=red>Almost done!</color></size>",
            ">... <size=200%><color=red>Striker on the terminal!</color></size>\r\n>... [gunfire]\r\n>... [screaming]",
            ">... I can't concentrate!\r\n>... <size=200%><color=red>Just type!</color></size>\r\n>... [alarm blaring]",
            ">... [creatures roaring]\r\n>... <size=200%><color=red>Cover me!</color></size>\r\n>... [typing]",
            ">... They're attacking the terminal!\r\n>... <size=200%><color=red>Protect it!</color></size>\r\n>... [gunfire]",
            ">... [alarm]\r\n>... I need to finish this!\r\n>... <size=200%><color=red>Hurry!</color></size>",
            ">... <size=200%><color=red>Charger incoming!</color></size>\r\n>... [typing frantically]\r\n>... [creatures shrieking]",
            ">... Keep them away!\r\n>... <size=200%><color=red>I'm typing!</color></size>\r\n>... [gunfire]",

            // Alarm & consequences (no variables)
            ">... UPLINK_CONNECT entered!\r\n>... <size=200%><color=red>Alarm triggered!</color></size>\r\n>... [klaxon blaring]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... [creatures shrieking]",
            ">... Connection established!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... [panic]",
            ">... The alarm won't stop!\r\n>... <size=200%><color=red>It's an error alarm!</color></size>\r\n>... [breathing heavily]",
            ">... [klaxon]\r\n>... <size=200%><color=red>Waves incoming!</color></size>\r\n>... [gunfire]",
            ">... Uplink initiated!\r\n>... <size=200%><color=red>Immediate response!</color></size>\r\n>... [alarm blaring]",
            ">... [alarm]\r\n>... Can't deactivate it!\r\n>... <size=200%><color=red>Error alarm!</color></size>",
            ">... Connection triggered it!\r\n>... <size=200%><color=red>They're spawning!</color></size>\r\n>... [creatures roaring]",
            ">... [klaxon blaring]\r\n>... <size=200%><color=red>Continuous alarm!</color></size>\r\n>... [panicking]",
            ">... The uplink caused this!\r\n>... <size=200%><color=red>No going back!</color></size>\r\n>... [alarm]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Wave after wave!</color></size>\r\n>... [gunfire]",
            ">... Error alarm active!\r\n>... <size=200%><color=red>Can't shut it down!</color></size>\r\n>... [creatures shrieking]",
            ">... [klaxon]\r\n>... Connection successful!\r\n>... <size=200%><color=red>But at what cost?!</color></size>",
            ">... Alarm won't stop!\r\n>... <size=200%><color=red>They keep coming!</color></size>\r\n>... [breathing heavily]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Immediate spawn!</color></size>\r\n>... [creatures approaching]",
            ">... Uplink triggered it!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... [panic]",
            ">... [klaxon blaring]\r\n>... <size=200%><color=red>Continuous waves!</color></size>\r\n>... [gunfire]",
            ">... The alarm is permanent!\r\n>... <size=200%><color=red>We're doomed!</color></size>\r\n>... [sobbing]",
            ">... [alarm]\r\n>... <size=200%><color=red>They're everywhere!</color></size>\r\n>... [creatures roaring]",
            ">... Connection caused this!\r\n>... <size=200%><color=red>No stopping it!</color></size>\r\n>... [alarm blaring]",

            // General panic & failure (no variables)
            ">... [screaming]\r\n>... <size=200%><color=red>Fall back!</color></size>\r\n>... [gunfire]",
            ">... We're all dead!\r\n>... <size=200%><color=red>Just run!</color></size>\r\n>... [creatures shrieking]",
            ">... [sobbing]\r\n>... I can't do this anymore!\r\n>... <size=200%><color=red>Get up!</color></size>",
            ">... They're overrunning us!\r\n>... <size=200%><color=red>Retreat!</color></size>\r\n>... [alarm blaring]",
            ">... [breathing heavily]\r\n>... <size=200%><color=red>I'm not gonna make it!</color></size>\r\n>... [creatures approaching]",
            ">... This is impossible!\r\n>... <size=200%><color=red>We're surrounded!</color></size>\r\n>... [gunfire]",
            ">... [crying]\r\n>... <size=200%><color=red>Leave me!</color></size>\r\n>... [screaming]",
            ">... We failed!\r\n>... <size=200%><color=red>Run!</color></size>\r\n>... [creatures roaring]",
            ">... [panic]\r\n>... <size=200%><color=red>They're everywhere!</color></size>\r\n>... [alarm]",
            ">... I'm out!\r\n>... <size=200%><color=red>Me too!</color></size>\r\n>... [creatures shrieking]",
            ">... [gunfire]\r\n>... <size=200%><color=red>It's over!</color></size>\r\n>... [screaming]",
            ">... We're not making it!\r\n>... <size=200%><color=red>Just keep moving!</color></size>\r\n>... [breathing hard]",
            ">... [sobbing]\r\n>... <size=200%><color=red>I can't see!</color></size>\r\n>... [alarm blaring]",
            ">... They got everyone!\r\n>... <size=200%><color=red>I'm the last one!</color></size>\r\n>... [creatures approaching]",
            ">... [panic]\r\n>... <size=200%><color=red>We're trapped!</color></size>\r\n>... [gunfire]",
            ">... This is the end!\r\n>... <size=200%><color=red>No!</color></size>\r\n>... [screaming]",
            ">... [breathing heavily]\r\n>... <size=200%><color=red>I'm done for!</color></size>\r\n>... [creatures roaring]",
            ">... We're finished!\r\n>... <size=200%><color=red>Run!</color></size>\r\n>... [alarm]",
            ">... [crying]\r\n>... <size=200%><color=red>Help me!</color></size>\r\n>... [creatures shrieking]",
            ">... It's hopeless!\r\n>... <size=200%><color=red>Keep fighting!</color></size>\r\n>... [gunfire]",

            // Atmospheric & sound effects (no variables)
            ">... [static]\r\n>... <size=200%><color=red>Connection lost!</color></size>\r\n>... [silence]",
            ">... [alarm blaring]\r\n>... [typing frantically]\r\n>... <size=200%><color=red>Almost there!</color></size>",
            ">... [creatures shrieking]\r\n>... <size=200%><color=red>They heard us!</color></size>\r\n>... [gunfire]",
            ">... [whispering]\r\n>... The terminal is active.\r\n>... <size=200%><color=red>Start the uplink!</color></size>",
            ">... [breathing heavily]\r\n>... <size=200%><color=red>I can't breathe!</color></size>\r\n>... [alarm]",
            ">... [klaxon blaring]\r\n>... <size=200%><color=red>Here they come!</color></size>\r\n>... [creatures approaching]",
            ">... [typing]\r\n>... [alarm]\r\n>... <size=200%><color=red>No time!</color></size>",
            ">... [static crackling]\r\n>... <size=200%><color=red>Network failing!</color></size>\r\n>... [panic]",
            ">... [creatures roaring]\r\n>... <size=200%><color=red>Giants!</color></size>\r\n>... [gunfire]",
            ">... [alarm blaring]\r\n>... [breathing hard]\r\n>... <size=200%><color=red>Keep going!</color></size>",
            ">... [whispering]\r\n>... <size=200%><color=red>They're waking up!</color></size>\r\n>... [creatures stirring]",
            ">... [typing frantically]\r\n>... <size=200%><color=red>Faster!</color></size>\r\n>... [alarm]",
            ">... [silence]\r\n>... <size=200%><color=red>Something's wrong!</color></size>\r\n>... [creatures shrieking]",
            ">... [klaxon]\r\n>... [gunfire]\r\n>... <size=200%><color=red>Incoming!</color></size>",
            ">... [static]\r\n>... <size=200%><color=red>Terminal offline!</color></size>\r\n>... [panic]",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>Network unstable!</color></size>\r\n>... [typing frantically]",
            ">... [creatures approaching]\r\n>... <size=200%><color=red>They know we're here!</color></size>\r\n>... [gunfire]",
        }))!);

        #endregion
    }
}
