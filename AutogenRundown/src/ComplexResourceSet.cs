using AutogenRundown.DataBlocks;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown;

public class ComplexResourceSet
{
    private const string FileName = "GameData_ComplexResourceSetDataBlock_bin.json";

    private JObject resourceSet;

    /// <summary>
    ///
    /// </summary>
    /// <exception cref="Exception"></exception>
    public ComplexResourceSet()
    {
        var from = Path.Combine(Paths.PluginPath, Plugin.Name, FileName);

        using var sourceFile = File.OpenText(from);
        using var reader = new JsonTextReader(sourceFile);

        resourceSet = (JObject)JToken.ReadFrom(reader);

        if (resourceSet["Blocks"] == null)
        {
            Plugin.Logger.LogFatal("No complex resource set data blocks found");
            throw new Exception("No complex resource set data blocks found");
        }
    }

    /// <summary>
    ///
    /// </summary>
    private void WriteFile()
    {
        var revision = CellBuildData.GetRevision();

        var from = Path.Combine(Paths.PluginPath, Plugin.Name, FileName);
        var destDir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}");
        var dest = Path.Combine(destDir, FileName);

        // Ensure the directory exists
        Directory.CreateDirectory(destDir);

        // write JSON directly to a file
        using var destFile = File.CreateText(dest);
        using var writer = new JsonTextWriter(destFile);

        resourceSet.WriteTo(writer);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="persistentId"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public JArray GetPrefabs(int persistentId, string group)
    {
        var blocks = resourceSet["Blocks"]!.OfType<JObject>();
        var complexResource = blocks.First(block => (int?)block["persistentID"] == persistentId);

        if (complexResource[group] == null)
        {
            Plugin.Logger.LogFatal("No complex resource set data blocks found");
            throw new Exception("No complex resource set data blocks found");
        }

        return (JArray)complexResource[group]!;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="complex"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public JArray GetPrefabs(Complex complex, string group) => GetPrefabs((int)complex, group);

    /// <summary>
    ///
    /// </summary>
    /// <param name="complex"></param>
    /// <param name="group"></param>
    /// <param name="prefab"></param>
    public void AddPrefab(Complex complex, string group, Prefab prefab)
        => GetPrefabs(complex, group).Insert(0, JObject.FromObject(prefab));

    /// <summary>
    /// Where to put new prefabs of geos that you're adding?
    ///
    ///     * Generic tiles -> GeomorphTiles_1x1
    ///     * Dead ends / more custom things -> CustomGeomorphs_Objective_1x1
    ///     * Elevator drops -> ElevatorShafts_1x1
    ///     * Elevator exit -> CustomGeomorphs_Exit_1x1
    /// </summary>
    public static void Setup()
    {
        var resourceSet = new ComplexResourceSet();

        #region Mining
        ///
        /// Mining (Storage / Refinery / Digsite) custom geomorphs
        ///

        #region Base game
        // R5D1 overload pit hub
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_32x32_elevator_shaft_dig_site_04.prefab",
            SubComplex = SubComplex.DigSite
        });

        // R8B3 tower
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
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
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
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
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
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
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Exit_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_dig_site_exit_dak_01.prefab",
            SubComplex = SubComplex.DigSite
            // Shard = "2"
        });
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_dig_site_dead_end_dak_01.prefab",
            SubComplex = SubComplex.DigSite
            // Shard = "2"
        });
        #endregion

        #region MODDER: Floweria
        // Floweria tiles
        resourceSet.AddPrefab(Complex.Mining, "GeomorphTiles_1x1", new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Mining/geo_storage_FA_01.prefab",
            SubComplex = SubComplex.Storage
        });
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Mining/geo_storage_FA_dead_end_01.prefab",
            SubComplex = SubComplex.Storage
        });
        #endregion

        #region MODDER: Red_Leicester_Cheese
        // Simple dead end tile
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_digsite_dead_end_RLC_01.prefab",
            SubComplex = SubComplex.DigSite
        });

        // Non-destroyed elevator exit.
        // TODO: not totally convinced on keeping this one
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Exit_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_storage_exit_hub_RLC_01.prefab",
            SubComplex = SubComplex.Storage
        });

        // Corridor / I tile
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_storage_I_RLC_01.prefab",
            SubComplex = SubComplex.Storage
        });

        // Hub tile
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_refinery_X_RLC_01.prefab",
            SubComplex = SubComplex.Refinery
        });

        // Basic Corridor I-tile
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_refinery_I_RLC_01.prefab",
            SubComplex = SubComplex.Refinery
        });

        // Corridor I-tile with a broken portal machine.
        // NOTE: Has no good terminal spawns it seems and there's a model prisoner under the tile.
        // Reference model perhaps?
        // TODO: Decide if we add this to the corridor list
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_refinery_portal_I_RLC_01.prefab",
            SubComplex = SubComplex.Refinery
        });

        // Reactor tile for digsite.
        // TODO: This one needs custom lighting (see the CheeseGeo readme). Adding this will have
        // to wait on working on the custom LightSettings.
        resourceSet.AddPrefab(Complex.Mining, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Mining/geo_64x64_mining_reactor_RLC_01.prefab",
            SubComplex = SubComplex.All
        });
        #endregion
        #endregion

        #region Tech
        ///
        /// Tech (Datacenter / Lab) custom geomorphs
        ///

        #region Base game
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
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
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
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_tech_data_center_hub_DS_01.prefab",
            SubComplex = SubComplex.DataCenter
            // "Shard": 10
        });

        // A tech reactor in the datacenter style. Two sets of double bridges, with a lower area
        // on both sides. Relatively cramped for a reactor.
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_data_center_reactor_DS_01.prefab",
            SubComplex = SubComplex.DataCenter
            // "Shard": 10
        });
        #endregion

        #region MODDER: Floweria
        // Floweria tiles
        // TODO: does this work?
        resourceSet.AddPrefab(Complex.Tech, "GeomorphTiles_1x1", new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_01.prefab",
            SubComplex = SubComplex.Lab
        });

        // Enemy spawn point version: "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_dead_end_01_enemyspawn.prefab"
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_dead_end_01.prefab",
            SubComplex = SubComplex.Lab
        });
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_I_01.prefab",
            SubComplex = SubComplex.DataCenter
        });

        // Version without exit: "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_exit_01_noexit.prefab"
        // Perhaps that could be a dead end tile?
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Exit_1x1", new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_exit_01.prefab",
            SubComplex = SubComplex.DataCenter
        });
        resourceSet.AddPrefab(Complex.Tech, "ElevatorShafts_1x1", new Prefab()
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
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Tech/geo_64x64_tech_lab_I_RLC_01.prefab",
            SubComplex = SubComplex.Lab
        });

        // Hub tile
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Tech/geo_64x64_tech_lab_Hub_RLC_01.prefab",
            SubComplex = SubComplex.Lab
        });

        // Hub tile
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Tech/geo_64x64_tech_data_center_hub_JG_RLC_02_v3.prefab",
            SubComplex = SubComplex.DataCenter
        });

        // Corridor I-tile
        resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Bundles/RLC_Tech/geo_64x64_tech_datacenter_I_RLC_01.prefab",
            SubComplex = SubComplex.DataCenter
        });
        #endregion
        #endregion

        #region Service Floodways
        ///
        /// Service (Floodways / Gardens) custom geomorph updates
        ///

        // Base game
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_dead_end_HA_02.prefab",
            SubComplex = SubComplex.Floodways
        });
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            // Mega Nightmare mother room
            Asset = "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03_V2.prefab",
            SubComplex = SubComplex.Floodways
        });

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
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
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
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
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
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_hub_ds_02.prefab",
            SubComplex = SubComplex.Floodways
        });

        // {
        //     "Prefab": "Assets/geo_64x64_service_floodways_hub_ds_02_gen.prefab",
        //     "SubComplex": "Floodways",
        //     "Shard": 10
        // }

        // Floodways Reactor! Based on the R8 tile (geo_64x64_service_floodways_I_HA_04) with some
        // added resource markers and reactor functionality. Still an I tile though. Retains the
        // World event for flyer spawns, as well as WE T-scan from that tile.
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_reactor_ds_01.prefab",
            SubComplex = SubComplex.Floodways
        });

        // Unique bridge tile. Has a funky pipe in the middle you can cross above and below.
        // Comes with flyer spawns and a Tscan path around the bridges.
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_i_bridge_ds_01.prefab",
            SubComplex = SubComplex.Floodways
        });

        // An armory tile that actually looks like an armory instead of just a storage space,
        // Floodways theme. Keep in mind it is small and a dead end. Has a marker for a terminal
        // at the back and a mini terminal on the left side. Weapon lockers are purely for
        // decoration, they can't be accessed.
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_armory_DS_01.prefab",
            SubComplex = SubComplex.Floodways,
            // Shard = 10
        });

        // Another Floodways reactor, but a four-way hub this time. Multi-level. Also included is
        // a version that's just a hub, with no reactor objective on it (exactly the same as
        // below, but called geo_64x64_service_floodways_reactor_ds_02_justhub.prefab instead.)
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/geo_64x64_service_floodways_reactor_ds_02.prefab",
            SubComplex = SubComplex.Floodways
        });
        // Just the hub version
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
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
        resourceSet.AddPrefab(Complex.Service, "CustomGeomorphs_Objective_1x1", new Prefab()
        {
            Asset = "Assets/Prefabs/Geomorph/Service/geo_floodways_FA_reactor_01.prefab",
            SubComplex = SubComplex.Floodways
        });
        #endregion
        #endregion

        resourceSet.WriteFile();
    }
}
