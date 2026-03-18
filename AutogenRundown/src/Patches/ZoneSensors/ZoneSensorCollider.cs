using Il2CppInterop.Runtime.Injection;
using Player;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// MonoBehaviour component attached to each sensor that detects players within radius.
/// Triggers events when a player enters the detection zone.
/// </summary>
public class ZoneSensorCollider : MonoBehaviour
{
    /// <summary>
    /// Definition ID of the sensor group this collider belongs to.
    /// </summary>
    public int Id;

    /// <summary>
    /// Index of this sensor within its group.
    /// </summary>
    public int SensorIndex;

    /// <summary>
    /// When true, this sensor triggers independently from the group.
    /// </summary>
    public bool TriggerEach;

    /// <summary>
    /// Detection radius for this sensor.
    /// </summary>
    public float Radius = 2.3f;

    private const float CHECK_INTERVAL = 0.1f;
    private const float GRACE_PERIOD = 1.0f;
    private static readonly Color GraceColor = new(0.5f, 0.5f, 0.5f, 0.4f);

    private float checkTimer = 0f;
    private int lastPlayerCount = 0;

    private float graceTimer = -1f;
    private bool isFirstActivation = true;

    /// <summary>
    /// Plain C# object holding visual references. Not an IL2CPP field —
    /// avoids ClassInjector trampoline issues with abstract/injected types.
    /// </summary>
    public SensorVisualRefs Visuals;

    void Update()
    {
        // Handle grace period countdown
        if (graceTimer > 0f)
        {
            graceTimer -= Time.deltaTime;
            if (graceTimer <= 0f)
            {
                graceTimer = -1f;
                RestoreVisuals();
            }
            return;
        }

        checkTimer += Time.deltaTime;
        if (checkTimer < CHECK_INTERVAL)
            return;

        checkTimer = 0f;

        // In TriggerEach mode, the sensor's active state is the source of truth
        // (synced via network). Skip if this sensor is disabled.
        if (!gameObject.activeSelf)
            return;

        // Skip if group is disabled
        if (!ZoneSensorManager.Current.IsGroupEnabled(Id))
            return;

        var currentCount = CountPlayersInRadius();

        // Trigger when player count increases (player entered)
        if (currentCount > lastPlayerCount)
        {
            if (TriggerEach)
                ZoneSensorManager.Current.SensorTriggeredIndividual(Id, SensorIndex);
            else
                ZoneSensorManager.Current.SensorTriggered(Id);
        }

        lastPlayerCount = currentCount;
    }

    /// <summary>
    /// Counts the number of alive human players within the detection radius.
    /// </summary>
    private int CountPlayersInRadius()
    {
        var count = 0;

        if (PlayerManager.PlayerAgentsInLevel == null)
            return count;

        foreach (var player in PlayerManager.PlayerAgentsInLevel)
        {
            if (player == null)
                continue;

            if (!player.Alive)
                continue;

            // Skip bots
            if (player.Owner != null && player.Owner.IsBot)
                continue;

            var distance = Vector3.Distance(transform.position, player.Position);
            if (distance <= Radius)
                count++;
        }

        return count;
    }

    /// <summary>
    /// Resets the player count (call when sensor is re-enabled).
    /// Starts a grace period on re-enable (skipped on initial spawn).
    /// </summary>
    public void ResetState()
    {
        lastPlayerCount = 0;
        checkTimer = 0f;

        if (isFirstActivation)
        {
            isFirstActivation = false;
            return;
        }

        graceTimer = GRACE_PERIOD;
        ApplyGraceVisuals();
    }

    private void ApplyGraceVisuals()
    {
        if (Visuals == null) return;

        if (Visuals.Renderer != null)
            Visuals.Renderer.material.SetColor("_ColorA", GraceColor);

        if (Visuals.TextAnimator != null)
            Visuals.TextAnimator.enabled = false;

        if (Visuals.TextComponent != null)
            Visuals.TextComponent.SetText("");
    }

    private void RestoreVisuals()
    {
        if (Visuals == null) return;

        if (Visuals.Renderer != null)
            Visuals.Renderer.material.SetColor("_ColorA", Visuals.OriginalColor);

        if (Visuals.TextAnimator != null)
        {
            // Animator handles its own text content
            Visuals.TextAnimator.enabled = true;
        }
        else if (Visuals.TextComponent != null)
        {
            Visuals.TextComponent.SetText(Visuals.OriginalText);
            Visuals.TextComponent.m_fontColor = Visuals.TextComponent.m_fontColor32 = Visuals.OriginalTextColor;
        }
    }

    static ZoneSensorCollider()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorCollider>();
    }
}

/// <summary>
/// Plain C# class (not registered with ClassInjector) to hold sensor visual references.
/// This avoids IL2CPP trampoline generation issues with abstract/injected parameter types.
/// </summary>
public class SensorVisualRefs
{
    public Renderer Renderer;
    public Color OriginalColor;
    public ZoneSensorTextAnimator TextAnimator;
    public TMPro.TextMeshPro TextComponent;
    public string OriginalText;
    public Color OriginalTextColor;
}
