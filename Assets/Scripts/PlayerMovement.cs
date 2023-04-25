using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    private readonly float _movementSpeed = 12f;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float sprintDecaySpeed;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 3f;

    [Header("Ground Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Player Data")]
    [SerializeField] private float playerVerticalVelocity;

    private Vector3 velocity;
    private bool isGrounded;
    private bool doublejumped;

    private CharacterController cController;
    private Rigidbody rBody;

    private bool resettingSpeed = false;
    private IEnumerator resetSpeed;

    private void Start()
    {
        cController = GetComponent<CharacterController>();
        rBody = GetComponent<Rigidbody>();
        moveSpeed = _movementSpeed;
        resetSpeed = ResetSpeed();
    }

    // Update is called once per frame
    private void Update()
    {
        // Checks if player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            doublejumped = false;
        }

        // Gets player input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Moves player based on movement
        Vector3 move = transform.right * x + transform.forward * z;
        cController.Move(move * (moveSpeed * Time.deltaTime));

        // Player jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Player doublejump
        if (Input.GetButtonDown("Jump") && !isGrounded && !doublejumped)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            doublejumped = true;
        }

        // Player sprint
        if (Input.GetButtonDown("Fire3") && Input.GetKey("w") && isGrounded)
        {
            // Resets the coroutine and stops it 
            StopCoroutine(resetSpeed);
            resetSpeed = ResetSpeed();
            resettingSpeed = false;
            
            // Adds sprint modifier to movement speed
            moveSpeed = _movementSpeed + sprintSpeed;
        }
        else if (Input.GetButtonUp("Fire3"))
        {
            if (!resettingSpeed)
            {
                // Sets movement speed back to normal over time
                StartCoroutine(resetSpeed);
            }
        }

        // Applies gravity to player    
        velocity.y += gravity * Time.deltaTime;
        cController.Move(velocity * Time.deltaTime);
        
        // Calculate player's velocity 
        playerVerticalVelocity = cController.velocity.y;
        
        Debug.Log(resettingSpeed);
    }   
    
    IEnumerator ResetSpeed()
    {
        resettingSpeed = true;
        Debug.Log("Resetting speed");
        while (moveSpeed > _movementSpeed+0.1f)
        {
            if (resettingSpeed)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, _movementSpeed, sprintDecaySpeed);
                yield return null;
            }
            else
            {
                break;
            }
        }

        // Hard sets the move speed back to normal after loop
        moveSpeed = _movementSpeed;
        Debug.Log("Done resetting");
        resettingSpeed = false;
    }
}
