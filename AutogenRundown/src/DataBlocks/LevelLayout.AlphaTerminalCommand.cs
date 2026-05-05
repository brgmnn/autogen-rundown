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
                        var prelude = AddBranch_Forward(challengeRoot, 2).Last();
                        // var (locked, _) = BuildChallenge_KeycardInSide(prelude);
                        // beforePortal = locked;
                        beforePortal = prelude;
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

        // The portal zone itself — Tech variant is a dead end (no further
        // outgoing connections), Mining has a forward expander.
        var (portal, portalZone) = AddZone_Forward(beforePortal);
        portal = level.GenPortalGeomorph(portal, maxConnections: level.Complex == Complex.Mining ? 1 : 0);

        // portalZone.EventsOnPortalWarp.AddTurnOffAlarms(1.0);
        portalZone.EventsOnPortalWarp.AddMessage("activated now", 5.0);

        // Choose the static alpha dimension.
        var dimensionData = Generator.Pick(new List<Dimensions.DimensionData>
        {
            Dimensions.DimensionData.AlphaOne,
            Dimensions.DimensionData.AlphaThree_Top,
        })!;

        level.DimensionDatas.Add(new Levels.DimensionData
        {
            Dimension = DimensionIndex.Dimension1,
            Data = new Dimension { Data = dimensionData }.FindOrPersist(),
        });
    }
}
