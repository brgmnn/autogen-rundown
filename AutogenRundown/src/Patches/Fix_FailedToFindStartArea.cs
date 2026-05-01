using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Managers;
using AutogenRundown.Utils;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Fix_FailedToFindStartArea
{
    public static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), uint> zoneFailures = new();

    /// <summary>
    /// Once per-zone failures exceed this, also bulk-reroll all MainLayer zones in the
    /// dimension to force broader topology variation across rebuilds.
    /// </summary>
    private const uint kBroadenThreshold = 50;

    /// <summary>
    /// Hard ceiling on retries. Beyond this we stop scheduling rerolls and emit a fatal log
    /// rather than spinning forever.
    /// </summary>
    private const uint kFatalThreshold = 500;

    /// <summary>
    /// Cycles StartExpansion through the four cardinal directions plus Random. Indexed by
    /// (rollCount - kBroadenThreshold) / kDirectionStride so each direction gets multiple
    /// rebuild attempts before rotating.
    /// </summary>
    private static readonly ZoneBuildExpansion[] kDirectionCycle =
    {
        ZoneBuildExpansion.Random,
        ZoneBuildExpansion.Forward,
        ZoneBuildExpansion.Backward,
        ZoneBuildExpansion.Left,
        ZoneBuildExpansion.Right,
    };

    private const uint kDirectionStride = 10;

    /// <summary>
    /// Tracks which (zone, expansion) pairs we've already logged the diagnostic for to avoid
    /// flooding the log with thousands of identical expander dumps.
    /// </summary>
    private static readonly HashSet<(eDimensionIndex, LG_LayerType, eLocalZoneIndex)> diagnosticsLogged = new();

    /// <summary>
    /// Set when the fatal threshold is hit on any zone. While set, every subsequent
    /// finalizer call in the same build pass short-circuits with __result=true so the
    /// engine drains its remaining job batches quickly without re-running the cascade
    /// (which would be wasted work — a rebuild is already pending).
    ///
    /// Cleared at the start of every Builder.Build call (fresh or rebuild) so the next
    /// attempt runs its cascade normally.
    /// </summary>
    public static bool fatalReached = false;

    public static void ResetDiagnostics() => diagnosticsLogged.Clear();

    /// <summary>
    /// Catches zones that fail to find valid start areas and triggers a proper subseed reroll.
    /// The reroll is persisted via ZoneSeedManager so it survives level rebuilds.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__exception"></param>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyFinalizer]
    public static void Post_LG_ZoneJob_CreateExpandFromData_Build(LG_ZoneJob_CreateExpandFromData __instance, ref bool __result, ref Exception? __exception)
    {
        if (__instance.m_mainStatus != LG_ZoneJob_CreateExpandFromData.MainStatus.FindStartArea ||
            __instance.m_subStatus != LG_ZoneJob_CreateExpandFromData.SubStatus.SelectArea ||
            __instance.m_scoredStartAreas.Count >= 1)
            return;

        // A prior zone in this same build pass already hit the fatal threshold and a
        // rebuild is pending. Short-circuit every subsequent failing job too so the engine
        // drains its job queues fast and reaches FactoryDone naturally — re-running the
        // cascade here would just be wasted work.
        if (fatalReached)
        {
            __result = true;
            return;
        }

        var zone = __instance.m_zone;
        var zoneKey = (zone.m_dimensionIndex, zone.m_layer.m_type, zone.LocalIndex);

        var rollCount = zoneFailures.GetValueOrDefault(zoneKey, 0u);
        zoneFailures[zoneKey] = rollCount + 1;

        Plugin.Logger.LogWarning($"Zone {zone.LocalIndex} failed to find start area (attempt {rollCount}). Triggering subseed reroll.");

        // One-shot diagnostic: dump source zone's expander state to confirm whether it's a
        // tile-level expander shortage (no free plugs/gates on any area) vs a directional
        // filter wipe.
        LogSourceExpanderDiagnostic(__instance, zone);

        if (rollCount > kFatalThreshold)
        {
            Plugin.Logger.LogError(
                $"Zone {zone.LocalIndex} in {zone.m_layer.m_type} (dim {zone.m_dimensionIndex}) " +
                $"exhausted {kFatalThreshold} reroll attempts — short-circuiting this job " +
                $"and marking for rebuild with the accumulated overrides");

            // Keep the accumulated subseed / expansion overrides — those are real progress.
            // But reset the per-zone failure counters so the next rebuild gets a fresh
            // budget; otherwise the counter is already past the fatal threshold and the
            // very first failure on the next pass would re-trigger this branch immediately.
            zoneFailures.Clear();
            diagnosticsLogged.Clear();

            // Set the gate so any other failing zones in this same build pass also
            // short-circuit instead of doing more cascade work.
            fatalReached = true;

            // Setting __result = true alone isn't enough — even with each zone job
            // reporting complete, the engine's later batches (FinalLogicLinking, culling
            // setup) iterate over the broken zones and never finish, so FactoryDone is
            // never reached and the rebuild never fires. Force LG_Factory.FactoryDone()
            // ourselves to short-circuit straight to the rebuild path.
            //
            // ShouldRebuild is set above via MarkForRebuild() so Patch_LG_Factory's
            // Prefix_FactoryDone will suppress the engine's own FactoryDone body (and
            // therefore the BuildDone / ElevatorShaftLanding handlers that would otherwise
            // NRE on broken state).
            __result = true;
            FactoryJobManager.MarkForRebuild();
            LG_Factory.Current.FactoryDone();
            return;
        }

        // Reroll the failing zone itself
        ZoneSeedManager.Reroll_SubSeed(zone);

        // Walk same-layer ancestors up to (and including) Zone_0
        if (zone.LocalIndex != eLocalZoneIndex.Zone_0 && __instance.m_zoneData != null)
        {
            WalkAncestors(
                __instance.m_zoneData.BuildFromLocalIndex,
                zone.DimensionIndex,
                zone.Layer.m_type);
        }

        // Cross-layer hop: when the failing zone is in a non-Main layer, the same-layer walk
        // terminates at Zone_0 of that layer but the actual source zone lives in another layer
        // (Main, or Extreme for chained Overload). Without rerolling that source, its geomorph
        // never changes and the topology dead-end persists indefinitely.
        if (zone.Layer.m_type != LG_LayerType.MainLayer && zone.Layer.m_buildData != null)
        {
            var srcLayer = zone.Layer.m_buildData.m_buildFromLayer;
            var srcIndex = zone.Layer.m_buildData.m_buildFromZone;

            ZoneSeedManager.Reroll_SubSeed(srcIndex, zone.DimensionIndex, srcLayer);
            Plugin.Logger.LogDebug($"Cross-layer cascade: also rerolling source {srcIndex} in {srcLayer}");

            if (srcIndex != eLocalZoneIndex.Zone_0)
                WalkAncestors(srcIndex, zone.DimensionIndex, srcLayer);
        }

        // Escalation: subseed variation alone may not be enough to escape a topology dead-end,
        // so after a threshold we bulk-reroll every MainLayer zone in the dimension to force
        // broader geometric variation across the next rebuild.
        if (rollCount > kBroadenThreshold)
        {
            Plugin.Logger.LogWarning($"Broadening reroll to all MainLayer zones (attempt {rollCount})");

            var floor = Builder.CurrentFloor;
            if (floor != null)
            {
                foreach (var z in floor.allZones)
                {
                    if (z?.Layer?.m_type == LG_LayerType.MainLayer && z.DimensionIndex == zone.DimensionIndex)
                        ZoneSeedManager.Reroll_SubSeed(z);
                }
            }

            // Direction cycling: the directional ScoreData filter inside ScoreAreas can wipe
            // every fringe candidate before FixDeadEndZoneExpansion's plug-bias gets to run.
            // Cycle the StartExpansion of the failing zone and the same-layer Zone_0 (the
            // immediate ancestor whose start-area pick is the actual failure point for
            // secondary bulkheads) through the cardinals + Random so we eventually try every
            // side of the source tile.
            var cycleIndex = (int)((rollCount - kBroadenThreshold) / kDirectionStride) % kDirectionCycle.Length;
            var nextDirection = kDirectionCycle[cycleIndex];

            ZoneSeedManager.Override_StartExpansion(zone, nextDirection);
            Plugin.Logger.LogWarning(
                $"Direction cycling: forcing StartExpansion={nextDirection} on Zone_{zone.LocalIndex} " +
                $"in {zone.Layer.m_type} (cycle {cycleIndex})");

            if (zone.LocalIndex != eLocalZoneIndex.Zone_0)
            {
                ZoneSeedManager.Override_StartExpansion(
                    eLocalZoneIndex.Zone_0,
                    zone.DimensionIndex,
                    zone.Layer.m_type,
                    nextDirection);
                Plugin.Logger.LogDebug(
                    $"Direction cycling: forcing StartExpansion={nextDirection} on Zone_0 in {zone.Layer.m_type}");
            }
        }
    }

    /// <summary>
    /// Dumps the source (build-from) zone's areas and expander link state on the first
    /// failure for a given (dimension, layer, zone) key. Used to confirm whether the failure
    /// is a tile-level expander shortage (every expander already linked) vs a directional
    /// scoring filter wipe.
    /// </summary>
    private static void LogSourceExpanderDiagnostic(LG_ZoneJob_CreateExpandFromData job, LG_Zone zone)
    {
        var key = (zone.m_dimensionIndex, zone.m_layer.m_type, zone.LocalIndex);
        if (!diagnosticsLogged.Add(key))
            return;

        var src = job.m_buildFromZone;

        if (src == null)
        {
            Plugin.Logger.LogInfo($"[ExpanderDiag] {zone.LocalIndex} in {zone.m_layer.m_type}: m_buildFromZone is null");
            return;
        }

        Plugin.Logger.LogInfo(
            $"[ExpanderDiag] {zone.LocalIndex} in {zone.m_layer.m_type} failed building from " +
            $"{src.LocalIndex} ({src.m_layer?.m_type}); src has {src.m_areas?.Count ?? 0} area(s); " +
            $"requested StartExpansion={job.m_zoneData?.StartExpansion}, " +
            $"currentBuildDirection={job.m_currentBuildDirection}");

        if (src.m_areas == null)
            return;

        for (var i = 0; i < src.m_areas.Count; i++)
        {
            var area = src.m_areas[i];
            if (area?.m_zoneExpanders == null)
                continue;

            var freeCount = 0;
            var totalCount = area.m_zoneExpanders.Count;

            for (var j = 0; j < totalCount; j++)
            {
                var exp = area.m_zoneExpanders[j];
                if (exp == null)
                    continue;

                var linksToZone = exp.m_linksTo?.m_zone;
                var isFree = linksToZone == null;
                if (isFree)
                    freeCount++;

                Plugin.Logger.LogInfo(
                    $"  area[{i}] expander[{j}] type={exp.ExpanderType} " +
                    (isFree
                        ? "FREE"
                        : $"linkedTo={linksToZone.LocalIndex} ({linksToZone.m_layer?.m_type})"));
            }

            Plugin.Logger.LogInfo($"  area[{i}] free={freeCount}/{totalCount}");
        }
    }

    private static void WalkAncestors(eLocalZoneIndex startIndex, eDimensionIndex dimension, LG_LayerType layer)
    {
        var currentIndex = startIndex;

        while (true)
        {
            ZoneSeedManager.Reroll_SubSeed(currentIndex, dimension, layer);
            Plugin.Logger.LogDebug($"Also rerolling ancestor zone {currentIndex} in {layer}");

            // Zone_0 has no same-layer parent, stop here
            if (currentIndex == eLocalZoneIndex.Zone_0)
                break;

            var currentZone = Game.FindZone(currentIndex, dimension, layer);
            if (currentZone?.m_settings?.m_zoneData == null)
                break;

            currentIndex = currentZone.m_settings.m_zoneData.BuildFromLocalIndex;
        }
    }
}
