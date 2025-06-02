using HarmonyLib;

namespace AutogenRundown.Patches;

/// <summary>
/// Patches for artifacts and boosters
/// </summary>
[HarmonyPatch]
internal static class Artifacts
{
    /// <summary>
    /// Fixes boosters to not be consumed upon use in levels
    /// </summary>
    /// <returns></returns>
    [HarmonyPatch(typeof(DropServerGameSession), nameof(DropServerGameSession.ConsumeBoosters))]
    [HarmonyPrefix]
    internal static bool Pre_DropServerGameSession_ConsumeBoosters()
    {
        Plugin.Logger.LogInfo($"Skipping booster consumption");
        return false;
    }
}
