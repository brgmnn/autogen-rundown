using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.ZoneData
{
    public class TerminalPlacement
    {
        public ZonePlacementWeights PlacementWeights { get; set; } = ZonePlacementWeights.EvenlyDistributed;

        #region Less used
        public int AreaSeedOffset { get; set; } = 0;
        public int MarkerSeedOffset { get; set; } = 0;
        public JArray LocalLogFiles { get; set; } = new JArray();

        // TODO: implement
        public List<string> UniqueCommands { get; set; } = new List<string>();

        // TODO: Implement this for more complex terminal usage
        public object StartingStateData { get; set; } = new
        {
            StartingState = 0,
            UseCustomInfoText = false,
            CustomInfoText = 0,
            KeepShowingLocalLogCount = 0,
            AudioEventEnter = 0,
            AudioEventExit = 0,
            PasswordProtected = false,
            PasswordHintText = "Password Required.",
            GeneratePassword = false,
            PasswordPartCount = 1,
            ShowPasswordLength = true,
            ShowPasswordPartPositions = false,
            TerminalZoneSelectionDatas = new JArray()
        };
        #endregion
    }
}
