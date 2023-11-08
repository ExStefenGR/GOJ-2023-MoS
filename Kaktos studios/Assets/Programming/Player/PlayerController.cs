using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Player Navigation Logic
    [SerializeField] private float speed = 5.0f;  // Movement speed
    [SerializeField] private float jumpForce = 7.0f;  // Jump force
    [SerializeField] private float airControlFactor = 0.6f;
    [SerializeField] private float wallJumpForce = 0.125f; // WallJump Force
    [SerializeField] private float wallJumpVerticalFactor = 0.75f;
    [SerializeField] private int maxWallJumps = 1;  // Maximum wall jumps before touching the groun

    //stage one stuff
    [SerializeField] private float zoomOutFOV = 60.0f; // Target FOV for zooming out
    [SerializeField] private float zoomOutDuration = 2.0f; // Duration of the zoom out effect
    private bool isZoomingOut = false;

    private List<int> wallIDs = new();

    private Rigidbody rb;
    private Camera cam;
    private float horizontalInput;
    private int wallJumpCount;
    private bool isGrounded;
    private bool isOnWall;
    private Collision lastWallCollision;

    //Player Spawn/Checkpoint
    private Vector3 lastCheckpointPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>(); // This will find the Camera component in the children of the GameObject.
        wallJumpCount = maxWallJumps;
        lastCheckpointPosition = transform.position;
    }

    void Update()
    {
        ProcessInput();
    }

    void FixedUpdate()
    {
        PerformMovement();
    }

    private void ProcessInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        // Check for jump input.
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || (isOnWall && wallJumpCount > 0))
            {
                PerformJump();
                if (isOnWall)
                {
                    PerformWallJump(lastWallCollision);
                }
            }
        }

    }
    private void PerformMovement()
    {
        float actualSpeed = isGrounded ? speed : (speed * airControlFactor); // airControlFactor is a value less than 1 to reduce air control

        Vector3 force = new(horizontalInput * actualSpeed, 0f, 0f);
        // Apply force only if on ground or not pressing against the wall
        if (isGrounded || !isOnWall)
        {
            rb.AddForce(force, ForceMode.VelocityChange);
        }

        float clampedX = Mathf.Clamp(rb.velocity.x, -speed, speed);
        rb.velocity = new Vector3(clampedX, rb.velocity.y, rb.velocity.z);
    }


    private void PerformJump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    private void PerformWallJump(Collision collision)
    {
        // Factor to adjust the vertical force during wall jump. 
        rb.AddForce(jumpForce * wallJumpVerticalFactor * Vector3.up, ForceMode.Impulse);

        // Add the force away from the wall.
        if (collision.contactCount > 0)
        {
            Vector3 forceAwayFromWall = -collision.contacts[0].normal; // Get the opposite direction of the normal

            // Adjusting the wall jump force. 
            float adjustedWallJumpForce = wallJumpForce * 0.5f;
            rb.AddForce(forceAwayFromWall * adjustedWallJumpForce, ForceMode.Impulse);
        }

        // Reset the jump count and wall status
        isOnWall = false;
        wallJumpCount--;
    }

    void OnCollisionStay(Collision collision)
    {
        // Check if the player is colliding with the ground.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            wallIDs.Clear();
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            lastWallCollision = collision;

            int currentWallID = collision.gameObject.GetInstanceID();
            // Add the wall ID to the list if not already present
            if (!wallIDs.Contains(currentWallID))
            {
                wallIDs.Add(currentWallID);
                isOnWall = true;
                // Reset wall jump count when hitting a new wall
                wallJumpCount = maxWallJumps;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isOnWall = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        /*if (other.CompareTag("Checkpoint"))
        {
            lastCheckpointPosition = other.transform.position; // Set the last checkpoint position
        }
        */
        if (other.gameObject.layer == LayerMask.NameToLayer("DeadZone"))
        {
            // Move the player to the last checkpoint position
            rb.position = lastCheckpointPosition;
            rb.velocity = Vector3.zero;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("ZoomOut"))
        {
            StartCoroutine(ZoomOutEffect());
        }
    }

    IEnumerator ZoomOutEffect()
    {
        if (isZoomingOut) // Check if the zoom out effect is already running
        {
            yield break; // Exit the coroutine early
        }

        isZoomingOut = true; // Set the flag to true as the effect is now running
        float initialFOV = cam.fieldOfView;
        float elapsed = 0.0f;

        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.deltaTime;
            float newFOV = Mathf.Lerp(initialFOV, zoomOutFOV, elapsed / zoomOutDuration);
            cam.fieldOfView = newFOV;
            yield return null;
        }

        cam.fieldOfView = zoomOutFOV; // Ensure the FOV is set to the target at the end
        isZoomingOut = false; // Reset the flag once the effect is finished
    }
}
