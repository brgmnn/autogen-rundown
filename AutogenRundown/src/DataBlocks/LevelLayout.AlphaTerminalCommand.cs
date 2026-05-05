using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using AutogenRundown.Patches.CustomTerminals;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Builds the Reality side of an AlphaTerminalCommand mission:
    /// elevator drop hosts the Matter Wave Projector pickup, a tier-scaled
    /// challenge chain leads to a portal geomorph, and warping the portal
    /// teleports the team into a static alpha dimension where a custom
    /// terminal hosts the warden objective command.
    ///
    /// Only runs for the Main bulkhead — this objective is Main-only.
    /// </summary>
    public void BuildLayout_AlphaTerminalCommand(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode start)
    {
        if (director.Bulkhead != Bulkhead.Main)
            return;

        // Drop the Matter Wave Projector into the actual elevator drop zone
        // (zone 0 in Reality), regardless of what `start` is. The bulkhead
        // strategy decides whether `start` is the elevator drop or a separate
        // bulkhead first zone — going through zone 0 directly avoids that
        // ambiguity and keeps the pickup in the elevator like the spec asks.
        var elevatorDrop = level.Planner.GetZoneNode(0, DimensionIndex.Reality);
        var elevatorDropZone = level.Planner.GetZone(elevatorDrop)!;
        elevatorDropZone.BigPickupDistributionInZone = BigPickupDistribution.MatterWaveProjector.PersistentId;

        // Insert a corridor zone immediately after `start` before any challenge
        // is built. Most challenge helpers call level.GenHubGeomorph(start)
        // which overwrites the source zone's geomorph -- doing that on the
        // elevator drop wipes out the elevator and Builder.GetElevatorArea()
        // returns null. The corridor isolates the elevator/bulkhead start zone
        // from challenge geomorph rewrites.
        var corridorNodes = AddBranch_Forward(start, 1);
        var challengeRoot = corridorNodes.Last();
        var challengeRootZone = planner.GetZone(challengeRoot)!;
        challengeRootZone.Coverage = CoverageMinMax.Medium;

        // Beyond the corridor we need an interesting traversal challenge before
        // the portal — modeled after HsuFindSample's tier-driven SelectRun. The
        // last node in the chain (`beforePortal`) is what the portal is built
        // from.
        var beforePortal = challengeRoot;

        switch (level.Tier)
        {
            #region Tier: A — short and forgiving
            case "A":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight shot, 1 zone of corridor before the portal
                    (0.30, () =>
                    {
                        beforePortal = AddBranch_Forward(challengeRoot, 1).Last();
                    }),

                    // Keycard in side
                    (0.40, () =>
                    {
                        var (locked, _) = BuildChallenge_KeycardInSide(challengeRoot);
                        beforePortal = locked;
                    }),

                    // Generator cell in side
                    (0.30, () =>
                    {
                        var (locked, _) = BuildChallenge_GeneratorCellInSide(challengeRoot);
                        beforePortal = locked;
                    }),
                });
                break;
            }
            #endregion

            #region Tier: B — pick a single locked challenge
            case "B":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.30, () =>
                    {
                        var (locked, _) = BuildChallenge_KeycardInSide(challengeRoot);
                        beforePortal = locked;
                    }),

                    (0.30, () =>
                    {
                        var (locked, _) = BuildChallenge_GeneratorCellInSide(challengeRoot);
                        beforePortal = locked;
                    }),

                    (0.20, () =>
                    {
                        var (locked, _) = BuildChallenge_LockedTerminalDoor(challengeRoot, sideZones: 1);
                        beforePortal = locked;
                    }),

                    (0.20, () =>
                    {
                        var (mid, _) = AddTravelScanAlarm(challengeRoot);
                        beforePortal = AddBranch_Forward(mid, 1).Last();
                    }),
                });
                break;
            }
            #endregion

            #region Tier: C — a prelude zone plus a meatier locked challenge
            case "C":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // 1 prelude zone, then keycard locked
                    (0.25, () =>
                    {
                        var prelude = AddBranch_Forward(challengeRoot, 1).Last();
                        var (locked, _) = BuildChallenge_KeycardInSide(prelude);
                        beforePortal = locked;
                    }),

                    // // 1 prelude zone, then generator locked
                    // (0.25, () =>
                    // {
                    //     var prelude = AddBranch_Forward(start, 1).Last();
                    //     var (locked, _) = BuildChallenge_GeneratorCellInSide(prelude);
                    //     beforePortal = locked;
                    // }),
                    //
                    // // Apex alarm into the portal hub
                    // (0.25, () =>
                    // {
                    //     var population = level.Settings.HasFlyers()
                    //         ? WavePopulation.Baseline_Flyers
                    //         : WavePopulation.Baseline;
                    //     var (locked, _) = BuildChallenge_ApexAlarm(start, population, WaveSettings.Baseline_Normal);
                    //     beforePortal = locked;
                    // }),
                    //
                    // // Travel scan + keycard
                    // (0.25, () =>
                    // {
                    //     var (mid, _) = AddTravelScanAlarm(start);
                    //     var (locked, _) = BuildChallenge_KeycardInSide(mid);
                    //     beforePortal = locked;
                    // }),
                });
                break;
            }
            #endregion

            #region Tier: D / E — multi-stage with apex / error alarms
            case "D":
            case "E":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error alarm with turnoff + keycard, leading to portal
                    (0.30, () =>
                    {
                        var (locked, _) = BuildChallenge_ErrorWithOff_KeycardInSide(challengeRoot, errorZones: 2, sideKeycardZones: 1, terminalTurnoffZones: 1);
                        beforePortal = locked;
                    }),

                    // Apex alarm with strong population
                    (0.30, () =>
                    {
                        var population = WavePopulation.Baseline_Hybrids;

                        if (level.Settings.HasShadows())
                            population = WavePopulation.Baseline_Shadows;
                        else if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;

                        var settings = level.Tier == "E" ? WaveSettings.Baseline_VeryHard : WaveSettings.Baseline_Hard;

                        var (locked, _) = BuildChallenge_ApexAlarm(challengeRoot, population, settings);
                        beforePortal = locked;
                    }),

                    // Generator chain: prelude → generator-cell-in-side → keycard
                    (0.25, () =>
                    {
                        var (gen, _) = BuildChallenge_GeneratorCellInSide(challengeRoot);
                        var (locked, _) = BuildChallenge_KeycardInSide(gen);
                        beforePortal = locked;
                    }),

                    // Sensors + locked door
                    (0.15, () =>
                    {
                        AddSecuritySensors(challengeRoot);
                        var (locked, _) = BuildChallenge_KeycardInSide(challengeRoot);
                        beforePortal = locked;
                    }),
                });
                break;
            }
            #endregion

            default:
            {
                beforePortal = AddBranch_Forward(challengeRoot, 1).Last();
                break;
            }
        }

        // Forward extract candidate — placed near the start so the team has the
        // option of returning out the front door rather than the elevator. The
        // game falls back to the elevator zone when no forward candidate is
        // selected, giving us "both forward and entrance" extracts in practice.
        // AddForwardExtractStart(start);

        // The portal zone itself — Tech variant is a dead end (no further
        // outgoing connections), Mining has a forward expander.
        var (portal, portalZone) = AddZone_Forward(beforePortal);
        var portalConnections = level.Complex == Complex.Mining ? 1 : 0;
        portal = level.GenPortalGeomorph(portal, maxConnections: portalConnections);

        portalZone.Coverage = new CoverageMinMax { Min = 25, Max = 35 };

        // Inserting the MWP and walking through the portal warps the team into
        // Dimension1 (the alpha dimension).
        // NOTE: We don't need this the geo does it for us
        // portalZone.EventsOnPortalWarp.AddDimensionWarp(DimensionIndex.Dimension1, delay: 1.5);

        // Choose the static alpha dimension.
        var dimensionData = Generator.Pick(new List<Dimensions.DimensionData>
        {
            Dimensions.DimensionData.AlphaOne,
            Dimensions.DimensionData.AlphaThree_Top,
        })!;

        var dimension = new Dimension { Data = dimensionData };
        dimension.FindOrPersist();

        level.DimensionDatas.Add(new Levels.DimensionData
        {
            Dimension = DimensionIndex.Dimension1,
            Data = dimension,
        });

        // Spawn the alpha terminal at a random pre-defined candidate position
        // inside the dimension geomorph. IsWardenObjective=true tells the game
        // that this terminal is the SpecialTerminalCommand objective terminal,
        // so the chosen [SPECIAL_COMMAND] gets bound to it.
        var candidates = LevelCustomTerminals.GetCandidates(dimensionData.DimensionGeomorph);
        var (terminalPos, terminalRot) = Generator.Pick(candidates);

        // CustomTerminalSpawnManager.AddSpawnRequest(
        //     level.LevelLayoutData,
        //     new CustomTerminalSpawnRequest
        //     {
        //         Bulkhead = director.Bulkhead,
        //         DimensionIndex = DimensionIndex.Dimension1,
        //         LocalIndex = 0,
        //         GeomorphName = dimensionData.DimensionGeomorph,
        //         LocalPosition = terminalPos,
        //         LocalRotation = terminalRot,
        //         IsWardenObjective = true,
        //     });

        // Tell the warden-objective system that the terminal lives in
        // Dimension1, zone 0 (static dimensions are always a single zone).
        var dataLayer = level.GetObjectiveLayerData(director.Bulkhead);

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>
            {
                new()
                {
                    Dimension = DimensionIndex.Reality,
                    LocalIndex = 0,
                    Weights = ZonePlacementWeights.NotAtStart,
                },
            });
    }
}
