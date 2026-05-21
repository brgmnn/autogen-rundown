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
/// </summary>
[HarmonyPatch]
public class Fix_FactoryJobExceptionCatchAll
{
    private static readonly HashSet<(string jobType, string exceptionType)> s_loggedFailures = new();

    public static void ResetDiagnostics() => s_loggedFailures.Clear();

    [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.Update))]
    [HarmonyFinalizer]
    public static void Post_Update(LG_Factory __instance, ref Exception? __exception)
    {
        if (__exception == null)
            return;

        if (!FactoryJobManager.ShouldRebuild)
            return;

        var job = __instance.m_currentJob;
        var jobType = job?.GetType().FullName ?? "<null>";
        var jobName = job?.GetName() ?? "<no name>";
        var exType = __exception.GetType().FullName ?? "<unknown>";

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
