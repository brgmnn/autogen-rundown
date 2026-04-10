using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Known issues:
///     * If the terminal is moved from an area that isn't the desired geomorph zone, it doesn't
///       render properly. I think it's being culled? Likely the solution would be to just spawn
///       our own terminal.
///
/// const string terminalPrefab = "assets/assetprefabs/complex/generic/functionmarkers/terminal_floor.prefab";
/// </summary>
[HarmonyPatch]
internal static class Patch_LG_ComputerTerminal_Setup
{
    private static List<LevelTerminalPlacements> _levelPlacements = new();

    private static readonly HashSet<int> PlacedAreas = new();

    // private
    public static void Setup()
    {
        _levelPlacements = LevelTerminalPlacements.LoadAll();

        LevelAPI.OnLevelCleanup += PlacedAreas.Clear;
        LevelAPI.OnBuildDone += PlacedAreas.Clear;
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

            var areaId = area.GetInstanceID();

            if (placement is null)
                continue;

            if (!PlacedAreas.Add(areaId))
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
        }
    }
}
