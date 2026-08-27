using AutogenRundown.DataBlocks.Enums;
using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown;

/// <summary>
/// Canonical names for the level rebuild validations that can be toggled via
/// GenerationOverrides.RebuildChecks.
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

    [JsonIgnore]
    public Complex? ForcedComplexValue =>
        Enum.TryParse<Complex>(ForcedComplex, true, out var complex) &&
        Enum.IsDefined(typeof(Complex), complex)
            ? complex
            : null;
}

/// <summary>
/// Hidden generation overrides, read from a JSON settings file in the profile's config folder.
/// The file does not exist by default and there is no BepInEx config entry for it, so the
/// overrides can only be enabled by hand-creating the file:
///
///     BepInEx/config/AutogenRundown.GenerationOverrides.json
///     {
///         "Daily": { "ForcedComplex": "Service", "PreferGardens": true },
///         "RebuildChecks": { "NullSourceExpander": false }
///     }
///
/// RebuildChecks toggles the level rebuild validations (see RebuildCheck for the valid keys);
/// a check set to false logs what it would have done instead of triggering a rebuild. All
/// checks default to enabled. The file must match across all players in a lobby — the checks
/// run in lockstep on host and clients.
///
/// A missing file, missing fields, or malformed JSON all leave generation at vanilla behavior.
/// </summary>
public record GenerationOverrides
{
    public RundownGenerationOverrides Daily { get; set; } = new();

    /// <summary>
    /// Rebuild-validation toggles keyed by RebuildCheck name; false disables that validation.
    /// Keys absent from the settings file are backfilled to true by Load().
    /// </summary>
    public Dictionary<string, bool> RebuildChecks { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public static GenerationOverrides Current { get; private set; } = new();

    public static bool RebuildCheckEnabled(string name)
        => !Current.RebuildChecks.TryGetValue(name, out var enabled) || enabled;

    private static string SettingsPath =>
        Path.Combine(Paths.BepInExRootPath, "config", "AutogenRundown.GenerationOverrides.json");

    public static void Load()
    {
        if (File.Exists(SettingsPath))
        {
            try
            {
                var json = File.ReadAllText(SettingsPath);
                Current = JsonConvert.DeserializeObject<GenerationOverrides>(json) ?? new();

                var unknown = Current.RebuildChecks.Keys
                    .Where(key => !RebuildCheck.All.Contains(key, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (unknown.Any())
                    Plugin.Logger.LogWarning(
                        $"Unknown RebuildChecks keys: {string.Join(", ", unknown)}. " +
                        $"Valid keys: {string.Join(", ", RebuildCheck.All)}");

                var disabled = RebuildCheck.All.Where(check => !RebuildCheckEnabled(check));

                Plugin.Logger.LogWarning(
                    "Generation overrides active: " +
                    $"Daily.ForcedComplex='{Current.Daily.ForcedComplex}', " +
                    $"Daily.PreferGardens={Current.Daily.PreferGardens}, " +
                    $"RebuildChecks disabled=[{string.Join(", ", disabled)}]");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogWarning($"Failed to load generation overrides: {ex.Message}");
            }
        }

        // All rebuild checks are enabled by default; only an explicit false in the file
        // disables one.
        foreach (var check in RebuildCheck.All)
            Current.RebuildChecks.TryAdd(check, true);
    }
}
