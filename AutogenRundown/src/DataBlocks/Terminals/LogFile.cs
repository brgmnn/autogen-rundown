using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Terminals;

public class LogFile
{
    /// <summary>
    /// Filename of the log
    /// </summary>
    public string FileName { set; get; } = "Log.txt";


    /// Reworks the display string both on screen and in logs to wrap for longer length codes
    ///
    /// For log files, the text will be automaticall wrapped at 43 chars.
    /// So make sure we wrap the logs appropriately to fit how we want
    /// for the terminal. Hence why we have separate logic for when it's
    /// on a log file. Below is the divider surrounding the codes:
    ///     -------------------------------------------
    ///     A01:four  A01:four  A01:four  A01:four
    ///     A02:fives  A02:fives  A02:fives  A02:fives
    ///     A03:sixsix  A03:sixsix  A03:sixsix
    ///     A04:sevenis  A04:sevenis  A04:sevenis
    ///

    /// <summary>
    /// File contents of the logs
    ///
    /// For log files, the text will be automatically wrapped at 43 chars.
    /// So make sure we wrap the logs appropriately to fit how we want
    /// for the terminal. Hence, why we have separate logic for when it's
    /// on a log file. Below is the divider surrounding the codes:
    ///     -------------------------------------------
    ///     A01:four  A01:four  A01:four  A01:four
    ///     A02:fives  A02:fives  A02:fives  A02:fives
    ///     A03:sixsix  A03:sixsix  A03:sixsix
    ///     A04:sevenis  A04:sevenis  A04:sevenis
    /// </summary>
    [JsonIgnore]
    public Text FileContent { set; get; } = Text.None;

    [JsonProperty("FileContent")]
    public uint FileContentId => FileContent.PersistentId;

    /// <summary>
    ///
    /// </summary>
    public Sound AttachedAudioFile { set; get; } = Sound.None;

    /// <summary>
    /// Displayed audio log size in bytes
    /// </summary>
    public int AttachedAudioByteSize { set; get; } = 0;

    /// <summary>
    /// Player dialog to play after audio finishes
    /// </summary>
    public Sound PlayerDialogToTriggerAfterAudio { set; get; } = Sound.None;
}
