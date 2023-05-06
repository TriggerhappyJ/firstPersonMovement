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
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            // Start wall run
            if (!pMovement.isWallrunning)
            {
                StartWallRun();
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
    }
    
    private void WallRunMovement()
    {
        rBody.useGravity = false;
        rBody.velocity = new Vector3(rBody.velocity.x, 0f, rBody.velocity.z);
        
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
    }
    
    private void StopWallRun()
    {
        pMovement.isWallrunning = false;
    }
}
