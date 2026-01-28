using AutogenRundown.Managers;
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
