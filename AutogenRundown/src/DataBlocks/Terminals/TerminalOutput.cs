using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Terminals;

public record TerminalOutput
{
    [JsonProperty("LineType")]
    public LineType Type { get; set; } = LineType.Normal;

    public string Output { get; set; } = "";

    public double Time { get; set; } = 0.0;

    #region Prebuilt

    /// <summary>
    /// Text = "Authenticating..."
    /// </summary>
    public static readonly TerminalOutput Authenticating = new()
    {
        Output = "Authenticating...",
        Type = LineType.SpinningWaitNoDone,
        Time = 1.5
    };

    /// <summary>
    /// Text = "Authenticating with BIOCOM..."
    /// </summary>
    public static readonly TerminalOutput AuthenticatingWithBiocom = new()
    {
        Output = "Authenticating with BIOCOM...",
        Type = LineType.SpinningWaitNoDone,
        Time = 2.0
    };

    /// <summary>
    /// Text = "Confirming valid terminal ID"
    /// </summary>
    public static readonly TerminalOutput ConfirmingTerminal = new()
    {
        Output = "Confirming valid terminal ID",
        Type = LineType.Normal,
        Time = 1.5
    };

    /// <summary>
    /// Text = "Done."
    /// </summary>
    public static readonly TerminalOutput Done = new()
    {
        Output = "Done.",
        Type = LineType.Normal,
        Time = 1.0
    };

    #endregion
}
