using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ZoneSensors;

/// <summary>
/// Defines a group of sensors to place within a zone.
/// All sensors in a group share the same trigger events.
/// </summary>
public record ZoneSensorGroupDefinition
{
    /// <summary>
    /// Which area within the zone to place sensors.
    /// -1 means random areas will be selected.
    /// Default = -1
    /// </summary>
    public int AreaIndex { get; set; } = -1;

    /// <summary>
    /// Number of sensors to place in this group.
    /// Default = 1
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// Detection radius of each sensor.
    /// Default = 2.3
    /// </summary>
    public double Radius { get; set; } = 2.3;

    /// <summary>
    /// Color of the sensor visual ring.
    /// Default = Red scan color (similar to R8D1)
    /// </summary>
    public Color Color { get; set; } = new()
    {
        Red = 0.9339623,
        Green = 0.1055641,
        Blue = 0.0,
        Alpha = 0.2627451
    };

    /// <summary>
    /// Text displayed on the sensor.
    /// Default = "S:_EC/uR_ITY S:/Ca_N"
    /// </summary>
    public string Text { get; set; } = "S:_EC/uR_ITY S:/Ca_N";

    /// <summary>
    /// Color of the sensor text.
    /// Default = Light gray
    /// </summary>
    public Color TextColor { get; set; } = new()
    {
        Red = 0.8862745,
        Green = 0.9019607,
        Blue = 0.8980392,
        Alpha = 0.7098039
    };

    /// <summary>
    /// When true, each sensor triggers independently and only that sensor is removed.
    /// Events fire for each sensor triggered. Default = false (group triggers once).
    /// </summary>
    public bool TriggerEach { get; set; } = false;

    /// <summary>
    /// Generates randomized glitchy sensor text.
    /// </summary>
    public static string GenText()
        => Generator.Pick(new List<string>
        {
            "Se@#$urity Sc3AN",
            "S:_EC/uR_ITY S:/Ca_N"
        })!;
}
