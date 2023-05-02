using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObject;
    private Rigidbody rBody;
    private PlayerMovement pMovement;
    
    [Header("Slide Settings")]
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideForce;
    private float slideTimer;
    
    [SerializeField] private float slideYScale;
    private float startYScale;
    
    [Header("Keybinds")]
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;
    
    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        pMovement = GetComponent<PlayerMovement>();
        
        startYScale = playerObject.localScale.y;
    }

    private void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        
        if (Input.GetKeyDown(slideKey) && (verticalInput >= 1) && (pMovement.state == PlayerMovement.MovementState.running && !pMovement.OnSlope() || pMovement.OnSlope() && rBody.velocity.y <= -0.2f))
        {
            StartSlide();
        }
        
        if (Input.GetKeyUp(slideKey) && pMovement.isSliding)
        {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if (pMovement.isSliding)
        {
            SlidingMovement();
        }
    }

    private void StartSlide()
    {
        pMovement.isSliding = true;
        
        transform.localScale = new Vector3(playerObject.localScale.x, slideYScale, playerObject.localScale.z);
        rBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        
        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Sliding on flat surface
        if (!pMovement.OnSlope() || rBody.velocity.y > -0.1f)
        {
            rBody.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
        
            slideTimer -= Time.deltaTime;
        } 
        // Sliding down slope
        else
        {
            rBody.AddForce(pMovement.GetSlopeDirection(inputDirection) * slideForce, ForceMode.Force);
        }
        
        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        pMovement.isSliding = false;
        
        transform.localScale = new Vector3(playerObject.localScale.x, startYScale, playerObject.localScale.z);
    }
}
