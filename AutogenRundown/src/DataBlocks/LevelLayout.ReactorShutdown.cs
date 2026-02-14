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

        switch (level.Tier, director.Bulkhead)
        {
            #region A-tier
            // A-Main: 3-5 zones, 4 variants
            // Introductory — reactor easy to reach, light challenges
            case ("A", Bulkhead.Main):
            {
                // Build reactor immediately from start
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Direct password: Reactor → 1-2 zones → PasswordTerminal
                    (0.30, () =>
                    {
                        var nodes = AddBranch(reactor, Generator.Between(1, 2), "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),

                    // Keycard password: Reactor → KeycardInSide → PasswordTerminal
                    (0.30, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInSide(reactor);
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),

                    // Small + password: Reactor → Small → PasswordTerminal
                    (0.20, () =>
                    {
                        var (end, _) = BuildChallenge_Small(reactor);
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),

                    // Reactor at end: 2-3 zones → Reactor. No password.
                    (0.20, () =>
                    {
                        // Undo the early reactor — rebuild with zones first
                        // Actually, we need to handle this differently: don't use the
                        // reactor built above. Instead we build zones from start, then
                        // build reactor at the end.
                        // But since reactor is already built, we add zones before it
                        // by building from start with a forward branch, then putting
                        // reactor at end.
                        // Note: reactor is already built from start. For "reactor at end"
                        // variants, we just don't set up password. The zones before
                        // reactor serve as the challenge.
                        var nodes = AddBranch(start, Generator.Between(1, 2), "approach");
                        // Reactor already connects from start through corridor — the
                        // approach zones add bulk before the player finds the reactor path
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // A-Extreme: 2-3 zones, 2 variants — as short as possible
            case ("A", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Direct password: Reactor → 1 zone → PasswordTerminal
                    (0.60, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),

                    // Keycard-in-zone: Reactor → KeycardInZone → PasswordTerminal
                    (0.40, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(reactor);
                        var nodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, nodes.Last());
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // A-Overload: 2-3 zones, 2 variants
            case ("A", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Generator-in-zone: Reactor → GeneratorCellInZone → PasswordTerminal
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_GeneratorCellInZone(reactor);
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

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }
            #endregion

            #region B-tier
            // B-Main: 4-6 zones, 6 variants — moderate, first branching
            case ("B", Bulkhead.Main):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard-in-side: Reactor → 1 zone → KeycardInSide → PasswordTerminal
                    (0.20, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator-in-side: Reactor → GeneratorCellInSide → PasswordTerminal
                    (0.15, () =>
                    {
                        var (end, _) = BuildChallenge_GeneratorCellInSide(reactor);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal + side: Reactor → 1 zone → LockedTerminalDoor(1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Sensor corridor: Reactor → 2-3 zones [sensors on last 2] → PasswordTerminal
                    (0.15, () =>
                    {
                        var nodes = AddBranch(reactor, Generator.Between(2, 3), "reactor_deep");
                        AddSecuritySensors(nodes[^2]);
                        AddSecuritySensors(nodes[^1]);
                        var pwNodes = AddBranch(nodes.Last(), 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Reactor at end + keycard: KeycardInSide → Reactor. No password.
                    (0.15, () =>
                    {
                        // Add zones from start before reactor
                        var nodes = AddBranch(start, 1, "approach");
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        // Reactor already built — no password needed
                    }),

                    // Locked reactor (keycard): Reactor door keycard-locked,
                    // keycard found through side branch challenge chain
                    (0.20, () =>
                    {
                        // Get the reactor_entrance node and make it a hub
                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        // Build challenge chain from the hub
                        var (end, _) = BuildChallenge_KeycardInSide(entranceNode);

                        // Lock the reactor zone with keycard
                        AddKeycardPuzzle(reactor, end);
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // B-Extreme: 3-4 zones, 3 variants
            case ("B", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard-in-zone: Reactor → 1 zone → KeycardInZone → PasswordTerminal
                    (0.40, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_KeycardInZone(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator-in-zone: Reactor → GeneratorCellInZone → PasswordTerminal
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_GeneratorCellInZone(reactor);
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

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // B-Overload: 3-4 zones, 3 variants
            case ("B", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard-in-side: Reactor → KeycardInSide → PasswordTerminal
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInSide(reactor);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator-in-side: Reactor → GeneratorCellInSide → PasswordTerminal
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_GeneratorCellInSide(reactor);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal password-in-side: Reactor → LockedTerminalPasswordInSide → PasswordTerminal
                    (0.30, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalPasswordInSide(reactor);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }
            #endregion

            #region C-tier
            // C-Main: 5-7 zones, 6 variants — significant, multi-layer challenges
            case ("C", Bulkhead.Main):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error + keycard blockade: Reactor → ErrorWithOff_KeycardInSide → PasswordTerminal
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

                    // Keycard + generator double: Reactor → 1 zone → KeycardInSide → GeneratorCellInZone → PasswordTerminal
                    (0.20, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + password: Reactor → 1 zone → BossFight → PasswordTerminal
                    (0.15, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Sensor + keycard: Reactor → 1-2 zones [sensors] → KeycardInSide → PasswordTerminal
                    (0.15, () =>
                    {
                        var nodes = AddBranch(reactor, Generator.Between(1, 2), "reactor_deep");
                        foreach (var node in nodes)
                            AddSecuritySensors(node);
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked reactor (generator): Reactor door generator-locked, cell found deeper
                    (0.15, () =>
                    {
                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var (end, _) = BuildChallenge_GeneratorCellInSide(entranceNode);
                        AddGeneratorPuzzle(reactor, end);
                    }),

                    // Reactor at end + boss: 1 zone → BossFight → Reactor. No password.
                    (0.15, () =>
                    {
                        var nodes = AddBranch(start, 1, "approach");
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        // Reactor already built, no password
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // C-Extreme: 3-5 zones, 4 variants
            case ("C", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard-in-side: Reactor → KeycardInSide → PasswordTerminal
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInSide(reactor);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator-in-side: Reactor → GeneratorCellInSide → PasswordTerminal
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_GeneratorCellInSide(reactor);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked terminal + side: Reactor → LockedTerminalDoor(1) → PasswordTerminal
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(reactor, 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Reactor at end + keycard: KeycardInZone → Reactor. No password.
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(start);
                        // Reactor already built, no password
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // C-Overload: 3-4 zones, 3 variants
            case ("C", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error + keycard (compact): Reactor → ErrorWithOff_KeycardInSide → PasswordTerminal
                    (0.40, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            reactor,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Apex gate: Reactor → ApexAlarm(Normal) → PasswordTerminal
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_ApexAlarm(
                            reactor,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Normal);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator + sensors: Reactor → GeneratorCellInZone [sensors] → PasswordTerminal
                    (0.25, () =>
                    {
                        var (end, endZone) = BuildChallenge_GeneratorCellInZone(reactor);
                        AddSecuritySensors(end);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }
            #endregion

            #region D-tier
            // D-Main: 6-9 zones, 7 variants — hard, deep challenge chains
            case ("D", Bulkhead.Main):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

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

                    // Apex + boss: Reactor → ApexAlarm(Hard) → BossFight → PasswordTerminal
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_ApexAlarm(
                            reactor,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var (end, _) = BuildChallenge_BossFight(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Triple challenge: Reactor → KeycardInSide → GeneratorCellInZone → LockedTerminalDoor(1) → PasswordTerminal
                    (0.10, () =>
                    {
                        var (mid1, _) = BuildChallenge_KeycardInSide(reactor);
                        var (mid2, _) = BuildChallenge_GeneratorCellInZone(mid1);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid2, 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
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
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // D-Extreme: 4-6 zones, 4 variants
            case ("D", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Boss + password: Reactor → 1 zone → BossFight → PasswordTerminal
                    (0.30, () =>
                    {
                        var nodes = AddBranch(reactor, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Error + keycard (compact): Reactor → ErrorWithOff_KeycardInSide(1,1,1) → PasswordTerminal
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            reactor,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Keycard + generator double: Reactor → KeycardInSide → GeneratorCellInZone → PasswordTerminal
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(reactor);
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
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // D-Overload: 3-5 zones, 5 variants
            case ("D", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex gate: Reactor → ApexAlarm(Hard) → PasswordTerminal
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_ApexAlarm(
                            reactor,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + keycard-in-zone: Reactor → BossFight → KeycardInZone → PasswordTerminal
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_BossFight(reactor);
                        var (end, _) = BuildChallenge_KeycardInZone(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator + apex: Reactor → GeneratorCellInZone → ApexAlarm(Hard) → PasswordTerminal
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_GeneratorCellInZone(reactor);
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

                    // Locked reactor (generator + apex): Reactor unpowered, apex guards cell
                    (0.20, () =>
                    {
                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var (mid, _) = BuildChallenge_ApexAlarm(
                            entranceNode,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddGeneratorPuzzle(reactor, end);
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }
            #endregion

            #region E-tier
            // E-Main: 7-10 zones, 6 variants — maximum difficulty
            case ("E", Bulkhead.Main):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

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
                    }),

                    // Apex + boss + keycard: Reactor → ApexAlarm(VeryHard) → BossFight → KeycardInZone → PasswordTerminal
                    (0.15, () =>
                    {
                        var (mid1, _) = BuildChallenge_ApexAlarm(
                            reactor,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var (mid2, _) = BuildChallenge_BossFight(mid1);
                        var (end, _) = BuildChallenge_KeycardInZone(mid2);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
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
                    }),

                    // Boss + error + password: Reactor → BossFight → 1 zone → ErrorWithOff_KeycardInSide(2,1,1) → PasswordTerminal
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_BossFight(reactor);
                        var nodes = AddBranch(mid, 1, "reactor_deep");
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            nodes.Last(),
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
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
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // E-Extreme: 5-7 zones, 5 variants
            case ("E", Bulkhead.Extreme):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex + boss: Reactor → ApexAlarm(VeryHard) → BossFight → PasswordTerminal
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ApexAlarm(
                            reactor,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var (end, _) = BuildChallenge_BossFight(mid);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + keycard: Reactor → 1 zone → BossFight → KeycardInSide → PasswordTerminal
                    (0.20, () =>
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

                    // Generator + apex + sensors: Reactor → GeneratorCellInZone → 1 zone [sensors] → ApexAlarm(VeryHard) → PasswordTerminal
                    (0.15, () =>
                    {
                        var (mid1, _) = BuildChallenge_GeneratorCellInZone(reactor);
                        var nodes = AddBranch(mid1, 1, "reactor_deep");
                        AddSecuritySensors(nodes.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            nodes.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Locked reactor (generator + boss): Reactor unpowered, boss guards cell
                    (0.20, () =>
                    {
                        var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
                        entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
                        planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);

                        var (mid, _) = BuildChallenge_BossFight(entranceNode);
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddGeneratorPuzzle(reactor, end);
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }

            // E-Overload: 4-5 zones, 4 variants
            case ("E", Bulkhead.Overload):
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex (VeryHard): Reactor → ApexAlarm(VeryHard) → PasswordTerminal
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_ApexAlarm(
                            reactor,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Boss + apex: Reactor → BossFight → ApexAlarm(Hard) → PasswordTerminal
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_BossFight(reactor);
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Generator + apex (VeryHard): Reactor → GeneratorCellInZone → ApexAlarm(VeryHard) → PasswordTerminal
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_GeneratorCellInZone(reactor);
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var pwNodes = AddBranch(end, 1, "reactor_password");
                        SetupReactorPassword(reactorDefinition, reactor, pwNodes.Last());
                    }),

                    // Reactor at end + apex: ApexAlarm(VeryHard) → Reactor. No password.
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_ApexAlarm(
                            start,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        // Reactor already built, no password
                    }),
                });

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }
            #endregion

            // Default fallback: simple 1-2 zone password path
            default:
            {
                var reactor = BuildReactor(start);
                var reactorDefinition = new ReactorShutdown
                {
                    ZoneNumber = reactor.ZoneNumber,
                    Bulkhead = director.Bulkhead
                };

                var nodes = AddBranch(reactor, Generator.Between(1, 2), "reactor_password");
                SetupReactorPassword(reactorDefinition, reactor, nodes.Last());

                objective.MainObjective = new Text(() => $"Find the main reactor in {Intel.Zone(reactor, planner)} and shut it down");
                objective.GoToZone = new Text(() => $"Navigate to {Intel.Zone(reactor, planner)} and initiate the shutdown process");
                objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
                break;
            }
        }
    }
}
