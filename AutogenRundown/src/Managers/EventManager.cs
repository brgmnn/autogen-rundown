namespace AutogenRundown.Managers;

public static class EventManager
{
    public static event Action OnSelectRundown;

    public static event Action OnClearRundown;

    // Check when Rundown_Surface_SelectionALT_R1 becomes visible
    public static event Action OnScreen_RundownSelection;

    // Check when GUIX_layer_Tier_1 becomes visible
    public static event Action OnScreen_ViewRundown;
}
