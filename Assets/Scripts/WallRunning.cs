using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wall Running")]
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask groundMask;
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
    
    private float horizontalInput;
    private float verticalInput;

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

    [Header("References")]
    [SerializeField] private Transform orientation;
    private PlayerMovement pMovement;
    private Rigidbody rBody;
    
    [Header("Camera Effects")]
    [SerializeField] private PlayerCam cam;
    [SerializeField] private float camFov;
    [SerializeField] private Vector3 camTilt;

    private void Start()
    {
        pMovement = GetComponent<PlayerMovement>();
        rBody = GetComponent<Rigidbody>();
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
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistanceCheck, wallMask);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistanceCheck, wallMask);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundMask);
    }

    private void StateMachine()
    {
        // Get player inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        upwardsRunning = Input.GetKey(upwardsKey);
        downwardsRunning = Input.GetKey(downwardsKey);

        // Wall running state
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
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

            if (Input.GetKeyDown(pMovement.jumpKey))
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
        
        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);

        // Set camera effects
        cam.DoFov(camFov);
        if (wallLeft)
        {
            cam.DoTilt(camTilt * -1);
        } 
        else if (wallRight)
        {
            cam.DoTilt(camTilt);
        }
    }
    
    private void WallRunMovement()
    {
        rBody.useGravity = useGravity;
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        // If the wall forward is facing the wrong way, invert it
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }
        
        // Forwards force
        rBody.AddForce(wallForward * wallRunForce, ForceMode.Force);
        
        // Upwards/downwards force
        if (upwardsRunning)
        {
            rBody.velocity = new Vector3(rBody.velocity.x, wallCimbSpeed, rBody.velocity.z);
        }
        else if (downwardsRunning)
        {
            rBody.velocity = new Vector3(rBody.velocity.x, -wallCimbSpeed, rBody.velocity.z);
        }
        
        // Sideways force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        {
            rBody.AddForce(-wallNormal * 100, ForceMode.Force);
        }
        
        // Weaken gravity
        if (useGravity)
        {
            rBody.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }
    
    private void StopWallRun()
    {
        pMovement.isWallrunning = false;
        
        // Reset camera effects
        cam.DoFov(80f);
        cam.DoTilt(new Vector3(0,0,0));
    }

    private void WallJump()
    {
        // Enter exit wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        
        Vector3 forceToApply = (wallNormal * wallJumpAwayForce) + (transform.up * wallJumpUpForce);
        
        // Add jump force and reset player's y speed
        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        rBody.AddForce(forceToApply, ForceMode.Impulse);
    }
}
