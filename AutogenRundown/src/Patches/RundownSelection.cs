using GameData;
using HarmonyLib;
using Il2CppSystem.Reflection;

namespace AutogenRundown.Patches;

// [HarmonyPatch(typeof(CM_RundownSelection), nameof(CM_RundownSelection.OnBtnPress))]
public class RundownSelection
{
    // static void Prefix(MethodBase __originalMethod)
    // {
    //     Plugin.Logger.LogDebug($"Method executed: {__originalMethod.DeclaringType.Name}.{__originalMethod.Name}");
    // }
}
