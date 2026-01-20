using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

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
    // private string[] distortedStrings = new string[9]
    // {
    //     " %#",
    //     "¤¤ ",
    //     "%#%",
    //     "###",
    //     "#..",
    //     "NUL",
    //     "% &",
    //     "¤¤¤",
    //     "___"
    // };
    // private string[] correctStrings = new string[8]
    // {
    //     " 03",
    //     "/16",
    //     "/20",
    //     "20 ",
    //     " 04",
    //     "/03",
    //     "/20",
    //     "20 "
    // };

    public const string Info = ":://<color=cyan>INFO</color>";

    public const string Warning = ":://WARNING";

    public const string Error = ":://ERROR";

    public static string Number(int number)
        => number switch
        {
            1 => "one",
            2 => "two",
            3 => "three",
            4 => "four",
            5 => "five",
            6 => "six",
            7 => "seven",
            8 => "eight",
            9 => "nine",
            10 => "ten",

            _ => "many"
        };

    public static string Terminal(ZoneNode node, int terminalIndex = 0)
    {
        var layer = node.Bulkhead switch
        {
            Bulkhead.Main => 0,
            Bulkhead.Extreme => 1,
            Bulkhead.Overload => 2,
            _ => 0
        };

        return $"[TERMINAL_0_{layer}_{node.ZoneNumber}_{terminalIndex}]";
    }

    public static string ZoneRaw(int number, bool underscore = false)
        => $"ZONE{(underscore ? "_" : " ")}{number}";

    public static string ZoneRaw(Zone zone, bool underscore = false)
        => ZoneRaw(zone.layout.ZoneAliasStart + zone.LocalIndex, underscore);

    public static string Zone(int number, bool underscore = false)
        => $"<color=orange>ZONE{(underscore ? "_" : " ")}{number}</color>";

    public static string Zone(Zone zone, bool underscore = false)
        => Zone(zone.layout.ZoneAliasStart + zone.LocalIndex, underscore);

    public static string Zone(ZoneNode node, LayoutPlanner planner, bool underscore = false)
    {
        var zone = planner.GetZone(node);

        return zone is not null ? Zone(zone, underscore) : "<i>No Zone</i>";
    }
}
