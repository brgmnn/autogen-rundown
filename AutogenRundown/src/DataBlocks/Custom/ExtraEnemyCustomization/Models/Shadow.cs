namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models;

/// <summary>
/// Applies Shadow model to enemies
/// </summary>
public record Shadow : CustomRecord
{
    /// <summary>
    /// WARNING: Shadow Eggsack is completely Invisible, not even a single shadow!
    ///
    /// Default = false
    /// </summary>
    public bool IncludeEggSack { get; set; } = false;

    /// <summary>
    /// No comment here
    ///
    /// Default = true
    /// </summary>
    public bool RequireTagForDetection { get; set; } = true;

    /// <summary>
    /// Fully Invisible Shadows (Ghosts)! You can't see them even with flashlight.
    ///
    /// Default = false
    /// </summary>
    public bool FullyInvisible { get; set; } = false;

    /// <summary>
    /// Accepted enums:
    /// * LegacyShadows
    /// * NewShadows
    ///
    /// Default = LegacyShadows
    /// </summary>
    public string Type { get; set; } = "LegacyShadows";

    /// <summary>
    /// If "NewShadows", is visible in thermal sights
    ///
    /// Default = true
    /// </summary>
    public bool IncludeThermals { get; set; } = true;

    /// <summary>
    /// If "NewShadows", tumors will become visible when viewed from the back
    ///
    /// Default = true
    /// </summary>
    public bool TumorVisibleFromBehind { get; set; } = true;
}
