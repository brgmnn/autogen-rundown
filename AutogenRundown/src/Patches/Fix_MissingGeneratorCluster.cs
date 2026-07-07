using AutogenRundown.Managers;
using GameData;
using LevelGeneration;

namespace AutogenRundown.Patches;

/// <summary>
/// What this solves:
///
/// Generator clusters are enqueued with allowFunctionFallback=false. When marker spawner
/// selection fails (zero-weight FunctionPotential groups, or the selected area's spawners were
/// already consumed), LG_PopulateFunctionMarkersInZoneJob.TriggerFunctionBuilder drops the item
/// silently -- no log output -- and the level loads without the cluster, making
/// CentralGeneratorCluster objectives uncompletable.
///
/// The spawner roll is driven by HashRandom over (BuildSeed, zone.SeedID, zone.m_subSeed,
/// zone.m_markerSubSeed), so re-rolling per-zone seeds and rebuilding can rescue the level.
/// (Which *area* the cluster lands in is driven by the global build seed instead; see
/// Patch_CentralGeneratorCluster for that stage's fix.)
///
/// How we fix it: validate at FactoryDone that every zone with GeneratorClustersInZone > 0
/// actually spawned its clusters. If one didn't, bump that zone's MarkerSubSeed (+1 per attempt),
/// every few attempts also bump the zone's SubSeed (+137, reshapes areas and can escape
/// spawner-less layouts), and trigger a rebuild via FactoryJobManager. Gives up after
/// MaxAttemptsPerZone so a pathological zone can't rebuild-loop forever.
/// </summary>
public static class Fix_MissingGeneratorCluster
{
    private const int MaxAttemptsPerZone = 12;

    /// <summary>
    /// Every Nth attempt also bumps the zone's SubSeed, for cases where marker subseed re-rolls
    /// alone never find a spawner.
    /// </summary>
    private const int SubSeedEscalationInterval = 3;

    private static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), int> Attempts = new();

    private static readonly HashSet<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index)> GaveUp = new();

    public static void Setup()
    {
        FactoryJobManager.OnDoneValidate += Validate;
    }

    /// <summary>
    /// Resets re-roll tracking. Called by FactoryJobManager at the start of a fresh build and
    /// when all validators pass.
    /// </summary>
    public static void Reset()
    {
        Attempts.Clear();
        GaveUp.Clear();
    }

    private static bool Validate()
    {
        try
        {
            var floor = Builder.CurrentFloor;

            if (floor == null)
                return true;

            var expected = new List<LG_Zone>();

            foreach (var zone in floor.allZones)
                if (zone.m_settings?.m_zoneData?.GeneratorClustersInZone > 0)
                    expected.Add(zone);

            if (expected.Count == 0)
                return true;

            var spawned = new Dictionary<(eDimensionIndex, LG_LayerType, eLocalZoneIndex), int>();

            foreach (var cluster in UnityEngine.Object.FindObjectsOfType<LG_PowerGeneratorCluster>())
            {
                var zone = cluster.SpawnNode?.m_zone;

                if (zone == null)
                    continue;

                var key = (zone.DimensionIndex, zone.Layer.m_type, zone.LocalIndex);

                spawned[key] = spawned.GetValueOrDefault(key, 0) + 1;
            }

            var success = true;

            foreach (var zone in expected)
            {
                var key = (zone.DimensionIndex, zone.Layer.m_type, zone.LocalIndex);

                if (spawned.GetValueOrDefault(key, 0) >= zone.m_settings.m_zoneData.GeneratorClustersInZone)
                    continue;

                if (Attempts.GetValueOrDefault(key, 0) >= MaxAttemptsPerZone)
                {
                    if (GaveUp.Add(key))
                        Plugin.Logger.LogError(
                            $"[GeneratorCluster] Zone {key} still has no generator cluster after " +
                            $"{MaxAttemptsPerZone} re-rolls. Letting the level load without it.");

                    continue;
                }

                var attempt = Attempts.GetValueOrDefault(key, 0) + 1;
                Attempts[key] = attempt;

                var markerSubSeed = (ZoneSeedManager.MarkerSubSeeds.TryGetValue(key, out var marker)
                    ? marker
                    : zone.m_markerSubSeed) + 1;
                ZoneSeedManager.MarkerSubSeeds[key] = markerSubSeed;

                if (attempt % SubSeedEscalationInterval == 0)
                {
                    var subSeed = (ZoneSeedManager.SubSeeds.TryGetValue(key, out var sub)
                        ? sub
                        : zone.m_subSeed) + 137;
                    ZoneSeedManager.SubSeeds[key] = subSeed;

                    Plugin.Logger.LogDebug($"[GeneratorCluster] Escalating: SubSeed={subSeed} for {key}");
                }

                Plugin.Logger.LogDebug(
                    $"[GeneratorCluster] No cluster spawned in {key}. Rebuilding with " +
                    $"MarkerSubSeed={markerSubSeed} (attempt {attempt}/{MaxAttemptsPerZone})");

                success = false;
            }

            return success;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"[GeneratorCluster] Validator error: {ex}");
            return true;
        }
    }
}
