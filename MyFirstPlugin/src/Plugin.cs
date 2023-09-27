using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using GameData;
using CellMenu;
using HarmonyLib;
using GTFO.API;
using Il2CppInterop.Runtime.Injection;
using Newtonsoft.Json;
using BepInEx.Logging;
using MyFirstPlugin.Loaders;
using UnityEngine.Diagnostics;

namespace MyFirstPlugin;

/**
 * Objective types
 * - Gather Items
 * - Establish Uplink
 * - Retrieve Item
 * - Retrieve HSU
 * - Input Command
 * - Clear Path
 * - Distribute Powercells
 * - Startup Reactor
 * - Activate Generator Cluster
 * - Process Item
 * - Survive Warden Protocol
 * - Reactor Shutdown
 */
class PatchRundown
{
    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
    [HarmonyPostfix]
    public static void MyPatch(CM_PageRundown_New __instance)
    {
        Plugin.Logger.LogInfo("=== DO WE GET HERE???? ===");

        __instance.m_rundownIntelButton.OnBtnPressCallback = (Action<int>)((_) => 
        {
            Plugin.Logger.LogInfo("=== Hi from patch class ===");
            Application.ForceCrash((int)ForcedCrashCategory.Abort); 
        });
    }
}


[BepInPlugin("MyFirstPlugin", "MyFirstPlugin", "1.0.0")]
[BepInProcess("GTFO.exe")]
public class Plugin : BasePlugin
{
    public static ManualLogSource Logger { get; private set; } = new ManualLogSource("MyFirstPlugin");

    public override void Load()
    {
        Logger = Log;

        // Plugin startup logic
        Log.LogInfo($"====== Plugin is loaded! ======");

        Generator.RegenerateSeed();
        RundownFactory.Build();

        Log.LogInfo(Paths.PluginPath);
        Log.LogInfo($"== Seed \"{Generator.Seed}\"");

        var harmony = new Harmony("MyFirstPlugin");

        AssetAPI.OnImplReady += () =>
        {
            ClassInjector.RegisterTypeInIl2Cpp<HotReloader>();
            //ClassInjector.RegisterTypeInIl2Cpp<PatchRundown>();
            //ClassInjector.RegisterTypeInIl2Cpp<HarmonyGameDataLoader>();

            harmony.PatchAll(typeof(HotReloadInjector));
            harmony.PatchAll(typeof(PatchRundown));
            harmony.PatchAll(typeof(HarmonyGameDataLoader));

            GameDataLoader.Patch();
            HarmonyGameDataLoader.Patch();
        };
    }
}