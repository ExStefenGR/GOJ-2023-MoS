using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Player Navigation Logic
    [SerializeField] private float speed = 5.0f;  // Movement speed
    [SerializeField] private float jumpForce = 7.0f;  // Jump force
    [SerializeField] private int maxWallJumps = 1;  // Maximum wall jumps before touching the groun

    private Rigidbody rb;
    private float horizontalInput;
    private int wallJumpCount;
    private int lastWallID = -1;
    private bool isGrounded;
    private bool isOnWall;

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
                    wallJumpCount--;
                    isOnWall = false; // Reset the isOnWall to ensure the player cannot cling without re-collision
                    // Optional: Add additional force away from the wall here
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
            // Prevent clinging to the same wall
            if (collision.gameObject.GetInstanceID() != lastWallID)
            {
                isOnWall = true;
                lastWallID = collision.gameObject.GetInstanceID(); // Store the current wall ID
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
