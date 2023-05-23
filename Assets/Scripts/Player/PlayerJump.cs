using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [HideInInspector] public bool canJump = true;
    [HideInInspector] public bool canDoubleJump = true;
    
    [Header("Camera Effects")]
    [SerializeField] private float jumpCamFov;
    [SerializeField] private Vector3 jumpCamTilt;
    
    private PlayerMovement pMovement;
    private PlayerKeybinds pKeybinds;

    private Vector3 platformVelocity;
    
    private void Start()
    {
        pMovement = GetComponent<PlayerMovement>();
        pKeybinds = GetComponent<PlayerKeybinds>();
    }

    private void Update()
    {
        // Set double jump availability
        if (pMovement.isGrounded || pMovement.isWallrunning)
        {
            canDoubleJump = true;
        }
        
        // Jump logic
        if (Input.GetKey(pKeybinds.jumpKey) && canJump && pMovement.isGrounded && pMovement.state != PlayerMovement.MovementState.Wallrunning)
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        } 
        else if (Input.GetKeyDown(pKeybinds.jumpKey) && !pMovement.isGrounded && canDoubleJump && pMovement.state != PlayerMovement.MovementState.Crouching && pMovement.state != PlayerMovement.MovementState.Wallrunning)
        {
            canDoubleJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void Jump()
    {
        pMovement.exitingSlope = true;
        
        // Reset y velocity
        pMovement.rBody.velocity = new Vector3(pMovement.rBody.velocity.x, 0f, pMovement.rBody.velocity.z);
        
        pMovement.rBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
        // Add camera effects
        if (jumpCamFov > 0)
        {
            pMovement.cam.DoFov(jumpCamFov);
        }
        pMovement.cam.DoTilt(jumpCamTilt);
    }

    // Made to fix a problem with downwards velocity (turned out to not be a jump issue)
    // Not in use as simplier way also works but could be useful in the future
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

        pMovement.exitingSlope = false;
        pMovement.ResetCamera();
    }
}
