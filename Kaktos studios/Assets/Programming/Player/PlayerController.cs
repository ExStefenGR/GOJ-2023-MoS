using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Player Navigation Logic
    public Slider stressBar;
    [SerializeField] private float speed = 5.0f;  // Movement speed
    [SerializeField] private float jumpForce = 7.0f;  // Jump force
    [SerializeField] private float airControlFactor = 0.6f;
    [SerializeField] private float wallJumpForce = 0.125f; // WallJump Force
    [SerializeField] private float wallJumpVerticalFactor = 0.75f;
    [SerializeField] private int maxWallJumps = 1;  // Maximum wall jumps before touching the ground
    [SerializeField] private float stressIncreaseRate = 1.0f;
    [SerializeField] private float stressUpdateSpeed = 5f;

    [SerializeField] private float stressLevel = 0f;
    private int currentStage = 1;
    private float targetStressLevel = 0f;


    //stage one stuff
    private bool isZoomingOut = false;

    private List<int> wallIDs = new();

    private Rigidbody rb;
    private Camera cam;
    private float horizontalInput;
    private int wallJumpCount;
    private bool isGrounded;
    private bool isOnWall;
    private Collision lastWallCollision;
    private Animator animator;


    private Transform quickRigRef;
    private Transform quickRigGuides;
    private Transform quickRigCtrlReference;

    //Player Spawn/Checkpoint
    private Vector3 lastCheckpointPosition;

    //post process
    private PostProcessVolume postProcessVolume;
    private ChromaticAberration chromaticAberration;

    private Vignette vignette;

    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        animator = GetComponentInChildren<Animator>();
        postProcessVolume = cam.GetComponent<PostProcessVolume>();
        wallJumpCount = maxWallJumps;
        lastCheckpointPosition = transform.position;

        quickRigRef = transform.Find("QuickRigCharacter_Reference");
        quickRigGuides = transform.Find("QuickRigCharacter_Guides");
        quickRigCtrlReference = transform.Find("QuickRigCharacter_Ctrl_Reference");

        if (postProcessVolume.profile.TryGetSettings(out chromaticAberration))
        {
            chromaticAberration.active = false; // Initially disable the effect
        }
        if (postProcessVolume.profile.TryGetSettings(out vignette))
        {
            vignette.active = false; // Initially disable the effect
        }

        InitializeStressBar();
        DetermineCurrentStage();
    }

    private void DetermineCurrentStage()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Stage-One")
        {
            currentStage = 2;
        }
        else if (sceneName == "Stage-Two")
        {
            currentStage = 3;
        }
        else if (sceneName == "Stage-Three")
        {
            currentStage = 4;
        }
        else
        {
            // Handle unknown or default case
            currentStage = 1; // Default to stage 1 or any other default you choose
        }
    }

    private void InitializeStressBar()
    {
        stressBar.maxValue = 100; // Set this to the maximum stress level
        stressBar.value = stressLevel; // Initialize with current stress level
    }

    void Update()
    {
        UpdateAnimator();
        ProcessInput();
        FlipPlayerDirection();

        if (stressLevel != targetStressLevel)
        {
            stressLevel = Mathf.Lerp(stressLevel, targetStressLevel, stressUpdateSpeed * Time.deltaTime);
            UpdateStressUI();
        }

        UpdateStress(); // Update stress based on the current stage

        // Check if stress level reaches or exceeds 100 and handle player respawn
        if (Mathf.Approximately(stressLevel, 100f) || stressLevel > 99.9f)
        {
            SceneManager.LoadScene(SceneManager.GetSceneByBuildIndex(currentStage).name);
        }

        if (stressLevel > 30)
        {
            // Chromatic aberration reaches maximum intensity at stress level 60
            chromaticAberration.intensity.value = Mathf.Clamp((stressLevel - 30) / 30.0f, 0, 1);
            chromaticAberration.active = true;

            // Vignette intensity interpolates between 0.100 and 0.326
            float vignetteIntensity = 0.100f + ((stressLevel - 30) / 30.0f) * (0.326f - 0.100f);
            vignette.intensity.value = Mathf.Clamp(vignetteIntensity, 0.100f, 0.326f);
            vignette.active = true;
        }
        else
        {
            chromaticAberration.active = false;
            vignette.active = false;
        }

    }




    void FixedUpdate()
    {
        ApplyAnimatorVelocity();
        PerformMovement();
    }

    private void UpdateStress()
    {
        switch (currentStage)
        {
            case 2:
                // For Stage 1, stress increase is handled in PerformJump
                break;

            case 3:
                // For Stage 2, stress increases when the character is moving
                if (IsMoving())
                {
                    IncreaseStressOverTime();
                }
                break;

            case 4:
                IncreaseStressOverTime();
                break;

                // Add additional cases for more stages if needed
        }
    }


    private void IncreaseStressOverTime()
    {
        targetStressLevel = Mathf.Clamp(stressLevel + stressIncreaseRate * Time.deltaTime, 0, stressBar.maxValue);
    }



    private bool IsMoving()
    {
        return horizontalInput != 0;
    }

    private void UpdateStressUI()
    {
        stressBar.value = stressLevel;
    }

    private void IncreaseStressOnJump()
    {
        targetStressLevel = Mathf.Clamp(stressLevel + 5f, 0, stressBar.maxValue);
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("velocity", Mathf.Abs(horizontalInput));
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
    private void ApplyAnimatorVelocity()
    {
        if (animator)
        {
            Vector3 animatorVelocity = animator.deltaPosition / Time.deltaTime;
            rb.velocity = new Vector3(animatorVelocity.x, rb.velocity.y, animatorVelocity.z); // Keep existing Y velocity
        }
    }

    private void FlipPlayerDirection()
    {
        // Determine the direction to flip based on the horizontal input.
        float direction = horizontalInput > 0 ? 90f : horizontalInput < 0 ? -90f : quickRigRef.eulerAngles.y;

        // Apply the rotation to each object if the player is moving
        if (horizontalInput != 0)
        {
            Quaternion newRotation = Quaternion.Euler(0, direction, 0);

            quickRigRef.rotation = newRotation;
            quickRigGuides.rotation = newRotation;
            quickRigCtrlReference.rotation = newRotation;
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
        animator.SetTrigger("jump");
        animator.SetBool("isJumping", true);

        if (currentStage == 2)
        {
            IncreaseStressOnJump();
        }
    }


    private void Land()
    {
        // Set the isJumping boolean to false to trigger the transition to idle or run animation
        animator.SetBool("isJumping", false);
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
            Land();
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
        if (other.gameObject.layer == LayerMask.NameToLayer("DeadZone"))
        {
            SceneManager.LoadScene(SceneManager.GetSceneByBuildIndex(currentStage).name);
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("ZoomOut"))
        {
            if (other.TryGetComponent<ZoomOutTrigger>(out var zoomOutTrigger) && !zoomOutTrigger.IsTriggered)
            {
                if (zoomOutTrigger.ActivateZoom()) // Only proceed if the zoom wasn't previously triggered
                {
                    StartCoroutine(ZoomOutEffect(zoomOutTrigger.TargetFOV, zoomOutTrigger.TargetDuration, zoomOutTrigger.TargetDistance));
                }
            }
        }
        if (other.CompareTag("pickup"))
        {
            PickupItem(other.gameObject);
        }
    }

    // Reduce the player's stress level
    private void PickupItem(GameObject pickupItem)
    {
        float stressReduction = 30f;

        stressLevel = Mathf.Max(0, stressLevel - stressReduction);
        targetStressLevel = stressLevel;

        UpdateStressUI();

        Destroy(pickupItem);
    }

    IEnumerator ZoomOutEffect(float TargetFOV, float TargetDuration, float TargetDistance)
    {
        if (isZoomingOut)
        {
            yield break;
        }

        isZoomingOut = true;
        float initialFOV = cam.fieldOfView;
        Vector3 initialPosition = cam.transform.localPosition;
        Vector3 targetPosition = initialPosition + new Vector3(TargetDistance, 0, 0); // local Z-axis
        float elapsed = 0.0f;

        while (elapsed < TargetDuration)
        {
            elapsed += Time.deltaTime;
            float newFOV = Mathf.Lerp(initialFOV, TargetFOV, elapsed / TargetDuration);
            cam.fieldOfView = newFOV;

            Vector3 newPosition = Vector3.Lerp(initialPosition, targetPosition, elapsed / TargetDuration);
            cam.transform.localPosition = newPosition;

            yield return null;
        }

        cam.fieldOfView = TargetFOV; // Ensure the FOV is set to the target at the end
        cam.transform.localPosition = targetPosition; // Ensure the camera is set to the target position at the end
        isZoomingOut = false;
    }
    // Stage3
    public void IncreaseStress(float amount)
    {
        stressLevel = Mathf.Clamp(stressLevel + amount, 0, 100);
        UpdateStressUI();
    }
}