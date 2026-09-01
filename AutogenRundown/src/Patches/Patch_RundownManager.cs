using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_RundownManager
{
    /// <summary>
    /// Updates the watermark to show the seed for the active expedition
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="expPackage"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.SetActiveExpedition))]
    private static void Post_Setup(RundownManager __instance, pActiveExpedition expPackage)
    {
        var rundown = PluginRundowns.FromRundownKey(expPackage.rundownKey.data);

        if (rundown == PluginRundown.None)
            Managers.WatermarkManager.ClearRundown();
        else
            Managers.WatermarkManager.SetRundown(rundown, expPackage);
    }
}
