using AutogenRundown.Extensions;
using HarmonyLib;

namespace AutogenRundown.Patches;

/// <summary>
/// Customizes glow sticks, with the option for client side vanity player colors.
/// </summary>
[HarmonyPatch]
public class Glowsticks
{
    // This comes from the base game
    private const float MaxIntensity = 0.2f;

    // Adjust the fade in / out duration to be longer
    private const double FadeInDuration = 6.0;
    private const double FadeOutDuration = 12.0;

    private const double Duration = 75.0;

    /// <summary>
    /// Adjusts the fade in / fade out / duration of the glow sticks.
    /// We balance them to be longer and slower than the base game
    /// for dramatic effect.
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(GlowstickInstance), nameof(GlowstickInstance.Update))]
    [HarmonyPrefix]
    internal static void Pre_GlowstickInstance_Update(GlowstickInstance __instance)
    {
        __instance.s_lightLifeTime = (float)Duration;

        // Happens right at the start of fade in, set fade in speed
        if (__instance.m_state == eFadeState.FadeIn && Numeric.ApproxEqual(__instance.m_progression, 0.0f))
            __instance.m_progressionSpeed = (float)(1.0 / FadeInDuration);

        // Happens right at the start of fade out, set the fade out speed
        if (__instance.m_state == eFadeState.FadeOut && Numeric.ApproxEqual(__instance.m_progression, 1.0f))
            __instance.m_progressionSpeed = (float)(1.0 / FadeOutDuration);
    }

    /// <summary>
    /// Adjusts the fade in / out duration, and optionally sets player
    /// glow stick colors if configured to do so
    /// </summary>
    [HarmonyPatch(typeof(GlowstickInstance), nameof(GlowstickInstance.Update))]
    [HarmonyPostfix]
    internal static void Post_GlowstickInstance_Update(GlowstickInstance __instance)
    {
        if (!Plugin.Config_UsePlayerColoredGlowsticks)
            return;

        __instance.m_light.Color = __instance.Owner.Owner.PlayerColor;

        // Set the color of the glow stick to the player color.
        switch (__instance.m_state)
        {
            case eFadeState.Zero:
                __instance.m_light.Intensity = 0.0f;
                break;

            case eFadeState.One:
                __instance.m_light.Intensity = MaxIntensity;
                break;

            case eFadeState.FadeOut:
            case eFadeState.FadeIn:
                __instance.m_light.Intensity = __instance.m_progression * MaxIntensity;
                break;
        }
    }
}
