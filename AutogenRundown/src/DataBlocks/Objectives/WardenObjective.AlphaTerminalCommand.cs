using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks.Objectives;

/// <summary>
/// Objective: AlphaTerminalCommand
///
/// Players land with the Matter Wave Projector in the elevator drop, carry it
/// through challenge zones to a portal, warp to a static alpha dimension
/// (AlphaOne or AlphaThree_Top), find a custom terminal placed somewhere on the
/// dimension geomorph, and execute a backdoor command. The command starts an
/// error wave in the dimension and a "data transfer" countdown timer; on
/// completion the team is warped back to Reality, dimension enemies are cleared,
/// and an extract alarm pushes them to the exit.
///
/// Internally serialized as <see cref="WardenObjectiveType.SpecialTerminalCommand"/>
/// so the game's native terminal-command machinery handles the input + activate
/// hook. The Type field is rewritten at the end of <see cref="Build_AlphaTerminalCommand"/>.
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

        // Display strings — leveraging the SpecialTerminalCommand placeholders
        // ([SPECIAL_COMMAND], [ITEM_SERIAL], [ITEM_ZONE]) which the game resolves
        // for the warden objective terminal.
        MainObjective = new Text("Carry the Matter Wave Projector to the portal, traverse to the alpha dimension, and execute the override command [SPECIAL_COMMAND] on [ITEM_SERIAL]");
        FindLocationInfo = new Text("Reach the portal and warp to the alpha dimension");
        FindLocationInfoHelp = new Text("Locate the alpha terminal [ITEM_SERIAL] in the alpha dimension");
        GoToZone = new Text("Insert the Matter Wave Projector into the portal");
        InZoneFindItem = new Text("Locate alpha terminal [ITEM_SERIAL] in the alpha dimension");
        SolveItem = new Text("Execute backdoor command [SPECIAL_COMMAND] on [ITEM_SERIAL] to start the data transfer");

        // Pick the backdoor command from the user-supplied set.
        (SpecialTerminalCommand, SpecialTerminalCommandDesc) = Generator.Pick(new List<(string, string)>
        {
            ("OVERRIDE_INITIALIZATION",   "Initialize alpha-dimension data transfer override."),
            ("EXEC_OVERRIDE",             "Execute Warden override and begin transfer protocol."),
            ("INTERPRET_NAV_DATA",        "Interpret recovered navigational data block."),
            ("COPY_GEN_INDEX_COMPLETE_DB", "Copy complete genome-index database to host."),
        })!;

        SpecialTerminalCommand_Type = SpecialCommand.ErrorAlarm;

        StartPuzzle = ChainedPuzzle.TeamScan;

        // Tier-scaled wave + transfer timer.
        var (waveSettings, transferDuration) = level.Tier switch
        {
            "A" => (WaveSettings.Error_Easy,     90.0),
            "B" => (WaveSettings.Error_Easy,     120.0),
            "C" => (WaveSettings.Error_Normal,   150.0),
            "D" => (WaveSettings.Error_Hard,     180.0),
            "E" => (WaveSettings.Error_VeryHard, 210.0),
            _   => (WaveSettings.Error_Normal,   120.0),
        };

        // EventsOnActivate runs the moment the player executes the command on
        // the alpha terminal in Dimension1. Spawn the error wave inside the
        // dimension and start the data-transfer countdown.
        EventsOnActivate.Add(new WardenObjectiveEvent
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

        // Countdown shows on the players' HUD as a timer. When it expires the
        // EventsOnDone fire: stop waves, clear the dimension, warp back to
        // Reality, and kick off the extract push.
        EventsOnActivate.AddCountdown(
            duration: transferDuration,
            countdown: new WardenObjectiveEventCountdown
            {
                TitleText = "DATA TRANSFER",
                TimerColor = "orange",
                EventsOnDone = new List<WardenObjectiveEvent>()
                    .AddTurnOffAlarms()
                    .AddClearDimension(DimensionIndex.Dimension1, delay: 0.5)
                    .AddDimensionWarp(DimensionIndex.Reality, delay: 1.5)
                    .AddMessage(":://TRANSFER COMPLETE — RETURN TO EXTRACTION", delay: 2.5)
                    .ToList(),
            },
            delay: 4.0);

        // Treat command-entry as objective activation so the game flips to the
        // win condition (extract) once EventsOnActivate has fired.
        OnActivateOnSolveItem = true;

        GenericItemFromStart = Items.Item.MatterWaveProjector;

        // Extract alarm at the elevator/forward-extract point.
        ChainedPuzzleAtExit = ChainedPuzzle.ExitAlarm.PersistentId;

        // Tier-scaled exit waves + scan time bump.
        AddCompletedObjectiveChallenge(level, director);

        // Game-side serialization: this is a special terminal command objective.
        Type = WardenObjectiveType.Empty;
    }
}
