using Agents;
using AIGraph;
using Enemies;
using GameData;
using HarmonyLib;
using LevelGeneration;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Decides which spawn align marker an enemy group actually gets, instead of trusting the
/// index its datablock asked for.
///
/// Markers live in LG_Area.m_spawnAligns — a per-area, prefab-authored list that only the
/// boss room of a boss geomorph carries. Two problems come out of that:
///
/// 1. THE AREA MAY HAVE NO MARKER AT ALL. LG_PopulateZone.TryPlaceGroup picks the highest
///    scored area in the zone with a seed randomised scorer, so a group is never guaranteed
///    to land in the area holding the markers. Align_0..Align_5 handle that themselves:
///
///        if (transform != null) { spawnType = Position; Position = transform.position; ... }
///        else                   { Rotation = GetRandomRotation(); }
///
///    CycleAllAligns does not:
///
///        Transform align = GetAlign(courseNode, 0);
///        data.spawnType  = eEnemyGroupSpawnType.CycleAligns;
///        data.Position   = align.position;   // NRE when the area has no aligns
///
///    That NRE escapes LG_PopulateArea.Build into LG_Factory.Update, which re-invokes the
///    same failing job every frame — the build never drains and the level hangs on the drop
///    screen.
///
/// 2. MARKERS CAN BE CO-LOCATED. Several boss geos author marker 0 and marker 1 on
///    effectively the same spot, so two bosses asking for different indices still end up
///    stacked. Nothing in the game prevents this: unlike RandomInArea, which pushes groups
///    apart by stamping PlacementHeat = 1000f on every node it hands out
///    (AIG_NodeCluster.TryGetPlacements / Scorer_BestPlacement), align markers have no
///    spread mechanism, and EnemyGroupDataBlock has no offset or radius field to lean on.
///
/// So we claim markers per area as they are handed out and only grant one that is at least
/// MinSeparation from everything already claimed there. When nothing qualifies — no markers,
/// or only markers too close to a boss already placed — we downgrade the block to Default for
/// that single call. The group then goes through the node cluster and the heat spread
/// separates it, which is what an un-aligned boss has always done.
///
/// Patched at GetSpawnData rather than AlignSpawn itself: AlignSpawn is a private static
/// taking `ref pEnemyGroupSpawnData`, and that non-blittable by-ref struct is exactly the
/// il2cpp interop shape that has produced ObjectCollectedException races here before.
/// GetSpawnData takes only reference types and primitives.
/// </summary>
[HarmonyPatch]
internal static class Fix_MissingSpawnAligns
{
    /// <summary>
    /// How far apart two boss markers have to be for us to treat them as distinct spots. A
    /// tank is roughly 3m across, so this is "not touching" without being strict enough to
    /// reject markers a geo genuinely placed apart.
    /// </summary>
    private const float MinSeparation = 4.0f;

    /// <summary>
    /// Highest index eSpawnPlacementType can express (Align_0..Align_5).
    /// </summary>
    private const int MaxAlignIndex = 5;

    /// <summary>
    /// Marker positions already handed out, keyed by area instance id. Cleared per build.
    /// </summary>
    private static readonly Dictionary<int, List<Vector3>> s_claimed = new();

    private static readonly HashSet<(uint groupId, string area)> s_loggedFallback = new();
    private static readonly HashSet<int> s_loggedLayout = new();

    public static void ResetDiagnostics()
    {
        s_claimed.Clear();
        s_loggedFallback.Clear();
        s_loggedLayout.Clear();
    }

    /// <summary>
    /// Marker index a placement type asks for, or null for Default (which never aligns).
    /// </summary>
    private static int? RequestedAlignIndex(eSpawnPlacementType placement)
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

    private static eSpawnPlacementType AlignPlacement(int index)
        => index switch
        {
            0 => eSpawnPlacementType.Align_0,
            1 => eSpawnPlacementType.Align_1,
            2 => eSpawnPlacementType.Align_2,
            3 => eSpawnPlacementType.Align_3,
            4 => eSpawnPlacementType.Align_4,
            _ => eSpawnPlacementType.Align_5
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
        EnemyGroupType groupType,
        uint persistentGameDataID,
        out eSpawnPlacementType? __state)
    {
        __state = null;

        // Both guards mirror AlignSpawn's own first line -- it returns before touching the
        // block for these, so wave spawns must not claim markers the static boss population
        // is going to want.
        if (persistentGameDataID == 0 || groupType == EnemyGroupType.Survival)
            return;

        var block = GameDataBlockBase<EnemyGroupDataBlock>.GetBlock(persistentGameDataID);

        if (block == null)
            return;

        var requested = RequestedAlignIndex(block.SpawnPlacementType);

        if (requested == null)
            return;

        // Unity null checks, not `?.`: these are UnityEngine.Objects and a destroyed one is
        // a live reference that compares equal to null.
        var area = courseNode == null ? null : courseNode.m_area;
        var aligns = area == null ? null : area.m_spawnAligns;
        var areaName = area == null ? "<null>" : area.name;
        var count = aligns == null ? 0 : aligns.Count;

        if (area != null)
            LogLayout(area, areaName, aligns, count);

        var claimed = ClaimedIn(area);
        var chosen = ChooseAlign(aligns, count, requested.Value, claimed);

        if (chosen != null)
        {
            claimed?.Add(aligns![chosen.Value].position);

            if (chosen.Value == requested.Value)
                return;

            __state = block.SpawnPlacementType;
            block.SpawnPlacementType = AlignPlacement(chosen.Value);

            return;
        }

        if (s_loggedFallback.Add((persistentGameDataID, areaName)))
            Plugin.Logger.LogWarning(
                $"[MissingSpawnAligns] EnemyGroup {persistentGameDataID} wants " +
                $"{block.SpawnPlacementType} in area \"{areaName}\" ({count} marker(s)), but no " +
                $"marker is free and at least {MinSeparation}m from one already used there. " +
                $"Spawning at a random position in the area instead.");

        __state = block.SpawnPlacementType;
        block.SpawnPlacementType = eSpawnPlacementType.Default;
    }

    /// <summary>
    /// Finalizer rather than postfix so the datablock is restored even if the original throws.
    /// Level generation is single threaded and GetSpawnData is not reentrant, so the
    /// prefix/finalizer pairing is enough to keep the shared block honest.
    /// </summary>
    [HarmonyFinalizer]
    [HarmonyPatch(typeof(EnemyGroup), nameof(EnemyGroup.GetSpawnData),
        new[]
        {
            typeof(Vector3), typeof(Quaternion), typeof(AIG_CourseNode), typeof(EnemyGroupType),
            typeof(eEnemyGroupSpawnType), typeof(uint), typeof(float), typeof(IReplicator),
            typeof(SurvivalWave), typeof(uint), typeof(AgentMode)
        })]
    public static void Post_GetSpawnData(uint persistentGameDataID, eSpawnPlacementType? __state)
    {
        if (__state == null)
            return;

        var block = GameDataBlockBase<EnemyGroupDataBlock>.GetBlock(persistentGameDataID);

        if (block != null)
            block.SpawnPlacementType = __state.Value;
    }

    /// <summary>
    /// Preferring the requested index, the first marker far enough from every marker already
    /// used in this area. Null when the area has none, or none that clear the separation.
    /// </summary>
    private static int? ChooseAlign(
        Il2CppSystem.Collections.Generic.List<Transform>? aligns,
        int count,
        int requested,
        List<Vector3>? claimed)
    {
        if (aligns == null || count < 1 || claimed == null)
            return null;

        // eSpawnPlacementType cannot express anything past Align_5, so markers beyond that
        // are unreachable however many the geo authors.
        var limit = Math.Min(count, MaxAlignIndex + 1);

        // Requested marker first, then every other one in order
        var candidates = new List<int>(limit);

        if (requested < limit)
            candidates.Add(requested);

        candidates.AddRange(Enumerable.Range(0, limit).Where(i => i != requested));

        foreach (var index in candidates)
        {
            var align = aligns[index];

            if (align == null)
                continue;

            var position = align.position;

            if (claimed.Any(used => Vector3.Distance(used, position) < MinSeparation))
                continue;

            return index;
        }

        return null;
    }

    private static List<Vector3>? ClaimedIn(LG_Area? area)
    {
        if (area == null)
            return null;

        var key = area.GetInstanceID();

        if (!s_claimed.TryGetValue(key, out var claimed))
        {
            claimed = new List<Vector3>();
            s_claimed[key] = claimed;
        }

        return claimed;
    }

    /// <summary>
    /// Dumps a boss area's marker spacing once. Geo prefabs are the one thing we cannot read
    /// offline, and the game picks its fallback silently, so this is how we find out which
    /// tiles actually author usable separate markers.
    /// </summary>
    private static void LogLayout(
        LG_Area area,
        string areaName,
        Il2CppSystem.Collections.Generic.List<Transform>? aligns,
        int count)
    {
        if (!s_loggedLayout.Add(area.GetInstanceID()))
            return;

        if (aligns == null || count < 1)
        {
            Plugin.Logger.LogDebug($"[MissingSpawnAligns] area \"{areaName}\" has no spawn aligns");
            return;
        }

        var origin = aligns[0] == null ? Vector3.zero : aligns[0].position;
        var spacing = string.Join(", ", Enumerable
            .Range(0, count)
            .Select(i => aligns[i] == null
                ? $"{i}:<null>"
                : $"{i}:{Vector3.Distance(origin, aligns[i].position):F1}m"));

        Plugin.Logger.LogDebug(
            $"[MissingSpawnAligns] area \"{areaName}\" spawn aligns: {count}, " +
            $"distance from marker 0 -> {spacing}");
    }
}
