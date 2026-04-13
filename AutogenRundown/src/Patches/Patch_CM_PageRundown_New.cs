using System;
using System.Reflection;
using AutogenRundown.Managers;
using BepInEx.Unity.IL2CPP.Hook;
using CellMenu;
using HarmonyLib;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.Runtime;
using UnityEngine;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_CM_PageRundown_New
{
    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.Setup))]
    [HarmonyPostfix]
    private static void Post_Setup(CM_PageRundown_New __instance)
    {
        EventManager.RegisterPage(__instance);
        EventManager.UpdateRundown();
    }

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.PlaceRundown))]
    [HarmonyPostfix]
    private static void Post_PlaceRundown(CM_PageRundown_New __instance)
    {
        EventManager.UpdateRundown();
    }

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
    [HarmonyPostfix]
    private static void Post_OnEnable(CM_PageRundown_New __instance)
    {
        EventManager.UpdateRundown();
    }

    #region Native detour for GetExpIconLocalPos

    // IL2CPP ABI delegate for:
    //   private Vector3 GetExpIconLocalPos(int expNo, int expCount, Vector2 ovalSize)
    private unsafe delegate Vector3 d_GetExpIconLocalPos(
        IntPtr instance,
        int expNo,
        int expCount,
        Vector2 ovalSize,
        Il2CppMethodInfo* methodInfo);

    private static INativeDetour _detour;
    private static d_GetExpIconLocalPos _original;

    /// <summary>
    /// When a tier has a single expedition (expCount == 0), the game places it
    /// at ratio=0 which is the left edge of the arc. This detour moves it to
    /// ratio=0.5, the center/front of the ellipse: (0, -ovalSize.y, 0).
    /// </summary>
    public static unsafe void Setup()
    {
        var method = typeof(CM_PageRundown_New).GetMethod(
            "GetExpIconLocalPos",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var ptrField = Il2CppInteropUtils
            .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method);
        var methodInfoPtr = (IntPtr)ptrField.GetValue(null);
        nint functionPtr = *(nint*)(nint)methodInfoPtr;

        _detour = INativeDetour.CreateAndApply(
            functionPtr, Detour_GetExpIconLocalPos, out _original);
    }

    private static unsafe Vector3 Detour_GetExpIconLocalPos(
        IntPtr instance, int expNo, int expCount,
        Vector2 ovalSize, Il2CppMethodInfo* methodInfo)
    {
        if (expCount == 0)
            return new Vector3(0f, -ovalSize.y, 0f);

        return _original(instance, expNo, expCount, ovalSize, methodInfo);
    }

    #endregion
}
