﻿using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public record Color
{
    #region Menu Visuals
    public static readonly Color MenuVisuals = new()
    {
        Alpha = 1.0,
        Red = 0.4509804,
        Green = 0.7490196,
        Blue = 0.858823538,
    };

    public static readonly Color MenuVisuals_MonthlyE = new()
    {
        Alpha = 1.0,
        Red = 0.1509804,
        Green = 0.4190196,
        Blue = 0.858823538,
    };

    public static readonly Color MenuVisuals_WeeklyE = new()
    {
        Alpha = 1.0,
        Green = 0.8509804,
        Blue = 0.4190196,
        Red = 0.158823538,
    };

    public static readonly Color MenuVisuals_DailyE = new()
    {
        Alpha = 1.0,
        Red = 0.8509804,
        Green = 0.4190196,
        Blue = 0.158823538,
    };
    #endregion

    public static readonly Color White = new() { Red = 1.0, Green = 1.0, Blue = 1.0, Alpha = 1.0 };

    #region Fog Preset colors
    /// <summary>
    /// Preset color used in several of the R8D1 fog colors
    /// </summary>
    public static readonly Color InfectiousFog_R8D1 = new()
        {
            Red = 0.356862754,
            Green = 1.0,
            Blue = 0.545098066,
            Alpha = 0.03137255
        };
    #endregion

    #region Properties

    /// <summary>
    /// Alpha channel. Range 0.0 to 1.0
    ///
    /// Default = 1.0
    /// </summary>
    [JsonProperty("a")]
    public double Alpha { get; set; } = 1.0;

    /// <summary>
    /// Red channel. Range 0.0 to 1.0
    ///
    /// Default = 1.0
    /// </summary>
    [JsonProperty("r")]
    public double Red { get; set; } = 1.0;

    /// <summary>
    /// Green channel. Range 0.0 to 1.0
    ///
    /// Default = 1.0
    /// </summary>
    [JsonProperty("g")]
    public double Green { get; set; } = 1.0;

    /// <summary>
    /// Blue channel. Range 0.0 to 1.0
    ///
    /// Default = 1.0
    /// </summary>
    [JsonProperty("b")]
    public double Blue { get; set; } = 1.0;

    #endregion
}
