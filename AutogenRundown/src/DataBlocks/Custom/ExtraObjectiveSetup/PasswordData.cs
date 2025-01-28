namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class PasswordData
{
    public bool PasswordProtected { get; set; } = false;

    public string PasswordHintText { get; set; } = "Password Required.";

    public bool GeneratePassword { get; set; } = true;

    public int PasswordPartCount { get; set; } = 1;

    public bool ShowPasswordLength { get; set; } = false;

    public bool ShowPasswordPartPositions { get; set; } = false;

    public List<List<ZoneSelectionData>> TerminalZoneSelectionDatas { get; set; } = new();
}
