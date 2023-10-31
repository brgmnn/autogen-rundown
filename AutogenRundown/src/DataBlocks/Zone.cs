using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    /// <summary>
    /// eZoneBuildFromExpansionType
    /// 
    /// Direction is global and forward is looking from the drop elevator
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonebuildfromexpansiontype
    /// </summary>
    enum ZoneBuildExpansion
    {
        Random = 0,
        Forward = 1,
        Backward = 2,
        Right = 3,
        Left = 4
    }

    /// <summary>
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezoneexpansiontype
    /// </summary>
    enum ZoneExpansion
    {
        Random = 0,
        Collapsed = 1,
        Expansional = 2,
        Forward = 3,
        Backward = 4,
        Right = 5,
        Left = 6,
        DirectionalRandom = 7
    }

    /// <summary>
    /// Where in the source zone to try to make the entrance to this zone.
    /// Note that a valid gate may not generate around the set source position/area.
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonebuildfromtype
    /// </summary>
    enum ZoneEntranceBuildFrom
    {
        Random = 0,
        Start = 1,
        AverageCenter = 2,
        Furthest = 3,
        BetweenStartAndFurthest = 4,
        IndexWeight = 5
    }

    /// <summary>
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonedistributionamount
    /// </summary>
    enum DistributionAmount
    {
        None = 0,

        /// <summary>
        /// Count = 2
        /// </summary>
        Pair = 1,

        /// <summary>
        /// Count = 5
        /// </summary>
        Few = 2,

        /// <summary>
        /// Count = 10
        /// </summary>
        Some = 3,

        /// <summary>
        /// Count = 15
        /// </summary>
        SomeMore = 4,

        /// <summary>
        /// Count = 20
        /// </summary>
        Many = 5,

        /// <summary>
        /// Count = 30
        /// </summary>
        Alot = 6,

        /// <summary>
        /// Count = 50
        /// </summary>
        Tons = 7
    }

    /// <summary>
    /// 
    /// </summary>
    internal record class Zone : DataBlock
    {
        #region Custom geomorph settings
        public void GenExitGeomorph(Complex complex)
        {
            switch (complex)
            {
                case Complex.Mining:
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_exit_01.prefab";
                    SubComplex = SubComplex.Refinery;
                    break;

                case Complex.Tech:
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_32x32_lab_exit_01.prefab";
                    SubComplex = SubComplex.Lab;
                    break;

                case Complex.Service:
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Service/Geomorphs/geo_32x32_floodways_exit_01.prefab";
                    SubComplex = SubComplex.Floodways;
                    break;
            };
        }

        /// <summary>
        /// Creates a reactor geomorph in the zone for use with reactor objectives.
        /// See: https://docs.google.com/document/d/1iSYUASlQSaP6l7PD3HszsXSAxJ-wb8MAVwYxb9xW92c/
        ///
        /// Note: Reactors can only be used in Mining/Tech complexes
        /// </summary>
        /// <param name="complex"></param>
        public void GenReactorGeomorph(Complex complex)
        {
            switch (complex)
            {
                case Complex.Mining:
                    {
                        // reactor_open_HA_01 does not work for reactor shutdown
                        //               "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_reactor_open_HA_01.prefab"
                        //               "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_reactor_HA_02.prefab"
                        CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_reactor_HA_02.prefab";
                        SubComplex = SubComplex.Refinery;
                        IgnoreRandomGeomorphRotation = true;
                        Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
                        break;
                    }

                case Complex.Tech:
                    {
                        //               "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_reactor_HA_01.prefab"
                        //               "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_reactor_HA_02.prefab"
                        CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_reactor_HA_01.prefab";
                        SubComplex = SubComplex.Lab;
                        IgnoreRandomGeomorphRotation = true;
                        Coverage = new CoverageMinMax { Min = 40.0, Max = 40.0 };
                        break;
                    }
            };
        }

        /// <summary>
        /// Picks an appropriate geomorph for the corridor to a reactor
        /// </summary>
        /// <param name="complex"></param>
        public void GenReactorCorridorGeomorph(Complex complex)
        {
            switch (complex)
            {
                case Complex.Mining:
                    {
                        var (subcomplex, geomorph) = Generator.Pick(Zones.Geomorphs.Mining_I_Tile);
                        CustomGeomorph = geomorph;
                        SubComplex = subcomplex;

                        Coverage = new CoverageMinMax { Min = 35.0, Max = 50.0 };
                        break;
                    }

                case Complex.Tech:
                    {
                        CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_I_HA_03_v2.prefab";
                        SubComplex = SubComplex.Lab;
                        Coverage = new CoverageMinMax { Min = 30.0, Max = 40.0 };
                        break;
                    }
            }
        }

        #endregion

        #region Enemies
        public class WeightedDifficulty : Generator.ISelectable
        {
            public double Weight { get; set; }

            public List<EnemyRoleDifficulty> Difficulties { get; set; } = new List<EnemyRoleDifficulty>();
        }

        /// <summary>
        /// Generate enemies for the zone
        /// </summary>
        /// <param name="director"></param>
        public void GenEnemies(BuildDirector director)
        {
            var points = director.GetPoints(this);

            switch (director.Tier)
            {
                // Easiest tier, we want the enemies to be relatively easy
                case "A":
                    {
                        var selected = Generator.Select(
                            new List<WeightedDifficulty>
                            {
                                new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy } },
                                new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy, EnemyRoleDifficulty.Medium } },
                            });

                        foreach (var difficulty in selected.Difficulties)
                        {
                            EnemySpawningInZone.Add(
                                new EnemySpawningData()
                                {
                                    GroupType = EnemyGroupType.Hibernate,
                                    Difficulty = (uint)difficulty,
                                    Points = points / selected.Difficulties.Count,
                                });
                        }

                        break;
                    }

                case "B":
                    {
                        var selected = Generator.Select(
                            new List<WeightedDifficulty>
                            {
                                new WeightedDifficulty { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Easy } },
                                new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy, EnemyRoleDifficulty.Medium } },
                                new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy, EnemyRoleDifficulty.Hard } }
                            });

                        foreach (var difficulty in selected.Difficulties)
                        {
                            EnemySpawningInZone.Add(
                                new EnemySpawningData()
                                {
                                    GroupType = EnemyGroupType.Hibernate,
                                    Difficulty = (uint)difficulty,
                                    Points = points / selected.Difficulties.Count,
                                });
                        }

                        break;
                    }

                // C-tier is the normal difficulty benchmark.
                case "C":
                    {
                        var selected = Generator.Select(
                            new List<WeightedDifficulty>
                            {
                                new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Easy, EnemyRoleDifficulty.Medium } },
                                new WeightedDifficulty { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Medium } },
                                new WeightedDifficulty { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Medium, EnemyRoleDifficulty.Hard } },
                                new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Hard } },
                            });

                        foreach (var difficulty in selected.Difficulties)
                        {
                            EnemySpawningInZone.Add(
                                new EnemySpawningData()
                                {
                                    GroupType = EnemyGroupType.Hibernate,
                                    Difficulty = (uint)difficulty,
                                    Points = points / selected.Difficulties.Count,
                                });
                        }

                        break;
                    }

                default:
                    break;
            }


        }
        #endregion

        #region Alarms
        public void RollAlarms(ICollection<ChainedPuzzle> puzzlePack)
        {
            if (LocalIndex == 0)
                return;

            // Grab a random puzzle from the puzzle pack
            var puzzle = Generator.Draw(puzzlePack);

            ChainedPuzzleToEnter = puzzle.PersistentId;

            if (puzzle.PersistentId != 0)
                Bins.ChainedPuzzles.AddBlock(puzzle);
        }
        #endregion

        #region Unused by us properties
        public int AliasOverride = -1;
        public bool OverrideAliasPrefix = false;
        #endregion

        #region Seed properties
        public int SubSeed { get; set; } = 24;
        public int MarkerSubSeed { get; set; } = 3;
        public int LightsSubSeed { get; set; } = 1;
        #endregion

        /// <summary>
        /// Zone number offset from the level layout
        /// </summary>
        public int LocalIndex { get; set; } = 0;

        /// <summary>
        /// Which zone to build this zone from
        /// </summary>
        public int BuildFromLocalIndex { get; set; } = 0;

        /// <summary>
        /// Which tileset to use
        /// </summary>
        public SubComplex SubComplex { get; set; } = SubComplex.All;

        [JsonProperty("CoverageMinMax")]
        public CoverageMinMax Coverage { get; set; } = CoverageMinMax.Medium;

        public ZoneEntranceBuildFrom StartPosition { get; set; } = ZoneEntranceBuildFrom.Random;

        public ZoneBuildExpansion StartExpansion { get; set; } = ZoneBuildExpansion.Random;

        public ZoneExpansion ZoneExpansion { get; set; } = ZoneExpansion.Random;

        /// <summary>
        /// If we specify a custom geomorph it goes here
        /// </summary>
        public string? CustomGeomorph { get; set; } = null;

        public bool IgnoreRandomGeomorphRotation { get; set; } = false;

        /// <summary>
        /// Which Light to select
        /// </summary>
        public Lights.Light LightSettings { get; set; } = Lights.Light.AlmostWhite_1;

        public Altitude AltitudeData { get; set; } = new Altitude();

        public List<JObject> EventsOnEnter { get; set; } = new List<JObject>();
        public List<JObject> EventsOnPortalWarp { get; set; } = new List<JObject>();
        public List<WardenObjectiveEvent> EventsOnApproachDoor { get; set; } = new List<WardenObjectiveEvent>();
        public List<WardenObjectiveEvent> EventsOnUnlockDoor { get; set; } = new List<WardenObjectiveEvent>();
        public List<WardenObjectiveEvent> EventsOnOpenDoor { get; set; } = new List<WardenObjectiveEvent>();
        public List<WardenObjectiveEvent> EventsOnDoorScanStart { get; set; } = new List<WardenObjectiveEvent>();
        public List<WardenObjectiveEvent> EventsOnDoorScanDone { get; set; } = new List<WardenObjectiveEvent>();

        #region Puzzle settings
        public ProgressionPuzzle ProgressionPuzzleToEnter { get; set; } = new ProgressionPuzzle();

        /// <summary>
        /// Which security scan to use to enter
        /// </summary>
        public UInt32 ChainedPuzzleToEnter { get; set; } = 0;
        #endregion

        public bool IsCheckpointDoor { get; set; } = false;

        public bool PlayScannerVoiceAudio { get; set; } = false;

        public bool SkipAutomaticProgressionObjective { get; set; } = false;

        // TODO: configure enum for gate
        public int SecurityGateToEnter { get; set; } = 0;

        public bool UseStaticBioscanPointsInZone { get; set; } = false;

        public bool TurnOffAlarmOnTerminal { get; set; } = false;

        public JObject TerminalPuzzleZone { get; set; } = new JObject
        {
            ["LocalIndex"] = 0,
            ["SeedType"] = 1,
            ["TerminalIndex"] = 0,
            ["StaticSeed"] = 0
        };

        public JArray EventsOnTerminalDeactivateAlarm = new JArray();

        #region Enemies
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("ActiveEnemyWave")]
        public BloodDoor BloodDoor { get; set; } = BloodDoor.None;

        public List<EnemySpawningData> EnemySpawningInZone { get; set; } = new List<EnemySpawningData>();
        #endregion

        #region Respawn settings
        /// <summary>
        /// Whether the enemies respawn
        /// </summary>
        public bool EnemyRespawning { get; set; } = false;

        public bool EnemyRespawnRequireOtherZone { get; set; } = true;

        public int EnemyRespawnRoomDistance { get; set; } = 2;

        public double EnemyRespawnTimeInterval { get; set; } = 10.0;

        public double EnemyRespawnCountMultiplier { get; set; } = 1.0;

        public JArray EnemyRespawnExcludeList = new JArray();
        #endregion

        public int HSUClustersInZone { get; set; } = 0;

        public int CorpseClustersInZone { get; set; } = 0;
        public int ResourceContainerClustersInZone { get; set; } = 0;
        public int GeneratorClustersInZone { get; set; } = 0;
        public DistributionAmount CorpsesInZone { get; set; } = DistributionAmount.None;
        public DistributionAmount GroundSpawnersInZone { get; set; } = DistributionAmount.Some;
        public DistributionAmount HSUsInZone { get; set; } = DistributionAmount.None;
        public DistributionAmount DeconUnitsInZone { get; set; } = DistributionAmount.None;
        public bool AllowSmallPickupsAllocation { get; set; } = true;
        public bool AllowResourceContainerAllocation { get; set; } = true;
        public bool ForceBigPickupsAllocation { get; set; } = false;
        public int ConsumableDistributionInZone { get; set; } = 62;
        public int BigPickupDistributionInZone { get; set; } = 0;

        public List<TerminalPlacement> TerminalPlacements { get; set; } = new List<TerminalPlacement>();

        public bool ForbidTerminalsInZone { get; set; } = false;
        public JArray PowerGeneratorPlacements { get; set; } = new JArray();
        public JArray DisinfectionStationPlacements { get; set; } = new JArray();
        public JArray DumbwaiterPlacements { get; set; } = new JArray();

        #region Resources
        public double HealthMulti { get; set; } = 1.0;

        public Placement HealthPlacement { get; set; } = new Placement();

        public double WeaponAmmoMulti { get; set; } = 1.0;

        public Placement WeaponAmmoPlacement { get; set; } = new Placement();

        public double ToolAmmoMulti { get; set; } = 1.0;

        public Placement ToolAmmoPlacement { get; set; } = new Placement();

        public double DisinfectionMulti { get; set; } = 0.0;

        public Placement DisinfectionPlacement { get; set; } = new Placement();
        #endregion

        public JArray StaticSpawnDataContainers { get; set; } = new JArray();

        public Zone()
        {
            // Always ensure a terminal is placed in the zone
            TerminalPlacements.Add(new TerminalPlacement());
        }
    }
}
