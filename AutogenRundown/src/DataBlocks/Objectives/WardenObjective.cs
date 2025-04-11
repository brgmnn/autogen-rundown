using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.Reactor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    public enum DistributionStrategy
    {
        /// <summary>
        /// Randomly placed across all zones in random locations.
        /// </summary>
        Random,

        /// <summary>
        /// All items in a single zone (randomly)
        /// </summary>
        SingleZone,

        /// <summary>
        /// Evenly distributed across all zones
        /// </summary>
        EvenlyAcrossZones
    }

    public partial record class WardenObjective : DataBlock
    {
        /// <summary>
        /// Places objective items in the level as needed
        /// </summary>
        /// <param name="level"></param>
        /// <param name="bulkhead"></param>
        /// <param name="strategy"></param>
        public void DistributeObjectiveItems(
            Level level,
            Bulkhead bulkhead,
            DistributionStrategy strategy)
        {
            var data = level.GetObjectiveLayerData(bulkhead);
            var layout = level.GetLevelLayout(bulkhead);

            if (layout == null)
            {
                Plugin.Logger.LogError($"Missing level layout: {level.Tier}{level.Index}, Bulkhead={bulkhead}");
                return;
            }

            var nodes = level.Planner.GetZones(bulkhead, "find_items");

            switch (strategy)
            {
                ///
                /// We want to place all the items into a single zone within the area.
                ///
                case DistributionStrategy.SingleZone:
                {
                    var node = Generator.Pick(nodes);

                    data.ObjectiveData.ZonePlacementDatas.Add(
                        new List<ZonePlacementData>()
                        {
                            new()
                            {
                                LocalIndex = node.ZoneNumber,
                                Weights = ZonePlacementWeights.EvenlyDistributed
                            }
                        });

                    break;
                }

                ///
                /// We want to distribute the items evenly across all the zones. This will only
                /// appear different for GatherSmallItems objectives. The HSU and Terminal command
                /// objectives this will appear just the same as SingleZone strategy.
                ///
                case DistributionStrategy.EvenlyAcrossZones:
                {
                    var placement = new List<ZonePlacementData>();

                    foreach (var node in nodes)
                        placement.Add(new()
                        {
                            LocalIndex = node.ZoneNumber,
                            Weights = ZonePlacementWeights.EvenlyDistributed
                        });

                    data.ObjectiveData.ZonePlacementDatas.Add(placement);

                    break;
                }

                ///
                /// Place the elements evenly in some random subset of the placement zones.
                ///
                case DistributionStrategy.Random:
                {
                    var placement = new List<ZonePlacementData>();
                    var toDraw = Generator.Random.Next(1, nodes.Count);

                    for (var i = 0; i < toDraw; i++)
                    {
                        var node = Generator.Draw(nodes);

                        placement.Add(new()
                        {
                            LocalIndex = node.ZoneNumber,
                            Weights = ZonePlacementWeights.GenRandom()
                        });
                    }

                    data.ObjectiveData.ZonePlacementDatas.Add(placement);

                    break;
                }
            }
        }

        /// <summary>
        /// Returns a random casualty warning for lore strings
        /// </summary>
        /// <returns></returns>
        public static string IntelCasualtyWarning()
            => Generator.Pick(new List<string>
            {
                "Anticipate significant casualty rates.",
                "High death rate estimated.",
                "High resource loss expected.",
                "High resource depletion anticipated.",
                "Anticipated extreme casualty rate.",
                "High risk, substantial losses expected.",
                "Standard resource loss tolerance increased.",
                "Elevated fatality risk confirmed.",
                "Prisoner survival chances minimal.",
                "Survival chances minimal.",
                "Significant human cost projected.",
                "Substantial prisoner depletion likely.",
                "High prisoner attrition rate expected."
            })!;

        public static string GenLevelDescription(WardenObjectiveType type, WardenObjectiveItem item = WardenObjectiveItem.PersonnelId)
            => type switch
            {
                WardenObjectiveType.HsuFindSample => Generator.Pick(new List<string>
                    {
                        // Vanilla
                        "Security clearance for section <color=red>-REDACTED-</color> needed. DNA sample will suffice. Prisoners sent to locate Administrator in stasis unit.",

                        // Autogen
                        $"Sample retrieval authorized. Prisoners to locate HSU and extract biological material. Specimen integrity mandatory. {IntelCasualtyWarning()}",
                        "Hydro-Stasis Unit signal acquired. DNA sample required for higher-access clearance. Prisoners dispatched to recover.",
                        $"HSU location unknown. Prisoners will identify, access, and extract tissue sample from designated unit. {IntelCasualtyWarning()}",
                        "Biological match necessary for system authentication. Prisoners to retrieve tissue from dormant subject.",
                        "Administrator genetic profile located within inactive HSU. Prisoners to extract viable sample and transmit.",
                        $"Old blood. Cold cell. Prisoners must reach the unit and pull what's left. Sample must be intact. {IntelCasualtyWarning()}",
                        "Warden directive: isolate correct unit and perform sample collection. Cross-contamination will not be tolerated.",
                        "Link to security sector dependent on specimen match. Prisoners to obtain biological imprint from assigned HSU.",
                        "Subject dormant for years. Tissue degradation likely. Immediate extraction required. Prisoners to comply.",
                        $"Warden access protocol blocked. Genetic override needed. Prisoners to secure sample from flagged hydro-stasis unit. {IntelCasualtyWarning()}",
                    })!,

                WardenObjectiveType.ReactorStartup => Generator.Pick(new List<string>
                    {
                        // Vanilla
                        $"Bypass required in forward Security Zone. Prisoners sent to overload grid.",
                        $"<color=orange>WARNING</color>: Cold storage bulkhead filtration system offline. Grid reboot required. {IntelCasualtyWarning()}",
                        "Power out in neighboring sector. Reactor to be brought online at any cost of prisoners.",
                        $"Power grid manager reports system errors in primary reactor. Prisoners dispatched to execute quadrant generator reboot. {IntelCasualtyWarning()}",
                        $"Essential systems offline. Prisoners to perform system reboot in central quadrant. {IntelCasualtyWarning()}",
                        "Protocol requiring additional power. Prisoners dispatched to sector.",
                        "Insufficient power supply for PGD decryption. Prisoners sent to activate quadrant reactor."

                        // Autogen
                    })!,

                WardenObjectiveType.ReactorShutdown => Generator.Pick(new List<string>
                    {
                        // Vanilla
                        $"A pathway out of section D has been located. Prisoners spent to gain access by power-grid shutdown. {IntelCasualtyWarning()}",
                        $"Security protocol prohibiting sub-level exploration. Prisoners sent to install override executable in local reactor. {IntelCasualtyWarning()}",

                        // Autogen
                        $"Pathway to the reactor core identified. Prisoners dispatched for core isolation procedures. {IntelCasualtyWarning()}",
                        $"Restricted access to lower levels enforced. Prisoners tasked with initiating reactor shutdown sequence. {IntelCasualtyWarning()}",
                        $"Entrance to sector B uncovered. Prisoners assigned to disable reactor's failsafe mechanisms. {IntelCasualtyWarning()}",
                        $"Prohibited area entry point detected. Prisoners ordered to execute reactor deactivation. {IntelCasualtyWarning()}",
                        $"Route to reactor room confirmed. Prisoners deployed for reactor power-down operation. {IntelCasualtyWarning()}",
                        $"Access to sub-level reactor granted. Prisoners required to initiate core shutdown protocol. {IntelCasualtyWarning()}",
                        $"Reactor access pathway secured. Prisoners sent to perform emergency shutdown. {IntelCasualtyWarning()}",
                        $"Entrance to reactor chamber pinpointed. Prisoners instructed to disable reactor controls. {IntelCasualtyWarning()}",
                        $"Sub-level reactor breach path clear. Prisoners dispatched to terminate reactor function. {IntelCasualtyWarning()}",
                        $"Corridor to reactor sector E located. Prisoners commanded to execute reactor shutdown sequence. {IntelCasualtyWarning()}",
                    })!,

                WardenObjectiveType.ClearPath => Generator.Pick(new List<string>
                    {
                        // Vanilla
                        "Unknown hostile lifeform readings in adjacent quadrant. Expendable prisoners sent to survey threat severity.",

                        // Autogen
                        $"Obstruction in access route detected. Prisoners to eliminate all resistance and secure route for future activity. {IntelCasualtyWarning()}",
                        "Route must remain operational for asset transfer. Prisoners instructed to clear hostile presence and ensure path integrity.",
                        $"Prisoners dispatched to prepare corridor for Warden deployment. Resistance expected. Suppress all threats. {IntelCasualtyWarning()}",
                        "Expedition requires uninterrupted passage. Prisoners ordered to neutralize localized threat cluster.",
                        $"Staging area compromised by biohazards. Area to be purged. Prisoners will proceed through and report exit status. {IntelCasualtyWarning()}",
                        "Opening vector for deeper incursions. Path must be cleared. Prisoners will verify structural viability en route.",
                        $"This zone must be made passable. Future operations depend on a secured transit line. Eliminate all opposition. {IntelCasualtyWarning()}",
                        "Diversionary objective initiated. Prisoners deployed to create movement corridor. Maintain pace and push through resistance.",
                        "Transit corridor compromised. Pathway must be purged and left navigable. Prisoners deployed accordingly.",
                        $"Advance directive: ensure zone access for secondary team. Prisoners to proceed through resistance and reach extraction intact. {IntelCasualtyWarning()}",
                    })!,

                WardenObjectiveType.SpecialTerminalCommand => Generator.Pick(new List<string>
                    {
                        $"Terminal override required. Prisoners must execute input command to proceed. Environmental shift likely. {IntelCasualtyWarning()}",
                        $"Access protocol stalled. Terminal command authorization transferred to prisoners. Manual input necessary. {IntelCasualtyWarning()}",
                        "Command link compromised. Prisoners dispatched to restore function via terminal instruction. Anomalous activity expected.",
                        $"Warden directive updated. Prisoners to trigger system event by direct terminal execution. Prepare for instability. {IntelCasualtyWarning()}",
                        $"System requires localized authentication. Terminal input will result in cascading system behavior. {IntelCasualtyWarning()}",
                        "Root-level intrusion anticipated. Manual intervention necessary. Execute terminal command when ready.",
                        "Prisoners granted temporary command authority. Bypass security restrictions through targeted terminal input.",
                        $"Unexpected protocol stack detected. Initiating failsafe. Input command to stabilize sector. {IntelCasualtyWarning()}",
                        "System awaiting final instruction. Command to be entered by active field units. Resultant conditions unpredictable.",
                        $"Command chain interrupted. Prisoners to restore control via legacy terminal instruction set. Residual defenses active. {IntelCasualtyWarning()}",
                    })!,

                WardenObjectiveType.PowerCellDistribution => Generator.Pick(new List<string>
                    {
                        // Vanilla
                        $"Power interruptions compromising grid integrity. Prisoners sent to link cells to local grid through the [Z085] power cluster. Power grid stability imperative. {IntelCasualtyWarning()}",
                        $"Prisoners sent to install power cells to local grid. Motile biomass detected. {IntelCasualtyWarning()}",

                        // Autogen
                        $"Power cells located. Distribution protocol initiated. Prisoners instructed to deliver payloads to designated generator units. {IntelCasualtyWarning()}",
                        "Multiple zones offline. Prisoners will carry power modules to local generators. Movement penalties expected.",
                        $"Energy delivery required to reactivate core systems. Prisoners must transport power cells across operational zones. {IntelCasualtyWarning()}",
                        "Generator matrix offline. Prisoners to retrieve and distribute energy units manually. Heavy resistance expected.",
                        $"Displacement of high-density power objects authorized. Prisoners tasked with strategic deployment to grid nodes. {IntelCasualtyWarning()}",
                        "Energy routing compromised. Prisoners instructed to secure corridors and restore network via manual cell placement.",
                        $"Cargo units must reach active generators. Prisoners are expendable; cells are not. Proceed accordingly. {IntelCasualtyWarning()}",
                        "Unpowered sectors identified. Prisoners dispatched with containment-grade energy cells. Maintain route security.",
                        $"Generator grid inert. Initiating fallback procedure. Prisoners to act as mobile power couriers. {IntelCasualtyWarning()}",
                        "Critical energy transmission deferred to field agents. Prisoners responsible for direct insertion of cells into site power units.",
                    })!,

                WardenObjectiveType.TerminalUplink => Generator.Pick(new List<string>
                    {
                        $"Network link required to obtain control over quadrant maintenance systems. Prisoners sent to establish terminal uplink. {IntelCasualtyWarning()}",
                        $"Local uplink required for remote system control. Prisoners sent to initiate terminal link. {IntelCasualtyWarning()}",
                        "Isolated data node discovered. Prisoners tasked with securing terminal access for Warden's upload.",
                        $"Access node integrity failing. Prisoners deployed to stabilize link and transmit priority data. {IntelCasualtyWarning()}",
                        $"Warden requires access to obsolete mainframe. Prisoners ordered to perform uplink at secured terminal. {IntelCasualtyWarning()}",
                        "Unauthorized local systems online. Terminal lockdown initiated. Prisoners sent to override encryption and establish data link.",
                        $"Signal relay compromised. Prisoners to uplink through regional access terminal before link collapse. {IntelCasualtyWarning()}",
                        "Command uplink needed. Prisoners instructed to input credentials and hold position through transfer.",
                        "Warden-mandated upload scheduled. Manual terminal confirmation required. Prisoners dispatched to initiate uplink.",
                        $"High-priority data package ready. Prisoners assigned to enter codes at terminal and confirm transmission. {IntelCasualtyWarning()}",
                        $"Terminal located in volatile zone. Prisoners must maintain connection during critical data transfer window. {IntelCasualtyWarning()}",
                    })!,

                WardenObjectiveType.GatherSmallItems => item switch
                {
                    WardenObjectiveItem.Glp_1 => "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.",
                    WardenObjectiveItem.Glp_2 => "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.",
                    _ => "Prisoners to collect items from storage facility. High asset fatality rate expected."
                },

                WardenObjectiveType.Survival => Generator.Pick(new List<string>
                {
                    $"Prisoners expended for diversion to clear adjacent sectors. Local power grid unstable. {IntelCasualtyWarning()}",
                    "Prisoners will act as decoy for undisclosed parallel objective. Surviving prisoners will return for extraction once undisclosed objective has been completed.",

                    $"Biomass detected in critical zone. Prisoners will hold ground until extraction window becomes viable. {IntelCasualtyWarning()}",
                    $"Stabilization protocol in progress. Prisoners must remain in designated zone until system confirms lock. {IntelCasualtyWarning()}",
                    "Prisoners required to maintain presence in conflict zone. Extraction contingent on external task completion.",
                    "Warden-ordered delay in retrieval. Prisoners instructed to endure hostile conditions until recall is authorized.",
                    $"Hold position directive issued. Prisoners will await extraction signal amidst escalating environmental threats. {IntelCasualtyWarning()}",
                    $"Decoy initiative active. Enemy redirection underway. Prisoners expected to endure contact duration. {IntelCasualtyWarning()}",
                    "Prisoners deployed to obstruct local adversaries. Extraction not available until uplink confirms clearance.",
                    $"Area containment required. Surviving prisoners will proceed to exit zone upon conclusion of external protocol. {IntelCasualtyWarning()}",
                    "Hostile suppression requested. Prisoners tasked with absorbing attention while secondary teams mobilize.",
                    "Temporal window for safe egress pending. Prisoners instructed to engage threat deterrence protocol until Warden grants withdrawal.",
                })!,

                WardenObjectiveType.TimedTerminalSequence => Generator.Pick(new List<string>
                {
                    "System handshake protocol failed. Manual verification required at linked terminals. Prisoners dispatched to execute timed input sequence.",
                    $"Redundant encryption detected. Terminal relay must be verified through simultaneous prisoner input. {IntelCasualtyWarning()}",
                    "Timed sequence required for data authentication. Two prisoner units to coordinate across sector. Expect hostile interference.",
                    $"Access to archived system blocked. Manual override initiated. Terminal sequence must be completed within time limit. {IntelCasualtyWarning()}",
                    "Sequence protocol: <color=orange>INIT</color> – <color=orange>VERIFY</color> – <color=orange>CONFIRM</color>. Prisoners instructed to synchronize terminal access or risk hostile resurgence.",
                    "Warden connection unstable. Input terminals require prisoner verification before data loss becomes permanent.",
                    $"Countdown-based protocol initiated. Expect interference. Prisoners must locate and confirm secondary terminal ID before uplink failure. {IntelCasualtyWarning()}",
                    $"Timed command relay engaged. Error tolerance: zero. Terminal pair must be activated in correct order. Consequences severe. {IntelCasualtyWarning()}",
                    "System requires human presence to execute critical data bridge. Failure to complete in time will reset sequence and escalate threat response.",
                    $"Terminal sync required. Prisoners ordered to perform chained input across network before countdown expires. {IntelCasualtyWarning()}",
                })!,

                // WardenObjectiveType.RetrieveBigItems => expr,
                // WardenObjectiveType.CentralGeneratorCluster => expr,
                // WardenObjectiveType.HsuActivateSmall => expr,
                // WardenObjectiveType.GatherTerminal => expr,
                // WardenObjectiveType.CorruptedTerminalUplink => expr,
                // WardenObjectiveType.Empty => expr,

                _ => "<color=red>-INTEL REDACTED-</color>"
            };

        public static List<(WardenObjectiveItem, string, string)> BuildSmallPickupPack(string tier)
            => new()
            {
                // Currently disabled items.
                //  * MemoryStick: The model is quite small and hard to see especially in boxes.
                //    Removed until some other pickup spot can be used

                //(WardenObjectiveItem.MemoryStick, "Memory stick", "Gather [COUNT_REQUIRED] Memory sticks and return the memory sticks for analysis."),

                (WardenObjectiveItem.PersonnelId, "Personnel ID", "Gather [COUNT_REQUIRED] Personnel IDs and return the data to be processed."),
                (WardenObjectiveItem.PartialDecoder, "Partial Decoder", "Gather [COUNT_REQUIRED] Partial Decoders and return the data to be processed."),
                (WardenObjectiveItem.Harddrive, "Hard drive", "Gather [COUNT_REQUIRED] Hard Drives and return the drives for data archival."),
                (WardenObjectiveItem.Glp_1, "GLP-1 canister", "Gather [COUNT_REQUIRED] GLP-1 canisters and return the canisters for genome sequencing."),
                (WardenObjectiveItem.Glp_2, "GLP-2 canister", "Gather [COUNT_REQUIRED] GLP-2 canisters and return the canisters for genome sequencing."),
                (WardenObjectiveItem.Osip, "OSIP vial", "Gather [COUNT_REQUIRED] OSIP vials and return the vials for chemical analysis."),
                (WardenObjectiveItem.PlantSample, "Plant sample", "Gather [COUNT_REQUIRED] Plant samples and return the samples for analysis."),
                (WardenObjectiveItem.DataCube, "Data cube", "Gather [COUNT_REQUIRED] Data cubes and return the cubes for data extraction."),
                (WardenObjectiveItem.DataCubeBackup, "Backup data cube", "Gather [COUNT_REQUIRED] Backup Data cubes and return the cubes for data archival."),
                (WardenObjectiveItem.DataCubeTampered, "Tampered data cube", "Gather [COUNT_REQUIRED] Data cubes and return the cubes for inspection.")
            };

        /// <summary>
        /// Calculates what multiplier should be used to give an exit scan time of "seconds"
        /// seconds. It seems the default exit time is 20 seconds.
        /// </summary>
        /// <param name="seconds">How many seconds the exit scan should take</param>
        /// <returns>The ChainedPuzzleAtExitScanSpeedMultiplier value to set</returns>
        public static double CalculateExitScanSpeedMultiplier(double seconds) => 20.0 / seconds;

        /// <summary>
        /// Randomly picks an exit time speed between min and max seconds inclusive. Returns a
        /// double that should be used to set ChainedPuzzleAtExitScanSpeedMultiplier
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double GenExitScanTime(int min, int max)
            => CalculateExitScanSpeedMultiplier(Generator.Random.Next(min, max + 1));

        /// <summary>
        /// Some settings from the objective are needed for level generation. However plenty of
        /// layout information is needed for the objective. Objective building is split into two
        /// phases. PreBuild() is called first to generate the objective and then Build() is called
        /// after level layout has been done.
        /// </summary>
        /// <param name="director"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static WardenObjective PreBuild(BuildDirector director, Level level)
        {
            var objective = new WardenObjective
            {
                Type = director.Objective,
            };

            switch (objective.Type)
            {
                case WardenObjectiveType.ReactorStartup:
                    {
                        objective.ReactorStartupGetCodes = true;

                        var waveCount = director.Tier switch
                        {
                            "A" => Generator.Random.Next(3, 4),
                            "B" => Generator.Random.Next(4, 6),
                            "C" => Generator.Random.Next(5, 7),
                            "D" => Generator.Random.Next(8, 10),
                            "E" => Generator.Random.Next(8, 12),
                            _ => 1
                        };

                        // Initialize the reactor Waves with the correct number of waves, these
                        // will be updated as we go.
                        for (var i = 0; i < waveCount; ++i)
                            objective.ReactorWaves.Add(new ReactorWave());

                        break;
                    }

                case WardenObjectiveType.ReactorShutdown:
                {
                    objective.PreBuild_ReactorShutdown(director, level);
                    break;
                }

                case WardenObjectiveType.RetrieveBigItems:
                    {
                        var choices = new List<(double, WardenObjectiveItem)>
                        {
                            (1.0, WardenObjectiveItem.DataSphere),
                            (1.0, WardenObjectiveItem.CargoCrate),
                            (1.0, WardenObjectiveItem.CargoCrateHighSecurity),
                            (1.0, WardenObjectiveItem.CryoCase),
                        };

                        // These would be main objective items only
                        if (director.Bulkhead.HasFlag(Bulkhead.Main))
                        {
                            //choices.Add((1.0, WardenObjectiveItem.NeonateHsu));
                            choices.Add((1.0, WardenObjectiveItem.MatterWaveProjector));
                        }

                        var item = Generator.Select(choices);

                        /**
                         * Some interesting options here for how many items we should spawn. We
                         * want to reduce the number of items for non Main objectives and also
                         * want to increase the number of items for deeper levels.
                         * */
                        var count = (item, director.Tier, director.Bulkhead & Bulkhead.Objectives) switch
                        {
                            (WardenObjectiveItem.CryoCase, "A", Bulkhead.Main) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CryoCase, "B", Bulkhead.Main) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CryoCase, "C", Bulkhead.Main) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CryoCase, "D", Bulkhead.Main) => Generator.Random.Next(2, 3),
                            (WardenObjectiveItem.CryoCase, "E", Bulkhead.Main) => Generator.Random.Next(2, 4),
                            (WardenObjectiveItem.CryoCase, "D", _) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CryoCase, "E", _) => 2,

                            (WardenObjectiveItem.CargoCrateHighSecurity, "D", Bulkhead.Main) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CargoCrateHighSecurity, "E", Bulkhead.Main) => 2,

                            (_, _, _) => 1

                        };

                        for (var i = 0; i < count; ++i)
                            objective.RetrieveItems.Add(item);

                        break;
                    }

                case WardenObjectiveType.SpecialTerminalCommand:
                {
                    objective.PreBuild_SpecialTerminalCommand(director, level);
                    break;
                }

                case WardenObjectiveType.PowerCellDistribution:
                {
                    objective.PowerCellsToDistribute = director.Tier switch
                    {
                        "A" => Generator.Random.Next(1, 2),
                        "B" => Generator.Random.Next(1, 2),
                        "C" => Generator.Random.Next(2, 3),
                        "D" => Generator.Random.Next(3, 4),
                        "E" => Generator.Random.Next(3, 5),
                        _ => 2
                    };

                    break;
                }

                case WardenObjectiveType.TerminalUplink:
                {
                    objective.PreBuild_TerminalUplink(director, level);
                    break;
                }

                case WardenObjectiveType.HsuActivateSmall:
                    objective.PreBuild_HsuActivateSmall(director, level);
                    break;

                case WardenObjectiveType.TimedTerminalSequence:
                    objective.PreBuild_TimedTerminalSequence(director, level);
                    break;
            }

            return objective;
        }

        public static (ObjectiveLayerData, LevelLayout) GetObjectiveLayerAndLayout(BuildDirector director, Level level)
        {
            var dataLayer = level.GetObjectiveLayerData(director.Bulkhead);

            if (dataLayer is null)
            {
                Plugin.Logger.LogError($"WardenObjective.Build(): Missing level data layer: " +
                    $"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead}");
                throw new Exception("Missing level data layer");
            }

            var layout = level.GetLevelLayout(director.Bulkhead);

            if (layout is null)
            {
                Plugin.Logger.LogError($"WardenObjective.Build(): Missing level layout: " +
                    $"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead}");
                throw new Exception("Missing level layout");
            }

            return (dataLayer, layout);
        }

        /// <summary>
        /// This is called _after_ the level layout has been built
        /// </summary>
        /// <param name="director"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public void Build(BuildDirector director, Level level)
        {
            var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

            GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
            GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
            GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition";

            // Set the level description if there's no description provided already
            if (director.Bulkhead.HasFlag(Bulkhead.Main) && level.Description == 0)
                level.Description = new Text(GenLevelDescription(director.Objective)).PersistentId;

            // Set the exit scan speed multiplier. Generally we want easier levels to be faster.
            // For some objectives this will be overridden.
            ChainedPuzzleAtExitScanSpeedMultiplier = director.Tier switch
            {
                "A" => GenExitScanTime(20, 30),
                "B" => GenExitScanTime(30, 45),
                "C" => GenExitScanTime(45, 80),
                "D" => GenExitScanTime(90, 120),
                "E" => GenExitScanTime(100, 140),
                _ => 1.0,
            };

            switch (director.Objective)
            {
                /**
                 * Collect the HSU from within a storage zone
                 */
                case WardenObjectiveType.HsuFindSample:
                {
                    Build_HsuFindSample(director, level);
                    break;
                }

                /**
                 * Find and start up a reactor, fighting waves and optionally getting codes from zones.
                 *
                 * Note that when spawning waves, waves with capped total points should be used to
                 * ensure the waves end when the team has finished fighting all of the enemies.
                 * */
                case WardenObjectiveType.ReactorStartup:
                {
                    Build_ReactorStartup(director, level);
                    break;
                }

                /**
                 * Reactor shutdown will result in the lights being off for the remainder of the
                 * level. Factor that as a difficulty modifier.
                 * */
                case WardenObjectiveType.ReactorShutdown:
                {
                    Build_ReactorShutdown(director, level);
                    break;
                }

                /**
                 * Gather small items from around the level. This is a fairly simple objective
                 * that can be completed in a variety of ways.
                 * */
                case WardenObjectiveType.GatherSmallItems:
                    {
                        var (itemId, name, description) = Generator.Pick(BuildSmallPickupPack(level.Tier));
                        var strategy = Generator.Pick(new List<DistributionStrategy>
                        {
                            DistributionStrategy.Random,
                            DistributionStrategy.SingleZone,
                            DistributionStrategy.EvenlyAcrossZones
                        });

                        MainObjective = description;
                        FindLocationInfo = $"Look for {name}s in the complex";
                        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";

                        GatherRequiredCount = level.Tier switch
                        {
                            "A" => Generator.Random.Next(4, 8),
                            "B" => Generator.Random.Next(6, 10),
                            "C" => Generator.Random.Next(7, 12),
                            "D" => Generator.Random.Next(8, 13),
                            "E" => Generator.Random.Next(9, 16),
                            _ => 1,
                        };

                        GatherItemId = (uint)itemId;
                        GatherSpawnCount = Generator.Random.Next(
                            GatherRequiredCount,
                            GatherRequiredCount + 6);

                        DistributeObjectiveItems(level, director.Bulkhead, strategy);

                        var zoneSpawns = dataLayer.ObjectiveData.ZonePlacementDatas[0].Count;

                        GatherMaxPerZone = GatherSpawnCount / zoneSpawns + GatherSpawnCount % zoneSpawns;

                        break;
                    }

                /**
                 * Fairly straight forward objective, get to the end zone. Some additional enemies
                 * at the end make this a more interesting experience.
                 *
                 * This objective can only be for Main given it ends the level on completion
                 * */
                case WardenObjectiveType.ClearPath:
                {
                    Build_ClearPath(director, level);
                    break;
                }

                /**
                 * TODO: It would be nice to add special commands other than just lights off that do other modifiers.
                 *       Such as fog, error alarm, etc.
                 *
                 *       Ideas:
                 *          1. Spawn boss
                 *          2. Flood with fog
                 *              a. Flood with fog slowly
                 *              b. Instantly flood
                 *          3. Trigger error alarm
                 *          4. Trigger unit wave
                 */
                case WardenObjectiveType.SpecialTerminalCommand:
                {
                    Build_SpecialTerminalCommand(director, level);
                    break;
                }

                /**
                 * Retrieve an item from within the complex.
                 * */
                case WardenObjectiveType.RetrieveBigItems:
                {
                    Build_RetrieveBigItems(director, level);
                    break;
                }

                /**
                 * Drop in with power cells and distribute them to generators in various zones.
                 *
                 * The power cells set with PowerCellsToDistribute are dropped in with you
                 * automatically.
                 * */
                case WardenObjectiveType.PowerCellDistribution:
                {
                    Build_PowerCellDistribution(director, level);
                    break;
                }

                /**
                 * Sets up a terminal uplink objective. Randomizes the number of terminals, number
                 * of uplink words, etc.
                 */
                case WardenObjectiveType.TerminalUplink:
                {
                    Build_TerminalUplink(director, level);
                    break;
                }

                /**
                 * Central generator cluster.
                 */
                case WardenObjectiveType.CentralGeneratorCluster:
                {
                    Build_CentralGeneratorCluster(director, level);
                    break;
                }

                case WardenObjectiveType.HsuActivateSmall:
                {
                    Build_HsuActivateSmall(director, level);
                    break;
                }

                /**
                 * Survival missions
                 */
                case WardenObjectiveType.Survival:
                {
                    Build_Survival(director, level);
                    break;
                }

                /**
                 * Timed terminal missions
                 */
                case WardenObjectiveType.TimedTerminalSequence:
                    Build_TimedTerminalSequence(director, level);
                    break;
            }

            dataLayer.ObjectiveData.DataBlockId = PersistentId;
        }

        /// <summary>
        /// Run after everything has been built
        /// </summary>
        /// <param name="director"></param>
        /// <param name="level"></param>
        public void PostBuild(BuildDirector director, Level level)
        {
            switch (director.Objective)
            {
                case WardenObjectiveType.ReactorShutdown:
                    PostBuild_ReactorShutdown(director, level);
                    break;

                case WardenObjectiveType.Survival:
                    PostBuild_Survival(director, level);
                    break;
            }
        }

        #region Internal Fields
        [JsonIgnore]
        public bool ReactorStartupGetCodes { get; set; } = false;

        [JsonIgnore]
        public int ReactorStartup_FetchWaves { get; set; } = 0;
        #endregion

        #region General fields
        /// <summary>
        /// What type of objective this is.
        /// </summary>
        public WardenObjectiveType Type { get; set; }

        #region Information and display strings
        public string MainObjective { get; set; } = "";
        public string FindLocationInfo { get; set; } = "";
        public string FindLocationInfoHelp { get; set; } = "Access more data in the terminal maintenance system";
        public string GoToZone { get; set; } = "";
        public string GoToZoneHelp { get; set; } = "";
        public string InZoneFindItem { get; set; } = "";
        public string InZoneFindItemHelp { get; set; } = "";
        public string SolveItem { get; set; } = "";
        public string SolveItemHelp { get; set; } = "";
        public string GoToWinCondition_Elevator { get; set; } = "";
        public string GoToWinConditionHelp_Elevator { get; set; } = "";
        public string GoToWinCondition_CustomGeo { get; set; } = "";
        public string GoToWinConditionHelp_CustomGeo { get; set; } = "";
        public string GoToWinCondition_ToMainLayer { get; set; } = "";
        public string GoToWinConditionHelp_ToMainLayer { get; set; } = "";
        public string WaveOnElevatorWardenIntel { get; set; } = "";
        public string Survival_TimerTitle { get; set; } = "";
        public string Survival_TimerToActivateTitle { get; set; } = "";
        public string GatherTerminal_CommandHelp { get; set; } = "";
        public string GatherTerminal_DownloadingText { get; set; } = "";
        public string GatherTerminal_DownloadCompleteText { get; set; } = "";
        public double ShowHelpDelay { get; set; } = 180.0;
        #endregion
        #endregion

        #region Events
        public List<WardenObjectiveEvent> EventsOnActivate { get; set; } = new List<WardenObjectiveEvent>();

        public List<WardenObjectiveEvent> EventsOnElevatorLand { get; set; } = new List<WardenObjectiveEvent>();

        public List<WardenObjectiveEvent> EventsOnGotoWin { get; set; } = new List<WardenObjectiveEvent>();

        /// <summary>
        /// This triggers waves that spawn as soon as you land
        /// </summary>
        public List<GenericWave> WavesOnElevatorLand { get; set; } = new();

        /// <summary>
        /// Waves to spawn on returning to win. This seems to only be for the main objective.
        /// </summary>
        public List<GenericWave> WavesOnGotoWin { get; set; } = new();

        /// <summary>
        /// Enemy waves to spawn on activating the objective.
        /// </summary>
        public List<GenericWave> WavesOnActivate { get; set; } = new();
        #endregion

        #region === MODs: Inas07/ExtraObjectiveSetup
        /// <summary>
        /// Any layout definitions we need for this objective
        /// </summary>
        [JsonIgnore]
        public LayoutDefinitions? LayoutDefinitions { get; set; } = null;
        #endregion

        #region Type=?: Chained puzzles
        [JsonIgnore]
        public ChainedPuzzle StartPuzzle { get; set; } = ChainedPuzzle.None;

        public uint ChainedPuzzleToActive
        {
            get => StartPuzzle.PersistentId;
            private set { }
        }

        [JsonIgnore]
        public ChainedPuzzle MidPuzzle { get; set; } = ChainedPuzzle.None;

        public uint ChainedPuzzleMidObjective
        {
            get => MidPuzzle.PersistentId;
            private set { }
        }
        #endregion

        #region Type=0: Find HSU sample
        public bool ActivateHSU_BringItemInElevator { get; set; } = true;

        public Items.Item ActivateHSU_ItemFromStart { get; set; } = Items.Item.None;

        public Items.Item ActivateHSU_ItemAfterActivation { get; set; } = Items.Item.None;

        public bool ActivateHSU_MarkItemInElevatorAsWardenObjective { get; set; } = false;

        public bool ActivateHSU_StopEnemyWavesOnActivation { get; set; } = false;

        public bool ActivateHSU_ObjectiveCompleteAfterInsertion { get; set; } = false;

        public bool ActivateHSU_RequireItemAfterActivationInExitScan { get; set; } = false;

        public List<WardenObjectiveEvent> ActivateHSU_Events { get; set; } = new();
        #endregion

        #region Type=1 & 2: Reactor startup/shutdown
        public List<ReactorWave> ReactorWaves { get; set; } = new();
        #endregion

        #region Type=3: Gather small items
        [JsonProperty("Gather_RequiredCount")]
        public int GatherRequiredCount { get; set; } = -1;

        [JsonProperty("Gather_ItemId")]
        public uint GatherItemId { get; set; } = 0;

        [JsonProperty("Gather_SpawnCount")]
        public int GatherSpawnCount { get; set; } = 0;

        [JsonProperty("Gather_MaxPerZone")]
        public int GatherMaxPerZone { get; set; } = 0;
        #endregion

        #region Type=4: Clear a path
        #endregion

        #region Type=5: Special terminal command
        /// <summary>
        /// Used internally to determine what we should do with the special terminal command
        /// </summary>
        [JsonIgnore]
        public SpecialCommand SpecialTerminalCommand_Type { get; set; } = SpecialCommand.None;

        /// <summary>
        /// The Special terminal command players have to enter
        /// </summary>
        public string SpecialTerminalCommand { get; set; } = "";

        /// <summary>
        /// Description displayed in the terminal COMMANDs listing
        /// </summary>
        public string SpecialTerminalCommandDesc { get; set; } = "";
        #endregion

        #region Type=6: Retrieve big items
        /// <summary>
        /// Specifies which items are to be retrieved for this objective
        /// </summary>
        [JsonProperty("Retrieve_Items")]
        public List<WardenObjectiveItem> RetrieveItems { get; set; } = new();
        #endregion

        #region Type=7: Power cell distribution
        #endregion

        #region Type=8: Uplink terminal
        /// <summary>
        ///
        /// </summary>
        public int Uplink_NumberOfVerificationRounds { get; set; } = 0;

        /// <summary>
        ///
        /// </summary>
        public int Uplink_NumberOfTerminals { get; set; } = 1;

        /// <summary>
        ///
        /// </summary>
        public SurvivalWaveSpawnType Uplink_WaveSpawnType { get; set; } = SurvivalWaveSpawnType.InSuppliedCourseNodeZone;
        #endregion

        #region Type=9: Central generator cluster --BROKEN--
        public int PowerCellsToDistribute { get; set; } = 0;

        public int CentralPowerGenClustser_NumberOfGenerators { get; set; } = 0;

        public int CentralPowerGenClustser_NumberOfPowerCells { get; set; } = 4;

        public JArray CentralPowerGenClustser_FogDataSteps = new JArray();
        #endregion

        #region Type=10: HsuActivateSmall (Bring Neonate/Datasphere to depressurizer)
        #endregion

        #region Type=11: Survival
        public double Survival_TimeToActivate { get; set; } = 0.0;

        public double Survival_TimeToSurvive { get; set; } = 0.0;

        /// <summary>
        /// Used exclusively to modify the terminal warden objective event list
        /// </summary>
        [JsonIgnore]
        public List<WardenObjectiveEvent> SecurityControlEvents { get; set; } = new List<WardenObjectiveEvent>();

        [JsonIgnore] public int SecurityControlEventLoopIndex { get; set; } = 0;
        #endregion

        #region Type=12: Gather Terminal
        #endregion

        #region Type=13: Corrupted Terminal Uplink
        #endregion

        #region Type=15: Timed terminal sequence
        public int TimedTerminalSequence_NumberOfRounds { get; set; } = 3;

        public int TimedTerminalSequence_NumberOfTerminals = 1;

        public double TimedTerminalSequence_TimePerRound = 90.0;

        public double TimedTerminalSequence_TimeForConfirmation = 10.0;

        public bool TimedTerminalSequence_UseFilterForSourceTerminalPicking = false;

        public string TimedTerminalSequence_SourceTerminalWorldEventObjectFilter = "";

        public List<List<WardenObjectiveEvent>> TimedTerminalSequence_EventsOnSequenceStart = new();

        public List<List<WardenObjectiveEvent>> TimedTerminalSequence_EventsOnSequenceDone = new();

        public List<List<WardenObjectiveEvent>> TimedTerminalSequence_EventsOnSequenceFail = new();
        #endregion

        #region Expedition exit
        /// <summary>
        /// What exit scan to use at the exit
        /// </summary>
        public uint ChainedPuzzleAtExit { get; set; } = ChainedPuzzle.ExitAlarm.PersistentId;

        /// <summary>
        /// Multiplier to use for the exit scan speed. This is calculated from the exit scan time
        /// which by default is 20 seconds
        /// </summary>
        public double ChainedPuzzleAtExitScanSpeedMultiplier { get; set; } = 1.0;
        #endregion

        #region Fields not yet implemented
        public int WardenObjectiveSpecialUpdateType = 0;
        public int GenericItemFromStart = 0;
        public bool DoNotMarkPickupItemsAsWardenObjectives = false;
        public bool OverrideNoRequiredItemsForExit = false;
        public int FogTransitionDataOnElevatorLand = 0;
        public double FogTransitionDurationOnElevatorLand = 0.0;
        public bool OnActivateOnSolveItem = false;
        public bool StopAllWavesBeforeGotoWin = false;
        public int WaveOnGotoWinTrigger = 0;
        public int EventsOnGotoWinTrigger = 0;
        public int FogTransitionDataOnGotoWin = 0;
        public double FogTransitionDurationOnGotoWin = 0.0;
        public bool LightsOnFromBeginning = false;
        public bool LightsOnDuringIntro = false;
        public bool LightsOnWhenStartupComplete = false;
        public bool DoNotSolveObjectiveOnReactorComplete = false;
        public JArray PostCommandOutput = new JArray();
        public int SpecialCommandRule = 0;
        public int GatherTerminal_SpawnCount = 0;
        public int GatherTerminal_RequiredCount = 0;
        public string GatherTerminal_Command = "";
        public double GatherTerminal_DownloadTime = -1.0;
        #endregion

        #region Unused fields
        public string Header { get; set; } = "";
        #endregion
    }
}
