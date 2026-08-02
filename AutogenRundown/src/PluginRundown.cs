namespace AutogenRundown;

public enum PluginRundown
{
    None = 0,

    Daily,
    Weekly,
    Monthly,
    Seasonal,

    Solo,
    Duo,
}

public static class PluginRundowns
{
    /// <summary>
    /// Rundowns generated with log archives. Daily is excluded, it re-rolls every day so
    /// there is nothing worth tracking across sessions.
    /// </summary>
    public static readonly PluginRundown[] WithLogs =
    {
        PluginRundown.Weekly,
        PluginRundown.Monthly,
        PluginRundown.Seasonal,
        PluginRundown.Solo,
        PluginRundown.Duo,
    };

    /// <summary>
    /// Maps the game's rundown key ("Local_1" ... "Local_6") onto our rundown enum. The
    /// numeric suffix is the rundown's persistent id, which the enum values mirror, so this
    /// must be kept in step with Rundown.R_* whenever a rundown is added.
    /// </summary>
    public static PluginRundown FromRundownKey(string? rundownKey)
        => rundownKey switch
        {
            "Local_1" => PluginRundown.Daily,
            "Local_2" => PluginRundown.Weekly,
            "Local_3" => PluginRundown.Monthly,
            "Local_4" => PluginRundown.Seasonal,
            "Local_5" => PluginRundown.Solo,
            "Local_6" => PluginRundown.Duo,

            _ => PluginRundown.None
        };
}
