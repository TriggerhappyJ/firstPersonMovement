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
    [SerializeField] private float slideCooldown;
    private float slideTimer;
    private bool canSlide = true;
    
    [SerializeField] private float slideYScale;
    private float startYScale;
    
    [Header("Keybinds")]
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;
    
    [Header("Camera Effects")]
    [SerializeField] private PlayerCam cam;
    [SerializeField] private float camFov;
    [SerializeField] private Vector3 camTilt;
    
    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        pMovement = GetComponent<PlayerMovement>();
        
        startYScale = playerObject.localScale.y;
    }

    private void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        
        if (Input.GetKey(slideKey) && verticalInput >= 1 && (!pMovement.OnSlope() && rBody.velocity.magnitude >= 11 || pMovement.OnSlope() && rBody.velocity.y <= -0.2f) && canSlide)
        {
            canSlide = false;
            StartSlide();
            Invoke(nameof(ResetSlide), slideCooldown);
        }
        
        if (Input.GetKeyUp(slideKey) && pMovement.isSliding || pMovement.state == PlayerMovement.MovementState.midair)
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
        
        // Set camera effects
        cam.DoFov(camFov);
        cam.DoTilt(camTilt);
    }

    private void ResetSlide()
    {
        canSlide = true;
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
        
        if (slideTimer <= 0 || rBody.velocity.magnitude <= 0.1f)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        pMovement.isSliding = false;
        
        transform.localScale = new Vector3(playerObject.localScale.x, startYScale, playerObject.localScale.z);
        
        // Reset camera
        cam.DoFov(80f);
        cam.DoTilt(new Vector3(0,0,0));
    }
}
