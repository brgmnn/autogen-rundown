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

        // Works
        // Plugin.Logger.LogWarning($"Post Builder.BuildDone(): Zone {{ Layer = {layer.m_type} LocalIndex = {zone.LocalIndex}}}, Position = {zone.CenterPosition}");

        // Plugin.Logger.LogWarning($"Post Builder.BuildDone(): Zone {{ Layer = {layer.m_type} LocalIndex = {zone.LocalIndex}}}");

        Plugin.Logger.LogWarning($"We got post Inas.ExtraObjectiveSetup terminal setup! Zone {{ Layer = {zone.Layer} LocalIndex = {zone.LocalIndex} }}");

        var i = 0;
        foreach (var area in areas)
        {
            Plugin.Logger.LogWarning($"    Area {{ Index = {i}, Position = {area.transform.position}, Rotation = {area.transform.rotation.eulerAngles} }}");

            var placement = terminalPlacements.Placements.Find(info =>
                info.Layer == layer.m_type.ToString() &&
                $"Zone_{info.LocalIndex}" == zone.LocalIndex.ToString() &&
                info.Area == area.m_navInfo.Suffix);

            if (placement is not null)
            {
                // var terminal = zone.TerminalsSpawnedInZone[0];
                Plugin.Logger.LogWarning($"        Found Placement! {{ Position = {placement.Position}, for terminal = {__instance.transform.position} }}. MOVED!");

                __instance.transform.position = new Vector3
                {
                    x = (float)(area.transform.position.x + placement.Position.X),
                    y = (float)(area.transform.position.y + placement.Position.Y),
                    z = (float)(area.transform.position.z + placement.Position.Z)
                };
                __instance.transform.rotation = Quaternion.Euler(
                    (float)(area.transform.rotation.eulerAngles.x + placement.Rotation.X),
                    (float)(area.transform.rotation.eulerAngles.y + placement.Rotation.Y),
                    (float)(area.transform.rotation.eulerAngles.z + placement.Rotation.Z));
            }

            i++;
        }

        // uint instanceIndex = TerminalInstanceManager.Current.Register(__instance);
        // TerminalInstanceManager.Current.SetupTerminalWrapper(__instance);
        //
        // // modify terminal position
        // if (__instance.SpawnNode == null) return; // disallow changing position of reactor terminal
        //
        // var globalZoneIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(__instance);
        // var _override = TerminalPositionOverrideManager.Current.GetDefinition(globalZoneIndex, instanceIndex);
        //
        // if (_override == null) return;
        //
        // if (_override.Position.ToVector3() != UnityEngine.Vector3.zeroVector)
        // {
        //     __instance.transform.position = _override.Position.ToVector3();
        //     __instance.transform.rotation = _override.Rotation.ToQuaternion();
        // }
        //
        // EOSLogger.Debug($"TerminalPositionOverride: {_override.LocalIndex}, {_override.LayerType}, {_override.DimensionIndex}, TerminalIndex {_override.InstanceIndex}");
    }
}


// LG_Layer.CreateZone
//  LevelGeneration.Builder

[HarmonyPatch]
internal static class Patch_LG_Layer_CreateZone
{
    private static List<LevelTerminalPlacements> _levelPlacements = new();

    // private
    public static void Setup()
    {
        _levelPlacements = LevelTerminalPlacements.LoadAll();
    }

    // public uint CurrentMainLevelLayout => RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Builder), nameof(Builder.BuildDone))]
    [HarmonyAfter("Inas.ExtraObjectiveSetup")]
    private static void Post_Builder_BuildDone(Builder __instance)
    {
        return;

        var mainLayoutId = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;
        var terminalPlacements = _levelPlacements.Find(ltp => ltp.MainLevelLayout == mainLayoutId);

        if (terminalPlacements is null)
            return;

        var layers = __instance.m_currentFloor.MainDimension.Layers;

        foreach (var layer in layers)
        {
            var zones = layer.m_zones;

            foreach (var zone in zones)
            {
                var terminals = zone.TerminalsSpawnedInZone;

                var areas = zone.m_areas;

                // Works
                // Plugin.Logger.LogWarning($"Post Builder.BuildDone(): Zone {{ Layer = {layer.m_type} LocalIndex = {zone.LocalIndex}}}, Position = {zone.CenterPosition}");

                Plugin.Logger.LogWarning($"Post Builder.BuildDone(): Zone {{ Layer = {layer.m_type} LocalIndex = {zone.LocalIndex}, ID = {zone.ID}, IDinLayer = {zone.IDinLayer}}}");

                var i = 0;
                foreach (var area in areas)
                {
                    var placement = terminalPlacements.Placements.Find(info =>
                        info.Layer == layer.m_type.ToString() &&
                        $"Zone_{info.LocalIndex}" == zone.LocalIndex.ToString() &&
                        info.Area == area.m_navInfo.Suffix);

                    Plugin.Logger.LogWarning($"    Area {{ Index = {i}, Position = {area.transform.position} }}");

                    if (placement is not null)
                    {
                        if (zone.TerminalsSpawnedInZone.Count > 0)
                        {
                            var terminal = zone.TerminalsSpawnedInZone[0];

                            Plugin.Logger.LogWarning($"        Found Placement! {{ Position = {placement.Position}, for terminal = {terminal.transform.position} }}. MOVED!");

                            terminal.transform.position = new Vector3
                            {
                                x = (float)(area.transform.position.x + placement.Position.X),
                                y = (float)(area.transform.position.y + placement.Position.Y),
                                z = (float)(area.transform.position.z + placement.Position.Z)
                            };
                        }
                    }
                    i++;
                }
            }
        }
    }
}
