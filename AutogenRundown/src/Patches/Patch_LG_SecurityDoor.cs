using System.Text.RegularExpressions;
using BepInEx;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public static class Patch_LG_SecurityDoor
{
    public static readonly Dictionary<(string item, int dimension, int layer, int zone), List<string>> map = new();

    private const string Pattern = @"\[(?<ItemName>[A-Z_]+?)_(?:(?:[^\d_]*)(?<Dimension>\d+))_(?:(?:[^\d_]*)(?<Layer>\d+))_(?:(?:[^\d_]*)(?<Zone>\d+))(?:_(?<InstanceIndex>\d+))?\]";
    private const string Terminal = "TERMINAL";
    private const string Zone = "ZONE";

    internal static void Setup()
    {
        LevelAPI.OnBuildDone += BuildMap;
        LevelAPI.OnLevelCleanup += () => map.Clear();
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
            Plugin.Logger.LogError($"[SerialLookupManager] We had to exit early because an error occured:\n{e}");
        }

        Plugin.Logger.LogInfo($"[SerialLookupManager] On build done, collected {count} serial numbers");
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
        Plugin.Logger.LogError($"[SerialLookupManager] No match found for TerminalItem: '{itemName}' in (D{dimension}, L{layer}, Z{zone}) at instance #{instanceIndex}");
        return false;
    }

    public static string PrintMap()
    {
        return map.Select((entry) =>
            $"[{entry.Key.item}_{entry.Key.dimension}_{entry.Key.layer}_{entry.Key.zone}] -> [{string.Join(",", entry.Value)}]")
            .Join(null, "\n");
    }

    [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.Setup))]
    [HarmonyFinalizer]
    // [HarmonyAfter("Inas.EOSExt.SecDoor")]
    private static void Post_LG_SecurityDoor_Setup(LG_SecurityDoor __instance)
    {
        var intOpenDoor = __instance.m_locks.Cast<LG_SecurityDoor_Locks>()?.m_intOpenDoor;

        if (intOpenDoor == null)
            return;

        Plugin.Logger.LogDebug($"What is the interaction text: {intOpenDoor.InteractionMessage}");

        // Plugin.Logger.LogDebug($"Ok what is this? {res}");


        var door = __instance;
        // var dim = door.Gate.DimensionIndex;
        // var layer = door.LinksToLayerType;
        // var localIndex = door.LinkedToZoneData.LocalIndex;
        // var def = SecDoorIntTextOverrideManager.Current.GetDefinition(dim, layer, localIndex);

        // Interact_Timed intOpenDoor = __instance.m_locks.Cast<LG_SecurityDoor_Locks>()?.m_intOpenDoor;

        // if (intOpenDoor == null)
        //     return;

        //if (state.status != eDoorStatus.Unlocked && state.status != eDoorStatus.Closed_LockedWithChainedPuzzle) return;
        door.m_sync.add_OnDoorStateChange(new Action<pDoorState, bool>((state, isRecall) =>
        {
            //EOSLogger.Warning($"OnSyncDoorStatusChange: {state.status}");

            // string textToReplace = string.IsNullOrEmpty(def.TextToReplace) ? intOpenDoor.InteractionMessage : def.TextToReplace; ;

            // StringBuilder sb = new();
            // if (!string.IsNullOrEmpty(def.Prefix))
            // {
                // sb.Append(def.Prefix).AppendLine();
            // }

            // sb.Append(textToReplace);

            // if (!string.IsNullOrEmpty(def.Postfix))
            // {
            //     sb.AppendLine().Append(def.Postfix);
            // }

            // intOpenDoor.InteractionMessage = sb.ToString();

            Plugin.Logger.LogWarning($"Got door message: {intOpenDoor.InteractionMessage}");

            Plugin.Logger.LogDebug($"Map is:\n{PrintMap()}");

            intOpenDoor.InteractionMessage = ReplaceAll(intOpenDoor.InteractionMessage);

            // EOSLogger.Debug($"SecDoorIntTextOverride: Override IntText. {def.LocalIndex}, {def.LayerType}, {def.DimensionIndex}");
        }));
    }
}
