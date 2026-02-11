using System.Text.RegularExpressions;
using HarmonyLib;
using SNetwork;

namespace AutogenRundown.Patches;

[HarmonyPatch(typeof(SNet_Core_STEAM), nameof(SNet_Core_STEAM.SetFriendsData), typeof(FriendsDataType), typeof(string))]
public class SteamPresence
{
    [HarmonyPrefix]
    [HarmonyAfter("com.dak.MTFO")]
    public static void Prefix(FriendsDataType type, ref string data)
    {
        if (type != FriendsDataType.ExpeditionName || data == null)
            return;

        // Remove modded
        data = data.Replace("MODDED - ", "");

        // Replace rundown names
        data = data.Replace("D</color>", "Daily ");
        data = data.Replace("W</color>", "Weekly ");
        data = data.Replace("M</color>", "Monthly ");
        data = data.Replace("S</color>", "Seasonal ");

        // Strip tags
        data = Regex.Replace(data, "<.*?>", "");
    }
}
