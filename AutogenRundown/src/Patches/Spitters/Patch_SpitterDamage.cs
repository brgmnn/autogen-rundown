using HarmonyLib;

namespace AutogenRundown.Patches.Spitters;

/// <summary>
/// Harmony taps/guards for killable spitters. All logic lives in
/// SpitterKillManager; every patch here is a thin guard or tap that defaults
/// to vanilla behavior on any error (and SpitterKillManager.Break permanently
/// reverts to vanilla after an unexpected exception).
///
/// Damage-type routing (user decision: bullets only):
/// - Bullets: InfectionSpitterDamage.BulletDamage receives the final
///   post-falloff damage (BulletWeapon.BulletHit applies falloff before the
///   IDamageable call, decompile Gear/BulletWeapon.cs:525-544) and is the only
///   bullets-specific hook — OnIncomingDamage funnels all damage types with no
///   type information. Sentry guns route through the same call and only deal
///   damage on the host. Shotguns report once per pellet.
/// - Melee/explosion/fire: untouched — they still pop the spitter via the
///   vanilla OnIncomingDamage path but deal no health damage.
///
/// IL2CPP notes: the interop assembly exposes InfectionSpitter's private
/// members (fields, Update, ReceiveDamage) as public, so they are patchable /
/// readable directly — same mechanism Fix_SpitterBotAggro relies on. The
/// target class of Pre_BulletDamage is the top-level global::InfectionSpitterDamage
/// component, NOT the identically-named nested packet struct
/// InfectionSpitter.InfectionSpitterDamage (which Pre_ReceiveDamage takes as
/// its parameter).
/// </summary>
[HarmonyPatch]
internal static class Patch_SpitterDamage
{
    /// <summary>
    /// Bullet damage tap + dead-guard. Dead/dying spitters take no hits at
    /// all; live ones report toward the health pool and then run the
    /// original, which funnels into OnIncomingDamage — where the prefix below
    /// pops the spitter on every hit (vanilla 5s cooldown removed).
    /// </summary>
    [HarmonyPatch(typeof(global::InfectionSpitterDamage), nameof(global::InfectionSpitterDamage.BulletDamage))]
    [HarmonyPrefix]
    public static bool Pre_BulletDamage(global::InfectionSpitterDamage __instance, float dam)
    {
        try
        {
            var spitter = __instance.m_spitter;
            if (spitter == null)
                return true;

            if (SpitterKillManager.IsDeadOrDying(spitter.m_spitterIndex))
                return false;

            SpitterKillManager.ReportBulletDamage(spitter, dam);
            return true;
        }
        catch (Exception ex)
        {
            SpitterKillManager.Break(ex);
            return true;
        }
    }

    /// <summary>
    /// Pop-per-hit + dead-guard for every pop source (bullets, melee,
    /// explosion, fire all funnel here — decompile
    /// InfectionSpitterDamage.cs:25-77). Vanilla gates damage pops behind a
    /// 5s cooldown (m_damageExplodeTimer, InfectionSpitter.cs:337-347); this
    /// replaces the body without it, so sustained fire keeps popping the
    /// spitter until it dies. DoExplode's own m_isExploding re-entry guard
    /// paces the pops to one per wind-up cycle, and the m_isExploding skip
    /// below also avoids re-broadcasting the vanilla explode packet for every
    /// bullet landing mid-wind-up. Pops are triggered on the shooter's client
    /// (vanilla design), so the LOCAL feature toggle gates the cooldown
    /// bypass; disabled peers keep vanilla behavior.
    /// </summary>
    [HarmonyPatch(typeof(InfectionSpitter), nameof(InfectionSpitter.OnIncomingDamage))]
    [HarmonyPrefix]
    public static bool Pre_OnIncomingDamage(InfectionSpitter __instance)
    {
        try
        {
            // Dead/dying spitters must not trigger further explosions.
            if (SpitterKillManager.IsDeadOrDying(__instance.m_spitterIndex))
                return false;

            if (!Plugin.Config_KillableSpitters)
                return true;

            // Pop already winding up — nothing to add, don't resend.
            if (__instance.m_isExploding)
                return false;

            // Vanilla body minus the 5s m_damageExplodeTimer gate.
            if (__instance.m_currentState == InfectionSpitter.eSpitterState.Retracted)
                __instance.SendSlowExplode();
            else
                __instance.SendExplode();

            return false;
        }
        catch (Exception ex)
        {
            SpitterKillManager.Break(ex);
            return true;
        }
    }

    /// <summary>
    /// C-foam kill tap: DoGetGlued runs on every peer for any foaming (local
    /// trigger via SendGlued, or the vanilla packet via ReceiveDamage —
    /// decompile InfectionSpitter.cs:349-404), giving the host a clean
    /// "became glued" edge for the foam-kill clock.
    /// </summary>
    [HarmonyPatch(typeof(InfectionSpitter), nameof(InfectionSpitter.DoGetGlued))]
    [HarmonyPostfix]
    public static void Post_DoGetGlued(InfectionSpitter __instance)
    {
        SpitterKillManager.OnSpitterGlued(__instance);
    }

    /// <summary>
    /// Dead-guard for glue: DoGetGlued's long timed retract would fight the
    /// death pop's state on a dying spitter.
    /// </summary>
    [HarmonyPatch(typeof(InfectionSpitter), nameof(InfectionSpitter.OnIncomingGlue))]
    [HarmonyPrefix]
    public static bool Pre_OnIncomingGlue(InfectionSpitter __instance)
    {
        try
        {
            return !SpitterKillManager.IsDeadOrDying(__instance.m_spitterIndex);
        }
        catch (Exception ex)
        {
            SpitterKillManager.Break(ex);
            return true;
        }
    }

    /// <summary>
    /// Drops stale vanilla explode/glue packets addressed to dead/dying
    /// spitters (decompile InfectionSpitter.cs:381-404 — ReceiveDamage would
    /// call DoExplode/DoGetGlued on the deactivated object). While a spitter
    /// is merely dying an inbound Explode is already harmless (DoExplode
    /// no-ops on m_isExploding), but pops during the dying window are unwanted
    /// anyway.
    /// </summary>
    [HarmonyPatch(typeof(InfectionSpitter), nameof(InfectionSpitter.ReceiveDamage))]
    [HarmonyPrefix]
    public static bool Pre_ReceiveDamage(InfectionSpitter.InfectionSpitterDamage data)
    {
        try
        {
            return !SpitterKillManager.IsDeadOrDying(data.spitterIndex);
        }
        catch (Exception ex)
        {
            SpitterKillManager.Break(ex);
            return true;
        }
    }

    /// <summary>
    /// Pop-completion watcher for dying spitters. Update only runs while the
    /// component is enabled, which is guaranteed during the death sequence
    /// (DoExplode and pop completion both set enabled = true). If patching
    /// this private Unity message ever fails on a game update, the deadline
    /// fallback in SpitterKillManager.ShouldBlockManagerUpdate still
    /// finalizes dying spitters (degraded: on the next ManagerUpdate tick
    /// instead of pop + grace).
    /// </summary>
    [HarmonyPatch(typeof(InfectionSpitter), nameof(InfectionSpitter.Update))]
    [HarmonyPostfix]
    public static void Post_Update(InfectionSpitter __instance)
    {
        SpitterKillManager.OnSpitterUpdatePostfix(__instance);
    }
}
