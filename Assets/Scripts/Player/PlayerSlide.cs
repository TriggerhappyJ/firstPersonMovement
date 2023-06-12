using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerObject;
    private PlayerMovement pMovement;
    private PlayerKeybinds pKeybinds;
    
    [Header("Slide Settings")]
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideForce;
    [SerializeField] private float slideCooldown;
    private float slideTimer;
    private bool canSlide = true;
    
    [SerializeField] private float slideYScale;
  
    [Header("Slide Camera Effects")]
    [SerializeField] private float slideCamFov;
    [SerializeField] private Vector3 slideCamTilt;
    
    private void Start()
    {
        pMovement = GetComponent<PlayerMovement>();
        pKeybinds = GetComponent<PlayerKeybinds>();
    }

    private void Update()
    {
        Debug.Log((pMovement.OnSlope()));
    
        // Start slide if key is pressed and conditions are met
        if (Input.GetKey(pKeybinds.slideKey) && pMovement.verticalInput >= 1 && (!pMovement.OnSlope() && pMovement.rBody.velocity.magnitude >= 12 || pMovement.OnSlope() && pMovement.rBody.velocity.y <= -0.2f) && canSlide && pMovement.isGrounded)
        {
            canSlide = false;
            StartSlide();
            Invoke(nameof(ResetSlide), slideCooldown);
        }
        
        // Stop slide when key is released or player stops moving (hits wall)
        if ((Input.GetKeyUp(pKeybinds.slideKey) && pMovement.isSliding || pMovement.rBody.velocity.magnitude <= 0.1f) && pMovement.isSliding)
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
        
        // Set player height and move them down into slide
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        pMovement.rBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        
        slideTimer = maxSlideTime;
        
        // Set camera effects
        if (slideCamFov > 0)
        {
            pMovement.cam.DoFov(slideCamFov);
        }
        pMovement.cam.DoTilt(slideCamTilt);
    }

    private void ResetSlide()
    {
        canSlide = true;
    }
    
    private void SlidingMovement()
    {
        // Get players input direction
        Vector3 inputDirection = pMovement.orientation.forward * pMovement.verticalInput + pMovement.orientation.right * pMovement.horizontalInput;

        // Sliding on flat surface
        if (!pMovement.OnSlope() || pMovement.rBody.velocity.y > -0.1f)
        {
            pMovement.rBody.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
        
            slideTimer -= Time.deltaTime;
        } 
        // Sliding down slope
        else
        {
            pMovement.rBody.AddForce(pMovement.GetSlopeDirection(inputDirection) * slideForce, ForceMode.Force);
        }
        
        if (slideTimer <= 0 || pMovement.rBody.velocity.magnitude <= 0.1f)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        // Set sliding state to false and reset players height back to normal
        pMovement.isSliding = false;

        transform.localScale = new Vector3(transform.localScale.x, pMovement.startYScale, transform.localScale.z);
        
        // Reset camera
        pMovement.ResetCamera();
    }
}
