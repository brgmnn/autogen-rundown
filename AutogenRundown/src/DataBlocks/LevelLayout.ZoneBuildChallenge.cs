using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Logs;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

/*
 * --- General layout building blocks ---
 */
public partial record LevelLayout
{
    #region Challenge pickers

    /// <summary>
    /// Target size is:
    ///
    ///     start -> mid -> end (ret)
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_Small(ZoneNode start)
    {
        var end = new ZoneNode();
        var endZone = new Zone(level, this);

        switch (level.Tier)
        {
            case "A":
            case "B":
            case "C":
            case "D":
            case "E":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // +1 zones, alarm door
                    (2.0, () => { (end, endZone) = AddZone(start); }),

                    // +1 zones, door guarded by keycard in start
                    (2.0, () => { (end, endZone) = BuildChallenge_KeycardInZone(start); }),

                    // +1 zones, door locked down in start
                    (2.0, () => { (end, endZone) = BuildChallenge_LockedTerminalDoor(start, sideZones: 0); }),

                    // +2 zones, alarm doors
                    (2.0, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var zone = planner.GetZone(nodes.Last())!;

                        (end, endZone) = (nodes.Last(), zone);
                    }),

                    // +2 zones, door guarded by keycard in mid
                    (2.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        midZone.Coverage = CoverageMinMax.Small_24;

                        (end, endZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // +2 zones, door guarded by terminal in mid
                    (2.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        midZone.Coverage = CoverageMinMax.Small_24;

                        (end, endZone) = BuildChallenge_LockedTerminalDoor(mid, sideZones: 0);
                    }),

                    // +2 zones, door requires power cell
                    (2.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        midZone.Coverage = CoverageMinMax.Small_24;

                        (end, endZone) = BuildChallenge_GeneratorCellInZone(mid);
                    }),
                });
                break;
            }
        }

        return (end, endZone);
    }

    #endregion

    /// <summary>
    /// Sets start to a hub zone with the next zone locked behind a powered down door that
    /// requires a generator to be powered. Cell is in a side zone from the hub. Layout:
    ///
    ///     start(hub) -> end
    ///                -> power_cell[0,...]
    ///
    /// Note that the start zone will be set to a hub wwith 4 connections
    /// </summary>
    /// <param name="start"></param>
    /// <param name="sideCellZones"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_GeneratorCellInSide(
        ZoneNode start,
        int sideCellZones = 1)
    {
        start = planner.UpdateNode(start with { MaxConnections = 3 });
        var startZone = planner.GetZone(start)!;
        start = level.GenHubGeomorph(start);

        var (end, endZone) = AddZone(start, new ZoneNode());
        var keycardNodes = AddBranch(start, sideCellZones, "power_cell");

        AddGeneratorPuzzle(end, keycardNodes.Last());

        return (end, endZone);
    }

    public (ZoneNode, Zone) BuildChallenge_GeneratorCellInZone(ZoneNode start)
    {
        var (end, endZone) = AddZone(start, new ZoneNode());

        AddGeneratorPuzzle(end, start);

        return (end, endZone);
    }

    /// <summary>
    /// Very simple puzzle. Creates:
    ///     start (keycard) -> end (locked by key)
    ///
    /// Simple wrapper call around AddKeycardPuzzle
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_KeycardInZone(ZoneNode start)
    {
        var (end, endZone) = AddZone(start, new ZoneNode());

        AddKeycardPuzzle(end, start);

        return (end, endZone);
    }

    /// <summary>
    /// Adds a hub zone with a locked keycard going to the next zone. Keycard is in a side zone(s)
    ///
    /// Layout:
    ///   start(hub) -> end
    ///              -> keycard[0,...]
    ///
    /// Note that the start zone will be set to a hub with 4 connections
    /// </summary>
    /// <param name="start"></param>
    /// <param name="sideKeycardZones"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_KeycardInSide(
        ZoneNode start,
        int sideKeycardZones = 1)
    {
        start = planner.UpdateNode(start with { MaxConnections = 3 });
        var startZone = planner.GetZone(start)!;
        start = level.GenHubGeomorph(start);

        var (end, endZone) = AddZone(start, new ZoneNode());
        var keycardNodes = AddBranch(start, sideKeycardZones, "keycard");

        AddKeycardPuzzle(end, keycardNodes.Last());

        return (end, endZone);
    }

    /// <summary>
    /// Adds a locked door that must be unlocked on a terminal. Optionally specify number of side
    /// zones to build out to place the terminal at the end of. If no side zones are specified the
    /// terminal is placed in the source zone.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="sideZones"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_LockedTerminalDoor(ZoneNode start, int sideZones = 0)
    {
        var (end, endZone) = AddZone(start, new ZoneNode());
        var terminal = start;

        if (sideZones > 0)
        {
            start = planner.UpdateNode(start with { MaxConnections = 3 });
            planner.GetZone(start)!.Coverage = CoverageMinMax.Small;

            terminal = AddBranch(start, sideZones, "terminal_door_unlock").Last();
        }

        AddTerminalUnlockPuzzle(end, terminal);

        return (end, endZone);
    }

    /// <summary>
    /// Adds a locked door that must be unlocked on a terminal. The terminal is in the start zone
    /// but is password locked. The password is fetched from another terminal in a side zone.
    /// At least 1 side zone must be specified.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="passwordBuilder"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_LockedTerminalPasswordInSide(
        ZoneNode start,
        Func<ZoneNode, (ZoneNode end, Zone endZone)>? passwordBuilder = null)
    {
        start = planner.UpdateNode(start with { MaxConnections = 3 });
        var startZone = planner.GetZone(start)!;
        start = level.GenHubGeomorph(start);

        var (end, endZone) = AddZone(start);

        // Build the side zone of passwords
        passwordBuilder ??= (node) => AddZone(node, new ZoneNode { Branch = "terminal_password" });
        var (password, _) = passwordBuilder(start);

        var terminalState = new TerminalStartingState
        {
            PasswordProtected = true,
            GeneratePassword = true,
            TerminalZoneSelectionDatas = new()
            {
                new()
                {
                    new ZoneSelectionData
                    {
                        ZoneNumber = password.ZoneNumber,
                        Bulkhead = password.Bulkhead,
                    }
                }
            }
        };

        AddTerminalUnlockPuzzle(end, start, terminalState);

        return (end, endZone);
    }

    /// <summary>
    /// Mostly just a wrapper around `AddApexAlarm()` but with the conveniences of returning the
    /// node/zone from the alarm
    /// </summary>
    /// <param name="start"></param>
    /// <param name="population"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_ApexAlarm(
        ZoneNode start,
        WavePopulation population,
        WaveSettings settings)
    {
        start = planner.UpdateNode(start with { MaxConnections = 3 });
        var startZone = planner.GetZone(start)!;
        start = level.GenHubGeomorph(start);

        var (end, endZone) = AddZone(start, new ZoneNode());

        startZone.AmmoPacks += 3.0;
        startZone.ToolPacks += 2.0;

        // TODO: consider adding a disinfect station somewhere for this
        if (startZone.InFog)
            startZone.ConsumableDistributionInZone = ConsumableDistribution.Alarms_FogRepellers.PersistentId;

        AddApexAlarm(end, population, settings);

        return (end, endZone);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_BossFight(ZoneNode start)
    {
        start = AddAlignedBoss_Hibernate(start);

        var (end, endZone) = AddZone(start);

        return (end, endZone);
    }

    /// <summary>
    /// Constructs a challenge layout that consists of:
    /// 1. Error alarm door
    ///     a. Any number of intermediate zones
    /// 2. Keycard puzzle
    ///     a. Any number of intermediate side zones
    ///
    /// Error alarm and keycard. Zone layout is as follows:
    ///   start -> firstErrorZone -> error zones [1,...] -> penultimate        -> end
    ///                                                      -> keycard[0,...]     -> error_off
    /// </summary>
    /// <param name="start"></param>
    /// <param name="errorZones">How many error zones to build. Must be at least 1</param>
    /// <param name="sideKeycardZones">
    ///     How many side keycard zones to build. If set to 0 the keycard will be placed in the locked keycard zone
    /// </param>
    /// <param name="terminalTurnoffZones">How many zones to get to the turn off terminal</param>
    /// <returns>The end ZoneNode of the challenge</returns>
    public (ZoneNode, Zone) BuildChallenge_ErrorWithOff_KeycardInSide(
        ZoneNode start,
        int errorZones,
        int sideKeycardZones,
        int terminalTurnoffZones)
    {
        // Enforce a minimum of 1 zone
        errorZones = Math.Max(1, errorZones);

        // first error alarm zone
        var (firstError, firstErrorZone) = AddZone(
            start,
            new ZoneNode { Branch = "primary", MaxConnections = 3 });
        firstErrorZone.Coverage = CoverageMinMax.Large;
        firstErrorZone.AmmoPacks += 5.0;
        firstErrorZone.ToolPacks += 3.0;
        firstErrorZone.HealthPacks += 4.0;

        // Add subsequent error alarm zones to go through
        var penultimate = AddBranch_Forward(
            firstError,
            errorZones - 1,
            "primary",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
            }).Last();
        planner.UpdateNode(penultimate with { MaxConnections = 2 });

        // Add the ending zone that will be returned
        var (end, endZone) = AddZone(penultimate, new ZoneNode { Branch = "primary" });

        // Build the side chain for the keycard if we have keycard zones
        // If this is zero it will just default to returning the penultimate zone
        var keycard = AddBranch(
            penultimate,
            sideKeycardZones,
            "keycard",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
            }).Last();

        // Lock the end zone
        AddKeycardPuzzle(end, keycard);

        // Build terminal zones
        // If `terminalTurnoffZones` is set to zero, then the turnoff terminal will be placed in the end zone.
        ZoneNode? terminal = AddBranch(
            end,
            terminalTurnoffZones,
            "error_turnoff",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
                zone.DisinfectPacks += 4.0;
            }).Last();

        var population = WavePopulation.Baseline;

        // First set shadows if we have them
        if (level.Settings.HasShadows())
            population = Generator.Flip(0.6) ? WavePopulation.OnlyShadows : WavePopulation
                .Baseline_Shadows;

        // Next check and set chargers first, then flyers
        if (level.Settings.HasChargers())
            population = WavePopulation.Baseline_Chargers;
        else if (level.Settings.HasFlyers())
            population = WavePopulation.Baseline_Flyers;

        // Error wave settings
        var settings = level.Tier switch
        {
            "B" => WaveSettings.Error_Easy,
            "C" => WaveSettings.Error_Normal,
            "D" => WaveSettings.Error_Hard,
            "E" => WaveSettings.Error_VeryHard,

            _ => WaveSettings.Error_Easy,
        };

        // Lock the first zone with the error alarm
        AddErrorAlarm(firstError, terminal, settings, population);

        return (end, endZone);
    }

    /// <summary>
    /// Generates a challenge zone build with:
    ///
    ///     start -> firstErrorZone (with cell) -> error zones [1,...] -> penultimate (with generator) -> end
    ///
    /// Note that this requires a team member carrying the cell through the error zones whilst
    /// the error alarm is ongoing which will be _hard_. Not recommended to use this challenge
    /// for anything above a C-tier.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="errorZones"></param>
    /// <param name="sideCellZones"></param>
    /// <param name="terminalTurnoffZones"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_ErrorWithOff_GeneratorCellCarry(
        ZoneNode start,
        int errorZones,
        int terminalTurnoffZones)
    {
        // Enforce a minimum of 1 zone
        errorZones = Math.Max(1, errorZones);

        // first error alarm zone
        var (firstError, firstErrorZone) = AddZone(
            start,
            new ZoneNode { Branch = "primary", MaxConnections = 3 });
        firstErrorZone.Coverage = CoverageMinMax.Large;
        firstErrorZone.AmmoPacks += 5.0;
        firstErrorZone.ToolPacks += 3.0;
        firstErrorZone.HealthPacks += 4.0;

        // Add subsequent error alarm zones to go through
        var penultimate = AddBranch_Forward(
            firstError,
            errorZones - 1,
            "primary",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
            }).Last();
        planner.UpdateNode(penultimate with { MaxConnections = 2 });

        // Add the ending zone that will be returned
        var (end, endZone) = AddZone(penultimate, new ZoneNode { Branch = "primary" });

        // Lock the end zone behind a generator puzzle
        // The cell has to be carried from the first error zone
        AddGeneratorPuzzle(end, firstError);

        // Build terminal zones
        // If `terminalTurnoffZones` is set to zero, then the turnoff terminal will be placed in the end zone.
        ZoneNode? terminal = AddBranch(
            end,
            terminalTurnoffZones,
            "error_turnoff",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
                zone.DisinfectPacks += 4.0;
            }).Last();

        var population = WavePopulation.Baseline;

        // First set shadows if we have them
        if (level.Settings.HasShadows())
            population = Generator.Flip(0.6) ? WavePopulation.OnlyShadows : WavePopulation
                .Baseline_Shadows;

        // Next check and set chargers first, then flyers
        if (level.Settings.HasChargers())
            population = WavePopulation.Baseline_Chargers;
        else if (level.Settings.HasFlyers())
            population = WavePopulation.Baseline_Flyers;

        // Error wave settings
        var settings = level.Tier switch
        {
            "B" => WaveSettings.Error_Easy,
            "C" => WaveSettings.Error_Normal,
            "D" => WaveSettings.Error_Hard,
            "E" => WaveSettings.Error_VeryHard,

            _ => WaveSettings.Error_Easy,
        };

        // Lock the first zone with the error alarm
        AddErrorAlarm(firstError, terminal, settings, population);

        return (end, endZone);
    }

    /// <summary>
    /// Cascading Lockdown Relay: 2-4 terminals in separate branches must be activated in
    /// sequence. Each relay unlocks the next terminal's zone but triggers environmental
    /// hazards (lights off, fog, enemy waves) in other branches. Players must station
    /// themselves at each terminal and coordinate.
    ///
    /// Layout:
    ///     start(hub) -> end (locked, unlocked by final relay)
    ///                -> relay_a(terminal) [open]
    ///                -> relay_b(terminal) [locked until A fires]
    ///                -> relay_c(terminal) [locked until B fires]
    ///
    /// The relay count scales by tier: A=2, B=2-3, C=3, D=3-4, E=4
    /// </summary>
    public (ZoneNode, Zone) BuildChallenge_CascadingRelay(ZoneNode start)
    {
        var relayCount = level.Tier switch
        {
            "A" => 2,
            "B" => Generator.Between(2, 3),
            "C" => 3,
            "D" => Generator.Between(3, 4),
            "E" => 4,
            _ => 2,
        };

        // Make start a hub with enough connections for all relay branches + forward
        start = planner.UpdateNode(start with { MaxConnections = relayCount + 1 });
        var startZone = planner.GetZone(start)!;
        start = level.GenHubGeomorph(start);

        // Create the locked forward door
        var (end, endZone) = AddZone(start, new ZoneNode());

        // Create relay branch zones
        var relayNodes = new List<ZoneNode>();
        var relayZones = new List<Zone>();

        for (var i = 0; i < relayCount; i++)
        {
            var (node, zone) = AddZone(start, new ZoneNode { Branch = $"relay_{i}" });
            zone.Coverage = CoverageMinMax.Small;
            relayNodes.Add(node);
            relayZones.Add(zone);
        }

        // Lock all relay zones except the first one
        for (var i = 1; i < relayCount; i++)
        {
            relayZones[i].ProgressionPuzzleToEnter = new ProgressionPuzzle
            {
                PuzzleType = ProgressionPuzzleType.Locked,
                CustomText = "<color=red>://RELAY NODE INACTIVE - Awaiting upstream handshake.</color>"
            };
        }

        // Lock the forward door
        endZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
        {
            PuzzleType = ProgressionPuzzleType.Locked,
            CustomText = "<color=red>://RELAY NETWORK INCOMPLETE - All nodes must handshake.</color>"
        };

        // Place a terminal in each relay zone with the relay command
        for (var i = 0; i < relayCount; i++)
        {
            var isLast = i == relayCount - 1;
            var commandEvents = new List<WardenObjectiveEvent>();

            if (isLast)
            {
                // Final relay unlocks the forward door
                commandEvents.AddUnlockDoor(
                    end.Bulkhead, end.ZoneNumber,
                    ":://RELAY NETWORK ESTABLISHED - Security door override authorized.",
                    delay: 8.0);
            }
            else
            {
                // Unlock next relay zone
                commandEvents.AddUnlockDoor(
                    relayNodes[i + 1].Bulkhead, relayNodes[i + 1].ZoneNumber,
                    $":://RELAY NODE {(char)('A' + i)} ONLINE - Handshake propagating...",
                    delay: 5.0);

                // Trigger hazards in the next relay zone based on tier
                AddRelayHazards(commandEvents, relayNodes[i + 1], i);
            }

            var terminalSerial = Lore.TerminalSerial(
                "Reality", relayNodes[i].Bulkhead, relayNodes[i].ZoneNumber, 0);

            // Log content describing the relay protocol
            var logContent = isLast
                ? $"RELAY PROTOCOL v2.4\n---\nNODE {(char)('A' + i)} - FINAL NODE\n\nExecute RELAY_HANDSHAKE to complete the relay network.\nAll upstream nodes must be active before this node can transmit.\n\nOnce complete, the controlled security door will be unlocked."
                : $"RELAY PROTOCOL v2.4\n---\nNODE {(char)('A' + i)} - INTERMEDIATE\n\nExecute RELAY_HANDSHAKE to propagate the signal downstream.\nThis will activate NODE {(char)('A' + i + 1)} for the next handshake.\n\nWARNING: Relay activation will trigger containment responses\nin downstream corridors. Advise team positioning before execution.";

            relayZones[i].TerminalPlacements.Add(new TerminalPlacement
            {
                UniqueCommands = new List<CustomTerminalCommand>
                {
                    new()
                    {
                        Command = "RELAY_HANDSHAKE",
                        CommandDesc = new Text($"Activates relay node {(char)('A' + i)} and propagates the handshake signal downstream."),
                        SpecialCommandRule = CommandRule.OnlyOnce,
                        CommandEvents = commandEvents,
                        PostCommandOutputs = new List<TerminalOutput>
                        {
                            new()
                            {
                                Output = $"Initializing relay node {(char)('A' + i)}...",
                                Type = LineType.SpinningWaitNoDone,
                                Time = 2.0
                            },
                            new()
                            {
                                Output = "Authenticating with relay network...",
                                Type = LineType.SpinningWaitDone,
                                Time = 3.0
                            },
                            new()
                            {
                                Output = isLast
                                    ? "Relay network fully established. Security override transmitted."
                                    : $"Handshake propagated to NODE {(char)('A' + i + 1)}. Signal confirmed.",
                                Type = LineType.Normal,
                                Time = 1.0
                            },
                        },
                    }
                },
                LogFiles = new List<LogFile>
                {
                    new()
                    {
                        FileName = "relay_protocol.log",
                        FileContent = new Text(logContent),
                    }
                }
            });
        }

        return (end, endZone);
    }

    /// <summary>
    /// Adds tier-scaled hazard events for the cascading relay challenge.
    /// Higher tiers get more severe hazards.
    /// </summary>
    private void AddRelayHazards(
        List<WardenObjectiveEvent> events,
        ZoneNode targetNode,
        int relayIndex)
    {
        switch (level.Tier)
        {
            case "A":
                // Mild: just a warning sound
                events.Add(new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.PlaySound,
                    Delay = 6.0,
                    SoundId = Sound.Enemies_DistantLowRoar,
                });
                break;

            case "B":
                // Lights off in the target zone
                events.Add(new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.AllLightsOff,
                    Delay = 6.0,
                });
                events.Add(new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.PlaySound,
                    Delay = 5.5,
                    SoundId = Sound.LightsOff,
                    WardenIntel = ":://WARNING - Electrical systems disrupted"
                });
                break;

            case "C":
                if (relayIndex == 0)
                {
                    // First relay: lights off
                    events.AddLightsOff(delay: 6.0);
                }
                else
                {
                    // Subsequent relays: fog
                    EventBuilder.AddFillFog(events, delay: 6.0, duration: 15.0);
                }
                break;

            case "D":
                if (relayIndex == 0)
                {
                    events.AddLightsOff(delay: 6.0);
                }
                else
                {
                    EventBuilder.AddFillFog(events, delay: 6.0, duration: 15.0);
                    events.AddSpawnWave(GenericWave.Exit_Objective_Easy, delay: 8.0);
                }
                break;

            case "E":
                events.AddLightsOff(delay: 5.0);
                EventBuilder.AddFillFog(events, delay: 6.0, duration: 12.0);
                events.AddSpawnWave(
                    relayIndex == 0
                        ? GenericWave.Exit_Objective_Easy
                        : GenericWave.Exit_Objective_Medium,
                    delay: 8.0);
                break;
        }
    }

    /// <summary>
    /// Quarantine Cascade: A control terminal can unseal 3-4 locked zones, but each release
    /// triggers hazards in other zones. Log files describe the containment matrix so players
    /// can plan the optimal release order.
    ///
    /// Layout:
    ///     control(terminal) -> sector_a(locked)
    ///                       -> sector_b(locked)
    ///                       -> sector_c(locked) -> end (locked, keycard in one sector)
    ///
    /// The sector count scales by tier: A=2, B=2-3, C=3, D=3-4, E=4
    /// </summary>
    public (ZoneNode, Zone) BuildChallenge_QuarantineCascade(ZoneNode start)
    {
        var sectorCount = level.Tier switch
        {
            "A" => 2,
            "B" => Generator.Between(2, 3),
            "C" => 3,
            "D" => Generator.Between(3, 4),
            "E" => 4,
            _ => 2,
        };

        // Make start a hub with enough connections
        start = planner.UpdateNode(start with { MaxConnections = sectorCount + 1 });
        var startZone = planner.GetZone(start)!;
        start = level.GenHubGeomorph(start);

        // Create locked forward door
        var (end, endZone) = AddZone(start, new ZoneNode());

        // Create quarantine sector zones
        var sectorNodes = new List<ZoneNode>();
        var sectorZones = new List<Zone>();
        var sectorNames = new List<string>();

        for (var i = 0; i < sectorCount; i++)
        {
            var (node, zone) = AddZone(start, new ZoneNode { Branch = $"sector_{i}" });
            zone.Coverage = CoverageMinMax.Medium;

            // Lock all sectors
            zone.ProgressionPuzzleToEnter = new ProgressionPuzzle
            {
                PuzzleType = ProgressionPuzzleType.Locked,
                CustomText = $"<color=red>://QUARANTINE ACTIVE - Sector {(char)('A' + i)} sealed. Use control terminal to release.</color>"
            };

            sectorNodes.Add(node);
            sectorZones.Add(zone);
            sectorNames.Add($"SECTOR_{(char)('A' + i)}");
        }

        // Pick which sector has the keycard for the forward door
        var keycardSectorIndex = Generator.Between(0, sectorCount - 1);
        AddKeycardPuzzle(end, sectorNodes[keycardSectorIndex]);

        // Build the containment matrix: releasing sector i triggers hazards in other sectors
        // The matrix is generated so there's one optimal release order
        var hazardMatrix = GenerateContainmentMatrix(sectorCount);

        // Build terminal commands for the control terminal
        var commands = new List<CustomTerminalCommand>();

        for (var i = 0; i < sectorCount; i++)
        {
            var commandEvents = new List<WardenObjectiveEvent>();

            // Unlock this sector
            commandEvents.AddUnlockDoor(
                sectorNodes[i].Bulkhead, sectorNodes[i].ZoneNumber,
                $":://QUARANTINE LIFTED - Sector {(char)('A' + i)} unsealed.",
                delay: 5.0);

            // Trigger hazards in other sectors based on the matrix
            foreach (var (targetSector, hazardType) in hazardMatrix[i])
            {
                AddQuarantineHazard(commandEvents, sectorNodes[targetSector], hazardType);
            }

            commands.Add(new CustomTerminalCommand
            {
                Command = $"RELEASE_{sectorNames[i]}",
                CommandDesc = new Text($"Releases quarantine seal on {sectorNames[i]}. WARNING: May trigger containment responses in adjacent sectors."),
                SpecialCommandRule = CommandRule.OnlyOnceDelete,
                CommandEvents = commandEvents,
                PostCommandOutputs = new List<TerminalOutput>
                {
                    new()
                    {
                        Output = $"Disengaging quarantine seal on {sectorNames[i]}...",
                        Type = LineType.SpinningWaitNoDone,
                        Time = 2.0
                    },
                    new()
                    {
                        Output = "Verifying containment integrity of adjacent sectors...",
                        Type = LineType.SpinningWaitDone,
                        Time = 3.0
                    },
                    new()
                    {
                        Output = $"{sectorNames[i]} quarantine lifted. Containment matrix updated.",
                        Type = LineType.Warning,
                        Time = 1.0
                    },
                },
            });
        }

        // Generate containment matrix log
        var matrixLog = GenerateContainmentMatrixLog(sectorCount, sectorNames, hazardMatrix);

        startZone.TerminalPlacements.Add(new TerminalPlacement
        {
            UniqueCommands = commands,
            LogFiles = new List<LogFile>
            {
                new()
                {
                    FileName = "containment_matrix.log",
                    FileContent = new Text(matrixLog),
                },
                new()
                {
                    FileName = "quarantine_status.log",
                    FileContent = new Text(GenerateQuarantineStatusLog(sectorCount, sectorNames, keycardSectorIndex)),
                }
            }
        });

        return (end, endZone);
    }

    /// <summary>
    /// Generates a containment matrix defining what hazards each sector release triggers.
    /// Returns: for each sector i, a list of (targetSector, hazardType) pairs.
    /// HazardType: 0=fog, 1=enemies, 2=lights_off
    /// </summary>
    private List<List<(int targetSector, int hazardType)>> GenerateContainmentMatrix(int sectorCount)
    {
        var matrix = new List<List<(int, int)>>();

        for (var i = 0; i < sectorCount; i++)
            matrix.Add(new List<(int, int)>());

        // Each sector release affects 1-2 other sectors
        for (var i = 0; i < sectorCount; i++)
        {
            var targets = new List<int>();
            for (var j = 0; j < sectorCount; j++)
            {
                if (j != i) targets.Add(j);
            }

            // Pick 1-2 targets depending on tier
            var targetCount = level.Tier switch
            {
                "A" => 1,
                "B" => 1,
                "C" => Generator.Between(1, 2),
                "D" => Generator.Between(1, 2),
                "E" => 2,
                _ => 1,
            };

            for (var t = 0; t < Math.Min(targetCount, targets.Count); t++)
            {
                var target = Generator.Pick(targets);
                targets.Remove(target);

                var hazardType = level.Tier switch
                {
                    "A" => 2, // lights off (mildest)
                    "B" => Generator.Between(0, 2),
                    _ => Generator.Between(0, 2),
                };

                matrix[i].Add((target, hazardType));
            }
        }

        return matrix;
    }

    /// <summary>
    /// Adds a specific hazard type to the events list for the quarantine cascade.
    /// </summary>
    private void AddQuarantineHazard(
        List<WardenObjectiveEvent> events,
        ZoneNode targetNode,
        int hazardType)
    {
        var delay = 7.0;

        switch (hazardType)
        {
            case 0: // Fog
                EventBuilder.AddFillFog(events, delay: delay, duration: 15.0,
                    message: ":://WARNING - Containment atmosphere venting into adjacent sector");
                break;

            case 1: // Enemies
                events.AddSpawnWave(
                    level.Tier switch
                    {
                        "A" or "B" => GenericWave.Exit_Objective_Easy,
                        "C" => GenericWave.Exit_Objective_Easy,
                        "D" => GenericWave.Exit_Objective_Medium,
                        "E" => GenericWave.Exit_Objective_Hard,
                        _ => GenericWave.Exit_Objective_Easy,
                    },
                    delay: delay);
                break;

            case 2: // Lights off
                events.AddLightsOff(delay: delay,
                    message: ":://WARNING - Power grid disrupted by quarantine release");
                break;
        }
    }

    private static readonly string[] HazardDescriptions = new[]
    {
        "atmospheric venting (fog contamination)",
        "biological containment breach (hostile entities)",
        "electrical grid disruption (power failure)"
    };

    /// <summary>
    /// Generates the log content describing the containment matrix relationships.
    /// </summary>
    private string GenerateContainmentMatrixLog(
        int sectorCount,
        List<string> sectorNames,
        List<List<(int targetSector, int hazardType)>> matrix)
    {
        var lines = new List<string>
        {
            "CONTAINMENT MATRIX - QUARANTINE ZONE",
            "Classification: RESTRICTED",
            "---",
            "",
            "The following sectors are under active",
            "quarantine. Releasing any sector seal",
            "may trigger cascading containment",
            "responses in linked sectors.",
            "",
            "INTERDEPENDENCY MAP:",
            ""
        };

        for (var i = 0; i < sectorCount; i++)
        {
            lines.Add($"  {sectorNames[i]}:");
            if (matrix[i].Count == 0)
            {
                lines.Add("    No linked sectors.");
            }
            else
            {
                foreach (var (target, hazard) in matrix[i])
                {
                    lines.Add($"    -> {sectorNames[target]}:");
                    lines.Add($"       {HazardDescriptions[hazard]}");
                }
            }
            lines.Add("");
        }

        lines.Add("RECOMMENDATION: Plan release order");
        lines.Add("carefully to minimize exposure to");
        lines.Add("cascading containment failures.");

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Generates the quarantine status log hinting at where the keycard is.
    /// </summary>
    private string GenerateQuarantineStatusLog(
        int sectorCount,
        List<string> sectorNames,
        int keycardSectorIndex)
    {
        var lines = new List<string>
        {
            "QUARANTINE STATUS REPORT",
            "---",
            ""
        };

        for (var i = 0; i < sectorCount; i++)
        {
            lines.Add($"{sectorNames[i]}:");
            if (i == keycardSectorIndex)
            {
                lines.Add("  Status: SEALED");
                lines.Add("  Note: Security keycard last logged");
                lines.Add("  in this sector before lockdown.");
            }
            else
            {
                var statusNote = Generator.Pick(new List<string>
                {
                    "  Status: SEALED",
                    "  Status: SEALED - No critical assets",
                    "  Status: SEALED - Standard inventory",
                });
                lines.Add(statusNote);
            }
            lines.Add("");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Cross-Reference Dossier: Password-locked terminal guarding a door. The password is an
    /// employee ID that can only be determined by cross-referencing personnel records scattered
    /// across 2-4 terminals in adjacent zones. Players must read duty rosters, shift assignments,
    /// and access logs to deduce the correct employee.
    ///
    /// Layout:
    ///     start(hub, password terminal) -> end (locked by password)
    ///                                   -> side_a (duty roster terminal)
    ///                                   -> side_b (shift assignments terminal)
    ///                                   -> side_c (access log terminal) [C+ tiers]
    ///
    /// Terminal count scales by tier: A=2, B=2-3, C=3, D=3-4, E=4
    /// </summary>
    public (ZoneNode, Zone) BuildChallenge_CrossReferenceDossier(ZoneNode start)
    {
        var terminalCount = level.Tier switch
        {
            "A" => 2,
            "B" => Generator.Between(2, 3),
            "C" => 3,
            "D" => Generator.Between(3, 4),
            "E" => 4,
            _ => 2,
        };

        var redHerringCount = level.Tier switch
        {
            "A" => 0,
            "B" => 1,
            "C" => 2,
            "D" => 3,
            "E" => 4,
            _ => 0,
        };

        // Generate personnel data
        var totalPersonnel = 4 + redHerringCount;
        var (correct, allRecords) = LogContentGenerator.GeneratePersonnel(totalPersonnel, redHerringCount);

        // Make start a hub
        start = planner.UpdateNode(start with { MaxConnections = terminalCount + 1 });
        var startZone = planner.GetZone(start)!;
        start = level.GenHubGeomorph(start);

        // Create the locked forward door
        var (end, endZone) = AddZone(start, new ZoneNode());

        // Create side zones with information terminals
        var logGenerators = new List<Func<List<PersonnelRecord>, LogFile>>
        {
            LogContentGenerator.GenerateDutyRoster,
            LogContentGenerator.GenerateShiftAssignments,
            LogContentGenerator.GenerateAccessLog,
            // Fourth terminal (E-tier) gets a duplicate roster with different ordering
            LogContentGenerator.GenerateDutyRoster,
        };

        for (var i = 0; i < terminalCount; i++)
        {
            var (sideNode, sideZone) = AddZone(start, new ZoneNode { Branch = $"dossier_{i}" });
            sideZone.Coverage = CoverageMinMax.Small;

            var logFile = logGenerators[i](allRecords);
            sideZone.TerminalPlacements.Add(new TerminalPlacement
            {
                LogFiles = new List<LogFile> { logFile }
            });
        }

        // Password-protected terminal at the hub
        var terminalState = new TerminalStartingState
        {
            PasswordProtected = true,
            GeneratePassword = false,
            PasswordHintText = LogContentGenerator.GenerateDossierHint(correct),
        };

        // Place the terminal with the door unlock command
        startZone.TerminalPlacements.Add(new TerminalPlacement
        {
            StartingStateData = terminalState,
            UniqueCommands = new List<CustomTerminalCommand>
            {
                new()
                {
                    Command = "AUTHORIZE_ACCESS",
                    CommandDesc = new Text("Authorizes security door access after personnel verification."),
                    SpecialCommandRule = CommandRule.OnlyOnce,
                    CommandEvents = new List<WardenObjectiveEvent>()
                        .AddUnlockDoor(
                            end.Bulkhead, end.ZoneNumber,
                            ":://PERSONNEL VERIFIED - Security door access authorized.",
                            delay: 8.0)
                        .ToList(),
                    PostCommandOutputs = new List<TerminalOutput>
                    {
                        new()
                        {
                            Output = "Verifying personnel credentials...",
                            Type = LineType.SpinningWaitNoDone,
                            Time = 2.0
                        },
                        TerminalOutput.ConfirmingTerminal,
                        new()
                        {
                            Output = "Personnel authorization confirmed. Door access granted.",
                            Type = LineType.Normal,
                            Time = 1.0
                        },
                    },
                }
            }
        });

        // Lock the forward door
        var terminalSerial = Lore.TerminalSerial(
            "Reality", start.Bulkhead, start.ZoneNumber,
            startZone.TerminalPlacements.Count - 1);

        endZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
        {
            PuzzleType = ProgressionPuzzleType.Locked,
            CustomText = $"<color=grey>DOOR LOCKED - Personnel verification required on {terminalSerial}</color>"
        };

        return (end, endZone);
    }

    /// <summary>
    /// Forensic Reconstruction: Fragmented incident logs across terminals describe multiple
    /// incidents. Players must reconstruct which case number matches specific criteria to
    /// form the password for the controlling terminal.
    ///
    /// Layout: Same as CrossReferenceDossier - hub with side zones containing log terminals.
    ///
    /// Incident count scales by tier: A=3, B=4, C=5, D=6, E=8
    /// </summary>
    public (ZoneNode, Zone) BuildChallenge_ForensicReconstruction(ZoneNode start)
    {
        var terminalCount = level.Tier switch
        {
            "A" => 2,
            "B" => 2,
            "C" => 3,
            "D" => 3,
            "E" => 4,
            _ => 2,
        };

        var incidentCount = level.Tier switch
        {
            "A" => 3,
            "B" => 4,
            "C" => 5,
            "D" => 6,
            "E" => 8,
            _ => 3,
        };

        var corrupted = level.Tier is "D" or "E";

        // Generate incident data
        var (correct, allIncidents) = LogContentGenerator.GenerateIncidents(incidentCount);

        // Make start a hub
        start = planner.UpdateNode(start with { MaxConnections = terminalCount + 1 });
        var startZone = planner.GetZone(start)!;
        start = level.GenHubGeomorph(start);

        // Create the locked forward door
        var (end, endZone) = AddZone(start, new ZoneNode());

        // Distribute incidents across terminals
        var incidentsPerTerminal = (int)Math.Ceiling((double)incidentCount / terminalCount);

        for (var i = 0; i < terminalCount; i++)
        {
            var (sideNode, sideZone) = AddZone(start, new ZoneNode { Branch = $"forensic_{i}" });
            sideZone.Coverage = CoverageMinMax.Small;

            var startIdx = i * incidentsPerTerminal;
            var count = Math.Min(incidentsPerTerminal, incidentCount - startIdx);

            if (count <= 0) continue;

            var logFile = LogContentGenerator.GenerateIncidentReport(
                allIncidents, startIdx, count, i, corrupted);

            sideZone.TerminalPlacements.Add(new TerminalPlacement
            {
                LogFiles = new List<LogFile> { logFile }
            });
        }

        // Password-protected terminal at hub
        var terminalState = new TerminalStartingState
        {
            PasswordProtected = true,
            GeneratePassword = false,
            PasswordHintText = LogContentGenerator.GenerateForensicHint(correct),
        };

        startZone.TerminalPlacements.Add(new TerminalPlacement
        {
            StartingStateData = terminalState,
            UniqueCommands = new List<CustomTerminalCommand>
            {
                new()
                {
                    Command = "SUBMIT_RECONSTRUCTION",
                    CommandDesc = new Text("Submits incident reconstruction for security verification."),
                    SpecialCommandRule = CommandRule.OnlyOnce,
                    CommandEvents = new List<WardenObjectiveEvent>()
                        .AddUnlockDoor(
                            end.Bulkhead, end.ZoneNumber,
                            ":://INCIDENT RECONSTRUCTION VERIFIED - Security door unlocked.",
                            delay: 8.0)
                        .ToList(),
                    PostCommandOutputs = new List<TerminalOutput>
                    {
                        new()
                        {
                            Output = "Cross-referencing incident database...",
                            Type = LineType.SpinningWaitNoDone,
                            Time = 2.5
                        },
                        new()
                        {
                            Output = "Validating reconstruction sequence...",
                            Type = LineType.SpinningWaitDone,
                            Time = 3.0
                        },
                        new()
                        {
                            Output = "Incident reconstruction accepted. Access granted.",
                            Type = LineType.Normal,
                            Time = 1.0
                        },
                    },
                }
            }
        });

        // Lock the forward door
        var terminalSerial = Lore.TerminalSerial(
            "Reality", start.Bulkhead, start.ZoneNumber,
            startZone.TerminalPlacements.Count - 1);

        endZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
        {
            PuzzleType = ProgressionPuzzleType.Locked,
            CustomText = $"<color=grey>DOOR LOCKED - Incident reconstruction required on {terminalSerial}</color>"
        };

        return (end, endZone);
    }
}
