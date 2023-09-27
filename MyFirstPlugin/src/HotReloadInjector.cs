using CellMenu;
using HarmonyLib;

namespace MyFirstPlugin
{
    public class HotReloadInjector
    {
        /// <summary>
        /// Gets called on CM_PageRundown_New.OnEnable if the BepInEx config for HotReloading is true.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
        public static void OnEnable() => HotReloader.Setup();
    }
}
