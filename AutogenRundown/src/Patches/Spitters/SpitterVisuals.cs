using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace AutogenRundown.Patches.Spitters;

/// <summary>
/// Damage-state visuals for killable spitters: the body glow shifts
/// green → orange → red as health drops, pops of a damaged spitter tint by
/// its missing health, and the death pop renders fully red. All state and
/// call sites are driven by SpitterKillManager; this class owns only the
/// color math, the propBlock writes and the FX tint/restore registry.
///
/// Body glow: vanilla writes _GlowColor in exactly two places — once at
/// AssignCourseNode (s_startGlowColor, decompile InfectionSpitter.cs:178) and
/// every frame during a pop wind-up (s_glowColor * m_explodeProgression,
/// line 508, whose completion write lands back on s_startGlowColor). The
/// retraction writes reuse the same MaterialPropertyBlock and therefore
/// preserve whatever color we set. So a tint persists on its own; wind-up
/// ramps are overridden per-frame from the Update postfix (which runs after
/// vanilla), and the tint is re-applied once when a pop completes.
///
/// FX tint: FX_InfectionSpit has no color API — it is a pooled GameObject
/// tree of ParticleSystems. We set each system's MainModule.startColor before
/// the burst simulates (the pop plays inside the same vanilla Update our
/// postfix follows; particle simulation happens after script updates, so the
/// tint lands before anything renders). This REPLACES the prefab's original
/// startColor gradient with a flat color — best effort; tune the color
/// constants below in-game.
///
/// Restore is mandatory and deferred: FX pools live for the whole game
/// session (FX_Manager is a GlobalManager; FX_Pool.OnLevelCleanup only
/// returns instances, never destroys them) and the FX_InfectionSpit pool is
/// ALSO used by infectious enemy projectiles (decompile ProjectileBase.cs:59,
/// 95) — an un-restored red instance would splash red from projectile hits
/// and leak across levels. Death-pop tints are restored when the spitter
/// finalizes (bounded by SpitterKillManager's 5s deadline); damaged-pop tints
/// restore on a ~3s timer (past the burst lifetime), ticked from the Update
/// postfix — the spitter that just popped keeps Updating for ≥30s
/// (m_freezeTime), guaranteeing a driver while restores are pending.
///
/// Failure containment: a tint failure must never break the kill flow. Tint
/// paths trip a local one-shot _visualsBroken flag (vanilla colors from then
/// on) instead of SpitterKillManager.Break; restore paths keep running even
/// when broken so a half-applied tint still gets cleaned up.
///
/// IL2CPP GC-race hardening: interop's MinMaxGradient and MainModule are
/// boxed IL2CPP classes, and IL2CPP's Boehm GC does not scan the .NET stack —
/// a freshly allocated boxed object can be collected before Il2CppInterop
/// attaches its GCHandle (ObjectCollectedException), most likely exactly at
/// pop frames (peak GC pressure). Therefore: startColor writes go through ONE
/// long-lived shared MinMaxGradient (mutated per use; set_startColor copies
/// by value into the native module), originals are captured BY VALUE (never
/// as a boxed wrapper reference), the boxed MainModule is cached per system,
/// and the operations that still allocate IL2CPP-side (first capture,
/// GetComponentsInChildren, the particle array) retry on
/// ObjectCollectedException and skip — never trip _visualsBroken — if the
/// race persists.
///
/// Out of scope: the on-visor splatter (ScreenLiquid "spitterJizz") is a
/// global ScriptableObject shared with infectious projectiles — it stays
/// vanilla green.
/// </summary>
public static class SpitterVisuals
{
    #region Tunables

    /// <summary>Seconds after a damaged (non-death) pop before its FX tint is
    /// restored; must exceed the burst's visual lifetime.</summary>
    private const float FxRestoreSeconds = 3f;

    /// <summary>Steady-glow scale relative to the ramp-scale base colors —
    /// matches vanilla, where s_startGlowColor = s_glowColor * 0.6.</summary>
    private const float SteadyGlowScale = 0.6f;

    // Body glow base colors (ramp scale, matching vanilla's premultiplied
    // s_glowColor intensity). Green is read from the live game static so a
    // game rebalance can't desync us; orange/red are the feature's tunables.
    private static readonly Color OrangeGlowBase = new Color(1f, 0.45f, 0f) * 0.5f;
    private static readonly Color RedGlowBase = new Color(1f, 0.06f, 0.04f) * 0.5f;

    // Pop particle colors (full-brightness flat startColor replacements).
    // GreenPop approximates the vanilla infection-goo hue for barely-damaged
    // pops, since tinting replaces the prefab's original gradient.
    private static readonly Color32 GreenPopColor = new(60, 255, 200, 255);
    private static readonly Color32 OrangePopColor = new(255, 150, 30, 255);
    private static readonly Color32 RedPopColor = new(255, 24, 16, 255);

    #endregion

    /// <summary>The death pop's particle color.</summary>
    public static Color32 DeathPopColor => RedPopColor;

    /// <summary>Steady body glow of a spitter that died and is awaiting
    /// finalization (applied after the death pop resets the vanilla color).</summary>
    public static Color DeathSteadyGlow => RedGlowBase * SteadyGlowScale;

    /// <summary>One-shot local kill-switch for the tint paths only; restore
    /// paths ignore it. The kill feature is unaffected.</summary>
    private static bool _visualsBroken;

    #region FX tint registry

    /// <summary>Attempts for operations that can hit the transient IL2CPP
    /// GC race (ObjectCollectedException) before skipping.</summary>
    private const int GcRaceAttempts = 3;

    /// <summary>
    /// Original startColor of a tinted ParticleSystem, captured BY VALUE — a
    /// stored boxed MinMaxGradient wrapper could itself be a collected object
    /// that throws at restore time. The Gradient references are natively
    /// rooted by the particle system's own configuration. The boxed
    /// MainModule is cached because every ps.main call allocates IL2CPP-side;
    /// it stays valid as long as the ParticleSystem lives (it only wraps the
    /// system pointer).
    /// </summary>
    private readonly struct OriginalStartColor
    {
        public readonly ParticleSystem Ps;
        public readonly ParticleSystem.MainModule Main;
        public readonly ParticleSystemGradientMode Mode;
        public readonly Color ColorMin;
        public readonly Color ColorMax;
        public readonly Gradient? GradientMin;
        public readonly Gradient? GradientMax;

        public OriginalStartColor(
            ParticleSystem ps, ParticleSystem.MainModule main, ParticleSystem.MinMaxGradient captured)
        {
            Ps = ps;
            Main = main;
            Mode = captured.m_Mode;
            ColorMin = captured.m_ColorMin;
            ColorMax = captured.m_ColorMax;
            GradientMin = captured.m_GradientMin;
            GradientMax = captured.m_GradientMax;
        }
    }

    /// <summary>The single long-lived boxed gradient every startColor write
    /// goes through (created once, then rooted by this static — immune to the
    /// allocation race). set_startColor copies it by value into the native
    /// module, so mutation + reuse is safe.</summary>
    private static ParticleSystem.MinMaxGradient? _sharedGradient;

    /// <summary>Original startColor per tinted ParticleSystem, keyed by
    /// GetInstanceID. Captured only when absent so a re-tint can never record
    /// our own color as the "original".</summary>
    private static readonly Dictionary<int, OriginalStartColor> _tintedSystems = new();

    /// <summary>Restore time per tinted system. float.MaxValue = anchored to a
    /// spitter's finalization (death pops) rather than a timer.</summary>
    private static readonly Dictionary<int, float> _pendingRestores = new();

    /// <summary>Finalize-anchored tinted systems per dying spitter.</summary>
    private static readonly Dictionary<ushort, List<int>> _tintedBySpitter = new();

    #endregion

    #region Color math

    /// <summary>
    /// Ramp-scale body glow color for a health fraction: green → orange over
    /// the first half of lost health, orange → red over the second.
    /// </summary>
    private static Color GetBaseGlow(float healthRel)
    {
        healthRel = Mathf.Clamp01(healthRel);

        return healthRel >= 0.5f
            ? Color.Lerp(InfectionSpitter.s_glowColor, OrangeGlowBase, (1f - healthRel) * 2f)
            : Color.Lerp(OrangeGlowBase, RedGlowBase, (0.5f - healthRel) * 2f);
    }

    /// <summary>Particle color for a damaged (non-death) pop, same two-segment
    /// progression as the body glow.</summary>
    public static Color32 GetPopColor(float healthRel)
    {
        healthRel = Mathf.Clamp01(healthRel);

        return healthRel >= 0.5f
            ? Color32.Lerp(GreenPopColor, OrangePopColor, (1f - healthRel) * 2f)
            : Color32.Lerp(OrangePopColor, RedPopColor, (0.5f - healthRel) * 2f);
    }

    #endregion

    #region Body glow

    /// <summary>
    /// Writes the glow color exactly the way vanilla does (decompile
    /// InfectionSpitter.cs:507-509). s_glowColorID is valid: the first
    /// AssignCourseNode necessarily ran before any spitter could be shot.
    /// </summary>
    public static void ApplyBodyGlow(InfectionSpitter spitter, Color color)
    {
        if (_visualsBroken)
            return;

        try
        {
            if (spitter == null || spitter.m_propBlock == null || spitter.m_renderer == null)
                return;

            spitter.m_propBlock.SetColor(InfectionSpitter.s_glowColorID, color);
            spitter.m_renderer.SetPropertyBlock(spitter.m_propBlock, 0);
        }
        catch (ObjectCollectedException)
        {
            // Transient IL2CPP GC race — skip this write (the next glow
            // application self-corrects); never a reason to disable visuals.
        }
        catch (Exception ex)
        {
            BreakVisuals(ex);
        }
    }

    /// <summary>Steady damage tint (works on disabled/Frozen spitters too —
    /// it's a direct propBlock write).</summary>
    public static void ApplyDamageTint(InfectionSpitter spitter, float healthRel)
        => ApplyBodyGlow(spitter, GetBaseGlow(healthRel) * SteadyGlowScale);

    /// <summary>Per-frame wind-up ramp override for a damaged spitter's normal
    /// pop (replaces vanilla's green s_glowColor * progression write).</summary>
    public static void ApplyDamageRamp(InfectionSpitter spitter, float healthRel)
    {
        if (spitter == null)
            return;

        ApplyBodyGlow(spitter, GetBaseGlow(healthRel) * spitter.m_explodeProgression);
    }

    /// <summary>Per-frame wind-up ramp override for the death pop.</summary>
    public static void ApplyDeathRamp(InfectionSpitter spitter)
    {
        if (spitter == null)
            return;

        ApplyBodyGlow(spitter, RedGlowBase * spitter.m_explodeProgression);
    }

    /// <summary>Back to the vanilla resting color (revive path).</summary>
    public static void ResetBodyGlow(InfectionSpitter spitter)
        => ApplyBodyGlow(spitter, InfectionSpitter.s_startGlowColor);

    #endregion

    #region FX tint / restore

    /// <summary>
    /// Tints the spitter's just-played pop effect (spitter.m_fx). Call on the
    /// frame the pop fires (burst not yet simulated → emits tinted), or with
    /// recolorLiveParticles for an already-airborne burst (KillSpitter case
    /// (b) adoption). untilFinalize anchors the restore to the spitter's
    /// finalization; otherwise a ~3s timer restores it.
    /// </summary>
    public static void TintPop(
        ushort index, InfectionSpitter spitter, Color32 color,
        bool recolorLiveParticles, bool untilFinalize)
    {
        if (_visualsBroken)
            return;

        for (var attempt = 1; attempt <= GcRaceAttempts; attempt++)
        {
            try
            {
                TintPopCore(index, spitter, color, recolorLiveParticles, untilFinalize);
                return;
            }
            catch (ObjectCollectedException)
            {
                // Transient IL2CPP GC race — retry. Partial re-runs are safe:
                // captures are if-absent and the registry adds are guarded.
            }
            catch (Exception ex)
            {
                BreakVisuals(ex);
                return;
            }
        }

        Plugin.Logger.LogDebug("[SpitterKill] FX tint skipped (IL2CPP GC race persisted)");
    }

    private static void TintPopCore(
        ushort index, InfectionSpitter spitter, Color32 color,
        bool recolorLiveParticles, bool untilFinalize)
    {
        var fx = spitter?.m_fx;

        // In-pool means the instance was already returned (deactivation
        // cleared its particles) — nothing visible to tint.
        if (fx == null || fx.m_inPool)
            return;

        var systems = fx.GetComponentsInChildren<ParticleSystem>(true);
        if (systems == null)
            return;

        foreach (var ps in systems)
        {
            if (ps == null)
                continue;

            var id = ps.GetInstanceID();

            if (!_tintedSystems.TryGetValue(id, out var entry))
            {
                // First sighting: one ps.main + one startColor read; the
                // boxed wrapper is copied out by value and never stored.
                var main = ps.main;
                entry = new OriginalStartColor(ps, main, main.startColor);
                _tintedSystems[id] = entry;
            }

            WriteStartColor(entry.Main, (Color)color);

            if (recolorLiveParticles)
                RecolorLiveParticles(ps, color);

            if (untilFinalize)
            {
                _pendingRestores[id] = float.MaxValue;

                if (!_tintedBySpitter.TryGetValue(index, out var ids))
                    _tintedBySpitter[index] = ids = new List<int>();
                if (!ids.Contains(id))
                    ids.Add(id);
            }
            else
            {
                _pendingRestores[id] = Clock.Time + FxRestoreSeconds;
            }
        }
    }

    /// <summary>Flat-color startColor write through the shared gradient.</summary>
    private static void WriteStartColor(ParticleSystem.MainModule main, Color color)
        => WriteStartColor(main, ParticleSystemGradientMode.Color, color, color, null, null);

    /// <summary>
    /// Writes a startColor by mutating the single shared MinMaxGradient and
    /// assigning it (copied by value into the native module) — no IL2CPP
    /// allocation except the one-time shared-instance creation, which the
    /// callers' retry loops cover.
    /// </summary>
    private static void WriteStartColor(
        ParticleSystem.MainModule main, ParticleSystemGradientMode mode,
        Color colorMin, Color colorMax, Gradient? gradientMin, Gradient? gradientMax)
    {
        var g = _sharedGradient ??= new ParticleSystem.MinMaxGradient(Color.white);

        g.m_Mode = mode;
        g.m_GradientMin = gradientMin;
        g.m_GradientMax = gradientMax;
        g.m_ColorMin = colorMin;
        g.m_ColorMax = colorMax;

        main.startColor = g;
    }

    /// <summary>Overwrites already-emitted particles' startColor. Per-particle
    /// alpha can't be preserved — the interop Particle.startColor is
    /// write-only (IL2CPP stripped the unused getter) — so full alpha is set;
    /// fade-out comes from colorOverLifetime, which multiplies on top. One
    /// Il2Cpp array alloc per call — pops are rare.</summary>
    private static void RecolorLiveParticles(ParticleSystem ps, Color32 color)
    {
        var count = ps.particleCount;
        if (count <= 0)
            return;

        var particles = new Il2CppStructArray<ParticleSystem.Particle>(count);
        var n = ps.GetParticles(particles);

        for (var i = 0; i < n; i++)
        {
            // The Il2Cpp array indexer returns a copy — must write back.
            var p = particles[i];
            p.startColor = new Color32(color.r, color.g, color.b, 255);
            particles[i] = p;
        }

        ps.SetParticles(particles, n);
    }

    /// <summary>
    /// Restores timer-based tints that are past due. Called from the
    /// InfectionSpitter.Update postfix (any spitter's frame drives it); fast
    /// path when nothing is pending. Runs even when _visualsBroken.
    /// </summary>
    public static void TickRestores()
    {
        if (_pendingRestores.Count == 0)
            return;

        List<int>? due = null;

        foreach (var (id, restoreAt) in _pendingRestores)
        {
            if (restoreAt != float.MaxValue && Clock.Time >= restoreAt)
                (due ??= new List<int>()).Add(id);
        }

        if (due == null)
            return;

        foreach (var id in due)
            RestoreSystem(id);
    }

    /// <summary>Restores a dying spitter's finalize-anchored tints
    /// (FinalizeSpitter / ReviveSpitter).</summary>
    public static void RestoreForSpitter(ushort index)
    {
        if (!_tintedBySpitter.TryGetValue(index, out var ids))
            return;

        foreach (var id in ids)
            RestoreSystem(id);

        _tintedBySpitter.Remove(index);
    }

    /// <summary>Restores everything and clears the registry (level cleanup,
    /// rebuild, permanent Break). Session-lifetime pools make this mandatory.</summary>
    public static void RestoreAll()
    {
        foreach (var id in _tintedSystems.Keys.ToList())
            RestoreSystem(id);

        _tintedSystems.Clear();
        _pendingRestores.Clear();
        _tintedBySpitter.Clear();
    }

    private static void RestoreSystem(int id)
    {
        if (_tintedSystems.TryGetValue(id, out var entry))
        {
            for (var attempt = 1; attempt <= GcRaceAttempts; attempt++)
            {
                try
                {
                    // Unity-alive check: destroyed systems compare equal to null.
                    if (entry.Ps != null)
                    {
                        WriteStartColor(entry.Main, entry.Mode,
                            entry.ColorMin, entry.ColorMax, entry.GradientMin, entry.GradientMax);
                    }

                    break;
                }
                catch (ObjectCollectedException)
                {
                    // Transient IL2CPP GC race — retry.
                    if (attempt == GcRaceAttempts)
                        Plugin.Logger.LogDebug("[SpitterKill] FX tint restore skipped (IL2CPP GC race persisted)");
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogWarning($"[SpitterKill] Failed to restore FX tint: {ex.Message}");
                    break;
                }
            }
        }

        _tintedSystems.Remove(id);
        _pendingRestores.Remove(id);
    }

    #endregion

    private static void BreakVisuals(Exception ex)
    {
        if (_visualsBroken)
            return;

        _visualsBroken = true;
        Plugin.Logger.LogWarning(
            $"[SpitterKill] Damage-state visuals failed, disabling visuals locally (kill feature unaffected): {ex}");
    }
}
