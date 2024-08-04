using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Terminals;

public record TerminalOutput
{
    [JsonProperty("LineType")]
    public LineType Type { get; set; } = LineType.Normal;

    public string Output { get; set; } = "";

    public double Time { get; set; } = 0.0;
}
