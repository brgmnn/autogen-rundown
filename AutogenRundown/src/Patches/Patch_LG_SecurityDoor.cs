using System.Text.RegularExpressions;
using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using BepInEx;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using static LevelGeneration.eDoorStatus;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public static class Patch_LG_SecurityDoor
{
    private static List<LevelSecurityDoors> _levelDoors = new();

    public static readonly Dictionary<(string item, int dimension, int layer, int zone), List<string>> map = new();
    private static bool _isReady = false;
    private static readonly List<Action> _deferredActions = new();

    private const string Pattern = @"\[(?<ItemName>[A-Z_]+?)_(?:(?:[^\d_]*)(?<Dimension>\d+))_(?:(?:[^\d_]*)(?<Layer>\d+))_(?:(?:[^\d_]*)(?<Zone>\d+))(?:_(?<InstanceIndex>\d+))?\]";
    private const string Terminal = "TERMINAL";
    private const string Zone = "ZONE";

    internal static void Setup()
    {
        _levelDoors = LevelSecurityDoors.LoadAll();

        LevelAPI.OnBuildDone += () =>
        {
            BuildMap();
            _isReady = true;
            _deferredActions.ForEach(action => action());
            _deferredActions.Clear();
        };
        LevelAPI.OnLevelCleanup += () =>
        {
            _isReady = false;
            _deferredActions.Clear();
            map.Clear();
        };
    }

    public static LevelSecurityDoors? GetDoors()
    {
        var mainLayoutId = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;
        var levelSecurityDoors = _levelDoors.Find(lsd => lsd.MainLevelLayout == mainLayoutId);

        if (levelSecurityDoors is null)
            return null;

        return !levelSecurityDoors.Doors.Any() ? null : levelSecurityDoors;
    }

    public static List<string> Find((string name, int dimension, int layer, int zone) key)
    {
        if (map.TryGetValue(key, out var found))
            return found;

        var item = new List<string>();
        map.Add(key, item);

        return item;
    }

    internal static void BuildMap()
    {
        int count = 0;

        try
        {
            // collect all general terminal items
            foreach (var serial in LG_LevelInteractionManager.GetAllTerminalInterfaces())
            {
                if (serial?.Value?.SpawnNode == null)
                    continue;

                int split = serial.Key.LastIndexOf('_');

                if (split == -1)
                    continue;

                string itemName = serial.Key.Substring(0, split);
                string serialNumber = serial.Key.Substring(split + 1);

                if (itemName == Terminal)
                    continue; // skip terminals here

                int dimension = (int)serial.Value.SpawnNode.m_dimension.DimensionIndex;
                int layer = (int)serial.Value.SpawnNode.LayerType;
                int zone = (int)serial.Value.SpawnNode.m_zone.LocalIndex;
                // var globalIndex = (dimension, layer, zone);

                // map.GetOrAddNew(itemName).GetOrAddNew(globalIndex).Add(serialNumber);
                Find((itemName, dimension, layer, zone)).Add(serialNumber);

                count++;
            }

            // collect all terminals and zone alias numbers
            foreach (var zone in Builder.CurrentFloor.allZones)
            {
                // var globalIndex = ((int)zone.DimensionIndex, (int)zone.Layer.m_type, (int)zone.LocalIndex);

                // map.GetOrAddNew(Zone).GetOrAddNew(globalIndex).Add(zone.Alias.ToString());
                Find((Zone, (int)zone.DimensionIndex, (int)zone.Layer.m_type, (int)zone.LocalIndex))
                    .Add(zone.Alias.ToString());

                count++;

                foreach (var term in zone.TerminalsSpawnedInZone)
                {
                    if (term is null)
                        continue;

                    int split = term.m_terminalItem.TerminalItemKey.LastIndexOf('_');

                    if (split == -1)
                        continue;

                    string serialNumber = term.m_terminalItem.TerminalItemKey.Substring(split + 1);

                    // map.GetOrAddNew(Terminal).GetOrAddNew(globalIndex).Add(serialNumber);
                    Find((Terminal, (int)zone.DimensionIndex, (int)zone.Layer.m_type, (int)zone.LocalIndex))
                        .Add(serialNumber);
                    count++;
                }
            }

            // // parse door interaction text
            // foreach (var locks in UnityEngine.Object.FindObjectsOfType<LG_SecurityDoor_Locks>())
            // {
            //     locks.m_intCustomMessage.m_message = ParseTextFragments(locks.m_intCustomMessage.m_message);
            //     locks.m_intOpenDoor.InteractionMessage = ParseTextFragments(locks.m_intOpenDoor.InteractionMessage);
            // }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Building map failed with error:\n{e}");
        }
    }

    public static string ReplaceAll(string input)
    {
        if (input.IsNullOrWhiteSpace())
            return input;

        var result = $"{input}";

        foreach (Match match in Regex.Matches(input, Pattern))
        {
            if (match.Success && TryFindSerialNumber(match, out var serialStr))
            {
                result = result.Replace(match.Value, serialStr);
            }
        }

        return result;
    }

    public static bool TryFindSerialNumber(Match match, out string serialStr)
    {
        string itemName = match.Groups["ItemName"].Value;
        int dimension = int.Parse(match.Groups["Dimension"].Value);
        int layer = int.Parse(match.Groups["Layer"].Value);
        int zone = int.Parse(match.Groups["Zone"].Value);
        int instanceIndex = match.Groups["InstanceIndex"].Success ? int.Parse(match.Groups["InstanceIndex"].Value) : 0;

        // if (map.TryGetValue(itemName, out var localSerialMap) &&
        //     localSerialMap.TryGetValue((dimension, layer, zone), out var serialList))
        if (map.TryGetValue((itemName, dimension, layer, zone), out var serialList))
        {
            Plugin.Logger.LogInfo($"[SerialLookupManager] Got to first part: {serialList}");

            if (instanceIndex < serialList.Count)
            {
                string serialNumber = serialList[instanceIndex];
                serialStr = $"<color=orange>{itemName}{(itemName != Zone ? "_" : " ")}{serialNumber}</color>";
                return true;
            }
        }

        serialStr = match.Value;
        Plugin.Logger.LogError($"[SerialLookupManager] No match found (map size = {map.Count}) for TerminalItem: '{itemName}' in (D{dimension}, L{layer}, Z{zone}) at instance #{instanceIndex}");
        return false;
    }

    public static string PrintMap()
    {
        return map.Select((entry) =>
            $"[{entry.Key.item}_{entry.Key.dimension}_{entry.Key.layer}_{entry.Key.zone}] -> [{string.Join(",", entry.Value)}]")
            .Join(null, "\n");
    }

    [HarmonyPatch(typeof(LG_SecurityDoor_Locks), nameof(LG_SecurityDoor_Locks.OnDoorState))]
    [HarmonyPostfix]
    private static void Post_LG_SecurityDoor_Locks_OnDoorState(LG_SecurityDoor_Locks __instance, pDoorState state, bool isDropinState = false)
    {
        var level = GetDoors();

        if (level is null)
            return;

        var dim = __instance.m_door.Gate.DimensionIndex;
        var layer = __instance.m_door.LinksToLayerType;
        var localIndex = __instance.m_door.LinkedToZoneData.LocalIndex;

        // TODO: add dimension check
        var door = level.Doors.Find(door => door.Layer == (int)layer &&
                                            door.ZoneNumber == (int)localIndex);

        if (door?.InteractionMessage is null)
            return;

        switch (state.status)
        {
            case Closed_LockedWithChainedPuzzle_Alarm:
            case Closed_LockedWithChainedPuzzle:
            case Closed_LockedWithPowerGenerator:
            case Closed_LockedWithNoKey:
            {
                if (_isReady)
                    SetMessage();
                else
                    _deferredActions.Add(SetMessage);

                break;

                void SetMessage()
                {
                    __instance.m_intOpenDoor.InteractionMessage = ReplaceAll(door.InteractionMessage);
                }
            }
        }
    }
}
