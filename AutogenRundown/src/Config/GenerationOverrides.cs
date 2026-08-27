using AutogenRundown.DataBlocks.Enums;
using BepInEx;

namespace AutogenRundown.Config;

/// <summary>
/// Canonical names for the level rebuild validations that can be toggled via the
/// [Advanced.RebuildChecks] config section.
/// </summary>
public static class RebuildCheck
{
    public const string NavMeshReachability = "NavMeshReachability";
    public const string MissingCustomGeomorph = "MissingCustomGeomorph";
    public const string NullSourceExpander = "NullSourceExpander";
    public const string ZoneZeroAreas = "ZoneZeroAreas";
    public const string FailedToFindStartArea = "FailedToFindStartArea";
    public const string MissingGeneratorCluster = "MissingGeneratorCluster";

    public static readonly string[] All =
    {
        NavMeshReachability,
        MissingCustomGeomorph,
        NullSourceExpander,
        ZoneZeroAreas,
        FailedToFindStartArea,
        MissingGeneratorCluster,
    };
}

/// <summary>
/// Per-rundown generation overrides.
/// </summary>
public record RundownGenerationOverrides
{
    /// <summary>
    /// Force every level in this rundown to use the given complex. One of "Mining", "Tech", or
    /// "Service" (case-insensitive). Empty or unrecognized values disable forcing.
    /// </summary>
    public string ForcedComplex { get; set; } = "";

    /// <summary>
    /// Prefer Gardens tiles on Service levels: geomorph pick lists take a Gardens variant when
    /// one exists for the role, and the gardens-heavy resource set replaces the floodways-only
    /// one so unforced zones mostly roll garden tiles too.
    /// </summary>
    public bool PreferGardens { get; set; } = false;

    public Complex? ForcedComplexValue =>
        Enum.TryParse<Complex>(ForcedComplex, true, out var complex) &&
        Enum.IsDefined(typeof(Complex), complex)
            ? complex
            : null;
}

/// <summary>
/// Advanced generation overrides, populated from the [Advanced.*] sections of the main
/// BepInEx config by PluginConfig.Setup. Everything defaults to vanilla behavior; rebuild
/// checks are all enabled unless explicitly set to false. These settings must match across
/// all players in a lobby.
/// </summary>
public record GenerationOverrides
{
    public RundownGenerationOverrides Daily { get; set; } = new();

    /// <summary>
    /// Rebuild-validation toggles keyed by RebuildCheck name; false disables that validation.
    /// Setup always fills in all known keys.
    /// </summary>
    public Dictionary<string, bool> RebuildChecks { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public static GenerationOverrides Current { get; private set; } = new();

    public static bool RebuildCheckEnabled(string name)
        => !Current.RebuildChecks.TryGetValue(name, out var enabled) || enabled;

    public static void Setup(string forcedComplex, bool preferGardens, Dictionary<string, bool> rebuildChecks)
    {
        Current = new GenerationOverrides
        {
            Daily = new RundownGenerationOverrides
            {
                ForcedComplex = forcedComplex,
                PreferGardens = preferGardens,
            },
            RebuildChecks = new Dictionary<string, bool>(rebuildChecks, StringComparer.OrdinalIgnoreCase),
        };

        foreach (var check in RebuildCheck.All)
            Current.RebuildChecks.TryAdd(check, true);

        var legacyPath = Path.Combine(
            Paths.BepInExRootPath, "config", "AutogenRundown.GenerationOverrides.json");

        if (File.Exists(legacyPath))
            Plugin.Logger.LogWarning(
                "AutogenRundown.GenerationOverrides.json is no longer read; these settings " +
                "moved to the [Advanced.Daily] and [Advanced.RebuildChecks] sections of " +
                "000-the_tavern-AutogenRundown.cfg");

        var disabled = RebuildCheck.All.Where(check => !RebuildCheckEnabled(check)).ToList();

        if (Current.Daily.ForcedComplex != "" || Current.Daily.PreferGardens || disabled.Any())
            Plugin.Logger.LogWarning(
                "Generation overrides active: " +
                $"Daily.ForcedComplex='{Current.Daily.ForcedComplex}', " +
                $"Daily.PreferGardens={Current.Daily.PreferGardens}, " +
                $"RebuildChecks disabled=[{string.Join(", ", disabled)}]");
    }
}
