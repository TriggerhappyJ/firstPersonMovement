using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunning : MonoBehaviour
{
    [Header("Wall Running")]
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpAwayForce;
    [SerializeField] private float wallCimbSpeed;
    [SerializeField] private float maxWallRunTime;
    private float wallRunTimer;

    [Header("Key Binds")] 
    [SerializeField] private KeyCode upwardsKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode downwardsKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;

    [Header("Exiting")] 
    private bool exitingWall;
    [SerializeField] private float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    [SerializeField] private bool useGravity;
    [SerializeField] private float gravityCounterForce;

    [Header("Detection")]
    [SerializeField] private float wallDistanceCheck;
    [SerializeField] private float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;
    
    [Header("Wallrun Camera Effects")]
    [SerializeField] private float wallCamFov;
    [SerializeField] private Vector3 wallCamTilt;
    
    private PlayerMovement pMovement;
    private PlayerJump pJump;

    private void Start()
    {
        pMovement = GetComponent<PlayerMovement>();
        pJump = GetComponent<PlayerJump>();
    }
    
    private void Update()
    {
        CheckWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pMovement.isWallrunning)
        {
            WallRunMovement();
        }
    }

    private void CheckWall()
    {
        wallRight = Physics.Raycast(transform.position, pMovement.orientation.right, out rightWallHit, wallDistanceCheck, wallMask);
        wallLeft = Physics.Raycast(transform.position, -pMovement.orientation.right, out leftWallHit, wallDistanceCheck, wallMask);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, pMovement.groundMask);
    }

    private void StateMachine()
    {
        upwardsRunning = Input.GetKey(upwardsKey);
        downwardsRunning = Input.GetKey(downwardsKey);

        // Wall running state
        if ((wallLeft || wallRight) && pMovement.verticalInput > 0 && AboveGround() && !exitingWall)
        {
            // Start wall run
            if (!pMovement.isWallrunning)
            {
                StartWallRun();
            }
            
            // Wall run timer
            if (wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }
            if (wallRunTimer <= 0 && pMovement.isWallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Input.GetKeyDown(pJump.jumpKey))
            {
                WallJump();
            }
        }
        
        // Exit wall run
        else if (exitingWall)
        {
            if (pMovement.isWallrunning)
            {
                StopWallRun();
            }

            if (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            
            if (exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }
        
        else if (pMovement.isWallrunning)
        {
            StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pMovement.isWallrunning = true;

        wallRunTimer = maxWallRunTime;
        
        pMovement.rBody.velocity = new Vector3(pMovement.rBody.velocity.x, 0f, pMovement.rBody.velocity.z);

        // Set camera effects
        if (wallCamFov > 0)
        {
            pMovement.cam.DoFov(wallCamFov);
        }

        if (wallLeft)
        {
            pMovement.cam.DoTilt(wallCamTilt * -1);
        } 
        else if (wallRight)
        {
            pMovement.cam.DoTilt(wallCamTilt);
        }
    }
    
    private void WallRunMovement()
    {
        pMovement.rBody.useGravity = useGravity;
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        // If the wall forward is facing the wrong way, invert it
        if ((pMovement.orientation.forward - wallForward).magnitude > (pMovement.orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }
        
        // Forwards force
        pMovement.rBody.AddForce(wallForward * wallRunForce, ForceMode.Force);
        
        // Upwards/downwards force
        if (upwardsRunning)
        {
            pMovement.rBody.velocity = new Vector3(pMovement.rBody.velocity.x, wallCimbSpeed, pMovement.rBody.velocity.z);
        }
        else if (downwardsRunning)
        {
            pMovement.rBody.velocity = new Vector3(pMovement.rBody.velocity.x, -wallCimbSpeed, pMovement.rBody.velocity.z);
        }
        
        // Sideways force
        if (!(wallLeft && pMovement.horizontalInput > 0) && !(wallRight && pMovement.horizontalInput < 0))
        {
            pMovement.rBody.AddForce(-wallNormal * 100, ForceMode.Force);
        }
        
        // Weaken gravity
        if (useGravity)
        {
            pMovement.rBody.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }
    
    private void StopWallRun()
    {
        pMovement.isWallrunning = false;
        
        // Reset camera effects
        pMovement.ResetCamera();
    }

    private void WallJump()
    {
        // Enter exit wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        
        Vector3 forceToApply = (wallNormal * wallJumpAwayForce) + (transform.up * wallJumpUpForce);
        
        // Add jump force and reset player's y speed
        pMovement.rBody.velocity = new Vector3(pMovement.rBody.velocity.x, 0f, pMovement.rBody.velocity.z);
        pMovement.rBody.AddForce(forceToApply, ForceMode.Impulse);
    }
}
