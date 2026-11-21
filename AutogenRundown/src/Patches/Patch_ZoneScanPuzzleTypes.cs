using AutogenRundown.DataBlocks.Custom.ZoneScan;
using ChainedPuzzles;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Registers the zone scan puzzle component types (<see cref="ZoneScans.Types"/>) with the
/// ChainedPuzzleManager: a copy of the base scan's prefab with a zero radius circle, a whole
/// team requirement and our scan speeds. ZoneScan then does the area/zone player check.
///
/// Done here rather than through MTFO's Custom/PuzzleTypes.json because MTFO reads that file
/// in its plugin Load, and AmorLib's dependency on MTFO makes MTFO load before us, i.e. before
/// we have generated anything.
/// </summary>
[HarmonyPatch]
internal static class Patch_ZoneScanPuzzleTypes
{
    /// <summary>
    /// Colours of the types that don't keep their base scan's colours. ZoneScan copies the
    /// scan's colour onto its boundary graphics, so these are also the colours of the outlines.
    /// </summary>
    private static readonly (eChainedPuzzleGraphicsColorMode Mode, Color Color)[] Colors =
    {
        (eChainedPuzzleGraphicsColorMode.Waiting,        new Color(0.00f, 0.60f, 1.00f, 1f)),
        (eChainedPuzzleGraphicsColorMode.Active,         new Color(0.20f, 0.85f, 1.00f, 1f)),
        (eChainedPuzzleGraphicsColorMode.TimedOut,       new Color(0.40f, 0.40f, 0.40f, 1f)),
        (eChainedPuzzleGraphicsColorMode.Alarm_Waiting,  new Color(0.60f, 0.00f, 1.00f, 1f)),
        (eChainedPuzzleGraphicsColorMode.Alarm_Active,   new Color(0.85f, 0.25f, 1.00f, 1f)),
        (eChainedPuzzleGraphicsColorMode.Alarm_TimedOut, new Color(0.35f, 0.00f, 0.60f, 1f)),
    };

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChainedPuzzleManager), nameof(ChainedPuzzleManager.OnAssetsLoaded))]
    public static void Post_OnAssetsLoaded(ChainedPuzzleManager __instance)
    {
        var prefabs = __instance.m_puzzleComponentPrefabs;

        // Inactive parent of the prefab copies: keeps them out of the level (no Awake/Update)
        // while the copies spawned from them are active.
        var holder = new GameObject("AutogenRundown_ZoneScanPrefabs");
        holder.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(holder);

        foreach (var type in ZoneScans.Types)
            prefabs.Add(
                (uint)type.Component.PuzzleType,
                CreatePrefab(type, prefabs[(uint)type.BaseScan], holder.transform));

        Plugin.Logger.LogDebug($"[ZoneScan] Registered {ZoneScans.Types.Count} zone scan puzzle component types");
    }

    private static GameObject CreatePrefab(ZoneScanType type, GameObject basePrefab, Transform holder)
    {
        var prefab = UnityEngine.Object.Instantiate(basePrefab, holder);
        prefab.name = $"{basePrefab.name}_ZoneScan_{(uint)type.Component.PuzzleType}";

        // Whole team required, so team size doesn't change the duration
        var speed = (float)(1.0 / type.Component.Duration);

        var scanner = prefab.GetComponent<CP_PlayerScanner>();
        scanner.m_playerRequirement = PlayerRequirement.All;
        // ZoneScan replaces the radius check, keep the vanilla one from adding progress too
        scanner.m_scanRadius = 1f;
        scanner.m_reduceWhenNoPlayer = type.Sustained;
        scanner.m_reduceSpeed = type.Sustained ? speed / 2 : 0f;

        var graphics = prefab.GetComponent<CP_Bioscan_Graphics>();
        graphics.m_radius = 0f;

        if (!type.BaseColors)
            graphics.m_colors = new Il2CppReferenceArray<ColorModeColor>(
                Colors.Select(c => new ColorModeColor { mode = c.Mode, col = c.Color }).ToArray());

        graphics.SetText(type.ScanText);

        return prefab;
    }
}
