using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    [HideInInspector] public bool isSliding;

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
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
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

    private float horizontalInput;
    private float verticalInput;
    
    private Vector3 moveDirection;

    private Rigidbody rBody;

    public MovementState state;
    public float playerVelocity;
    
    public enum MovementState
    {
        walking,
        running,
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
        // Check for ground under player
        if (state == MovementState.crouching)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, crouchYScale * 0.5f + 0.3f, groundMask);
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, groundMask);
        }

        Inputs();
        SpeedController();
        StateHandler();
        
        // Apply drag, set double jump availability
        if (isGrounded)
        {
            canDoubleJump = true;
            rBody.drag = groundDrag;
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
    }

    private void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        // Jump logic
        if (Input.GetKey(jumpKey) && canJump && isGrounded)
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        } 
        else if (Input.GetKeyDown(jumpKey) && !isGrounded && canDoubleJump && state != MovementState.crouching)
        {
            canDoubleJump = false;
            Jump();
        }
        
        // Start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
        
    }

    private void StateHandler()
    {
        // Set sliding state
        if (isSliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rBody.velocity.y > 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = runSpeed;
            }
        }
        
        // Set crouch state
        else if (Input.GetKey(crouchKey))
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

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(ReduceSpeed());
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

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
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
            rBody.AddForce(GetSlopeDirection(moveDirection) * (desiredMoveSpeed * 20f), ForceMode.Force);

            if (rBody.velocity.y > 0)
            {
                rBody.AddForce(Vector3.down * 120f, ForceMode.Force);
            }
        }
        
        // On ground
        else if (isGrounded)
        {
            rBody.AddForce(moveDirection.normalized * (desiredMoveSpeed * 10f), ForceMode.Force);
        } 
        // midair
        else if (!isGrounded)
        {
            rBody.AddForce(moveDirection.normalized * (desiredMoveSpeed * 10f * airMultiplier), ForceMode.Force);
        }
        
        // Turns gravity off while on slope
        rBody.useGravity = !OnSlope();
    }

    private void Jump()
    {
        exitingSlope = true;
        
        // Reset y velocity
        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        
        rBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;

        exitingSlope = false;
    }

    private void SpeedController()
    {
        // Limit speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rBody.velocity.magnitude > desiredMoveSpeed)
            {
                rBody.velocity = rBody.velocity.normalized * desiredMoveSpeed;
            }
        }
        // Limit speed in air or on ground
        else
        {
            Vector3 flatVelocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        
            // Clamp velocity when needed
            if (flatVelocity.magnitude > desiredMoveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * desiredMoveSpeed;
                rBody.velocity = new Vector3(limitedVelocity.x, rBody.velocity.y, limitedVelocity.z);
            }
        }
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f))
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
