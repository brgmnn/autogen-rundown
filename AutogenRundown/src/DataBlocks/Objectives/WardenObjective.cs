using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives.CentralGeneratorCluster;
using AutogenRundown.DataBlocks.Objectives.Reactor;
using AutogenRundown.DataBlocks.Zones;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Objectives;

public partial record WardenObjective : DataBlock<WardenObjective>
{
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
                "Sample retrieval authorized. Prisoners to locate HSU and extract biological material. Specimen integrity mandatory.",
                "Hydro-Stasis Unit signal acquired. DNA sample required for higher-access clearance. Prisoners dispatched to recover.",
                "HSU location unknown. Prisoners will identify, access, and extract tissue sample from designated unit.",
                "Biological match necessary for system authentication. Prisoners to retrieve tissue from dormant subject.",
                "Administrator genetic profile located within inactive HSU. Prisoners to extract viable sample and transmit.",
                "Old blood. Cold cell. Prisoners must reach the unit and pull what's left. Sample must be intact.",
                "Warden directive: isolate correct unit and perform sample collection. Cross-contamination will not be tolerated.",
                "Link to security sector dependent on specimen match. Prisoners to obtain biological imprint from assigned HSU.",
                "Subject dormant for years. Tissue degradation likely. Immediate extraction required. Prisoners to comply.",
                "Warden access protocol blocked. Genetic override needed. Prisoners to secure sample from flagged hydro-stasis unit.",
            })!,

            WardenObjectiveType.ReactorStartup => Generator.Pick(new List<string>
            {
                // Vanilla
                "Bypass required in forward Security Zone. Prisoners sent to overload grid.",
                "<color=orange>WARNING</color>: Cold storage bulkhead filtration system offline. Grid reboot required.",
                "Power out in neighboring sector. Reactor to be brought online at any cost of prisoners.",
                "Power grid manager reports system errors in primary reactor. Prisoners dispatched to execute quadrant generator reboot.",
                "Essential systems offline. Prisoners to perform system reboot in central quadrant.",
                "Protocol requiring additional power. Prisoners dispatched to sector.",
                "Insufficient power supply for PGD decryption. Prisoners sent to activate quadrant reactor."

                // Autogen
            })!,

            WardenObjectiveType.ReactorShutdown => Generator.Pick(new List<string>
            {
                // Vanilla
                "A pathway out of section D has been located. Prisoners spent to gain access by power-grid shutdown.",
                "Security protocol prohibiting sub-level exploration. Prisoners sent to install override executable in local reactor.",

                // Autogen
                "Pathway to the reactor core identified. Prisoners dispatched for core isolation procedures.",
                "Restricted access to lower levels enforced. Prisoners tasked with initiating reactor shutdown sequence.",
                "Entrance to sector B uncovered. Prisoners assigned to disable reactor's failsafe mechanisms.",
                "Prohibited area entry point detected. Prisoners ordered to execute reactor deactivation.",
                "Route to reactor room confirmed. Prisoners deployed for reactor power-down operation.",
                "Access to sub-level reactor granted. Prisoners required to initiate core shutdown protocol.",
                "Reactor access pathway secured. Prisoners sent to perform emergency shutdown.",
                "Entrance to reactor chamber pinpointed. Prisoners instructed to disable reactor controls.",
                "Sub-level reactor breach path clear. Prisoners dispatched to terminate reactor function.",
                "Corridor to reactor sector E located. Prisoners commanded to execute reactor shutdown sequence.",
            })!,

            WardenObjectiveType.ClearPath => Generator.Pick(new List<string>
            {
                // Vanilla
                "Unknown hostile lifeform readings in adjacent quadrant. Expendable prisoners sent to survey threat severity.",

                // Autogen
                "Obstruction in access route detected. Prisoners to eliminate all resistance and secure route for future activity.",
                "Route must remain operational for asset transfer. Prisoners instructed to clear hostile presence and ensure path integrity.",
                "Prisoners dispatched to prepare corridor for Warden deployment. Resistance expected. Suppress all threats.",
                "Expedition requires uninterrupted passage. Prisoners ordered to neutralize localized threat cluster.",
                "Staging area compromised by biohazards. Area to be purged. Prisoners will proceed through and report exit status.",
                "Opening vector for deeper incursions. Path must be cleared. Prisoners will verify structural viability en route.",
                "This zone must be made passable. Future operations depend on a secured transit line. Eliminate all opposition.",
                "Diversionary objective initiated. Prisoners deployed to create movement corridor. Maintain pace and push through resistance.",
                "Transit corridor compromised. Pathway must be purged and left navigable. Prisoners deployed accordingly.",
                "Advance directive: ensure zone access for secondary team. Prisoners to proceed through resistance and reach extraction intact.",
            })!,

            WardenObjectiveType.SpecialTerminalCommand => Generator.Pick(new List<string>
            {
                "Terminal override required. Prisoners must execute input command to proceed. Environmental shift likely.",
                "Access protocol stalled. Terminal command authorization transferred to prisoners. Manual input necessary.",
                "Command link compromised. Prisoners dispatched to restore function via terminal instruction. Anomalous activity expected.",
                "Warden directive updated. Prisoners to trigger system event by direct terminal execution. Prepare for instability.",
                "System requires localized authentication. Terminal input will result in cascading system behavior.",
                "Root-level intrusion anticipated. Manual intervention necessary. Execute terminal command when ready.",
                "Prisoners granted temporary command authority. Bypass security restrictions through targeted terminal input.",
                "Unexpected protocol stack detected. Initiating failsafe. Input command to stabilize sector.",
                "System awaiting final instruction. Command to be entered by active field units. Resultant conditions unpredictable.",
                "Command chain interrupted. Prisoners to restore control via legacy terminal instruction set. Residual defenses active.",
            })!,

            WardenObjectiveType.PowerCellDistribution => Generator.Pick(new List<string>
            {
                // Vanilla
                "Power interruptions compromising grid integrity. Prisoners sent to link cells to local grid through the [Z085] power cluster. Power grid stability imperative.",
                "Prisoners sent to install power cells to local grid. Motile biomass detected.",

                // Autogen
                "Power cells located. Distribution protocol initiated. Prisoners instructed to deliver payloads to designated generator units.",
                "Multiple zones offline. Prisoners will carry power modules to local generators. Movement penalties expected.",
                "Energy delivery required to reactivate core systems. Prisoners must transport power cells across operational zones.",
                "Generator matrix offline. Prisoners to retrieve and distribute energy units manually. Heavy resistance expected.",
                "Displacement of high-density power objects authorized. Prisoners tasked with strategic deployment to grid nodes.",
                "Energy routing compromised. Prisoners instructed to secure corridors and restore network via manual cell placement.",
                "Cargo units must reach active generators. Prisoners are expendable; cells are not. Proceed accordingly.",
                "Unpowered sectors identified. Prisoners dispatched with containment-grade energy cells. Maintain route security.",
                "Generator grid inert. Initiating fallback procedure. Prisoners to act as mobile power couriers.",
                "Critical energy transmission deferred to field agents. Prisoners responsible for direct insertion of cells into site power units.",
            })!,

            WardenObjectiveType.TerminalUplink => Generator.Pick(new List<string>
            {
                "Network link required to obtain control over quadrant maintenance systems. Prisoners sent to establish terminal uplink.",
                "Local uplink required for remote system control. Prisoners sent to initiate terminal link.",
                "Isolated data node discovered. Prisoners tasked with securing terminal access for Warden's upload.",
                "Access node integrity failing. Prisoners deployed to stabilize link and transmit priority data.",
                "Warden requires access to obsolete mainframe. Prisoners ordered to perform uplink at secured terminal.",
                "Unauthorized local systems online. Terminal lockdown initiated. Prisoners sent to override encryption and establish data link.",
                "Signal relay compromised. Prisoners to uplink through regional access terminal before link collapse.",
                "Command uplink needed. Prisoners instructed to input credentials and hold position through transfer.",
                "Warden-mandated upload scheduled. Manual terminal confirmation required. Prisoners dispatched to initiate uplink.",
                "High-priority data package ready. Prisoners assigned to enter codes at terminal and confirm transmission.",
                "Terminal located in volatile zone. Prisoners must maintain connection during critical data transfer window.",
            })!,

            WardenObjectiveType.GatherSmallItems => item switch
            {
                WardenObjectiveItem.MemoryStick => Generator.Pick(new List<string>
                {
                    "Prisoners to locate encrypted memory stick. Data extraction critical.",
                    "Field units ordered to recover lost data storage from hostile sector.",
                    "Secure memory module detected. Retrieval necessary for Warden systems.",
                    "Data fragment isolated in hazardous zone. Recovery team dispatched.",
                    "Memory device offline. Physical retrieval required. Avoid data corruption.",
                    "Intel package stored in portable modules. Must be recovered intact.",
                    "Prisoners dispatched to acquire sector-specific storage device.",
                    "Portable memory carrier located deep in restricted sector. Recover immediately.",
                    "Encrypted storage critical to system reboot. Retrieve without compromise.",
                    "Loss of memory stick will hinder operations. Recovery is mandatory.",
                    "High-priority retrieval of encrypted drive. Failure not tolerated.",
                    "Locate and secure portable drive before hostile sweep.",
                    "Memory stick data flagged for priority analysis. Prisoners to retrieve now.",
                    "Portable module location compromised. Recovery under time pressure.",
                    "Field recovery of storage device required for Warden objectives.",
                })!,

                WardenObjectiveItem.PersonnelId => Generator.Pick(new List<string>
                {
                    "Identity verification incomplete. Prisoners to retrieve personnel ID card.",
                    "Personnel clearance tag missing. Retrieve to complete authentication.",
                    "Sector access locked pending ID retrieval. Locate target credentials.",
                    "Locate and secure personnel identification for access chain.",
                    "Identity record required. Prisoners dispatched to hostile zone to recover.",
                    "Lost clearance card located in high-risk sector. Recovery needed.",
                    "Target identification data essential. Secure without delay.",
                    "Personnel ID card flagged in Warden systems. Retrieve intact.",
                    "Authentication pipeline blocked. Prisoners to recover missing identity token.",
                    "ID recovery operation in progress. Maintain possession until extraction.",
                    "Personnel badge vital for system override. Secure from last known location.",
                    "Identity clearance stored in physical card format. Recovery mandated.",
                    "Locate personnel identification linked to priority subject.",
                    "Authorization halted until clearance tag is in Warden possession.",
                    "Identification artifact retrieval required. Proceed to marked zone.",
                })!,

                WardenObjectiveItem.PartialDecoder => Generator.Pick(new List<string>
                {
                    "Decoder fragment required to access restricted protocol. Locate and retrieve.",
                    "Partial decryption key located in unstable zone. Recovery required.",
                    "System unlock incomplete. Missing decoder section must be found.",
                    "Decoder segment retrieval necessary for chain command execution.",
                    "Locate incomplete decoder device to restore access pathway.",
                    "Partial decoder module vital for sequence continuation. Secure now.",
                    "Recovery of decoder piece necessary for uplink.",
                    "Decoder shard location verified. Prisoners dispatched.",
                    "Partial decoder absent from array. Retrieval critical.",
                    "Locate decoder section to enable further transmission.",
                    "System lock bypass blocked until decoder segment retrieved.",
                    "Missing decoder element found in high-threat area. Units sent for recovery.",
                    "Obtain decoder portion for operational continuity.",
                    "Decoder fragment to be extracted and integrated immediately.",
                    "Field units to retrieve decoder section from specified sector.",
                })!,

                WardenObjectiveItem.Harddrive => Generator.Pick(new List<string>
                {
                    "Critical data contained on portable hard-drive. Secure and return intact.",
                    "Portable disk storage located in hostile environment. Recovery required.",
                    "Operational continuity dependent on hard-drive retrieval.",
                    "Locate large-capacity drive containing key operational files.",
                    "Field recovery of portable drive essential for system restoration.",
                    "Data hub offline until portable storage returned.",
                    "Drive must be recovered for decryption task. Proceed now.",
                    "Locate and secure hard-drive from last known terminal location.",
                    "Drive houses mission-critical logs. Recovery priority elevated.",
                    "Loss of hard-drive unacceptable. Extraction team deployed.",
                    "Portable disk recovery essential for phase two operations.",
                    "Retrieve external drive to continue operations.",
                    "Secure large data drive before sector lockdown.",
                    "Hard-drive retrieval from infected sector authorized.",
                    "Disk unit loss will terminate mission progress. Retrieve now.",
                })!,

                WardenObjectiveItem.Glp_1 => Generator.Pick(new List<string>
                {
                    "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.",
                    "Conduit genetic profile compromised. Recover first-phase GLP sample.",
                    "Phase one GLP specimen required for conduit calibration.",
                    "Retrieve GLP-1 biological sample from marked containment.",
                    "Genetic link sample missing. GLP-1 recovery in progress.",
                    "Field units tasked with collecting GLP-1 target tissue.",
                    "Recovery of GLP-1 critical for ongoing conduit research.",
                    "Locate and secure phase one GLP vial from storage.",
                    "Prisoners dispatched to recover GLP-1 from biological vault.",
                    "Specimen chain incomplete without GLP-1 retrieval.",
                    "GLP-1 containment breach suspected. Recover asset.",
                    "GLP-1 extraction required for final sequence.",
                    "Recover GLP-1 tissue for genetic matching process.",
                    "GLP-1 stored in restricted biolab. Secure immediately.",
                    "Locate missing GLP-1 sample before degradation.",
                    "Biological stability threatened. Retrieve GLP-1 without delay.",
                })!,

                WardenObjectiveItem.Glp_2 => Generator.Pick(new List<string>
                {
                    "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.",
                    "Second-phase GLP sample required. Recovery mission active.",
                    "GLP-2 biological material essential for conduit link completion.",
                    "Retrieve GLP-2 specimen from marked secure location.",
                    "Field units to recover GLP-2 to finalize chain.",
                    "Loss of GLP-2 will halt research protocols.",
                    "Secure GLP-2 container before hostile breach.",
                    "GLP-2 recovery authorized for chain integrity.",
                    "Prisoners ordered to recover second-stage GLP tissue.",
                    "Locate GLP-2 vial for conduit completion.",
                    "GLP-2 specimen retrieval required for Warden analysis.",
                    "Biohazard zone contains GLP-2 asset. Retrieve intact.",
                    "Transport GLP-2 back to extraction point under guard.",
                    "GLP-2 loss unacceptable. Secure now.",
                    "Find GLP-2 in assigned zone before containment fails.",
                    "GLP-2 retrieval task initiated. Do not compromise.",
                })!,

                WardenObjectiveItem.Osip => Generator.Pick(new List<string>
                {
                    "OSIP unit missing from network chain. Recovery mission engaged.",
                    "Portable OSIP module required for operational sync.",
                    "OSIP hardware offline. Retrieval needed to restore systems.",
                    "Locate and recover OSIP device from hostile zone.",
                    "OSIP data link broken. Recover device to restore feed.",
                    "Operational sync impossible without OSIP. Secure now.",
                    "OSIP hardware asset retrieval authorized.",
                    "Field units tasked with retrieving OSIP module.",
                    "Loss of OSIP breaks uplink protocol. Retrieve immediately.",
                    "Locate OSIP module from last known ping coordinates.",
                    "Recovery of OSIP ensures continuity of system control.",
                    "Extract OSIP from compromised storage node.",
                    "Prisoners deployed to recover OSIP from containment.",
                    "Secure OSIP hardware before systems degrade further.",
                    "Locate OSIP in hostile sector and extract.",
                })!,

                WardenObjectiveItem.PlantSample => Generator.Pick(new List<string>
                {
                    "Botanical specimen required for analysis. Retrieve from growth chamber.",
                    "Locate plant sample before contamination spreads.",
                    "Biological sample from flora essential for study. Recover intact.",
                    "Hazardous sector contains necessary plant specimen.",
                    "Plant-based sample needed for environmental scan.",
                    "Retrieve botanical specimen from sealed greenhouse.",
                    "Loss of plant sample will compromise research.",
                    "Prisoners deployed to collect flora sample under quarantine.",
                    "Plant matter required for protocol review. Retrieve carefully.",
                    "Specimen located in overgrown zone. Recovery authorized.",
                    "Botanical recovery required for sequence completion.",
                    "Locate and secure plant tissue sample from targeted area.",
                    "Flora sample extraction to be performed by prisoners.",
                    "Overgrowth sector houses required specimen. Recover intact.",
                    "Botanical sample retrieval critical. Proceed with caution.",
                })!,

                WardenObjectiveItem.DataCube => Generator.Pick(new List<string>
                {
                    "Primary data cube detected. Retrieval will restore Warden archives.",
                    "Locate cube to recover essential system datasets.",
                    "Critical cube asset missing. Recovery required for analysis.",
                    "Data cube offline. Manual recovery required.",
                    "Locate and secure primary cube from hostile environment.",
                    "Cube houses sensitive archives. Recovery is top priority.",
                    "Field teams ordered to recover primary data cube.",
                    "Cube retrieval operation underway. Maintain security.",
                    "Loss of cube halts progress. Secure from marked location.",
                    "Locate cube to complete primary mission chain.",
                    "Cube integrity threatened. Recover immediately.",
                    "Data cube recovery ensures operational continuity.",
                    "Cube asset retrieval authorized for current protocol.",
                    "Secure cube from hostile-controlled zone.",
                    "Cube retrieval mandatory for final objective.",
                })!,

                WardenObjectiveItem.DataCubeBackup => Generator.Pick(new List<string>
                {
                    "Backup cube contains secondary system archives. Recovery needed.",
                    "Locate backup cube for redundancy protocol.",
                    "Loss of backup cube will risk data integrity.",
                    "Retrieve secondary cube from containment vault.",
                    "Backup cube needed to restore secondary systems.",
                    "Secondary cube location marked. Recover intact.",
                    "Backup cube retrieval ensures operational failover.",
                    "Secure secondary cube to complete redundancy chain.",
                    "Backup cube offline. Manual extraction required.",
                    "Locate and secure secondary cube from hostile area.",
                    "Cube recovery mission authorized. Backup integrity critical.",
                    "Backup cube to be recovered before hostile breach.",
                    "Prisoners ordered to retrieve redundancy cube.",
                    "Backup data cube recovery essential for Warden resilience.",
                    "Locate cube asset for backup protocol.",
                    "Secure backup cube before hostile contact.",
                })!,

                WardenObjectiveItem.DataCubeTampered => Generator.Pick(new List<string>
                {
                    "Tampered cube detected. Recovery required for forensic analysis.",
                    "Locate and secure altered cube for investigation.",
                    "Compromised cube contains modified data. Retrieve intact.",
                    "Retrieve tampered cube before hostile data purge.",
                    "Data integrity breach suspected. Cube recovery mandated.",
                    "Altered data cube may hold critical anomalies. Secure it.",
                    "Tampered cube recovery ensures evidence preservation.",
                    "Field teams deployed to extract compromised data cube.",
                    "Locate data cube with modified payload for Warden review.",
                    "Recovery of tampered cube will aid system security.",
                    "Cube flagged for tampering. Retrieve without delay.",
                    "Compromised cube asset must be secured before analysis.",
                    "Locate modified cube to continue investigation.",
                    "Tampered cube storage location identified. Recovery active.",
                    "Secure altered cube before data loss escalates.",
                })!,

                _ => "Prisoners to collect items from storage facility. High asset fatality rate expected."
            },

            WardenObjectiveType.Survival => Generator.Pick(new List<string>
            {
                "Prisoners expended for diversion to clear adjacent sectors. Local power grid unstable.",
                "Prisoners will act as decoy for undisclosed parallel objective. Surviving prisoners will return for extraction once undisclosed objective has been completed.",

                "Biomass detected in critical zone. Prisoners will hold ground until extraction window becomes viable.",
                "Stabilization protocol in progress. Prisoners must remain in designated zone until system confirms lock.",
                "Prisoners required to maintain presence in conflict zone. Extraction contingent on external task completion.",
                "Warden-ordered delay in retrieval. Prisoners instructed to endure hostile conditions until recall is authorized.",
                "Hold position directive issued. Prisoners will await extraction signal amidst escalating environmental threats.",
                "Decoy initiative active. Enemy redirection underway. Prisoners expected to endure contact duration.",
                "Prisoners deployed to obstruct local adversaries. Extraction not available until uplink confirms clearance.",
                "Area containment required. Surviving prisoners will proceed to exit zone upon conclusion of external protocol.",
                "Hostile suppression requested. Prisoners tasked with absorbing attention while secondary teams mobilize.",
                "Temporal window for safe egress pending. Prisoners instructed to engage threat deterrence protocol until Warden grants withdrawal.",
            })!,

            WardenObjectiveType.TimedTerminalSequence => Generator.Pick(new List<string>
            {
                "System handshake protocol failed. Manual verification required at linked terminals. Prisoners dispatched to execute timed input sequence.",
                "Redundant encryption detected. Terminal relay must be verified through simultaneous prisoner input.",
                "Timed sequence required for data authentication. Two prisoner units to coordinate across sector. Expect hostile interference.",
                "Access to archived system blocked. Manual override initiated. Terminal sequence must be completed within time limit.",
                "Sequence protocol: <color=orange>INIT</color> – <color=orange>VERIFY</color> – <color=orange>CONFIRM</color>. Prisoners instructed to synchronize terminal access or risk hostile resurgence.",
                "Warden connection unstable. Input terminals require prisoner verification before data loss becomes permanent.",
                "Countdown-based protocol initiated. Expect interference. Prisoners must locate and confirm secondary terminal ID before uplink failure.",
                "Timed command relay engaged. Error tolerance: zero. Terminal pair must be activated in correct order. Consequences severe.",
                "System requires human presence to execute critical data bridge. Failure to complete in time will reset sequence and escalate threat response.",
                "Terminal sync required. Prisoners ordered to perform chained input across network before countdown expires.",
            })!,

            // WardenObjectiveType.RetrieveBigItems => expr,
            // WardenObjectiveType.CentralGeneratorCluster => expr,
            // WardenObjectiveType.HsuActivateSmall => expr,
            // WardenObjectiveType.GatherTerminal => expr,
            // WardenObjectiveType.CorruptedTerminalUplink => expr,
            // WardenObjectiveType.Empty => expr,

            _ => "<color=red>-INTEL REDACTED-</color>"
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
        => CalculateExitScanSpeedMultiplier(Generator.Between(min, max));

    /// <summary>
    /// Add's default exit/completion waves for an objective. Often we want
    /// these but want to adjust them based on difficulty and what objective
    /// we are on.
    /// </summary>
    public void AddCompletedObjectiveWaves(Level level, BuildDirector director)
    {
        // Extraction waves. These are progressively harder
        // Overload is balanced to get harder error waves when getting the sample
        switch (level.Tier, director.Bulkhead)
        {
            case ("A", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Easy);
                break;
            case ("A", Bulkhead.Extreme):
                break;
            case ("A", Bulkhead.Overload):
                break;

            case ("B", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Medium);
                break;
            case ("B", Bulkhead.Extreme):
                break;
            case ("B", Bulkhead.Overload):
                break;

            case ("C", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Medium);
                break;
            case ("C", Bulkhead.Extreme):
                break;
            case ("C", Bulkhead.Overload):
                WavesOnGotoWin.Add(GenericWave.ErrorAlarm_Easy);
                break;

            case ("D", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Hard);
                break;
            case ("D", Bulkhead.Extreme):
                WavesOnGotoWin.Add(GenericWave.ErrorAlarm_Easy);
                break;
            case ("D", Bulkhead.Overload):
                WavesOnGotoWin.Add(GenericWave.ErrorAlarm_Normal);
                break;

            case ("E", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_VeryHard);
                break;
            case ("E", Bulkhead.Extreme):
                WavesOnGotoWin.Add(GenericWave.ErrorAlarm_Normal);
                break;
            case ("E", Bulkhead.Overload):
                WavesOnGotoWin.Add(GenericWave.ErrorAlarm_Hard);
                break;
        }

        if (director.Bulkhead == Bulkhead.Main)
        {
            // Set a longer extract scan then the default flat rate time
            ChainedPuzzleAtExitScanSpeedMultiplier = director.Tier switch
            {
                "A" => GenExitScanTime(25, 35),
                "B" => GenExitScanTime(35, 45),
                "C" => GenExitScanTime(45, 80),
                "D" => GenExitScanTime(90, 120),
                "E" => GenExitScanTime(100, 140),
                _ => 1.0,
            };
        }
    }

    /// <summary>
    /// Some settings from the objective are needed for level generation. However plenty of
    /// layout information is needed for the objective. Objective building is split into two
    /// phases. PreBuild() is called first to generate the objective and then Build() is called
    /// after level layout has been done.
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public static WardenObjective PreBuild(BuildDirector director, Level level)
    {
        var objective = new WardenObjective
        {
            Type = director.Objective,
            SubType = director.SubObjective,
        };

        switch (objective.Type)
        {
            case WardenObjectiveType.HsuFindSample:
                break;

            case WardenObjectiveType.ReactorStartup:
                objective.PreBuild_ReactorStartup(director, level);
                break;

            case WardenObjectiveType.ReactorShutdown:
                objective.PreBuild_ReactorShutdown(director, level);
                break;

            case WardenObjectiveType.ClearPath:
                break;

            case WardenObjectiveType.RetrieveBigItems:
                objective.PreBuild_RetrieveBigItems(director, level);
                break;

            case WardenObjectiveType.GatherSmallItems:
                objective.PreBuild_GatherSmallItems(director, level);
                break;

            case WardenObjectiveType.SpecialTerminalCommand:
                objective.PreBuild_SpecialTerminalCommand(director, level);
                break;

            case WardenObjectiveType.PowerCellDistribution:
                objective.PreBuild_PowerCellDistribution(director, level);
                break;

            case WardenObjectiveType.TerminalUplink:
                objective.PreBuild_TerminalUplink(director, level);
                break;

            case WardenObjectiveType.CentralGeneratorCluster:
                objective.PreBuild_CentralGeneratorCluster(director, level);
                break;

            case WardenObjectiveType.HsuActivateSmall:
                objective.PreBuild_HsuActivateSmall(director, level);
                break;

            case WardenObjectiveType.Survival:
                break;

            case WardenObjectiveType.GatherTerminal:
                objective.PreBuild_GatherTerminal(director, level);
                break;

            case WardenObjectiveType.CorruptedTerminalUplink:
                objective.PreBuild_CorruptedTerminalUplink(director, level);
                break;

            case WardenObjectiveType.Empty:
                break;

            case WardenObjectiveType.TimedTerminalSequence:
                objective.PreBuild_TimedTerminalSequence(director, level);
                break;

            #region Autogen Custom Objectives

            case WardenObjectiveType.ReachKdsDeep:
                objective.PreBuild_ReachKdsDeep(director, level);
                break;

            #endregion

            default:
                throw new ArgumentOutOfRangeException(nameof(director));
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

        GoToWinCondition_Elevator  = new Text(() => $"Return to the point of entrance in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        GoToWinCondition_CustomGeo = new Text(() => $"Go to the forward exit point in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition";

        // Set the level description if there's no description provided already
        if (director.Bulkhead.HasFlag(Bulkhead.Main) && level.Description == 0)
        {
            var text = GenLevelDescription(director.Objective);

            if (Generator.Flip(0.4))
                text += " " + IntelCasualtyWarning();

            level.Description = new Text(text).PersistentId;
        }

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
                Build_GatherSmallItems(director, level);
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

            case WardenObjectiveType.GatherTerminal:
            {
                Build_GatherTerminal(director, level);
                break;
            }

            case WardenObjectiveType.CorruptedTerminalUplink:
            {
                Build_CorruptedTerminalUplink(director, level);
                break;
            }

            /**
             * Timed terminal missions
             */
            case WardenObjectiveType.TimedTerminalSequence:
                Build_TimedTerminalSequence(director, level);
                break;

            #region Autogen Custom Objectives

            case WardenObjectiveType.ReachKdsDeep:
                Build_ReachKdsDeep(director, level);
                break;

            #endregion
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

        if (director.Bulkhead != Bulkhead.Main)
            return;

        switch (director.Objective)
        {
            case WardenObjectiveType.HsuFindSample:
                PostBuildIntel_HsuFindSample(level);
                break;

            case WardenObjectiveType.ReactorStartup:
                PostBuildIntel_ReactorStartup(level);
                break;

            case WardenObjectiveType.ReactorShutdown:
                PostBuildIntel_ReactorShutdown(level);
                break;

            case WardenObjectiveType.GatherSmallItems:
                PostBuildIntel_GatherSmallItems(level);
                break;

            case WardenObjectiveType.ClearPath:
                PostBuildIntel_ClearPath(level);
                break;

            case WardenObjectiveType.SpecialTerminalCommand:
                PostBuildIntel_SpecialTerminalCommand(level);
                break;

            case WardenObjectiveType.RetrieveBigItems:
                PostBuildIntel_RetrieveBigItems(level);
                break;

            case WardenObjectiveType.PowerCellDistribution:
                PostBuildIntel_PowerCellDistribution(level);
                break;

            case WardenObjectiveType.TerminalUplink:
                PostBuildIntel_TerminalUplink(level);
                break;

            case WardenObjectiveType.CentralGeneratorCluster:
                PostBuildIntel_CentralGeneratorCluster(level);
                break;

            case WardenObjectiveType.HsuActivateSmall:
                PostBuildIntel_HsuActivateSmall(level);
                break;

            case WardenObjectiveType.Survival:
                PostBuildIntel_Survival(level);
                break;

            case WardenObjectiveType.GatherTerminal:
                PostBuildIntel_GatherTerminal(level);
                break;

            case WardenObjectiveType.CorruptedTerminalUplink:
                PostBuildIntel_CorruptedTerminalUplink(level);
                break;

            case WardenObjectiveType.TimedTerminalSequence:
                PostBuildIntel_TimedTerminalSequence(level);
                break;

            case WardenObjectiveType.ReachKdsDeep:
                PostBuildIntel_ReachKdsDeep(level);
                break;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="level"></param>
    public void PostBuild_ForwardExtract(Level level)
    {
        // Return early if the level doesn't use a forward extract
        if (level.ExtractionZone is { ZoneNumber: 0, Bulkhead: Bulkhead.Main | Bulkhead.StartingArea })
            return;

        // Assign the custom geo gotowin intel to the elevator one. This ensures we always show
        // the forward extract message
        GoToWinCondition_Elevator = GoToWinCondition_CustomGeo;
    }

    #region Internal Fields
    [JsonIgnore]
    public bool ReactorStartupGetCodes { get; set; } = false;

    [JsonIgnore]
    public int ReactorStartup_FetchWaves { get; set; } = 0;

    /// <summary>
    /// Collect the nodes where items have been placed
    /// </summary>
    [JsonIgnore]
    public List<ZoneNode> PlacementNodes { get; set; } = new();
    #endregion

    #region General fields
    /// <summary>
    /// What type of objective this is.
    /// </summary>
    public WardenObjectiveType Type { get; set; }

    /// <summary>
    /// Autogen only, subtype is used for more specific level crafting
    /// </summary>
    [JsonIgnore]
    public WardenObjectiveSubType SubType { get; set; } = WardenObjectiveSubType.Default;

    #region Information and display strings
    [JsonIgnore]
    public Text MainObjective { get; set; } = Text.None;

    [JsonProperty("MainObjective")]
    public uint MainObjectiveId => MainObjective.PersistentId;

    public string FindLocationInfo { get; set; } = "";
    public string FindLocationInfoHelp { get; set; } = "Access more data in the terminal maintenance system";

    [JsonIgnore]
    public Text GoToZone { get; set; } = Text.None;

    [JsonProperty("GoToZone")]
    public uint GoToZoneId => GoToZone.PersistentId;

    public string GoToZoneHelp { get; set; } = "";
    public string InZoneFindItem { get; set; } = "";
    public string InZoneFindItemHelp { get; set; } = "";
    public string SolveItem { get; set; } = "";
    public string SolveItemHelp { get; set; } = "";

    /// <summary>
    /// Default = "Return to the point of entrance in [EXTRACTION_ZONE]"
    /// </summary>
    [JsonIgnore]
    public Text GoToWinCondition_Elevator { get; set; } = Text.None;

    [JsonProperty("GoToWinCondition_Elevator")]
    public uint GoToWinCondition_ElevatorId => GoToWinCondition_Elevator.PersistentId;

    public string GoToWinConditionHelp_Elevator { get; set; } = "";

    /// <summary>
    /// Default = "Go to the forward exit point in [EXTRACTION_ZONE]"
    /// </summary>
    [JsonIgnore]
    public Text GoToWinCondition_CustomGeo { get; set; } = Text.None;

    [JsonProperty("GoToWinCondition_CustomGeo")]
    public uint GoToWinCondition_CustomGeoId => GoToWinCondition_CustomGeo.PersistentId;

    public string GoToWinConditionHelp_CustomGeo { get; set; } = "";
    public string GoToWinCondition_ToMainLayer { get; set; } = "";

    /// <summary>
    /// Default = "Go back to the main objective and complete the expedition."
    /// </summary>
    public string GoToWinConditionHelp_ToMainLayer { get; set; } = "Go back to the main objective and complete the expedition.";

    public string WaveOnElevatorWardenIntel { get; set; } = "";
    public string Survival_TimerTitle { get; set; } = "";
    public string Survival_TimerToActivateTitle { get; set; } = "";

    /// <summary>
    /// Default = 180
    /// </summary>
    public double ShowHelpDelay { get; set; } = 180.0;
    #endregion

    #region Fog related

    /// <summary>
    ///
    /// </summary>
    [JsonIgnore]
    public Fog FogOnGotoWin { get; set; } = Fog.None;

    public uint FogTransitionDataOnGotoWin => FogOnGotoWin.PersistentId;

    public double FogTransitionDurationOnGotoWin { get; set; } = 0.0;

    #endregion
    #endregion

    #region Events
    public List<WardenObjectiveEvent> EventsOnActivate { get; set; } = new List<WardenObjectiveEvent>();

    public List<WardenObjectiveEvent> EventsOnElevatorLand { get; set; } = new List<WardenObjectiveEvent>();

    public List<WardenObjectiveEvent> EventsOnGotoWin { get; set; } = new List<WardenObjectiveEvent>();

    public ExitWaveTrigger WaveOnGotoWinTrigger { get; set; } = ExitWaveTrigger.OnObjectiveCompleted;

    public ExitWaveTrigger EventsOnGotoWinTrigger { get; set; } = ExitWaveTrigger.OnObjectiveCompleted;

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

    /// <summary>
    /// Add the zone nodes which will have the items to this list
    /// </summary>
    [JsonIgnore]
    public List<ZoneNode> Gather_PlacementNodes { get; set; } = new();
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
    /// Determines how the uplink waves spawn should happen.
    ///
    /// Note: The base game use `SurvivalWaveSpawnType.InSuppliedCourseNodeZone` for almost every
    /// single uplink in the game. Almost every single alarm or otherwise outside of uplinks use
    /// `SurvivalWaveSpawnType.InRelationToClosestAlivePlayer`. `InSuppliedCourseNodeZone` forces
    /// the waves to spawn in the same zone as the uplink terminal. Whereas
    /// `InRelationToClosestAlivePlayer` searches for spawn points that are two rooms away.
    ///
    /// For Autogen we will depart a bit from the base game and align the uplink waves to spawn
    /// with the same behavior as almost all other alarm waves.
    /// </summary>
    public SurvivalWaveSpawnType Uplink_WaveSpawnType { get; set; } =
        SurvivalWaveSpawnType.InRelationToClosestAlivePlayer;
    #endregion

    #region Type=9: Central generator cluster
    /// <summary>
    /// How many cells to distribute
    ///
    /// Seems this may be unused?
    /// </summary>
    [JsonProperty("PowerCellsToDistribute")]
    public int PowerCellsToDistribute { get; set; } = 0;

    /// <summary>
    /// Number of generators to spawn. Max is 8 for normal single generator tiles. Some tiles do
    /// have room for more but 8 is the practical limit for it to work with all geos.
    ///
    /// Max = 8
    /// </summary>
    [JsonProperty("CentralPowerGenClustser_NumberOfGenerators")]
    public int CentralGeneratorCluster_NumberOfGenerators { get; set; } = 0;

    /// <summary>
    /// How many power cells
    /// </summary>
    [JsonProperty("CentralPowerGenClustser_NumberOfPowerCells")]
    public int CentralGeneratorCluster_NumberOfPowerCells { get; set; } = 4;

    /// <summary>
    /// How the fog should change after inserting each cell
    /// </summary>
    [JsonProperty("CentralPowerGenClustser_FogDataSteps")]
    public List<GeneralFogStep> CentralGeneratorCluster_FogDataSteps { get; set; } = new();
    #endregion

    #region Type=10: HsuActivateSmall (Bring Neonate/Datasphere to depressurizer)

    /// <summary>
    /// Lets us dynamically set the machine name based on the geomorph.
    /// </summary>
    [JsonIgnore]
    public string HsuActivateSmall_MachineName { get; set; } = "";

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

    /// <summary>
    /// Internal property
    ///
    /// Determines what variant of this mission we play
    ///
    /// Default = GatherTerminalType.Default
    /// </summary>
    [JsonIgnore]
    public GatherTerminalType GatherTerminal_SubType { get; set; } = GatherTerminalType.Default;

    public int GatherTerminal_SpawnCount { get; set; } = 0;

    public int GatherTerminal_RequiredCount { get; set; } = 0;

    public string GatherTerminal_Command { get; set; } = "";

    public string GatherTerminal_CommandHelp { get; set; } = "";

    public string GatherTerminal_DownloadingText { get; set; } = "";

    public string GatherTerminal_DownloadCompleteText { get; set; } = "";

    /// <summary>
    /// How long should the command take to execute (seconds). Defaults to -1 which signifies
    /// the command should execute as fast as any other command on the terminal
    ///
    /// Default = -1.0
    /// </summary>
    public double GatherTerminal_DownloadTime { get; set; } = -1.0;

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

    #region Type=20: -CUSTOM- Reach KDS Deep

    [JsonIgnore]
    public int KdsDeepUnit { get; set; } = 1;

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
    public double ChainedPuzzleAtExitScanSpeedMultiplier { get; set; } = GenExitScanTime(20, 25);
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
    public bool LightsOnFromBeginning = false;
    public bool LightsOnDuringIntro = false;
    public bool LightsOnWhenStartupComplete = false;
    public bool DoNotSolveObjectiveOnReactorComplete = false;
    public JArray PostCommandOutput = new JArray();
    public int SpecialCommandRule = 0;

    #endregion

    #region Unused fields
    public string Header { get; set; } = "";
    #endregion
}
