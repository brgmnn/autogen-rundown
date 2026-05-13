using AutogenRundown.Managers;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

/// <summary>
/// When the broken-zone cascade reaches the Distribution batch, LG_Distribute_WardenObjective.Build
/// throws an ArgumentOutOfRangeException somewhere inside the chain-loop / placement code (the
/// running game version ships with chainIndex-based code we can't statically read — the cpp2il
/// decompile has empty method bodies). Without a guard the same job re-throws every frame and
/// the engine never reaches FactoryDone, blocking the pending rebuild.
///
/// Finalizer catches the exception, logs it once per (layer, dim, exception-type) with a full
/// stack trace, and short-circuits the job (__result = true, __exception = null) so the engine
/// drains to FactoryDone and the rebuild triggers via Patch_LG_Factory.Prefix_FactoryDone.
///
/// The logged stack trace is also the next diagnostic step — it points us at the exact AOOR
/// site so a targeted prefix can replace this finalizer later if desired.
/// </summary>
[HarmonyPatch]
public class Fix_WardenObjectiveDistributionOnBrokenZones
{
    private static readonly HashSet<int> s_loggedExceptions = new();

    public static void ResetDiagnostics() => s_loggedExceptions.Clear();

    [HarmonyPatch(typeof(LG_Distribute_WardenObjective), nameof(LG_Distribute_WardenObjective.Build))]
    [HarmonyFinalizer]
    public static void Post_Build(LG_Distribute_WardenObjective __instance, ref bool __result, ref Exception? __exception)
    {
        if (__exception == null)
            return;

        var layer = __instance.m_layer;
        var layerType = layer?.m_type.ToString() ?? "<null>";
        var dim = layer?.m_dimension?.DimensionIndex.ToString() ?? "<null>";
        var key = layerType.GetHashCode() ^ dim.GetHashCode() ^ __exception.GetType().GetHashCode();

        if (s_loggedExceptions.Add(key))
        {
            Plugin.Logger.LogError(
                $"LG_Distribute_WardenObjective.Build threw on layer={layerType} dim={dim}: " +
                $"{__exception.GetType().Name}: {__exception.Message}\n{__exception.StackTrace}\n" +
                $"ShouldRebuild={FactoryJobManager.ShouldRebuild} — short-circuiting and letting the engine drain.");
        }

        __exception = null;
        __result = true;
    }
}
