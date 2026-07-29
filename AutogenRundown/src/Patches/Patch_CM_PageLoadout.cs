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
    private static void Post_Update()
    {
        BuildFailureManager.TryShowPopup();
    }
}
