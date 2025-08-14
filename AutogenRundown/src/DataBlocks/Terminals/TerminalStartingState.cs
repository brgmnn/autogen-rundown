using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

namespace AutogenRundown.DataBlocks.Terminals;

public record TerminalStartingState
{
    #region Properties

    public TerminalState StartingState { get; set; } = TerminalState.Sleeping;

    public bool UseCustomInfoText { get; set; } = false;

    public string CustomInfoText { get; set; } = "";

    public int KeepShowingLocalLogCount { get; set; } = 0;

    public int AudioEventEnter { get; set; } = 0;

    public int AudioEventExit { get; set; } = 0;

    public bool PasswordProtected { get; set; } = false;

    public string PasswordHintText { get; set; } = "Password Required.";

    public bool GeneratePassword { get; set; } = false;

    public int PasswordPartCount { get; set; } = 1;

    public bool ShowPasswordLength { get; set; } = true;

    public bool ShowPasswordPartPositions { get; set; } = false;

    public List<List<ZoneSelectionData>> TerminalZoneSelectionDatas { get; set; } = new();

    #endregion
}
