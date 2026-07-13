using AmorLib.Networking.StateReplicators;
using AutogenRundown.Events;
using GTFO.API;
using GTFO.API.Resources;
using SNetwork;

namespace AutogenRundown.Patches.Spitters;

/// <summary>
/// Makes InfectionSpitters killable with bullets. Owns all killable-spitter
/// state; the Harmony taps/guards live in Patch_SpitterDamage, the
/// dead-spitter ManagerUpdate gate lives in Fix_SpitterBotAggro, and the
/// damage-state visuals (glow ramp, tinted pops, red death pop) live in
/// SpitterVisuals, driven from this manager's kill flow plus a host→all
/// health-fraction broadcast (HealthEventName).
///
/// Vanilla spitters have no health: InfectionSpitterDamage.BulletDamage funnels
/// into InfectionSpitter.OnIncomingDamage which just pops the spitter (5s
/// cooldown) — it can never die (decompile: InfectionSpitter.cs:337-347,
/// InfectionSpitterDamage.cs:25-36). This manager adds a host-authoritative
/// health pool per spitter. Bullets report damage to the host; when a pool
/// hits zero the host marks the spitter dead in a replicated bitmask, every
/// peer plays one final explosion, and the spitter is permanently deactivated.
///
/// Network design:
/// - Identity: InfectionSpitter.m_spitterIndex, assigned in registration order
///   during deterministic seeded level-gen — identical on every peer. Dead
///   spitters are deactivated but NEVER removed from s_allSpitters (removing
///   an entry would shift every later index).
/// - Damage reports: GTFO-API NetworkAPI event sent by the shooting client
///   directly to SNet.Master on a reliable channel. Host shots skip the
///   network. Sentry bullets ride the same BulletDamage path and only deal
///   damage on the host, so they accumulate directly too.
/// - Death state: SHARD_COUNT AmorLib StateReplicators (fixed IDs, always
///   created at OnBuildDone on every peer — no count negotiation), each
///   holding a dead-bitmask shard. AmorLib handles host broadcast, drop-in
///   recall and the post-recall client sync handshake, and its sends resolve
///   through dynamic SNet send groups — correct in >4-player LobbyExpansion
///   lobbies. State diffs are applied in both directions: 0→1 kills, 1→0
///   revives (checkpoint recall can restore a pre-death snapshot).
///
/// Config semantics (host-authoritative): only the HOST's KillableSpitters /
/// SpitterHealth values matter. Damage accumulation and the death decision run
/// exclusively on the host and are gated by the host's config. Clients always
/// send damage reports (the host discards them when disabled), always apply
/// replicated death states regardless of their local toggle, and the
/// dead-guards always apply so dead spitters stay dead everywhere.
///
/// Death sequence ("pops on every hit + death pop"): Patch_SpitterDamage
/// removes the vanilla 5s damage-pop cooldown, so sustained fire pops the
/// spitter each wind-up cycle until the pool empties, and the killing blow
/// always produces exactly one final explosion. KillSpitter adopts an
/// in-flight or just-finished pop where one exists, otherwise triggers
/// DoExplode directly (DoExplode only guards on m_isExploding).
/// The flyer's death burst + sound (SpitterDeathFx) land together with the
/// death pop, locally on every peer, and the model is hidden on that same
/// frame so the explosion and the disappearance coincide. Finalization
/// (deactivate + sound cleanup + FX tint restore) follows a grace period
/// later on the already-invisible object, driven by the
/// InfectionSpitter.Update postfix, with a hard deadline fallback driven from
/// the ManagerUpdate gate.
///
/// Failure mode: any unexpected exception permanently flips _broken and the
/// feature degrades to pure vanilla behavior locally (guards return "run
/// original", reports stop). Host migration is out of scope (the mod has no
/// host-migration handling anywhere): already-replicated deaths persist on
/// every peer, in-progress health pools reset under a new host.
/// </summary>
public static class SpitterKillManager
{
    private const string DamageEventName = "autogen_spitter_damage";

    /// <summary>Host→all broadcast of a spitter's health fraction, driving the
    /// damage-state glow on every peer (see SpitterVisuals).</summary>
    private const string HealthEventName = "autogen_spitter_health";

    /// <summary>Replicator IDs: BASE + shard (0..SHARD_COUNT-1). "SPIT".</summary>
    private const uint REPLICATOR_BASE_ID = 0x53504954;

    /// <summary>
    /// Fixed shard count so every peer creates identical replicators without
    /// knowing the spitter count. Capacity: 4 * 1920 = 7680 spitters; the
    /// heaviest generator output (Cryptomnesia InfectionFog) requests ~2800.
    /// </summary>
    private const int SHARD_COUNT = 4;

    public const int SpitterCapacity = SHARD_COUNT * SpitterDeathState.SpittersPerShard;

    /// <summary>Delay after the death pop completes before deactivation, so the
    /// spit FX/audio land naturally.</summary>
    private const float FinalizeGraceSeconds = 1.5f;

    /// <summary>Failsafe: a dying spitter is force-finalized this long after the
    /// kill even if its pop never completes.</summary>
    private const float FinalizeDeadlineSeconds = 5f;

    /// <summary>A death arriving within this window after a completed pop
    /// adopts that pop as the death explosion instead of triggering a second
    /// one (covers the killing hit's own pop finishing just before the death
    /// state arrives).</summary>
    private const float PopAdoptWindowSeconds = 1.5f;

    /// <summary>Host-side clamp on a single reported hit (anti-grief / garbage
    /// data guard; the strongest sane bullet hits are well below this).</summary>
    private const float MaxReportedDamagePerHit = 100f;

    /// <summary>Permanent local kill-switch: flips on any unexpected exception,
    /// reverting to vanilla spitter behavior (house convention).</summary>
    private static bool _broken;

    private static bool _setupDone;
    private static bool _warnedCapacity;

    /// <summary>HOST only: remaining health per damaged spitter. Absent entry
    /// means full health (lazy-init from config on first hit).</summary>
    private static readonly Dictionary<ushort, float> _healthByIndex = new();

    /// <summary>ALL peers: cosmetic health-fraction mirror fed by the host's
    /// health broadcasts; drives the damage-state glow and pop tints.</summary>
    private static readonly Dictionary<ushort, float> _healthRel = new();

    /// <summary>Dead spitter indices on this peer (includes ones still playing
    /// their death pop). Mirrors the replicated bitmask; source of truth for
    /// all patch guards.</summary>
    private static readonly HashSet<ushort> _dead = new();

    /// <summary>Dying spitters awaiting finalization. float.MaxValue = pop still
    /// winding up; otherwise the Clock.Time at which to finalize.</summary>
    private static readonly Dictionary<ushort, float> _dyingFinalizeAt = new();

    /// <summary>Hard finalize deadline per dying spitter.</summary>
    private static readonly Dictionary<ushort, float> _dyingDeadline = new();

    /// <summary>Spitters already deactivated (idempotency guard).</summary>
    private static readonly HashSet<ushort> _finalized = new();

    /// <summary>Spitters currently observed mid-explosion by the Update
    /// postfix; used to detect the pop-completion transition.</summary>
    private static readonly HashSet<ushort> _explodingNow = new();

    /// <summary>Clock.Time at which each spitter's most recent pop completed.
    /// Tracked locally per peer (pops play per-peer); consulted by KillSpitter
    /// case (b) so a death arriving just after a pop adopts it. A real
    /// timestamp is used instead of inferring "just popped" from
    /// m_stayInTimer/m_isGlued signatures — those false-negative on glued
    /// spitters (double pop) and false-positive on unticked nodes where
    /// m_stayInTimer freezes at 32 (no death pop at all).</summary>
    private static readonly Dictionary<ushort, float> _lastPopCompletedAt = new();

    /// <summary>Live (non-silent) deaths whose destruction burst
    /// (SpitterDeathFx) hasn't played yet. Normally consumed the frame the
    /// death pop completes; FinalizeSpitter consumes any leftover at the
    /// deadline fallback so the burst is never lost.</summary>
    private static readonly HashSet<ushort> _pendingDeathFx = new();

    /// <summary>Dying spitters whose model was hidden at the death burst so
    /// the explosion and the disappearance land on the same frame — the
    /// finalize grace only protects the sound cleanup and FX tint restore,
    /// which run 1.5s later on the already-invisible object.</summary>
    private static readonly HashSet<ushort> _modelHidden = new();

    private static readonly StateReplicator<SpitterDeathState>?[] _replicators =
        new StateReplicator<SpitterDeathState>?[SHARD_COUNT];

    private static readonly SpitterDeathState[] _currentStates = new SpitterDeathState[SHARD_COUNT];

    /// <summary>Per-shard OnStateChanged handlers, stored for unsubscribe.</summary>
    private static readonly Action<SpitterDeathState, SpitterDeathState, bool>?[] _handlers =
        new Action<SpitterDeathState, SpitterDeathState, bool>?[SHARD_COUNT];

    /// <summary>
    /// Wired to GameDataAPI.OnGameDataInitialized in Plugin. NetworkAPI event
    /// registration is safe here even though NetworkAPI_Impl doesn't exist yet
    /// (GTFO-API caches registrations; LogArchivistManager precedent).
    /// StateReplicators can NOT be created here — they require
    /// APIStatus.Network.Ready, so they are created at OnBuildDone.
    /// </summary>
    public static void Setup()
    {
        if (_setupDone)
            return;
        _setupDone = true;

        NetworkAPI.RegisterEvent<SpitterDamageEvent>(DamageEventName, OnDamageEventReceived);
        NetworkAPI.RegisterEvent<SpitterHealthEvent>(HealthEventName, OnHealthEventReceived);

        LevelAPI.OnBuildDone += OnBuildDone;
        LevelAPI.OnLevelCleanup += OnLevelCleanup;
        LevelAPI.OnEnterLevel += ReconcileDeadSpitters;
    }

    #region Patch entry points

    /// <summary>
    /// True when the spitter is dead or currently dying. Patch guards use this
    /// to suppress pops, damage reports and stale vanilla packets.
    /// </summary>
    public static bool IsDeadOrDying(ushort spitterIndex)
        => _dead.Count != 0 && _dead.Contains(spitterIndex);

    /// <summary>
    /// Called from the InfectionSpitterDamage.BulletDamage prefix with the
    /// final (post-falloff) bullet damage. Host accumulates directly; clients
    /// report to the host on a reliable channel.
    /// </summary>
    public static void ReportBulletDamage(InfectionSpitter spitter, float dam)
    {
        if (_broken || spitter == null)
            return;

        if (float.IsNaN(dam) || dam <= 0f)
            return;

        var index = spitter.m_spitterIndex;

        if (IsDeadOrDying(index))
            return;

        if (index >= SpitterCapacity)
        {
            WarnCapacityOnce(index);
            return;
        }

        if (SNet.IsMaster)
        {
            if (Plugin.Config_KillableSpitters)
                AccumulateDamage(index, Math.Min(dam, MaxReportedDamagePerHit));
            return;
        }

        // Client: report to the host, who owns the health ledger. Same
        // targeted-send overload AmorLib's replicator handshake uses.
        if (!SNet.HasMaster)
            return;

        NetworkAPI.InvokeEvent(
            DamageEventName,
            new SpitterDamageEvent { SpitterIndex = index, Damage = dam },
            SNet.Master,
            SNet_ChannelType.GameOrderCritical);
    }

    /// <summary>
    /// Called at the top of Fix_SpitterBotAggro.Pre_ManagerUpdate: dead/dying
    /// spitters must not tick (StaticUpdateManager invokes ManagerUpdate via
    /// the node's owner list regardless of GameObject active state). Also acts
    /// as the fallback finalize driver should the Update postfix ever stop
    /// firing (e.g. a game update breaks that patch).
    /// </summary>
    public static bool ShouldBlockManagerUpdate(InfectionSpitter spitter)
    {
        if (_broken)
            return false;

        try
        {
            if (_dead.Count == 0 || spitter == null)
                return false;

            var index = spitter.m_spitterIndex;

            if (!_dead.Contains(index))
                return false;

            if (_dyingFinalizeAt.TryGetValue(index, out var finalizeAt))
            {
                var deadline = _dyingDeadline.TryGetValue(index, out var d) ? d : float.MaxValue;

                if (Clock.Time >= deadline
                    || (finalizeAt != float.MaxValue && Clock.Time >= finalizeAt))
                {
                    FinalizeSpitter(index, spitter);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Break(ex);
            return false;
        }
    }

    /// <summary>
    /// Called from the InfectionSpitter.Update postfix. Watches dying spitters
    /// for pop completion (m_isExploding flipping false), then finalizes after
    /// the grace period. Update always runs for dying spitters: every death
    /// path either finds m_isExploding already true or calls DoExplode, both
    /// of which set enabled = true.
    /// </summary>
    public static void OnSpitterUpdatePostfix(InfectionSpitter spitter)
    {
        if (_broken || spitter == null)
            return;

        try
        {
            var index = spitter.m_spitterIndex;

            // Restore any timer-based FX tints that are past due (the spitter
            // that popped keeps Updating ≥30s, so a driver is guaranteed
            // while restores are pending).
            SpitterVisuals.TickRestores();

            // Track pop completions for every spitter (KillSpitter case (b)).
            // Update only runs while the component is enabled, and both
            // DoExplode and the pop completion keep enabled = true, so the
            // m_isExploding false-transition is always observed here.
            if (spitter.m_isExploding)
            {
                _explodingNow.Add(index);

                // Override vanilla's green wind-up ramp (its per-frame
                // s_glowColor * progression write runs just before this
                // postfix): red for the death pop, damage-tinted otherwise.
                if (_dyingFinalizeAt.ContainsKey(index))
                    SpitterVisuals.ApplyDeathRamp(spitter);
                else if (_healthRel.TryGetValue(index, out var rel))
                    SpitterVisuals.ApplyDamageRamp(spitter, rel);
            }
            else if (_explodingNow.Remove(index))
            {
                _lastPopCompletedAt[index] = Clock.Time;

                if (_dyingFinalizeAt.ContainsKey(index))
                {
                    // The death pop fired earlier in this same vanilla Update
                    // call (m_fx acquired + played, not yet simulated or
                    // rendered) — tint the burst red, and land the
                    // destruction burst + the model's disappearance together
                    // with the pop.
                    SpitterVisuals.TintPop(index, spitter, SpitterVisuals.DeathPopColor,
                        recolorLiveParticles: false, untilFinalize: true);

                    if (_pendingDeathFx.Remove(index))
                    {
                        SpitterDeathFx.PlayAt(spitter.m_position);
                        HideSpitterModel(index, spitter);
                    }
                }
                else if (_healthRel.TryGetValue(index, out var rel))
                {
                    // Normal pop of a damaged spitter: tint the burst by its
                    // missing health (timer-based restore) and re-apply the
                    // steady damage tint vanilla just wiped.
                    SpitterVisuals.TintPop(index, spitter, SpitterVisuals.GetPopColor(rel),
                        recolorLiveParticles: false, untilFinalize: false);
                    SpitterVisuals.ApplyDamageTint(spitter, rel);
                }
            }

            if (_dyingFinalizeAt.Count == 0)
                return;

            if (!_dyingFinalizeAt.TryGetValue(index, out var finalizeAt))
                return;

            var deadline = _dyingDeadline.TryGetValue(index, out var d) ? d : float.MaxValue;

            if (finalizeAt == float.MaxValue)
            {
                if (!spitter.m_isExploding)
                {
                    // Pop just completed — deactivate after the grace period.
                    _dyingFinalizeAt[index] = Clock.Time + FinalizeGraceSeconds;
                }
                else if (Clock.Time >= deadline)
                {
                    FinalizeSpitter(index, spitter);
                }

                return;
            }

            if (Clock.Time >= finalizeAt || Clock.Time >= deadline)
                FinalizeSpitter(index, spitter);
        }
        catch (Exception ex)
        {
            Break(ex);
        }
    }

    /// <summary>
    /// Permanently disables the feature on this peer after an unexpected
    /// exception, reverting to vanilla behavior.
    /// </summary>
    public static void Break(Exception ex)
    {
        if (_broken)
            return;

        _broken = true;
        Plugin.Logger.LogError($"[SpitterKill] Unexpected error, disabling killable spitters locally: {ex}");

        // Never strand a tinted FX pool instance (session lifetime) behind a
        // permanent feature kill.
        try
        {
            SpitterVisuals.RestoreAll();
        }
        catch (Exception restoreEx)
        {
            Plugin.Logger.LogWarning($"[SpitterKill] FX restore during Break failed: {restoreEx.Message}");
        }

        // Nor an invisible-but-alive spitter: models hidden at a death burst
        // that never finalized go back to vanilla visible.
        try
        {
            foreach (var index in _modelHidden.ToList())
            {
                var spitter = TryGetSpitter(index);
                if (spitter != null)
                    ShowSpitterModel(index, spitter);
            }
            _modelHidden.Clear();
        }
        catch (Exception showEx)
        {
            Plugin.Logger.LogWarning($"[SpitterKill] Model restore during Break failed: {showEx.Message}");
        }
    }

    #endregion

    #region Host health ledger

    /// <summary>
    /// NetworkAPI handler for client damage reports. Sends are targeted at the
    /// master, but the guard also keeps this correct under broadcast delivery.
    /// </summary>
    private static void OnDamageEventReceived(ulong senderPlayer, SpitterDamageEvent data)
    {
        if (_broken || !SNet.IsMaster || !Plugin.Config_KillableSpitters)
            return;

        try
        {
            if (float.IsNaN(data.Damage) || data.Damage <= 0f)
                return;

            if (data.SpitterIndex >= SpitterCapacity)
            {
                WarnCapacityOnce(data.SpitterIndex);
                return;
            }

            AccumulateDamage(data.SpitterIndex, Math.Min(data.Damage, MaxReportedDamagePerHit));
        }
        catch (Exception ex)
        {
            Break(ex);
        }
    }

    /// <summary>
    /// HOST only: applies damage to a spitter's pool; at zero, marks it dead in
    /// the replicated bitmask and kills it. Reports are processed serially, so
    /// two simultaneous killing blows can't double-kill (the second hits the
    /// IsDeadOrDying guard).
    /// </summary>
    private static void AccumulateDamage(ushort index, float dam)
    {
        if (IsDeadOrDying(index))
            return;

        if (!_healthByIndex.TryGetValue(index, out var health))
            health = Math.Max(1f, Plugin.Config_SpitterHealth);

        health -= dam;

        if (health > 0f)
        {
            _healthByIndex[index] = health;
            BroadcastHealth(index, health);
            return;
        }

        _healthByIndex.Remove(index);
        _dead.Add(index);

        var (shard, local) = SpitterDeathState.MapIndex(index);
        _currentStates[shard].SetDead(local);

        var replicator = _replicators[shard];
        if (replicator != null && replicator.IsValid)
        {
            // Broadcasts to all peers AND fires our OnStateChanged locally;
            // the direct KillSpitter below is belt-and-braces (idempotent).
            replicator.SetState(_currentStates[shard]);
        }
        else
        {
            Plugin.Logger.LogWarning(
                $"[SpitterKill] Replicator for shard {shard} missing, spitter {index} death is local-only");
        }

        Plugin.Logger.LogDebug($"[SpitterKill] Spitter {index} killed (host)");
        KillSpitter(index, silent: false);
    }

    /// <summary>
    /// HOST only: broadcasts a damaged spitter's health fraction so every peer
    /// can tint its glow. No broadcast on the killing blow — the replicated
    /// death state drives the death visuals. The broadcast does not loop back
    /// to the sender, so the host applies locally via a direct handler call
    /// (LogArchivistManager precedent).
    /// </summary>
    private static void BroadcastHealth(ushort index, float health)
    {
        var data = new SpitterHealthEvent
        {
            SpitterIndex = index,
            HealthRel = Math.Clamp(health / Math.Max(1f, Plugin.Config_SpitterHealth), 0f, 1f),
        };

        NetworkAPI.InvokeEvent(HealthEventName, data);
        OnHealthEventReceived(0uL, data);
    }

    /// <summary>
    /// NetworkAPI handler for health broadcasts (all peers): mirrors the
    /// fraction and applies the steady damage tint. Direct propBlock writes
    /// work on disabled/Frozen spitters too; if the spitter is mid-windup,
    /// vanilla overwrites next frame and the pop-completion re-apply in
    /// OnSpitterUpdatePostfix fixes it.
    /// </summary>
    private static void OnHealthEventReceived(ulong senderPlayer, SpitterHealthEvent data)
    {
        if (_broken)
            return;

        try
        {
            if (float.IsNaN(data.HealthRel))
                return;

            if (data.SpitterIndex >= SpitterCapacity)
            {
                WarnCapacityOnce(data.SpitterIndex);
                return;
            }

            if (IsDeadOrDying(data.SpitterIndex))
                return;

            var healthRel = Math.Clamp(data.HealthRel, 0f, 1f);
            _healthRel[data.SpitterIndex] = healthRel;

            var spitter = TryGetSpitter(data.SpitterIndex);
            if (spitter != null)
                SpitterVisuals.ApplyDamageTint(spitter, healthRel);
        }
        catch (Exception ex)
        {
            Break(ex);
        }
    }

    #endregion

    #region Death application (all peers)

    private static void OnDeathStateChanged(
        int shard, SpitterDeathState oldState, SpitterDeathState newState, bool isRecall)
    {
        if (_broken)
            return;

        try
        {
            _currentStates[shard] = newState;

            for (var i = 0; i < SpitterDeathState.MaskBytes; i++)
            {
                var oldByte = oldState.GetMaskByte(i);
                var newByte = newState.GetMaskByte(i);

                if (oldByte == newByte)
                    continue;

                var set = (byte)(newByte & ~oldByte);
                var cleared = (byte)(oldByte & ~newByte);

                for (var bit = 0; bit < 8; bit++)
                {
                    var index = (ushort)(shard * SpitterDeathState.SpittersPerShard + i * 8 + bit);
                    var mask = 1 << bit;

                    if ((set & mask) != 0)
                    {
                        _dead.Add(index);
                        // Recall = late joiner drop-in or checkpoint restore:
                        // apply the death without replaying the explosion.
                        KillSpitter(index, silent: isRecall);
                    }
                    else if ((cleared & mask) != 0)
                    {
                        // Only checkpoint recall can clear bits (state restored
                        // to a pre-death snapshot).
                        ReviveSpitter(index);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Break(ex);
        }
    }

    /// <summary>
    /// Applies a spitter death locally. Non-silent deaths guarantee exactly one
    /// final explosion by adopting a pop that is already in flight (a) or just
    /// finished (b), else triggering one (c). Silent deaths (recall) deactivate
    /// immediately with no pop or sound.
    /// </summary>
    private static void KillSpitter(ushort index, bool silent)
    {
        if (_finalized.Contains(index))
            return;

        if (_dyingFinalizeAt.ContainsKey(index) && !silent)
            return;

        _dead.Add(index);

        var spitter = TryGetSpitter(index);
        if (spitter == null)
        {
            // Registry not populated yet (recall raced level build) — the dead
            // bit is kept and ReconcileDeadSpitters sweeps it at OnEnterLevel.
            Plugin.Logger.LogDebug($"[SpitterKill] Spitter {index} not resolvable yet, deferring removal");
            return;
        }

        if (silent)
        {
            FinalizeSpitter(index, spitter);
            return;
        }

        if (spitter.m_isExploding)
        {
            // (a) The killing hit's own pop (or a racing vanilla explode
            // packet) is still winding up — adopt it as the death pop; the
            // destruction burst plays the frame it completes.
            _dyingFinalizeAt[index] = float.MaxValue;
            _pendingDeathFx.Add(index);
        }
        else if (_lastPopCompletedAt.TryGetValue(index, out var poppedAt)
                 && Clock.Time - poppedAt <= PopAdoptWindowSeconds)
        {
            // (b) A pop completed moments ago (the killing hit's own
            // explosion finished before the death state arrived) — adopt it
            // instead of double-popping. Timestamped by the Update postfix,
            // so this holds for glued spitters and stays bounded on nodes
            // where ManagerUpdate isn't ticking.
            _dyingFinalizeAt[index] = Clock.Time + FinalizeGraceSeconds;

            // The adopted burst already played (green / damage-tinted) and is
            // likely still airborne — switch its live particles to death red,
            // and the pop being over means the destruction lands now.
            SpitterVisuals.TintPop(index, spitter, SpitterVisuals.DeathPopColor,
                recolorLiveParticles: true, untilFinalize: true);
            SpitterDeathFx.PlayAt(spitter.m_position);
            HideSpitterModel(index, spitter);
        }
        else
        {
            // (c) No pop available — rare fallback now that every hit pops
            // (e.g. a client with the feature toggled off landed the killing
            // blow under the vanilla cooldown). Trigger the death pop
            // directly; matches vanilla SendSlowExplode semantics for hidden
            // spitters.
            spitter.DoExplode(
                spitter.m_currentState == InfectionSpitter.eSpitterState.Retracted ? 0.5f : 1f);
            _dyingFinalizeAt[index] = float.MaxValue;
            _pendingDeathFx.Add(index);
        }

        _dyingDeadline[index] = Clock.Time + FinalizeDeadlineSeconds;
    }

    /// <summary>
    /// Visually removes a dying spitter the moment its death burst plays:
    /// renderer and glow light off, damage proxy deactivated (no invisible
    /// shot-blocker). The spitter's own GameObject stays active — its Update
    /// drives finalization — so the sound cleanup and FX tint restore keep
    /// their full grace period behind an already-invisible model.
    /// </summary>
    private static void HideSpitterModel(ushort index, InfectionSpitter spitter)
    {
        _modelHidden.Add(index);

        if (spitter.m_renderer != null)
            spitter.m_renderer.enabled = false;

        if (spitter.m_light != null)
            spitter.m_light.enabled = false;

        if (spitter.m_damage != null && spitter.m_damage.gameObject != spitter.gameObject)
            spitter.m_damage.gameObject.SetActive(false);
    }

    /// <summary>Reverses HideSpitterModel (checkpoint revive, Break).</summary>
    private static void ShowSpitterModel(ushort index, InfectionSpitter spitter)
    {
        _modelHidden.Remove(index);

        if (spitter.m_renderer != null)
            spitter.m_renderer.enabled = true;

        if (spitter.m_light != null)
            spitter.m_light.enabled = true;

        if (spitter.m_damage != null && spitter.m_damage.gameObject != spitter.gameObject)
            spitter.m_damage.gameObject.SetActive(true);
    }

    /// <summary>
    /// Deactivates a dead spitter. Mirrors the resource cleanup the vanilla
    /// Frozen transition would do (which the ManagerUpdate gate now prevents):
    /// the CellSoundPlayer must be recycled and any wind-up FX light released.
    /// The spitter intentionally stays in s_allSpitters (index stability) and
    /// keeps its node registration (ticking is blocked by the gate instead —
    /// mutating the IL2CPP m_staticUpdateOwners interface list is not safe).
    /// </summary>
    private static void FinalizeSpitter(ushort index, InfectionSpitter spitter)
    {
        if (!_finalized.Add(index))
            return;

        _dyingFinalizeAt.Remove(index);
        _dyingDeadline.Remove(index);
        _explodingNow.Remove(index);
        _lastPopCompletedAt.Remove(index);
        _modelHidden.Remove(index);
        _healthRel.Remove(index);

        // Single convergence point of every death path — restore the death
        // pop's FX tint here (the pool instance outlives the level).
        SpitterVisuals.RestoreForSpitter(index);

        spitter.CleanupSound();

        // (The R6 decompile's wind-up FX_PointLight no longer exists in the
        // current game build — no light to release here; the pooled spit FX
        // in m_fx self-manages and is returned by OnDestroy at level teardown.)

        // Deadline fallback: the burst normally plays at pop completion, but
        // if the pop never completed the death must still read as an
        // explosion (live deaths only — recall/reconcile never mark the set).
        if (_pendingDeathFx.Remove(index))
            SpitterDeathFx.PlayAt(spitter.m_position);

        spitter.enabled = false;
        spitter.gameObject.SetActive(false);

        if (spitter.m_damage != null && spitter.m_damage.gameObject != spitter.gameObject)
            spitter.m_damage.gameObject.SetActive(false);

        Plugin.Logger.LogDebug($"[SpitterKill] Spitter {index} removed");
    }

    /// <summary>
    /// Reverses a death after a checkpoint recall restores a pre-death
    /// snapshot. The spitter reactivates hidden/frozen and wakes naturally via
    /// ManagerUpdate; the host ledger entry is dropped so it returns at full
    /// health.
    /// </summary>
    private static void ReviveSpitter(ushort index)
    {
        _dead.Remove(index);
        _finalized.Remove(index);
        _dyingFinalizeAt.Remove(index);
        _dyingDeadline.Remove(index);
        _explodingNow.Remove(index);
        _lastPopCompletedAt.Remove(index);
        _pendingDeathFx.Remove(index);
        _healthByIndex.Remove(index);
        _healthRel.Remove(index);
        SpitterVisuals.RestoreForSpitter(index);

        var spitter = TryGetSpitter(index);
        if (spitter == null)
            return;

        spitter.gameObject.SetActive(true);

        // Unconditional: SetActive(true) alone would leave the renderer/light
        // disabled if the model was hidden at the death burst (idempotent for
        // never-hidden spitters; also re-activates the damage proxy).
        ShowSpitterModel(index, spitter);

        // The propBlock still holds the death/damage tint otherwise.
        SpitterVisuals.ResetBodyGlow(spitter);

        Plugin.Logger.LogDebug($"[SpitterKill] Spitter {index} revived (checkpoint recall)");
    }

    /// <summary>
    /// OnEnterLevel insurance: finalize any dead spitter that is still active,
    /// e.g. when a recall delivered dead bits before the local registry was
    /// populated.
    /// </summary>
    private static void ReconcileDeadSpitters()
    {
        if (_broken || _dead.Count == 0)
            return;

        try
        {
            foreach (var index in _dead.ToList())
            {
                if (_finalized.Contains(index) || _dyingFinalizeAt.ContainsKey(index))
                    continue;

                var spitter = TryGetSpitter(index);
                if (spitter != null)
                    FinalizeSpitter(index, spitter);
            }
        }
        catch (Exception ex)
        {
            Break(ex);
        }
    }

    private static InfectionSpitter? TryGetSpitter(ushort index)
    {
        var all = InfectionSpitter.s_allSpitters;

        if (all == null || index >= all.Count)
            return null;

        return all[index];
    }

    #endregion

    #region Level lifecycle

    /// <summary>
    /// Creates the fixed replicator set once the level is built (NetworkAPI is
    /// guaranteed ready by then; creating replicators also registers AmorLib's
    /// network event handlers for this payload type before any recall can
    /// arrive — a late joiner's own OnBuildDone runs before their recall).
    /// </summary>
    private static void OnBuildDone()
    {
        if (_broken)
            return;

        try
        {
            UnloadReplicators();
            ClearRuntimeState();

            if (!APIStatus.Network.Ready)
            {
                // By OnBuildDone NetworkAPI is always ready; if not, something
                // is fundamentally wrong — fall back to vanilla spitters.
                Plugin.Logger.LogError("[SpitterKill] NetworkAPI not ready during OnBuildDone, spitters stay vanilla");
                return;
            }

            for (var i = 0; i < SHARD_COUNT; i++)
            {
                var initial = new SpitterDeathState { ShardIndex = (byte)i };

                var replicator = StateReplicator<SpitterDeathState>.Create(
                    REPLICATOR_BASE_ID + (uint)i,
                    initial,
                    LifeTimeType.Session);

                if (replicator == null)
                {
                    Plugin.Logger.LogError($"[SpitterKill] Failed to create replicator shard {i}");
                    continue;
                }

                _currentStates[i] = initial;

                var shard = i;
                Action<SpitterDeathState, SpitterDeathState, bool> handler =
                    (oldState, newState, isRecall) => OnDeathStateChanged(shard, oldState, newState, isRecall);

                replicator.OnStateChanged += handler;
                _handlers[i] = handler;
                _replicators[i] = replicator;
            }

            Plugin.Logger.LogDebug(
                $"[SpitterKill] Ready ({SHARD_COUNT} shards, killable={Plugin.Config_KillableSpitters}, " +
                $"health={Plugin.Config_SpitterHealth}, IsMaster={SNet.IsMaster})");
        }
        catch (Exception ex)
        {
            Break(ex);
        }
    }

    private static void OnLevelCleanup()
    {
        try
        {
            UnloadReplicators();
            ClearRuntimeState();
        }
        catch (Exception ex)
        {
            // Cleanup failures shouldn't disable the feature permanently.
            Plugin.Logger.LogError($"[SpitterKill] Cleanup error: {ex}");
        }
    }

    /// <summary>
    /// Called from FactoryJobManager.LevelCleanup during a mid-load level
    /// rebuild (which destroys all spitter GameObjects). Rebuilds happen before
    /// gameplay so nothing can be dead yet, and replicators don't exist yet
    /// (OnBuildDone only fires after the final successful build attempt) —
    /// clearing the runtime state is all that's needed.
    /// </summary>
    public static void OnLevelRebuild()
    {
        ClearRuntimeState();
    }

    private static void UnloadReplicators()
    {
        for (var i = 0; i < SHARD_COUNT; i++)
        {
            var replicator = _replicators[i];
            if (replicator == null)
                continue;

            if (_handlers[i] != null)
                replicator.OnStateChanged -= _handlers[i];

            replicator.Unload();
            _replicators[i] = null;
            _handlers[i] = null;
        }
    }

    private static void ClearRuntimeState()
    {
        _healthByIndex.Clear();
        _healthRel.Clear();
        _dead.Clear();
        _dyingFinalizeAt.Clear();
        _dyingDeadline.Clear();
        _finalized.Clear();
        _explodingNow.Clear();
        _lastPopCompletedAt.Clear();
        _pendingDeathFx.Clear();
        _modelHidden.Clear();
        _warnedCapacity = false;

        // Pool instances outlive the level — never leave a tint behind.
        SpitterVisuals.RestoreAll();

        for (var i = 0; i < SHARD_COUNT; i++)
            _currentStates[i] = new SpitterDeathState { ShardIndex = (byte)i };
    }

    private static void WarnCapacityOnce(ushort index)
    {
        if (_warnedCapacity)
            return;

        _warnedCapacity = true;
        Plugin.Logger.LogWarning(
            $"[SpitterKill] Spitter index {index} exceeds capacity {SpitterCapacity}; " +
            "spitters beyond capacity stay vanilla (unkillable)");
    }

    #endregion
}
