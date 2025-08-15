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
        switch (expPackage.rundownKey.data)
        {
            case "Local_1":
                Managers.WatermarkManager.SetRundown(PluginRundown.Daily, expPackage);
                break;

            case "Local_2":
                Managers.WatermarkManager.SetRundown(PluginRundown.Weekly, expPackage);
                break;

            case "Local_3":
                Managers.WatermarkManager.SetRundown(PluginRundown.Monthly, expPackage);
                break;

            default:
                Managers.WatermarkManager.ClearRundown();
                break;
        }
    }
}
