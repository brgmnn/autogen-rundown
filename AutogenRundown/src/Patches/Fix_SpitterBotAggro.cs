using AIGraph;
using AK;
using HarmonyLib;
using Player;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Stops AI bot teammates from triggering InfectionSpitter proximity aggro and
/// explosions. Vanilla InfectionSpitter.ManagerUpdate iterates
/// PlayerManager.PlayerAgentsInLevel with no bot check, so bots pathing near a
/// spitter wake it, run down its trigger timer, and (on the host) explode it —
/// causing infection damage and a noise event that wakes sleepers.
///
/// IL2CPP forbids transpilers, so this prefix skips the original and reimplements
/// the method body (decompile reference:
/// gtfo-decompile/Modules-ASM/InfectionSpitter.cs:183-301). Intentional
/// divergences from vanilla, all in the player loop:
///
/// 1. Bots (Owner.IsBot) are skipped entirely.
/// 2. Vanilla caps the loop at the first 4 entries of PlayerAgentsInLevel
///    (m_coverageDatas is sized [4]). The cap is lifted so spitters also react
///    to humans in slots 5+ when LobbyExpansion is installed (it expands the
///    coverage arrays to MaxPlayers but does not patch this method).
/// 3. Coverage is read per PlayerSlotIndex instead of per list position.
///    PlayerCoverageSystem.GamestateUpdate writes coverage at the slot index
///    (propagator.m_playerID = Owner.PlayerSlotIndex()), so the vanilla
///    list-position read is only correct while list order matches slot order.
///    The slot is bounds-checked against the coverage array, which also keeps
///    us safe if LobbyExpansion's native array-expansion patch fails to apply.
///
/// Explosions are master-only (SNet.IsMaster gate before SendExplode), so the
/// patch is only load-bearing on the host; on clients it just keeps the cosmetic
/// wake/purr state consistent.
///
/// The damage (OnIncomingDamage) and glue (OnIncomingGlue) explode paths are
/// untouched.
///
/// Interplay with killable spitters (Patches/Spitters): dead/dying spitters
/// are gated out at the very top of the prefix via
/// SpitterKillManager.ShouldBlockManagerUpdate — StaticUpdateManager invokes
/// ManagerUpdate through the node's m_staticUpdateOwners list regardless of
/// GameObject active state, and mutating that IL2CPP interface list is not
/// safe, so blocking the tick here is the removal mechanism. The gate sits
/// before the _broken check so dead spitters stay inert even when this
/// reimplementation has fallen back to vanilla.
/// </summary>
[HarmonyPatch]
internal static class Fix_SpitterBotAggro
{
    /// <summary>
    /// Permanently fall back to vanilla behavior if the reimplementation ever
    /// throws (e.g. a game update changed ManagerUpdate's internals).
    /// </summary>
    private static bool _broken;

    [HarmonyPatch(typeof(InfectionSpitter), nameof(InfectionSpitter.ManagerUpdate))]
    [HarmonyPrefix]
    public static bool Pre_ManagerUpdate(
        InfectionSpitter __instance,
        AIG_CourseNode courseNode,
        ref bool __result)
    {
        // Dead/dying spitters must not tick or proximity-explode (exception-
        // safe: returns false on any internal error). Also drives the
        // fallback finalize deadline for dying spitters.
        if (Spitters.SpitterKillManager.ShouldBlockManagerUpdate(__instance))
        {
            __result = false;
            return false;
        }

        if (_broken)
            return true;

        try
        {
            __result = ManagerUpdateNoBots(__instance, courseNode);
            return false;
        }
        catch (Exception ex)
        {
            _broken = true;
            Plugin.Logger.LogError($"[SpitterBotAggro] Reimplementation failed, reverting to vanilla behavior: {ex}");
            return true;
        }
    }

    /// <summary>
    /// Faithful port of InfectionSpitter.ManagerUpdate with bots skipped. Local
    /// names match the decompile (flagN/numN) to ease re-diffing after game
    /// updates.
    /// </summary>
    private static bool ManagerUpdateNoBots(InfectionSpitter s, AIG_CourseNode courseNode)
    {
        var localDelta = Clock.Time - s.m_lastUpdate;
        s.m_lastUpdate = Clock.Time;
        s.m_freezeTime = s.m_lastUpdate + 30f;

        var flag1 = false;
        var flag2 = s.m_currentState == InfectionSpitter.eSpitterState.Retracted;
        var flag3 = false;
        var num2 = 8.5f;
        var num3 = 0.0f;

        var coverage = courseNode.m_playerCoverage.m_coverageDatas;

        foreach (var playerAgent in PlayerManager.PlayerAgentsInLevel)
        {
            // Bots are invisible to spitters
            if (playerAgent?.Owner == null || playerAgent.Owner.IsBot)
                continue;

            // Coverage is written per slot index, not per list position. The
            // bounds check replaces vanilla's hard cap of 4 and covers >4-slot
            // lobbies where the coverage arrays were not expanded.
            var slot = playerAgent.PlayerSlotIndex;

            if (slot < 0 || slot >= coverage.Length)
                continue;

            if (coverage[slot].m_nodeDistanceUnblocked < 2 && playerAgent.Alive)
            {
                flag1 = true;
                var vector3 = playerAgent.EyePosition - s.m_position;
                var magnitude = vector3.magnitude;
                vector3.Normalize();
                var num5 = 0.0f;
                var flag4 = false;

                if (playerAgent.HasDetectionMod(out var distance) && magnitude < distance)
                {
                    num5 = playerAgent.GetDetectionMod(vector3, magnitude);
                    flag4 = num5 > 1f / 1000f;
                }

                var flag5 = !flag2 && magnitude < num2;

                // Precedence matches vanilla IL: flag5 | (flag4 && dot && !linecast)
                if (flag5 | flag4
                    && Vector3.Dot(s.m_fwd, vector3) >= -0.5f
                    && !Physics.Linecast(s.m_position, playerAgent.EyePosition, LayerManager.MASK_WORLD))
                {
                    if (flag5)
                    {
                        num3 = playerAgent.Locomotion.HorizontalVelocity.magnitude;
                        num2 = magnitude;
                        flag3 = true;
                    }
                    s.m_lightMeasure += num5 * localDelta * 5f;
                }
            }
        }

        if (s.m_lightMeasure > 0.5f)
        {
            if (s.m_currentState != InfectionSpitter.eSpitterState.Retracted)
            {
                s.TryPlaySound(EVENTS.INFECTION_SPITTER_SCARED, false, 1f);
                s.SetTimedRetract(30f, s.m_lightRetractSpeed);
            }
            else if (s.m_stayInTimer < 30f)
                s.m_stayInTimer = 30f;
        }

        if (s.m_currentState == InfectionSpitter.eSpitterState.Retracted)
        {
            s.m_timeToExplode = 3f;
            s.StopPurring();
            s.m_stayInTimer -= localDelta;

            if (s.m_stayInTimer < 0f)
            {
                s.m_stayInTimer = 0f;
                s.m_currentState = InfectionSpitter.eSpitterState.Woke;
            }
            else if (s.m_stayInTimer < 1f)
                s.TryPlaySound(EVENTS.INFECTION_SPITTER_PRIMED, true, 2f);
        }
        else if (flag3)
        {
            if (!s.m_isSoundOut)
            {
                s.TryPlaySound(EVENTS.INFECTION_SPITTER_OUT, true, 1f);
                s.m_isSoundOut = true;
            }
            if (num2 < 6f)
            {
                var num6 = 1f - num2 / 6f;
                var num7 = num3 > 5f ? 3f : 1f;
                s.m_timeToExplode -= localDelta * (num6 * num6 * 6.5f) * num7;
                s.UpdateTargetingRetraction();

                if (!s.m_purrLoopPlaying)
                {
                    s.TryPlaySound(EVENTS.INFECTION_SPITTER_PURR_LOOP, false, 1f);
                    s.m_purrLoopPlaying = true;
                }
                if (SNet.IsMaster && !s.m_isExploding && (num2 < 1f || s.m_timeToExplode <= 0f))
                    s.SendExplode();
            }
            else
                s.RestoreTargetTimer(localDelta);

            s.m_currentState = InfectionSpitter.eSpitterState.Woke;
        }
        else
        {
            s.RestoreTargetTimer(localDelta);
            if (s.m_currentState != InfectionSpitter.eSpitterState.Frozen)
            {
                if (s.m_hasReachedTarget && s.m_targetRetract > 0.99f)
                {
                    s.enabled = false;
                    s.m_currentState = InfectionSpitter.eSpitterState.Frozen;
                    s.CleanupSound();
                }
                else
                {
                    if (s.m_isSoundOut)
                    {
                        s.m_isSoundOut = false;
                        s.TryPlaySound(EVENTS.INFECTION_SPITTER_IN, true, 1f);
                    }

                    s.SetRetractTarget(0.5f, s.m_retractSpeed);
                }
            }
        }

        s.m_lightMeasure = Mathf.Clamp01(s.m_lightMeasure - localDelta * 0.25f);

        return flag1;
    }
}
