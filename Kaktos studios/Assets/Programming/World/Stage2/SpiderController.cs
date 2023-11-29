using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    public GameObject[] waypoints; // Array of waypoints
    public GameObject model;       // The model to move
    public float speed = 5.0f;     // Movement speed
    public float margin = 0.1f;    // Margin of error for reaching a waypoint

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0)
        {
            return; // No waypoints to follow
        }

        MoveTowardsWaypoint();
    }

    void MoveTowardsWaypoint()
    {
        Vector3 targetPosition = waypoints[currentWaypointIndex].transform.position;
        model.transform.position = Vector3.MoveTowards(model.transform.position, targetPosition, speed * Time.deltaTime);

        // Check if the model has reached the current waypoint within the margin of error
        if (Vector3.Distance(model.transform.position, targetPosition) < margin)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                model.SetActive(false);
            }
        }
    }
}
