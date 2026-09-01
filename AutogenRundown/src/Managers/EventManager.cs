using CellMenu;

namespace AutogenRundown.Managers;

public static class EventManager
{
    private static CM_PageRundown_New? page;

    public static event Action OnSelectRundown = delegate { };

    public static event Action OnClearRundown = delegate { };

    // Check when Rundown_Surface_SelectionALT_R1 becomes visible
    public static event Action OnScreen_RundownSelection = delegate { };

    // Check when GUIX_layer_Tier_1 becomes visible
    public static event Action OnScreen_ViewRundown = delegate { };

    public static event Action<PluginRundown> OnRundownUpdate = delegate { };

    public static event Action OnFactoryDone = delegate { };

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
        var rundown = PluginRundowns.FromRundownKey(RundownManager.ActiveRundownKey);

        Plugin.Logger.LogDebug($"Active rundown = {rundown}");

        // Invoked one handler at a time. A raw multicast invoke would let a single throwing
        // subscriber abort every later one and escape into the game method we were called
        // from -- UpdateRundown() runs inside CM_PageRundown_New.PlaceRundown/OnEnable
        // postfixes, so an escaping exception skips the rest of those postfixes.
        foreach (var handler in OnRundownUpdate.GetInvocationList())
        {
            try
            {
                ((Action<PluginRundown>)handler).Invoke(rundown);
            }
            catch (Exception error)
            {
                Plugin.Logger.LogWarning($"OnRundownUpdate handler failed: {error}");
            }
        }
    }
}
