using AK;
using AutogenRundown.Utils;
using Enemies;
using FX_EffectSystem;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace AutogenRundown.Patches.Spitters;

/// <summary>
/// Plays the Flyer's death effect — the FXC_FlyerDeath gib burst plus the
/// FLYER_DIE squelch — at a spitter's position when it is removed, so the
/// deactivation reads as the spitter being destroyed. Called once per live
/// death from SpitterKillManager.FinalizeSpitter; silent recall/reconcile
/// removals never reach here.
///
/// The Flyer's death visual is a standalone pooled FX prefab played at a
/// world position (old decompile ES_HitreactFlyer.cs:38,122); it has no
/// dependency on the flyer's mesh, so it works anywhere. In the current game
/// build the prefab reference lives as a serialized field on the flyer
/// model's FlyerAnimationController (m_deathFX), and the flyer's own pool is
/// only created when a flyer spawns — so in flyer-less levels the prefab is
/// read off the pre-built enemy prefab instead: EnemyPrefabManager generates
/// prefabs for EVERY EnemyDataBlock when enemy assets load, regardless of
/// level contents, and never destroys them (EnemyPrefabManager.cs:40-73).
/// FX pools are keyed by prefab name (FX_Manager.cs:194-211), so the pool
/// created here is exactly the one a later-spawning flyer would reuse.
///
/// Failure containment mirrors SpitterVisuals: this is cosmetic, so any
/// unexpected exception trips a local one-shot _broken flag (no more bursts)
/// rather than SpitterKillManager.Break. ObjectCollectedException is the
/// known transient IL2CPP GC race and only skips the current burst.
/// </summary>
public static class SpitterDeathFx
{
    /// <summary>EnemyDataBlock persistentID of the Flyer, whose death FX and
    /// sound this borrows.</summary>
    private const uint FlyerEnemyId = 42;

    /// <summary>Pool lookups are keyed by prefab name.</summary>
    private const string DeathFxPrefabName = "FXC_FlyerDeath";

    /// <summary>One-shot cosmetic kill-switch (never affects the kill flow).</summary>
    private static bool _broken;

    private static bool _warnedNoPool;

    /// <summary>Cached pool; Unity-null when destroyed, then re-resolved.</summary>
    private static FX_Pool? _pool;

    /// <summary>
    /// Plays the death burst and sound at <paramref name="position"/> on this
    /// peer. The sound posts even when the visual pool cannot be resolved.
    /// </summary>
    public static void PlayAt(Vector3 position)
    {
        if (_broken)
            return;

        try
        {
            SoundPlayer.Play(EVENTS.FLYER_DIE, position);

            var pool = ResolvePool();
            if (pool == null)
            {
                WarnNoPoolOnce();
                return;
            }

            var effect = pool.AquireEffect();
            if (effect == null)
                return;

            // Vanilla flyer death call shape (ES_HitreactFlyer.cs:122).
            effect.Play(null, position, Quaternion.identity);
        }
        catch (ObjectCollectedException)
        {
            // Transient IL2CPP GC race (see SpitterVisuals) — drop this burst
            // and re-resolve on the next death.
            _pool = null;
        }
        catch (Exception ex)
        {
            _broken = true;
            Plugin.Logger.LogWarning($"[SpitterKill] Spitter death FX disabled: {ex}");
        }
    }

    private static FX_Pool? ResolvePool()
    {
        if (_pool != null)
            return _pool;

        // A flyer already spawned this session — its own pool exists.
        _pool = FX_Manager.GetPreloadedEffectPool(DeathFxPrefabName);
        if (_pool != null)
            return _pool;

        var prefab = GetDeathFxPrefab();

        // Old-build fallback, where the FX prefab lived in Resources.
        if (prefab == null)
            prefab = Resources.Load<GameObject>(DeathFxPrefabName);

        if (prefab != null)
            _pool = FX_Manager.GetEffectPool(prefab);

        return _pool;
    }

    /// <summary>Reads the serialized death-FX prefab off the flyer's
    /// pre-built enemy prefab (built for every EnemyDataBlock at asset load,
    /// flyers in the level or not).</summary>
    private static GameObject? GetDeathFxPrefab()
    {
        var flyerPrefab = EnemyPrefabManager.GetEnemyPrefab(FlyerEnemyId);
        if (flyerPrefab == null)
            return null;

        var controller = flyerPrefab.GetComponentInChildren<FlyerAnimationController>(true);
        if (controller == null)
            return null;

        return controller.m_deathFX;
    }

    private static void WarnNoPoolOnce()
    {
        if (_warnedNoPool)
            return;

        _warnedNoPool = true;
        Plugin.Logger.LogWarning(
            "[SpitterKill] Could not resolve the flyer death FX pool " +
            "(FXC_FlyerDeath); spitter removals play sound only");
    }
}
