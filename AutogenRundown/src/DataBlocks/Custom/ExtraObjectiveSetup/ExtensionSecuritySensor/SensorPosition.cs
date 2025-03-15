using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup.ExtensionSecuritySensor;

public class SensorPosition
{
    [JsonProperty("x")]
    public double X { get; set; }

    [JsonProperty("y")]
    public double Y { get; set; }

    [JsonProperty("z")]
    public double Z { get; set; }
}
