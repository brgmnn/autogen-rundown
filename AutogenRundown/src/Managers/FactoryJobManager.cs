using AIGraph;
using CellMenu;
using Globals;
using LevelGeneration;

namespace AutogenRundown.Managers;

public class FactoryJobManager
{
    /// <summary>
    ///
    /// </summary>
    public static void Rebuild()
    {
        var activeExpedition = RundownManager.GetActiveExpeditionData();
        var expeditionData = RundownManager.ActiveExpedition;

        // WardenObjectiveManager.Current.Setup();

        Builder.Current.Build();

        WardenObjectiveManager.Current.OnExpeditionUpdated(activeExpedition, expeditionData);
    }

    /// <summary>
    /// Performs a level cleanup so a rebuild can occur. This mostly follows what the base game
    /// does for level cleanup.
    /// </summary>
    public static void LevelCleanup()
    {
        LG_BuildNodeCluster.LevelCleanup();
        LG_FunctionMarkerBuilder.LevelCleanup();
        LG_MarkerFactory.Cleanup();

        // LG_PrefabSpawner.OnLevelCleanup();

        // LG_LevelInteractionManager.Current.OnLevelCleanup();

        // Minimap
        MapDetails.OnLevelCleanup();
        MapDataManager.Current.OnLevelCleanup();
        CM_PageMap.Current.OnLevelCleanup();

        // // Terminals
        // LG_ComputerTerminalCommandInterpreter.OnLevelCleanup();

        // Warden objective
        WardenObjectiveManager.Current.OnLevelCleanup();

        // Remove terminals/uplinks from previous attempts
        foreach (var t in UnityEngine.Object.FindObjectsOfType<LG_ComputerTerminal>())
            UnityEngine.Object.Destroy(t.gameObject);


        // Builder.Current.OnLevelCleanup();

        // Global.OnLevelCleanup();


        // try {
        //     // foreach (var u in UnityEngine.Object.FindObjectsOfType<LevelGeneration.TerminalUplinkPuzzle>())
        //     //     UnityEngine.Object.Destroy(u.gameObject);
        // } catch {}
    }
}
