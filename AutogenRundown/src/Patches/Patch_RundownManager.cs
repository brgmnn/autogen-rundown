using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_RundownManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.SetActiveExpedition))]
    private static void Post_Setup(RundownManager __instance, pActiveExpedition expPackage)
    {
        Plugin.Logger.LogWarning($"Got set active expedition: {expPackage.rundownKey.data}");

        switch (expPackage.rundownKey.data)
        {
            case "Local_1":
                Managers.WatermarkManager.SetRundown(PluginRundown.Daily);
                break;

            case "Local_2":
                Managers.WatermarkManager.SetRundown(PluginRundown.Weekly);
                break;

            case "Local_3":
                Managers.WatermarkManager.SetRundown(PluginRundown.Monthly);
                break;
        }
    }
}
