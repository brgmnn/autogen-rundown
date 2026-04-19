using Expedition;
using GameData;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Forces a zone-source gate to spawn as LG_GateType.FreePassage (no security door, traversable
/// from start) when the target zone's ExpeditionZoneData.SecurityGateToEnter is set to the
/// autogen-only sentinel value 2 (SecurityGate.FreePassage).
///
/// Vanilla LG_BuildGateJob.Build only handles Security (0) and Apex (1) in its switch on
/// SecurityGateToEnter (LG_BuildGateJob.cs:121-130). Any other value falls through and leaves
/// the gate prefab null, crashing on Instantiate. This prefix catches those zones first,
/// replicates the relevant FreePassage branch (LG_BuildGateJob.cs:159-165) inline, and returns
/// false so vanilla never sees the sentinel.
///
/// The gate GameObject still uses its original LG_Gate subclass (LG_Gate.Type has a no-op
/// setter, so we cannot re-type it in place). The effect is equivalent: no door is instantiated,
/// the passage is marked traversable, and an LG_LinkAIGateJob is queued so AI pathing spans
/// the opening.
/// </summary>
[HarmonyPatch]
public static class Patch_LG_BuildGateJob_OpenPath
{
    private const eSecurityGateType FreePassageSentinel = (eSecurityGateType)2;

    [HarmonyPatch(typeof(LG_BuildGateJob), nameof(LG_BuildGateJob.Build))]
    [HarmonyPrefix]
    public static bool Pre_Build(LG_BuildGateJob __instance, ref bool __result)
    {
        var gate = __instance.m_gate;
        if (gate == null || gate.m_wasProcessed)
            return true;

        var plug = __instance.m_plug;

        // Vanilla's plug-link block (lines 62-75) runs before the gate-type switch and mutates
        // gate.m_linksTo / gate.ExpanderStatus. We need the post-link values to decide whether
        // this gate is ours, but we cannot run the mutation then bail out (we'd double-apply it
        // when vanilla runs). So compute what those values *will be* without mutating yet.
        var linksTo = (plug != null && plug.CoursePortal != null) ? plug.m_linksTo : gate.m_linksTo;
        var expanderStatus = plug != null ? plug.ExpanderStatus : gate.ExpanderStatus;

        var zoneData = linksTo?.m_zone?.m_settings?.m_zoneData;
        if (!gate.m_isZoneSource
            || expanderStatus == LG_ZoneExpanderStatus.Blocked
            || zoneData == null
            || zoneData.SecurityGateToEnter != FreePassageSentinel)
        {
            return true;
        }

        gate.m_wasProcessed = true;

        // Plug linking (vanilla LG_BuildGateJob.cs:62-75)
        if (plug != null)
        {
            gate.m_linksFrom = plug.m_linksFrom;
            gate.ExpanderStatus = plug.ExpanderStatus;
            if (plug.CoursePortal != null)
            {
                gate.CoursePortal = plug.CoursePortal;
                gate.CoursePortal.Gate = gate;
                gate.m_needsBorderLinks = true;
                gate.m_linksTo = plug.m_linksTo;
            }
        }
        else if (gate.CoursePortal != null)
        {
            gate.CoursePortal.Gate = gate;
        }

        // FreePassage branch (vanilla LG_BuildGateJob.cs:159-165). CalcOpenAtStart is a no-op
        // for zone-source gates, so we skip it.
        gate.m_isTraversableFromStart = true;
        gate.name = gate.name + " freePassage!";
        gate.m_hasBeenFlipped = __instance.m_plugWasFlipped
            || gate.ExpanderRotation == LG_ZoneExpanderRotation.Gate_RelinkedAndFlipped;
        LG_Factory.InjectJob(
            new LG_LinkAIGateJob(gate),
            LG_Factory.BatchName.AIGraph_LinkGates);

        Plugin.Logger.LogDebug(
            $"[OpenPath] Zone {linksTo.m_zone.Alias} forced to FreePassage (no security door)");

        __result = true;
        return false;
    }

    // Vanilla "portal" plug prefabs. Both are members of StraightPlugsWithGates in vanilla
    // ComplexResourceSetDataBlocks (Tech_Portal = SubComplex 10, Mining portal at SubComplex 12).
    // They're styled with trim around the opening, which reads better as an open archway than
    // a plain with_gate plug when no door is instantiated.
    private const string TechPortalPrefab =
        "Assets/AssetPrefabs/Complex/Tech/Plugs/env_plug_8mheight_flat_tech_portal_01.prefab";
    private const string MiningPortalPrefab =
        "Assets/AssetPrefabs/Complex/Mining/Plugs/env_plug_8mheight_flat_mining_portal_01.prefab";

    /// <summary>
    /// Swaps in a styled portal plug prefab for zone-source plugs whose target zone is flagged
    /// FreePassage. Without this, the FreePassage gate leaves a raw rectangular cutout in a
    /// generic with_gate plug; the portal variants have decorative framing around the opening.
    ///
    /// Only affects flat (same-altitude) plugs. Dropped plugs keep their vanilla prefab since
    /// the portal variants are flat-only and swapping them in would produce broken geometry.
    /// </summary>
    [HarmonyPatch(typeof(LG_BuildPlugBaseJob), nameof(LG_BuildPlugBaseJob.SpawnPlug))]
    [HarmonyPrefix]
    public static void Pre_SpawnPlug(LG_Plug plug, ref GameObject prefab)
    {
        if (plug == null || !plug.m_isZoneSource)
            return;

        var zoneData = plug.m_linksTo?.m_zone?.m_settings?.m_zoneData;
        if (zoneData == null || zoneData.SecurityGateToEnter != FreePassageSentinel)
            return;

        // Skip dropped plugs — the flat portal prefab would clip through.
        if (plug.m_pariedWith != null
            && !Mathf.Approximately(plug.transform.position.y, plug.m_pariedWith.transform.position.y))
        {
            return;
        }

        var path = plug.m_subComplex switch
        {
            SubComplex.DigSite or SubComplex.Refinery or SubComplex.Storage
                or SubComplex.Mining_Reactor => MiningPortalPrefab,
            _ => TechPortalPrefab,
        };

        var replacement = AssetAPI.GetLoadedAsset<GameObject>(path);
        if (replacement != null)
        {
            prefab = replacement;
            Plugin.Logger.LogDebug(
                $"[OpenPath] Zone {plug.m_linksTo.m_zone.Alias}: swapped plug prefab to {path}");
        }
    }
}
