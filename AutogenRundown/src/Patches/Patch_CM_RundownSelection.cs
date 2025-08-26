using AutogenRundown.Managers;
using HarmonyLib;
using UnityEngine;
using Random = System.Random;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_CM_RundownSelection
{
    public static Random randomDaily { get; private set; }    = new(Generator.GetHashCode("Daily_0"));
    public static Random randomWeekly { get; private set; }   = new(Generator.GetHashCode("Weekly_0"));
    public static Random randomMonthly { get; private set; }  = new(Generator.GetHashCode("Monthly_0"));
    public static Random randomSeasonal { get; private set; } = new(Generator.GetHashCode("Seasonal_0"));

    /// <summary>
    /// Removes the rundown text for any disabled rundowns and performs some text position
    /// adjustments
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CM_RundownSelection), nameof(CM_RundownSelection.Setup))]
    public static void SetupPostfix(ref CM_RundownSelection __instance)
    {
        var id = __instance.m_rundownText.text;

        if (!RundownSelection.R1.Enabled && id == "R1" ||
            !RundownSelection.R2.Enabled && id == "R2" ||
            !RundownSelection.R3.Enabled && id == "R3" ||
            !RundownSelection.R4.Enabled && id == "R4" ||
            !RundownSelection.R5.Enabled && id == "R5" ||
            !RundownSelection.R6.Enabled && id == "R6" ||
            !RundownSelection.R7.Enabled && id == "R7" ||
            !RundownSelection.R8.Enabled && id == "R8")
        {
            __instance.m_rundownText.text = "";
            __instance.SetButtonEnabled(false);
        }


        if (RundownSelection.R1.Enabled && id == "R1")
            __instance.transform.localPosition += new Vector3 { x = -10.0f, y = 10.0f, z = 0.0f };

        if (RundownSelection.R2.Enabled && id == "R2")
            __instance.m_rundownText.transform.localPosition += new Vector3 { x = 0.0f, y = -10.0f, z = 0.0f };

        if (RundownSelection.R8.Enabled && id == "R8")
            __instance.m_rundownText.transform.localPosition += new Vector3 { x = -10.0f, y = -60.0f, z = 0.0f };
    }

    // These values are inspected directly from the base game
    private static readonly Color Disabled = new() { r = 0.04f, g = 0.04f, b = 0.04f, a = 0.0314f };
    private static readonly Color Enabled  = new() { r = 0.40f, g = 0.40f, b = 0.40f, a = 0.3137f };

    /// <summary>
    /// Patches the rundown selection screen sprites to have the right color
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GUIX_Layer), nameof(GUIX_Layer.SetColor))]
    public static bool GUIX_Layer_SetColor_Prefix(GUIX_Layer __instance, ref Color color)
    {
        // Disable unused rundowns
        if (!RundownSelection.R1.Enabled && LayerHasParent(__instance, RundownSelection.R1.UnityName))
            color = Disabled;
        if (!RundownSelection.R2.Enabled && LayerHasParent(__instance, RundownSelection.R2.UnityName))
            color = Disabled;
        if (!RundownSelection.R3.Enabled && LayerHasParent(__instance, RundownSelection.R3.UnityName))
            color = Disabled;
        if (!RundownSelection.R4.Enabled && LayerHasParent(__instance, RundownSelection.R4.UnityName))
            color = Disabled;
        if (!RundownSelection.R5.Enabled && LayerHasParent(__instance, RundownSelection.R5.UnityName))
            color = Disabled;
        if (!RundownSelection.R6.Enabled && LayerHasParent(__instance, RundownSelection.R6.UnityName))
            color = Disabled;
        if (!RundownSelection.R7.Enabled && LayerHasParent(__instance, RundownSelection.R7.UnityName))
            color = Disabled;
        if (!RundownSelection.R8.Enabled && LayerHasParent(__instance, RundownSelection.R8.UnityName))
            color = Disabled;

        return true;
    }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GUIX_Layer), nameof(GUIX_Layer.OnEnable))]
    // [HarmonyPatch(typeof(GUIX_Layer), nameof(GUIX_Layer.SetColor))]
    // [HarmonyPatch(typeof(GUIX_Layer), nameof(GUIX_Layer.Setup))]
    // public static void GUIX_Layer_SetRadarColors(GUIX_Layer __instance)
    // {
    //     if (LayerHasParent(__instance, RundownSelection.R1.UnityName))
    //         SetLineSubsetColor(__instance, 90, new Color { r = 0.8509804f, g = 0.4190196f, b = 0.158823538f, a = 1.0f }, randomDaily);
    //     if (LayerHasParent(__instance, RundownSelection.R2.UnityName))
    //         SetLineSubsetColor(__instance, 80, new Color { r = 0.158823538f, g = 0.8509804f, b = 0.4190196f, a = 1.0f }, randomWeekly);
    //     if (LayerHasParent(__instance, RundownSelection.R7.UnityName))
    //         SetLineSubsetColor(__instance, 70, new Color { r = 0.158823538f, g = 0.4190196f, b = 0.8509804f, a = 1.0f }, randomMonthly);
    //     if (LayerHasParent(__instance, RundownSelection.R8.UnityName))
    //         SetLineSubsetColor(__instance, 70, new Color { r = 0.9509804f, g = 0.158823538f, b = 0.158823538f, a = 1.0f }, randomSeasonal);
    // }

    private static void SetRadarColor(GUIX_Layer layer, Color color)
    {
        try
        {
            var radars = layer.transform.FindChildrenRecursive("radar", false);

            if (radars.Count < 1)
                return;

            for (var i = 0; i < radars.Count - 1; i++)
            {
                var elementSprite = radars[i].GetComponent<GUIX_ElementSprite>();

                if (elementSprite != null)
                    elementSprite.spriteRenderer.color = color;
            }
        }
        catch (NullReferenceException)
        {
            // Do nothing
        }
    }

    private static void SetLineSubsetColor(GUIX_Layer layer, int count, Color color, Random rng)
    {
        var rawLines = layer.transform.FindChildrenRecursive("Line", false);

        if (rawLines == null)
            return;

        var removeTop = 300;
        var lines = rawLines.GetRange(removeTop, rawLines.Count - removeTop);

        var modified = Math.Min(count, lines.Count);

        while (modified > 0)
        {
            var index = rng.Next(0, lines.Count);

            try
            {
                var elementSprite = lines[index].GetComponent<GUIX_ElementSprite>();

                if (elementSprite != null)
                    elementSprite.spriteRenderer.color = color;
            }
            catch (NullReferenceException)
            {
                // skip that element if it fails for some reason
            }

            lines.RemoveAt(index);

            modified--;
        }
    }

    private static bool LayerHasParent(GUIX_Layer layer, string parentName)
    {
        // Check if this transform is under "Rundown_Surface_SelectionALT_R3"
        var current = layer.gameObject.transform;

        while (current != null)
        {
            if (current.name == parentName)
                return true;

            current = current.parent;
        }
        return false;
    }
}
