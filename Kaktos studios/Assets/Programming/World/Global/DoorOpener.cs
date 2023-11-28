using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private string nextSceneName;

    private bool isOpen = false;
    private bool isToggling = false;

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Submit")) && !isToggling && IsPlayerNearby())
        {
            ToggleDoor();
        }
    }

    private bool IsPlayerNearby()
    {
        // You may want to use a specific method to check the player's proximity, such as using a trigger collider or a more sophisticated detection method.
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        isToggling = true;

        if (isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, openAngle, 0f);
        StartCoroutine(RotateDoor(targetRotation, LoadNextScene)); // LoadNextScene called when door fully open.
    }

    private void CloseDoor()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
        StartCoroutine(RotateDoor(targetRotation, () => isToggling = false));
    }

    private IEnumerator RotateDoor(Quaternion targetRotation, System.Action onComplete = null)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            // Calculate the angle to rotate in this frame
            float step = rotationSpeed * Time.deltaTime;
            // Rotate around the pivot point
            transform.RotateAround(pivotPoint.position, Vector3.up, step);

            // Check if the rotation is close enough to the target
            if (Quaternion.Angle(transform.rotation, targetRotation) <= step)
            {
                // Directly set the rotation to the target to prevent overshooting
                transform.rotation = targetRotation;
                break;
            }

            yield return null;
        }

        // Invoke the completion callback, if any
        onComplete?.Invoke();
    }
    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name is not set!");
        }
    }
}
