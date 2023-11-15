using System.Collections;
using UnityEngine;

public class MovingPlatformController : MonoBehaviour
{
    [SerializeField] private Transform pointATransform;
    [SerializeField] private Transform pointBTransform;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float waitTime = 1.0f;
    private Vector3 nextPosition;
    private bool isWaiting;

    void Start()
    {
        // Store the initial positions of the assigned points
        Vector3 pointAPosition = pointATransform.position;
        Vector3 pointBPosition = pointBTransform.position;

        nextPosition = pointAPosition;
        StartCoroutine(MovePlatform(pointAPosition, pointBPosition));
    }

    IEnumerator MovePlatform(Vector3 pointAPosition, Vector3 pointBPosition)
    {
        while (true)
        {
            if (!isWaiting && Vector3.Distance(transform.position, nextPosition) > 0.01f)
            {
                // Move towards the next position
                transform.position = Vector3.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);
                yield return null;
            }
            else if (!isWaiting)
            {
                isWaiting = true;
                yield return new WaitForSeconds(waitTime);
                isWaiting = false;

                // Switch the target position
                nextPosition = nextPosition == pointAPosition ? pointBPosition : pointAPosition;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
