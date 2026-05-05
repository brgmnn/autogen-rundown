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
    public void PreBuild_AlphaTerminalCommand(BuildDirector director, Level level)
    {
        if (director.Bulkhead != Bulkhead.Main)
            return;

        // Portal geomorphs only exist for Mining and Tech complexes. If the
        // level rolled Service, swap it out before any layout builds run.
        if (level.Complex == Complex.Service)
            level.Complex = Generator.Flip() ? Complex.Mining : Complex.Tech;
    }

    public void Build_AlphaTerminalCommand(BuildDirector director, Level level)
    {
        var (dataLayer, _) = GetObjectiveLayerAndLayout(director, level);

        // Display strings -- with Type=Empty the [SPECIAL_COMMAND] / [ITEM_SERIAL]
        // placeholders aren't resolved by the SpecialTerminalCommand machinery,
        // so the command name is interpolated into the strings directly.
        MainObjective = new Text("Carry the Matter Wave Projector to the portal, traverse to the alpha dimension, and execute the override command on the alpha terminal");
        FindLocationInfo = new Text("Reach the portal and warp to the alpha dimension");
        FindLocationInfoHelp = new Text("Locate the alpha terminal in the alpha dimension");
        GoToZone = new Text("Insert the Matter Wave Projector into the portal");
        InZoneFindItem = new Text("Locate the alpha terminal in the alpha dimension");
        SolveItem = new Text("Execute the backdoor command on the alpha terminal to start the data transfer");

        StartPuzzle = ChainedPuzzle.TeamScan;

        // Tier-scaled wave + data-transfer duration. The duration governs the
        // total length of the command's event chain (mid-progress pings,
        // alarm-off, dim-clear, warp-back, force-complete).
        var (waveSettings, transferDuration) = level.Tier switch
        {
            "A" => (WaveSettings.Error_Easy,     90.0),
            "B" => (WaveSettings.Error_Easy,     120.0),
            "C" => (WaveSettings.Error_Normal,   150.0),
            "D" => (WaveSettings.Error_Hard,     180.0),
            "E" => (WaveSettings.Error_VeryHard, 210.0),
            _   => (WaveSettings.Error_Normal,   120.0),
        };

        // Pick the backdoor command name + description.
        var (commandName, commandDescText) = Generator.Pick(new List<(string, string)>
        {
            ("OVERRIDE_INITIALIZATION",   "Initialize alpha-dimension data transfer override."),
            ("EXEC_OVERRIDE",             "Execute Warden override and begin transfer protocol."),
            ("INTERPRET_NAV_DATA",        "Interpret recovered navigational data block."),
            ("COPY_GEN_INDEX_COMPLETE_DB", "Copy complete genome-index database to host."),
        })!;

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
                Population = WavePopulation.AlphaSwarm,
                Settings = waveSettings,
                TriggerAlarm = true,
            },
        });

        // Mid-progress flavor pings. These are pure cosmetic feedback since
        // there's no HUD timer.
        commandEvents.AddMessage(":://TRANSFER 25%", delay: transferDuration * 0.25);
        commandEvents.AddMessage(":://TRANSFER 50%", delay: transferDuration * 0.50);
        commandEvents.AddMessage(":://TRANSFER 75%", delay: transferDuration * 0.75);

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

        // Resolve the dimension geomorph so we can pick a candidate terminal
        // position on it. The dimension itself is registered earlier (in
        // BuildLayout_AlphaTerminalCommand).
        var dim = level.DimensionDatas.Find(d => d.Dimension == DimensionIndex.Dimension1)!;
        var dimensionData = dim.Data.Data;

        var candidates = LevelCustomTerminals.GetCandidates(dimensionData.DimensionGeomorph);
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
                GeomorphName = dimensionData.DimensionGeomorph,
                LocalPosition = terminalPos,
                LocalRotation = terminalRot,
                UniqueCommands = new List<CustomTerminalCommand>
                {
                    new()
                    {
                        Command = commandName,
                        CommandDesc = new Text(commandDescText),
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
