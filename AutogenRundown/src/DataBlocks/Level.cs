using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.GeneratorData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
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

                // TODO: This needs some work. For now we just do forward/left/right

                // // Seems like using global backwards is generally a bad idea.
                // // We need to either have a distant
                // RelativeDirection.Global_Backward
            };

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
        #endregion

        #region === MODS ===
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
        /// Reactor Shutdown LayoutDefinitions
        /// </summary>
        [JsonIgnore]
        public LayoutDefinitions EOS_SecuritySensor { get; private set; } = new()
        {
            Type = ExtraObjectiveSetupType.SecuritySensor
        };
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
        public Dictionary<Bulkhead, ObjectiveLayerData> ObjectiveLayer { get; private set; }
            = new Dictionary<Bulkhead, ObjectiveLayerData>
            {
                { Bulkhead.Main, new ObjectiveLayerData() },
                { Bulkhead.Extreme, new ObjectiveLayerData() },
                { Bulkhead.Overload, new ObjectiveLayerData() }
            };

        /// <summary>
        /// Tracking of other objectives
        /// </summary>
        [JsonIgnore]
        public Dictionary<Bulkhead, WardenObjective> Objective { get; set; } = new();

        /// <summary>
        /// Allows easy access to the directors without having to switch
        /// </summary>
        [JsonIgnore]
        public Dictionary<Bulkhead, uint> LayoutRef { get; private set; }
            = new Dictionary<Bulkhead, uint>
            {
                { Bulkhead.Main, 0 },
                { Bulkhead.Extreme, 0 },
                { Bulkhead.Overload, 0 }
            };
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

        public JObject Descriptive
        {
            get => new JObject
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

                // TODO: Replace these with actual wave settings / populations
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
            get => LayoutRef[Bulkhead.Main];
            set => LayoutRef[Bulkhead.Main] = value;
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
            get => SecondaryLayout > 0;
            private set { }
        }

        public uint SecondaryLayout
        {
            get => LayoutRef[Bulkhead.Extreme];
            set => LayoutRef[Bulkhead.Extreme] = value;
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
            get => ThirdLayout > 0;
            private set { }
        }

        public uint ThirdLayout
        {
            get => LayoutRef[Bulkhead.Overload];
            set => LayoutRef[Bulkhead.Overload] = value;
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
        /// Generates the zone alias start numbers, tries to ensure there will be no collisions.
        /// </summary>
        private void GenerateZoneAliasStarts()
        {
            var minmax = Tier switch
            {
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

                // A-Tier: 195 spread
                _ => new List<(int, int)>
                {
                    (  5, 70),
                    ( 95, 135),
                    (160, 200),
                }
            };

            var (min, max) = Generator.Draw(minmax);
            ZoneAliasStart_Main = Generator.Random.Next(min, max);

            (min, max) = Generator.Draw(minmax);
            ZoneAliasStart_Extreme = Generator.Random.Next(min, max);

            (min, max) = Generator.Draw(minmax);
            ZoneAliasStart_Overload = Generator.Random.Next(min, max);
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
                Bulkhead.Main => Bins.LevelLayouts.Find(LevelLayoutData),
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

            if (bulkhead == Bulkhead.Main)
            {
                direction = Generator.Draw(RelativeDirections);

                // TODO: I don't think we actually need to do this for the more simpler approach
                //       of now just selecting forward/left/right

                // // We also want to remove the reverse direction of what we have selected for Main.
                // // The rationale is we don't want Extreme/Overload to attempt to build backwards
                // // along the same direction as Main is heading, but instead to branch off.
                // var removeBackwards = direction.Forward switch
                // {
                //     ZoneExpansion.Forward => RelativeDirection.Global_Backward,
                //     ZoneExpansion.Left => RelativeDirection.Global_Right,
                //     ZoneExpansion.Right => RelativeDirection.Global_Left,
                //     ZoneExpansion.Backward => RelativeDirection.Global_Forward,
                // };
                //
                // RelativeDirections.Remove(removeBackwards);
            }
            else if (bulkhead == Bulkhead.Extreme || bulkhead == Bulkhead.Overload)
            {
                direction = Generator.Draw(RelativeDirections);
            }

            Settings.SetDirections(bulkhead, direction);
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

            EOS_EventsOnBossDeath.Name = $"{Tier}{Index}_{Name.Replace(" ", "_")}";
            EOS_EventsOnBossDeath.MainLevelLayout = LevelLayoutData;

            EOS_EventsOnScoutScream.Name = $"{Tier}{Index}_{Name.Replace(" ", "_")}";
            EOS_EventsOnScoutScream.MainLevelLayout = LevelLayoutData;

            EOS_IndividualGenerator.Name = $"{Tier}{Index}_{Name.Replace(" ", "_")}";
            EOS_IndividualGenerator.MainLevelLayout = LevelLayoutData;

            EOS_ReactorShutdown.Name = $"{Tier}{Index}_{Name.Replace(" ", "_")}";
            EOS_ReactorShutdown.MainLevelLayout = LevelLayoutData;

            EOS_SecuritySensor.Name = $"{Tier}{Index}_{Name.Replace(" ", "_")}";
            EOS_SecuritySensor.MainLevelLayout = LevelLayoutData;

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
        /// Builds a specific bulkhead layout
        /// </summary>
        /// <param name="bulkhead"></param>
        private void BuildLayout(Bulkhead bulkhead)
        {
            var director = Director[bulkhead];
            var objective = Objective[bulkhead];

            var layout = LevelLayout.Build(this, director, objective);
            LayoutRef[bulkhead] = layout.PersistentId;

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

            // Randomly select which bulkheads to use
            var selectedBulkheads = Generator.Select(new List<(double, Bulkhead)>
            {
                (0.25, Bulkhead.Main),
                (0.4, Bulkhead.Main | Bulkhead.Extreme),
                (0.2, Bulkhead.Main | Bulkhead.Overload),
                (0.15, Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload)
            });

            // Assign bulkheads
            level.Settings.Bulkheads = selectedBulkheads;

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

            /*
             * Options for bulkhead keys and bulkhead DCs:
             *
             *  * All unlocked: main, extreme, overload
             *    Each bulkhead is unlocked, no keys required
             *
             *  * Chained: main -> extreme -> overload -> END
             *    Each bulkhead is locked behind the previous one with one key in each.
             *    Main has extreme DC, extreme has overload DC.
             *
             *  * Choice: main -> overload -> END
             *                 -> extreme -> overload -> END
             *    Main has extreme/overload DC, extreme has extra key for overload DC.
             * */
            var bulkheadKeys = Generator.Select(new List<(double, string)>
            {
                (1.0, "chained"),

                // TODO: implement these
                // (1.0, "all"),
                // (1.0, "choice"), // TODO: implement
            });

            #region Bulkhead direction selection
            /*
             * We must select the relative directions we want to try and build each of the bulkhead
             * zones upfront as it has an impact on the other areas of level generation.
             * */
            level.SelectDirection(Bulkhead.Main);

            if (selectedBulkheads.HasFlag(Bulkhead.Extreme))
                level.SelectDirection(Bulkhead.Extreme);

            if (selectedBulkheads.HasFlag(Bulkhead.Overload))
                level.SelectDirection(Bulkhead.Overload);
            #endregion

            #region Objective prebuild
            /* We prebuild the objectives as certain objectives have components that affect level
             * generation. For example the "distribute cells to generator cluster" objective
             * requires that the level generate enough generators for each of the cells to be
             * distributed to. */
            level.PreBuildObjective(Bulkhead.Main);

            if (selectedBulkheads.HasFlag(Bulkhead.Extreme))
                level.PreBuildObjective(Bulkhead.Extreme);

            if (selectedBulkheads.HasFlag(Bulkhead.Overload))
                level.PreBuildObjective(Bulkhead.Overload);
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

            if (selectedBulkheads.HasFlag(Bulkhead.Extreme))
                level.BuildLayout(Bulkhead.Extreme);

            if (selectedBulkheads.HasFlag(Bulkhead.Overload))
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

            if (selectedBulkheads.HasFlag(Bulkhead.Extreme))
                level.SecondaryLayerData.BulkheadKeyPlacements.Add(
                    new List<ZonePlacementData>
                    {
                        new() { LocalIndex = 0, Weights = ZonePlacementWeights.NotAtStart }
                    });

            if (selectedBulkheads.HasFlag(Bulkhead.Overload))
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

            if (selectedBulkheads.HasFlag(Bulkhead.Extreme) && level.GetObjective(Bulkhead.Extreme) != null)
                level.GetObjective(Bulkhead.Extreme)!.PostBuild(level.SecondaryDirector, level);

            if (selectedBulkheads.HasFlag(Bulkhead.Overload) && level.GetObjective(Bulkhead.Overload) != null)
                level.GetObjective(Bulkhead.Overload)!.PostBuild(level.OverloadDirector, level);
            #endregion

            #region Finalize -- ExtraObjectiveSetup
            level.FinalizeExtraObjectiveSetup();
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
                var layout = new LevelLayout(level, director, level.Settings, level.Planner);

                level.LayoutRef[Bulkhead.Main] = layout.PersistentId;

                // objective.Build(director, this);

                var layerData = level.ObjectiveLayer[Bulkhead.Main];
                layerData.ObjectiveData.DataBlockId = objective.PersistentId;

                Bins.WardenObjectives.AddBlock(objective);

                #endregion


                // var dimension = new Dimension
                // {
                //     Data = new Dimensions.DimensionData
                //     {
                //         IsStaticDimension = true
                //     }
                // };
                // dimension.FindOrPersist();
                //
                // level.DimensionDatas.Add(new Levels.DimensionData
                // {
                //     Dimension = DimensionIndex.Dimension1,
                //     DataPid = dimension.PersistentId,
                // });

                // The zones
                var elevatorDrop = new ZoneNode(Bulkhead.Main, level.Planner.NextIndex(Bulkhead.Main));
                var elevatorDropZone = new Zone(level.Tier)
                {
                    Coverage = new CoverageMinMax { Min = 25, Max = 35 },
                    LightSettings = Lights.GenRandomLight(),
                    LocalIndex = 0,
                    CustomGeomorph = geo
                };

                // elevatorDropZone.EnemySpawningInZone.Add(
                //     EnemySpawningData.MegaMother_AlignedSpawn);

                elevatorDropZone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer);

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

                // elevatorDrop = layout.BuildBranch(elevatorDrop, 2);

                // layout.AddAlignedBossFight_MegaMom(elevatorDrop);

                // elevatorDropZone.EnemySpawningInZone.Add(EnemySpawningData.PouncerShadow);

                for (var z = 0; z < forwardZones; z++)
                {
                    var zone = new Zone(level.Tier)
                    {
                        Coverage = new CoverageMinMax { Min = 5, Max = 10 },
                        LightSettings = Lights.GenRandomLight(),
                        LocalIndex = z + 1,
                        BuildFromLocalIndex = 0
                    };

                    // zone.EventsOnOpenDoor.Add(
                    //     new WardenObjectiveEvent
                    //     {
                    //         Type = WardenObjectiveEventType.DimensionWarpTeam,
                    //         DimensionIndex = (int)DimensionIndex.Dimension1,
                    //         Layer = (int)Bulkhead.Main
                    //     });

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
}
