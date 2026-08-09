using Agents;
using AIGraph;
using Enemies;
using GameData;
using HarmonyLib;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Handles enemy groups that ask for a spawn align marker the area doesn't have.
///
/// AlignSpawn resolves a marker from LG_Area.m_spawnAligns — a per-area, prefab-authored
/// list that only the boss room of a boss geomorph carries. LG_PopulateZone.TryPlaceGroup
/// picks the highest scored area in the zone with a seed randomised scorer, so a group is
/// never guaranteed to land in the area holding the markers.
///
/// Two behaviours, one cause:
///
/// 1. CRASH FIX. Align_0..Align_5 are null checked before use:
///
///        if (transform != null) { spawnType = Position; Position = transform.position; ... }
///        else                   { Rotation = GetRandomRotation(); }
///
///    The CycleAllAligns branch is not:
///
///        Transform align = GetAlign(courseNode, 0);
///        data.spawnType  = eEnemyGroupSpawnType.CycleAligns;
///        data.Position   = align.position;   // NRE when the area has no aligns
///
///    That NRE propagates out of LG_PopulateArea.Build into LG_Factory.Update, which
///    re-invokes the same failing job every frame — the build never drains and the level
///    hangs on the drop screen. We downgrade the block to Default for just the call that
///    would have thrown, then restore it, so a group that cycles fine elsewhere still does.
///
///    Our own boss groups emit Align_N now (EnemyGroup.SaveStatic), so this is a net for
///    anything else shipping CycleAllAligns — vanilla data, peer mods, future geos.
///
/// 2. DIAGNOSTIC. For Align_0..Align_5 the game falls back to a random in-area spawn
///    silently, which means there is no way to tell whether a geo actually authors the
///    marker a group asked for. We log it. Absence of these lines on a two-boss room is the
///    confirmation that both markers exist and the second boss really is being placed.
///
/// Patched at GetSpawnData rather than AlignSpawn itself: AlignSpawn is a private static
/// taking `ref pEnemyGroupSpawnData`, and that non-blittable by-ref struct is exactly the
/// il2cpp interop shape that has produced ObjectCollectedException races here before.
/// GetSpawnData takes only reference types and primitives.
/// </summary>
[HarmonyPatch]
internal static class Fix_MissingSpawnAligns
{
    private static readonly HashSet<(uint groupId, string area)> s_logged = new();

    public static void ResetDiagnostics() => s_logged.Clear();

    /// <summary>
    /// Marker index a placement type needs, or null for Default (which never aligns).
    /// </summary>
    private static int? RequiredAlignIndex(eSpawnPlacementType placement)
        => placement switch
        {
            eSpawnPlacementType.Align_0 => 0,
            eSpawnPlacementType.Align_1 => 1,
            eSpawnPlacementType.Align_2 => 2,
            eSpawnPlacementType.Align_3 => 3,
            eSpawnPlacementType.Align_4 => 4,
            eSpawnPlacementType.Align_5 => 5,

            // Reads marker 0 and then cycles the whole list
            eSpawnPlacementType.CycleAllAligns => 0,

            _ => null
        };

    // Argument types pin the 11 argument overload. The 10 argument one forwards to it, and it
    // is the only caller of AlignSpawn, so this is the single chokepoint.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EnemyGroup), nameof(EnemyGroup.GetSpawnData),
        new[]
        {
            typeof(Vector3), typeof(Quaternion), typeof(AIG_CourseNode), typeof(EnemyGroupType),
            typeof(eEnemyGroupSpawnType), typeof(uint), typeof(float), typeof(IReplicator),
            typeof(SurvivalWave), typeof(uint), typeof(AgentMode)
        })]
    public static void Pre_GetSpawnData(
        AIG_CourseNode courseNode,
        uint persistentGameDataID,
        out EnemyGroupDataBlock? __state)
    {
        __state = null;

        // AlignSpawn returns before touching the block for id 0
        if (persistentGameDataID == 0)
            return;

        var block = GameDataBlockBase<EnemyGroupDataBlock>.GetBlock(persistentGameDataID);

        if (block == null)
            return;

        var wanted = RequiredAlignIndex(block.SpawnPlacementType);

        if (wanted == null)
            return;

        // Unity null checks, not `?.`: these are UnityEngine.Objects and a destroyed one is
        // a live reference that compares equal to null.
        var area = courseNode == null ? null : courseNode.m_area;
        var aligns = area == null ? null : area.m_spawnAligns;

        // GetAlign returns null for a short list, and the list can also hold a destroyed
        // transform. Both end up dereferenced by the CycleAllAligns branch.
        var index = wanted.Value;
        var hasAlign = aligns != null && index < aligns.Count && aligns[index] != null;

        if (hasAlign)
            return;

        var areaName = area == null ? "<null>" : area.name;

        if (s_logged.Add((persistentGameDataID, areaName)))
            Plugin.Logger.LogWarning(
                $"[MissingSpawnAligns] EnemyGroup {persistentGameDataID} wants " +
                $"{block.SpawnPlacementType} but area \"{areaName}\" has " +
                $"{(aligns == null ? 0 : aligns.Count)} spawn align(s). " +
                $"Enemy will spawn at a random position in the area instead.");

        // Align_0..Align_5 already fall back safely; only CycleAllAligns would throw.
        if (block.SpawnPlacementType != eSpawnPlacementType.CycleAllAligns)
            return;

        __state = block;
        block.SpawnPlacementType = eSpawnPlacementType.Default;
    }

    /// <summary>
    /// Finalizer rather than postfix so the datablock is restored even if the original still
    /// throws for some unrelated reason. Level generation is single threaded and GetSpawnData
    /// is not reentrant, so the prefix/finalizer pairing is enough to keep the block honest.
    /// </summary>
    [HarmonyFinalizer]
    [HarmonyPatch(typeof(EnemyGroup), nameof(EnemyGroup.GetSpawnData),
        new[]
        {
            typeof(Vector3), typeof(Quaternion), typeof(AIG_CourseNode), typeof(EnemyGroupType),
            typeof(eEnemyGroupSpawnType), typeof(uint), typeof(float), typeof(IReplicator),
            typeof(SurvivalWave), typeof(uint), typeof(AgentMode)
        })]
    public static void Post_GetSpawnData(EnemyGroupDataBlock? __state)
    {
        if (__state != null)
            __state.SpawnPlacementType = eSpawnPlacementType.CycleAllAligns;
    }
}
