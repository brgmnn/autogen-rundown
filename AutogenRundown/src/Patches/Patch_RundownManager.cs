using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_RundownManager
{
    public static PluginRundown CurrentRundown { get; private set; } = PluginRundown.None;
    public static pActiveExpedition CurrentExpedition { get; private set; }

    /// <summary>
    /// Updates the watermark to show the seed for the active expedition
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="expPackage"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.SetActiveExpedition))]
    private static void Post_Setup(RundownManager __instance, pActiveExpedition expPackage)
    {
        CurrentExpedition = expPackage;
        CurrentRundown = expPackage.rundownKey.data switch
        {
            "Local_1" => PluginRundown.Daily,
            "Local_2" => PluginRundown.Weekly,
            "Local_3" => PluginRundown.Monthly,
            "Local_4" => PluginRundown.Seasonal,
            _ => PluginRundown.None
        };

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

            case "Local_4":
                Managers.WatermarkManager.SetRundown(PluginRundown.Seasonal, expPackage);
                break;

            default:
                Managers.WatermarkManager.ClearRundown();
                break;
        }
    }
}
