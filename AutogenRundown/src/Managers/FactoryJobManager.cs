using AIGraph;
using CellMenu;
using Globals;
using LevelGeneration;

namespace AutogenRundown.Managers;

public class FactoryJobManager
{
    public static bool ShowMessage { get; private set; }

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
        LG_Factory.Current.m_batchTimes.Clear();

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


        // Builder.Current.OnLevelCleanup();

        // Global.OnLevelCleanup();


        // try {
        //     // foreach (var u in UnityEngine.Object.FindObjectsOfType<LevelGeneration.TerminalUplinkPuzzle>())
        //     //     UnityEngine.Object.Destroy(u.gameObject);
        // } catch {}
    }
}
