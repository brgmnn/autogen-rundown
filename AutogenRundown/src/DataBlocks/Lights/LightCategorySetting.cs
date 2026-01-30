using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Light;

public record LightCategorySetting
{
    #region Properties

    /// <summary>
    /// Whether the light is on. I presume this is used with things like reactor lights on.
    /// </summary>
    public bool On { get; set; } = true;

    /// <summary>
    /// Which category of lights in the geo this will be applied to
    /// </summary>
    public LightCategory Category { get; set; } = LightCategory.General;

    /// <summary>
    /// Chance it's on? Or selected? Unclear
    /// </summary>
    public double Chance { get; set; } = 1.0;

    /// <summary>
    /// Chance light is flashing broken
    /// </summary>
    public double ChanceBroken { get; set; } = 0.0;

    /// <summary>
    /// What color this light is
    /// </summary>
    public Color Color { get; set; } = new();

    /// <summary>
    /// Seems like it might be how bright to multiply up this light
    /// </summary>
    [JsonProperty("IntensityMul")]
    public double Intensity { get; set; } = 1.0;

    /// <summary>
    /// Weighted chance of being selected
    /// </summary>
    public double Weight { get; set; } = 1.0;

    #endregion

    public static LightCategorySetting Off(LightCategory category) => new() { Category = category, Chance = 0.0 };

    public static LightCategorySetting SecurityDoor_White => new()
    {
        Color = new() { Red = 0.9255541, Green = 1.0, Blue = 0.7877358 },
        Category = LightCategory.DoorImportant,
        Chance = 1.0,
        Intensity = 1.0,
        On = true
    };
}
