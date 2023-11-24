using UnityEngine;

public class SkullFollowPlayer : MonoBehaviour
{
    [SerializeField] Transform playerTransform; // Drag and drop the player's transform component in the Unity editor.

    void Update()
    {
        if (playerTransform != null)
        {
            // Get the direction vector from the skull to the player.
            Vector3 directionToPlayer = playerTransform.position - transform.position;

            // Use LookRotation to smoothly rotate the skull towards the player.
            Quaternion rotation = Quaternion.LookRotation(directionToPlayer);

            // Apply the rotation to the skull.
            transform.rotation = rotation;
        }
    }
}
