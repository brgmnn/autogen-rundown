using AutogenRundown.Extensions;
using Newtonsoft.Json;

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

    /// <summary>
    /// Color = #db2727
    /// </summary>
    public static readonly Color MenuVisuals_SeasonalE = new()
    {
        Alpha = 1.0,
        Red = 0.858823538, // #db
        Green = 0.1509804, // #27
        Blue = 0.1509804,  // #27
    };

    /// <summary>
    /// Color = #276bdb
    /// </summary>
    public static readonly Color MenuVisuals_MonthlyE = new()
    {
        Alpha = 1.0,
        Red = 0.1509804,    // #27
        Green = 0.4190196,  // #6b
        Blue = 0.858823538, // #db
    };

    /// <summary>
    /// Color = #29d96b
    /// </summary>
    public static readonly Color MenuVisuals_WeeklyE = new()
    {
        Alpha = 1.0,
        Red = 0.158823538, // #29
        Green = 0.8509804, // #d9
        Blue = 0.4190196,  // #6b
    };

    /// <summary>
    /// Color = #d96b29
    /// </summary>
    public static readonly Color MenuVisuals_DailyE = new()
    {
        Alpha = 1.0,
        Red = 0.8509804,    // #d9
        Green = 0.4190196,  // #6b
        Blue = 0.158823538, // #29
    };
    #endregion

    #region Zone Security Sensors

    /// <summary>
    /// Default = Red scan color (similar to R8D1)
    /// </summary>
    public static readonly Color ZoneSensor_RedSensor = new()
    {
        Red = 0.9339623,
        Green = 0.1055641,
        Blue = 0.0,
        Alpha = 0.2627451
    };

    /// <summary>
    /// Default = Red scan color (similar to R8D1)
    ///
    /// 6fff70
    /// </summary>
    public static readonly Color ZoneSensor_InfectionGreenSensor = new()
    {
        Red = 0.4352941,
        Green = 1.0,
        Blue = 0.4352941,
        Alpha = 0.2627451
    };

    public static readonly Color ZoneSensor_EncryptedText = new()
    {
        Alpha = 0.8,
        Red = 0.85,
        Green = 0.31,
        Blue = 0.10,
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

    public virtual bool Equals(Color? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null || GetType() != other.GetType())
            return false;

        return Alpha.ApproxEqual(other.Alpha) &&
               Red.ApproxEqual(other.Red) &&
               Green.ApproxEqual(other.Green) &&
               Blue.ApproxEqual(other.Blue);
    }

    public override int GetHashCode() => HashCode.Combine(Alpha, Red, Green, Blue);

    /// <summary>
    /// Returns a new Color that is lightened by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to lighten (0.0 = no change, 1.0 = fully white). Default = 0.2</param>
    /// <returns>A new Color record with lightened values</returns>
    public Color Lighten(double amount = 0.2)
    {
        amount = Math.Clamp(amount, 0.0, 1.0);

        // Lerp towards white (1.0, 1.0, 1.0)
        var newRed = Red + (1.0 - Red) * amount;
        var newGreen = Green + (1.0 - Green) * amount;
        var newBlue = Blue + (1.0 - Blue) * amount;

        return this with
        {
            Red = newRed,
            Green = newGreen,
            Blue = newBlue
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="includeAlpha"></param>
    /// <returns></returns>
    public string ToHex(bool includeAlpha = false)
    {
        var a = (int)Math.Round(Math.Clamp(Alpha, 0.0, 1.0) * 255);
        var r = (int)Math.Round(Math.Clamp(Red, 0.0, 1.0) * 255);
        var g = (int)Math.Round(Math.Clamp(Green, 0.0, 1.0) * 255);
        var b = (int)Math.Round(Math.Clamp(Blue, 0.0, 1.0) * 255);

        return includeAlpha ? $"#{r:X2}{g:X2}{b:X2}{a:X2}" : $"#{r:X2}{g:X2}{b:X2}";
    }

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
