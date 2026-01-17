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
    /// Index of the sensor group this collider belongs to.
    /// </summary>
    public int GroupIndex;

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
    private float checkTimer = 0f;
    private int lastPlayerCount = 0;

    void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer < CHECK_INTERVAL)
            return;

        checkTimer = 0f;

        // In TriggerEach mode, the sensor's active state is the source of truth
        // (synced via network). Skip if this sensor is disabled.
        if (!gameObject.activeSelf)
            return;

        // Skip if group is disabled
        if (!ZoneSensorManager.Current.IsGroupEnabled(GroupIndex))
            return;

        int currentCount = CountPlayersInRadius();

        // Trigger when player count increases (player entered)
        if (currentCount > lastPlayerCount)
        {
            if (TriggerEach)
                ZoneSensorManager.Current.SensorTriggeredIndividual(GroupIndex, SensorIndex);
            else
                ZoneSensorManager.Current.SensorTriggered(GroupIndex);
        }

        lastPlayerCount = currentCount;
    }

    /// <summary>
    /// Counts the number of alive human players within the detection radius.
    /// </summary>
    private int CountPlayersInRadius()
    {
        int count = 0;

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

            float distance = Vector3.Distance(transform.position, player.Position);
            if (distance <= Radius)
                count++;
        }

        return count;
    }

    /// <summary>
    /// Resets the player count (call when sensor is re-enabled).
    /// </summary>
    public void ResetState()
    {
        lastPlayerCount = 0;
        checkTimer = 0f;
    }

    static ZoneSensorCollider()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorCollider>();
    }
}
