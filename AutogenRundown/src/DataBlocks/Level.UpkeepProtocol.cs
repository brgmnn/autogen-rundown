using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

/// <summary>
/// The UpkeepProtocol level signature (R8E2's ADMIN_TEMP_OVERRIDE): the level starts a
/// countdown at drop and every terminal carries a one-use override command that buys back
/// roughly its own zone's clear time. Expiry never fails the level — it starts a scoped
/// surge wave stream that runs until the next override is entered; once every terminal is
/// spent (typically extraction) the surge runs to the end of the level.
///
/// Runs from the Level.Build finalize phase, not ApplyLevelSignature: the grants need
/// clear-time estimates for every bulkhead's zones, which are only valid after all
/// FinalizeLayout calls have rolled alarms and enemies.
///
/// Known limitations (accepted): checkpoint restore aborts the countdown (AWO behavior,
/// same as Survival); two overrides landing in the same frame lose one grant; warnings
/// are one-shot per countdown even if a grant pushes remaining time back above them.
/// </summary>
public partial class Level
{
    /// <summary>Grace added to the first zone's clear estimate for the initial countdown.</summary>
    private const double UpkeepInitialGraceSeconds = 60.0;

    /// <summary>
    /// Grant = zone clear estimate × this factor. The margin above 1.0 is what funds
    /// objective dwell time (uplink sequences, HSU extract, Alpha transfer) — the clear
    /// estimates only cover traversal and combat.
    /// </summary>
    private const double UpkeepGrantFactor = 1.2;

    /// <summary>Duration of each post-expiry fallback countdown.</summary>
    private const double UpkeepFallbackWindowSeconds = 240.0;

    /// <summary>
    /// Fallback windows after the first expiry (≈32 min of runway) before the chain ends
    /// and the surge runs uncontested.
    /// </summary>
    private const int UpkeepFallbackCount = 8;

    /// <summary>
    /// AWO wave identifier for the surge stream: the overrides' stops are scoped to it,
    /// and global untagged stops (Alpha/TTS/uplink completion) never touch it.
    /// </summary>
    private const string UpkeepWaveIdentifier = "upkeep_surge";

    /// <summary>Override command name, verbatim from R8E2.</summary>
    private const string UpkeepCommand = "ADMIN_TEMP_OVERRIDE";

    private const string UpkeepTitle = ":://UPKEEP PROTOCOL - NEXT MAINTENANCE DUE";

    private const string UpkeepFallbackTitle = "<color=red>UPKEEP FAILURE</color> - SURGE UNTIL NEXT OVERRIDE";

    /// <summary>
    /// Applies the UpkeepProtocol signature: wires the override command onto every
    /// terminal in every Reality zone (all bulkheads) and starts the countdown chain on
    /// the Main objective's elevator land events.
    /// </summary>
    public void ApplyUpkeepProtocol()
    {
        if (Settings.Signature != LevelSignature.UpkeepProtocol)
            return;

        var commandCount = 0;

        foreach (var node in Planner.GetZones(Bulkhead.All, null, DimensionIndex.Reality))
        {
            var zone = Planner.GetZone(node);

            // Zones with deliberately stripped terminals (reactor zones, hill spawn
            // closets) stay stripped — skip, never re-add.
            if (zone == null || zone.ForbidTerminalsInZone || !zone.TerminalPlacements.Any())
                continue;

            var grant = Math.Round(zone.GetClearTimeEstimate() * UpkeepGrantFactor);

            for (var index = 0; index < zone.TerminalPlacements.Count; index++)
            {
                zone.TerminalPlacements[index].UniqueCommands.Add(
                    BuildUpkeepOverrideCommand(
                        grant,
                        Lore.TerminalSerial(DimensionIndex.Reality, node.Bulkhead, node.ZoneNumber, index)));
                commandCount++;
            }
        }

        // First real Main zone (the elevator drop zone needs no clearing); falls back to
        // a flat window if the progression is somehow all starting area.
        var firstNode = Planner
            .GetZones(Bulkhead.Main, "primary", DimensionIndex.Reality)
            .FirstOrDefault(node => !node.Bulkhead.HasFlag(Bulkhead.StartingArea));
        var initial = (Planner.GetZone(firstNode)?.GetClearTimeEstimate() ?? 120.0)
                      + UpkeepInitialGraceSeconds;

        GetObjective(Bulkhead.Main)!.EventsOnElevatorLand.AddCountdownWithExpiryChain(
            initial,
            GenericWave.UpkeepSurge,
            UpkeepWaveIdentifier,
            UpkeepFallbackCount,
            UpkeepFallbackWindowSeconds,
            UpkeepTitle,
            UpkeepFallbackTitle,
            expiryMessage: ":://UPKEEP FAILURE - SURGE PROTOCOL ACTIVE",
            warningMessage: ":://WARNING - MAINTENANCE WINDOW CLOSING",
            delay: 5.0);

        MarkAsUpkeepProtocol();

        Plugin.Logger.LogDebug(
            $"Level={Tier}{Index} -- Level signature: UpkeepProtocol, initial={initial}s, " +
            $"commands={commandCount}, fallback={UpkeepFallbackCount}x{UpkeepFallbackWindowSeconds}s");
    }

    private static CustomTerminalCommand BuildUpkeepOverrideCommand(double grantSeconds, string serial)
        => new()
        {
            Command = UpkeepCommand,
            CommandDesc = new Text("Applies a one-time maintenance credit to the sector upkeep ledger"),
            SpecialCommandRule = CommandRule.OnlyOnceDelete,
            CommandEvents = new List<WardenObjectiveEvent>()
                .AddAdjustTimer(grantSeconds, 6.0)
                .AddTurnOffAlarms(6.0, UpkeepWaveIdentifier)
                .AddSound(Sound.Alarms_Error_AmbientStop, 6.5)
                .AddMessage($":://MAINTENANCE CREDIT ACCEPTED - {serial}", 6.5)
                .ToList(),
            PostCommandOutputs = new List<TerminalOutput>
            {
                new()
                {
                    Output = "Requesting temporary administrator session...",
                    Type = LineType.SpinningWaitNoDone,
                    Time = 2.0
                },
                new()
                {
                    Output = "Credential accepted. Session scope: SECTOR MAINTENANCE",
                    Type = LineType.Normal,
                    Time = 1.5
                },
                new()
                {
                    Output = "Applying maintenance credit to upkeep ledger",
                    Type = LineType.SpinningWaitDone,
                    Time = 2.0
                },
                new()
                {
                    Output = "Credit applied. Credential expended - session closed.",
                    Type = LineType.Warning,
                    Time = 1.0
                },
            },
        };
}
