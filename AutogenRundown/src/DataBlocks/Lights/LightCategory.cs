namespace AutogenRundown.DataBlocks.Light;

public enum LightCategory
{
    /// <summary>
    /// General Lights most common
    /// </summary>
    General = 0,

    /// <summary>
    /// Special Lights second common
    /// </summary>
    Special = 1,

    /// <summary>
    /// Often used on flickering light, not that common
    /// </summary>
    Emergency = 2,

    /// <summary>
    /// Mostly used on reactor
    /// </summary>
    Independent = 3,

    /// <summary>
    /// Weak door lights
    /// </summary>
    Door = 4,

    /// <summary>
    /// Zone sign lights
    /// </summary>
    Sign = 5,

    /// <summary>
    /// mostly Security door lights
    /// </summary>
    DoorImportant = 6
}
