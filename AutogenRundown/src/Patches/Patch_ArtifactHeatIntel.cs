using AutogenRundown.Managers;
using HarmonyLib;

namespace AutogenRundown.Patches;

/// <summary>
/// Replaces the "Artifact heat at..." warden intel message shown during the elevator ride
/// with the current log read count for the level.
/// Matches based on TextDataBlock ID 802 to work with any localization.
/// </summary>
[HarmonyPatch]
internal static class Patch_ArtifactHeatIntel
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerGuiLayer), nameof(PlayerGuiLayer.ShowWardenIntel))]
    internal static bool Pre_ShowWardenIntel(ref string intel)
    {
        if (string.IsNullOrEmpty(intel))
            return true;

        // TextDataBlock 802: ">Artifact Heat at {0}, potential booster results:{1}"
        var template = Localization.Text.Get(802U);
        var placeholderIndex = template.IndexOf("{0}");

        if (placeholderIndex >= 0 && intel.Contains(template.Substring(0, placeholderIndex)))
        {
            var mainId = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;

            if (mainId == 0)
                return true;

            var logsRead = LogArchivistManager.PrintLogsRead(mainId);

            if (logsRead == null)
                return false;

            intel = $"<size=200%><color=white>>Logs: {logsRead}</color></size>";
            return true;
        }

        return true;
    }
}
