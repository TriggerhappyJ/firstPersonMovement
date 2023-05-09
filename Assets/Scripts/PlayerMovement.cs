using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float maxSlideSpeed;
    [SerializeField] private float maxWallRunSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    
    [HideInInspector] public bool isSliding;
    [HideInInspector] public bool isWallrunning;
    [HideInInspector] public bool isCrouching;
    
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;
    
    [SerializeField] private float groundDrag;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    
    private bool canJump = true;
    private bool canDoubleJump = true;
    

    [Header("Crouch Settings")] 
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    private float startYScale;
    
        
    [Header("Keybinds")] 
    public KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        
    [Header("Ground Check")] 
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool isGrounded;
    
    [Header("Slope Check")]
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    
    [SerializeField] private Transform orientation;
    [SerializeField] private TextMeshProUGUI velocityText;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("Camera Effects")]
    [SerializeField] private PlayerCam cam;
    [SerializeField] private Vector3 camTilt;

    private float horizontalInput;
    private float verticalInput;
    
    private Vector3 moveDirection;

    private Rigidbody rBody;

    [HideInInspector] public MovementState state;
    private float playerVelocity;
    
    public enum MovementState
    {
        walking,
        running,
        wallrunning,
        crouching,
        sliding,
        midair
    }

    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.freezeRotation = true;
        
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // Check if player is on the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, groundMask);

        Inputs();
        SpeedController();
        StateHandler();
        
        // Apply drag, set double jump availability
        if (isGrounded)
        {
            canDoubleJump = true;
            rBody.drag = groundDrag;
        }
        else if (isWallrunning)
        {
            canDoubleJump = true;
        }
        else
        {
            rBody.drag = 0;
        }
    }

    private void FixedUpdate()
    {
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
        
        // Jump logic
        if (Input.GetKey(jumpKey) && canJump && isGrounded && state != MovementState.wallrunning)
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        } 
        else if (Input.GetKeyDown(jumpKey) && !isGrounded && canDoubleJump && state != MovementState.crouching && state != MovementState.wallrunning)
        {
            canDoubleJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        
        // Start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            isCrouching = true;
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            isCrouching = false;
        }
        
    }

    private void StateHandler()
    {
        // Set wall running state
        if (isWallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = maxWallRunSpeed;
        }
        
        // Set sliding state
        else if (isSliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rBody.velocity.y < -0.2f)
            {
                desiredMoveSpeed = maxSlideSpeed;
            }
            else if (!isGrounded && !OnSlope())
            {
                state = MovementState.midair;
            }
            else
            {
                desiredMoveSpeed = runSpeed;
            }
        }
        // Set crouch state
        else if (isCrouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        
        // Set run state
        else if (isGrounded && Input.GetKey(runKey))
        {
            state = MovementState.running;
            desiredMoveSpeed = runSpeed;
        }
        
        // Set walk state
        else if (isGrounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        
        // Set midair state
        else
        {
            state = MovementState.midair;
        }
        
        // Check if the desired move speed has largely changed
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 8f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(ReduceSpeed());
        }
        else if((horizontalInput == 0 && verticalInput == 0) || ((state == MovementState.walking || state == MovementState.running) && moveSpeed <= 15))
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
            rBody.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
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

    private void Jump()
    {
        exitingSlope = true;
        
        // Reset y velocity
        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        
        rBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
        // Add camera effects
        cam.DoTilt(camTilt);
    }

    // Made to fix problem with downwards velocity (turns out the jump wasn't the issue :/)
    /*private void DoubleJump()
    {
        // If player is moving downwards, cancel out their downwards velocity and then jump
        if (rBody.velocity.y <= 0)
        {
            Vector3 velocityReverse = new Vector3(0f, rBody.velocity.y * -1, 0f);
            rBody.AddForce(transform.up * jumpForce + velocityReverse, ForceMode.Impulse);
            return;
        }
        
        // If the player is already moving upwards, just jump
        rBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }*/

    private void ResetJump()
    {
        canJump = true;

        exitingSlope = false;
        cam.DoTilt(new Vector3(0,0,0));
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
            Vector3 flatVelocity = new Vector3(rBody.velocity.x, rBody.velocity.y, rBody.velocity.z);
            // Debug.Log(flatVelocity.normalized + " vs " + moveSpeed);

            // Clamp velocity when needed
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                Debug.Log(limitedVelocity);
                rBody.velocity = new Vector3(limitedVelocity.x, rBody.velocity.y, limitedVelocity.z);
            }
        }
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
