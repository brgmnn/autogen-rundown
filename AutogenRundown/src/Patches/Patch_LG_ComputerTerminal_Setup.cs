using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_LG_ComputerTerminal_Setup
{
    const string terminalPrefab = "assets/assetprefabs/complex/generic/functionmarkers/terminal_floor.prefab";

    private static List<LevelTerminalPlacements> _levelPlacements = new();

    // private
    public static void Setup()
    {
        _levelPlacements = LevelTerminalPlacements.LoadAll();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.Setup))]
    private static void Post_LG_ComputerTerminal_Setup(LG_ComputerTerminal __instance)
    {
        var mainLayoutId = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;
        var terminalPlacements = _levelPlacements.Find(ltp => ltp.MainLevelLayout == mainLayoutId);

        if (terminalPlacements is null)
            return;

        if (!terminalPlacements.Placements.Any())
            return;

        var zone = __instance.SpawnNode.m_zone;
        var areas = zone.m_areas;
        var layer = zone.m_layer;

        var positions = terminalPlacements.Placements.Where(terminal =>
            $"Zone_{terminal.LocalIndex}" == zone.LocalIndex.ToString() &&
            terminal.Layer == layer.m_type.ToString()).ToList();

        if (!positions.Any())
            return;

        foreach (var area in areas)
        {
            // Match area based on Geomorph
            var placement = positions.Find(terminal =>
                terminal.HasGeomorphName(area.m_geomorph.name));

            if (placement is null)
                continue;

            var localPosition = new Vector3
            {
                x = (float)placement.Position.X,
                y = (float)placement.Position.Y,
                z = (float)placement.Position.Z
            };
            var position = area.transform.position + area.transform.rotation * localPosition;
            var rotation = Quaternion.Euler(
                (float)(area.transform.rotation.eulerAngles.x + placement.Rotation.X),
                (float)(area.transform.rotation.eulerAngles.y + placement.Rotation.Y),
                (float)(area.transform.rotation.eulerAngles.z + placement.Rotation.Z));

            __instance.transform.position = position;
            __instance.m_position = position;
            __instance.transform.rotation = rotation;

            Plugin.Logger.LogInfo($"Repositioned Terminal {__instance.name} {{ Position = {position} }}");

            // [HarmonyPatch(typeof(LG_WardenObjective_Reactor), nameof(LG_WardenObjective_Reactor.GenericObjectiveSetup))]
            // static class Inject_LG_Reactor
            // {
            //     const string TERMINAL_PREFAB = "ASSETS/ASSETPREFABS/COMPLEX/GENERIC/FUNCTIONMARKERS/TERMINAL_FLOOR.PREFAB";
            //
            //     static void Prefix(LG_WardenObjective_Reactor __instance)
            //     {
            //         if (__instance.m_terminalPrefab == null)
            //         {
            //             var ter = AssetAPI.GetLoadedAsset<GameObject>(TERMINAL_PREFAB);
            //             __instance.m_terminalPrefab = ter;
            //             FlowGeosLogger.Info("Terminal Prefab Resolved!");
            //         }
            //     }
            // }

            // var prefab = AssetAPI.
            // var ter = AssetAPI.GetLoadedAsset<GameObject>(TERMINAL_PREFAB);
            // __instance.m_terminalPrefab = ter;
            // FlowGeosLogger.Info("Terminal Prefab Resolved!");

            // var terminal = GOUtil.SpawnChildAndGetComp<LG_ComputerTerminal>(
            //     ter,
            //     new Transform
            //     {
            //         position = position,
            //         rotation = rotation,
            //     });
            // terminal.Setup();
        }
    }
}
