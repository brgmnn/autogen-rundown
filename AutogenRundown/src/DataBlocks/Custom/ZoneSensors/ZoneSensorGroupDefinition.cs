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
    public Color Color { get; set; } = Color.ZoneSensor_RedSensor;

    /// <summary>
    /// Text displayed on the sensor.
    /// If null, a random corrupted text will be selected for each sensor.
    /// Default = null
    /// </summary>
    public string? Text { get; set; } = null;

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
    /// When true, text cycles through hex characters with periodic reveals of actual text.
    /// Default = false
    /// </summary>
    public bool EncryptedText { get; set; } = false;

    /// <summary>
    /// Color for the encrypted hex text when EncryptedText is true.
    /// Default = Orange
    /// </summary>
    public Color EncryptedTextColor { get; set; } = Color.ZoneSensor_EncryptedText;
        // new()
        // {
        //     Red = 0.0,
        //     Green = 0.549,
        //     Blue = 0.0,
        //     Alpha = 0.8
        // };

    /// <summary>
    /// When true, no text is displayed on the sensor.
    /// Takes precedence over EncryptedText.
    /// Default = false
    /// </summary>
    public bool HideText { get; set; } = false;

    /// <summary>
    /// When true, each sensor triggers independently and only that sensor is removed.
    /// Events fire for each sensor triggered. Default = false (group triggers once).
    /// </summary>
    public bool TriggerEach { get; set; } = false;

    /// <summary>
    /// Number of patrol points for sensor movement.
    /// 1 = stationary, 2 = patrol between start and one point, 3+ = multi-point patrol.
    /// Default = 1
    /// </summary>
    public int Moving { get; set; } = 1;

    /// <summary>
    /// Movement speed in units per second when Moving is true.
    /// Default = 1.5
    /// </summary>
    public double Speed { get; set; } = 1.5;

    /// <summary>
    /// Minimum distance from NavMesh edges for path waypoints when Moving is true.
    /// Helps prevent sensors from hugging walls at corners.
    /// Default = 0.3
    /// </summary>
    public double EdgeDistance { get; set; } = 0.3;

    /// <summary>
    /// Height of the sensor visual. Combined with Radius for vertical offset.
    /// Default = 0.6
    /// </summary>
    public double Height { get; set; } = 0.8;

}
