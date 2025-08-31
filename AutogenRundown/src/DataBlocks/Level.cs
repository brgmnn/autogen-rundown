using System.Text.RegularExpressions;
using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.GeneratorData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public class BuildFrom
{
    public int LayerType { get; set; } = 0;
    public int Zone { get; set; } = 0;
}

public class Level
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
        if (SecondaryLayerEnabled)
        {
            var extremeSize = Layouts[Bulkhead.Extreme].Zones.Count;

            ZoneAliasStart_Extreme = Generator.BetweenConstrained(min, max, blocked, extremeSize + 5);

            blocked.Add((ZoneAliasStart_Extreme, ZoneAliasStart_Extreme + extremeSize - 1));
        }

        // Overload
        if (ThirdLayerEnabled)
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
    public LevelSecurityDoors SecurityDoors { get; private set; } = new();

    /// <summary>
    /// For performing custom terminal placement in a zone/area
    /// </summary>
    [JsonIgnore]
    public LevelTerminalPlacements TerminalPlacements { get; private set; } = new();

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

    #region GlobalWaveSettings

    [JsonIgnore]
    public GlobalWaveSettings GlobalWaveSettings { get; set; } = GlobalWaveSettings.Default;

    #endregion
    #endregion

    #region Build directors
    [JsonIgnore]
    /// <summary>
    /// Allows easy access to the directors without having to switch
    /// </summary>
    public Dictionary<Bulkhead, BuildDirector> Director { get; private set; } = new();

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
    /// Which zone the main bulkhead door gates. Often we want objectives to be spawned here
    /// or later.
    /// </summary>
    [JsonIgnore]
    public int MainBulkheadZone { get; set; } = 0;

    /// <summary>
    /// Level description
    /// </summary>
    [JsonIgnore]
    public uint Description { get; set; } = 0;

    [JsonIgnore]
    public Fog FogSettings { get; set; } = Fog.DefaultFog;

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

    public JObject Descriptive
    {
        get => new()
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
    }

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

    public JObject Seeds
    {
        get => new JObject
        {
            ["BuildSeed"] = BuildSeed,
            ["FunctionMarkerOffset"] = 1,
            ["StandardMarkerOffset"] = 0,
            ["LightJobSeedOffset"] = 0
        };
    }

    public JObject Expedition
    {
        get => new JObject
        {
            ["ComplexResourceData"] = (int)Complex,
            ["MLSLevelKit"] = 0,
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
    }

    public JObject VanityItemsDropData = new() { ["Groups"] = new JArray() };

    #region Main Objective Data
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

    public bool SecondaryLayerEnabled
    {
        get => Layouts.ContainsKey(Bulkhead.Extreme);
        private set { }
    }

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

    public bool ThirdLayerEnabled
    {
        get => Layouts.ContainsKey(Bulkhead.Overload);
        private set { }
    }

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
    public int SoundEventOnWarpToReality = 0;

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
    public JObject SpecialOverrideData
    {
        get => new()
        {
            ["WeakResourceContainerWithPackChanceForLocked"] = -1.0,
            ["InfectionLevelAtExpeditionStart"] = StartingInfection,
            ["HealthLevelAtExpeditionStart"] = StartingHealth,
            ["StandardAmmoAtExpeditionStart"] = StartingMainAmmo,
            ["SpecialAmmoAtExpeditionStart"] = StartingSpecialAmmo,
            ["ToolAmmoAtExpeditionStart"] = StartingTool
        };
    }
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
            "A" => Generator.Random.Next(420, 650),
            "B" => Generator.Random.Next(600, 850),
            "C" => Generator.Random.Next(800, 1000),
            "D" => Generator.Random.Next(900, 1100),
            "E" => Generator.Random.Next(950, 1500),
            _ => Depth
        };
    }

    /// <summary>
    ///
    /// </summary>
    public void MarkAsErrorAlarm()
    {
        Name = $"<color=red>?!</color><color=#444444>-</color>{Name}";

        ElevatorDropWardenIntel.Add((Generator.Between(1, 4), Generator.Draw(new List<string>
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
        Name = $"<color=red>!!!</color><color=#444444>/</color><s>{Name}</s>";

        ElevatorDropWardenIntel.Add((Generator.Between(6, 12), Generator.Draw(new List<string>
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

    public WardenObjective? GetObjective(Bulkhead variant) =>
        variant switch
        {
            Bulkhead.Main => Bins.WardenObjectives.Find(MainLayerData.ObjectiveData.DataBlockId),
            Bulkhead.Extreme => Bins.WardenObjectives.Find(SecondaryLayerData.ObjectiveData.DataBlockId),
            Bulkhead.Overload => Bins.WardenObjectives.Find(ThirdLayerData.ObjectiveData.DataBlockId),
            Bulkhead.None => throw new NotImplementedException(),
            Bulkhead.StartingArea => throw new NotImplementedException(),
            Bulkhead.All => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };

    public LevelLayout? GetLevelLayout(Bulkhead variant) =>
        variant switch
        {
            Bulkhead.Main or (Bulkhead.Main | Bulkhead.StartingArea) => Bins.LevelLayouts.Find(LevelLayoutData),
            Bulkhead.Extreme => Bins.LevelLayouts.Find(SecondaryLayout),
            Bulkhead.Overload => Bins.LevelLayouts.Find(ThirdLayout),

            _ => Bins.LevelLayouts.Find(LevelLayoutData)
        };

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
                (0.5, BukheadStrategy.Default)
            },
            Bulkhead.Main | Bulkhead.Extreme => new List<(double, BukheadStrategy)>
            {
                (0.5, BukheadStrategy.Default),
                (0.5, BukheadStrategy.CentralHub_x2)
            },
            Bulkhead.Main | Bulkhead.Overload => new List<(double, BukheadStrategy)>
            {
                (0.5, BukheadStrategy.Default),
                (0.5, BukheadStrategy.CentralHub_x2)
            },
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload => new List<(double, BukheadStrategy)>
            {
                (0.1, BukheadStrategy.Default),
                (0.2, BukheadStrategy.CentralHub_x3),
                (0.7, BukheadStrategy.SingleChain)
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

        if (Settings.Bulkheads.HasFlag(Bulkhead.Extreme))
            SelectDirection(Bulkhead.Extreme);

        if (Settings.Bulkheads.HasFlag(Bulkhead.Overload))
            SelectDirection(Bulkhead.Overload);
    }

    /// <summary>
    /// Saves all of the EOS definitions
    /// </summary>
    private void FinalizeExtraObjectiveSetup()
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

        SecurityDoors.Name = $"{Tier}{Index}_{fsName}";
        SecurityDoors.MainLevelLayout = LevelLayoutData;

        TerminalPlacements.Name = $"{Tier}{Index}_{fsName}";
        TerminalPlacements.MainLevelLayout = LevelLayoutData;

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

        switch (Tier)
        {
            case "A":
            case "B":
            {
                settings = WaveSettings.Scout_Easy;
                break;
            }

            case "C":
            {
                settings = WaveSettings.Scout_Normal;

                if (Settings.Modifiers.Contains(LevelModifiers.ManyShadows) && Generator.Flip())
                {
                    population = WavePopulation.OnlyShadows;
                    settings = WaveSettings.SingleWave_MiniBoss_6pts;
                }
                else if (Settings.Modifiers.Contains(LevelModifiers.ManyChargers) && Generator.Flip())
                {
                    population = WavePopulation.OnlyChargers;
                    settings = WaveSettings.SingleWave_MiniBoss_6pts;
                }
                break;
            }

            case "D":
            {
                settings = WaveSettings.Scout_Hard;

                if (Settings.Modifiers.Contains(LevelModifiers.ManyNightmares) && Generator.Flip())
                {
                    population = WavePopulation.OnlyNightmareGiants;
                    settings = WaveSettings.SingleWave_MiniBoss_8pts;
                }
                else if (Settings.Modifiers.Contains(LevelModifiers.ManyShadows) && Generator.Flip())
                {
                    population = WavePopulation.OnlyShadows;
                    settings = WaveSettings.SingleWave_MiniBoss_6pts;
                }
                else if (Settings.Modifiers.Contains(LevelModifiers.ManyChargers) && Generator.Flip())
                {
                    population = WavePopulation.OnlyChargers;
                    settings = WaveSettings.SingleWave_MiniBoss_8pts;
                }
                break;
            }

            case "E":
            {
                settings = WaveSettings.Scout_VeryHard;

                if (Settings.Modifiers.Contains(LevelModifiers.ManyNightmares) && Generator.Flip())
                {
                    population = WavePopulation.OnlyNightmareGiants;
                    settings = WaveSettings.SingleWave_MiniBoss_12pts;
                }
                else if (Settings.Modifiers.Contains(LevelModifiers.ManyShadows) && Generator.Flip())
                {
                    population = WavePopulation.OnlyShadows;
                    settings = WaveSettings.SingleWave_MiniBoss_12pts;
                }
                else if (Settings.Modifiers.Contains(LevelModifiers.ManyChargers) && Generator.Flip())
                {
                    population = WavePopulation.OnlyChargers;
                    settings = WaveSettings.SingleWave_MiniBoss_12pts;
                }
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
        if (level.Name == "")
            level.Name = Generator.Pick(Words.NounsLevel) ?? "";

        var logLevelId = $"Level={level.Tier}{level.Index}";

        level.GenerateDepth();
        level.GenerateZoneAliasStarts();
        level.BuildBulkheads();

        #region Fog settings
        var lowFog = level.Settings.Modifiers.Contains(LevelModifiers.FogIsInfectious)
            ? Fog.LowFog_Infectious
            : Fog.LowFog;
        var lowMidFog = level.Settings.Modifiers.Contains(LevelModifiers.FogIsInfectious)
            ? Fog.LowMidFog_Infectious
            : Fog.LowMidFog;

        // Set low fog if we have fog
        if (level.Settings.Modifiers.Contains(LevelModifiers.Fog))
            level.FogSettings = lowFog;

        // For heavy fog we can also roll low mid fog
        if (level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
            level.FogSettings = Generator.Flip(0.75) ? lowFog : lowMidFog;
        #endregion

        Plugin.Logger.LogDebug($"{logLevelId} ({level.Complex}) - Modifiers: {level.Settings.Modifiers}, Fog: {level.FogSettings.Name}");

        #region Objective prebuild
        /* We prebuild the objectives as certain objectives have components that affect level
         * generation. For example the "distribute cells to generator cluster" objective
         * requires that the level generate enough generators for each of the cells to be
         * distributed to. */
        level.PreBuildObjective(Bulkhead.Main);

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Extreme))
            level.PreBuildObjective(Bulkhead.Extreme);

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Overload))
            level.PreBuildObjective(Bulkhead.Overload);
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

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Extreme))
            level.BuildLayout(Bulkhead.Extreme);

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Overload))
            level.BuildLayout(Bulkhead.Overload);
        #endregion

        #region Bulkhead Keys
        /*
         * Ensure we place the bulkhead keys. For now, we just place one at the start of each
         * bulkhead zone. This is guaranteed to allow us to complete all the objectives.
         */
        level.MainLayerData.BulkheadKeyPlacements.Add(
            new List<ZonePlacementData>
            {
                new() { LocalIndex = 0, Weights = ZonePlacementWeights.NotAtStart }
            });

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Extreme))
            level.SecondaryLayerData.BulkheadKeyPlacements.Add(
                new List<ZonePlacementData>
                {
                    new() { LocalIndex = 0, Weights = ZonePlacementWeights.NotAtStart }
                });

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Overload))
            level.ThirdLayerData.BulkheadKeyPlacements.Add(
                new List<ZonePlacementData>
                {
                    new() { LocalIndex = 0, Weights = ZonePlacementWeights.NotAtStart }
                });
        #endregion

        #region Adjust coverage sizes
        var entrances = level.Planner.GetBulkheadEntranceZones();

        foreach (var node in entrances)
        {
            var zone = level.GetLevelLayout(node.Bulkhead)?.Zones[node.ZoneNumber];

            if (zone == null)
            {
                Plugin.Logger.LogWarning($"Level={level.Tier}{level.Index} - Cannot resize " +
                                         $"bulkhead entrance zone. Zone_{node.ZoneNumber} could not be found.");
                continue;
            }

            // TODO: rebalance enemies
            // TODO: skip this if it's a custom geomorph
            zone.Coverage = new CoverageMinMax { Min = 80.0, Max = 80.0 };
        }
        #endregion

        #region Finalize -- WardenObjective.PostBuild()
        level.GetObjective(Bulkhead.Main)!.PostBuild(level.MainDirector, level);

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Extreme) && level.GetObjective(Bulkhead.Extreme) != null)
            level.GetObjective(Bulkhead.Extreme)!.PostBuild(level.SecondaryDirector, level);

        if (level.Settings.Bulkheads.HasFlag(Bulkhead.Overload) && level.GetObjective(Bulkhead.Overload) != null)
            level.GetObjective(Bulkhead.Overload)!.PostBuild(level.OverloadDirector, level);
        #endregion

        #region Finalize -- ExtraObjectiveSetup
        level.FinalizeExtraObjectiveSetup();
        #endregion

        #region Finalize -- Zone numbers

        level.RecalculateZoneAliasStarts();

        #endregion

        Plugin.Logger.LogDebug($"Level={level.Tier}{level.Index} level plan: {level.Planner}");

        return level;
    }

    /// <summary>
    /// Test level construction for testing out new geos
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static Level Debug_BuildGeoTest(string geo, Level level, int forwardZones = 0)
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


            var dimension = new Dimension
            {
                Data = new Dimensions.DimensionData
                {
                    // DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Static_01.prefab",
                    DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_R6A2.prefab",
                    DimensionResourceSetID = 47,
                    IsStaticDimension = true
                },
                PersistentId = 2,
            };
            // dimension.FindOrPersist();
            dimension.Persist();

            level.DimensionDatas.Add(new Levels.DimensionData
            {
                Dimension = DimensionIndex.Dimension1,
                Data = dimension
            });

            // The zones
            var elevatorDrop = new ZoneNode(Bulkhead.Main, level.Planner.NextIndex(Bulkhead.Main));
            var elevatorDropZone = new Zone(level, layout)
            {
                // Coverage = new CoverageMinMax { Min = 25, Max = 35 },
                Coverage = CoverageMinMax.Large_150,
                LightSettings = Lights.GenRandomLight(),
                LocalIndex = 0,
                // CustomGeomorph = geo
            };

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

            // var zone2 = new Zone(level)
            // {
            //     Coverage = new CoverageMinMax { Min = 100, Max = 100 },
            //     LightSettings = Lights.GenRandomLight(),
            //     LocalIndex = 1,
            //     BuildFromLocalIndex = 0,
            //     CustomGeomorph = geo
            // };
            //
            // level.Planner.AddZone(new ZoneNode(Bulkhead.Main, 1), zone2);
            // layout.Zones.Add(zone2);

            elevatorDropZone.EnemySpawningInZone.Add(
                EnemySpawningData.TierA with { Points = 30 });

            // layout.AddAlignedBossFight_MegaMom(elevatorDrop);

            // elevatorDropZone.EnemySpawningInZone.Add(
            //     EnemySpawningData.Mother with { Points = 10 });
            //
            // elevatorDropZone.EnemySpawningInZone.Add(
            //     EnemySpawningData.PMother with { Points = 10 });


            // elevatorDropZone.EnemySpawningInZone.Add(
                // new EnemySpawningData
                // {
                //     Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Hybrids),
                //     Points = 1,
                //     GroupType = EnemyGroupType.Hibernate
                // });
                // EnemySpawningData.NightmareGiant with
                // {
                //     Points = 6
                // });

            // elevatorDropZone.EnemySpawningInZone.Add(new EnemySpawningData
            // {
            //     GroupType = EnemyGroupType.Hibernate,
            //     Difficulty = (uint)Enemy.ChargerGiant,
            //     Points = 4
            // });
            // elevatorDropZone.EnemySpawningInZone.Add(new EnemySpawningData
            // {
            //     GroupType = EnemyGroupType.Hibernate,
            //     Difficulty = (uint)Enemy.Charger,
            //     Points = 1
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
                        DimensionIndex = (int)DimensionIndex.Dimension1,
                        Layer = (int)Bulkhead.Main
                    });

                layout.Zones.Add(zone);
            }

            // layout.AddSecuritySensors_SinglePouncerShadow((0, 1));

            Bins.LevelLayouts.AddBlock(layout);
        }
        catch (Exception err)
        {
            Plugin.Logger.LogError($"OH NO: {err}");
        }

        level.FinalizeExtraObjectiveSetup();

        return level;
    }
}
