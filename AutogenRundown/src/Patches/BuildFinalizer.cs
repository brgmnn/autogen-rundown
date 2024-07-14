using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;
using XXHashing;

namespace AutogenRundown.Patches;

public class LevelGenPatcher
{
    public static LG_ZoneJob_CreateExpandFromData currentZone;

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    public static void BuildFinalizer(LG_ZoneJob_CreateExpandFromData __instance, ref Exception __exception, ref bool __result)
    {
        currentZone = __instance;
        if (__exception != null)
        {
            Plugin.Logger.LogInfo($"We failed {__instance.m_zone.AliasName} -- {__instance.m_zone.m_coverage} -- {__instance.m_subSeed} -- {__instance.m_zone.m_subSeed}");
            __instance.CleanupFailedZoneBuilds();
            __instance.m_zone.m_subSeed++;
            __instance.m_subSeed++;
            __result = true;
        }
        else
        {
            Plugin.Logger.LogInfo($"Building {__instance.m_zone.AliasName} -- {__instance.m_zone.m_sourceExpander.name} -- {__result}");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_FunctionMarkerBuilder), nameof(LG_FunctionMarkerBuilder.BuildMarkerSpawnerRec))]
    public static void MarkerPatch(LG_FunctionMarkerBuilder __instance,
        ref LG_MarkerSpawner markerSpawner,
        ref ExpeditionFunction function)
    {
        Plugin.Logger.LogInfo("We're Functioning");

        ExpeditionFunction expeditionFunction = function;
        bool functionWasSpawned = false;
        if (markerSpawner != null)
        {
            if (markerSpawner.m_zone == null)
                //Debug.LogError((object)string.Format("BuildMarkerSpawnerRec:: had null m_zone! function: {0}, markerSpawner: {1} ID: {2}", (object)function, (object)markerSpawner, markerSpawner != null ? markerSpawner.m_UID.ToString() : "Null"));
                if (!markerSpawner.AllowFunction())
                {
                    markerSpawner.m_producerSource.name += "_FuncNotAllowed";
                    expeditionFunction = ExpeditionFunction.None;
                }
            XXHashSequence xxHashSequence1 = new XXHashSequence(XXHash.Hash(markerSpawner.m_markerInstanceSeed, 973686256U));
            GameObject gameObject1 = LG_MarkerFactory.InstantiateMarkerGameObject(markerSpawner, expeditionFunction, xxHashSequence1.NextFloat(), xxHashSequence1.NextFloat(), xxHashSequence1.NextRange(-1f, 1f), ref functionWasSpawned);
            if (gameObject1 == null)
            {
                Plugin.Logger.LogInfo((expeditionFunction + " Did not function correctly"));
                if (expeditionFunction.ToString() == "PowerGenerator")
                {
                    Plugin.Logger.LogInfo("OPPSSS... This was really important.... REROLLL");
                    currentZone.CleanupFailedZoneBuilds();
                    currentZone.m_zone.m_subSeed++;
                    currentZone.m_subSeed++;
                }
            }
        }
    }
}


/* 
 
 public static LG_ZoneJob_CreateExpandFromData currentZone;
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
        public static void BuildFinalizer(LG_ZoneJob_CreateExpandFromData __instance,ref Exception __exception,ref bool __result)
        {
            currentZone = __instance;
            if (__exception != null)
            {
                EntryPoint.LogIt($"We failed {__instance.m_zone.AliasName} -- {__instance.m_zone.m_coverage} -- {__instance.m_subSeed} -- {__instance.m_zone.m_subSeed}");
                __instance.CleanupFailedZoneBuilds();
                __instance.m_zone.m_subSeed++;
                __instance.m_subSeed++;
                __result = true;
            }
            else { 
                EntryPoint.LogIt($"Building {__instance.m_zone.AliasName} -- {__instance.m_zone.m_sourceExpander.name} -- {__result}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LG_FunctionMarkerBuilder), nameof(LG_FunctionMarkerBuilder.BuildMarkerSpawnerRec))]
        public static void MarkerPatch(LG_FunctionMarkerBuilder __instance,
          ref LG_MarkerSpawner markerSpawner,
          ref ExpeditionFunction function)
        {
            EntryPoint.LogIt("We're Functioning");
            ExpeditionFunction expeditionFunction = function;
            bool functionWasSpawned = false;
            if (markerSpawner != null)
            {
                if (markerSpawner.m_zone == null)
                    //Debug.LogError((object)string.Format("BuildMarkerSpawnerRec:: had null m_zone! function: {0}, markerSpawner: {1} ID: {2}", (object)function, (object)markerSpawner, markerSpawner != null ? markerSpawner.m_UID.ToString() : "Null"));
                    if (!markerSpawner.AllowFunction())
                    {
                        markerSpawner.m_producerSource.name += "_FuncNotAllowed";
                        expeditionFunction = ExpeditionFunction.None;
                    }
                XXHashSequence xxHashSequence1 = new XXHashSequence(XXHash.Hash(markerSpawner.m_markerInstanceSeed, 973686256U));
                GameObject gameObject1 = LG_MarkerFactory.InstantiateMarkerGameObject(markerSpawner, expeditionFunction, xxHashSequence1.NextFloat(), xxHashSequence1.NextFloat(), xxHashSequence1.NextRange(-1f, 1f), ref functionWasSpawned);
                if (gameObject1 == null)
                {
                    EntryPoint.LogIt((expeditionFunction + " Did not function correctly"));
                    if (expeditionFunction.ToString() == "PowerGenerator") {
                        EntryPoint.LogIt("OPPSSS... This was really important.... REROLLL");
                        currentZone.CleanupFailedZoneBuilds();
                        currentZone.m_zone.m_subSeed++;
                        currentZone.m_subSeed++;
                    }
                }
            }
        }
 
 */
