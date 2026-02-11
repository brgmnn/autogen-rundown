using System.Text.RegularExpressions;
using HarmonyLib;
using SNetwork;

namespace AutogenRundown.Patches;

[HarmonyPatch(typeof(SNet_Core_STEAM), nameof(SNet_Core_STEAM.SetFriendsData), typeof(FriendsDataType), typeof(string))]
public class Patch_SteamPresence
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    public static void Prefix(FriendsDataType type, ref string data)
    {
        if (type != FriendsDataType.ExpeditionName || data == null)
            return;

        var rundown = Patch_RundownManager.CurrentRundown;
        var expedition = Patch_RundownManager.CurrentExpedition;

        if (rundown == PluginRundown.None)
            return;

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

        var rundownName = rundown switch
        {
            PluginRundown.Daily => "Daily",
            PluginRundown.Weekly => "Weekly",
            PluginRundown.Monthly => "Monthly",
            PluginRundown.Seasonal => "Seasonal",
            _ => ""
        };

        // Extract level name from ActiveExpeditionHeader, stripping rich text tags
        var header = RundownManager.ActiveExpeditionHeader ?? "";
        var colonIndex = header.IndexOf(':');
        var name = colonIndex >= 0 ? header.Substring(colonIndex + 1) : header;
        name = Regex.Replace(name, "<.*?>", "");

        data = $"{rundownName} {level}: {name}";
    }
}
