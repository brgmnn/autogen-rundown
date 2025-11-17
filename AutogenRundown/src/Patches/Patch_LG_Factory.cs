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

        return false;
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
