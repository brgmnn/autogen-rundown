using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using AutogenRundown.Patches.CustomTerminals;

namespace AutogenRundown.DataBlocks.Objectives;

/// <summary>
/// Objective: AlphaTerminalCommand
///
/// Players land with the Matter Wave Projector in their inventory, traverse
/// challenge zones to a portal, warp to a static alpha dimension (AlphaOne or
/// AlphaThree_Top), find a custom terminal placed on the dimension geomorph,
/// and execute a backdoor command. The command starts an error wave inside
/// the dimension, runs a "data transfer" period, then warps the team back to
/// Reality, clears the dimension, and force-completes the objective.
///
/// The objective Type is left as <see cref="WardenObjectiveType.Empty"/> --
/// none of the game's objective machinery drives the flow. Instead, the
/// backdoor command is attached directly to the spawned custom terminal as a
/// <see cref="Terminals.CustomTerminalCommand"/>, with all gameplay events
/// hung off its <see cref="Terminals.CustomTerminalCommand.CommandEvents"/>
/// list. Patch_SpawnCustomTerminals.BuildPlacementData converts those into
/// the game-side WardenObjectiveEventData entries that LG_TerminalUniqueCommandsSetupJob
/// wires into the terminal at level build.
/// </summary>
public partial record WardenObjective
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void PreBuild_AlphaTerminalCommand(BuildDirector director, Level level)
    {
        if (director.Bulkhead != Bulkhead.Main)
            return;

        // Portal geomorphs only exist for Mining and Tech complexes. If the
        // level rolled Service, swap it out before any layout builds run.
        if (level.Complex == Complex.Service)
            level.Complex = Generator.Flip() ? Complex.Mining : Complex.Tech;

        // Choose the static alpha dimension.
        // TODO: add AlphaTwo (supports fliers)
        AlphaTerminal_Dimension = Generator.Pick(new List<Dimensions.DimensionData>
        {
            Dimensions.DimensionData.AlphaOne,
            Dimensions.DimensionData.AlphaThree_Top,
        })!;

        // Pick the backdoor command name + description.
        (AlphaTerminalCommand, AlphaTerminalCommandDesc) = Generator.Pick(new List<(string, Text)>
        {
            ("OVERRIDE_INITIALIZATION",    new Text("Initialize alpha-dimension data transfer override.")),
            ("EXEC_OVERRIDE",              new Text("Execute Warden override and begin transfer protocol.")),
            ("INTERPRET_NAV_DATA",         new Text("Interpret recovered navigational data block.")),
            ("COPY_GEN_INDEX_COMPLETE_DB", new Text("Copy complete genome-index database to host.")),
        })!;

        AlphaTerminal_DimensionName = AlphaTerminal_Dimension.DimensionGeomorph switch
        {
            "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Boss_Arena.prefab" => "Alpha One",
            "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_R6A2.prefab" => "Alpha Three",
            _ => ""
        };
        AlphaTerminal_DimensionName = $"<color=orange>{AlphaTerminal_DimensionName}</color>";
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void Build_AlphaTerminalCommand(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text($"Locate terminal in {AlphaTerminal_DimensionName} and begin transfer " +
                                 $"of data cube coordinates");

        FindLocationInfo = new Text(() => $"Transport {Intel.ObjectiveItem("Matter Wave Projector")} to " +
                                          $"Jump Gate in {Intel.Zone(layout.ZoneAliasStart + PlacementNodes.First().ZoneNumber)}");
        FindLocationInfoHelp = new Text("Access more data in the terminal maintenance system");

        // MainObjective = new Text(() => $"Bring the {Intel.ObjectiveItem("Matter Wave Projector")} " +
        //                                $"to the portal, traverse to the alpha dimension, and execute " +
        //                                $"the override command on [TERMINAL_1_0_0_0]");

        StartPuzzle = ChainedPuzzle.TeamScan;

        var wavePopulation = AlphaTerminal_DimensionName switch
        {
            "Alpha One" => WavePopulation.AlphaSwarm,

            _ => WavePopulation.Baseline
        };
        var (waveSettings, transferDuration) = level.Tier switch
        {
            "A" => (WaveSettings.Baseline_Easy,     90.0),
            "B" => (WaveSettings.Baseline_Normal,   120.0),
            "C" => (WaveSettings.Baseline_Hard,     150.0),
            "D" => (WaveSettings.Baseline_Hard,     180.0),
            "E" => (WaveSettings.Baseline_VeryHard, 210.0),

            _   => (WaveSettings.Baseline_Normal,   120.0)
        };

        // Build the event chain that fires when the player runs the command.
        // No Countdown UI -- AWO's Countdown event has no game-side equivalent
        // on WardenObjectiveEventData and the patch's converter is vanilla-only.
        // Progress is communicated via warden-intel pings at 25/50/75% instead.
        var commandEvents = new List<WardenObjectiveEvent>();

        commandEvents.AddMessage(":://DATA TRANSFER INITIATED", delay: 0.5);

        // The error wave needs to spawn IN the alpha dimension. The
        // AddSpawnWave helper doesn't take a Dimension parameter, so build
        // the event explicitly.
        commandEvents.Add(new WardenObjectiveEvent
        {
            Type = WardenObjectiveEventType.SpawnEnemyWave,
            Delay = 3.0,
            Dimension = DimensionIndex.Dimension1,
            SoundId = Sound.Enemies_DistantLowRoar,
            EnemyWaveData = new GenericWave
            {
                Population = wavePopulation,
                Settings = waveSettings,
                TriggerAlarm = true,
            },
        });

        commandEvents.AddCountup(100, new WardenObjectiveEventCountup
        {
            StartValue = 0.0,
            Speed = 10.0,
            Title = new Text("Data transfer progress"),
            BodyText = "[COUNTUP]%",
            TimerColor = "orange",
            DecimalPoints = 1,
        }, delay: 0.5);

        // Transfer complete: stop alarms, clear Dim1, warp the team back to
        // Reality, message, then force-complete the objective.
        commandEvents.AddTurnOffAlarms(delay: transferDuration);
        commandEvents.AddClearDimension(DimensionIndex.Dimension1, delay: transferDuration + 0.5);
        commandEvents.AddDimensionWarp(DimensionIndex.Reality, delay: transferDuration + 1.5);
        commandEvents.AddMessage(":://TRANSFER COMPLETE — RETURN TO EXTRACTION", delay: transferDuration + 2.5);

        // ForceCompleteObjective is what tells the game the objective is solved
        // and the team should head to extract. With Type=Empty there's no
        // OnActivateOnSolveItem flow to flip the win condition automatically.
        commandEvents.Add(new WardenObjectiveEvent
        {
            Type = WardenObjectiveEventType.ForceCompleteObjective,
            Delay = transferDuration + 3.0,
        });

        var candidates = LevelCustomTerminals.GetCandidates(AlphaTerminal_Dimension.DimensionGeomorph);
        var (terminalPos, terminalRot) = Generator.Pick(candidates);

        // Spawn the alpha terminal in Dim1 with the backdoor command attached.
        // CommandEvents above is what fires when the player runs the command.
        // No IsWardenObjective flag -- we are NOT routing through the
        // SpecialTerminalCommand objective machinery.
        CustomTerminalSpawnManager.AddSpawnRequest(
            level.LevelLayoutData,
            new CustomTerminalSpawnRequest
            {
                Bulkhead = director.Bulkhead,
                DimensionIndex = DimensionIndex.Dimension1,
                LocalIndex = 0,
                GeomorphName = AlphaTerminal_Dimension.DimensionGeomorph,
                LocalPosition = terminalPos,
                LocalRotation = terminalRot,
                UniqueCommands = new List<CustomTerminalCommand>
                {
                    new()
                    {
                        Command = AlphaTerminalCommand,
                        CommandDesc = AlphaTerminalCommandDesc,
                        SpecialCommandRule = CommandRule.Normal,
                        CommandEvents = commandEvents,
                    },
                },
            });

        // Team drops in already carrying the MWP -- no in-level pickup needed.
        GenericItemFromStart = Items.Item.MatterWaveProjector;

        // Extract scan at the elevator/forward-extract point. The default
        // win condition (GoToElevator) takes the team back to extract once
        // ForceCompleteObjective fires from the command chain.
        ChainedPuzzleAtExit = ChainedPuzzle.ExitAlarm.PersistentId;

        // Type stays Empty -- the entire flow is driven by the CommandEvents
        // above, not by any objective-type machinery.
        Type = WardenObjectiveType.Empty;
    }
}
