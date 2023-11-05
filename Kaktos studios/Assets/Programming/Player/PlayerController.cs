using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Player Navigation Logic
    [SerializeField] private float speed = 5.0f;  // Movement speed
    [SerializeField] private float jumpForce = 7.0f;  // Jump force
    [SerializeField] private float wallJumpForce = 0.125f; // WallJump Force
    [SerializeField] private float wallJumpVerticalFactor = 0.75f;
    [SerializeField] private int maxWallJumps = 1;  // Maximum wall jumps before touching the groun

    private Rigidbody rb;
    private float horizontalInput;
    private int wallJumpCount;
    [SerializeField] private int lastWallID = -1;
    private bool isGrounded;
    private bool isOnWall;
    private Collision lastWallCollision;

    //Player Spawn/Checkpoint
    private Vector3 lastCheckpointPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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

        // Use AddForce to move the player based on input and speed.
        Vector3 force = new(horizontalInput * speed, 0f, 0f);
        rb.AddForce(force, ForceMode.VelocityChange);

        // Clamp the velocity to ensure that it doesn't exceed the desired speed.
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -speed, speed), rb.velocity.y, rb.velocity.z);
    }

    private void PerformJump()
    {
        // Perform the jump action.
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        // Reset grounded status after jumping.
        isGrounded = false;
    }

    private void PerformWallJump(Collision collision)
    {
        // Factor to adjust the vertical force during wall jump. 
        // Less than 1 reduces the upward force, greater than 1 increases it.
        rb.AddForce(jumpForce * wallJumpVerticalFactor * Vector3.up, ForceMode.Impulse);

        // Add the force away from the wall.
        // Using the collision contact's normal to determine the direction away from the wall.
        if (collision.contactCount > 0)
        {
            Vector3 forceAwayFromWall = -collision.contacts[0].normal; // get the opposite direction of the normal

            // Adjusting the wall jump force. 
            // Less than 1 makes the player move less away from the wall, greater than 1 increases it.
            float adjustedWallJumpForce = wallJumpForce * 0.5f;
            rb.AddForce(forceAwayFromWall * adjustedWallJumpForce, ForceMode.Impulse);
        }

        // Reset the jump count and wall status
        isOnWall = false;
        wallJumpCount--;
    }


    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            wallJumpCount = maxWallJumps;
            lastWallID = -1; // Reset last wall ID
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {

            lastWallCollision = collision;
            // Prevent clinging to the same wall
            if (collision.gameObject.GetInstanceID() != lastWallID)
            {
                isOnWall = true;
                lastWallID = collision.gameObject.GetInstanceID(); // Store the current wall ID
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

        // When exiting a collision with a wall, set isOnWall to false.
        // If this is the last wall the player was attached to, reset the wall ID.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (collision.gameObject.GetInstanceID() == lastWallID)
            {
                isOnWall = false;
                lastWallID = -1; // Reset last wall ID, allowing the next wall collision to be considered new
            }
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
    }
}
