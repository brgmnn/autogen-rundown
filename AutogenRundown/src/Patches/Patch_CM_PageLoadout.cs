using AutogenRundown.Managers;
using CellMenu;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_CM_PageLoadout
{
    /// <summary>
    /// Drives the build failure popup.
    ///
    /// This exists instead of an injected MonoBehaviour updater. The first version of this feature
    /// used one, and it hard crashed the game with:
    ///
    ///     System.AccessViolationException: Attempted to read or write protected memory
    ///        at Il2CppInterop.Runtime.IL2CPP.il2cpp_runtime_invoke(...)
    ///        at DynamicClass.DMD&lt;CellMenu.CM_PageBase::Update&gt;(CellMenu.CM_PageBase)
    ///        at DynamicClass.(il2cpp -> managed) Update(IntPtr, Il2CppMethodInfo*)
    ///
    /// The injected type's il2cpp -> managed Update bridge was dispatching into CM_PageBase.Update
    /// with a mismatched instance. Injected updaters are only safe here in-level (see
    /// ZoneSensorManagerUpdater); one that lives on the main menu ticks alongside the CM_Page*
    /// components and collides.
    ///
    /// CM_PageLoadout.Update is the right host instead: a substantial unique body (no ICF fold
    /// hazard), it is the page players land on after an aborted drop, and it is the method that
    /// runs ProcessBoosterImplantEvents / TryShowNextNewVanityItemPopup -- so a postfix is ordered
    /// after the game's own popups on the same frame.
    /// </summary>
    [HarmonyPatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.Update))]
    [HarmonyPostfix]
    private static void Post_Update(CM_PageLoadout __instance)
    {
        BuildFailureManager.TryShowPopup();

        GateDropButton(__instance);
    }

    /// <summary>
    /// Stops the host dropping back into a level we already know cannot be generated.
    ///
    /// After an abort the lobby still has the failed expedition selected, so the DROP button would
    /// happily send everyone straight back in. We gate the button rather than clearing the
    /// selection: RundownManager.MasterSelectActiveExpedition is master only, and a null
    /// ActiveExpedition is read in several places in the lobby UI.
    ///
    /// UpdateReadyState() sets the button state and is called from inside Update() on a 1 second
    /// timer, so this postfix always runs after it and re-asserts the override.
    /// </summary>
    private static void GateDropButton(CM_PageLoadout page)
    {
        try
        {
            var expedition = RundownManager.ActiveExpedition;

            if (expedition == null || !BuildFailureManager.IsLocked(expedition.LevelLayoutData))
                return;

            var drop = page.m_dropButton;

            // Vanilla hides the drop button until everyone is ready -- don't resurrect it
            if (drop == null || !drop.gameObject.activeSelf)
                return;

            drop.SetText("EXPEDITION UNREACHABLE\n<size=12px><align=center>Select a different expedition</align></size>");
            drop.SetButtonEnabled(false);
            drop.ShowBox = false;
        }
        catch (Exception error)
        {
            Plugin.Logger.LogWarning($"[BuildFailure] Could not gate drop button: {error.Message}");
        }
    }
}
