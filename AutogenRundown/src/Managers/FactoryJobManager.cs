using AIGraph;
using CellMenu;
using GameData;
using Globals;
using LevelGeneration;

namespace AutogenRundown.Managers;

public class FactoryJobManager
{
    /// <summary>
    /// Register new validations here to be called when LG_Factory.FactoryDone is called. These
    /// methods will validate the state of the level with whatever code they want and can return
    /// true/false. True is a passing value and will allow the game to continue, false will trigger
    /// a level rebuild.
    /// </summary>
    public static Func<bool> OnDoneValidate { get; } = () => true;

    public static bool ShowMessage { get; private set; }

    public static bool ShouldRebuild { get; private set; }

    public static bool Rebuilding { get; private set; }

    public static int RebuildCount { get; private set; }

    public static void FlashMessage()
    {
        ShowMessage = true;

        DropServerManager.Current.Update();

        Task.Run(async () =>
        {
            await Task.Delay(4_000);
            ShowMessage = false;
        });
    }

    public static void MarkForRebuild()
    {
        ShouldRebuild = true;
    }

    #region Build / Cleanup

    /// <summary>
    ///
    /// </summary>
    public static void Rebuild()
    {
        RebuildCount++;

        var activeExpedition = RundownManager.GetActiveExpeditionData();
        var expeditionData = RundownManager.ActiveExpedition;

        // WardenObjectiveManager.Current.Setup();

        FlashMessage();

        Builder.Current.Build();

        WardenObjectiveManager.Current.OnExpeditionUpdated(activeExpedition, expeditionData);
    }

    /// <summary>
    /// Performs a level cleanup so a rebuild can occur. This mostly follows what the base game
    /// does for level cleanup.
    /// </summary>
    public static void LevelCleanup()
    {
        // --- Level ---
        LG_BuildNodeCluster.LevelCleanup();
        LG_FunctionMarkerBuilder.LevelCleanup();
        LG_MarkerFactory.Cleanup();
        LG_PrefabSpawner.OnLevelCleanup();
        LG_LevelInteractionManager.Current.OnLevelCleanup();

        // // HSUs
        // WO_ActivateSmallHSU.OnLevelCleanup();

        // --- Minimap ---
        MapDetails.OnLevelCleanup();
        MapDataManager.Current.OnLevelCleanup();
        CM_PageMap.Current.OnLevelCleanup();

        // --- Terminals ---
        // Remove terminals/uplinks from previous attempts
        foreach (var terminal in UnityEngine.Object.FindObjectsOfType<LG_ComputerTerminal>())
        {
            terminal.OnDisable();

            if (terminal.UplinkPuzzle != null)
            {
                terminal.UplinkPuzzle.OnCleanup();
                terminal.UplinkPuzzle.OnPuzzleSolved = null;
                terminal.UplinkPuzzle.m_terminal = null;
                terminal.UplinkPuzzle = null;
            }

            UnityEngine.Object.Destroy(terminal.gameObject);
        }

        LG_ComputerTerminalCommandInterpreter.OnLevelCleanup();

        // --- Warden Objective ---
        WardenObjectiveManager.Current.OnLevelCleanup();
        WardenObjectiveManager.Current.m_layerObjectiveGameObjects[LG_LayerType.MainLayer].Clear();
        WardenObjectiveManager.Current.m_layerObjectiveGameObjects[LG_LayerType.SecondaryLayer].Clear();
        WardenObjectiveManager.Current.m_layerObjectiveGameObjects[LG_LayerType.ThirdLayer].Clear();

        Builder.Current.OnLevelCleanup();

        foreach (var batch in LG_Factory.Current.m_batches)
            batch.Jobs.Clear();

        LG_Factory.Current.m_currentBatchName = LG_Factory.BatchName.Setup;
        LG_Factory.Current.m_currentBatch.Jobs.Clear();
        LG_Factory.Current.m_batchTimes.Clear();
    }

    #endregion

    #region Event handlers


    public static readonly HashSet<int> s_building = new();
    public static bool s_suppressHook = false; // prevent recursion while we recompute
    // private const int MaxAttempts = 8; // your choice

    public const int MaxAttemptsPerZone = 128;

    // public static bool initialized = false;
    // public static bool rebuildInProgress = false;
    // public static bool shouldSuppressFactoryDone = false;

    public static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), int> zoneAttempts = new();

    public static readonly HashSet<(eDimensionIndex dim, eLocalZoneIndex lz)> s_targetsDetected = new();

    public static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), uint> markerSubSeeds = new();



    public static void OnFactoryDone()
    {
        try
        {
            if (Rebuilding)
                return; // guard

            // Find first unhealthy detected target
            foreach (var key in s_targetsDetected)
            {
                var zone = FindZone(key);

                if (zone == null)
                    continue;

                if (IsZoneHealthy(zone))
                    continue;

                // Bump and rebuild
                var current = markerSubSeeds.TryGetValue(key, out var v) ? v : zone.m_markerSubSeed;
                var next = current + 1;
                markerSubSeeds[key] = next;
                zoneAttempts[key] = (zoneAttempts.TryGetValue(key, out var a) ? a : 0) + 1;

                if (zoneAttempts[key] > MaxAttemptsPerZone)
                {
                    Plugin.Logger.LogError($"[Reroll] Max attempts reached for {key}. Last m_markerSubSeed={current}");
                    // Give up on this key but keep others (remove from detected set)
                    // Optionally: keep it to retry later
                    s_targetsDetected.Remove(key);
                    break;
                }

                Plugin.Logger.LogDebug($"[Reroll] Rebuilding {key} with m_markerSubSeed={next} (attempt {zoneAttempts[key]})");

                Rebuilding = true;

                LevelCleanup();
                Rebuild();

                return; // one rebuild per completion
            }

            // All detected targets healthy? Clean up
            // (re-check and prune)
            var toRemove = new List<(eDimensionIndex, eLocalZoneIndex)>();

            foreach (var key in s_targetsDetected)
            {
                var zone = FindZone(key);

                if (zone != null && IsZoneHealthy(zone))
                {
                    Plugin.Logger.LogDebug($"[Reroll] {key} healthy. Attempts={zoneAttempts.GetValueOrDefault(key, 0)}, m_markerSubSeed={zone.m_markerSubSeed}");
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
                s_targetsDetected.Remove(key);

            // Release suppression when done
            if (s_targetsDetected.Count == 0)
            {
                Plugin.Logger.LogDebug("[Reroll] All detected zones healthy. Releasing factory done suppression.");
                ShouldRebuild = false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Encountered a bad error: {ex}");
            ShouldRebuild = false;
        }
        finally
        {
            Rebuilding = false;
        }
    }

    private static bool IsZoneHealthy(LG_Zone zone)
    {
        foreach (var cn in zone.m_courseNodes)
        {
            var cl = cn?.m_nodeCluster;

            if (cl == null || cl.m_reachableNodes == null || cl.m_reachableNodes.Count <= 1)
                return false;
        }
        return true;
    }

    private static LG_Zone? FindZone((eDimensionIndex dim, eLocalZoneIndex lz) key)
    {
        var floor = Builder.CurrentFloor;

        if (floor == null)
            return null;

        Dimension dim;

        if (!floor.GetDimension(key.dim, out dim) || dim == null)
            return null;

        foreach (var layer in dim.Layers)
        {
            foreach (var zone in layer.m_zones)
                if (zone.LocalIndex == key.lz)
                    return zone;
            //
            // if (layer?.m_zonesByLocalIndex != null && layer.m_zonesByLocalIndex.TryGetValue(key.lz, out var zone))
            //     return zone;
        }

        return null;
    }


    #endregion
}
