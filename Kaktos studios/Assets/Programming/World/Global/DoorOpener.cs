using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private string nextSceneName; // Serialized field for the next scene name.

    bool isOpen = false;
    bool isToggling = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("submit"))
        {
            if (!isToggling && IsPlayerNearby())
            {
                ToggleDoor();
            }
        }
    }

    bool IsPlayerNearby()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f); // Adjust the radius based on your needs.

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void ToggleDoor()
    {
        isToggling = true;
        isOpen = !isOpen;

        if (isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    void OpenDoor()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, openAngle, 0f);
        StartCoroutine(RotateDoor(targetRotation, () => isToggling = false));
    }

    void CloseDoor()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
        StartCoroutine(RotateDoor(targetRotation, LoadNextScene)); // LoadNextScene is called when the door is fully open.
    }

    IEnumerator RotateDoor(Quaternion targetRotation, System.Action onComplete = null)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.RotateAround(pivotPoint.position, Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        // Check if a callback function is provided and call it.
        onComplete?.Invoke();
    }

    void LoadNextScene()
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
