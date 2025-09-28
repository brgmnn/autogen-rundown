using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class LogArchivist
{
    [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.ReadLog))]
    [HarmonyPostfix]
    internal static void Post_Terminal_ReadLog(
        LG_ComputerTerminalCommandInterpreter __instance,
        string param1 = "",
        string param2 = "")
    {
        var terminalLogs = __instance.m_terminal.GetLocalLogs();

        Plugin.Logger.LogInfo($"Trying to read log: {param1}");

        if (terminalLogs.ContainsKey(param1.ToUpper()))
        {
            var logData = terminalLogs[param1.ToUpper()];

            Plugin.Logger.LogInfo($"Player has read a log: name = {logData.FileName}");

            var data = RundownManager.GetActiveExpeditionData();
            var rundown = data.rundownKey.data;

            LogArchivistManager.RecordRead(rundown, RundownManager.ActiveExpedition.LevelLayoutData, logData.FileName);
        }
    }
}
