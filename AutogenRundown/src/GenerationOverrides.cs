using AutogenRundown.DataBlocks.Enums;
using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown;

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
///     { "Daily": { "ForcedComplex": "Service", "PreferGardens": true } }
///
/// A missing file, missing fields, or malformed JSON all leave generation at vanilla behavior.
/// </summary>
public record GenerationOverrides
{
    public RundownGenerationOverrides Daily { get; set; } = new();

    public static GenerationOverrides Current { get; private set; } = new();

    private static string SettingsPath =>
        Path.Combine(Paths.BepInExRootPath, "config", "AutogenRundown.GenerationOverrides.json");

    public static void Load()
    {
        if (!File.Exists(SettingsPath))
            return;

        try
        {
            var json = File.ReadAllText(SettingsPath);
            Current = JsonConvert.DeserializeObject<GenerationOverrides>(json) ?? new();

            Plugin.Logger.LogWarning(
                "Generation overrides active: " +
                $"Daily.ForcedComplex='{Current.Daily.ForcedComplex}', " +
                $"Daily.PreferGardens={Current.Daily.PreferGardens}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"Failed to load generation overrides: {ex.Message}");
        }
    }
}
