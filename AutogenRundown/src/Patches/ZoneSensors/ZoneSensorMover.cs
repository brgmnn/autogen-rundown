using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.AI;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// MonoBehaviour that handles sensor movement along a NavMesh path.
/// Moves the sensor back and forth between waypoints at a constant speed.
///
/// Network sync:
/// - Host generates waypoints via Initialize() and broadcasts them
/// - Clients/late joiners receive waypoints via SetWaypoints()
/// - Movement state is periodically synced via GetMovementState/ApplyMovementState
/// </summary>
public class ZoneSensorMover : MonoBehaviour
{
    private Vector3[] waypoints = Array.Empty<Vector3>();
    private int currentWaypointIndex;
    private bool movingForward = true;
    private float speed;
    private bool initialized = false;

    // For calculating progress between waypoints
    private Vector3 lastWaypointPosition;

    /// <summary>
    /// Initializes movement with generated waypoints.
    /// Returns the computed waypoints array for network sync.
    /// </summary>
    public Vector3[] Initialize(List<Vector3> positions, float moveSpeed, float edgeDistance = 0.1f)
    {
        speed = moveSpeed;

        // Build waypoints by calculating NavMesh paths between consecutive positions
        var allWaypoints = new List<Vector3>();

        for (var i = 0; i < positions.Count - 1; i++)
        {
            var path = new NavMeshPath();
            if (NavMesh.CalculatePath(positions[i], positions[i + 1], -1, path))
            {
                var adjusted = AdjustWaypointsForEdgeDistance(path.corners, edgeDistance);
                // Skip first point on subsequent segments to avoid duplicates
                var startIdx = (i == 0) ? 0 : 1;
                for (var j = startIdx; j < adjusted.Length; j++)
                    allWaypoints.Add(adjusted[j]);
            }
            else
            {
                // Fallback: direct line
                if (i == 0) allWaypoints.Add(positions[i]);
                allWaypoints.Add(positions[i + 1]);
            }
        }

        waypoints = allWaypoints.ToArray();
        currentWaypointIndex = 1;
        lastWaypointPosition = waypoints[0];
        initialized = true;

        Plugin.Logger.LogDebug($"ZoneSensorMover: Initialized with {waypoints.Length} waypoints from {positions.Count} positions, speed={speed}");

        return waypoints;
    }

    /// <summary>
    /// Sets waypoints from network sync (for clients and late joiners).
    /// This replaces any locally generated waypoints with the host's authoritative waypoints.
    /// </summary>
    public void SetWaypoints(Vector3[] newWaypoints, float moveSpeed)
    {
        if (newWaypoints == null || newWaypoints.Length < 2)
        {
            Plugin.Logger.LogWarning("ZoneSensorMover: SetWaypoints called with invalid waypoints");
            return;
        }

        waypoints = newWaypoints;
        speed = moveSpeed;
        initialized = true;
        currentWaypointIndex = Math.Clamp(currentWaypointIndex, 0, waypoints.Length - 1);

        // Ensure currentWaypointIndex is at least 1 for proper movement
        if (currentWaypointIndex == 0)
            currentWaypointIndex = 1;

        // Ensure lastWaypointPosition is valid
        var fromIndex = movingForward ? currentWaypointIndex - 1 : currentWaypointIndex + 1;
        if (fromIndex >= 0 && fromIndex < waypoints.Length)
            lastWaypointPosition = waypoints[fromIndex];
        else
            lastWaypointPosition = transform.position;

        Plugin.Logger.LogDebug($"ZoneSensorMover: SetWaypoints with {waypoints.Length} waypoints, speed={moveSpeed}");
    }

    /// <summary>
    /// Gets the current movement state for network sync.
    /// </summary>
    public (int waypointIndex, bool forward, float progress) GetMovementState()
    {
        if (waypoints == null || waypoints.Length < 2)
            return (0, true, 0f);

        var progress = CalculateProgress();
        return (currentWaypointIndex, movingForward, progress);
    }

    /// <summary>
    /// Applies movement state from network sync.
    /// </summary>
    /// <param name="waypointIndex">Target waypoint index</param>
    /// <param name="forward">Movement direction</param>
    /// <param name="progress">Progress toward target (0.0-1.0)</param>
    /// <param name="snap">If true, immediately snap to position; otherwise smoothly interpolate</param>
    public void ApplyMovementState(int waypointIndex, bool forward, float progress, bool snap)
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        // Clamp to valid range
        waypointIndex = Math.Clamp(waypointIndex, 0, waypoints.Length - 1);

        // Handle boundary cases to ensure fromIndex != waypointIndex
        if (waypointIndex == 0 && !forward)
        {
            // At start but moving backward - should be moving forward from index 1
            waypointIndex = 1;
            forward = true;
        }
        else if (waypointIndex == waypoints.Length - 1 && forward)
        {
            // At end but moving forward - should be moving backward from second-to-last
            waypointIndex = waypoints.Length - 2;
            forward = false;
        }

        currentWaypointIndex = waypointIndex;
        movingForward = forward;

        // Calculate the "from" waypoint based on direction
        var fromIndex = forward ? waypointIndex - 1 : waypointIndex + 1;
        fromIndex = Math.Clamp(fromIndex, 0, waypoints.Length - 1);

        // Ensure we don't have from == target (would cause zero distance calculation issues)
        if (fromIndex == waypointIndex)
        {
            // Fallback: use current position as "from"
            lastWaypointPosition = transform.position;
        }
        else
        {
            lastWaypointPosition = waypoints[fromIndex];
        }

        if (snap)
        {
            // Interpolate position between from and target waypoints
            var targetPos = waypoints[waypointIndex];
            var fromPos = (fromIndex != waypointIndex) ? waypoints[fromIndex] : transform.position;
            transform.position = Vector3.Lerp(fromPos, targetPos, progress);
        }
        // If not snapping, the Update() loop will naturally move toward the target
    }

    /// <summary>
    /// Calculates progress from last waypoint to current target (0.0-1.0).
    /// </summary>
    private float CalculateProgress()
    {
        if (waypoints == null || waypoints.Length < 2 || currentWaypointIndex < 0 || currentWaypointIndex >= waypoints.Length)
            return 0f;

        var target = waypoints[currentWaypointIndex];
        var totalDistance = Vector3.Distance(lastWaypointPosition, target);

        if (totalDistance < 0.001f)
            return 1f;

        var remainingDistance = Vector3.Distance(transform.position, target);
        return 1f - (remainingDistance / totalDistance);
    }

    private Vector3[] AdjustWaypointsForEdgeDistance(Vector3[] corners, float edgeDistance)
    {
        if (edgeDistance <= 0f)
            return corners;

        var adjusted = new Vector3[corners.Length];
        for (var i = 0; i < corners.Length; i++)
        {
            adjusted[i] = PullAwayFromEdge(corners[i], edgeDistance);
        }
        return adjusted;
    }

    private Vector3 PullAwayFromEdge(Vector3 position, float minDistance)
    {
        if (!NavMesh.FindClosestEdge(position, out var hit, -1))
            return position;

        if (hit.distance >= minDistance)
            return position;

        // Move away from edge (hit.normal points away from edge)
        var pullDirection = hit.normal;
        var pullAmount = minDistance - hit.distance;
        var newPos = position + pullDirection * pullAmount;

        // Verify new position is still on NavMesh
        if (NavMesh.SamplePosition(newPos, out var sample, 0.5f, -1))
            return sample.position;

        return position;
    }

    void Update()
    {
        if (!initialized || waypoints == null || waypoints.Length < 2)
            return;

        // Skip movement if disabled (paused)
        if (!gameObject.activeSelf)
            return;

        // Move toward current waypoint at constant speed
        var target = waypoints[currentWaypointIndex];
        var step = speed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, target, step);

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            AdvanceWaypoint();
        }
    }

    private void AdvanceWaypoint()
    {
        // Store current position as the "from" waypoint for progress calculation
        lastWaypointPosition = waypoints[currentWaypointIndex];

        if (movingForward)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                movingForward = false;
                currentWaypointIndex = waypoints.Length - 2;
                // When reversing, update lastWaypointPosition to the endpoint we just left
                if (currentWaypointIndex >= 0)
                    lastWaypointPosition = waypoints[waypoints.Length - 1];
            }
        }
        else
        {
            currentWaypointIndex--;
            if (currentWaypointIndex < 0)
            {
                movingForward = true;
                currentWaypointIndex = 1;
                // When reversing, update lastWaypointPosition to the endpoint we just left
                if (waypoints.Length > 0)
                    lastWaypointPosition = waypoints[0];
            }
        }
    }

    static ZoneSensorMover()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorMover>();
    }
}
