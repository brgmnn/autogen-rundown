using AIGraph;
using Enemies;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Fix_NullSourceExpanderInCourseGraph
{
    /// <summary>
    /// Vanilla LG_CreateCourseGraphJob.Build dereferences zone.m_sourceExpander.m_linksFrom.m_zone
    /// for every non-zero zone in every main-dimension layer. When Fix_FailedToFindStartArea hits
    /// its fatalReached threshold it short-circuits a failing zone with __result=true and
    /// MarkForRebuild() but deliberately does NOT force LG_Factory.FactoryDone() (forcing it
    /// caused other crashes). The engine then drains the rest of the batches with the broken
    /// zone still in layer.m_zones — and the post-CreateGraph loop here NREs on its null
    /// m_sourceExpander. Each retry frame re-runs CreateGraph and re-logs every "Adding culling
    /// jobs for zone …" line, producing the observed log spam.
    ///
    /// We re-implement Build with the same shape as vanilla but guard the post-CreateGraph
    /// deref. CreateGraph itself is safe to run on broken zones (its inner loop is keyed on
    /// zone.m_areas.Count which is 0 for the broken zone, so no AIG_CourseNode is created).
    /// </summary>
    [HarmonyPatch(typeof(LG_CreateCourseGraphJob), nameof(LG_CreateCourseGraphJob.Build))]
    [HarmonyPrefix]
    public static bool Pre_Build(LG_CreateCourseGraphJob __instance, ref bool __result)
    {
        var floor = __instance.m_floor;
        var dimension = __instance.m_dimension;

        AIG_CourseGraph.CreateGraph(floor, dimension);

        if (dimension.IsMainDimension)
            EnemyUpdateManager.Current.ClearRegisteredNodes();
        EnemyUpdateManager.Current.RegisterNodes(dimension.CourseGraph.m_nodes);

        for (var i = 0; i < dimension.Layers.Count; i++)
        {
            var layer = dimension.Layers[i];
            for (var j = 1; j < layer.m_zones.Count; j++)
            {
                var zone = layer.m_zones[j];
                if (!zone.IsMainDimension)
                    continue;

                var src = zone.m_sourceExpander;
                if (src == null || src.m_linksFrom == null || src.m_linksTo == null)
                {
                    Plugin.Logger.LogDebug(
                        $"Skipping GeographicalChildZones link for {zone.name} " +
                        $"(LocalIndex={zone.LocalIndex}) — m_sourceExpander chain has null.");
                    continue;
                }

                var fromZone = src.m_linksFrom.m_zone;
                var toZone = src.m_linksTo.m_zone;
                var parent = (UnityEngine.Object)fromZone != (UnityEngine.Object)zone ? fromZone : toZone;
                if (parent == null)
                    continue;
                parent.GeographicalChildZones.Add(zone);
            }
        }

        __result = true;
        return false;
    }
}
