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

    public void Initialize(Vector3 startPos, Vector3 endPos, float moveSpeed)
    {
        speed = moveSpeed;

        // Calculate NavMesh path
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(startPos, endPos, -1, path))
        {
            waypoints = path.corners;
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
