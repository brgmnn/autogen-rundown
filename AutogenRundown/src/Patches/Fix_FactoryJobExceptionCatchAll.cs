using AutogenRundown.Config;
using AutogenRundown.Managers;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

/// <summary>
/// Catch-all Finalizer on LG_Factory.Update — the single chokepoint at LG_Factory.cs:221
/// where every factory job's Build() is invoked. When the broken-zone cascade triggers
/// after Fix_FailedToFindStartArea hits fatalReached, downstream Distribution-batch jobs
/// throw AOOR/NRE in code paths we can't identify statically (the cpp2il decompile has
/// empty method bodies in the user's game version).
///
/// We only activate the safety net when a rebuild is already pending (ShouldRebuild=true),
/// so legitimate exceptions in healthy builds still surface normally. When triggered we:
///
///   1. Log the concrete job type + name + exception + stack trace once per
///      (jobType, exceptionType) pair — the type field is the diagnostic that tells us
///      which class to patch next if we want to replace the catch-all with a targeted prefix.
///   2. Drop the failing job (m_currentJob = null) so LG_Factory.Update's next tick calls
///      GetNewJob and advances past the failure instead of re-invoking the same broken
///      Build() every frame.
///   3. Suppress the exception so the engine can drain to FactoryDone, where
///      Patch_LG_Factory.Prefix_FactoryDone fires the queued rebuild.
///
/// Without a pending rebuild there is a second net: LG_Factory.Update only advances
/// m_currentJob after a successful Build(), so a job that throws deterministically
/// (e.g. LG_HSUScannerJob on an unmatched node cluster) re-throws every frame and hangs
/// the loading screen forever. When the same job instance throws StuckJobThreshold
/// consecutive ticks we declare it stuck: queue a rebuild, drop the job, and let the
/// drain net above handle the rest of the pass. The rebuild itself is bounded by
/// FactoryJobManager.MaxRebuilds, so an undetectable deterministic failure ends at the
/// give-up abort instead of an infinite loop.
/// </summary>
[HarmonyPatch]
public class Fix_FactoryJobExceptionCatchAll
{
    private static readonly HashSet<(string jobType, string exceptionType)> s_loggedFailures = new();

    /// <summary>
    /// Consecutive Update ticks the same job instance may throw before we declare it stuck.
    /// First occurrences still surface normally so one-off exceptions keep their stack trace
    /// in the log without triggering a rebuild.
    /// </summary>
    private const int StuckJobThreshold = 3;

    private static IntPtr s_lastFailedJobPointer = IntPtr.Zero;
    private static int s_consecutiveFailures;
    private static bool s_stuckJobDetected;

    public static void Setup()
    {
        FactoryJobManager.OnDoneValidate += Validate;
    }

    public static void ResetDiagnostics()
    {
        s_loggedFailures.Clear();
        s_lastFailedJobPointer = IntPtr.Zero;
        s_consecutiveFailures = 0;
        s_stuckJobDetected = false;
    }

    private static bool Validate()
    {
        if (!s_stuckJobDetected)
            return true;

        s_stuckJobDetected = false;

        Plugin.Logger.LogDebug("[FactoryJobCatchAll] Rebuilding after dropping a stuck job");

        return false;
    }

    [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.Update))]
    [HarmonyFinalizer]
    public static void Post_Update(LG_Factory __instance, ref Exception? __exception)
    {
        if (__exception == null)
        {
            s_lastFailedJobPointer = IntPtr.Zero;
            s_consecutiveFailures = 0;

            return;
        }

        var job = __instance.m_currentJob;
        var jobType = job?.GetType().FullName ?? "<null>";
        var jobName = job?.GetName() ?? "<no name>";
        var exType = __exception.GetType().FullName ?? "<unknown>";

        // On the give-up frame GetNewJob() already nulled m_currentJob and we suppressed the
        // rebuild, so Update() throws on the null job. That is expected, and we are not
        // "advancing past" anything -- the factory is about to be frozen. Swallow it silently.
        if (FactoryJobManager.GaveUp)
        {
            __instance.m_currentJob = null;
            __exception = null;

            return;
        }

        if (!FactoryJobManager.ShouldRebuild)
        {
            // Boehm GC never moves objects, so the il2cpp pointer is a stable identity for
            // "same job instance as last tick".
            var pointer = job?.Pointer ?? IntPtr.Zero;

            if (pointer == IntPtr.Zero || pointer != s_lastFailedJobPointer)
            {
                s_lastFailedJobPointer = pointer;
                s_consecutiveFailures = 1;

                return;
            }

            if (++s_consecutiveFailures < StuckJobThreshold)
                return;

            if (!GenerationOverrides.RebuildCheckEnabled(RebuildCheck.StuckFactoryJob))
            {
                if (s_loggedFailures.Add((jobType, exType)))
                    Plugin.Logger.LogWarning(
                        $"[RebuildChecks] {RebuildCheck.StuckFactoryJob} disabled — would have " +
                        $"dropped a stuck job and triggered a rebuild: " +
                        $"type={jobType} name=\"{jobName}\" exception={exType}");
                return;
            }

            Plugin.Logger.LogError(
                $"[FactoryJobCatchAll] Job stuck: threw {s_consecutiveFailures} consecutive ticks: " +
                $"type={jobType} name=\"{jobName}\" exception={exType}: {__exception.Message}\n" +
                $"{__exception.StackTrace}\n" +
                $"Dropping the job and queueing a level rebuild.");

            s_lastFailedJobPointer = IntPtr.Zero;
            s_consecutiveFailures = 0;
            s_stuckJobDetected = true;

            FactoryJobManager.MarkForRebuild();

            __instance.m_currentJob = null;
            __exception = null;

            return;
        }

        if (s_loggedFailures.Add((jobType, exType)))
        {
            Plugin.Logger.LogError(
                $"[FactoryJobCatchAll] Job threw during drain (ShouldRebuild=True): " +
                $"type={jobType} name=\"{jobName}\" exception={exType}: {__exception.Message}\n" +
                $"{__exception.StackTrace}\n" +
                $"Advancing past this job so the engine can reach FactoryDone.");
        }

        __instance.m_currentJob = null;
        __exception = null;
    }
}
