namespace AutogenRundown.DataBlocks;

public class Intel
{
    public const string Info = ":://<color=cyan>INFO</color>";

    public const string Warning = ":://WARNING";

    public const string Error = ":://ERROR";

    public string Zone(int number) => $"<color=orange>ZONE {number}</color>";
}

