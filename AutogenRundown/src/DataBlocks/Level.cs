using System.Resources;
using System.Text.RegularExpressions;
using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.Patches.CustomTerminals;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Logs;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using AutogenRundown.GeneratorData;
using AutogenRundown.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public class BuildFrom
{
    public int LayerType { get; set; } = 0;
    public int Zone { get; set; } = 0;
}

public partial class Level
{
    #region Filler settings that won't change
    public bool Enabled = true;
    public bool IsSinglePlayer = false;
    public bool SkipLobby = false;
    public bool PutIconAboveTier = false;
    public bool DisablePlayerVoicelines = false;
    public bool ExcludeFromProgression = false;
    public bool HideOnLocked = false;
    public bool HasExternalStyle = false;
    public bool HasStoryStyle = false;
    public bool UseGearPicker = false;
    #endregion

    #region Internal settings for us
    /// <summary>
    /// Level name
    /// </summary>
    [JsonIgnore]
    public string Name { get; set; } = "";

    /// <summary>
    /// Level Tier, roughly difficulty
    /// </summary>
    [JsonIgnore]
    public string Tier { get; set; } = "A";

    /// <summary>
    /// Level index (A1, A2, A3, etc)
    /// </summary>
    [JsonIgnore]
    public int Index { get; set; } = 1;

    /// <summary>
    /// Level depth in meters
    /// </summary>
    [JsonIgnore]
    public int Depth { get; set; } = 1;

    /// <summary>
    ///
    /// </summary>
    [JsonIgnore]
    public List<(double, ZoneNode)> ForwardExtractStartCandidates { get; set; } = new();

    [JsonIgnore]
    public bool HasMedBay { get; set; } = false;

    /// <summary>
    /// Mainly used for calculating what the text should be for extract
    /// </summary>
    [JsonIgnore]
    public ZoneNode ExtractionZone { get; set; } = new()
    {
        Bulkhead = Bulkhead.Main | Bulkhead.StartingArea,
        ZoneNumber = 0
    };

    /// <summary>
    /// Which complex type to use.
    ///
    /// By default set to a random value from the available complexes. Weight more towards
    /// mining and tech.
    /// </summary>
    [JsonIgnore]
    public Complex Complex { get; set; } = Generator.Select(
        new List<(double, Complex)>
        {
            (1.0, Complex.Mining),
            (1.0, Complex.Tech),
            (0.7, Complex.Service)
        });

    [JsonIgnore]
    public ComplexResourceSet ResourceSet { get; set; } = ComplexResourceSet.Mining;

    /// <summary>
    /// When set, geomorph pick lists prefer entries of this SubComplex and (for
    /// Service+Gardens) the gardens-heavy resource set replaces the floodways-only one.
    /// Set only by generation overrides; null = default behavior.
    /// </summary>
    [JsonIgnore]
    public SubComplex? PreferredSubComplex { get; set; }

    /// <summary>
    /// Chances of a level selecting each combination of bulkheads
    /// </summary>
    [JsonIgnore]
    public List<(double, Bulkhead)> BulkheadChanceTable { get; set; } = new()
    {
        (0.25, Bulkhead.Main),
        (0.4, Bulkhead.Main | Bulkhead.Extreme),
        (0.2, Bulkhead.Main | Bulkhead.Overload),
        (0.15, Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload)
    };

    [JsonIgnore]
    public LayoutPlanner Planner { get; set; } = new();

    [JsonIgnore]
    public Dictionary<DimensionIndex, Dictionary<Bulkhead, LevelLayout>> DimensionLayouts { get; } = new();

    [JsonIgnore]
    public LevelSettings Settings { get; set; }

    [JsonIgnore]
    public List<RelativeDirection> RelativeDirections { get; set; } = new()
    {
        RelativeDirection.Global_Forward,
        RelativeDirection.Global_Left,
        RelativeDirection.Global_Right,
        RelativeDirection.Global_Backward
    };

    #region Zone Numbers

    /// <summary>
    /// What zone does Main start with
    /// </summary>
    [JsonIgnore]
    public int ZoneAliasStart_Main { get; set; } = 0;

    /// <summary>
    /// What zone does Extreme start with
    /// </summary>
    [JsonIgnore]
    public int ZoneAliasStart_Extreme { get; set; } = 0;

    /// <summary>
    /// What zone does Overload start with
    /// </summary>
    [JsonIgnore]
    public int ZoneAliasStart_Overload { get; set; } = 0;

    public int GetZoneAliasStart(Bulkhead bulkhead)
        => bulkhead switch
        {
            Bulkhead.Main => ZoneAliasStart_Main,
            Bulkhead.Extreme => ZoneAliasStart_Extreme,
            Bulkhead.Overload => ZoneAliasStart_Overload,
            _ => 0,
        };

    /// <summary>
    /// Generates the zone alias start numbers, tries to ensure there will be no collisions.
    ///
    /// TODO: we can probably remove this
    /// </summary>
    private void GenerateZoneAliasStarts()
    {
        var minmax = Tier switch
        {
            // A-Tier: 195 spread
            "A" => new List<(int, int)>
            {
                (  5, 70),
                ( 95, 135),
                (160, 200),
            },

            "B" => new List<(int, int)>
            {
                ( 50, 170),  // 250 spread
                (200, 275),
                (300, 450),
            },

            "C" => new List<(int, int)>
            {
                (200, 275), // 400 spread
                (300, 475),
                (500, 600),
            },

            "D" => new List<(int, int)>
            {
                (300, 475), // 550 spread
                (500, 680),
                (700, 850),
            },

            "E" => new List<(int, int)>
            {
                (450, 570), // 500 spread
                (600, 750),
                (790, 950),
            },

            _ => new List<(int, int)>
            {
                (  5, 70),
                ( 95, 135),
                (160, 200),
            }
        };

        var (min, max) = Generator.Draw(minmax);
        ZoneAliasStart_Main = Generator.Between(min, max);

        (min, max) = Generator.Draw(minmax);
        ZoneAliasStart_Extreme = Generator.Between(min, max);

        (min, max) = Generator.Draw(minmax);
        ZoneAliasStart_Overload = Generator.Between(min, max);
    }

    /// <summary>
    /// Recalculates the zone alias starts using the BetweenConstrained method. This lets us have
    /// closer and more naturally selected zone numbers instead of having to bucket them into
    /// their own groups of 100.
    /// </summary>
    public void RecalculateZoneAliasStarts()
    {
        var (min, max) = Tier switch
        {
            "A" => (  1, 190),
            "B" => ( 80, 340),
            "C" => (120, 660),
            "D" => (170, 820),
            "E" => (300, 950),

            _ => (500, 600)
        };

        var blocked = new List<(int, int)>();

        // Main
        var mainSize = Layouts[Bulkhead.Main].Zones.Count;

        ZoneAliasStart_Main = Generator.Between(min, max);

        blocked.Add((ZoneAliasStart_Main, ZoneAliasStart_Main + mainSize - 1));

        // Extreme
        if (HasExtreme)
        {
            var extremeSize = Layouts[Bulkhead.Extreme].Zones.Count;

            ZoneAliasStart_Extreme = Generator.BetweenConstrained(min, max, blocked, extremeSize + 5);

            blocked.Add((ZoneAliasStart_Extreme, ZoneAliasStart_Extreme + extremeSize - 1));
        }

        // Overload
        if (HasOverload)
        {
            var overloadSize = Layouts[Bulkhead.Overload].Zones.Count;

            ZoneAliasStart_Overload = Generator.BetweenConstrained(min, max, blocked, overloadSize + 5);
        }
    }

    #endregion
    #endregion

    #region === MODS ===
    #region Autogen Custom Definitions

    /// <summary>
    /// For customizing security doors
    /// </summary>
    [JsonIgnore]
    public LevelLogArchives LogArchives { get; private set; } = new();

    /// <summary>
    /// For customizing security doors
    /// </summary>
    [JsonIgnore]
    public LevelSecurityDoors SecurityDoors { get; private set; } = new();

    /// <summary>
    /// For performing custom terminal placement in a zone/area
    /// </summary>
    [JsonIgnore]
    public LevelTerminalPlacements TerminalPlacements { get; private set; } = new();

    /// <summary>
    /// Per-zone minimum tile (area) counts. Zones registered here will have their
    /// LG tile-expansion loop kept running until m_areas.Count reaches the recorded
    /// Count, regardless of coverage. Opt-in via Level.GenMultiRoomSpawnGeomorph().
    /// </summary>
    [JsonIgnore]
    public LevelAreaCounts AreaCounts { get; private set; } = new();

    #endregion

    #region ExtraObjectiveSetup Definitions
    /// <summary>
    /// Events on boss death definitions
    ///
    /// These can actually be used to trigger events on any units death
    /// </summary>
    [JsonIgnore]
    public LayoutDefinitions EOS_EventsOnBossDeath { get; private set; } = new()
    {
        Type = ExtraObjectiveSetupType.EventsOnBossDeath
    };

    /// <summary>
    /// Individual Generator LayoutDefinitions
    /// </summary>
    [JsonIgnore]
    public LayoutDefinitions EOS_EventsOnScoutScream { get; private set; } = new()
    {
        Type = ExtraObjectiveSetupType.EventsOnScoutScream
    };

    /// <summary>
    /// Individual Generator LayoutDefinitions
    /// </summary>
    [JsonIgnore]
    public LayoutDefinitions EOS_IndividualGenerator { get; private set; } = new()
    {
        Type = ExtraObjectiveSetupType.IndividualGenerator
    };

    /// <summary>
    /// Reactor Shutdown LayoutDefinitions
    /// </summary>
    [JsonIgnore]
    public LayoutDefinitions EOS_ReactorShutdown { get; private set; } = new()
    {
        Type = ExtraObjectiveSetupType.ReactorShutdown
    };

    /// <summary>
    /// Security Sensors definitions
    /// </summary>
    [JsonIgnore]
    public LayoutDefinitions EOS_SecuritySensor { get; private set; } = new()
    {
        Type = ExtraObjectiveSetupType.SecuritySensor
    };
    #endregion

    #region Zone Sensors (Autogen Custom)
    /// <summary>
    /// Zone-based security sensors that are placed automatically within zones.
    /// These are handled by AutogenRundown's ZoneSensorManager at runtime.
    /// </summary>
    [JsonIgnore]
    public List<ZoneSensorDefinition> ZoneSensors { get; private set; } = new();
    #endregion

    #region GlobalWaveSettings

    [JsonIgnore]
    public GlobalWaveSettings GlobalWaveSettings { get; set; } = GlobalWaveSettings.Default;

    #endregion
    #endregion

    #region Build directors
    /// <summary>
    /// Allows easy access to the directors without having to switch
    ///
    /// TODO: find all reads/gets on Director and convert them to use GetDirector().
    ///       As Bulkhead.Main | Bulkhead.StartingArea fails to find the main director with dictionary get
    /// </summary>
    [JsonIgnore]
    public Dictionary<Bulkhead, BuildDirector> Director { get; } = new();

    [JsonIgnore]
    public BuildDirector MainDirector
    {
        get => Director[Bulkhead.Main];
        set => Director[Bulkhead.Main] = value;
    }

    [JsonIgnore]
    public BuildDirector SecondaryDirector
    {
        get => Director[Bulkhead.Extreme];
        set => Director[Bulkhead.Extreme] = value;
    }

    [JsonIgnore]
    public BuildDirector OverloadDirector
    {
        get => Director[Bulkhead.Overload];
        set => Director[Bulkhead.Overload] = value;
    }

    public BuildDirector GetDirector(Bulkhead bulkhead)
    {
        if (bulkhead.HasFlag(Bulkhead.Extreme))
            return SecondaryDirector;

        if (bulkhead.HasFlag(Bulkhead.Overload))
            return OverloadDirector;

        return MainDirector;
    }
    #endregion

    #region Layout and layer data
    /// <summary>
    /// Allows easy access to the directors without having to switch
    /// </summary>
    [JsonIgnore]
    public Dictionary<Bulkhead, ObjectiveLayerData> ObjectiveLayer { get; }
        = new()
        {
            { Bulkhead.Main, new ObjectiveLayerData() },
            { Bulkhead.Extreme, new ObjectiveLayerData() },
            { Bulkhead.Overload, new ObjectiveLayerData() }
        };

    /// <summary>
    /// Tracking of other objectives
    /// </summary>
    [JsonIgnore]
    public Dictionary<Bulkhead, WardenObjective> Objective { get; } = new();

    /// <summary>
    /// Allows easy access to the directors without having to switch
    /// </summary>
    [JsonIgnore]
    private Dictionary<Bulkhead, LevelLayout> Layouts { get; } = new();
    #endregion

    /// <summary>
    /// Level description
    /// </summary>
    [JsonIgnore]
    public uint Description { get; set; } = 0;

    [JsonIgnore]
    public Fog FogSettings { get; set; } = Fog.DefaultFog;

    /// <summary>
    /// Tracks how fog is being used in this level to prevent incompatible fog transitions.
    /// </summary>
    [JsonIgnore]
    public FogUsage FogUsage { get; set; } = FogUsage.None;

    /// <summary>
    /// Attempts to reserve fog usage for this level. Returns true if compatible,
    /// false if the requested usage conflicts with existing fog usage.
    /// </summary>
    public bool TrySetFogUsage(FogUsage requested)
    {
        if (FogUsage == FogUsage.None)
        {
            FogUsage = requested;

            return true;
        }

        if (FogUsage == FogUsage.ShortDuration && requested == FogUsage.ShortDuration)
            return true;

        return false;
    }

    /// <summary>
    /// Flags the level as a test level
    /// </summary>
    [JsonIgnore]
    public bool IsTest { get; set; } = false;

    [JsonIgnore]
    public string? Prefix { get; set; }

    /// <summary>
    /// How the level should appear in the rundown screen
    /// </summary>
    public Accessibility Accessibility { get; set; } = Accessibility.Normal;

    public JObject Descriptive => new()
        {
            ["Prefix"] = IsTest ? "TEST" : (Prefix ?? Tier),
            ["PublicName"] = Name,
            ["IsExtraExpedition"] = false,
            ["SkipExpNumberInName"] = IsTest,
            ["UseCustomMatchmakingTier"] = false,
            ["CustomMatchmakingTier"] = 1,
            ["ProgressionVisualStyle"] = IsTest ? 1 : 0,
            ["ExpeditionDepth"] = Depth,
            ["EstimatedDuration"] = 930,
            ["DevInfo"] = $"globalwavesettings_{GlobalWaveSettings.PersistentId}",

            // Description shows up in menu
            ["ExpeditionDescription"] = Description,

            // Warden intel displays during drop
            // Original:
            //
            //  "... Shoot me then, 'cause I'm not going in there.  \r\n... Look, there's no time– \r\n... [gunshot]  \r\n... <size=200%><color=red>Start scanning.\r</color></size>",
            ["RoleplayedWardenIntel"] = ElevatorDropWardenIntel.MaxBy(intel => intel.Item1).Item2,
        };

    /// <summary>
    /// This string is displayed in the drop of the elevator as a role played intel message
    /// from the warden. It's always supposed to be an intercepted audio transmission from
    /// other prisoners attempting this particular mission and should hint at some of the
    /// surprises that will be found in the level.
    /// </summary>
    [JsonIgnore]
    public List<(int, string)> ElevatorDropWardenIntel { get; set; } = new();

    /// <summary>
    /// Level build seed. Use this to re-roll the level
    /// </summary>
    public int BuildSeed { get; set; } = Generator.Between(1, 2000);

    public JObject Seeds => new()
        {
            ["BuildSeed"] = BuildSeed,
            ["FunctionMarkerOffset"] = 1,
            ["StandardMarkerOffset"] = 0,
            ["LightJobSeedOffset"] = 0
        };

    public JObject Expedition => new()
        {
            ["ComplexResourceData"] = ResourceSet.PersistentId,
            ["MLSLevelKit"] = 1,
            ["LightSettings"] = 36,
            ["FogSettings"] = FogSettings.PersistentId,
            ["EnemyPopulation"] = 1,
            ["ExpeditionBalance"] = ExpeditionBalance.DefaultBalance.PersistentId,

            // ExtraObjectiveSetup will override these in some cases. But by
            // default we use the inbuilt scout wave spawning
            ["ScoutWaveSettings"] = ScoutWaveSettings.PersistentId,
            ["ScoutWavePopulation"] = ScoutWavePopulation.PersistentId,

            ["EnvironmentWetness"] = 0.0,
            ["DustColor"] = JObject.FromObject(
                new Color { Alpha = 1.0, Red = 0.5, Green = 0.5, Blue = 0.5 }),
            ["DustTurbulence"] = 1.0
        };

    public JObject VanityItemsDropData = new() { ["Groups"] = new JArray() };

    #region Main Objective Data
    /// <summary>
    /// Which zone the main bulkhead door gates. Often we want objectives to be spawned here
    /// or later.
    /// </summary>
    [JsonIgnore]
    public int MainBulkheadZone { get; set; } = 0;

    /// <summary>
    /// Match this to the persistent ID of the Level Layout
    /// </summary>
    public uint LevelLayoutData
    {
        get => Layouts.TryGetValue(Bulkhead.Main, out var val) ? val?.PersistentId ?? 0 : 0;
        private set { }
    }

    /// <summary>
    /// All levels must have a main objective
    /// </summary>
    public ObjectiveLayerData MainLayerData
    {
        get => ObjectiveLayer[Bulkhead.Main];
        set => ObjectiveLayer[Bulkhead.Main] = value;
    }

    /// <summary>
    /// True if this level has both an Extreme and Overload optional objectives
    /// </summary>
    [JsonIgnore]
    public bool HasPrisonerEfficiency => Settings.Bulkheads.HasFlag(Bulkhead.PrisonerEfficiency);

    #endregion

    #region Secondary (Extreme) Objective Data
    /// <summary>
    /// Secondary (Extreme) objectives data
    /// </summary>
    public ObjectiveLayerData SecondaryLayerData
    {
        get => ObjectiveLayer[Bulkhead.Extreme];
        set => ObjectiveLayer[Bulkhead.Extreme] = value;
    }

    [JsonProperty("SecondaryLayerEnabled")]
    public bool HasExtreme => Settings.Bulkheads.HasFlag(Bulkhead.Extreme);

    public uint SecondaryLayout
    {
        get => Layouts.TryGetValue(Bulkhead.Extreme, out var val) ? val?.PersistentId ?? 0 : 0;
        private set { }
    }

    public BuildFrom BuildSecondaryFrom = new BuildFrom
    {
        LayerType = 0,
        Zone = 0
    };
    #endregion

    #region Third (Overload) Objective Data
    /// <summary>
    /// Third (Overload) objectives data
    /// </summary>
    public ObjectiveLayerData ThirdLayerData
    {
        get => ObjectiveLayer[Bulkhead.Overload];
        set => ObjectiveLayer[Bulkhead.Overload] = value;
    }

    [JsonProperty("ThirdLayerEnabled")]
    public bool HasOverload => Settings.Bulkheads.HasFlag(Bulkhead.Overload);

    private bool HasMainBulkheadDoor => Settings.BulkheadStrategy is
        BukheadStrategy.Default or
        BukheadStrategy.CentralHub_x2 or
        BukheadStrategy.CentralHub_x3;

    public uint ThirdLayout
    {
        get => Layouts.TryGetValue(Bulkhead.Overload, out var val) ? val?.PersistentId ?? 0 : 0;
        private set { }
    }

    public BuildFrom BuildThirdFrom = new BuildFrom
    {
        LayerType = 0,
        Zone = 0
    };
    #endregion

    #region Dimension Data

    /// <summary>
    /// Dimensions used in this level
    ///
    /// By default we always add the pouncer arena to the level, even if a level doesn't use
    /// it. This is simpler than trying to conditionally add it to each level.
    /// </summary>
    public List<Levels.DimensionData> DimensionDatas { get; set; } = new()
    {
        new Levels.DimensionData
        {
            Dimension = DimensionIndex.Arena,
            Data = Dimension.PouncerArena
        }
    };

    /// <summary>
    /// Sound for warping?
    /// </summary>
    public Sound SoundEventOnWarpToReality = Sound.WarpReality;

    #endregion

    #region Modifiers
    [JsonIgnore]
    public double StartingInfection { get; set; } = 0.0;

    [JsonIgnore]
    public double StartingHealth { get; set; } = 1.0;

    [JsonIgnore]
    public double StartingMainAmmo { get; set; } = 1.0;

    [JsonIgnore]
    public double StartingSpecialAmmo { get; set; } = 1.0;

    [JsonIgnore]
    public double StartingTool { get; set; } = 1.0;

    /// <summary>
    /// Additional override data JSON encoding
    /// </summary>
    [JsonProperty("SpecialOverrideData")]
    public JObject SpecialOverrideData => new()
    {
        ["WeakResourceContainerWithPackChanceForLocked"] = -1.0,
        ["InfectionLevelAtExpeditionStart"] = StartingInfection,
        ["HealthLevelAtExpeditionStart"] = StartingHealth,
        ["StandardAmmoAtExpeditionStart"] = StartingMainAmmo,
        ["SpecialAmmoAtExpeditionStart"] = StartingSpecialAmmo,
        ["ToolAmmoAtExpeditionStart"] = StartingTool,

        // R8E2 fade out
        // ["PreSuccessScreen"] = "CM_PreSuccessScreen_Fadeout",

        ["CustomSuccessScreen"] = CustomSuccessScreen switch
        {
            SuccessScreen.ResourcesExpended => "CM_PageExpeditionSuccess_Resources expended_CellUI 2",
            SuccessScreen.SignalLost => "CM_PageExpeditionSuccess_SignalLost_CellUI",
            SuccessScreen.StackEmpty => "CM_PageExpeditionSuccess_Stack Empty_CellUI 1",

            _ => null,
        }
    };

    public SuccessScreen CustomSuccessScreen { get; set; } = SuccessScreen.Default;
    #endregion

    #region Scout Waves
    /// <summary>
    /// What wave population to use for scout waves.
    /// </summary>
    [JsonIgnore]
    public WavePopulation ScoutWavePopulation { get; set; } = WavePopulation.Baseline;

    /// <summary>
    /// What wave settings to use for scout waves.
    ///
    /// Note that wave setting should have finite points, otherwise scout waves will _never_
    /// end when triggered.
    /// </summary>
    [JsonIgnore]
    public WaveSettings ScoutWaveSettings { get; set; } = WaveSettings.Scout_Easy;
    #endregion

    /// <summary>
    /// Constructor, we initialize some defaults here
    /// </summary>
    public Level(string tier)
    {
        Tier = tier;
        Settings = new LevelSettings(tier);

        GlobalWaveSettings = tier switch
        {
            "D" => GlobalWaveSettings.HighCap_30pts,
            "E" => GlobalWaveSettings.HighCap_35pts,

            _ => GlobalWaveSettings.Default
        };

        // --- Ideas for level specific items
        // ">... [static crackle]\r\n>... We search for the key?\r\n>... <size=200%><color=red>Time is running out!</color></size>\n"
        // ">... [coughing] This air is thick.\r\n>... Where is that repeller?\r\n>... <size=200%><color=red>We need it now!</color></size>"
        // ">... [beeping] Is that a motion scan?\r\n>... Keep eyes on that display.\r\n>... <size=200%><color=red>They move fast. Stay close.</color></size>",

        // We pick some
        var intel = Generator.Pick(new List<string>
        {
            // Base game messages
            ">... Shoot me then, 'cause I'm not going in there.\r\n>... Look, there's no time–\r\n>... [gunshot]\r\n>... <size=200%><color=red>Start scanning.</color></size>",
            ">... And... got it! That's all of them.\r\n>... Let's get that door open then.\r\n<size=200%><color=red>>... Ready?</color></size>",
            ">... Quiet now. <size=200%><color=red>They hear everything.</size></color>\r\n>... Turn off your damn light...\r\n>... There it is! You take that side and–\r\n>... [unintelligible]",

            // New messages
            ">... [whispering] Lights off, keep heads low.\r\n>... Do not wake them, ever.\r\n>... <size=200%><color=red>They can hear us.</color></size>",
            ">... [footsteps] Keep your eyes sharp.\r\n>... Any movement?\r\n>... <size=200%><color=red>Something is here. Stay low.</color></size>",
            ">... Watch that corridor.\r\n>... We lost contact before.\r\n>... <size=200%><color=red>Stay alert. They're ahead.</color></size>",
            ">... [gasp] It's so dark...\r\n>... Use that flashlight carefully.\r\n>... <size=200%><color=red>They hate bright light!</color></size>",
            ">... Wait, what's that noise?\r\n>... [low rumbling]\r\n>... <size=200%><color=red>Ready weapons. This could be bad.</color></size>",
            ">... Check that locker.\r\n>... See any ammo?\r\n>... <size=200%><color=red>We can't fight empty-handed!</color></size>",
            ">... [whispering] Keep formation.\r\n>... No sudden moves.\r\n>... <size=200%><color=red>They sense motion and sound.</color></size>",
            ">... [footsteps] Keep your eyes sharp.\r\n>... Any movement?\r\n>... <size=200%><color=red>Something is here. Stay low.</color></size>",
            ">... <size=200%><color=red>[footsteps]</color></size>\r\n>... They are close.\r\n>... Keep your weapons ready.",
            ">... <size=200%><color=red>We must hurry</color></size> or die.\r\n>... [low growl] Listen carefully.\r\n>... Wait for movement.",
            ">... Keep it quiet.\r\n>... They sense <size=200%><color=red>every sound</color></size> here.\r\n>... [heartbeat]",
            ">... [labored breathing] Keep scanning.\r\n>... They come from behind <size=200%><color=red>that door</color></size>.",
            ">... <size=200%><color=red>Hold</color></size> your fire!\r\n>... They're not awake yet.\r\n>... Stay behind cover.\r\n>... [faint clicking]",
            ">... This place stinks of decay.\r\n>... <size=200%><color=red>Don't breathe it in</color></size>.\r\n>... [gagging, coughing]",
            ">... [muffled screams]\r\n>... <size=200%><color=red>Open that locker</color></size>, now.\r\n>... We need gear, fast.",
            ">... <size=200%><color=red>Don't wake them!</color></size>\r\n>... Lights out, no chatter.\r\n>... [soft skittering]",
            ">... I see something shining.\r\n>... <size=200%><color=red>Check that panel</color></size> carefully.\r\n>... Might be our way out.",
            ">... This won't be easy.\r\n>... <size=200%><color=red>Load up</color></size> everything we have.\r\n>... We face them soon."
        })!;

        ElevatorDropWardenIntel.Add((0, intel));
    }

    /// <summary>
    /// Generates a random depth for the level based on the Tier
    /// </summary>
    private void GenerateDepth()
    {
        Depth = Tier switch
        {
            "A" => Generator.Between(420, 650),
            "B" => Generator.Between(600, 850),
            "C" => Generator.Between(800, 1000),
            "D" => Generator.Between(900, 1100),
            "E" => Generator.Between(950, 1500),
            _ => Depth
        };
    }

    /// <summary>
    ///
    /// </summary>
    public void MarkAsErrorAlarm()
    {
        ElevatorDropWardenIntel.Add((Generator.Between(1, 6), Generator.Draw(new List<string>
        {
            ">... That alarm started the moment we dropped.\r\n>... [static crackle]\r\n>... <size=200%><color=red>There's no way to turn it off!</color></size>",
            ">... [warning siren blares]\r\n>... Everything's already awake.\r\n>... <size=200%><color=red>We push forward regardless!</color></size>",
            ">... <size=200%><color=red>This alarm won't shut down!</color></size>\r\n>... The terminal is locked.\r\n>... We'll just have to fight through.",
            ">... The lights won't stop flashing.\r\n>... My head's pounding.\r\n>... <size=200%><color=red>It's an error we can't fix!</color></size>",
            ">... [heavy footsteps closing in]\r\n>... They know we're here.\r\n>... <size=200%><color=red>No silent approach now.</color></size>",
            ">... <size=200%><color=red>The alarm won't stop!</color></size>\r\n>... That means they won't stop coming.\r\n>... We must keep moving!",
            ">... The siren's at full volume.\r\n>... It's drawing them from everywhere!\r\n>... <size=200%><color=red>No way to cut power.</color></size>",
            ">... [console flickering]\r\n>... Everything's jammed.\r\n>... <size=200%><color=red>We can't override this alarm!</color></size>",
            ">... <size=200%><color=red>Speed is our only option!</color></size>\r\n>... The alarm is permanent.\r\n>... Let's not waste time here.",
            ">... [gunfire in the distance]\r\n>... They keep coming.\r\n>... <size=200%><color=red>Brace for constant assault!</color></size>",
            ">... The Warden must've locked it.\r\n>... <size=200%><color=red>There's no off switch now.</color></size>\r\n>... We get in, do the job, get out.",
            ">... <size=200%><color=red>This is madness!</color></size>\r\n>... That alarm is unstoppable.\r\n>... We'll be swarmed every minute.",
            ">... [metal clanking]\r\n>... No time to plan carefully.\r\n>... <size=200%><color=red>Just move and shoot!</color></size>",
            ">... <size=200%><color=red>We'll have to fight on the run!</color></size>\r\n>... Standing still is suicide.\r\n>... Keep each other covered!",
            ">... The alarm won't pause.\r\n>... [heavy breathing]\r\n>... <size=200%><color=red>We do the mission under fire!</color></size>",
            ">... [sensor reading spikes]\r\n>... More and more signals.\r\n>... <size=200%><color=red>No choice but to hold them off!</color></size>",
            ">... <size=200%><color=red>We can't silence the siren!</color></size>\r\n>... Maybe it's intentional.\r\n>... The Warden wants us in chaos.",
            ">... Keep reloading on the move.\r\n>... <size=200%><color=red>We won't find any quiet corners!</color></size>\r\n>... The alarm reaches everywhere.",
            ">... [desperate breathing]\r\n>... It's an endless onslaught.\r\n>... <size=200%><color=red>Stay alive; there's no shutoff!</color></size>",
            ">... <size=200%><color=red>Eyes up, stay mobile!</color></size>\r\n>... This error alarm never ends.\r\n>... We finish or we die trying."
        }))!);
    }

    /// <summary>
    /// Marks the level as having a boss error alarm in it
    /// </summary>
    public void MarkAsBossErrorAlarm()
    {
        ElevatorDropWardenIntel.Add((Generator.Between(5, 12), Generator.Draw(new List<string>
        {
            ">... [distant rumbling]\r\n>... Feels like something massive is nearby.\r\n>... <size=200%><color=red>We can't face it unprepared!</color></size>",
            ">... There's a lull right now.\r\n>... Could be gathering strength.\r\n>... <size=200%><color=red>When it comes, be ready.</color></size>",
            ">... [ominous vibration]\r\n>... That alarm won't hush.\r\n>... <size=200%><color=red>A greater threat stirs in the dark!</color></size>",
            ">... <size=200%><color=red>Hold your breath!</color></size>\r\n>... Something big roams these halls.\r\n>... We only have minutes to prepare.",
            ">... [faint roar in distance]\r\n>... Everyone felt that, right?\r\n>... <size=200%><color=red>It's heading our way eventually!</color></size>",
            ">... The alarm's quiet... for now.\r\n>... But that won't last long.\r\n>... <size=200%><color=red>It always returns, bigger each time!</color></size>",
            ">... <size=200%><color=red>Whatever that thing is...</color></size>\r\n>... We heard it tearing steel.\r\n>... Pray we're not next.",
            ">... [slow metallic scrape]\r\n>... It's out there, hunting.\r\n>... <size=200%><color=red>We must fortify while we can!</color></size>",
            ">... This alarm doesn't wake sleepers.\r\n>... It's more... selective.\r\n>... <size=200%><color=red>And far more dangerous.</color></size>",
            ">... <size=200%><color=red>Everyone stay quiet!</color></size>\r\n>... That presence won't be fooled easily.\r\n>... We have a small window, mere minutes.",
            ">... [thudding footsteps echo]\r\n>... It's heavier than any normal foe.\r\n>... <size=200%><color=red>We can't fight carelessly!</color></size>",
            ">... The alarm intervals are longer.\r\n>... But each time, it returns.\r\n>... <size=200%><color=red>Ready or not, it's coming.</color></size>",
            ">... <size=200%><color=red>Something huge is stirring!</color></size>\r\n>... My gut tells me it's unstoppable.\r\n>... We have to work fast.",
            ">... [nervous shuffling]\r\n>... The last one nearly crushed us.\r\n>... <size=200%><color=red>Next time, no mistakes!</color></size>",
            ">... <size=200%><color=red>Hurry!</color></size>\r\n>... We only have moments before it shows.\r\n>... Gather ammo and regroup!",
            ">... It's an odd alarm cycle.\r\n>... Brings forth something massive.\r\n>... <size=200%><color=red>We can't outrun it forever!</color></size>",
            ">... [deep growl resonates]\r\n>... It's marking its territory.\r\n>... <size=200%><color=red>We are trespassing!</color></size>",
            ">... <size=200%><color=red>That howl again...</color></size>\r\n>... Means the next round is near.\r\n>... We have to brace ourselves.",
            ">... The floor vibrated under its weight.\r\n>... It's biding time.\r\n>... <size=200%><color=red>We must strike first, or hide!</color></size>",
            ">... [alarm hum fades, then restarts]\r\n>... This cycle is never-ending.\r\n>... <size=200%><color=red>Each time, a greater terror arrives!</color></size>"
        }))!);
    }

    /// <summary>
    /// Marks the level as starting the players at (usually) max infection
    /// </summary>
    public void MarkAsStartingInfected()
    {
        ElevatorDropWardenIntel.Add((Generator.Between(8, 14), Generator.Draw(new List<string>
        {
            ">... [violent coughing]\r\n>... The elevator air was wrong.\r\n>... <size=200%><color=red>We're already infected!</color></size>",
            ">... My veins are burning.\r\n>... [labored breathing]\r\n>... <size=200%><color=red>The infection is inside us!</color></size>",
            ">... <size=200%><color=red>Everyone's showing symptoms!</color></size>\r\n>... The Warden sent us down sick.\r\n>... Find disinfection, fast.",
            ">... [heartbeat pounding]\r\n>... My vision keeps blurring.\r\n>... <size=200%><color=red>We won't last long like this!</color></size>",
            ">... The cage was flooded on the way down.\r\n>... [wet coughing]\r\n>... <size=200%><color=red>It's already in our lungs!</color></size>",
            ">... <size=200%><color=red>Quarantine failure!</color></size>\r\n>... They dropped us anyway.\r\n>... Every step costs us now.",
            ">... I can barely stand.\r\n>... [ragged wheezing]\r\n>... <size=200%><color=red>We work sick or we don't work at all.</color></size>",
            ">... The others look grey.\r\n>... It's eating at all of us.\r\n>... <size=200%><color=red>No margin for mistakes down here!</color></size>",
            ">... [retching]\r\n>... Whatever was in that cage, we breathed it the whole way down.\r\n>... <size=200%><color=red>We start this one already dying!</color></size>",
            ">... Look at your hands. Look at them.\r\n>... <size=200%><color=red>The skin's gone the color of ash!</color></size>\r\n>... Mine too. All of us.",
            ">... The elevator seal never took, the whole way down.\r\n>... [filters hissing]\r\n>... <size=200%><color=red>Ten minutes of bad air, straight into us!</color></size>",
            ">... <size=200%><color=red>Find a disinfection station. Now.</color></size>\r\n>... Before we open a single door.\r\n>... I can hear myself rattling.",
            ">... <size=200%><color=red>My sight keeps swimming!</color></size>\r\n>... Shapes and shadows, I can't tell them apart.\r\n>... Don't stand in front of me.",
            ">... [teeth chattering]\r\n>... Fever's climbing. All of us.\r\n>... <size=200%><color=red>Every one of them hits twice as hard now!</color></size>",
            ">... Check your readout.\r\n>... <size=200%><color=red>We're in the red before we've even moved!</color></size>\r\n>... And it only goes one way from here.",
            ">... <size=200%><color=red>Spend the packs early!</color></size>\r\n>... Save them and you won't be standing to use them.\r\n>... Nothing gets better from here.",
            ">... [wheezing]\r\n>... Give me a second-\r\n>... <size=200%><color=red>There are no seconds. Move!</color></size>",
            ">... They dropped us in like this on purpose.\r\n>... <size=200%><color=red>It wanted us weak before the first door!</color></size>\r\n>... Then it gets exactly what it wanted.",
            ">... <size=200%><color=red>Nobody takes a hit. Nobody!</color></size>\r\n>... One clean strike and any of us drops.\r\n>... There's no cushion left in us.",
            ">... [gagging]\r\n>... <size=200%><color=red>Something's crawling under my ribs!</color></size>\r\n>... It's already working on us."
        }))!);
    }

    /// <summary>
    /// Marks the level as having whole-level cycling fog (failing ventilation)
    /// </summary>
    public void MarkAsCyclingFog()
    {
        ElevatorDropWardenIntel.Add((Generator.Between(5, 12), Generator.Draw(new List<string>
        {
            ">... Listen. The vents just died again.\r\n>... [deep mechanical groan]\r\n>... <size=200%><color=red>The fog keeps coming back!</color></size>",
            ">... It clears, then it rises.\r\n>... Like the sector is breathing.\r\n>... <size=200%><color=red>Move while it's low!</color></size>",
            ">... [fans winding down]\r\n>... Ventilation is on its last legs.\r\n>... <size=200%><color=red>Count the cycles or drown in it!</color></size>",
            ">... <size=200%><color=red>Here it comes again!</color></size>\r\n>... The whole floor fills up.\r\n>... Then it drains, like clockwork.",
            ">... The turbine by the elevator still works.\r\n>... Drag it with us.\r\n>... <size=200%><color=red>It's the only clear air we'll get!</color></size>",
            ">... [distant rush of air]\r\n>... The purge cycle keeps failing.\r\n>... <size=200%><color=red>We work in the gaps between!</color></size>",
            ">... Watch the low ground.\r\n>... It pools there first.\r\n>... <size=200%><color=red>When it rises, climb!</color></size>",
            ">... The system fights itself.\r\n>... Vents open, then choke shut.\r\n>... <size=200%><color=red>Time your push to the cycle!</color></size>",
            ">... [ducts rattling]\r\n>... The scrubbers cut out again.\r\n>... <size=200%><color=red>Get off the floor, now!</color></size>",
            ">... <size=200%><color=red>Count it! Learn the rhythm!</color></size>\r\n>... Up, then down, always the same length.\r\n>... Get it wrong and you're blind in the open.",
            ">... Grab every repeller we find.\r\n>... <size=200%><color=red>They only buy us one lungful each!</color></size>\r\n>... Better than nothing when the grey climbs.",
            ">... Don't cross the open floor when it's high.\r\n>... You'll lose sight of us in three steps.\r\n>... <size=200%><color=red>Wait for the drain!</color></size>",
            ">... <size=200%><color=red>Hold your breath and climb!</color></size>\r\n>... [gasping]\r\n>... The window gets shorter every time it comes back.",
            ">... Something's out there in the murk.\r\n>... <size=200%><color=red>It only moves when we can't see it!</color></size>\r\n>... And it's never where it was.",
            ">... The grey's over the railings now.\r\n>... Up the stairs, all of you.\r\n>... <size=200%><color=red>It fills from the bottom up!</color></size>",
            ">... <size=200%><color=red>Nobody splits up in the thick!</color></size>\r\n>... You lose a man in that and you don't find him.\r\n>... Stay close enough to grab an arm.",
            ">... [turbines coughing]\r\n>... <size=200%><color=red>That's the last of the airflow!</color></size>\r\n>... Whatever's in here with us stays in here.",
            ">... Feel that? The pressure drops just before it comes.\r\n>... That's your warning. Your only one.\r\n>... <size=200%><color=red>Watch the air, not the doors!</color></size>",
            ">... <size=200%><color=red>Don't reload in the middle of it!</color></size>\r\n>... You won't see what's on you until it's on you.\r\n>... Back out to clear air first.",
            ">... It burns going down, every cycle.\r\n>... <size=200%><color=red>We come out of this sicker than we went in!</color></size>\r\n>... Every rise takes another piece of us."
        }))!);
    }

    /// <summary>
    /// Marks the level as running the upkeep protocol (countdown fed by terminal overrides)
    /// </summary>
    public void MarkAsUpkeepProtocol()
    {
        ElevatorDropWardenIntel.Add((Generator.Between(5, 12), Generator.Draw(new List<string>
        {
            ">... The sector systems are failing.\r\n>... Some kind of maintenance countdown.\r\n>... <size=200%><color=red>Keep feeding it overrides!</color></size>",
            ">... [klaxon chirp]\r\n>... Admin access still works at the terminals.\r\n>... <size=200%><color=red>Buy time at every one we pass!</color></size>",
            ">... The credentials burn out after one use.\r\n>... Every terminal is a stay of execution.\r\n>... <size=200%><color=red>Don't walk past a single one!</color></size>",
            ">... <size=200%><color=red>The timer is already running!</color></size>\r\n>... When it lapses, they come in waves.\r\n>... Another override shuts them out.",
            ">... Upkeep protocol. Warden's own systems.\r\n>... [keys clattering]\r\n>... <size=200%><color=red>Type fast or fight!</color></size>",
            ">... It doesn't kill you when it runs out.\r\n>... It just opens the doors to them.\r\n>... <size=200%><color=red>Stay ahead of the countdown!</color></size>",
            ">... Maintenance windows, they called them.\r\n>... Miss one and the sector purges.\r\n>... <size=200%><color=red>ADMIN_TEMP_OVERRIDE. Remember it!</color></size>",
            ">... [alarm winding up]\r\n>... The ledger runs dry near the end.\r\n>... <size=200%><color=red>The last stretch is a sprint!</color></size>",
            ">... [console chirping]\r\n>... Every terminal we walk past is time we never get back.\r\n>... <size=200%><color=red>Sign in at every single one!</color></size>",
            ">... <size=200%><color=red>The count doesn't stop for us!</color></size>\r\n>... Not for reloads, not for the wounded.\r\n>... It just falls, and falls.",
            ">... [ticking]\r\n>... <size=200%><color=red>I can't stop watching that number!</color></size>\r\n>... It's the only thing down here still moving on schedule.",
            ">... ADMIN_TEMP_OVERRIDE takes once. Once.\r\n>... Then that terminal is dead to us forever.\r\n>... <size=200%><color=red>Don't burn one on a full clock!</color></size>",
            ">... <size=200%><color=red>Find the terminals before we push!</color></size>\r\n>... Know where the next one is before you need it.\r\n>... Running blind at zero is how this ends.",
            ">... [keys clacking]\r\n>... <size=200%><color=red>That was the last credential on this floor!</color></size>\r\n>... After this we're just running.",
            ">... What happens when it reaches zero?\r\n>... The sector cleans itself. We're what's dirty.\r\n>... <size=200%><color=red>The purge doesn't care that we're in here!</color></size>",
            ">... <size=200%><color=red>It lapsed! They're already coming!</color></size>\r\n>... I told you not to stop for the boxes.\r\n>... Now we pay for it.",
            ">... The deeper we went, the fewer terminals there were.\r\n>... <size=200%><color=red>At the end there's nothing left to feed it!</color></size>\r\n>... You just run and take what comes.",
            ">... Type it exactly. No mistakes.\r\n>... A fumbled line costs us a whole window.\r\n>... <size=200%><color=red>Slow hands get us killed!</color></size>",
            ">... The lights dipped for a second.\r\n>... <size=200%><color=red>That's the last warning we get!</color></size>\r\n>... Find the next terminal. Fast.",
            ">... <size=200%><color=red>We're on borrowed minutes down here!</color></size>\r\n>... Someone booked this sector for maintenance long before us.\r\n>... Nobody ever came to close it out."
        }))!);
    }

    /// <summary>
    /// Gets the right layer data given the objective being asked for
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public ObjectiveLayerData GetObjectiveLayerData(Bulkhead variant) =>
        variant switch
        {
            Bulkhead.Main => MainLayerData,
            Bulkhead.Extreme => SecondaryLayerData,
            Bulkhead.Overload => ThirdLayerData,
            _ => MainLayerData
        };

    /// <summary>
    /// Places default bulkhead keys if no keys have been placed already by layout builders.
    /// </summary>
    public void PlaceDefaultBulkheadKeys()
    {
        // Main always needs a key if there are any secondary bulkheads
        if ((HasExtreme || HasOverload) && MainLayerData.BulkheadKeyPlacements.Count == 0)
        {
            var candidates = GetKeyPlacementCandidates(Bulkhead.Main);
            MainLayerData.BulkheadKeyPlacements.Add(BuildKeyAlternatives(candidates));
        }

        switch (Settings.BulkheadStrategy)
        {
            case BukheadStrategy.SingleChain:
                // Chain: Main → Extreme → Overload. No main bulkhead door, so
                // only the Extreme key is needed (to gate Overload).
                if (HasExtreme && SecondaryLayerData.BulkheadKeyPlacements.Count == 0)
                {
                    var candidates = GetKeyPlacementCandidates(Bulkhead.Overload);
                    SecondaryLayerData.BulkheadKeyPlacements.Add(BuildKeyAlternatives(candidates));
                }
                break;

            case BukheadStrategy.Default_NoMainBulkhead:
                // No main bulkhead. With 3 bulkheads, randomly pick which secondary
                // layer gets a key. With 2 bulkheads, no secondary key needed.
                if (HasExtreme && HasOverload)
                {
                    if (Generator.Flip(0.5))
                    {
                        if (SecondaryLayerData.BulkheadKeyPlacements.Count == 0)
                        {
                            var candidates = GetKeyPlacementCandidates(Bulkhead.Extreme);
                            SecondaryLayerData.BulkheadKeyPlacements.Add(BuildKeyAlternatives(candidates));
                        }
                    }
                    else
                    {
                        if (ThirdLayerData.BulkheadKeyPlacements.Count == 0)
                        {
                            var candidates = GetKeyPlacementCandidates(Bulkhead.Overload);
                            ThirdLayerData.BulkheadKeyPlacements.Add(BuildKeyAlternatives(candidates));
                        }
                    }
                }
                break;

            default:
                // Standard strategies: place keys for all secondary layers
                if (HasExtreme && SecondaryLayerData.BulkheadKeyPlacements.Count == 0)
                {
                    var candidates = GetKeyPlacementCandidates(Bulkhead.Extreme);
                    SecondaryLayerData.BulkheadKeyPlacements.Add(BuildKeyAlternatives(candidates));
                }

                if (HasOverload && ThirdLayerData.BulkheadKeyPlacements.Count == 0)
                {
                    var candidates = GetKeyPlacementCandidates(Bulkhead.Overload);
                    ThirdLayerData.BulkheadKeyPlacements.Add(BuildKeyAlternatives(candidates));
                }
                break;
        }
    }

    private List<ZoneNode> GetKeyPlacementCandidates(Bulkhead keyFor)
    {
        var candidates = keyFor switch
        {
            Bulkhead.Main when HasMainBulkheadDoor
                => Planner.GetZones(Bulkhead.StartingArea, null),
            Bulkhead.Main
                => Planner.GetZones(Bulkhead.Main, null),
            Bulkhead.Extreme
                => Planner.GetZones(Bulkhead.Main, null),
            Bulkhead.Overload when Settings.BulkheadStrategy is BukheadStrategy.SingleChain
                => Planner.GetZones(Bulkhead.Extreme, null),
            Bulkhead.Overload
                => Planner.GetZones(Bulkhead.Main, null),
            _ => Planner.GetZones(Bulkhead.Main, null)
        };

        return candidates
            .Where(n => !n.Tags.Contains("no_access"))
            .ToList();
    }

    private List<ZonePlacementData> BuildKeyAlternatives(List<ZoneNode> candidates)
    {
        if (candidates.Count == 0)
            return new List<ZonePlacementData>
            {
                new() { Dimension = DimensionIndex.Reality, LocalIndex = 0, Weights = ZonePlacementWeights.NotAtStart }
            };

        if (candidates.Count <= 3)
            return candidates.Select(c => new ZonePlacementData
            {
                Dimension = c.Dimension,
                LocalIndex = c.ZoneNumber,
                Weights = ZonePlacementWeights.EvenlyDistributed
            }).ToList();

        var third = candidates.Count / 3;
        var early = candidates.Take(third).ToList();
        var mid = candidates.Skip(third).Take(third).ToList();
        var deep = candidates.Skip(third * 2).ToList();

        ZonePlacementData FromNode(ZoneNode n, ZonePlacementWeights w) =>
            new() { Dimension = n.Dimension, LocalIndex = n.ZoneNumber, Weights = w };

        var alternatives = Tier switch
        {
            "A" or "B" => new List<ZonePlacementData>
            {
                FromNode(Generator.Pick(early)!, ZonePlacementWeights.EvenlyDistributed),
                FromNode(Generator.Pick(early.Concat(mid).ToList())!, ZonePlacementWeights.NotAtStart)
            },
            "C" => new List<ZonePlacementData>
            {
                FromNode(Generator.Pick(early)!, ZonePlacementWeights.NotAtStart),
                FromNode(Generator.Pick(mid)!, ZonePlacementWeights.NotAtStart),
                FromNode(Generator.Pick(deep)!, ZonePlacementWeights.NotAtStart)
            },
            "D" => new List<ZonePlacementData>
            {
                FromNode(Generator.Pick(mid)!, ZonePlacementWeights.NotAtStart),
                FromNode(Generator.Pick(deep)!, ZonePlacementWeights.AtEnd),
                FromNode(Generator.Pick(mid.Concat(deep).ToList())!, ZonePlacementWeights.NotAtStart)
            },
            _ => new List<ZonePlacementData>
            {
                FromNode(Generator.Pick(deep)!, ZonePlacementWeights.NotAtStart),
                FromNode(Generator.Pick(mid.Concat(deep).ToList())!, ZonePlacementWeights.AtEnd),
                FromNode(Generator.Pick(deep)!, ZonePlacementWeights.AtEnd)
            }
        };

        return alternatives
            .GroupBy(a => a.LocalIndex)
            .Select(g => g.First())
            .ToList();
    }

    /// <summary>
    /// Places a bulkhead key in the specified zone. The bulkhead layer is
    /// inferred from the node's Bulkhead property.
    /// </summary>
    public void PlaceBulkheadKey(ZoneNode node, ZonePlacementWeights? weights = null)
    {
        var layerData = GetObjectiveLayerData(node.Bulkhead);
        layerData.BulkheadKeyPlacements.Add(
            new List<ZonePlacementData>
            {
                new()
                {
                    Dimension = node.Dimension,
                    LocalIndex = node.ZoneNumber,
                    Weights = weights ?? ZonePlacementWeights.NotAtStart
                }
            });
    }

    public WardenObjective GetObjective(Bulkhead variant)
    {
        if (variant.HasFlag(Bulkhead.Main))
            return Objective[Bulkhead.Main];

        return variant is Bulkhead.Extreme or Bulkhead.Overload ? Objective[variant] : Objective[Bulkhead.Main];
    }

    public LevelLayout? GetLevelLayout(Bulkhead variant) =>
        variant switch
        {
            Bulkhead.Main or (Bulkhead.Main | Bulkhead.StartingArea) => Bins.LevelLayouts.Find(LevelLayoutData),
            Bulkhead.Extreme => Bins.LevelLayouts.Find(SecondaryLayout),
            Bulkhead.Overload => Bins.LevelLayouts.Find(ThirdLayout),

            _ => Bins.LevelLayouts.Find(LevelLayoutData)
        };

    public void SetDimensionLayout(DimensionIndex dimension, Bulkhead bulkhead, LevelLayout layout)
    {
        if (!DimensionLayouts.TryGetValue(dimension, out var layouts))
        {
            layouts = new Dictionary<Bulkhead, LevelLayout>();
            DimensionLayouts[dimension] = layouts;
        }
        layouts[bulkhead] = layout;
    }

    public LevelLayout? GetDimensionLayout(DimensionIndex dimension, Bulkhead bulkhead)
    {
        if (dimension == DimensionIndex.Reality)
            return GetLevelLayout(bulkhead);
        return DimensionLayouts.TryGetValue(dimension, out var layouts)
            && layouts.TryGetValue(bulkhead, out var layout) ? layout : null;
    }

    /// <summary>
    /// Returns all layouts (Reality + all dimensions) for iteration
    /// </summary>
    public IEnumerable<(Bulkhead Bulkhead, LevelLayout Layout)> GetAllLayouts()
    {
        foreach (var kvp in Layouts)
            yield return (kvp.Key, kvp.Value);

        foreach (var dimEntry in DimensionLayouts)
            foreach (var layoutEntry in dimEntry.Value)
                yield return (layoutEntry.Key, layoutEntry.Value);
    }

    /// <summary>
    /// Prebuild one of the layouts, this is needed for setting up the objectives which is then used for level
    /// generation across all the other layouts
    /// </summary>
    /// <param name="bulkhead"></param>
    private void PreBuildObjective(Bulkhead bulkhead)
    {
        var existing = new List<WardenObjectiveType>();

        if (Director.ContainsKey(Bulkhead.Main))
            existing.Add(Director[Bulkhead.Main].Objective);
        if (Director.ContainsKey(Bulkhead.Extreme))
            existing.Add(Director[Bulkhead.Extreme].Objective);
        if (Director.ContainsKey(Bulkhead.Overload))
            existing.Add(Director[Bulkhead.Overload].Objective);

        // All objectives that make use of the timer can't work together
        if (existing.Contains(WardenObjectiveType.ReactorStartup) ||
            existing.Contains(WardenObjectiveType.Survival) ||
            existing.Contains(WardenObjectiveType.TimedTerminalSequence))
        {
            existing.Add(WardenObjectiveType.ReactorStartup);
            existing.Add(WardenObjectiveType.Survival);
            existing.Add(WardenObjectiveType.TimedTerminalSequence);
        }

        // Exclude central generator cluster if there's other fog changing happening
        if (FogUsage != FogUsage.None)
        {
            existing.Add(WardenObjectiveType.CentralGeneratorCluster);
        }

        // Allow multiple instances of these objectives
        existing.Remove(WardenObjectiveType.GatherSmallItems);

        if (!Director.ContainsKey(bulkhead))
        {
            Director[bulkhead] = new BuildDirector
            {
                Bulkhead = bulkhead,
                Complex = Complex,
                Complexity = Complexity.Low,
                Settings = Settings,
                Tier = Tier
            };

            Director[bulkhead].GenObjective(existing);
        }

        var director = Director[bulkhead];
        director.GenPoints();

        // Assign these values to make sure they're all the same
        director.Complex = Complex;
        director.Settings = Settings;

        var objective = WardenObjective.PreBuild(director, this);

        Objective[bulkhead] = objective;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bulkhead"></param>
    private void SelectDirection(Bulkhead bulkhead)
    {
        var direction = RelativeDirection.Global_Forward;

        // We assume this must be called first so we can manipulate the list of directions
        if (bulkhead == Bulkhead.Main)
        {
            RelativeDirections = new List<RelativeDirection>
            {
                RelativeDirection.Global_Forward,
                RelativeDirection.Global_Left,
                RelativeDirection.Global_Right
            };

            direction = Generator.Draw(RelativeDirections);

            // Allow global backwards to be selected by another bulkhead
            RelativeDirections.Add(RelativeDirection.Global_Backward);
        }
        else if (bulkhead == Bulkhead.Extreme || bulkhead == Bulkhead.Overload)
        {
            // Set the single chain method to branch the overload bulkhead forwards as well
            if (Settings.BulkheadStrategy == BukheadStrategy.SingleChain && bulkhead == Bulkhead.Overload)
                direction = Settings.GetDirections(Bulkhead.Main);
            else
                direction = Generator.Draw(RelativeDirections);
        }

        Settings.SetDirections(bulkhead, direction);
    }

    /// <summary>
    /// Options for bulkhead keys and bulkhead DCs:
    ///
    /// </summary>
    private void BuildBulkheads()
    {
        // Randomly select which bulkheads to use
        if (!IsTest)
            Settings.Bulkheads = Generator.Select(BulkheadChanceTable);

        // Options for starting areas
        var options = Settings.Bulkheads switch
        {
            Bulkhead.Main => new List<(double, BukheadStrategy)>
            {
                (1.0, BukheadStrategy.MainOnly_NoBulkhead)
            },
            Bulkhead.Main | Bulkhead.Extreme => new List<(double, BukheadStrategy)>
            {
                (0.35, BukheadStrategy.Default),
                (0.30, BukheadStrategy.Default_NoMainBulkhead),
                (0.35, BukheadStrategy.CentralHub_x2)
            },
            Bulkhead.Main | Bulkhead.Overload => new List<(double, BukheadStrategy)>
            {
                (0.35, BukheadStrategy.Default),
                (0.30, BukheadStrategy.Default_NoMainBulkhead),
                (0.35, BukheadStrategy.CentralHub_x2)
            },
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload => new List<(double, BukheadStrategy)>
            {
                (0.05, BukheadStrategy.Default),
                (0.05, BukheadStrategy.Default_NoMainBulkhead),
                (0.20, BukheadStrategy.CentralHub_x3),
                (0.70, BukheadStrategy.SingleChain)
            },

            _ => throw new ArgumentOutOfRangeException()
        };

        Settings.BulkheadStrategy = Generator.Select(options);
        Plugin.Logger.LogDebug($"StartingArea strategy = {Settings.BulkheadStrategy}");

        /*
         * We must select the relative directions we want to try and build each of the bulkhead
         * zones upfront as it has an impact on the other areas of level generation.
         * */
        SelectDirection(Bulkhead.Main);

        if (HasExtreme)
            SelectDirection(Bulkhead.Extreme);

        if (HasOverload)
            SelectDirection(Bulkhead.Overload);
    }

    /// <summary>
    /// Purely RAM/memory optimization step. This prunes any unneeded custom geos from the custom
    /// geo list so they are not loaded by the game for this level. This saves around 1-2gb of
    /// ram for people.
    /// </summary>
    private void FinalizeComplexResourceSet()
    {
        // If the resource set was already customized (e.g. by Cryptomnesia to pin the
        // elevator geo), keep it. Otherwise clone from the base complex resource set.
        var isCustom = ResourceSet.PersistentId != ComplexResourceSet.Mining.PersistentId
                    && ResourceSet.PersistentId != ComplexResourceSet.Tech.PersistentId
                    && ResourceSet.PersistentId != ComplexResourceSet.Service.PersistentId;

        if (!isCustom)
        {
            ResourceSet = Complex switch
            {
                Complex.Mining => ComplexResourceSet.Mining,
                Complex.Tech => ComplexResourceSet.Tech,
                Complex.Service when PreferredSubComplex == SubComplex.Gardens
                    => ComplexResourceSet.ServiceGardens,
                Complex.Service => ComplexResourceSet.Service,
            };

            ResourceSet = ResourceSet.Duplicate();
        }

        ResourceSet.BlockName = $"{ResourceSet.BlockName}_{Tier}{Index}_{Filesystem.Filename(Name)}";

        var usedCustomGeos = new HashSet<string>();

        foreach (var (_, layout) in GetAllLayouts())
            foreach (var zone in layout.Zones)
                if (zone.CustomGeomorph is not null)
                    usedCustomGeos.Add(zone.CustomGeomorph);

        // Keep any custom geos used as dimension origin tiles
        foreach (var dimData in DimensionDatas)
            usedCustomGeos.Add(dimData.Data.Data.DimensionGeomorph);

        ValidateForcedGeomorphs(usedCustomGeos);

        ResourceSet.CustomGeomorphs.RemoveAll(prefab => !usedCustomGeos.Contains(prefab.Asset));
    }

    /// <summary>
    /// A forced tile (Zone.CustomGeomorph) is resolved by path through
    /// ComplexResourceSetDataBlock.GetCustomGeomorph, which only searches the three
    /// CustomGeomorphs_{Exit,Objective,Challenge}_1x1 lists -- never GeomorphTiles_1x1. So a
    /// perfectly ordinary base game tile that only lives in the random pool resolves to null
    /// when forced.
    ///
    /// The failure mode is brutal and six steps removed from the cause: null tile prefab ->
    /// LG_Floor.FindExternalArea NRE -> zone completes with 0 areas -> reroll -> the rebuilt
    /// factory drains every remaining batch empty -> no zones and no AI graph. Catch it here,
    /// where we already have every forced path in hand.
    /// </summary>
    private void ValidateForcedGeomorphs(HashSet<string> usedCustomGeos)
    {
        var registered = ResourceSet.CustomGeomorphs
            .Concat(ResourceSet.CustomGeomorphs_Exit_1x1)
            .Concat(ResourceSet.CustomGeomorphs_Challenge_1x1)
            .Select(prefab => prefab.Asset)
            .ToHashSet();

        foreach (var geo in usedCustomGeos.Where(geo => !string.IsNullOrEmpty(geo) && !registered.Contains(geo)))
            Plugin.Logger.LogError(
                $"{Tier}{Index} \"{Name}\" ({Complex}) forces geomorph \"{geo}\" but it is not " +
                $"registered as a custom geomorph in ComplexResourceSet.SaveStatic(). The game " +
                $"resolves forced tiles by path against the CustomGeomorphs_* lists only, so " +
                $"this will fail level generation. Add it to {Complex}.CustomGeomorphs.");
    }

    /// <summary>
    /// Saves all of the EOS definitions
    /// </summary>
    private void FinalizeCustomMods()
    {
        /*
         * We need to make sure the ExtraObjectiveSetup layout definitions are set up with the
         * correct main level layout persistent id and that they are saved if we added any
         * definitions to them.
         */

        var fsName = Regex.Replace(
            Name,
            @"<color(\s*=\s*[^>]+)?>|</color>|<s>|</s>|/|\?|!",
            string.Empty,
            RegexOptions.IgnoreCase);

        LogArchives.Name = $"{Tier}{Index}_{fsName}";
        LogArchives.MainLevelLayout = LevelLayoutData;

        SecurityDoors.Name = $"{Tier}{Index}_{fsName}";
        SecurityDoors.MainLevelLayout = LevelLayoutData;

        TerminalPlacements.Name = $"{Tier}{Index}_{fsName}";
        TerminalPlacements.MainLevelLayout = LevelLayoutData;

        AreaCounts.Name = $"{Tier}{Index}_{fsName}";
        AreaCounts.MainLevelLayout = LevelLayoutData;

        EOS_EventsOnBossDeath.Name = $"{Tier}{Index}_{fsName}";
        EOS_EventsOnBossDeath.MainLevelLayout = LevelLayoutData;

        EOS_EventsOnScoutScream.Name = $"{Tier}{Index}_{fsName}";
        EOS_EventsOnScoutScream.MainLevelLayout = LevelLayoutData;

        EOS_IndividualGenerator.Name = $"{Tier}{Index}_{fsName}";
        EOS_IndividualGenerator.MainLevelLayout = LevelLayoutData;

        EOS_ReactorShutdown.Name = $"{Tier}{Index}_{fsName}";
        EOS_ReactorShutdown.MainLevelLayout = LevelLayoutData;

        EOS_SecuritySensor.Name = $"{Tier}{Index}_{fsName}";
        EOS_SecuritySensor.MainLevelLayout = LevelLayoutData;

        SecurityDoors.Save();
        TerminalPlacements.Save();
        AreaCounts.Save();

        if (EOS_EventsOnBossDeath.Definitions.Any())
            EOS_EventsOnBossDeath.Save();

        if (EOS_EventsOnScoutScream.Definitions.Any())
            EOS_EventsOnScoutScream.Save();

        if (EOS_IndividualGenerator.Definitions.Any())
            EOS_IndividualGenerator.Save();

        if (EOS_ReactorShutdown.Definitions.Any())
            EOS_ReactorShutdown.Save();

        if (EOS_SecuritySensor.Definitions.Any())
            EOS_SecuritySensor.Save();

        // Save zone sensors to JSON for runtime loading
        if (ZoneSensors.Any())
        {
            var levelZoneSensors = new LevelZoneSensors
            {
                Name = $"{Tier}{Index}_{fsName}",
                MainLevelLayout = LevelLayoutData,
                Definitions = ZoneSensors
            };
            levelZoneSensors.Save();
        }

        // Save custom terminal spawn requests to JSON for runtime loading
        SaveCustomTerminals();
    }

    /// <summary>
    /// Saves custom terminal spawn requests to JSON for runtime loading. Called during
    /// FinalizeCustomMods() and again after D-Lock log distribution, which mutates the
    /// requests' LogFiles after the first save has already been written.
    /// </summary>
    public void SaveCustomTerminals()
    {
        var fsName = Regex.Replace(
            Name,
            @"<color(\s*=\s*[^>]+)?>|</color>|<s>|</s>|/|\?|!",
            string.Empty,
            RegexOptions.IgnoreCase);

        var customTerminalRequests = CustomTerminalSpawnManager.GetRequests(LevelLayoutData);
        if (customTerminalRequests.Any())
        {
            var levelCustomTerminals = new LevelCustomTerminals
            {
                Name = $"{Tier}{Index}_{fsName}",
                MainLevelLayout = LevelLayoutData,
                Requests = customTerminalRequests
            };
            levelCustomTerminals.Save();
        }
    }

    /// <summary>
    /// Sets the default scout wave screaming behavior. Other layouts can override this with more
    /// specific settings in the "EventsOnScoutScream" settings.
    /// </summary>
    private void BuildDefaultScoutWaves()
    {
        var population = WavePopulation.Baseline;
        var settings = WaveSettings.Scout_Easy;

        if (Settings.Modifiers.Contains(LevelModifiers.Shadows))
            population = WavePopulation.Baseline_Shadows;
        else if (Settings.Modifiers.Contains(LevelModifiers.ManyShadows))
            population = WavePopulation.OnlyShadows;
        else if (Settings.Modifiers.Contains(LevelModifiers.Chargers))
            population = WavePopulation.Baseline_Shadows;
        else if (Settings.Modifiers.Contains(LevelModifiers.ManyChargers))
            population = WavePopulation.OnlyShadows;
        else if (Settings.Modifiers.Contains(LevelModifiers.Nightmares))
            population = WavePopulation.Baseline_Nightmare;
        else if (Settings.Modifiers.Contains(LevelModifiers.OnlyNightmares))
            population = WavePopulation.OnlyNightmares;

        switch (Tier)
        {
            case "A":
            {
                settings = WaveSettings.Scout_Easy;
                break;
            }

            case "B":
            {
                (population, settings) = Generator.Select(
                    new List<(double, (WavePopulation, WaveSettings))>
                    {
                        (70, (population, WaveSettings.Scout_Normal)),
                        (15, (WavePopulation.OnlyHybrids, WaveSettings.SingleWave_MiniBoss_4pts)),
                        (15, (WavePopulation.OnlyInfestedStrikers, WaveSettings.Scout_Easy)),
                    });
                break;
            }

            case "C":
            {
                (population, settings) = Generator.Select(
                    new List<(double, (WavePopulation, WaveSettings))>
                    {
                        (45, (population, WaveSettings.Scout_Normal)),
                        (15, (WavePopulation.OnlyHybrids, WaveSettings.SingleWave_MiniBoss_6pts)),
                        (10, (WavePopulation.OnlyInfectedHybrids, WaveSettings.SingleWave_MiniBoss_6pts)),
                        (10, (WavePopulation.OnlyInfestedStrikers, WaveSettings.Scout_Normal)),
                        (10, (WavePopulation.OnlyNightmareGiants, WaveSettings.SingleWave_MiniBoss_4pts)),
                        ( 5, (WavePopulation.SingleEnemy_Mother, WaveSettings.SingleMiniBoss)),
                        ( 5, (WavePopulation.SingleEnemy_Tank, WaveSettings.SingleMiniBoss)),
                    });
                break;
            }

            case "D":
            {
                (population, settings) = Generator.Select(
                    new List<(double, (WavePopulation, WaveSettings))>
                    {
                        (30, (population, WaveSettings.Scout_Hard)),
                        (15, (WavePopulation.OnlyHybrids, WaveSettings.SingleWave_MiniBoss_8pts)),
                        (10, (WavePopulation.OnlyInfectedHybrids, WaveSettings.SingleWave_MiniBoss_8pts)),
                        (10, (WavePopulation.OnlyInfestedStrikers, WaveSettings.Scout_Hard)),
                        (10, (WavePopulation.OnlyNightmareGiants, WaveSettings.SingleWave_MiniBoss_6pts)),
                        ( 8, (WavePopulation.OnlyShadows, WaveSettings.SingleWave_MiniBoss_12pts)),
                        ( 7, (WavePopulation.SingleEnemy_Tank, WaveSettings.SingleMiniBoss)),
                        ( 5, (WavePopulation.SingleEnemy_Mother, WaveSettings.SingleMiniBoss)),
                        ( 5, (WavePopulation.SingleEnemy_TankPotato, WaveSettings.SingleMiniBoss)),
                    });
                break;
            }

            case "E":
            {
                (population, settings) = Generator.Select(
                    new List<(double, (WavePopulation, WaveSettings))>
                    {
                        (20, (population, WaveSettings.Scout_VeryHard)),
                        (15, (WavePopulation.OnlyHybrids, WaveSettings.SingleWave_MiniBoss_12pts)),
                        (10, (WavePopulation.OnlyInfectedHybrids, WaveSettings.SingleWave_MiniBoss_12pts)),
                        (10, (WavePopulation.OnlyInfestedStrikers, WaveSettings.Scout_VeryHard)),
                        (10, (WavePopulation.OnlyNightmareGiants, WaveSettings.SingleWave_MiniBoss_8pts)),
                        (10, (WavePopulation.OnlyShadows, WaveSettings.SingleWave_MiniBoss_16pts)),
                        (10, (WavePopulation.SingleEnemy_Tank, WaveSettings.SingleMiniBoss)),
                        ( 8, (WavePopulation.SingleEnemy_TankPotato, WaveSettings.SingleMiniBoss)),
                        ( 7, (WavePopulation.SingleEnemy_Mother, WaveSettings.SingleMiniBoss)),
                    });
                break;
            }
        }

        ScoutWavePopulation = population;
        ScoutWaveSettings = settings;

        // TODO: just doesn't quite seem to be working right. I think it's the target definition.
        // So what layer it's targeting
        // if (wave is not null)
        //     events.AddSpawnWave(wave);
        //
        // if (events.Any())
        //     EOS_EventsOnScoutScream.Definitions.Add(
        //         new EventsOnScoutScream
        //         {
        //             ZoneNumber = -1,
        //             Bulkhead = Bulkhead.All,
        //             DimensionIndex = null,
        //
        //             SuppressVanillaScoutWave = true,
        //             Events = events
        //         });
    }

    /// <summary>
    /// Builds a specific bulkhead layout
    /// </summary>
    /// <param name="bulkhead"></param>
    private void BuildLayout(Bulkhead bulkhead)
    {
        var director = Director[bulkhead];
        var objective = Objective[bulkhead];

        var layout = LevelLayout.Build(this, director, objective);
        Layouts[bulkhead] = layout;

        objective.Build(director, this);

        var layerData = ObjectiveLayer[bulkhead];
        layerData.ObjectiveData.DataBlockId = objective.PersistentId;

        Bins.WardenObjectives.AddBlock(objective);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public static Level Build(Level level)
    {
        level.Name = level.MainDirector.Objective switch
        {
            WardenObjectiveType.AlphaTerminalCommand => "Alpha One",
            WardenObjectiveType.Cryptomnesia => "Cryptomnesia",
            WardenObjectiveType.ReachKdsDeep => "Valiant",
            _ => level.Name
        };

        if (level.Name == "")
            level.Name = Generator.Pick(Words.NounsLevel) ?? "";

        var logLevelId = $"Level={level.Tier}{level.Index}";

        level.GenerateDepth();
        level.GenerateZoneAliasStarts();
        level.BuildBulkheads();

        #region Fog settings
        // The objectives here match ApplyLevelSignature's skip list. Demote the signature
        // to None so the level-wide consumers (error-alarm damp, apex/ClearPath boss
        // suppression, per-zone ammo bump) don't fire on a level with no signature
        // content. CyclingFog additionally re-rolls the standard E-tier fog modifier so
        // the level rejoins the normal fog distribution (its roll was skipped in
        // LevelSettings.Generate); CentralGeneratorCluster is excluded from the
        // Main-objective draw for cycling fog levels in RundownFactory.
        if (level.Settings.Signature != LevelSignature.None
            && level.MainDirector.Objective is WardenObjectiveType.Survival
                or WardenObjectiveType.ReachKdsDeep
                or WardenObjectiveType.Cryptomnesia)
        {
            if (level.Settings.Signature == LevelSignature.CyclingFog)
                level.Settings.Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.3, LevelModifiers.NoFog),
                        (0.5, LevelModifiers.Fog),
                        (0.2, LevelModifiers.HeavyFog),
                    }));

            Plugin.Logger.LogDebug(
                $"{logLevelId} -- Demoted {level.Settings.Signature} signature for main objective {level.MainDirector.Objective}");

            level.Settings.Signature = LevelSignature.None;
        }

        var lowFog = level.Settings.Modifiers.Contains(LevelModifiers.FogIsInfectious)
            ? Fog.LowFog_Infectious
            : Fog.LowFog;
        var lowMidFog = level.Settings.Modifiers.Contains(LevelModifiers.FogIsInfectious)
            ? Fog.LowMidFog_Infectious
            : Fog.LowMidFog;

        if (level.Settings.Signature == LevelSignature.CyclingFog)
        {
            // Drop state is the clear trough of the cycle; ApplyLevelSignature adds the
            // event loop that raises the first heavy phase after a grace period.
            level.FogSettings = level.Settings.Modifiers.Contains(LevelModifiers.FogIsInfectious)
                ? Fog.CyclingFog_Clear_Infectious
                : Fog.CyclingFog_Clear;

            // Reserve fog for the whole level: blocks fog-flood alarm rolls, objective
            // fog challenges, and CentralGeneratorCluster selection for side objectives.
            level.TrySetFogUsage(FogUsage.LongDuration);
        }
        else
        {
            // Randomize no fog to add variety
            if (level.Settings.Modifiers.Contains(LevelModifiers.NoFog))
            {
                level.FogSettings = Fog.Randomized();
                Plugin.Logger.LogWarning($"Settings for fog: density = {level.FogSettings.FogDensity}");
            }

            // Set low fog if we have fog
            if (level.Settings.Modifiers.Contains(LevelModifiers.Fog))
                level.FogSettings = lowFog;

            // For heavy fog we can also roll low mid fog
            if (level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
                level.FogSettings = Generator.Flip(0.75) ? lowFog : lowMidFog;
        }
        #endregion

        Plugin.Logger.LogDebug($"{logLevelId} ({level.Complex}) - Modifiers: {level.Settings.Modifiers}, Fog: {level.FogSettings.Name}");

        #region Objective prebuild
        /* We prebuild the objectives as certain objectives have components that affect level
         * generation. For example the "distribute cells to generator cluster" objective
         * requires that the level generate enough generators for each of the cells to be
         * distributed to. */
        level.PreBuildObjective(Bulkhead.Main);

        if (level.HasExtreme)
            level.PreBuildObjective(Bulkhead.Extreme);

        if (level.HasOverload)
            level.PreBuildObjective(Bulkhead.Overload);
        #endregion

        #region Signature vs. objective wave-stop conflicts
        // These objectives fire identifier-less (global) StopEnemyWaves events mid-level
        // — terminal command events, portal warps, or the vanilla uplink-completion stop
        // that Patch_UplinkWaveIsolation can fall back to — which would silently kill the
        // BossAlarm signature's untagged boss stream. Secondary/overload objectives are
        // only drawn during the prebuild above, so this check has to run here rather than
        // with the Main-objective demotion. Re-roll to a signature that survives a global
        // stop; CyclingFog is not a candidate because its fog-roll skip and
        // CentralGeneratorCluster draw exclusion have already happened.
        if (level.Settings.Signature == LevelSignature.BossAlarm
            && level.Director.Values.Any(d => d.Objective
                is WardenObjectiveType.AlphaTerminalCommand
                or WardenObjectiveType.TimedTerminalSequence
                or WardenObjectiveType.TerminalUplink
                or WardenObjectiveType.CorruptedTerminalUplink))
        {
            level.Settings.Signature = Generator.Select(new List<(double, LevelSignature)>
            {
                (1.0, LevelSignature.Stalker),
                (1.0, LevelSignature.StartWithInfection)
            });

            // StartWithInfection normally forces an infection modifier during the E-tier
            // roll in LevelSettings.Generate; mirror that for a late re-roll. FogSettings
            // are already assigned above, so FogIsInfectious is deliberately not added.
            if (level.Settings.Signature == LevelSignature.StartWithInfection
                && !level.Settings.HasInfection)
                level.Settings.Modifiers.Add(Generator.Flip(0.6)
                    ? LevelModifiers.HeavyInfection
                    : LevelModifiers.Infection);

            Plugin.Logger.LogDebug(
                $"{logLevelId} -- Re-rolled BossAlarm signature to {level.Settings.Signature} " +
                "due to an objective with a global wave stop");
        }

        // Reactors have no terminal in the reactor zone plus ~10 minute stationary
        // phases, and TimedTerminalSequence has long stationary rounds — on any bulkhead
        // they starve the upkeep-override time economy. Re-roll (never demote: E-tier
        // keeps 100% signature incidence). The tagged surge stream survives global wave
        // stops, so Alpha/uplink objectives need no exclusion here.
        if (level.Settings.Signature == LevelSignature.UpkeepProtocol
            && level.Director.Values.Any(d => d.Objective
                is WardenObjectiveType.ReactorStartup
                or WardenObjectiveType.ReactorShutdown
                or WardenObjectiveType.TimedTerminalSequence))
        {
            level.Settings.Signature = Generator.Select(new List<(double, LevelSignature)>
            {
                (1.0, LevelSignature.Stalker),
                (1.0, LevelSignature.StartWithInfection)
            });

            // Mirror the E-tier StartWithInfection infection-modifier bias, as above.
            if (level.Settings.Signature == LevelSignature.StartWithInfection
                && !level.Settings.HasInfection)
                level.Settings.Modifiers.Add(Generator.Flip(0.6)
                    ? LevelModifiers.HeavyInfection
                    : LevelModifiers.Infection);

            Plugin.Logger.LogDebug(
                $"{logLevelId} -- Re-rolled UpkeepProtocol signature to {level.Settings.Signature} " +
                "due to an objective with long stationary phases");
        }
        #endregion

        #region Scout Waves
        level.BuildDefaultScoutWaves();
        #endregion

        #region Layout generation
        /*
         * Here we go ahead and generate the level and zones. We want to start with main first,
         * and then go with extreme -> overload.
         *
         * TODO: for now bulkhead placement is always at the start of main.
         * In the future we will want to look at placing the extreme / overload bulkheads
         * within each other and main, instead of all in the starting area.
         * */
        level.BuildLayout(Bulkhead.Main);

        if (level.HasExtreme)
            level.BuildLayout(Bulkhead.Extreme);

        if (level.HasOverload)
            level.BuildLayout(Bulkhead.Overload);
        #endregion

        #region Bulkhead Keys
        level.PlaceDefaultBulkheadKeys();
        #endregion

        #region Finalize -- WardenObjective.PostBuild()
        level.GetObjective(Bulkhead.Main)!.PostBuild(level.MainDirector, level);

        if (level.HasExtreme && level.GetObjective(Bulkhead.Extreme) != null)
            level.GetObjective(Bulkhead.Extreme)!.PostBuild(level.SecondaryDirector, level);

        if (level.HasOverload && level.GetObjective(Bulkhead.Overload) != null)
            level.GetObjective(Bulkhead.Overload)!.PostBuild(level.OverloadDirector, level);
        #endregion

        #region Finalize -- Level.PostBuild()
        // Runs after every bulkhead's FinalizeLayout (alarms/enemies rolled, so
        // clear-time estimates are valid for all zones) and after objective PostBuild
        // (which can add terminal placements). Zones serialize at bin save, so late
        // mutation here is safe.
        level.ApplyUpkeepProtocol();
        #endregion

        #region Finalize -- ExtraObjectiveSetup
        level.FinalizeCustomMods();
        #endregion

        #region Finalize -- Zone numbers & Extraction Intel

        level.RecalculateZoneAliasStarts();
        level.Objective[Bulkhead.Main].PostBuild_ForwardExtract(level);

        #endregion

        #region Finalize -- Complex Resource Set
        level.FinalizeComplexResourceSet();
        #endregion

        Plugin.Logger.LogDebug(
            $"Level={level.Tier}{level.Index} level plan: {level.Planner}\n" +
            $"==========\n{level.Planner.ToMermaidChart()}==========");

        return level;
    }

    /// <summary>
    /// Test level construction for testing out new geos
    /// </summary>
    /// <param name="geo"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static Level Debug_BuildGeoTest(string? geo, Level level, int forwardZones = 0)
    {
        try
        {
            level.GenerateDepth();
            level.GenerateZoneAliasStarts();

            #region Level.Build()

            // level.PreBuildObjective(Bulkhead.Main);
            var director = new BuildDirector
            {
                Bulkhead = Bulkhead.Main,
                Complex = level.Complex,
                Complexity = Complexity.Low,
                Objective = WardenObjectiveType.SpecialTerminalCommand,
                Settings = level.Settings,
                Tier = level.Tier
            };
            var objective = WardenObjective.PreBuild(director, level);

            objective.EventsOnElevatorLand.AddSound(Sound.Woooo_Machine1);

            level.Director[Bulkhead.Main] = director;
            level.Objective[Bulkhead.Main] = objective;

            //level.GetObjective(Bulkhead.Main)!.PostBuild(level.MainDirector, level);

            #endregion

            #region LevelLayout.Build()

            // var layout = LevelLayout.Build(this, director, objective, direction);
            var layout = new LevelLayout(level, director, objective, level.Settings, level.Planner);

            level.Layouts[Bulkhead.Main] = layout;

            // objective.Build(director, this);

            var layerData = level.ObjectiveLayer[Bulkhead.Main];
            layerData.ObjectiveData.DataBlockId = objective.PersistentId;

            Bins.WardenObjectives.AddBlock(objective);

            level.BuildDefaultScoutWaves();

            #endregion

            level.CustomSuccessScreen = SuccessScreen.StackEmpty;

            // level.GlobalWaveSettings = GlobalWaveSettings.HighCap_40pts;


            var dim1ResourceSet = ComplexResourceSet.Mining.Duplicate();

            dim1ResourceSet.CustomGeomorphs.Add(new Prefab
            {
                Asset = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_32x32_elevator_shaft_mining_01.prefab",
                SubComplex = SubComplex.Storage,
                Shard = 17
            });
            dim1ResourceSet.CustomGeomorphs.Add(new Prefab
            {
                Asset = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_HA_02.prefab",
                SubComplex = SubComplex.DigSite,
                Shard = 17
            });


            var dimensionIndex = DimensionIndex.Dimension1;
            var (dimensionLayout, dimStart) = LevelLayout.BuildDimension(level, director, objective, dimensionIndex, Complex.Mining);

            dimensionLayout.Zones.Add(
                new Zone(level, dimensionLayout)
                {
                    LightSettings = Lights.Light.HeavyRedToCyan_1
                });

            var dimension = new Dimension
            {
                // Data = new Dimensions.DimensionData
                // {
                //     Layout = dimensionLayout,
                //
                //     DimensionGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_HA_02.prefab",
                //     // DimensionGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_hub_HA_01.prefab",
                //     // DimensionGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_32x32_elevator_shaft_mining_01.prefab",
                //
                //     Fog = Fog.HeavyFullFog_Infectious,
                //     ResourceSet = dim1ResourceSet
                // },

                Data = Dimensions.DimensionData.AlphaSix,

                PersistentId = 3,
            };
            // dimension.FindOrPersist();
            dimension.Persist();

            level.DimensionDatas.Add(new Levels.DimensionData
            {
                Dimension = dimensionIndex,
                Data = dimension
            });

            var (position, rotation) = LevelCustomTerminals.GetCandidates(dimension.Data.DimensionGeomorph).First();

            CustomTerminalSpawnManager.AddSpawnRequest(
                level.LevelLayoutData,
                new CustomTerminalSpawnRequest
                {
                    Bulkhead = director.Bulkhead,
                    DimensionIndex = DimensionIndex.Dimension1,
                    LocalIndex = 0,
                    GeomorphName = dimension.Data.DimensionGeomorph,
                    LocalPosition = position,
                    LocalRotation = rotation
                });



            // The zones
            var elevatorDrop = new ZoneNode(Bulkhead.Main, level.Planner.NextIndex(Bulkhead.Main));
            var elevatorDropZone = new Zone(level, layout)
            {
                // Coverage = new CoverageMinMax { Min = 25, Max = 35 },
                Coverage = CoverageMinMax.Large_150,
                LightSettings = Lights.GenRandomLight(),
                LocalIndex = 0,
                CustomGeomorph = geo
            };

            // elevatorDropZone.GeneratorClustersInZone = 1;

            // elevatorDropZone.EnemySpawningInZone.Add(
            //     EnemySpawningData.MegaMother_AlignedSpawn);

            // elevatorDropZone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer);

            // EnemyCustomization.Model.Shadows.Add(
            //     new Shadow()
            //     {
            //         Target = new Target
            //         {
            //             Mode = Mode.PersistentId,
            //             PersistentIds = new() { (uint)Enemy.Mother }
            //         },
            //         Type = "NewShadows",
            //         TumorVisibleFromBehind = true
            //     });


            #region Scout Wave Customization
            // var events = new List<WardenObjectiveEvent>().AddGenericWave(GenericWave.SingleMother).ToList();
            //
            // level.EOS_EventsOnScoutScream.Definitions.Add(
            //     new EventsOnScoutScream
            //     {
            //         ZoneNumber = -1,
            //         Bulkhead = Bulkhead.All,
            //         DimensionIndex = null,
            //
            //         SuppressVanillaScoutWave = true,
            //         Events = events
            //     });
            //
            // elevatorDropZone.EnemySpawningInZone.Add(
            //     EnemySpawningData.Scout with { Points = 5 });
            #endregion

            level.Planner.AddZone(elevatorDrop, elevatorDropZone);
            layout.Zones.Add(elevatorDropZone);

            // elevatorDropZone.EnemySpawningInZone.Add(new EnemySpawningData
            // {
            //     GroupType = EnemyGroupType.Hibernate,
            //     Difficulty = (uint)Enemy.ChargerGiant,
            //     Points = 4
            // });


            for (var z = 0; z < forwardZones; z++)
            {
                var zone = new Zone(level, layout)
                {
                    Coverage = new CoverageMinMax { Min = 5, Max = 10 },
                    LightSettings = Lights.GenRandomLight(),
                    LocalIndex = z + 1,
                    BuildFromLocalIndex = 0
                };

                zone.EventsOnOpenDoor.Add(
                    new WardenObjectiveEvent
                    {
                        Type = WardenObjectiveEventType.DimensionWarpTeam,
                        Dimension = DimensionIndex.Dimension1,
                        Layer = (int)Bulkhead.Main
                    });

                layout.Zones.Add(zone);

                // var puzzle = ChainedPuzzle.TravelAlarm_Team with
                // {
                //     PublicAlarmName = "Class S T Alarm",
                //     Puzzle = new List<PuzzleComponent>
                //     {
                //         PuzzleComponent.SustainedTravel
                //     }
                // };

                // if (z == 0)
                //     puzzle = ChainedPuzzle.TravelAlarm_Team;
                // else if (z == 1)
                //     puzzle = ChainedPuzzle.TravelAlarm_Team;
                // else if (z == 2)
                //     puzzle = ChainedPuzzle.None;

                // zone.Alarm = ChainedPuzzle.FindOrPersist(puzzle);

                // zone.ChainedPuzzleToEnter =

                // if (z == 0)
                // {
                //     var sensorEvents = new List<WardenObjectiveEvent>();
                //
                //     sensorEvents
                //         .AddSound(Sound.LightsOff)
                //         .AddSpawnWave(GenericWave.SingleTank, 2.0);
                //
                //     level.ZoneSensors.Add(new ZoneSensorDefinition
                //     {
                //         ZoneNumber = zone.LocalIndex,
                //         Bulkhead = Bulkhead.Main,
                //
                //         SensorGroups = new List<ZoneSensorGroupDefinition>
                //         {
                //             new ZoneSensorGroupDefinition
                //             {
                //                 TriggerEach = true,
                //                 Density = SensorDensity.High,
                //                 Radius = 1,
                //                 AreaIndex = -1,
                //             }
                //         },
                //
                //         EventsOnTrigger = sensorEvents
                //     });
                // }
            }

            // var sensorEvents2 = new List<WardenObjectiveEvent>();
            //
            // sensorEvents2
            //     .AddSound(Sound.LightsOff)
            //     .AddSpawnWave(new GenericWave
            //     {
            //         Population = WavePopulation.Baseline,
            //         Settings = WaveSettings.SingleMiniBoss
            //     }, 1.0);
            //
            // level.ZoneSensors.Add(new ZoneSensorDefinition
            // {
            //     Id = 123,
            //     ZoneNumber = 0,
            //     Bulkhead = Bulkhead.Main,
            //     SensorGroups = new List<ZoneSensorGroupDefinition>
            //     {
            //         new ZoneSensorGroupDefinition
            //         {
            //             TriggerEach = false,
            //             // Count = 128,
            //             Density = SensorDensity.High,
            //             Moving = 3,
            //             Speed = 0.5,
            //             // Radius = 2.0,
            //             EdgeDistance = 0.7,
            //             AreaIndex = 1,
            //             EncryptedText = true,
            //         }
            //     },
            //
            //     EventsOnTrigger = sensorEvents2
            // });
            //
            // var resetTime = 5;
            // sensorEvents2
            //     .EnableZoneSensorsWithReset(123, resetTime)
            //     .AddSound(Sound.LightsOn_Vol4, resetTime - 0.4);

            // var (med, medZone) = layout.BuildOptional_MedicalBay(elevatorDrop);
            // layout.Zones.Add(medZone);
            // level.HasMedBay = true;

            // level.Settings.Modifiers.Add(LevelModifiers.FogIsInfectious);

            // medZone.EventsOnOpenDoor.AddCyclingFog(level);

            // elevatorDropZone.TerminalPlacements.First().UniqueCommands.Add(
            //     new CustomTerminalCommand
            //     {
            //         Command = "DEACTIVATE_SENSORS",
            //         CommandDesc = new Text($"Deactivate security sensors in {Intel.ZoneRaw(elevatorDropZone)}"),
            //         CommandEvents = new List<WardenObjectiveEvent>()
            //             // .AddStopLoop(263, 0.4)
            //             .DisableZoneSensors(123, 1.4)
            //             .ToList(),
            //         PostCommandOutputs = new List<TerminalOutput>
            //         {
            //             new()
            //             {
            //                 Output = "Authenticating with BIOCOM...",
            //                 Type = LineType.SpinningWaitNoDone,
            //                 Time = 1.0
            //             },
            //             new()
            //             {
            //                 Output = "Done.",
            //                 Type = LineType.Normal,
            //                 Time = 1.0
            //             },
            //         }
            //     });
            //
            // elevatorDropZone.TerminalPlacements.First().UniqueCommands.Add(
            //     new CustomTerminalCommand
            //     {
            //         Command = "ACTIVATE_SENSORS",
            //         CommandDesc = new Text($"Activate security sensors in {Intel.ZoneRaw(elevatorDropZone)}"),
            //         CommandEvents = new List<WardenObjectiveEvent>()
            //             // .AddStopLoop(263, 0.4)
            //             .EnableZoneSensors(123, 1.4)
            //             .ToList(),
            //         PostCommandOutputs = new List<TerminalOutput>
            //         {
            //             new()
            //             {
            //                 Output = "Authenticating with BIOCOM...",
            //                 Type = LineType.SpinningWaitNoDone,
            //                 Time = 1.0
            //             },
            //             new()
            //             {
            //                 Output = "Done.",
            //                 Type = LineType.Normal,
            //                 Time = 1.0
            //             },
            //         }
            //     });
            //
            // elevatorDropZone.TerminalPlacements.First().UniqueCommands.Add(
            //     new CustomTerminalCommand
            //     {
            //         Command = "RESET_SENSORS",
            //         CommandDesc = new Text($"Fully reset security sensors in {Intel.ZoneRaw(elevatorDropZone)}"),
            //         CommandEvents = new List<WardenObjectiveEvent>()
            //             // .AddStopLoop(263, 0.4)
            //             .EnableZoneSensorsWithReset(123, 1.4)
            //             .ToList(),
            //         PostCommandOutputs = new List<TerminalOutput>
            //         {
            //             new()
            //             {
            //                 Output = "Authenticating with BIOCOM...",
            //                 Type = LineType.SpinningWaitNoDone,
            //                 Time = 1.0
            //             },
            //             new()
            //             {
            //                 Output = "Done.",
            //                 Type = LineType.Normal,
            //                 Time = 1.0
            //             },
            //         }
            //     });

            // layout.AddSecuritySensors_SinglePouncerShadow((0, 1));

            Bins.LevelLayouts.AddBlock(layout);
        }
        catch (Exception err)
        {
            Plugin.Logger.LogError($"OH NO: {err}");
        }

        level.FinalizeCustomMods();
        level.FinalizeComplexResourceSet();

        return level;
    }
}
