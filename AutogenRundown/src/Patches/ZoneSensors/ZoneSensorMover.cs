using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.AI;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// MonoBehaviour that handles sensor movement along a NavMesh path.
/// Moves the sensor back and forth between two points at a constant speed.
/// </summary>
public class ZoneSensorMover : MonoBehaviour
{
    private Vector3[] waypoints;
    private int currentWaypointIndex;
    private bool movingForward = true;
    private float speed;
    private bool initialized = false;

    public void Initialize(Vector3 startPos, Vector3 endPos, float moveSpeed, float edgeDistance = 0.1f)
    {
        speed = moveSpeed;

        // Calculate NavMesh path
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(startPos, endPos, -1, path))
        {
            waypoints = AdjustWaypointsForEdgeDistance(path.corners, edgeDistance);
        }
        else
        {
            // Fallback: direct line between points
            waypoints = new[] { startPos, endPos };
        }

        currentWaypointIndex = 1; // Start moving toward second point
        initialized = true;

        Plugin.Logger.LogDebug($"ZoneSensorMover: Initialized with {waypoints.Length} waypoints, speed={speed}");
    }

    private Vector3[] AdjustWaypointsForEdgeDistance(Vector3[] corners, float edgeDistance)
    {
        if (edgeDistance <= 0f)
            return corners;

        var adjusted = new Vector3[corners.Length];
        for (int i = 0; i < corners.Length; i++)
        {
            adjusted[i] = PullAwayFromEdge(corners[i], edgeDistance);
        }
        return adjusted;
    }

    private Vector3 PullAwayFromEdge(Vector3 position, float minDistance)
    {
        if (!NavMesh.FindClosestEdge(position, out NavMeshHit hit, -1))
            return position;

        if (hit.distance >= minDistance)
            return position;

        // Move away from edge (hit.normal points away from edge)
        Vector3 pullDirection = hit.normal;
        float pullAmount = minDistance - hit.distance;
        Vector3 newPos = position + pullDirection * pullAmount;

        // Verify new position is still on NavMesh
        if (NavMesh.SamplePosition(newPos, out NavMeshHit sample, 0.5f, -1))
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
        Vector3 target = waypoints[currentWaypointIndex];
        float step = speed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, target, step);

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            AdvanceWaypoint();
        }
    }

    private void AdvanceWaypoint()
    {
        if (movingForward)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                movingForward = false;
                currentWaypointIndex = waypoints.Length - 2;
            }
        }
        else
        {
            currentWaypointIndex--;
            if (currentWaypointIndex < 0)
            {
                movingForward = true;
                currentWaypointIndex = 1;
            }
        }
    }

    static ZoneSensorMover()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorMover>();
    }
}
