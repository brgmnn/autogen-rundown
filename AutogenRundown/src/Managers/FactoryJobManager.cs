using System;
using AIGraph;
using AutogenRundown.Patches;
using CellMenu;
using Enemies;
using AmorLib.API;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Managers;

/// <summary>
/// Handles managing rebuilds for levels
/// </summary>
public static class FactoryJobManager
{
    /// <summary>
    /// Register new validations here to be called when LG_Factory.FactoryDone is called. These
    /// methods will validate the state of the level with whatever code they want and can return
    /// true/false. True is a passing value and will allow the game to continue, false will trigger
    /// a level rebuild.
    /// </summary>
    public static Func<bool> OnDoneValidate { get; set; } = () => true;

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

    /// <summary>
    /// Resets all rebuild tracking state for a fresh build cycle.
    /// Called at the start of Builder.Build() when it's not a rebuild.
    /// </summary>
    public static void NewBuild()
    {
        ShowMessage = false;
        ShouldRebuild = false;
        RebuildCount = 0;

        ZoneSeedManager.SubSeeds.Clear();
        Fix_NavMeshMarkerSubSeed.TargetsDetected.Clear();
        Fix_NavMeshMarkerSubSeed.MarkerSubSeeds.Clear();
        Fix_NavMeshMarkerSubSeed.ZoneAttempts.Clear();
        Fix_FailedToFindStartArea.zoneFailures.Clear();
    }

    #region Build / Cleanup

    /// <summary>
    ///
    /// </summary>
    private static void Rebuild()
    {
        RebuildCount++;

        var activeExpedition = RundownManager.GetActiveExpeditionData();
        var expeditionData = RundownManager.ActiveExpedition;

        FlashMessage();

        Builder.Current.Build();

        WardenObjectiveManager.Current.OnExpeditionUpdated(activeExpedition, expeditionData);
    }

    /// <summary>
    /// Performs a level cleanup so a rebuild can occur. This mostly follows what the base game
    /// does for level cleanup.
    /// </summary>
    private static void LevelCleanup()
    {
        // Clear AmorLib's LightWorkers directly to prevent accumulation across
        // rebuilds (NullRef errors and light toggle failures). We avoid firing
        // the broad LevelAPI.OnLevelCleanup event because it has side effects
        // in the SNet session layer that cause Kick_GenerationChecksum for
        // late joiners.
        var lightsMapField = AccessTools.Field(typeof(LightAPI), "AllLightsMap");
        (lightsMapField?.GetValue(null) as System.Collections.IDictionary)?.Clear();

        // --- Enemies ---
        // Clear enemy spawn manager cache
        EnemySpawnManager.m_groupRandomizers.Clear();

        // Clear Mastermind's factory group spawn data to prevent duplicate enemy spawns.
        // LG_PopulateArea adds spawn data to m_factoryGroupDatas during build, which must be
        // cleared before rebuild or enemies will spawn twice.
        Mastermind.Current.OnLevelCleanup();

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

    /// <summary>
    /// Checks all validation methods and catches FactoryDone and optionally triggers a rebuild
    /// </summary>
    public static void OnFactoryDone()
    {
        if (Rebuilding)
            return;

        var delegates = OnDoneValidate.GetInvocationList();
        var results = new List<bool>(delegates.Length);

        foreach (var @delegate in delegates)
        {
            var check = (Func<bool>)@delegate;
            var result = check();

            results.Add(result);

            // Fast fail, mark the manager as attempting a rebuild
            if (!result)
                ShouldRebuild = true;
        }

        if (results.Any(r => !r))
        {
            Rebuilding = true;

            LevelCleanup();
            Rebuild();

            Rebuilding = false;
        }
        else
        {
            // Only re-enable the handlers when all validation checks pass
            ShouldRebuild = false;
            RebuildCount = 0;
            ShowMessage = false;

            // TODO: move this logic to the respective managers
            ZoneSeedManager.SubSeeds.Clear();
            Fix_NavMeshMarkerSubSeed.TargetsDetected.Clear();
            Fix_NavMeshMarkerSubSeed.MarkerSubSeeds.Clear();
            Fix_NavMeshMarkerSubSeed.ZoneAttempts.Clear();
            Fix_FailedToFindStartArea.zoneFailures.Clear();
        }
    }

    #endregion
}
