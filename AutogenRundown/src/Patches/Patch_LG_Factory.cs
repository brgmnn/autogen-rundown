using AutogenRundown.Managers;
using AutogenRundown.Patches.CustomTerminals;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_LG_Factory
{
    /// <summary>
    /// Calls the manual factory done method
    /// </summary>
    /// <returns></returns>
    [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.FactoryDone))]
    [HarmonyPrefix]
    public static bool Prefix_FactoryDone()
    {
        if (!FactoryJobManager.ShouldRebuild)
            return true;

        FactoryJobManager.OnFactoryDone();

        // If validation passed (ShouldRebuild is now false), let original run
        // to properly finish the factory and fire events. Otherwise, a rebuild
        // was triggered and we skip the original.
        return !FactoryJobManager.ShouldRebuild;
    }

    /// <summary>
    /// Freezes the factory once the rebuild budget is exhausted, and drives the abort.
    ///
    /// This is necessary because LG_Factory.GetNewJob() sets m_currentJob = null and then calls
    /// FactoryDone(). With no rebuild queued, Update() would dereference the null job every
    /// frame. Skipping Update entirely stops that cleanly.
    ///
    /// It doubles as the deferral point for the abort: this runs at the top of Update, one frame
    /// after FactoryDone, so the session command is issued from outside the factory job loop.
    /// </summary>
    [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.Update))]
    [HarmonyPrefix]
    public static bool Prefix_Update()
    {
        if (!FactoryJobManager.GaveUp)
            return true;

        BuildFailureManager.TickAbort();

        return false;
    }

    /// <summary>
    /// Resets rebuild tracking state when a fresh build starts (not a rebuild).
    /// </summary>
    [HarmonyPatch(typeof(Builder), nameof(Builder.Build))]
    [HarmonyPrefix]
    public static void Prefix_Build()
    {
        if (!FactoryJobManager.Rebuilding)
        {
            FactoryJobManager.NewBuild();
        }

        // Always release the fatal-threshold gate at the start of any build. NewBuild
        // already covers fresh builds; this catches the rebuild path too so the new
        // attempt's cascade can run normally.
        Fix_FailedToFindStartArea.fatalReached = false;
        Fix_DistributionOnBrokenZones.ResetDiagnostics();
        Fix_FactoryJobExceptionCatchAll.ResetDiagnostics();

        // Must clear on rebuilds too: stale warden objective zone claims from a
        // previous pass of the same level would block the custom terminal's own
        // warden objective setup.
        Patch_SpawnCustomTerminals.ResetBuildState();
    }

    /// <summary>
    /// Stops the default factory done listeners from running if a re-roll is requested
    /// </summary>
    /// <returns></returns>
    [HarmonyPatch(typeof(Builder), nameof(Builder.OnFactoryDone))]
    [HarmonyPatch(typeof(EnvironmentStateManager), nameof(EnvironmentStateManager.OnFactoryBuildDone))]
    [HarmonyPrefix]
    public static bool SupressEventHandlers()
    {
        if (FactoryJobManager.ShouldRebuild)
        {
            Plugin.Logger.LogInfo("[FactoryJobManager] Suppressing event handler");
            return false;
        }

        return true;
    }
}
