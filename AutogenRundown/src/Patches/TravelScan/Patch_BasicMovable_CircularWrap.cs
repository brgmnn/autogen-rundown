using ChainedPuzzles;
using HarmonyLib;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// Fixes the loop wrap in CP_BasicMovable's circular movement, which assumes the scan never moves
/// more than one waypoint in a single frame.
///
/// DoMoveScanner adds Clock.Delta * speed / segmentLength to m_lerpAmount each frame and calls
/// UpdateMovementRoutine once it crosses the next whole number. TryGetNextIndex takes the index as
/// (int)m_lerpAmount and only wraps it when it equals AmountOfPositions exactly; it folds a lerp
/// above AmountOfPositions back to its fraction, but only after the index has been read. So if one
/// frame carries the lerp over the end of the loop and past the next waypoint too, the index is
/// AmountOfPositions + 1, ScanPositions[] throws ArgumentOutOfRangeException, the move coroutine
/// dies and the scan stops for good.
///
/// Vanilla paths never trip it: their waypoints are metres apart. Ours follow the floor, so a
/// segment near the end of the loop can be shorter than one frame of travel — especially under a
/// raised timescale or a frame hitch. Folding the lerp into [0, AmountOfPositions) before the index
/// is read lands every case on a real waypoint, and is exactly what vanilla already does for a
/// single-step wrap.
/// </summary>
[HarmonyPatch(typeof(CP_BasicMovable), nameof(CP_BasicMovable.UpdateMovementRoutine))]
public static class Patch_BasicMovable_CircularWrap
{
    static void Prefix(CP_BasicMovable __instance)
    {
        if (__instance.m_typeOfMovement != MovementType.Circular)
            return;

        var count = __instance.AmountOfPositions;
        var lerp = __instance.m_lerpAmount;

        if (count <= 0 || lerp < count)
            return;

        __instance.m_lerpAmount = lerp % count;

        // Push the folded value into the replicated state, as the private LerpAmount setter would.
        __instance.SyncUpdate();
    }
}
