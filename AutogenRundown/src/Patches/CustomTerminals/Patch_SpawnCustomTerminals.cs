using GameData;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches.CustomTerminals;

[HarmonyPatch]
internal static class Patch_SpawnCustomTerminals
{
    private const string TerminalPrefab =
        "Assets/AssetPrefabs/Complex/Generic/FunctionMarkers/Terminal_Floor.prefab";

    /// <summary>
    /// Postfix on LG_DistributionSetup.Build() which runs in the DistributionSetup batch (14).
    /// At this point all areas and marker spawners exist, but terminal distribution (batch 15)
    /// and function marker population (batch 17) haven't run yet. This is the ideal time to:
    ///   1. Clear nearby marker spawners to prevent collisions
    ///   2. Spawn custom terminals at exact positions using the generic prefab
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LG_DistributionSetup), nameof(LG_DistributionSetup.Build))]
    private static void Post_LG_DistributionSetup_Build(LG_DistributionSetup __instance)
    {
        var mainLayoutId = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;
        if (mainLayoutId == 0)
            return;

        var requests = CustomTerminalSpawnManager.GetRequests(mainLayoutId);
        if (requests.Count == 0)
            return;

        var layer = __instance.m_layer;

        foreach (var request in requests)
        {
            if (request.LayerType != layer.m_type)
                continue;

            SpawnTerminalForRequest(layer, request);
        }
    }

    private static void SpawnTerminalForRequest(LG_Layer layer, CustomTerminalSpawnRequest request)
    {
        // Find the zone by local index
        LG_Zone? targetZone = null;
        foreach (var zone in layer.m_zones)
        {
            if (zone.LocalIndex.ToString() == $"Zone_{request.LocalIndex}")
            {
                targetZone = zone;
                break;
            }
        }

        if (targetZone == null)
        {
            Plugin.Logger.LogWarning(
                $"[CustomTerminal] Could not find zone {request.LocalIndex} in layer {layer.m_type}");
            return;
        }

        // Find the area matching the geomorph
        LG_Area? targetArea = null;
        foreach (var area in targetZone.m_areas)
        {
            if (request.HasGeomorphName(area.m_geomorph.name))
            {
                targetArea = area;
                break;
            }
        }

        if (targetArea == null)
        {
            Plugin.Logger.LogWarning(
                $"[CustomTerminal] Could not find area with geomorph matching " +
                $"'{Path.GetFileName(request.GeomorphName)}' in zone {request.LocalIndex}");
            return;
        }

        // Calculate world position and rotation from area transform
        var localPos = new Vector3(
            (float)request.LocalPosition.X,
            (float)request.LocalPosition.Y,
            (float)request.LocalPosition.Z);
        var worldPos = targetArea.transform.TransformPoint(localPos);

        var areaEuler = targetArea.transform.rotation.eulerAngles;
        var worldRot = Quaternion.Euler(
            areaEuler.x + (float)request.LocalRotation.X,
            areaEuler.y + (float)request.LocalRotation.Y,
            areaEuler.z + (float)request.LocalRotation.Z);

        // Clear nearby marker spawners to prevent collisions
        ClearNearbyMarkers(targetArea, worldPos, request.MarkerClearRadius);

        // Load the generic terminal prefab
        var prefab = AssetAPI.GetLoadedAsset<GameObject>(TerminalPrefab);
        if (prefab == null)
        {
            Plugin.Logger.LogError(
                $"[CustomTerminal] Failed to load terminal prefab: {TerminalPrefab}");
            return;
        }

        // Instantiate the terminal
        var terminalGO = UnityEngine.Object.Instantiate(prefab, worldPos, worldRot);
        var terminal = terminalGO.GetComponentInChildren<LG_ComputerTerminal>();

        if (terminal == null)
        {
            Plugin.Logger.LogError(
                $"[CustomTerminal] Instantiated prefab has no LG_ComputerTerminal component");
            UnityEngine.Object.Destroy(terminalGO);
            return;
        }

        // Set SpawnNode on all spawned-in-node handlers (same pattern as LG_FunctionMarkerBuilder)
        foreach (var handler in terminalGO.GetComponentsInChildren<iLG_SpawnedInNodeHandler>())
            handler.SpawnNode = targetArea.m_courseNode;

        terminal.Setup();
        targetZone.TerminalsSpawnedInZone.Add(terminal);

        Plugin.Logger.LogInfo(
            $"[CustomTerminal] Spawned terminal at {worldPos} in zone {request.LocalIndex} " +
            $"(geo: {Path.GetFileName(request.GeomorphName)})");
    }

    private static void ClearNearbyMarkers(LG_Area area, Vector3 worldPos, float radius)
    {
        if (radius <= 0)
            return;

        // Collect all nearby spawners across ALL function types, then remove them.
        // This prevents any marker-spawned object from colliding with our terminal.
        var toRemove = new HashSet<IntPtr>();
        var functions = System.Enum.GetValues(typeof(ExpeditionFunction));

        foreach (ExpeditionFunction func in functions)
        {
            var spawners = area.GetAllMarkerSpawners(func);
            if (spawners == null)
                continue;

            foreach (var spawner in spawners)
            {
                if (toRemove.Contains(spawner.Pointer))
                    continue;

                var spawnerWorldPos = spawner.m_parent.transform.TransformPoint(spawner.m_localPosition);
                if (Vector3.Distance(spawnerWorldPos, worldPos) < radius)
                {
                    toRemove.Add(spawner.Pointer);
                    area.RemoveMarkerSpawner(spawner);  // removes from all function groups
                }
            }
        }

        Plugin.Logger.LogDebug(
            $"[CustomTerminal] Cleared {toRemove.Count} markers within {radius}m of {worldPos}");
    }
}
