using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CellMenu;
using HarmonyLib;

namespace AutogenRundown
{
    internal class SeedButton
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
        public static void OnEnable()
        {
        }
    }
}
