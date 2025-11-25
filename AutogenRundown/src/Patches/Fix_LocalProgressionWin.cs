using HarmonyLib;
using LocalProgression;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Fix_LocalProgressionWin
{
    /// <summary>
    /// Fixes an issue where LocalProgression doesn't record the main objective as cleared when
    /// WinOnDeath is set
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(GS_ExpeditionSuccess), nameof(GS_ExpeditionSuccess.Enter))]
    [HarmonyPostfix]
    public static void GS_ExpeditionSuccess_Enter(GS_ExpeditionSuccess __instance)
    {
        var CompletedExpedtionData = RundownManager.GetActiveExpeditionData();

        var mainLayerCleared = WardenObjectiveManager.CurrentState.main_status == eWardenObjectiveStatus.WardenObjectiveItemSolved;
        var winOnDeath = WardenObjectiveManager.GetWinOnDeath();

        var expeditionKey = LocalProgressionManager.Current.ExpeditionKey(
            CompletedExpedtionData.tier,
            CompletedExpedtionData.expeditionIndex);

        if (!mainLayerCleared && winOnDeath)
        {
            LocalProgressionManager.Current.RecordExpeditionSuccessForCurrentRundown(
                expeditionKey,
                true,
                false,
                false,
                false);
            Plugin.Logger.LogDebug($"Marking win in LocalProgression");
        }
    }
}
