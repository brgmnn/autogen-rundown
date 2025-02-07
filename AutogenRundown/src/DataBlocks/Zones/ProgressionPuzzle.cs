namespace AutogenRundown.DataBlocks.Zones;

/// <summary>
/// Defines puzzles required to open security doors.
///
/// https://gtfo-modding.gitbook.io/wiki/reference/nested-types/progressionpuzzledata
/// </summary>
public class ProgressionPuzzle
{
    public static readonly ProgressionPuzzle Locked = new()
    {
        PuzzleType = ProgressionPuzzleType.Locked,
        CustomText = "<color=red>://ERROR: Door in emergency lockdown, unable to operate.</color>"
    };

    /// <summary>
    /// The type of the puzzle.
    /// </summary>
    public ProgressionPuzzleType PuzzleType { get; set; } = ProgressionPuzzleType.None;

    /// <summary>
    /// Custom text for the door.
    ///
    /// Only used for Locked_No_Key type.
    /// </summary>
    public string CustomText { get; set; } = "";

    /// <summary>
    /// Number of cells to place.
    ///
    /// Only used for PowerGenerator_And_PowerCell type.
    ///
    /// Note that cells can also be spawned by big pickup distribution, arguably with better
    /// random placement results.
    /// </summary>
    public int PlacementCount { get; set; } = 1;

    /// <summary>
    /// Alternative placement locations. Picked randomly from the list.
    /// </summary>
    public List<ZonePlacementData> ZonePlacementData { get; set; } = new List<ZonePlacementData>();
}
