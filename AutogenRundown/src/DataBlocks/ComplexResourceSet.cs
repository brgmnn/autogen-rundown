using AutogenRundown.DataBlocks.ComplexResourceSets;
using AutogenRundown.DataBlocks.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

/// <summary>
/// This is actually the individual data block in ComplexResourceSet
/// </summary>
public record ComplexResourceSet : DataBlock
{
    #region Properties
    [JsonProperty("name")]
    public new string BlockName { get; set; }

    #region Basic properties
    public int ComplexType { get; set; } = 0;

    public int PrimareSubComplexUsed { get; set; } = 0;

    public int BundleName { get; set; } = 2;

    [JsonProperty("LevelGenConfig")]
    public LevelGenConfig Config { get; set; } = new();

    public bool RandomizeGeomorphOrder { get; set; } = true;
    #endregion

    #region Geomorph Tiles
    public List<Prefab> GeomorphTiles_1x1 { get; set; } = new();

    public List<Prefab> GeomorphTiles_2x1 { get; set; } = new();

    public List<Prefab> GeomorphTiles_2x2 { get; set; } = new();
    #endregion

    #region Small Gates
    public List<Prefab> SmallWeakGates { get; set; } = new();

    public List<Prefab> SmallSecurityGates { get; set; } = new();

    public List<Prefab> SmallApexGates { get; set; } = new();

    public List<Prefab> SmallBulkheadGates { get; set; } = new();

    public List<Prefab> SmallMainPathBulkheadGates { get; set; } = new();
    #endregion

    #region Small Caps
    public List<Prefab> SmallWallCaps { get; set; } = new();

    public List<Prefab> SmallDestroyedCaps { get; set; } = new();

    public List<Prefab> SmallWallAndDestroyedCaps { get; set; } = new();
    #endregion

    #region Medium Gates
    public List<Prefab> MediumWeakGates { get; set; } = new();

    public List<Prefab> MediumSecurityGates { get; set; } = new();

    public List<Prefab> MediumApexGates { get; set; } = new();

    public List<Prefab> MediumBulkheadGates { get; set; } = new();

    public List<Prefab> MediumMainPathBulkheadGates { get; set; } = new();
    #endregion

    #region Medium Caps
    public List<Prefab> MediumWallCaps { get; set; } = new();

    public List<Prefab> MediumDestroyedCaps { get; set; } = new();

    public List<Prefab> MediumWallAndDestroyedCaps { get; set; } = new();
    #endregion

    #region Large Gates
    public List<Prefab> LargeWeakGates { get; set; } = new();

    public List<Prefab> LargeSecurityGates { get; set; } = new();

    public List<Prefab> LargeApexGates { get; set; } = new();

    public List<Prefab> LargeBulkheadGates { get; set; } = new();

    public List<Prefab> LargeMainPathBulkheadGates { get; set; } = new();
    #endregion

    #region Large Caps
    public List<Prefab> LargeWallCaps { get; set; } = new();

    public List<Prefab> LargeDestroyedCaps { get; set; } = new();

    public List<Prefab> LargeWallAndDestroyedCaps { get; set; } = new();
    #endregion

    #region Plugs
    public List<Prefab> StraightPlugsNoGates { get; set; } = new();

    public List<Prefab> StraightPlugsWithGates { get; set; } = new();

    public List<Prefab> SingleDropPlugsNoGates { get; set; } = new();

    public List<Prefab> SingleDropPlugsWithGates { get; set; } = new();

    public List<Prefab> DoubleDropPlugsNoGates { get; set; } = new();

    public List<Prefab> DoubleDropPlugsWithGates { get; set; } = new();

    public List<Prefab> PlugCaps { get; set; } = new();
    #endregion

    #region Elevator Shafts
    /// <summary>
    /// These are the prefabs used when players drop in. It will always be the first 0,0 tile
    /// </summary>
    public List<Prefab> ElevatorShafts_1x1 { get; set; } = new();

    /// <summary>
    /// For forward extraction elevator tiles
    /// </summary>
    public List<Prefab> CustomGeomorphs_Exit_1x1 { get; set; } = new();
    #endregion

    #region Custom Geomorphs / Objectives
    /// <summary>
    /// This list is only used when setting a geomorph with the custom geomorph setting in the tile.
    ///
    /// Most of the custom geos go here where we want to control it in the level gen instead of
    /// allowing the game to run this
    /// </summary>
    [JsonProperty("CustomGeomorphs_Objective_1x1")]
    public List<Prefab> CustomGeomorphs { get; set; } = new();

    /// <summary>
    /// Not sure what this is used for
    /// </summary>
    public List<Prefab> CustomGeomorphs_Challenge_1x1 { get; set; } = new();
    #endregion

    #region Ladders
    public List<Prefab> Ladders_4m { get; set; } = new();

    public List<Prefab> Ladders_2m { get; set; } = new();

    public List<Prefab> Ladders_1m { get; set; } = new();

    public List<Prefab> Ladders_05m { get; set; } = new();

    public List<Prefab> Ladders_Bottom { get; set; } = new();

    public List<Prefab> Ladders_Top { get; set; } = new();
    #endregion
    #endregion

    public static ComplexResourceSet Mining { get; set; } = new()
    {
        PersistentId = (uint)Complex.Mining
    };

    public static ComplexResourceSet Tech { get; set; } = new()
    {
        PersistentId = (uint)Complex.Tech
    };

    public static ComplexResourceSet Service { get; set; } = new()
    {
        PersistentId = (uint)Complex.Service
    };

    public ComplexResourceSet(PidOffsets offsets = PidOffsets.Normal)
        : base(Generator.GetPersistentId(offsets))
    { }

    public static ComplexResourceSet Find(uint persistentId) => Bins.ComplexResources.Find(persistentId)!;

    public static void Setup()
        => Setup<GameDataComplexResourceSet, ComplexResourceSet>(Bins.ComplexResources, "ComplexResourceSet");

    public new static void SaveStatic()
    {
        Mining = Find(Mining.PersistentId);
        Tech = Find(Tech.PersistentId);
        Service = Find(Service.PersistentId);

        // Use .Insert(0, ...) instead of .Add(). For some reason appending the blocks to the end
        // of the list causes problems

        #region Mining
        ///
        /// Mining (Storage / Refinery / Digsite) custom geomorphs
        ///

        #region Base game
        // R5D1 overload pit hub
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_32x32_elevator_shaft_dig_site_04.prefab",
            SubComplex = SubComplex.DigSite
        });

        // R8B3 tower
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_04_test.prefab",
            SubComplex = SubComplex.Storage
        });
        #endregion

        #region MODDER: dakkhuza
        // Modder notes:
        //
        // > This tile is a T intersection. Exits are forward and right. There is a flyer
        // > spawnpoint in the fan. A terminal can spawn inside the small elevator.
        // > There is a variation without the spawnpoints called
        // > "assets/geo_64x64_mining_dig_site_t_dak_01_nospawn.prefab"
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_mining_dig_site_t_dak_01.prefab",
            SubComplex = SubComplex.DigSite
            // Shard = "2"
        });

        // Modder notes:
        //
        // > This tile is a bridge tile. There is one exit opposite the entrance. The bridge has
        // > a roof area and catwalks running underneath. There is a variation without the
        // > spawnpoints called "assets/geo_64x64_dig_site_i_dak_01_nospawn.prefab"
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_dig_site_i_dak_01.prefab",
            SubComplex = SubComplex.DigSite
            // Shard = "2"
        });

        // Modder notes (for both tiles):
        //
        // > This tile is configured as an exit. It is a dead end tile. There are open areas to the
        // > left and right of the evac point. There is a variation of this configured as a regular
        // > dead end tile, "ASSETS/GEO_64X64_DIG_SITE_DEAD_END_DAK_01.PREFAB"
        // > Use the dead end tile if you don't want to be forced into a forward evac.
        Mining.CustomGeomorphs_Exit_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_dig_site_exit_dak_01.prefab",
            SubComplex = SubComplex.DigSite
            // Shard = "2"
        });
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_dig_site_dead_end_dak_01.prefab",
            SubComplex = SubComplex.DigSite
            // Shard = "2"
        });
        #endregion

        #region MODDER: Floweria
        // Floweria tiles
        Mining.GeomorphTiles_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Mining/geo_storage_FA_01.prefab",
            SubComplex = SubComplex.Storage
        });
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Mining/geo_storage_FA_dead_end_01.prefab",
            SubComplex = SubComplex.Storage
        });
        #endregion

        #region MODDER: Red_Leicester_Cheese
        // Simple dead end tile
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_digsite_dead_end_RLC_01.prefab",
            SubComplex = SubComplex.DigSite
        });

        // Non-destroyed elevator exit.
        // TODO: not totally convinced on keeping this one
        Mining.CustomGeomorphs_Exit_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_storage_exit_hub_RLC_01.prefab",
            SubComplex = SubComplex.Storage
        });

        // Corridor / I tile
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_storage_I_RLC_01.prefab",
            SubComplex = SubComplex.Storage
        });

        // Hub tile
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_refinery_X_RLC_01.prefab",
            SubComplex = SubComplex.Refinery
        });

        // Basic Corridor I-tile
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_refinery_I_RLC_01.prefab",
            SubComplex = SubComplex.Refinery
        });

        // Corridor I-tile with a broken portal machine.
        // NOTE: Has no good terminal spawns it seems and there's a model prisoner under the tile.
        // Reference model perhaps?
        // TODO: Decide if we add this to the corridor list
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_refinery_portal_I_RLC_01.prefab",
            SubComplex = SubComplex.Refinery
        });

        // Reactor tile for digsite.
        // TODO: This one needs custom lighting (see the CheeseGeo readme). Adding this will have
        // to wait on working on the custom LightSettings.
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_reactor_RLC_01.prefab",
            SubComplex = SubComplex.All
        });
        #endregion

        #region MODDER: SamDB
        // I tile with multiple caves and tunnels that run deep underground. A large cave sits at
        // the bottom with a flooded pool and a bridge connecting both ways.
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Digsite/disite_i_tile_1_optimized/digsite_i_tile_1_V2.prefab",
            SubComplex = SubComplex.DigSite
        });

        // A big room with multiple pathways and elevation. There are underground caves and a
        // small tunnel connecting them together
        //
        // V3: Alternate version with one big cave... The smaller cave has been removed...
        // Still a large tile but with less verticality...
        //
        // TODO: This would be quite a hard tile to get for a hub
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Digsite/digsite_x_tile_1/digsite_x_tile_1.prefab",
            SubComplex = SubComplex.DigSite
        });
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Digsite/digsite_x_tile_1_V3/digsite_x_tile_1_V3.prefab",
            SubComplex = SubComplex.DigSite
        });

        // A generator tile with a large maze of corridors underneath. It has built in custom
        // scans going up to a Tier 9.
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Digsite/Disite generator/Digsite_X_Tile_Generator.prefab",
            SubComplex = SubComplex.DigSite
        });

        // A compact tile with multiple elevation points. Multiple bridges and lots of depth in
        // this tile.
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/refinery/HA_1_i_tile/I_tile_V2.prefab",
            SubComplex = SubComplex.Refinery
        });

        // Refinery bridge with 3 floors. This tile has a built-in scan align going up to a
        // Tier 4 scan. The scans will start on the bottom floor, traversing to the middle and
        // ending at the top.
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/refinery bridge/refinery_i_tile_bridge.prefab",
            SubComplex = SubComplex.Refinery
        });

        // A dilapidated bridge that the warden deems viable for an extract point
        Mining.CustomGeomorphs_Exit_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Mining exit/Mining_exit_V1.prefab",
            SubComplex = SubComplex.Refinery
        });

        // A dilapidated bridge that the warden deems viable for an extract point
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Storage_neonate_room/neonate_storage.prefab",
            SubComplex = SubComplex.Storage
        });

        #region Geos V2

        // Large Digsite tile for the MWP or other large pickup objects.
        //
        // TODO: Add this and the other big pickup tiles for non MWP big pickups
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/SamdownGeos/T2D2 MWP room/MWP_Room.prefab",
            SubComplex = SubComplex.DigSite
        });

        // Simple storage elevator re-imagined.
        Mining.ElevatorShafts_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/SamdownGeos/Storage Elevator 1/Storage_elevator_1.prefab",
            SubComplex = SubComplex.Refinery
        });

        // Simple storage dead end for spawning enemies.
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/SamdownGeos/Storage dead end spawn/DeadEnd_Storage.prefab",
            SubComplex = SubComplex.Storage
        });

        // Large refinery tile with multiple elevators and enemy spawnpoints.
        Mining.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/SamdownGeos/Refinery_X_tile_1/Refinery_X_Tile_1.prefab",
            SubComplex = SubComplex.Refinery
        });

        #endregion
        #endregion
        #endregion

        #region Tech
        ///
        /// Tech (Datacenter / Lab) custom geomorphs
        ///

        #region Base game
        // Remove problem tile
        //
        // geo_64x64_tech_data_center_HA_05 is a large two-level tile, however we _often_ see
        // failed map generations because it has a side door that leads to the under side of it
        // with no way to connect to other tiles. This often manifests with odd tiny rooms and
        // then the rest of the level spawning not off the correct tile
        Tech.GeomorphTiles_1x1.RemoveAll(prefab =>
            prefab.Asset == "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_HA_05.prefab");

        // Add the problem tile to the Custom Geomorphs so we can still use it, because it is
        // still a cool tile
        Tech.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_HA_05.prefab",
            SubComplex = SubComplex.DataCenter,
            Shard = 4
        });

        // Add the boss pillar room for mega mom / other bosses
        Tech.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_R3D1.prefab",
            SubComplex = SubComplex.All,
            Shard = 18
        });

        // Rundown 7 tiles

        // Rundown 8 tiles
        #endregion

        #region MODDER: donan3967
        // Confired that this geo only supports a forward/backward zone connection.
        // MaxConnections should be set to 2.
        // Notes from modder:
        //
        // > A relatively simple I tile, with a lot of robot props and server markers. More open
        // > than most datacenter tiles, with a few different levels of elevation (could be good
        // > for flyer scans). Has four static bisocan points around the room.
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_tech_data_center_I_tile_DS_1.prefab",
            SubComplex = SubComplex.DataCenter
            // "Shard": 10
        });

        // Simple hub tile, 4 connections. Notes from modder:
        //
        // > A simple tile, aesthetically designed to be a storage area. Pillars in the center
        // > area can spawn bigpickup items, though you'll probably have to reroll markers to get
        // > what you want. Has a static bioscan point in front of the central pillar on the
        // > bottom level. The lights in the floor are marked as Independent if you want to change
        // > their color for an "emergency lighting" feel.
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_tech_data_center_hub_DS_01.prefab",
            SubComplex = SubComplex.DataCenter
            // "Shard": 10
        });

        // A tech reactor in the datacenter style. Two sets of double bridges, with a lower area
        // on both sides. Relatively cramped for a reactor.
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_data_center_reactor_DS_01.prefab",
            SubComplex = SubComplex.DataCenter
            // "Shard": 10
        });
        #endregion

        #region MODDER: Floweria
        // Floweria tiles
        // TODO: does this work?
        Tech.GeomorphTiles_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_01.prefab",
            SubComplex = SubComplex.Lab
        });

        // Enemy spawn point version: "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_dead_end_01_enemyspawn.prefab"
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_dead_end_01.prefab",
            SubComplex = SubComplex.Lab
        });
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_I_01.prefab",
            SubComplex = SubComplex.DataCenter
        });

        // Version without exit: "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_exit_01_noexit.prefab"
        // Perhaps that could be a dead end tile?
        Tech.CustomGeomorphs_Exit_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_exit_01.prefab",
            SubComplex = SubComplex.DataCenter
        });
        Tech.ElevatorShafts_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_elevator_shaft_01.prefab",
            SubComplex = SubComplex.DataCenter
        });
        #endregion

        #region MODDER: Red_Leicester_Cheese
        // Lab Corridor
        // NOTE: they have a weird stair railing on the way to the bottom level. It goes
        // into the ground
        // TODO: Saw a big empty hole in the ground for this. Probably disable it.
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Tech/geo_64x64_tech_lab_I_RLC_01.prefab",
            SubComplex = SubComplex.Lab
        });

        // // Hub tile
        // // Disabled as there's a NCR machine that breaks ActivateSmallHSU objectives
        // Tech.CustomGeomorphs.Insert(0, new Prefab()
        // {
        //     Asset = "Assets/Bundles/RLC_Tech/geo_64x64_tech_lab_Hub_RLC_01.prefab",
        //     SubComplex = SubComplex.Lab
        // });

        // Hub tile
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Tech/geo_64x64_tech_data_center_hub_JG_RLC_02_v3.prefab",
            SubComplex = SubComplex.DataCenter
        });

        // Corridor I-tile
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Tech/geo_64x64_tech_datacenter_I_RLC_01.prefab",
            SubComplex = SubComplex.DataCenter
        });
        #endregion

        #region MODDER: SamDB
        // A revamped labs elevator tile. It has a full 360 circular pathway as well as a
        // lower floor.
        Tech.ElevatorShafts_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Labs/Labs Elevator/geo_32x32_Labs_Elevator_V1.prefab",
            SubComplex = SubComplex.Lab
        });

        // A completely remade elevator tile, multiple levels for verticality. Made to look
        // abandoned yet still maintained.
        Tech.ElevatorShafts_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Labs/Labs Elevator V2/Labs_elevator_V2.prefab",
            SubComplex = SubComplex.Lab
        });

        // A big tile with three depressurizer's that are randomized every time. It is an open
        // tile with multiple pathways and three floors.
        //
        // TODO: incorporate into neonate depressurization mission
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Labs/Labs neonate insertion room/Labs_Neonate_Insertion_room_Depressure.prefab",
            SubComplex = SubComplex.Lab
        });

        // The normal reactor I tile except there are multiple paths that connect the tile together.
        // The middle path has been destroyed and side paths are now present.
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Labs/lab_i_tile/lab_i_tile_V1.prefab",
            SubComplex = SubComplex.Lab
        });

        // A dead end tile similar to the R7A1 variant. However, this tile now has marker spawns
        // for objective items.
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Labs/Labs dead end neonate cargo room/Labs_Neonate_cargo.prefab",
            SubComplex = SubComplex.Lab
        });

        // A big labs exit tile with a boss align spawn.
        Tech.CustomGeomorphs_Exit_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Labs/lab_exit_V1/lab_exit_V1.prefab",
            SubComplex = SubComplex.Lab
        });

        #region Geos V2

        // Extended Data Center tile with extra floors.
        Tech.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/SamdownGeos/C2 Mainframe room/C2_Mainframe.prefab",
            SubComplex = SubComplex.DataCenter
        });

        #endregion
        #endregion
        #endregion

        #region Service Floodways
        ///
        /// Service (Floodways / Gardens) custom geomorph updates
        ///

        #region Base game
        // We want to have a mix between Complex=27 and Complex=53. 27 is just Floodways and
        // 53 is Floodways with garden tiles. The garden tiles are particularly hard to balance
        // so the custom version we use doesn't have the garden tiles in the regular random roll
        // list, but instead can be placed as custom geos

        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_dead_end_HA_02.prefab",
            SubComplex = SubComplex.Floodways
        });
        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            // Mega Nightmare mother room
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03_V2.prefab",
            SubComplex = SubComplex.Floodways
        });

        #region Garden GeomorphTiles_1x1 -> CustomGeomorphs_Objectives_1x1
        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_HA_01.prefab",
            Shard = 7,
            SubComplex = SubComplex.Gardens
        });
        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_HA_02.prefab",
            Shard = 8,
            SubComplex = SubComplex.Gardens
        });
        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_SF_01.prefab",
            Shard = 7,
            SubComplex = SubComplex.Gardens
        });
        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_AW_01.prefab",
            Shard = 9,
            SubComplex = SubComplex.Gardens
        });
        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_JG_01.prefab",
            Shard = 10,
            SubComplex = SubComplex.Gardens
        });
        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_JG_02.prefab",
            Shard = 11,
            SubComplex = SubComplex.Gardens
        });
        Service.CustomGeomorphs.Insert(0, new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_HA_03.prefab",
            Shard = 8,
            SubComplex = SubComplex.Gardens
        });
        #endregion

        #region Garden Wall Caps
        // Small
        Service.SmallWallCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Gates/4x4/gate_4x4_gardens_cap_wall_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });
        Service.SmallDestroyedCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Gates/4x4/gate_4x4_gardens_cap_destroyed_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });
        Service.SmallWallAndDestroyedCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Gates/4x4/gate_4x4_gardens_cap_destroyed_wall_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });

        // Medium
        Service.MediumWallCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Gates/8x4/gate_8x4_floodways_cap_wall_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });
        Service.MediumDestroyedCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Gates/8x4/gate_8x4_floodways_cap_destroyed_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });
        Service.MediumWallAndDestroyedCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Gates/8x4/gate_8x4_floodways_cap_destroyed_wall_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });

        // Large
        Service.LargeWallCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Generic/Gates/8x8/gate_8x8_cap_wall_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 16
        });
        Service.LargeDestroyedCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Generic/Gates/8x8/gate_8x8_cap_wall_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 16
        });
        Service.LargeWallAndDestroyedCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Generic/Gates/8x8/gate_8x8_cap_wall_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 16
        });

        // Plug Caps
        Service.PlugCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Plugs/env_plug_8mheight_cap_gardens_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });
        Service.PlugCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Plugs/env_plug_8mheight_cap_gardens_02.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });
        Service.PlugCaps.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Plugs/env_plug_8mheight_cap_gardens_03.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 15
        });
        #endregion

        #region Custom Geomorphs
        Service.ElevatorShafts_1x1.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/geo_32x32_elevator_shaft_Gardens_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 7
        });

        Service.CustomGeomorphs_Exit_1x1.Add(new Prefab
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/geo_32x32_elevator_Gardens_exit_01.prefab",
            SubComplex = SubComplex.Gardens,
            Shard = 4
        });

        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_I_01.prefab",
          SubComplex = SubComplex.Gardens,
          Shard = 8
        });
        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_X_01.prefab",
          SubComplex = SubComplex.Gardens,
          Shard = 9
        });
        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_Lab_HSU_Womb.prefab",
          SubComplex = SubComplex.Gardens,
          Shard = 8
        });
        // TODO: what is this tile?
        // Service.CustomGeomorphs.Add(new Prefab
        // {
        //   Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_I_HA_01_R7D1.prefab",
        //   SubComplex = SubComplex.All,
        //   Shard = 5
        // });
        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_Lab_HSU_Prep.prefab",
          SubComplex = SubComplex.Gardens,
          Shard = 9
        });
        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_Lab_HSU_Prep_TestingFunc.prefab",
          SubComplex = SubComplex.All,
          Shard = 9
        });
        // Service.CustomGeomorphs.Add(new Prefab
        // {
        //   Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_Lab_HSU_Prep_Timur.prefab",
        //   SubComplex = SubComplex.Gardens,
        //   Shard = 9
        // });
        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_hub_SF_01.prefab",
          SubComplex = SubComplex.Gardens,
          Shard = 9
        });
        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_I_02_SF.prefab",
          SubComplex = SubComplex.Gardens,
          Shard = 8
        });
        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_dead_end_SF.prefab",
          SubComplex = SubComplex.Gardens,
          Shard = 8
        });
        Service.CustomGeomorphs.Add(new Prefab
        {
          Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_VS_01.prefab",
          SubComplex = SubComplex.Gardens,
          Shard = 8
        });

        #endregion
        #endregion

        #region MODDER: donan3967
        #region geo_pack_1
        // Floodways hub with tower at the top. Plenty of verticality, it may be hard with some
        // scans going up/down. Modder notes:
        //
        // > A big pit with catwalks and a central pillar. Terminals, generators, big pickup
        // > shelves, and decontaminators can spawn on all four sides of the central pillar.
        // > There are several invisible walls to prevent players from falling off of the catwalks,
        // > though you can drop down from the stairs onto a lower catwalk in a specific place on
        // > the stairs (if youre willing to take a ton of fall damage.) Does have a central
        // > bioscan point for an arena fight, but havent fully tested how well it works.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_hub_DS_01.prefab",
            SubComplex = SubComplex.Floodways,
            // Shard = "10"
        });

        // Floodways boss hub. Modder notes:
        //
        // > A large, open room ideal for bossfights. Has a boss align and scan align in the center
        // > of the massive pipe cover on the floor. Looks best at high elevation with minimal fog,
        // > otherwise the center is engulfed.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_boss_hub_DS_01.prefab",
            SubComplex = SubComplex.Floodways,
            // Shard = "10"
        });
        #endregion

        #region geo_pack_2
        // Open, symmetrical hub tile focused around a big central marker (the one with the deep
        // hole). Feel of the tile can change a lot based on the marker rolled, but it stays
        // relatively flat.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_hub_ds_02.prefab",
            SubComplex = SubComplex.Floodways
        });

        // Same as previous tile, but instead of rolling a big marker (with many submarkers) in
        // the middle, it only has the new FW gencluster marker. Designed to make it easier to
        // roll gencluster objective.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_hub_ds_02_gen.prefab",
            SubComplex = SubComplex.Floodways
        });

        // Floodways Reactor! Based on the R8 tile (geo_64x64_service_floodways_I_HA_04) with some
        // added resource markers and reactor functionality. Still an I tile though. Retains the
        // World event for flyer spawns, as well as WE T-scan from that tile.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_reactor_ds_01.prefab",
            SubComplex = SubComplex.Floodways
        });

        // Unique bridge tile. Has a funky pipe in the middle you can cross above and below.
        // Comes with flyer spawns and a Tscan path around the bridges.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_i_bridge_ds_01.prefab",
            SubComplex = SubComplex.Floodways
        });

        // An armory tile that actually looks like an armory instead of just a storage space,
        // Floodways theme. Keep in mind it is small and a dead end. Has a marker for a terminal
        // at the back and a mini terminal on the left side. Weapon lockers are purely for
        // decoration, they can't be accessed.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_armory_DS_01.prefab",
            SubComplex = SubComplex.Floodways,
            // Shard = 10
        });

        // Another Floodways reactor, but a four-way hub this time. Multi-level. Also included is
        // a version that's just a hub, with no reactor objective on it (exactly the same as
        // below, but called geo_64x64_service_floodways_reactor_ds_02_justhub.prefab instead.)
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_reactor_ds_02.prefab",
            SubComplex = SubComplex.Floodways
        });
        // Just the hub version
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_reactor_ds_02_justhub.prefab",
            SubComplex = SubComplex.Floodways
        });
        #endregion
        #endregion

        #region MODDER: Floweria
        // Floweria tiles

        // Modder notes:
        //
        // > Reactor tile, Also have bulkhead spawn on middle where the reactor spawns
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Service/geo_floodways_FA_reactor_01.prefab",
            SubComplex = SubComplex.Floodways
        });
        #endregion

        #region MODDER: SamDB
        #region Geos V1
        // A revamped floodways elevator tile. It has been largely extended to feature a custom
        // spawn and revamped to look less destroyed and more abandoned. It has a full 360 pathway
        // and a bridge with a custom exit scan aligned to it. The ladder has been removed to stop
        // all ladder cheesing for this tile.
        Service.ElevatorShafts_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways Elevator 1/Floodways_Elevator_1.prefab",
            SubComplex = SubComplex.Floodways
        });

        // The regular floodways bridge with an extra floor underneath for traversing. Lots of
        // extra room and locker spawns with an underground middle section as well. A great tile
        // for hectic scans.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways_I_tile_bridge/Floodways_I_tile_Bridge.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // A long tile with walkways at the top, 3 bridges in the middle and a hole underneath.
        // This tile has a set spawn for single scans, works best with the large S scan.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/floodways_I_tile_1/floodways_i_tile_1.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // The same as the vanilla tile except the middle has been hollowed out and a massive hole
        // is at the top. There's also more sewer grates and flowing water on the floor. More of
        // an aesthetic tile. This tile also comes with a scan align for an arena fight.
        //
        // Also comes with:
        //  * boss aligned - "Assets/Custom Geo's/Floodways_x_tile_1_V2/floodways_x_tile_1_V2.prefab"
        //  * flyer spawn  - "Assets/Custom Geo's/Floodways_x_tile_1_V3/floodways_x_tile_1_V3.prefab"
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways_x_tile_1/floodways_x_tile_1.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // A large room with a hollow middle that extends all the way down. No terminals or boxes
        // spawn down there but enemies can.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways_x_tile_2/floodways_x_tile_2.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // A big room with a large hole at the start for water storage. Can be used as a
        // T-scan tile. Has scaffolding and platforms round the side.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways_x_tile_3/floodways_x_tile_3.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // A massive room with walkways and platforms that span up to 4 floors. Terminals can
        // spawn at the bottom as well as resource boxes. This tile also has spawn-points for flyers.
        // Note: This is the biggest tile in the game and can sometimes cause some FPS drops.
        //
        // This is a HUGE tile. Probably it can't be placed in any hub as balancing may be a problem?
        // TODO: figure out a way to use it
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways_x_tile_4/floodway_x_tile_4.prefab",
            SubComplex = SubComplex.Floodways
        });

        // A remastered Floodways vanilla tile. Has multiple bridges and pathways and is now one
        // zone instead of three... A long bridge sits in the middle of the tile and leads to
        // every part of the room.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways_x_tile_5/Floodways_x_tile_5.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // The big HUB room with three extra levels doing down into the depths below.
        //
        // TODO: figure out how to use this, same as x_tile_4
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways_HUB/Floodways_hub.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // A deadend tile with spawnpoints for waves. A tile with a narrow corridor for
        // holding enemies. It has been made to look very degraded and worn.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways dead end spawn points/Floodways_Deadend_Spawn.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // Large exit tile with multiple scaffolding walkways, allowing full access around the room
        Service.CustomGeomorphs_Exit_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/Custom Geo's/Floodways_exit/floodways_exit_tile.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });
        #endregion

        #region Geos V2

        // Very large floodways HUB tile. Multiple floors and spawn points for flyers.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/SamdownGeos/Floodways Scaffolding HUB/Floodways_Scaffolding_HUB.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // I tile with multiple floors.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/SamdownGeos/Floodways I Tile F1/Floodways_i_Tile_F1.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // Simple dead end tile that can be used to spawn enemies with custom plugins. There are
        // invisible ladders on each sewer hole for enemies to jump down.
        Service.CustomGeomorphs.Insert(0, new Prefab()
        {
            Asset = "Assets/GameObject/Floodways_Reactor_Cooling_Spawns.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        // Floodways exit tile for arena fights. There are invisible ladders on each hole for
        // enemies to jump down. Enemies must be spawned in using custom plugins for this to work.
        Service.CustomGeomorphs_Exit_1x1.Insert(0, new Prefab()
        {
            Asset = "Assets/SamdownGeos/Floodways Abberation Exit/Abberation_Exit_tile.prefab",
            SubComplex = SubComplex.Floodways,
            Shard = 10
        });

        #endregion
        #endregion
        #endregion
    }
}

public record GameDataComplexResourceSet : ComplexResourceSet
{
    public GameDataComplexResourceSet() : base(PidOffsets.None)
    { }
}
