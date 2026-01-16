using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.Extensions;
using AutogenRundown.Patches;

namespace AutogenRundown.DataBlocks.Objectives;

/**
 * Objective: CorruptedTerminalUplink
 *
 *
 * Terminal uplink but the codes get sent to a second terminal
 */
public partial record WardenObjective
{
    public void PreBuild_CorruptedTerminalUplink(BuildDirector director, Level level)
    {
        Uplink_NumberOfTerminals = (level.Tier, director.Bulkhead) switch
        {
            ("A", _) => 1,
            ("B", _) => 1,

            ("C", Bulkhead.Main) => Generator.Between(1, 2),
            ("C", _) => 1,

            ("D", Bulkhead.Main) => 2,
            ("D", _) => Generator.Between(1, 2),

            ("E", Bulkhead.Main) => 3,
            ("E", _) => 2,

            (_, _) => 1
        };
        Uplink_NumberOfVerificationRounds = (level.Tier, Uplink_NumberOfTerminals) switch
        {
            ("A", _) => 2,

            ("B", _) => 3,

            ("C", 1) => 4,
            ("C", 2) => 2,

            ("D", 1) => 4,
            ("D", 2) => 3,

            ("E", 2) => 4,
            ("E", 3) => 5,

            (_, _) => 1,
        };
    }

    public void Build_CorruptedTerminalUplink(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        if (Uplink_NumberOfTerminals > 1)
        {
            MainObjective = new Text("Find the <u>Uplink Terminals</u> [ALL_ITEMS] and establish an external uplink from each terminal");
            FindLocationInfo = "Gather information about the location of the terminals";
        }
        else
        {
            MainObjective = new Text("Find the <u>Uplink Terminal</u> [ALL_ITEMS] and establish an external uplink");
            FindLocationInfo = "Gather information about the location of the terminal";
        }

        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = new Text("Navigate to [ITEM_ZONE] and find [ALL_ITEMS]");
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Use maintenance terminal command PING to find [ITEM_SERIAL]";
        InZoneFindItemHelp = "CORTEX INTERFACE ESTABLISHED";
        SolveItem = "Use [ITEM_SERIAL] to create an uplink to [UPLINK_ADDRESS]";
        SolveItemHelp = "Use the UPLINK_CONNECT command to establish the connection";

        GoToWinCondition_Elevator = new Text(() =>
            $"Neural Imprinting Protocols retrieved. Return to the point of entrance in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        GoToWinConditionHelp_Elevator =
            "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = new Text(() =>
            $"Neural Imprinting Protocols retrieved. Go to the forward exit point in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        GoToWinConditionHelp_CustomGeo =
            "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition";

        var nodes = level.Planner.GetZonesByTag(director.Bulkhead, "uplink_terminal")
            .TakeLast(Uplink_NumberOfTerminals).ToList();

        for (var i = 0; i < Uplink_NumberOfTerminals; i++)
        {
            var node = nodes[i % nodes.Count];
            var zone = level.Planner.GetZone(node);

            // Always add another terminal for the uplink
            zone.TerminalPlacements.Add(new TerminalPlacement());

            if (i < nodes.Count)
            {
                // Add extra terminal placements for the verification codes
                // This is only if we didn't add them yet
                zone.TerminalPlacements.Add(new TerminalPlacement());
                zone.TerminalPlacements.Add(new TerminalPlacement());
            }

            dataLayer.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
            {
                new()
                {
                    LocalIndex = node.ZoneNumber,
                    Weights = ZonePlacementWeights.NotAtStart
                }
            });
        }

        // Wave configuration for corrupted uplink (easier than regular uplink due to team split)
        // Settings are one tier lower and spawn delays are longer to give split team breathing room
        var (settings, spawnDelay) = (level.Tier, director.Bulkhead) switch
        {
            ("A", _) => (WaveSettings.Baseline_Easy, 4.0),
            ("B", Bulkhead.Main) => (WaveSettings.Baseline_Easy, 3.5),
            ("B", _) => (WaveSettings.Baseline_Normal, 3.0),
            ("C", Bulkhead.Main) => (WaveSettings.Baseline_Normal, 3.0),
            ("C", _) => (WaveSettings.Baseline_Hard, 3.0),
            ("D", Bulkhead.Main) => (WaveSettings.Baseline_Hard, 3.0),
            ("D", _) => (WaveSettings.Baseline_Hard, 2.5),
            ("E", _) => (WaveSettings.Baseline_VeryHard, 2.5),
            _ => (WaveSettings.Baseline_Normal, 3.0)
        };

        // Always add primary baseline wave
        WavesOnActivate.Add(new GenericWave
        {
            Settings = settings,
            Population = WavePopulation.Baseline,
            SpawnDelay = spawnDelay,
            TriggerAlarm = true
        });

        // Fewer secondary waves than regular uplink due to team split requirement
        if (level.Tier is "D" or "E")
        {
            // Add charger wave if chargers are enabled (Overload only for corrupted)
            if (level.Settings.HasChargers() && director.Bulkhead == Bulkhead.Overload)
            {
                WavesOnActivate.Add(new GenericWave
                {
                    Settings = settings,
                    Population = WavePopulation.Baseline_Chargers,
                    SpawnDelay = spawnDelay + 15.0,
                    TriggerAlarm = false
                });
            }

            // Add nightmare wave if nightmares enabled (E-tier Overload only)
            if (level.Tier == "E" && level.Settings.HasNightmares() &&
                director.Bulkhead == Bulkhead.Overload)
            {
                WavesOnActivate.Add(new GenericWave
                {
                    Settings = WaveSettings.Baseline_Hard, // One tier lower than regular uplink
                    Population = WavePopulation.Baseline_Nightmare,
                    SpawnDelay = spawnDelay + 20.0,
                    TriggerAlarm = false
                });
            }
        }

        AddCompletedObjectiveWaves(level, director);
    }

    private void PostBuildIntel_CorruptedTerminalUplink(Level level)
    {
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
        var logCodesCount = (level.Tier switch
        {
            "C" => 8,
            "D" => 12,
            "E" => 15,
            _ => 6,
        }).ToCardinal();
        var correctLogCode = (level.Tier switch
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

        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
        {
            // Log File Navigation & Code Retrieval (70 messages with logCodesCount and correctLogCode)
            $">... There's {logCodesCount.ToTitleCase()} codes in the log!\r\n>... <size=200%><color=red>Which one is it?!</color></size>\r\n>... [typing frantically]",
            $">... I found the log file!\r\n>... {logCodesCount.ToTitleCase()} different codes...\r\n>... <size=200%><color=red>We need {correctLogCode}!</color></size>",
            $">... READ the verification file!\r\n>... <size=200%><color=red>{logCodesCount.ToTitleCase()} codes to sort through!</color></size>\r\n>... [scrolling]",
            $">... {correctLogCode}... that's the one!\r\n>... <size=200%><color=red>Found it in the log!</color></size>\r\n>... [typing]",
            $">... LOGS command shows the file!\r\n>... {logCodesCount.ToTitleCase()} entries...\r\n>... <size=200%><color=red>Looking for {correctLogCode}!</color></size>",
            $">... The log has {logCodesCount} codes!\r\n>... <size=200%><color=red>I can't find {correctLogCode}!</color></size>\r\n>... [panicking]",
            $">... Which terminal has the log?!\r\n>... Found it! {logCodesCount.ToTitleCase()} codes!\r\n>... <size=200%><color=red>Searching for {correctLogCode}!</color></size>",
            $">... Runner, find {correctLogCode}!\r\n>... There's {logCodesCount} codes here!\r\n>... <size=200%><color=red>Hurry!</color></size>",
            $">... {logCodesCount.ToTitleCase()} codes in the file!\r\n>... <size=200%><color=red>{correctLogCode} is the right one!</color></size>\r\n>... [typing]",
            $">... I'm at the code terminal!\r\n>... {logCodesCount.ToTitleCase()} codes!\r\n>... <size=200%><color=red>Where's {correctLogCode}?!</color></size>",
            $">... READ verification.log!\r\n>... {correctLogCode}... there it is!\r\n>... <size=200%><color=red>Got the code!</color></size>",
            $">... {logCodesCount.ToTitleCase()} codes to check!\r\n>... <size=200%><color=red>Finding {correctLogCode}!</color></size>\r\n>... [searching]",
            $">... The log file!\r\n>... {logCodesCount} codes listed!\r\n>... <size=200%><color=red>{correctLogCode} should be here!</color></size>",
            $">... I'm scrolling through {logCodesCount} codes!\r\n>... <size=200%><color=red>Looking for {correctLogCode}!</color></size>\r\n>... [typing]",
            $">... Found {correctLogCode} in the log!\r\n>... Out of {logCodesCount} codes!\r\n>... <size=200%><color=red>Relaying it now!</color></size>",
            $">... There's {logCodesCount} codes here!\r\n>... {correctLogCode}...\r\n>... <size=200%><color=red>That's the one!</color></size>",
            $">... READ command executed!\r\n>... {logCodesCount.ToTitleCase()} codes displayed!\r\n>... <size=200%><color=red>Searching for {correctLogCode}!</color></size>",
            $">... {correctLogCode} is in this log!\r\n>... Among {logCodesCount} other codes!\r\n>... <size=200%><color=red>Found it!</color></size>",
            $">... LOGS shows verification file!\r\n>... <size=200%><color=red>{logCodesCount.ToTitleCase()} codes to search!</color></size>\r\n>... [opening file]",
            $">... The code terminal is here!\r\n>... {logCodesCount} codes in the log!\r\n>... <size=200%><color=red>Need {correctLogCode}!</color></size>",
            $">... {logCodesCount.ToTitleCase()} different codes!\r\n>... <size=200%><color=red>Which one?!</color></size>\r\n>... {correctLogCode}!",
            $">... I'm reading through {logCodesCount} codes!\r\n>... <size=200%><color=red>{correctLogCode}... there!</color></size>\r\n>... [typing]",
            $">... {correctLogCode} is listed!\r\n>... In a file with {logCodesCount} codes!\r\n>... <size=200%><color=red>Confirming!</color></size>",
            $">... Searching {logCodesCount} log codes!\r\n>... <size=200%><color=red>Found {correctLogCode}!</color></size>\r\n>... [relaying]",
            $">... There's {logCodesCount} codes here!\r\n>... {correctLogCode} should be one of them!\r\n>... <size=200%><color=red>Keep looking!</color></size>",
            $">... {logCodesCount.ToTitleCase()} codes displayed!\r\n>... <size=200%><color=red>Looking for {correctLogCode}!</color></size>\r\n>... [scrolling rapidly]",
            $">... The log file has {logCodesCount} entries!\r\n>... {correctLogCode}...\r\n>... <size=200%><color=red>Got it!</color></size>",
            $">... READ shows {logCodesCount} codes!\r\n>... <size=200%><color=red>Where's {correctLogCode}?!</color></size>\r\n>... [searching]",
            $">... {correctLogCode} is the right code!\r\n>... From {logCodesCount} options!\r\n>... <size=200%><color=red>Sending it!</color></size>",
            $">... I'm at the log terminal!\r\n>... {logCodesCount.ToTitleCase()} codes!\r\n>... <size=200%><color=red>Need {correctLogCode} fast!</color></size>",
            $">... {logCodesCount} codes in this log!\r\n>... <size=200%><color=red>{correctLogCode} must be here!</color></size>\r\n>... [typing]",
            $">... Found the log!\r\n>... {logCodesCount.ToTitleCase()} codes!\r\n>... <size=200%><color=red>Searching for {correctLogCode}!</color></size>",
            $">... {correctLogCode}... yes!\r\n>... Out of {logCodesCount} codes!\r\n>... <size=200%><color=red>That's it!</color></size>",
            $">... There are {logCodesCount} codes!\r\n>... <size=200%><color=red>We need {correctLogCode}!</color></size>\r\n>... [searching frantically]",
            $">... Reading through {logCodesCount} codes!\r\n>... {correctLogCode}...\r\n>... <size=200%><color=red>Found it!</color></size>",
            $">... {logCodesCount.ToTitleCase()} codes listed!\r\n>... <size=200%><color=red>{correctLogCode} is one of them!</color></size>\r\n>... [looking]",
            $">... I see {logCodesCount} codes!\r\n>... {correctLogCode}... there!\r\n>... <size=200%><color=red>Relaying!</color></size>",
            $">... The log has {logCodesCount} entries!\r\n>... <size=200%><color=red>Looking for {correctLogCode}!</color></size>\r\n>... [scrolling]",
            $">... {correctLogCode} from the log!\r\n>... {logCodesCount.ToTitleCase()} codes total!\r\n>... <size=200%><color=red>Got it!</color></size>",
            $">... Checking {logCodesCount} codes!\r\n>... <size=200%><color=red>Where's {correctLogCode}?!</color></size>\r\n>... [typing]",
            $">... {logCodesCount.ToTitleCase()} different codes!\r\n>... {correctLogCode} is the one!\r\n>... <size=200%><color=red>Confirmed!</color></size>",
            $">... The log file shows {logCodesCount} codes!\r\n>... <size=200%><color=red>{correctLogCode}... found it!</color></size>\r\n>... [relaying]",
            $">... I'm searching {logCodesCount} codes!\r\n>... {correctLogCode}!\r\n>... <size=200%><color=red>That's the right one!</color></size>",
            $">... {logCodesCount} codes to sort!\r\n>... <size=200%><color=red>Looking for {correctLogCode}!</color></size>\r\n>... [searching]",
            $">... Found {correctLogCode}!\r\n>... Among {logCodesCount} codes!\r\n>... <size=200%><color=red>Sending!</color></size>",
            $">... There's {logCodesCount} codes here!\r\n>... <size=200%><color=red>{correctLogCode} should be listed!</color></size>\r\n>... [typing]",
            $">... {correctLogCode}... that's it!\r\n>... From {logCodesCount} total!\r\n>... <size=200%><color=red>Got the code!</color></size>",
            $">... Scanning {logCodesCount} log codes!\r\n>... <size=200%><color=red>{correctLogCode} is here!</color></size>\r\n>... [confirming]",
            $">... {logCodesCount.ToTitleCase()} codes in the file!\r\n>... {correctLogCode}...\r\n>... <size=200%><color=red>Found!</color></size>",
            $">... READ verification shows {logCodesCount} codes!\r\n>... <size=200%><color=red>Need {correctLogCode}!</color></size>\r\n>... [searching]",
            $">... {correctLogCode} is in the log!\r\n>... With {logCodesCount} other codes!\r\n>... <size=200%><color=red>Relaying now!</color></size>",
            $">... I'm checking all {logCodesCount} codes!\r\n>... <size=200%><color=red>Looking for {correctLogCode}!</color></size>\r\n>... [typing]",
            $">... {logCodesCount} codes listed!\r\n>... {correctLogCode}... yes!\r\n>... <size=200%><color=red>That's the one!</color></size>",
            $">... The log shows {logCodesCount} codes!\r\n>... <size=200%><color=red>{correctLogCode} must be one!</color></size>\r\n>... [scrolling]",
            $">... {correctLogCode} found!\r\n>... Out of {logCodesCount} codes!\r\n>... <size=200%><color=red>Sending it!</color></size>",
            $">... Searching through {logCodesCount} codes!\r\n>... <size=200%><color=red>{correctLogCode}!</color></size>\r\n>... [relaying]",
            $">... {logCodesCount.ToTitleCase()} codes in this log!\r\n>... {correctLogCode} is there!\r\n>... <size=200%><color=red>Got it!</color></size>",
            $">... I see {logCodesCount} codes!\r\n>... <size=200%><color=red>Finding {correctLogCode}!</color></size>\r\n>... [typing frantically]",
            $">... {correctLogCode} from {logCodesCount} codes!\r\n>... <size=200%><color=red>That's it!</color></size>\r\n>... [confirming]",
            $">... The log has {logCodesCount} entries!\r\n>... {correctLogCode}...\r\n>... <size=200%><color=red>Found the code!</color></size>",
            $">... Checking {logCodesCount} codes!\r\n>... <size=200%><color=red>{correctLogCode} is here!</color></size>\r\n>... [relaying]",
            $">... {logCodesCount.ToTitleCase()} different codes!\r\n>... <size=200%><color=red>Where's {correctLogCode}?!</color></size>\r\n>... [searching]",
            $">... {correctLogCode}... got it!\r\n>... From {logCodesCount} codes!\r\n>... <size=200%><color=red>Relaying!</color></size>",
            $">... I'm scanning {logCodesCount} log codes!\r\n>... {correctLogCode}!\r\n>... <size=200%><color=red>That's the right one!</color></size>",
            $">... {logCodesCount} codes to check!\r\n>... <size=200%><color=red>{correctLogCode} is listed!</color></size>\r\n>... [typing]",
            $">... Found {correctLogCode} in the log!\r\n>... {logCodesCount.ToTitleCase()} codes total!\r\n>... <size=200%><color=red>Sending!</color></size>",
            $">... The log shows {logCodesCount} codes!\r\n>... {correctLogCode}... there!\r\n>... <size=200%><color=red>Got it!</color></size>",
            $">... Searching {logCodesCount} codes!\r\n>... <size=200%><color=red>{correctLogCode} must be here!</color></size>\r\n>... [scrolling]",
            $">... {correctLogCode} is the code!\r\n>... Among {logCodesCount} options!\r\n>... <size=200%><color=red>Found it!</color></size>",
            $">... I'm checking {logCodesCount} codes!\r\n>... {correctLogCode}...\r\n>... <size=200%><color=red>That's it!</color></size>",
            $">... Log terminal located!\r\n>... {logCodesCount.ToTitleCase()} codes inside!\r\n>... <size=200%><color=red>Searching for {correctLogCode}!</color></size>",
            $">... {correctLogCode} is here!\r\n>... Out of {logCodesCount} total!\r\n>... <size=200%><color=red>Relaying!</color></size>",

            // Verification Code Matching (60 messages with verifyCode and correctLogCode)
            $">... {correctLogCode} matches {verifyCode}!\r\n>... <size=200%><color=red>That's the verification code!</color></size>\r\n>... [typing]",
            $">... The log says {correctLogCode} is {verifyCode}!\r\n>... <size=200%><color=red>Entering it now!</color></size>\r\n>... [typing frantically]",
            $">... {correctLogCode}... verification is {verifyCode}!\r\n>... <size=200%><color=red>UPLINK_VERIFY {verifyCode}!</color></size>\r\n>... [typing]",
            $">... Found it! {correctLogCode} = {verifyCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>\r\n>... [typing]",
            $">... {verifyCode} is next to {correctLogCode}!\r\n>... <size=200%><color=red>That's the code!</color></size>\r\n>... [relaying]",
            $">... The verification for {correctLogCode} is {verifyCode}!\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [typing]",
            $">... {correctLogCode}... shows {verifyCode}!\r\n>... <size=200%><color=red>Typing it now!</color></size>\r\n>... [typing]",
            $">... {verifyCode}! That's the code for {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>\r\n>... [typing]",
            $">... Log shows {correctLogCode}: {verifyCode}!\r\n>... <size=200%><color=red>Got it!</color></size>\r\n>... [typing]",
            $">... {correctLogCode} gives us {verifyCode}!\r\n>... <size=200%><color=red>Entering verification!</color></size>\r\n>... [typing frantically]",
            $">... The code is {verifyCode}!\r\n>... From {correctLogCode}!\r\n>... <size=200%><color=red>Verifying now!</color></size>",
            $">... {correctLogCode} in the log!\r\n>... Verification: {verifyCode}!\r\n>... <size=200%><color=red>Typing!</color></size>",
            $">... {verifyCode}... that's from {correctLogCode}!\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [typing]",
            $">... {correctLogCode} = {verifyCode}!\r\n>... <size=200%><color=red>UPLINK_VERIFY!</color></size>\r\n>... [typing]",
            $">... The log says {verifyCode}!\r\n>... Next to {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>",
            $">... {verifyCode} is the verification!\r\n>... From code {correctLogCode}!\r\n>... <size=200%><color=red>Entering now!</color></size>",
            $">... {correctLogCode} shows {verifyCode}!\r\n>... <size=200%><color=red>That's it!</color></size>\r\n>... [typing]",
            $">... Verification code {verifyCode}!\r\n>... From log entry {correctLogCode}!\r\n>... <size=200%><color=red>Typing!</color></size>",
            $">... {verifyCode}! Code {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>\r\n>... [typing frantically]",
            $">... {correctLogCode}: {verifyCode}!\r\n>... <size=200%><color=red>Got the verification!</color></size>\r\n>... [typing]",
            $">... The code for {correctLogCode} is {verifyCode}!\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [typing]",
            $">... {verifyCode}... from {correctLogCode}!\r\n>... <size=200%><color=red>Verifying now!</color></size>\r\n>... [typing]",
            $">... Log entry {correctLogCode}!\r\n>... Verification: {verifyCode}!\r\n>... <size=200%><color=red>Typing!</color></size>",
            $">... {correctLogCode} in the file!\r\n>... Code is {verifyCode}!\r\n>... <size=200%><color=red>Entering!</color></size>",
            $">... {verifyCode}! That's the code!\r\n>... From {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>",
            $">... {correctLogCode} = {verifyCode}!\r\n>... <size=200%><color=red>Entering verification!</color></size>\r\n>... [typing]",
            $">... The verification is {verifyCode}!\r\n>... Code {correctLogCode}!\r\n>... <size=200%><color=red>Typing now!</color></size>",
            $">... {verifyCode} from log code {correctLogCode}!\r\n>... <size=200%><color=red>Got it!</color></size>\r\n>... [typing]",
            $">... {correctLogCode}: {verifyCode}!\r\n>... <size=200%><color=red>That's the verification!</color></size>\r\n>... [typing frantically]",
            $">... Code {verifyCode}!\r\n>... From {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>",
            $">... {verifyCode}... {correctLogCode}...\r\n>... <size=200%><color=red>Match found!</color></size>\r\n>... [typing]",
            $">... Log shows {correctLogCode} is {verifyCode}!\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [typing]",
            $">... {verifyCode}! Code {correctLogCode}!\r\n>... <size=200%><color=red>Typing!</color></size>\r\n>... [typing frantically]",
            $">... {correctLogCode} gives {verifyCode}!\r\n>... <size=200%><color=red>Verifying now!</color></size>\r\n>... [typing]",
            $">... The code is {verifyCode}!\r\n>... Log entry {correctLogCode}!\r\n>... <size=200%><color=red>Entering!</color></size>",
            $">... {verifyCode} from {correctLogCode}!\r\n>... <size=200%><color=red>That's it!</color></size>\r\n>... [typing]",
            $">... {correctLogCode} shows {verifyCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>\r\n>... [typing]",
            $">... Verification {verifyCode}!\r\n>... From code {correctLogCode}!\r\n>... <size=200%><color=red>Typing!</color></size>",
            $">... {verifyCode}! {correctLogCode}!\r\n>... <size=200%><color=red>Got the match!</color></size>\r\n>... [typing frantically]",
            $">... {correctLogCode}: verification {verifyCode}!\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [typing]",
            $">... Code {verifyCode} from log {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>\r\n>... [typing]",
            $">... {verifyCode}... that's from {correctLogCode}!\r\n>... <size=200%><color=red>Typing now!</color></size>\r\n>... [typing]",
            $">... {correctLogCode} = {verifyCode}!\r\n>... <size=200%><color=red>Found the verification!</color></size>\r\n>... [typing]",
            $">... The log shows {verifyCode}!\r\n>... Code {correctLogCode}!\r\n>... <size=200%><color=red>Entering!</color></size>",
            $">... {verifyCode} is the code!\r\n>... From {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>",
            $">... {correctLogCode}: {verifyCode}!\r\n>... <size=200%><color=red>That's the verification!</color></size>\r\n>... [typing]",
            $">... Code {verifyCode}!\r\n>... Log entry {correctLogCode}!\r\n>... <size=200%><color=red>Typing!</color></size>",
            $">... {verifyCode} from {correctLogCode}!\r\n>... <size=200%><color=red>Got it!</color></size>\r\n>... [typing frantically]",
            $">... {correctLogCode} shows verification {verifyCode}!\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [typing]",
            $">... The verification is {verifyCode}!\r\n>... From code {correctLogCode}!\r\n>... <size=200%><color=red>Typing now!</color></size>",
            $">... {verifyCode}! Code {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>\r\n>... [typing]",
            $">... {correctLogCode} = {verifyCode}!\r\n>... <size=200%><color=red>Entering verification!</color></size>\r\n>... [typing]",
            $">... Log code {correctLogCode}!\r\n>... Verification {verifyCode}!\r\n>... <size=200%><color=red>Typing!</color></size>",
            $">... {verifyCode}... from {correctLogCode}!\r\n>... <size=200%><color=red>That's it!</color></size>\r\n>... [typing frantically]",
            $">... {correctLogCode}: code {verifyCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>\r\n>... [typing]",
            $">... The code {verifyCode}!\r\n>... From log {correctLogCode}!\r\n>... <size=200%><color=red>Entering!</color></size>",
            $">... {verifyCode} is next to {correctLogCode}!\r\n>... <size=200%><color=red>Got the verification!</color></size>\r\n>... [typing]",
            $">... {correctLogCode} shows {verifyCode}!\r\n>... <size=200%><color=red>Typing!</color></size>\r\n>... [typing frantically]",
            $">... Verification code {verifyCode}!\r\n>... Log entry {correctLogCode}!\r\n>... <size=200%><color=red>Verifying!</color></size>",

            // Verification Rounds Stress (50 messages with verificationRounds)
            $">... This is round {midRound}!\r\n>... <size=200%><color=red>Out of {verificationRounds}!</color></size>\r\n>... [exhausted]",
            $">... {verificationRounds.ToTitleCase()} verification rounds?!\r\n>... <size=200%><color=red>That's too many!</color></size>\r\n>... [breathing heavily]",
            $">... We're at round {midRound}!\r\n>... Still {verificationRounds} total!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {verificationRounds.ToTitleCase()} rounds to verify!\r\n>... <size=200%><color=red>This will take forever!</color></size>\r\n>... [panicking]",
            $">... Round {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>Halfway there!</color></size>\r\n>... [panting]",
            $">... We need {verificationRounds} verifications!\r\n>... <size=200%><color=red>I can't do this!</color></size>\r\n>... [gunfire]",
            $">... {verificationRounds.ToTitleCase()} verification rounds!\r\n>... <size=200%><color=red>How many left?!</color></size>\r\n>... [typing]",
            $">... This is round {midRound}!\r\n>... {verificationRounds.ToTitleCase()} total!\r\n>... <size=200%><color=red>Almost there!</color></size>",
            $">... {verificationRounds} rounds?!\r\n>... <size=200%><color=red>We'll never finish!</color></size>\r\n>... [alarm blaring]",
            $">... We're on round {midRound}!\r\n>... Out of {verificationRounds}!\r\n>... <size=200%><color=red>Keep verifying!</color></size>",
            $">... {verificationRounds.ToTitleCase()} verifications needed!\r\n>... <size=200%><color=red>This is insane!</color></size>\r\n>... [exhausted]",
            $">... Round {midRound} complete!\r\n>... <size=200%><color=red>Still {verificationRounds} total!</color></size>\r\n>... [typing]",
            $">... {verificationRounds} verification rounds!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [panicking]",
            $">... We're at {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>More to go!</color></size>\r\n>... [breathing heavily]",
            $">... {verificationRounds.ToTitleCase()} rounds to do!\r\n>... <size=200%><color=red>I'm exhausted!</color></size>\r\n>... [panting]",
            $">... Round {midRound}!\r\n>... {verificationRounds} total!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {verificationRounds} verifications?!\r\n>... <size=200%><color=red>That can't be right!</color></size>\r\n>... [typing frantically]",
            $">... This is {midRound} out of {verificationRounds}!\r\n>... <size=200%><color=red>Still more!</color></size>\r\n>... [gunfire]",
            $">... {verificationRounds.ToTitleCase()} rounds required!\r\n>... <size=200%><color=red>How many done?!</color></size>\r\n>... [typing]",
            $">... We're on round {midRound}!\r\n>... {verificationRounds.ToTitleCase()} total!\r\n>... <size=200%><color=red>Halfway!</color></size>",
            $">... {verificationRounds} verification stages!\r\n>... <size=200%><color=red>This is brutal!</color></size>\r\n>... [exhausted]",
            $">... Round {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>Keep verifying!</color></size>\r\n>... [typing]",
            $">... {verificationRounds.ToTitleCase()} rounds to complete!\r\n>... <size=200%><color=red>We're not even halfway!</color></size>\r\n>... [panicking]",
            $">... This is round {midRound}!\r\n>... Out of {verificationRounds}!\r\n>... <size=200%><color=red>Almost done!</color></size>",
            $">... {verificationRounds} verifications needed!\r\n>... <size=200%><color=red>Too many rounds!</color></size>\r\n>... [alarm blaring]",
            $">... We're at {midRound}!\r\n>... {verificationRounds} total!\r\n>... <size=200%><color=red>More to go!</color></size>",
            $">... {verificationRounds.ToTitleCase()} verification rounds!\r\n>... <size=200%><color=red>I'm running out of stamina!</color></size>\r\n>... [breathing heavily]",
            $">... Round {midRound} complete!\r\n>... {verificationRounds.ToTitleCase()} rounds total!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {verificationRounds} rounds?!\r\n>... <size=200%><color=red>This is impossible!</color></size>\r\n>... [gunfire]",
            $">... This is {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>Still more rounds!</color></size>\r\n>... [typing]",
            $">... {verificationRounds.ToTitleCase()} verifications!\r\n>... <size=200%><color=red>How many left?!</color></size>\r\n>... [exhausted]",
            $">... We're on round {midRound}!\r\n>... Out of {verificationRounds}!\r\n>... <size=200%><color=red>Halfway there!</color></size>",
            $">... {verificationRounds} verification stages!\r\n>... <size=200%><color=red>We'll never make it!</color></size>\r\n>... [panicking]",
            $">... Round {midRound}!\r\n>... {verificationRounds.ToTitleCase()} total!\r\n>... <size=200%><color=red>Almost done!</color></size>",
            $">... {verificationRounds.ToTitleCase()} rounds to verify!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [typing frantically]",
            $">... This is {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>Keep verifying!</color></size>\r\n>... [alarm blaring]",
            $">... {verificationRounds} verifications needed!\r\n>... <size=200%><color=red>I'm exhausted!</color></size>\r\n>... [breathing heavily]",
            $">... We're at round {midRound}!\r\n>... {verificationRounds} total!\r\n>... <size=200%><color=red>More to go!</color></size>",
            $">... {verificationRounds.ToTitleCase()} verification rounds!\r\n>... <size=200%><color=red>This is insane!</color></size>\r\n>... [gunfire]",
            $">... Round {midRound} complete!\r\n>... Out of {verificationRounds}!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {verificationRounds} rounds?!\r\n>... <size=200%><color=red>We're doomed!</color></size>\r\n>... [panicking]",
            $">... This is {midRound}!\r\n>... {verificationRounds.ToTitleCase()} total!\r\n>... <size=200%><color=red>Halfway!</color></size>",
            $">... {verificationRounds.ToTitleCase()} verifications!\r\n>... <size=200%><color=red>Too many rounds!</color></size>\r\n>... [exhausted]",
            $">... We're on round {midRound}!\r\n>... Out of {verificationRounds}!\r\n>... <size=200%><color=red>Almost there!</color></size>",
            $">... {verificationRounds} verification stages!\r\n>... <size=200%><color=red>I can't keep this up!</color></size>\r\n>... [typing]",
            $">... Round {midRound} of {verificationRounds}!\r\n>... <size=200%><color=red>Still more!</color></size>\r\n>... [alarm blaring]",
            $">... {verificationRounds.ToTitleCase()} rounds to complete!\r\n>... <size=200%><color=red>This is brutal!</color></size>\r\n>... [breathing heavily]",
            $">... This is {midRound}!\r\n>... {verificationRounds} total!\r\n>... <size=200%><color=red>Keep verifying!</color></size>",
            $">... {verificationRounds} verifications needed!\r\n>... <size=200%><color=red>How is this possible?!</color></size>\r\n>... [gunfire]",

            // Multiple Terminal Uplinks (45 messages with terminalCount)
            $">... {terminalCount.ToTitleCase()} terminals to uplink?!\r\n>... <size=200%><color=red>That's insane!</color></size>\r\n>... [exhausted]",
            $">... We need to uplink {terminalCount} terminals!\r\n>... <size=200%><color=red>How?!</color></size>\r\n>... [panicking]",
            $">... This is terminal one!\r\n>... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>More to go!</color></size>",
            $">... {terminalCount} terminal uplinks!\r\n>... <size=200%><color=red>We'll never finish!</color></size>\r\n>... [breathing heavily]",
            $">... First terminal done!\r\n>... <size=200%><color=red>Still {terminalCount} total!</color></size>\r\n>... [gunfire]",
            $">... {terminalCount.ToTitleCase()} uplinks required!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [alarm blaring]",
            $">... We're on terminal two!\r\n>... Out of {terminalCount}!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {terminalCount} terminals?!\r\n>... <size=200%><color=red>This can't be right!</color></size>\r\n>... [typing]",
            $">... How many terminals?\r\n>... {terminalCount.ToTitleCase()}!\r\n>... <size=200%><color=red>Too many!</color></size>",
            $">... Terminal one complete!\r\n>... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>More uplinks!</color></size>",
            $">... {terminalCount.ToTitleCase()} terminal uplinks!\r\n>... <size=200%><color=red>I'm exhausted!</color></size>\r\n>... [panting]",
            $">... We're on the second terminal!\r\n>... Out of {terminalCount}!\r\n>... <size=200%><color=red>Almost there!</color></size>",
            $">... {terminalCount} uplinks needed!\r\n>... <size=200%><color=red>This is brutal!</color></size>\r\n>... [gunfire]",
            $">... First uplink done!\r\n>... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {terminalCount.ToTitleCase()} terminals to connect!\r\n>... <size=200%><color=red>How many left?!</color></size>\r\n>... [typing]",
            $">... This is terminal two!\r\n>... {terminalCount} total!\r\n>... <size=200%><color=red>More to go!</color></size>",
            $">... {terminalCount} terminal uplinks?!\r\n>... <size=200%><color=red>We're doomed!</color></size>\r\n>... [alarm blaring]",
            $">... We need {terminalCount} uplinks!\r\n>... <size=200%><color=red>Too many terminals!</color></size>\r\n>... [panicking]",
            $">... Terminal one complete!\r\n>... Out of {terminalCount}!\r\n>... <size=200%><color=red>Still more!</color></size>",
            $">... {terminalCount.ToTitleCase()} uplinks required!\r\n>... <size=200%><color=red>This is insane!</color></size>\r\n>... [breathing heavily]",
            $">... We're on the second one!\r\n>... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {terminalCount} terminals to uplink!\r\n>... <size=200%><color=red>I can't!</color></size>\r\n>... [gunfire]",
            $">... First terminal done!\r\n>... {terminalCount} total!\r\n>... <size=200%><color=red>More uplinks!</color></size>",
            $">... {terminalCount.ToTitleCase()} terminal uplinks!\r\n>... <size=200%><color=red>How is this possible?!</color></size>\r\n>... [exhausted]",
            $">... This is terminal two!\r\n>... Out of {terminalCount}!\r\n>... <size=200%><color=red>Almost there!</color></size>",
            $">... {terminalCount} uplinks needed!\r\n>... <size=200%><color=red>We'll never make it!</color></size>\r\n>... [typing]",
            $">... Terminal one complete!\r\n>... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {terminalCount.ToTitleCase()} terminals?!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [alarm blaring]",
            $">... We're on the second terminal!\r\n>... {terminalCount} total!\r\n>... <size=200%><color=red>More to go!</color></size>",
            $">... {terminalCount} terminal uplinks!\r\n>... <size=200%><color=red>This is impossible!</color></size>\r\n>... [panicking]",
            $">... First uplink done!\r\n>... Out of {terminalCount}!\r\n>... <size=200%><color=red>Still more!</color></size>",
            $">... {terminalCount.ToTitleCase()} uplinks required!\r\n>... <size=200%><color=red>I'm exhausted!</color></size>\r\n>... [breathing heavily]",
            $">... This is terminal two!\r\n>... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {terminalCount} terminals to connect!\r\n>... <size=200%><color=red>We're not gonna make it!</color></size>\r\n>... [gunfire]",
            $">... Terminal one complete!\r\n>... {terminalCount} total!\r\n>... <size=200%><color=red>More uplinks!</color></size>",
            $">... {terminalCount.ToTitleCase()} terminal uplinks!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... [exhausted]",
            $">... We're on the second one!\r\n>... Out of {terminalCount}!\r\n>... <size=200%><color=red>Almost there!</color></size>",
            $">... {terminalCount} uplinks needed!\r\n>... <size=200%><color=red>This is brutal!</color></size>\r\n>... [typing]",
            $">... First terminal done!\r\n>... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>Keep going!</color></size>",
            $">... {terminalCount.ToTitleCase()} terminals?!\r\n>... <size=200%><color=red>How many uplinks?!</color></size>\r\n>... [alarm blaring]",
            $">... This is terminal two!\r\n>... {terminalCount} total!\r\n>... <size=200%><color=red>More to go!</color></size>",
            $">... {terminalCount} terminal uplinks!\r\n>... <size=200%><color=red>I can't do this!</color></size>\r\n>... [panicking]",
            $">... Terminal one complete!\r\n>... Out of {terminalCount}!\r\n>... <size=200%><color=red>Still more!</color></size>",
            $">... {terminalCount.ToTitleCase()} uplinks required!\r\n>... <size=200%><color=red>This is insane!</color></size>\r\n>... [breathing heavily]",
            $">... We're on the second terminal!\r\n>... {terminalCount.ToTitleCase()} total!\r\n>... <size=200%><color=red>Keep going!</color></size>",

            // Team Splitting & Code Runner (25 messages, no variables)
            ">... Runner, get to the code terminal!\r\n>... <size=200%><color=red>We'll hold here!</color></size>\r\n>... [gunfire]",
            ">... I'm going for the log!\r\n>... <size=200%><color=red>Cover me!</color></size>\r\n>... [running]",
            ">... Runner's separated!\r\n>... <size=200%><color=red>They're under attack!</color></size>\r\n>... [screaming]",
            ">... I'm at the code terminal!\r\n>... <size=200%><color=red>Reading the log!</color></size>\r\n>... [typing frantically]",
            ">... Runner's down!\r\n>... <size=200%><color=red>Who's getting the codes?!</color></size>\r\n>... [panicking]",
            ">... Team, I'm making the run!\r\n>... <size=200%><color=red>Hold the uplink terminal!</color></size>\r\n>... [running]",
            ">... I can't reach the log terminal!\r\n>... <size=200%><color=red>Too many enemies!</color></size>\r\n>... [gunfire]",
            ">... Runner's lost!\r\n>... <size=200%><color=red>They're not responding!</color></size>\r\n>... [static]",
            ">... I'm reading the codes!\r\n>... <size=200%><color=red>Relaying them now!</color></size>\r\n>... [typing]",
            ">... Runner, status?!\r\n>... <size=200%><color=red>Runner?!</color></size>\r\n>... [silence]",
            ">... I'm heading to the log terminal!\r\n>... <size=200%><color=red>Defend the uplink!</color></size>\r\n>... [running]",
            ">... Runner's pinned down!\r\n>... <size=200%><color=red>Can't get to the codes!</color></size>\r\n>... [gunfire]",
            ">... I found the code terminal!\r\n>... <size=200%><color=red>Reading the log!</color></size>\r\n>... [typing frantically]",
            ">... We're split up!\r\n>... <size=200%><color=red>Runner, get back here!</color></size>\r\n>... [alarm blaring]",
            ">... I'm at the log!\r\n>... <size=200%><color=red>They're everywhere!</color></size>\r\n>... [gunfire]",
            ">... Runner's not coming back!\r\n>... <size=200%><color=red>Someone else go!</color></size>\r\n>... [panicking]",
            ">... I'm making the code run!\r\n>... <size=200%><color=red>Hold position!</color></size>\r\n>... [running]",
            ">... Runner, do you have the codes?!\r\n>... <size=200%><color=red>We need them now!</color></size>\r\n>... [screaming]",
            ">... I can see the code terminal!\r\n>... <size=200%><color=red>Almost there!</color></size>\r\n>... [running]",
            ">... Runner's been taken!\r\n>... <size=200%><color=red>No codes!</color></size>\r\n>... [gunfire]",
            ">... I'm reading the log file!\r\n>... <size=200%><color=red>Got the codes!</color></size>\r\n>... [typing]",
            ">... Team's defending alone!\r\n>... <size=200%><color=red>Runner, hurry!</color></size>\r\n>... [alarm blaring]",
            ">... I'm at the terminal!\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... [gunfire]",
            ">... Runner's under attack!\r\n>... <size=200%><color=red>Can't help them!</color></size>\r\n>... [screaming]",
            ">... I'm heading back with the codes!\r\n>... <size=200%><color=red>Cover me!</color></size>\r\n>... [running]",

            // Combat During Uplink (20 messages, no variables)
            ">... UPLINK_CONNECT entered!\r\n>... <size=200%><color=red>Alarm triggered!</color></size>\r\n>... [alarm blaring]",
            ">... They're coming from everywhere!\r\n>... <size=200%><color=red>Defend the terminal!</color></size>\r\n>... [gunfire]",
            ">... Giants incoming!\r\n>... <size=200%><color=red>Focus fire!</color></size>\r\n>... [shooting]",
            ">... I'm out of ammo!\r\n>... <size=200%><color=red>Need more!</color></size>\r\n>... [reloading]",
            ">... Chargers!\r\n>... <size=200%><color=red>They're rushing!</color></size>\r\n>... [gunfire]",
            ">... Sentries are down!\r\n>... <size=200%><color=red>We're exposed!</color></size>\r\n>... [alarm blaring]",
            ">... Shooters in the back!\r\n>... <size=200%><color=red>Turn around!</color></size>\r\n>... [gunfire]",
            ">... The alarm won't stop!\r\n>... <size=200%><color=red>Keep fighting!</color></size>\r\n>... [shooting]",
            ">... Too many of them!\r\n>... <size=200%><color=red>Fall back to terminal!</color></size>\r\n>... [running]",
            ">... I can't hold them!\r\n>... <size=200%><color=red>They're breaking through!</color></size>\r\n>... [gunfire]",
            ">... Giant's coming!\r\n>... <size=200%><color=red>Take it down!</color></size>\r\n>... [shooting]",
            ">... Running low on ammo!\r\n>... <size=200%><color=red>Check the boxes!</color></size>\r\n>... [reloading]",
            ">... Charger squad!\r\n>... <size=200%><color=red>Incoming!</color></size>\r\n>... [gunfire]",
            ">... The sentries are overwhelmed!\r\n>... <size=200%><color=red>Help them!</color></size>\r\n>... [alarm blaring]",
            ">... Shooters everywhere!\r\n>... <size=200%><color=red>Take cover!</color></size>\r\n>... [gunfire]",
            ">... Wave after wave!\r\n>... <size=200%><color=red>When will it stop?!</color></size>\r\n>... [shooting]",
            ">... I'm down to my sidearm!\r\n>... <size=200%><color=red>Out of ammo!</color></size>\r\n>... [gunfire]",
            ">... Giant's charging!\r\n>... <size=200%><color=red>Everyone shoot!</color></size>\r\n>... [shooting]",
            ">... They won't stop coming!\r\n>... <size=200%><color=red>Hold the line!</color></size>\r\n>... [alarm blaring]",
            ">... More Chargers!\r\n>... <size=200%><color=red>Defend the terminal!</color></size>\r\n>... [gunfire]",

            // General Panic & Failure (20 messages, no variables)
            ">... Wrong code!\r\n>... <size=200%><color=red>Stage reset!</color></size>\r\n>... [typing frantically]",
            ">... I can't find the log terminal!\r\n>... <size=200%><color=red>Where is it?!</color></size>\r\n>... [panicking]",
            ">... Which code was it?!\r\n>... <size=200%><color=red>I forgot!</color></size>\r\n>... [screaming]",
            ">... The log file's not here!\r\n>... <size=200%><color=red>Wrong terminal!</color></size>\r\n>... [typing]",
            ">... I entered the wrong code!\r\n>... <size=200%><color=red>We have to start over!</color></size>\r\n>... [alarm blaring]",
            ">... Where's the verification terminal?!\r\n>... <size=200%><color=red>I'm lost!</color></size>\r\n>... [running]",
            ">... I can't remember the code!\r\n>... <size=200%><color=red>What was it?!</color></size>\r\n>... [panicking]",
            ">... Wrong terminal!\r\n>... <size=200%><color=red>This isn't it!</color></size>\r\n>... [typing frantically]",
            ">... Stage reset again!\r\n>... <size=200%><color=red>I keep messing up!</color></size>\r\n>... [screaming]",
            ">... The codes are confusing!\r\n>... <size=200%><color=red>Which one?!</color></size>\r\n>... [typing]",
            ">... I can't find the log!\r\n>... <size=200%><color=red>It's not here!</color></size>\r\n>... [alarm blaring]",
            ">... Wrong verification!\r\n>... <size=200%><color=red>Reset!</color></size>\r\n>... [gunfire]",
            ">... I'm at the wrong terminal!\r\n>... <size=200%><color=red>Where's the right one?!</color></size>\r\n>... [running]",
            ">... The code's not matching!\r\n>... <size=200%><color=red>Try again!</color></size>\r\n>... [typing frantically]",
            ">... I lost the code!\r\n>... <size=200%><color=red>Runner, relay it again!</color></size>\r\n>... [panicking]",
            ">... This terminal doesn't have the log!\r\n>... <size=200%><color=red>Find another one!</color></size>\r\n>... [typing]",
            ">... Verification failed!\r\n>... <size=200%><color=red>Stage reset!</color></size>\r\n>... [alarm blaring]",
            ">... I can't remember which code!\r\n>... <size=200%><color=red>Was it X or Y?!</color></size>\r\n>... [screaming]",
            ">... Wrong log file!\r\n>... <size=200%><color=red>This isn't the right one!</color></size>\r\n>... [typing]",
            ">... The codes keep resetting!\r\n>... <size=200%><color=red>We'll never finish!</color></size>\r\n>... [gunfire]",

            // Atmospheric & Sound Effects (10 messages, no variables)
            ">... [static]\r\n>... <size=200%><color=red>[alarm blaring]</color></size>\r\n>... [gunfire]",
            ">... [typing frantically]\r\n>... <size=200%><color=red>[screaming]</color></size>\r\n>... [silence]",
            ">... [running]\r\n>... [breathing heavily]\r\n>... <size=200%><color=red>[gunfire]</color></size>",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>[typing]</color></size>\r\n>... [static]",
            ">... [scrolling]\r\n>... <size=200%><color=red>[relaying]</color></size>\r\n>... [gunfire]",
            ">... [static]\r\n>... <size=200%><color=red>[connection lost]</color></size>\r\n>... [silence]",
            ">... [typing]\r\n>... <size=200%><color=red>[alarm blaring]</color></size>\r\n>... [screaming]",
            ">... [gunfire]\r\n>... [running]\r\n>... <size=200%><color=red>[breathing heavily]</color></size>",
            ">... [alarm blaring]\r\n>... <size=200%><color=red>[gunfire intensifies]</color></size>\r\n>... [static]",
            ">... [typing frantically]\r\n>... <size=200%><color=red>[static]</color></size>\r\n>... [silence]",
        }))!);
        #endregion
    }
}
