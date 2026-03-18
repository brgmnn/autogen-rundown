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

    // Cached visual references (set via SetVisualReferences)
    private Renderer sensorRenderer;
    private Color originalColor;
    private ZoneSensorTextAnimator textAnimator;
    private TMPro.TextMeshPro textComponent;
    private string originalText;
    private Color originalTextColor;

    public void SetVisualReferences(Renderer renderer, Color color,
        ZoneSensorTextAnimator animator, TMPro.TextMeshPro text,
        string textStr, Color textColor)
    {
        sensorRenderer = renderer;
        originalColor = color;
        textAnimator = animator;
        textComponent = text;
        originalText = textStr;
        originalTextColor = textColor;
    }

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
        if (sensorRenderer != null)
            sensorRenderer.material.SetColor("_ColorA", GraceColor);

        if (textAnimator != null)
            textAnimator.enabled = false;

        if (textComponent != null)
            textComponent.SetText("");
    }

    private void RestoreVisuals()
    {
        if (sensorRenderer != null)
            sensorRenderer.material.SetColor("_ColorA", originalColor);

        if (textAnimator != null)
        {
            // Animator handles its own text content
            textAnimator.enabled = true;
        }
        else if (textComponent != null)
        {
            textComponent.SetText(originalText);
            textComponent.m_fontColor = textComponent.m_fontColor32 = originalTextColor;
        }
    }

    static ZoneSensorCollider()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorCollider>();
    }
}
