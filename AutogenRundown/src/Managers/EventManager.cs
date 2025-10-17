using CellMenu;

namespace AutogenRundown.Managers;

public static class EventManager
{
    private static CM_PageRundown_New page;

    public static event Action OnSelectRundown;

    public static event Action OnClearRundown;

    // Check when Rundown_Surface_SelectionALT_R1 becomes visible
    public static event Action OnScreen_RundownSelection;

    // Check when GUIX_layer_Tier_1 becomes visible
    public static event Action OnScreen_ViewRundown;

    public static event Action<PluginRundown> OnRundownUpdate;

    public static void Setup()
    {
        RundownManager.OnRundownProgressionUpdated += new Action(UpdateRundown);
    }

    public static void RegisterPage(CM_PageRundown_New newPage)
    {
        page = newPage;
    }

    public static void UpdateRundown()
    {
        var rundown = RundownManager.ActiveRundownKey switch
        {
            "Local_1" => PluginRundown.Daily,
            "Local_2" => PluginRundown.Weekly,
            "Local_3" => PluginRundown.Monthly,
            "Local_4" => PluginRundown.Seasonal,

            _ => PluginRundown.None
        };

        Plugin.Logger.LogDebug($"Active rundown = {rundown}");

        OnRundownUpdate?.Invoke(rundown);
    }
}
