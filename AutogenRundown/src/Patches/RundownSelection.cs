using GameData;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch(typeof(CM_RundownSelection), nameof(CM_RundownSelection.OnBtnPress))]
public class RundownSelection
{

}
