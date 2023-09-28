using GameData;
using Gear;
using Globals;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyFirstPlugin.DataBlockTypes;
using CellMenu;
using HarmonyLib;
using Newtonsoft.Json;
using GTFO.API;

namespace MyFirstPlugin
{
    public class SetRundownInjector
    {
        /// <summary>
        /// Gets called on CM_PageRundown_New.OnEnable if the BepInEx config for HotReloading is true.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuGuiLayer), nameof(MainMenuGuiLayer.ChangePage))]
        public static void OnChangePage()
        {
            Plugin.Logger.LogInfo("---> Changing page");
        }

        // CM_PageRundown_New.UpdateRundownExpeditionProgression
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnRundownProgressionFileUpdated))]
        public static void OnSelect(object[] __args)
        {
            Plugin.Logger.LogMessage("=============== SELECTED RUNDOWN ===============");
            Plugin.Logger.LogMessage($"args: {JsonConvert.SerializeObject(__args)}");

            GameManager.SetRundown();
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.PlaceRundown))]
        public static void PlaceRundownPostfix(object[] __args)
        {
            Plugin.Logger.LogMessage("=============== Place rundown, this must be it! ===============");
            Plugin.Logger.LogMessage($"args: ");
        }

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.PlaceRundown))]
        public static void PlaceRundownPrefix(object[] __args)
        {
            Plugin.Logger.LogMessage("=============== Prefix Place Rundown ===============");
            Plugin.Logger.LogMessage("=============== Replacing with Rundown 1");
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.Intro_RevealRundown))]
        public static void RevealRundownPostfix(CM_PageRundown_New __instance)
        {
            Plugin.Logger.LogMessage("=============== Reveal Rundown ===============");

            //__instance.m_currentRundownData = GameDataBlockBase<RundownDataBlock>.GetBlock(1);

            GameManager.SetRundown(__instance);
        }
    }

    internal static class GameManager
    {
        private readonly static List<IDataBlockType> _DataBlockCache = new List<IDataBlockType>();
        private readonly static List<IDBuffer> _DataBlockIdBuffers = new List<IDBuffer>();

        public static void SetRundown()
        {
            SetRundown(MainMenuGuiLayer.Current.PageRundownNew);
        }

        public static void SetRundown(CM_PageRundown_New rundownPage)
        {
            //var rundownPage = MainMenuGuiLayer.Current.PageRundownNew;
            rundownPage.m_dataIsSetup = false;

            try
            {
                clearIcon(rundownPage.m_expIconsTier1);
                clearIcon(rundownPage.m_expIconsTier2);
                clearIcon(rundownPage.m_expIconsTier3);
                clearIcon(rundownPage.m_expIconsTier4);
                clearIcon(rundownPage.m_expIconsTier5);
                clearIcon(rundownPage.m_expIconsTierExt);

                static void clearIcon(Il2CppSystem.Collections.Generic.List<CellMenu.CM_ExpeditionIcon_New> tier)
                {
                    if (tier == null)
                        return;

                    foreach (var icon in tier)
                    {
                        var obj = icon.gameObject;
                        if (obj != null)
                            GameObject.Destroy(icon.gameObject);
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"{e}");
            }

            rundownPage.m_currentRundownData = GameDataBlockBase<RundownDataBlock>.GetBlock(1);

            if (rundownPage.m_currentRundownData != null)
            {
                rundownPage.PlaceRundown(rundownPage.m_currentRundownData);
            }
            else
            {
                Plugin.Logger.LogError("Could not load the custom rundown");
            }

            rundownPage.m_dataIsSetup = true;
        }

        public static bool Initialize()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var asm = assemblies
                    .Where(a => !a.IsDynamic && a.Location.Contains("interop", StringComparison.InvariantCultureIgnoreCase))
                    .First(a => a.Location.EndsWith("Modules-ASM.dll", StringComparison.InvariantCultureIgnoreCase));

                var dataBlockTypes = new List<Type>();
                foreach (var type in asm.ExportedTypes)
                {
                    if (type == null)
                        continue;

                    if (string.IsNullOrEmpty(type.Namespace))
                        continue;

                    if (!type.Namespace.Equals("GameData"))
                        continue;

                    var baseType = type.BaseType;
                    if (baseType == null)
                        continue;

                    if (!baseType.Name.Equals("GameDataBlockBase`1"))
                    {
                        continue;
                    }

                    dataBlockTypes.Add(type);
                }

                var genericBaseType = typeof(DataBlockTypeWrapper<>);
                foreach (var type in dataBlockTypes)
                {
                    var genericType = genericBaseType.MakeGenericType(type);
                    var cache = (IDataBlockType)Activator.CreateInstance(genericType);
                    AssignForceChangeMethod(cache);
                    //_DataBlockCache.Add(cache);
                    //_DataBlockIdBuffers.Add(new IDBuffer());
                }

                return true;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Can't make cache from Modules-ASM.dll!: {e}");
                return false;
            }
        }

        public static void AssignForceChangeMethod(IDataBlockType blockTypeCache)
        {
            //TODO: Better Support
            switch (blockTypeCache.GetShortName().ToLower())
            {
                case "rundown":
                    blockTypeCache.RegisterOnChangeEvent(() =>
                    {
                        var rundownPage = MainMenuGuiLayer.Current.PageRundownNew;
                        rundownPage.m_dataIsSetup = false;
                        try
                        {
                            clearIcon(rundownPage.m_expIconsTier1);
                            clearIcon(rundownPage.m_expIconsTier2);
                            clearIcon(rundownPage.m_expIconsTier3);
                            clearIcon(rundownPage.m_expIconsTier4);
                            clearIcon(rundownPage.m_expIconsTier5);
                            clearIcon(rundownPage.m_expIconsTierExt);

                            static void clearIcon(Il2CppSystem.Collections.Generic.List<CellMenu.CM_ExpeditionIcon_New> tier)
                            {
                                if (tier == null)
                                    return;


                                foreach (var icon in tier)
                                {
                                    var obj = icon.gameObject;
                                    if (obj != null)
                                        GameObject.Destroy(icon.gameObject);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //Logger.Error($"{e}");
                        }


                        rundownPage.m_currentRundownData = GameDataBlockBase<RundownDataBlock>.GetBlock(Global.RundownIdToLoad);
                        if (rundownPage.m_currentRundownData != null)
                        {
                            rundownPage.PlaceRundown(rundownPage.m_currentRundownData);
                            rundownPage.m_dataIsSetup = true;
                        }
                    });
                    break;

                /*case "fogsettings":
                    blockTypeCache.RegisterOnChangeEvent(() =>
                    {
                        if (!Builder.CurrentFloor.IsBuilt)
                        {
                            return;
                        }

                        var state = EnvironmentStateManager.Current.m_stateReplicator.State;
                        EnvironmentStateManager.Current.UpdateFogSettingsForState(state);
                    });
                    break;

                case "lightsettings":
                    blockTypeCache.RegisterOnChangeEvent(() =>
                    {
                        if (!Builder.CurrentFloor.IsBuilt)
                        {
                            return;
                        }

                        foreach (var zone in Builder.CurrentFloor.allZones)
                        {
                            foreach (var node in zone.m_courseNodes)
                            {
                                LG_BuildZoneLightsJob.ApplyLightSettings(0, node.m_lightsInNode, zone.m_lightSettings, false);
                            }
                        }
                    });
                    break;
                */
            }
        }

        public static bool TryFindCache(string blockTypeName, out IDataBlockType cache)
        {
            var index = GetIndex(blockTypeName);
            if (index != -1)
            {
                cache = _DataBlockCache[index];
                return true;
            }

            cache = null;
            return false;
        }

        public static bool TryGetNextID(string blockTypeName, out uint id)
        {
            var index = GetIndex(blockTypeName);
            if (index != -1)
            {
                id = _DataBlockIdBuffers[index].GetNext();
                return true;
            }

            id = 0;
            return false;
        }

        public static void SetIDBuffer(string blockTypeName, uint id)
        {
            var index = GetIndex(blockTypeName);
            if (index != -1)
            {
                _DataBlockIdBuffers[index].CurrentID = id;
            }
        }

        public static void SetIDBuffer(string blockTypeName, uint id, IncrementMode mode)
        {
            var index = GetIndex(blockTypeName);
            if (index != -1)
            {
                var buffer = _DataBlockIdBuffers[index];
                buffer.CurrentID = id;
                buffer.IncrementMode = mode;
            }
        }

        private static int GetIndex(string blockTypeName)
        {
            blockTypeName = GetBlockName(blockTypeName);
            return _DataBlockCache.FindIndex(x => x.GetShortName().Equals(blockTypeName, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetBlockName(string blockTypeName)
        {
            blockTypeName = blockTypeName.Trim();
            if (blockTypeName.EndsWith("DataBlock"))
                blockTypeName = blockTypeName[0..^9];

            return blockTypeName;
        }
    }
}