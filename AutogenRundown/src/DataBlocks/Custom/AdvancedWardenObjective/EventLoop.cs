using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;

/// <summary>
/// Loop events! Very useful for running repeated endless sets of events
///
/// !!! IMPORTANT NOTE !!!
///
///     Events must be set with `None` as their trigger type. `Start` will cause the events to
///     not be triggered and will cause a lot of head scratching.
/// </summary>
public record EventLoop
{
    public int LoopIndex { get; set; } = 0;

    public double LoopDelay { get; set; } = 1.0;

    public int LoopCount { get; set; } = -1;

    public List<WardenObjectiveEvent> EventsToActivate { get; set; } = new();
}
