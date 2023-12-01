using UnityEngine;

public class WallCrush : MonoBehaviour
{
    private Transform objectA;
    private Transform objectB;
    private Transform meetingPoint;
    [SerializeField] private float meetTime = 5.0f; // Time in seconds to reach the meeting point

    private float timer;
    private Vector3 startPositionA;
    private Vector3 startPositionB;
    private bool isMoving = false;

    void Start()
    {
        // Assuming the children are in a specific order
        objectA = transform.GetChild(0); // Wall-A
        objectB = transform.GetChild(1); // Wall-B
        meetingPoint = transform.GetChild(2); // GG

        startPositionA = objectA.position;
        startPositionB = objectB.position;

        StartMeeting();
    }

    void Update()
    {
        if (isMoving)
        {
            if (timer <= meetTime)
            {
                timer += Time.deltaTime;
                float progress = timer / meetTime;

                // Move object A and B towards the meeting point
                objectA.position = Vector3.Lerp(startPositionA, meetingPoint.position, progress);
                objectB.position = Vector3.Lerp(startPositionB, meetingPoint.position, progress);
            }
            else
            {
                isMoving = false;
                // Optionally snap to the exact meeting point position to ensure they get there
                objectA.position = meetingPoint.position;
                objectB.position = meetingPoint.position;
            }
        }
    }

    public void StartMeeting()
    {
        timer = 0f;
        isMoving = true;
    }
}
