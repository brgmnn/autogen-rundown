namespace AutogenRundown.Serialization;

/// <summary>
/// A single expedition that could not be generated.
/// </summary>
public class BuildFailureRecord
{
    /// <summary>
    /// Tier letter, e.g. "C".
    /// </summary>
    public string Tier { get; set; } = "";

    /// <summary>
    /// Zero based index of the expedition within its tier.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Main layout persistent id at the time of failure. Used to verify the stored entry still
    /// refers to the same generated level before we lock it out.
    /// </summary>
    public uint MainLevelLayout { get; set; }

    /// <summary>
    /// How many rebuilds were attempted before giving up.
    /// </summary>
    public int Rebuilds { get; set; }

    public string FirstFailedUtc { get; set; } = "";
}

/// <summary>
/// Durable record of every level in a rundown that failed to generate.
///
/// One file per rundown, named after the rundown (which embeds the seed, see
/// <c>Rundown.Build</c>). <see cref="PluginVersion"/> pins the record to the generator that
/// produced those layouts -- a different Autogen version generates different levels, so the
/// record is discarded when the version no longer matches.
/// </summary>
public class RundownBuildFailureRecord
{
    public string Name { get; set; } = "";

    public string PluginVersion { get; set; } = "";

    public List<BuildFailureRecord> Levels { get; set; } = new();
}
