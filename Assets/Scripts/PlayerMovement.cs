using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float movementSpeed = 12f;
    float moveSpeed;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [SerializeField] private float playerVelocity;

    Vector3 velocity;
    bool isGrounded;
    bool doublejumped;

    // Player's current velocity that can be accessed by other scripts
    public static float playerVelocityHorizontal;
    public static float playerVelocityVertical;

    // Update is called once per frame
    void Update()
    {
        // Checks if player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            doublejumped = false;
        }
        // Calculate player speed 
        playerVelocityHorizontal = controller.velocity.x;
        playerVelocityVertical = controller.velocity.z;

        // Gets player input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Moves player based on movement
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

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
        if (Input.GetButton("Fire3") && Input.GetKey("w"))
        {
            moveSpeed = movementSpeed * 2f;
        } else {
            moveSpeed = movementSpeed;
        }

        // Applies gravity to player
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        playerVelocity = velocity.z;

    }
}
