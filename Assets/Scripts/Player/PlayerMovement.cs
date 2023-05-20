using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")] [SerializeField]
    private float walkSpeed;

    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;
    [Space(10)] [SerializeField] private float maxSlideSpeed;
    [SerializeField] private float maxWallRunSpeed;
    public float moveSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [HideInInspector] public bool isSliding;
    [HideInInspector] public bool isWallrunning;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public bool isBoosting;

    [Space(10)] [SerializeField] private float airMultiplier;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    [Space(10)] [SerializeField] private float groundDrag;

    [HideInInspector] public float startYScale;

    [Header("Ground Check")] [SerializeField]
    private float playerHeight;

    public LayerMask groundMask;
    [HideInInspector] public bool isGrounded;

    [Header("Slope Check")] [SerializeField]
    private float maxSlopeAngle;

    private RaycastHit slopeHit;
    [HideInInspector] public bool exitingSlope;

    public Transform orientation;
    [SerializeField] private TextMeshProUGUI velocityText;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("Camera Effects")] public PlayerCam cam;
    [SerializeField] private float defaultFov;
    [SerializeField] private Vector3 defaultTilt;

    internal float horizontalInput;
    internal float verticalInput;

    public Vector3 moveDirection;

    [HideInInspector] public Rigidbody rBody;

    [HideInInspector] public MovementState state;
    private float playerVelocity;
    private PlayerKeybinds pKeybinds;
    private SpeedBoost speedBoost;

    private Vector3 platformVelocity;

    public enum MovementState
    {
        Walking,
        Running,
        Wallrunning,
        Crouching,
        Sliding,
        Midair
    }

    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.freezeRotation = true;

        pKeybinds = GetComponent<PlayerKeybinds>();
        speedBoost = GetComponent<SpeedBoost>();

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // Check if player is on the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, groundMask);

        Inputs();
        SpeedController();
        StateHandler();

        // Apply drag
        if (isGrounded)
        {
            rBody.drag = groundDrag;
        }
        else
        {
            rBody.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        // Add boost to speed
        // moveSpeed *= speedBoost.currentSpeedMultiplier;

        MovePlayer();

        // Update player forwards velocity variable with rigidbody velocity
        playerVelocity = rBody.velocity.magnitude;

        // Update HUD text
        velocityText.text = "Velocity: " + Math.Round(playerVelocity, 2) + " m/s";
        stateText.text = "State: " + state;
    }

    private void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void StateHandler()
    {
        // Set wall running state
        if (isWallrunning)
        {
            state = MovementState.Wallrunning;
            desiredMoveSpeed = maxWallRunSpeed;
        }

        // Set sliding state
        else if (isSliding)
        {
            state = MovementState.Sliding;

            if (OnSlope() && rBody.velocity.y < -0.2f)
            {
                desiredMoveSpeed = maxSlideSpeed;
            }
            else if (!isGrounded && !OnSlope())
            {
                state = MovementState.Midair;
            }
            else
            {
                desiredMoveSpeed = runSpeed;
            }
        }
        // Set crouch state
        else if (isCrouching)
        {
            state = MovementState.Crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Set run state
        else if (isGrounded && Input.GetKey(pKeybinds.runKey))
        {
            state = MovementState.Running;
            desiredMoveSpeed = runSpeed;
        }

        // Set walk state
        else if (isGrounded)
        {
            state = MovementState.Walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Set midair state
        else
        {
            state = MovementState.Midair;
        }

        // Check if the desired move speed has largely changed
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 8f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(ReduceSpeed());
        }
        else if ((horizontalInput == 0 && verticalInput == 0) || ((state == MovementState.Walking || state == MovementState.Running) && moveSpeed <= 10))
        {
            StopAllCoroutines();
            moveSpeed = desiredMoveSpeed;
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator ReduceSpeed()
    {
        // Smoothly reduces player's move speed back to the desired speed
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // Calculate players move direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // On slope
        if (OnSlope() && !exitingSlope)
        {
            rBody.AddForce(GetSlopeDirection(moveDirection) * (moveSpeed * 20f), ForceMode.Force);

            if (rBody.velocity.y > 0)
            {
                rBody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // On ground
        else if (isGrounded)
        {
            rBody.AddForce(moveDirection.normalized * (moveSpeed * 10f) + platformVelocity, ForceMode.Force);
        }
        // midair
        else if (!isGrounded)
        {
            rBody.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
        }

        // Turns gravity off while on slope
        if (!isWallrunning)
        {
            rBody.useGravity = !OnSlope();
        }
    }

    private void SpeedController()
    {
        // Limit speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rBody.velocity.magnitude > moveSpeed)
            {
                rBody.velocity = rBody.velocity.normalized * moveSpeed;
            }
        }
        // Limit speed in air or on ground
        else
        {
            Vector3 flatVelocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);

            // Clamp velocity when needed
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * (moveSpeed * speedBoost.currentSpeedMultiplier);
                rBody.velocity = new Vector3(limitedVelocity.x, rBody.velocity.y, limitedVelocity.z);
            }
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f) && isGrounded)
        {

            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle != 0 && angle < maxSlopeAngle;
        }

        return false;
    }

    public Vector3 GetSlopeDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public void ResetCamera()
    {
        cam.DoFov(defaultFov);
        cam.DoTilt(defaultTilt);
    }

    public void addPlatformVelocity(Vector3 velocity)
    {
        platformVelocity = velocity;
    }
}
