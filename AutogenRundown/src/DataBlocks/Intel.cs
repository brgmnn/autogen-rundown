namespace AutogenRundown.DataBlocks;

/// <summary>
/// The game can also sub in strings for us:
///
///     * [ALL_ITEMS] - List of items for the objective
///     * [CELL_AREA]
///     * [CELL_NAME]
///     * [CELL_ZONE]
///     * [EXTRACTION_ZONE] - Where players should extract
///     * [ITEM_SERIAL]
///     * [ITEM_ZONE] - Item to find? TODO: does this work?
///     * [KEYCARD_AREA] -
///     * [KEYCARD_NAME] - Name of keycard
///     * [KEYCARD_ZONE] - Keycard zone
///     * [TARGET_ZONE] - Used with Keycards?
///     * [ZONE_{num}] - Will replace it with the correct zone number and formatting
/// </summary>
public class Intel
{
    public const string Info = ":://<color=cyan>INFO</color>";

    public const string Warning = ":://WARNING";

    public const string Error = ":://ERROR";

    public static string Zone(int number) => $"<color=orange>ZONE {number}</color>";
}

