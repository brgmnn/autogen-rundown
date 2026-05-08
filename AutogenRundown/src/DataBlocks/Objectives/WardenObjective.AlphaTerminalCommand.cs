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

        // Team drops in already carrying the MWP
        GenericItemFromStart = Items.Item.MatterWaveProjector;

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

        level.Name = AlphaTerminal_DimensionName;
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

        var wavePopulation = AlphaTerminal_DimensionName switch
        {
            // "Alpha One" => WavePopulation.AlphaSwarm,

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

        var commandEvents = new List<WardenObjectiveEvent>();
        var eventsOnDone = new List<WardenObjectiveEvent>();
        var eventsOnProgress = new List<ProgressEvent>();

        #region Command Events

        commandEvents
            .AddScan(ChainedPuzzle.TeamScan)
            .AddSpecialHudTimer(transferDuration, new WardenObjectiveEventSpecialHudTimer
                {
                    Type = SpecialHudTimerType.StartTimer,
                    Message = "Data transfer progress - <color=white>[PERCENT]</color>",
                    Style = PUIMessageStyle.Message,
                    ShowTimeInProgressBar = true,
                    EventsOnProgress = eventsOnProgress,
                    EventsOnDone = eventsOnDone,
                },
                delay: 1.5)
            .AddUpdateSubObjective(
                header: new Text("Translocation tether resolving"),
                description: new Text("Wait for translocation to complete"),
                intel: "Translocation tether resolving",
                delay: 3.0);

        // The error wave needs to spawn IN the alpha dimension. The
        // AddSpawnWave helper doesn't take a Dimension parameter, so build
        // the event explicitly.
        commandEvents.Add(new WardenObjectiveEvent
        {
            Type = WardenObjectiveEventType.SpawnEnemyWave,
            Delay = 2.0,
            Dimension = DimensionIndex.Dimension1,
            SoundId = Sound.Enemies_DistantLowRoar,
            EnemyWaveData = new GenericWave
            {
                Population = wavePopulation,
                Settings = waveSettings,
                TriggerAlarm = true,
            },
        });

        #endregion

        #region Progress events

        // Scary one-shot waves are safe here: the dimension is cleared on teleport,
        // so anything spawned in the alpha dimension is removed when the team warps out.

        void SpawnVariant(WavePopulation population, WaveSettings settings)
        {
            eventsOnProgress.Add(new ProgressEvent
            {
                Progress = (transferDuration - 50.0) / transferDuration,
                Events = new List<WardenObjectiveEvent>()
                    .AddGenericWave(
                        new GenericWave
                        {
                            Population = population,
                            Settings = settings
                        },
                        delay: 0).ToList()
            });
        }

        var spawnVariants = level.Tier switch
        {
            "D" or "E" => new List<(double, Action)>
            {
                // Pablo (immortal Tank Boss)
                (0.25, () => SpawnVariant(WavePopulation.SingleEnemy_Immortal,    WaveSettings.SingleWave_MiniBoss_4pts)),
                // Hybrid Volley — ranged projectile pressure forces cover
                (0.15, () => SpawnVariant(WavePopulation.OnlyHybrids,             WaveSettings.SingleWave_20pts)),
                // Infested Bloom — killing them spawns babies + persistent fog
                (0.20, () => SpawnVariant(WavePopulation.OnlyInfestedStrikers,    WaveSettings.SingleWave_20pts)),
                // Nightmare Tanks — 2x Potato Tanks
                (0.20, () => SpawnVariant(WavePopulation.SingleEnemy_TankPotato,  WaveSettings.SingleWave_MiniBoss_4pts)),
                // Mother's Brood — stationary spawners with chasing children
                (0.20, () => SpawnVariant(WavePopulation.SingleEnemy_Mother,      WaveSettings.SingleWave_MiniBoss_4pts)),
            },
            _ => new List<(double, Action)>
            {
                (0.35, () => SpawnVariant(WavePopulation.SingleEnemy_Immortal,    WaveSettings.SingleWave_MiniBoss_4pts)),
                (0.20, () => SpawnVariant(WavePopulation.OnlyHybrids,             WaveSettings.SingleWave_20pts)),
                (0.15, () => SpawnVariant(WavePopulation.OnlyInfestedStrikers,    WaveSettings.SingleWave_20pts)),
                (0.15, () => SpawnVariant(WavePopulation.SingleEnemy_TankPotato,  WaveSettings.SingleWave_MiniBoss_4pts)),
                (0.15, () => SpawnVariant(WavePopulation.SingleEnemy_Mother,      WaveSettings.SingleWave_MiniBoss_4pts)),
            }
        };

        Generator.SelectRun(spawnVariants);

        #endregion

        #region Done events

        eventsOnDone
            .AddUpdateSubObjective(
                header: new Text("Transfer complete"),
                delay: 0.7)
            .AddDimensionWarp(DimensionIndex.Reality, delay: 1.5)
            .AddTurnOffAlarms(2.0)
            .AddClearDimension(DimensionIndex.Dimension1, delay: 5.0);

        eventsOnDone.Add(new WardenObjectiveEvent
        {
            Type = WardenObjectiveEventType.ForceCompleteObjective,
            Delay = 3.0,
        });

        #endregion


        var candidates = LevelCustomTerminals.GetCandidates(AlphaTerminal_Dimension.DimensionGeomorph);
        var (terminalPos, terminalRot) = Generator.Pick(candidates);

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

        AddCompletedObjectiveChallenge(level, director);

        // Type stays Empty - the entire flow is driven by the CommandEvents
        Type = WardenObjectiveType.Empty;
    }
}
