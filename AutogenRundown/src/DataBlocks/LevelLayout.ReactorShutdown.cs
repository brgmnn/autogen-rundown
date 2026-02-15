using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Sets up reactor password protection: red lights on reactor zone, password terminal
    /// in the code terminal zone, and optional garden geomorph.
    /// </summary>
    private void SetupReactorPassword(
        ReactorShutdown reactorDefinition,
        ZoneNode reactor,
        ZoneNode codeTerminalZone)
    {
        var reactorZone = planner.GetZone(reactor)!;
        reactorZone.LightSettings = Lights.Light.Monochrome_Red_R7D1;

        reactorDefinition.Password.PasswordProtected = true;
        reactorDefinition.Password.TerminalZoneSelectionDatas = new()
        {
            new()
            {
                new ZoneSelectionData
                {
                    ZoneNumber = codeTerminalZone.ZoneNumber,
                    Bulkhead = director.Bulkhead
                }
            }
        };

        if (Generator.Flip(0.66))
        {
            var codeTerminalZoneData = planner.GetZone(codeTerminalZone)!;
            codeTerminalZoneData.GenGardenGeomorph(level.Complex);
        }
    }

    /// <summary>
    /// Fast version of reactor shutdown layout for ReachKdsDeep objective.
    /// Builds reactor directly from start with no password.
    /// </summary>
    public void BuildLayout_ReactorShutdown_Fast(ZoneNode start)
    {
        var reactor = BuildReactor(start);

        var reactorDefinition = new ReactorShutdown
        {
            ZoneNumber = reactor.ZoneNumber,
            Bulkhead = director.Bulkhead
        };

        objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
        objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");

        objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
    }

    /// <summary>
    /// Builds a reactor shutdown level layout.
    ///
    /// Core design inversion: unlike most objectives where challenges lead TO the target,
    /// ReactorShutdown places the reactor EARLY and challenges extend DEEPER past it.
    /// Players see the reactor immediately but must venture further to find the password/unlock.
    ///
    /// Three reactor placement modes:
    ///   - Password-locked (majority): Reactor early, password terminal deeper. Red lights.
    ///   - Door-locked (minority): Reactor early, door locked, unlock item deeper.
    ///   - Reactor at end (minority): No password. Challenge builders first, reactor last.
    /// </summary>
    public void BuildLayout_ReactorShutdown(
            BuildDirector director,
            WardenObjective objective,
            ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;

        // --- Fast version ---
        if (level.MainDirector.Objective is WardenObjectiveType.ReachKdsDeep)
        {
            BuildLayout_ReactorShutdown_Fast(start);
            return;
        }

        // Shared state: set by each case, used in the footer after the switch.
        // Cases with "reactor at end" variants set reactorNode inside each lambda.
        ZoneNode? reactorNode = null;
        var reactorDefinition = new ReactorShutdown { Bulkhead = director.Bulkhead };

        switch (level.Tier, director.Bulkhead)
        {
            #region A-tier
            // A-Main: 3-5 zones, 5 variants
            // Introductory — reactor easy to reach, light challenges.
            // Has "reactor at end" variant: BuildReactor inside each lambda.
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Direct password: Reactor → 1-2 zones → PasswordTerminal
                    (0.25, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var nodes = AddBranch(reactor, Generator.Between(1, 2), "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Keycard password: Reactor → transition → KeycardInSide → PasswordTerminal
                    (0.20, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_KeycardInSide(deep.Last());
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Locked terminal: Reactor → transition → LockedTerminalDoor(1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalDoor(deep.Last(), 1);
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Small + password: Reactor → Small → PasswordTerminal
                    (0.20, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var (end, _) = BuildChallenge_Small(reactor);
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Reactor at end: 1-2 approach zones → Reactor. No password.
                    (0.20, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(1, 2), "approach");
                        var reactor = BuildReactor(nodes.Last());
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        AddForwardExtractStart(reactor);
                    }),
                });
                break;
            }

            // A-Extreme: 2-3 zones, 3 variants — as short as possible
            case ("A", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Direct password: Reactor → 1 zone → PasswordTerminal
                    (0.45, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),

                    // Keycard-in-zone: Reactor → KeycardInZone → PasswordTerminal
                    (0.30, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(reactor);
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),

                    // Locked terminal: Reactor → LockedTerminalDoor(0) → PasswordTerminal
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(reactor, 0);
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),
                });
                break;
            }

            // A-Overload: 2-3 zones, 2 variants
            case ("A", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Generator-in-zone: Reactor → transition → GeneratorCellInZone → PasswordTerminal
                    (0.50, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_GeneratorCellInZone(deep.Last());
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),

                    // Locked terminal: Reactor → LockedTerminalDoor(0) → PasswordTerminal
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(reactor, 0);
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),
                });
                break;
            }
            #endregion

            #region B-tier
            // B-Main: 4-6 zones, 6 variants — moderate, first branching.
            // Has "reactor at end" variant: BuildReactor inside each lambda.
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard-in-side: Reactor → 1 zone → KeycardInSide → PasswordTerminal
                    (0.20, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Generator-in-side: Reactor → transition → GeneratorCellInSide → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_GeneratorCellInSide(deep.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Locked terminal + side: Reactor → 1 zone → LockedTerminalDoor(1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Sensor corridor: Reactor → 2-3 zones [sensors on last 2] → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var nodes = AddBranch(reactor, Generator.Between(2, 3), "reactor_deep");
                        AddSecuritySensors(nodes[^2]);
                        AddSecuritySensors(nodes[^1]);
                        var pwNodes = AddBranch(nodes.Last(), 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Reactor at end + keycard: KeycardInSide → Reactor. No password.
                    (0.15, () =>
                    {
                        var nodes = AddBranch(start, 1, "approach");
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var reactor = BuildReactor(end);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        AddForwardExtractStart(reactor);
                    }),

                    // Locked reactor (keycard): Reactor door keycard-locked,
                    // keycard found through side branch challenge chain
                    (0.20, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var (end, _) = BuildChallenge_KeycardInSide(entranceNode);
                        AddKeycardPuzzle(reactor, end);
                        AddForwardExtractStart(end);
                    }),
                });
                break;
            }

            // B-Extreme: 3-4 zones, 4 variants
            case ("B", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard-in-zone: Reactor → 1 zone → KeycardInZone → PasswordTerminal
                    (0.25, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_KeycardInZone(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator-in-zone: Reactor → transition → GeneratorCellInZone → PasswordTerminal
                    (0.25, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_GeneratorCellInZone(deep.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal: Reactor → 1 zone → LockedTerminalDoor(0) → PasswordTerminal
                    (0.25, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 0);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Direct + sensors: Reactor → 1-2 zones [sensors] → PasswordTerminal
                    (0.25, () =>
                    {
                        var nodes = AddBranch(reactor, Generator.Between(1, 2), "reactor_deep");
                        foreach (var node in nodes)
                            AddSecuritySensors(node);
                        var pwNodes = AddBranch(nodes.Last(), 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),
                });
                break;
            }

            // B-Overload: 3-4 zones, 3 variants
            case ("B", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard-in-side: Reactor → transition → KeycardInSide → PasswordTerminal
                    (0.35, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_KeycardInSide(deep.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator-in-side: Reactor → transition → GeneratorCellInSide → PasswordTerminal
                    (0.35, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_GeneratorCellInSide(deep.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal password-in-side: Reactor → transition → LockedTerminalPasswordInSide → PasswordTerminal
                    (0.30, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalPasswordInSide(deep.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),
                });
                break;
            }
            #endregion

            #region C-tier
            // C-Main: 5-7 zones, 7 variants — significant, multi-layer challenges.
            // Has "reactor at end" variant: BuildReactor inside each lambda.
            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error + keycard blockade: Reactor → ErrorWithOff_KeycardInSide → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            reactor,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Keycard + generator double: Reactor → 1 zone → KeycardInSide → GeneratorCellInZone → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Boss + password: Reactor → 1 zone → BossFight → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Sensor + keycard: Reactor → 1-2 zones [sensors] → KeycardInSide → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var nodes = AddBranch(reactor, Generator.Between(1, 2), "reactor_deep");
                        foreach (var node in nodes)
                            AddSecuritySensors(node);
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Locked terminal + deep: Reactor → 1 zone → LockedTerminalDoor(1) → PasswordTerminal
                    (0.10, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Locked reactor (generator): Reactor door generator-locked, cell in side branch
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var cellNodes = AddBranch(entranceNode, Generator.Between(1, 2), "power_cell");
                        AddGeneratorPuzzle(reactor, cellNodes.Last());
                        AddForwardExtractStart(cellNodes.Last());
                    }),

                    // Reactor at end + boss: 1 zone → BossFight → Reactor. No password.
                    (0.15, () =>
                    {
                        var nodes = AddBranch(start, 1, "approach");
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        var reactor = BuildReactor(end);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        AddForwardExtractStart(reactor);
                    }),
                });
                break;
            }

            // C-Extreme: 3-5 zones, 4 variants.
            // Has "reactor at end" variant: BuildReactor inside each lambda.
            case ("C", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard-in-side: Reactor → transition → KeycardInSide → PasswordTerminal
                    (0.25, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_KeycardInSide(deep.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator-in-side: Reactor → transition → GeneratorCellInSide → PasswordTerminal
                    (0.25, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_GeneratorCellInSide(deep.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal + side: Reactor → transition → LockedTerminalDoor(1) → PasswordTerminal
                    (0.25, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalDoor(deep.Last(), 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Reactor at end + keycard: KeycardInZone → Reactor. No password.
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(start);
                        var reactor = BuildReactor(end);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                    }),
                });
                break;
            }

            // C-Overload: 3-4 zones, 4 variants
            case ("C", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error + keycard (compact): Reactor → ErrorWithOff_KeycardInSide → PasswordTerminal
                    (0.30, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            reactor,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Apex gate: Reactor → transition → ApexAlarm(Normal) → PasswordTerminal
                    (0.25, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_ApexAlarm(
                            deep.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Normal);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator + sensors: Reactor → transition → GeneratorCellInZone [sensors] → PasswordTerminal
                    (0.25, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_GeneratorCellInZone(deep.Last());
                        AddSecuritySensors(end);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal: Reactor → transition → LockedTerminalDoor(1) → PasswordTerminal
                    (0.20, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalDoor(deep.Last(), 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),
                });
                break;
            }
            #endregion

            #region D-tier
            // D-Main: 6-9 zones, 7 variants — hard, deep challenge chains
            case ("D", Bulkhead.Main):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error + keycard (deep): Reactor → 1 zone → ErrorWithOff_KeycardInSide(2,1,1) → PasswordTerminal
                    (0.20, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            nodes.Last(),
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Error + generator carry: Reactor → ErrorWithOff_GeneratorCellCarry(2,1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            reactor,
                            errorZones: 2,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Boss + keycard: Reactor → 1 zone → BossFight → KeycardInSide → PasswordTerminal
                    (0.15, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        var (end, _) = BuildChallenge_KeycardInSide(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Apex + boss: Reactor → transition → ApexAlarm(Hard) → BossFight → PasswordTerminal
                    (0.15, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_ApexAlarm(
                            deep.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var (end, _) = BuildChallenge_BossFight(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Triple challenge: Reactor → transition → KeycardInSide → GeneratorCellInZone → LockedTerminalDoor(1) → PasswordTerminal
                    (0.10, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid1, _) = BuildChallenge_KeycardInSide(deep.Last());
                        var (mid2, _) = BuildChallenge_GeneratorCellInZone(mid1);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid2, 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                    }),

                    // Sensor + error: Reactor → 2 zones [sensors] → ErrorWithOff_KeycardInSide(1,1,1) → PasswordTerminal
                    (0.10, () =>
                    {
                        var nodes = AddBranch(reactor, 2, "reactor_deep");
                        foreach (var node in nodes)
                            AddSecuritySensors(node);
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            nodes.Last(),
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Locked reactor (keycard + boss): Reactor door keycard-locked, boss guards keycard
                    (0.15, () =>
                    {
                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var nodes = AddBranch(entranceNode, 1, "reactor_unlock");
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        var (end, _) = BuildChallenge_KeycardInZone(mid);
                        AddKeycardPuzzle(reactor, end);
                        AddForwardExtractStart(end);
                    }),
                });
                break;
            }

            // D-Extreme: 4-6 zones, 5 variants
            case ("D", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Boss + password: Reactor → 1 zone → BossFight → PasswordTerminal
                    (0.25, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Error + keycard (compact): Reactor → ErrorWithOff_KeycardInSide(1,1,1) → PasswordTerminal
                    (0.20, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            reactor,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Keycard + generator double: Reactor → transition → KeycardInSide → GeneratorCellInZone → PasswordTerminal
                    (0.20, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_KeycardInSide(deep.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Sensor + apex: Reactor → 1 zone [sensors] → ApexAlarm(Hard) → PasswordTerminal
                    (0.20, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        AddSecuritySensors(nodes.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            nodes.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal + deep: Reactor → 1 zone → LockedTerminalDoor(1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),
                });
                break;
            }

            // D-Overload: 3-5 zones, 6 variants
            case ("D", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex gate: Reactor → transition → ApexAlarm(Hard) → PasswordTerminal
                    (0.20, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_ApexAlarm(
                            deep.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + keycard-in-zone: Reactor → transition → BossFight → KeycardInZone → PasswordTerminal
                    (0.15, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_BossFight(deep.Last());
                        var (end, _) = BuildChallenge_KeycardInZone(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator + apex: Reactor → transition → GeneratorCellInZone → ApexAlarm(Hard) → PasswordTerminal
                    (0.20, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_GeneratorCellInZone(deep.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Error + keycard (tight): Reactor → ErrorWithOff_KeycardInSide(1,1,1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            reactor,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + locked terminal: Reactor → transition → BossFight → LockedTerminalDoor(0) → PasswordTerminal
                    (0.10, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_BossFight(deep.Last());
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 0);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked reactor (generator + apex): Reactor unpowered, apex guards cell
                    (0.20, () =>
                    {
                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            entranceNode,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var cellNodes = AddBranch(apexEnd, 1, "power_cell");
                        AddGeneratorPuzzle(reactor, cellNodes.Last());
                    }),
                });
                break;
            }
            #endregion

            #region E-tier
            // E-Main: 7-10 zones, 6 variants — maximum difficulty
            case ("E", Bulkhead.Main):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error deep + boss: Reactor → ErrorWithOff_KeycardInSide(2-3,1,1) → BossFight → PasswordTerminal
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            reactor,
                            errorZones: Generator.Between(2, 3),
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var (end, _) = BuildChallenge_BossFight(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Error + generator carry + apex: Reactor → ErrorWithOff_GeneratorCellCarry(2,1) → ApexAlarm(VeryHard) → PasswordTerminal
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            reactor,
                            errorZones: 2,
                            terminalTurnoffZones: 1);
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Apex + boss + keycard: Reactor → transition → ApexAlarm(VeryHard) → BossFight → KeycardInZone → PasswordTerminal
                    (0.15, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid1, _) = BuildChallenge_ApexAlarm(
                            deep.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var (mid2, _) = BuildChallenge_BossFight(mid1);
                        var (end, _) = BuildChallenge_KeycardInZone(mid2);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Triple + sensors: Reactor → 1 zone [sensors] → KeycardInSide → GeneratorCellInSide → LockedTerminalDoor(1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        AddSecuritySensors(nodes.Last());
                        var (mid1, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (mid2, _) = BuildChallenge_GeneratorCellInSide(mid1);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid2, 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Boss + error + password: Reactor → transition → BossFight → 1 zone → ErrorWithOff_KeycardInSide(2,1,1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_BossFight(deep.Last());
                        var nodes = AddBranch(mid, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            nodes.Last(),
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                        AddForwardExtractStart(pwNodes.Last());
                        AddForwardExtractStart(reactor, chance: 0.3);
                    }),

                    // Locked reactor (keycard + error): Reactor keycard-locked, error blockade guards key
                    (0.15, () =>
                    {
                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            entranceNode,
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        AddKeycardPuzzle(reactor, end);
                        AddForwardExtractStart(end);
                    }),
                });
                break;
            }

            // E-Extreme: 5-7 zones, 6 variants
            case ("E", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex + boss: Reactor → transition → ApexAlarm(VeryHard) → BossFight → PasswordTerminal
                    (0.20, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_ApexAlarm(
                            deep.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var (end, _) = BuildChallenge_BossFight(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + keycard: Reactor → 1 zone → BossFight → KeycardInSide → PasswordTerminal
                    (0.15, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        var (end, _) = BuildChallenge_KeycardInSide(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Error + keycard: Reactor → ErrorWithOff_KeycardInSide(1-2,1,1) → PasswordTerminal
                    (0.20, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            reactor,
                            errorZones: Generator.Between(1, 2),
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator + apex + sensors: Reactor → transition → GeneratorCellInZone → 1 zone [sensors] → ApexAlarm(VeryHard) → PasswordTerminal
                    (0.15, () =>
                    {
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid1, _) = BuildChallenge_GeneratorCellInZone(deep.Last());
                        var nodes = AddBranch(mid1, 1, "reactor_deep");
                        AddSecuritySensors(nodes.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            nodes.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + locked terminal: Reactor → 1 zone → BossFight → LockedTerminalDoor(1) → PasswordTerminal
                    (0.10, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked reactor (generator + boss): Reactor unpowered, boss guards cell
                    (0.20, () =>
                    {
                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var (bossEnd, _) = BuildChallenge_BossFight(entranceNode);
                        var cellNodes = AddBranch(bossEnd, 1, "power_cell");
                        AddGeneratorPuzzle(reactor, cellNodes.Last());
                    }),
                });
                break;
            }

            // E-Overload: 4-5 zones, 5 variants.
            // Has "reactor at end" variant: BuildReactor inside each lambda.
            case ("E", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex (VeryHard): Reactor → transition → ApexAlarm(VeryHard) → PasswordTerminal
                    (0.20, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_ApexAlarm(
                            deep.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + apex: Reactor → transition → BossFight → ApexAlarm(Hard) → PasswordTerminal
                    (0.20, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_BossFight(deep.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator + apex (VeryHard): Reactor → transition → GeneratorCellInZone → ApexAlarm(VeryHard) → PasswordTerminal
                    (0.20, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_GeneratorCellInZone(deep.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal + apex: Reactor → transition → LockedTerminalDoor(0) → ApexAlarm(VeryHard) → PasswordTerminal
                    (0.15, () =>
                    {
                        var reactor = BuildReactor(start);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                        var deep = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_LockedTerminalDoor(deep.Last(), 0);
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Reactor at end + apex: approach → ApexAlarm(VeryHard) → Reactor. No password.
                    (0.25, () =>
                    {
                        var nodes = AddBranch(start, 1, "approach");
                        var (end, _) = BuildChallenge_ApexAlarm(
                            nodes.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var reactor = BuildReactor(end);
                        reactorNode = reactor;
                        reactorDefinition.ZoneNumber = reactor.ZoneNumber;
                    }),
                });
                break;
            }
            #endregion

            // Default fallback: simple 1-2 zone password path
            default:
            {
                var reactor = BuildReactor(start);
                reactorNode = reactor;
                reactorDefinition.ZoneNumber = reactor.ZoneNumber;

                var nodes = AddBranch(reactor, Generator.Between(1, 2), "reactor_password");
                SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                AddForwardExtractStart(nodes.Last());
                break;
            }
        }

        // Shared footer — reactorNode is guaranteed non-null (set in every code path above)
        objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactorNode!.Value, planner)} and shut it down");
        objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactorNode!.Value, planner)} and initiate the shutdown process");
        objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
    }
}
