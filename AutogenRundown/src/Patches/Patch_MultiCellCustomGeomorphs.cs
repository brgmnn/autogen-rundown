using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

/// <summary>
/// Lets forced custom geomorphs that are longer than one 64x64 cell be used safely.
///
/// LG_Floor.FindExternalArea always gives a forced custom geomorph a 1x1 tile (GetTileShape
/// hardcodes s1x1) and places the prefab on that cell, rotated to face along the source plug.
/// A prefab whose origin is the centre of its first cell and which carries on straight ahead
/// (e.g. the 64x128 LegacyRX2_Bridge) therefore overhangs into cells the grid still thinks
/// are empty: later zones build tiles inside the overhang, and AI graph lookups there find
/// no geomorph.
///
/// For the prefabs listed in <see cref="ForwardCells"/> this refuses the placement when the
/// overhang cells are not free (the zone then tries another expander), and claims them for
/// the new tile once it is placed. GetExternalCell already steps past cells owned by the
/// source geomorph, so the far-end plugs then link to the correct neighbouring cell.
/// </summary>
[HarmonyPatch]
internal static class Patch_MultiCellCustomGeomorphs
{
    /// <summary>
    /// Extra cells each prefab extends straight ahead of its origin cell (prefab +Z).
    /// </summary>
    private static readonly Dictionary<string, int> ForwardCells = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Assets/DogCustomGeos/Tilepack/Releases/LegacyRX2/LegacyRX2_Bridge.prefab"] = 1,
    };

    [ThreadStatic] private static List<LG_Cell>? _pendingCells;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.FindExternalArea))]
    public static bool Pre_FindExternalArea(
        LG_Floor __instance,
        LG_Zone zone,
        LG_Plug plug,
        LG_Area sourceArea,
        bool allowedToAddGeomorph,
        ref LG_Area area,
        ref bool createdGeoMorph,
        ref bool __result)
    {
        _pendingCells = null;

        if (!allowedToAddGeomorph || !zone.m_settings.HasCustomGeomorphPrefab)
            return true;

        var path = zone.m_settings.m_zoneData.CustomGeomorph;

        if (string.IsNullOrEmpty(path) || !ForwardCells.TryGetValue(path, out var count))
            return true;

        // An existing tile in the target cell means the game links to it instead of placing
        // the prefab; leave that (and any lookup failure) to the original.
        if (!__instance.GetExternalCell(plug, sourceArea, out var cell, (int)zone.DimensionIndex)
            || cell.m_grouping != null
            || !__instance.GetDimension(zone.DimensionIndex, out var dimension))
            return true;

        var (dx, dz) = plug.m_dir switch
        {
            LG_PlugDir.Up => (0, 1),
            LG_PlugDir.Right => (1, 0),
            LG_PlugDir.Down => (0, -1),
            LG_PlugDir.Left => (-1, 0),
            _ => (0, 0)
        };

        if (dx == 0 && dz == 0)
            return true;

        var cells = new List<LG_Cell>();

        for (var i = 1; i <= count; i++)
        {
            var next = dimension.Grid.GetCell(cell.x + dx * i, cell.z + dz * i);

            if (next == null || next.m_grouping != null)
            {
                Plugin.Logger.LogDebug(
                    $"[MultiCellGeo] Zone {zone.Alias}: no room for {path} from cell " +
                    $"({cell.x}, {cell.z}) facing {plug.m_dir}, trying another expander");

                area = null!;
                createdGeoMorph = false;
                __result = false;

                return false;
            }

            cells.Add(next);
        }

        _pendingCells = cells;

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.FindExternalArea))]
    public static void Post_FindExternalArea(LG_Zone zone, ref bool createdGeoMorph)
    {
        var cells = _pendingCells;
        _pendingCells = null;

        if (cells == null || !createdGeoMorph)
            return;

        var tile = zone.m_settings.CustomGeomorphInstance.GetComponent<LG_Geomorph>().m_tile;

        foreach (var cell in cells)
            tile.AddCell(cell);

        Plugin.Logger.LogDebug(
            $"[MultiCellGeo] Zone {zone.Alias}: claimed {cells.Count} extra cell(s) for " +
            zone.m_settings.m_zoneData.CustomGeomorph);
    }
}
