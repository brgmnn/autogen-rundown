using AIGraph;
using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

// using ExtraObjectiveSetup.Instances;
// using ExtraObjectiveSetup.Tweaks.TerminalPosition;
// using ExtraObjectiveSetup.Utils;

// namespace ExtraObjectiveSetup.Patches.Terminal;
namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_LG_ComputerTerminal_Setup
{
    private static List<LevelTerminalPlacements> _levelPlacements = new();

    // private
    public static void Setup()
    {
        _levelPlacements = LevelTerminalPlacements.LoadAll();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.Setup))]
    [HarmonyAfter("Inas.ExtraObjectiveSetup")]
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

        var i = 0;
        foreach (var area in areas)
        {
            var gridPos = area.m_geomorph.m_tile.m_shape.m_gridPosition;

            Plugin.Logger.LogWarning($"    Area {{ Index = {i}, " +
                                     $"Position = {area.transform.position}, " +
                                     $"Rotation = {area.transform.rotation.eulerAngles}, " +
                                     $"Geo = {area.m_geomorph.name}, " +
                                     $"Grid Position = {{ x = {gridPos.x}, z = {gridPos.z} }} }}");

            // Match area based on Geomorph
            var placement = positions.Find(terminal =>
                terminal.HasGeomorphName(area.m_geomorph.name));

            if (placement is not null)
            {
                // var terminal = zone.TerminalsSpawnedInZone[0];
                Plugin.Logger.LogWarning($"        Found Placement! {{ Position = {placement.Position}, for terminal = {__instance.transform.position} }}. MOVED!");

                // re-register this terminal to the correct SpawnNode.
                // s_allNodes

                // var newNode = AIG_CourseNode.s_allNodes.Find(new Func<AIG_CourseNode, bool>(node =>
                //     node.m_dimension == layer.m_dimension &&
                //     node.m_zone == zone &&
                //     node.m_area == area));

                // if (newNode != __instance.SpawnNode)
                    // newNode.RegisterComputerTerminal(__instance);

                var localPosition = new Vector3
                {
                    x = (float)placement.Position.X,
                    y = (float)placement.Position.Y,
                    z = (float)placement.Position.Z
                };
                var position = area.transform.position + area.transform.rotation * localPosition;

                __instance.transform.position = position;
                __instance.m_position = position;

                __instance.transform.rotation = Quaternion.Euler(
                    (float)(area.transform.rotation.eulerAngles.x + placement.Rotation.X),
                    (float)(area.transform.rotation.eulerAngles.y + placement.Rotation.Y),
                    (float)(area.transform.rotation.eulerAngles.z + placement.Rotation.Z));
            }

            i++;
        }
    }
}


// LG_Layer.CreateZone
//  LevelGeneration.Builder
//
// [HarmonyPatch]
// internal static class Patch_LG_Layer_CreateZone
// {
//     private static List<LevelTerminalPlacements> _levelPlacements = new();
//
//     // private
//     public static void Setup()
//     {
//         _levelPlacements = LevelTerminalPlacements.LoadAll();
//     }
//
//     // public uint CurrentMainLevelLayout => RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;
//
//     [HarmonyPostfix]
//     [HarmonyPatch(typeof(Builder), nameof(Builder.BuildDone))]
//     [HarmonyAfter("Inas.ExtraObjectiveSetup")]
//     private static void Post_Builder_BuildDone(Builder __instance)
//     {
//         return;
//
//         var mainLayoutId = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;
//         var terminalPlacements = _levelPlacements.Find(ltp => ltp.MainLevelLayout == mainLayoutId);
//
//         if (terminalPlacements is null)
//             return;
//
//         var layers = __instance.m_currentFloor.MainDimension.Layers;
//
//         foreach (var layer in layers)
//         {
//             var zones = layer.m_zones;
//
//             foreach (var zone in zones)
//             {
//                 var terminals = zone.TerminalsSpawnedInZone;
//
//                 var areas = zone.m_areas;
//
//                 // Works
//                 // Plugin.Logger.LogWarning($"Post Builder.BuildDone(): Zone {{ Layer = {layer.m_type} LocalIndex = {zone.LocalIndex}}}, Position = {zone.CenterPosition}");
//
//                 Plugin.Logger.LogWarning($"Post Builder.BuildDone(): Zone {{ Layer = {layer.m_type} LocalIndex = {zone.LocalIndex}, ID = {zone.ID}, IDinLayer = {zone.IDinLayer}}}");
//
//                 var i = 0;
//                 foreach (var area in areas)
//                 {
//                     var placement = terminalPlacements.Placements.Find(info =>
//                         info.Layer == layer.m_type.ToString() &&
//                         $"Zone_{info.LocalIndex}" == zone.LocalIndex.ToString() &&
//                         info.Area == area.m_navInfo.Suffix);
//
//                     Plugin.Logger.LogWarning($"    Area {{ Index = {i}, Position = {area.transform.position} }}");
//
//                     if (placement is not null)
//                     {
//                         if (zone.TerminalsSpawnedInZone.Count > 0)
//                         {
//                             var terminal = zone.TerminalsSpawnedInZone[0];
//
//                             Plugin.Logger.LogWarning($"        Found Placement! {{ Position = {placement.Position}, for terminal = {terminal.transform.position} }}. MOVED!");
//
//                             terminal.transform.position = new Vector3
//                             {
//                                 x = (float)(area.transform.position.x + placement.Position.X),
//                                 y = (float)(area.transform.position.y + placement.Position.Y),
//                                 z = (float)(area.transform.position.z + placement.Position.Z)
//                             };
//                         }
//                     }
//                     i++;
//                 }
//             }
//         }
//     }
// }
