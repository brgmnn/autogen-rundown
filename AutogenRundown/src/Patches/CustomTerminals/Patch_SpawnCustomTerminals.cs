using GameData;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using Localization;
using UnityEngine;

namespace AutogenRundown.Patches.CustomTerminals;

[HarmonyPatch]
internal static class Patch_SpawnCustomTerminals
{
    private const string TerminalPrefab =
        "Assets/AssetPrefabs/Complex/Generic/FunctionMarkers/Terminal_Floor.prefab";

    // Track zones where a custom terminal has claimed the warden objective,
    // so the game's standard distribution doesn't duplicate it on another terminal
    // in the same zone. Scoped to zone (not layer) to avoid blocking legitimate
    // multi-objective setups on different zones within the same layer.
    private static readonly HashSet<(LG_LayerType, int)> CustomWardenObjectiveZones = new();
    private static uint _lastBuildLayoutId;

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

        // Clear once per level build, not per layer
        if (mainLayoutId != _lastBuildLayoutId)
        {
            CustomWardenObjectiveZones.Clear();
            _lastBuildLayoutId = mainLayoutId;
        }

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

    /// <summary>
    /// Prevents the game's standard distribution from setting up a second warden objective
    /// terminal when a custom terminal has already claimed that role for the layer.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.SetupAsWardenObjectiveSpecialCommand))]
    private static bool Pre_SetupAsWardenObjectiveSpecialCommand(LG_ComputerTerminal __instance)
    {
        var layerType = __instance.SpawnNode.LayerType;
        var zoneIndex = (int)__instance.SpawnNode.m_zone.LocalIndex;

        if (CustomWardenObjectiveZones.Contains((layerType, zoneIndex)))
        {
            Plugin.Logger.LogDebug(
                $"[CustomTerminal] Skipping duplicate warden objective setup on {__instance.name} " +
                $"— custom terminal already owns zone {__instance.SpawnNode.m_zone.LocalIndex} in {layerType}");
            return false;
        }

        return true;
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

        // Instantiate the terminal as a child of the area so that
        // LG_GenerateNavigationInfoJob finds its LG_ComputerTerminalMapLookatRevealer
        // via area.GetComponentsInChildren and adds it to the map.
        var terminalGO = UnityEngine.Object.Instantiate(prefab, worldPos, worldRot, targetArea.transform);
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

        // Build game-native terminal data from spawn request
        var startStateData = BuildStartStateData(request);
        var placementData = BuildPlacementData(request);

        terminal.Setup(startStateData, placementData);

        // Add log files to the terminal
        foreach (var logFile in request.LogFiles)
        {
            var logFileData = BuildLogFileData(logFile);
            if (logFileData != null)
                terminal.AddLocalLog(logFileData);
        }

        // Set up as warden objective terminal if flagged
        if (request.IsWardenObjective)
        {
            terminal.SetupAsWardenObjectiveSpecialCommand(0);
            var zoneIndex = (int)targetZone.LocalIndex;
            CustomWardenObjectiveZones.Add((request.LayerType, zoneIndex));
        }

        targetZone.TerminalsSpawnedInZone.Add(terminal);

        Plugin.Logger.LogInfo(
            $"[CustomTerminal] Spawned terminal at {worldPos} in zone {request.LocalIndex} " +
            $"(geo: {Path.GetFileName(request.GeomorphName)}, wardenObj: {request.IsWardenObjective})");
    }

    private static void ClearNearbyMarkers(LG_Area area, Vector3 worldPos, float radius)
    {
        if (radius <= 0)
            return;

        // Collect all nearby spawners across ALL function types, then remove them.
        // This prevents any marker-spawned object from colliding with our terminal.
        // NOTE: GetAllMarkerSpawners returns the internal list, so we must NOT modify
        // it during iteration. Collect first, then remove.
        var toRemove = new List<LG_MarkerSpawner>();
        var seen = new HashSet<IntPtr>();
        var functions = System.Enum.GetValues(typeof(ExpeditionFunction));

        foreach (ExpeditionFunction func in functions)
        {
            var spawners = area.GetAllMarkerSpawners(func);
            if (spawners == null)
                continue;

            foreach (var spawner in spawners)
            {
                if (seen.Contains(spawner.Pointer))
                    continue;
                seen.Add(spawner.Pointer);

                var spawnerWorldPos = spawner.m_parent.transform.TransformPoint(spawner.m_localPosition);
                if (Vector3.Distance(spawnerWorldPos, worldPos) < radius)
                    toRemove.Add(spawner);
            }
        }

        foreach (var spawner in toRemove)
            area.RemoveMarkerSpawner(spawner);

        Plugin.Logger.LogDebug(
            $"[CustomTerminal] Cleared {toRemove.Count} markers within {radius}m of {worldPos}");
    }

    private static TerminalStartStateData? BuildStartStateData(CustomTerminalSpawnRequest request)
    {
        var src = request.StartingStateData;
        if (src == null)
            return null;

        return new TerminalStartStateData
        {
            StartingState = (TERM_State)src.StartingState,
            AudioEventEnter = (uint)src.AudioEventEnter,
            AudioEventExit = (uint)src.AudioEventExit,
            PasswordProtected = src.PasswordProtected,
            PasswordHintText = src.PasswordHintText,
            GeneratePassword = src.GeneratePassword,
            PasswordPartCount = src.PasswordPartCount,
            ShowPasswordLength = src.ShowPasswordLength,
            ShowPasswordPartPositions = src.ShowPasswordPartPositions
        };
    }

    private static TerminalPlacementData? BuildPlacementData(CustomTerminalSpawnRequest request)
    {
        if (request.UniqueCommands.Count == 0)
            return null;

        var placementData = new TerminalPlacementData();

        foreach (var cmd in request.UniqueCommands)
        {
            var gameCmd = new GameData.CustomTerminalCommand
            {
                Command = cmd.Command,
                CommandDesc = cmd.CommandDescId,
                SpecialCommandRule = (TERM_CommandRule)cmd.SpecialCommandRule
            };
            placementData.UniqueCommands.Add(gameCmd);
        }

        return placementData;
    }

    private static TerminalLogFileData? BuildLogFileData(AutogenRundown.DataBlocks.Terminals.LogFile logFile)
    {
        if (logFile.FileContentId == 0)
            return null;

        // Resolve text content from the game's text datablock system
        var textBlock = GameDataBlockBase<TextDataBlock>.GetBlock(logFile.FileContentId);
        if (textBlock == null)
        {
            Plugin.Logger.LogWarning(
                $"[CustomTerminal] Could not resolve text block {logFile.FileContentId} " +
                $"for log file '{logFile.FileName}'");
            return null;
        }

        return new TerminalLogFileData
        {
            FileName = logFile.FileName,
            FileContent = new LocalizedText
            {
                Id = logFile.FileContentId,
                UntranslatedText = textBlock.English
            },
            AttachedAudioFile = (uint)logFile.AttachedAudioFile,
            AttachedAudioByteSize = logFile.AttachedAudioByteSize,
            PlayerDialogToTriggerAfterAudio = (uint)logFile.PlayerDialogToTriggerAfterAudio
        };
    }
}
