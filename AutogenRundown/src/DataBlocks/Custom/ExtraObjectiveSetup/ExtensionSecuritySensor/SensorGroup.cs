namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup.ExtensionSecuritySensor;

public record Sensor
{
    public static string GenText()
        => Generator.Pick(new List<string>
        {
            "Se@#$urity Sc3AN",
            "S:_EC/uR_ITY S:/Ca_N"
        })!;

    public SensorPosition Position { get; set; } = new();

    /// <summary>
    /// Default = 5.0
    /// </summary>
    public double Radius { get; set; } = 5.0;

    /// <summary>
    /// Default = Red scan color in R8D1
    /// </summary>
    public Color Color { get; set; } = new()
    {
        Red = 0.9339623,
        Green = 0.1055641,
        Blue = 0.0,
        Alpha = 0.2627451
    };

    /// <summary>
    /// Default = Se@#$urity Sc3AN or S:_EC/uR_ITY S:/Ca_N
    /// </summary>
    public string Text { get; set; } = "S:_EC/uR_ITY S:/Ca_N"; // "Se@#$urity Sc3AN";

    public Color TextColor { get; set; } = new()
    {
        Red = 0.8862745,
        Green = 0.9019607,
        Blue = 0.8980392,
        Alpha = 0.7098039
    };

    /// <summary>
    /// Always set to BASIC
    ///
    /// Other option is to do MOVABLE for T-scan like behavior
    /// </summary>
    public string SensorType { get; set; } = "BASIC";

    /// <summary>
    /// This field is only used when SensorType == "MOVABLE"
    /// Movement speed multiplier to the movable sensor,
    /// would be applied if set to a positive value.
    /// </summary>
    public double MovingSpeedMulti { get; set; } = -1.0;

    /// <summary>
    /// This field is only used when SensorType == "MOVABLE"
    /// Configure the traveling path of the moving sensor (or T-sensor, if you are more used to 'T-scan').
    /// The "Position" field above is used as the first point (or in other words, starting point)
    /// of the movable sensor.
    /// After traveling through all the positions, the sensor would either instantly TP back to the starting point,
    /// or travel back to the starting point in a straight line.
    /// You ought to make "Position" and "MovingPosition" constitute a close path.
    /// </summary>
    public List<SensorPosition> MovingPosition { get; set; } = new();
}
