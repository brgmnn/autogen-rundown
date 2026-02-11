using System.Text.RegularExpressions;
using HarmonyLib;
using SNetwork;

namespace AutogenRundown.Patches;

[HarmonyPatch(typeof(SNet_Core_STEAM), nameof(SNet_Core_STEAM.SetFriendsData), typeof(FriendsDataType), typeof(string))]
public class Patch_SteamPresence
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyAfter("com.dak.MTFO")]
    // [HarmonyBefore("com.dak.MTFO")]
    public static void Prefix(FriendsDataType type, ref string data)
    {
        if (type != FriendsDataType.ExpeditionName || data == null)
        {
            Plugin.Logger.LogDebug($"Wrong call");
            return;
        }

        var rundown = Patch_RundownManager.CurrentRundown;
        var expedition = Patch_RundownManager.CurrentExpedition;

        // if (expedition?.rundownKey == null)
        // {
        //     Plugin.Logger.LogDebug($"No expedition found");
        //     return;
        // }

        // if (rundown == PluginRundown.None)
        // {
        //     Plugin.Logger.LogDebug($"Rundown appears to be none: {rundown} {expedition}");
        //     return;
        // }

        var tier = expedition.tier switch
        {
            eRundownTier.TierA => "A",
            eRundownTier.TierB => "B",
            eRundownTier.TierC => "C",
            eRundownTier.TierD => "D",
            eRundownTier.TierE => "E",
            _ => ""
        };

        var level = $"{tier}{expedition.expeditionIndex + 1}";

        // var rundownName = rundown switch
        // {
        //     PluginRundown.Daily => "Daily",
        //     PluginRundown.Weekly => "Weekly",
        //     PluginRundown.Monthly => "Monthly",
        //     PluginRundown.Seasonal => "Seasonal",
        //     _ => ""
        // };
        // var rundownName = expedition.rundownKey.data switch
        // {
        //     "Local_1" => "Daily",
        //     "Local_2" => "Weekly",
        //     "Local_3" => "Monthly",
        //     "Local_4" => "Seasonal",
        //     _ => ""
        // };

        // Extract level name from ActiveExpeditionHeader, stripping rich text tags
        var header = RundownManager.ActiveExpeditionHeader ?? "";
        var colonIndex = header.IndexOf(':');
        var name = colonIndex >= 0 ? header.Substring(colonIndex + 1) : header;
        name = Regex.Replace(name, "<.*?>", "");

        // data = $"{rundownName} {level} - {name}";
        data = $"{level} - {name}";

        Plugin.Logger.LogDebug($"Just checking did we make it here: {data}");
    }
}
