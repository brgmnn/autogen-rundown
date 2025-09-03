using AutogenRundown.DataBlocks.Terminals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.ZoneData;

public class TerminalPlacement
{
    public ZonePlacementWeights PlacementWeights { get; set; } = ZonePlacementWeights.EvenlyDistributed;

    public List<CustomTerminalCommand> UniqueCommands { get; set; } = new();

    public TerminalStartingState StartingStateData { get; set; } = new();

    /// <summary>
    /// Defines the logs stored on this terminal
    /// </summary>
    [JsonProperty("LocalLogFiles")]
    public List<LogFile> LogFiles { get; set; } = new();

    #region Less used

    public int AreaSeedOffset { get; set; } = 0;
    public int MarkerSeedOffset { get; set; } = 0;

    #endregion
}
