using Agents;
using AIGraph;
using Enemies;
using GameData;
using HarmonyLib;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Guards the one unchecked null dereference in EnemyGroup.AlignSpawn.
///
/// AlignSpawn resolves a spawn align transform from LG_Area.m_spawnAligns — a per-area,
/// prefab-authored list that only the boss room of a boss geomorph carries. Every
/// Align_0..Align_5 placement is null checked before use:
///
///     if (transform != null) { spawnType = Position; Position = transform.position; ... }
///     else                   { Rotation = GetRandomRotation(); }
///
/// The CycleAllAligns branch is not:
///
///     Transform align = GetAlign(courseNode, 0);
///     data.spawnType  = eEnemyGroupSpawnType.CycleAligns;
///     data.Position   = align.position;   // NRE when the area has no aligns
///
/// LG_PopulateZone.TryPlaceGroup picks the highest scored area in the zone using a seed
/// randomised scorer, so a group is not guaranteed to land in the area that holds the
/// aligns. When it doesn't, the NRE propagates out of LG_PopulateArea.Build into
/// LG_Factory.Update, which re-invokes the same failing job every frame — the build never
/// drains and the level hangs on the drop screen.
///
/// Our own boss groups now emit Align_0 instead (EnemyGroup.SaveStatic), so this is a net
/// for anything else that ships CycleAllAligns — vanilla data, peer mods, future geos.
/// We downgrade to Default only for the single call that would have thrown, so a group
/// that cycles correctly elsewhere keeps doing so, and restore the block immediately after.
///
/// Patched at GetSpawnData rather than AlignSpawn itself: AlignSpawn is a private static
/// taking `ref pEnemyGroupSpawnData`, and that non-blittable by-ref struct is exactly the
/// il2cpp interop shape that has produced ObjectCollectedException races here before.
/// GetSpawnData takes only reference types and primitives.
/// </summary>
[HarmonyPatch]
internal static class Fix_CycleAllAlignsNullSpawn
{
    private static readonly HashSet<uint> s_logged = new();

    public static void ResetDiagnostics() => s_logged.Clear();

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

        if (block == null || block.SpawnPlacementType != eSpawnPlacementType.CycleAllAligns)
            return;

        // Unity null checks, not `?.`: these are UnityEngine.Objects and a destroyed one is
        // a live reference that compares equal to null.
        var area = courseNode == null ? null : courseNode.m_area;
        var aligns = area == null ? null : area.m_spawnAligns;

        // GetAlign(courseNode, 0) returns null for an empty list, and the list can also hold
        // a destroyed transform. Both end up dereferenced.
        if (aligns != null && aligns.Count > 0 && aligns[0] != null)
            return;

        if (s_logged.Add(persistentGameDataID))
            Plugin.Logger.LogWarning(
                $"[CycleAllAlignsNullSpawn] EnemyGroup {persistentGameDataID} requests CycleAllAligns " +
                $"but area \"{(area == null ? "<null>" : area.name)}\" has no usable spawn aligns. " +
                $"Falling back to Default placement for this spawn to avoid hanging LG_Factory.");

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
