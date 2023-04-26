using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool canJump = true;
    [SerializeField] private bool canDoubleJump;

    [Header("Keybinds")] 
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        
    [Header("Ground Check")] 
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool isGrounded;
    
    [SerializeField] private Transform orientation;

    private float horizontalInput;
    private float verticalInput;
    
    private Vector3 moveDirection;

    private Rigidbody rBody;


    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.freezeRotation = true;
    }

    private void Update()
    {
        // Check for ground under player
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);
        
        Inputs();
        SpeedController();
        
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
        else if (Input.GetKeyDown(jumpKey) && !isGrounded && canDoubleJump)
        {
            canDoubleJump = false;
            Jump();
        }
    }

    private void MovePlayer()
    {
        // Calculate players move direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isGrounded)
        {
            rBody.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        } 
        else if (!isGrounded)
        {
            rBody.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
        }
    }

    private void Jump()
    {
        // Reset y velocity
        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        
        rBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private void SpeedController()
    {
        Vector3 flatVelocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        
        // Clamp velocity when needed
        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rBody.velocity = new Vector3(limitedVelocity.x, rBody.velocity.y, limitedVelocity.z);
        }
    }
}
