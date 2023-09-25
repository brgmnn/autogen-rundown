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

namespace MyFirstPlugin;

[BepInPlugin("MyFirstPlugin", "MyFirstPlugin", "1.0.0")]
public class Plugin : BasePlugin
{
    public override void Load()
    {
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
            harmony.PatchAll(typeof(HotReloadInjector));
        };

        harmony.PatchAll();
    }
}