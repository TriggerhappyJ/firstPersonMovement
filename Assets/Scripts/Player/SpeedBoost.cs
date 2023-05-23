using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    [SerializeField] private float speedBoostMultiplier;
    [SerializeField] private float speedBoostDuration;
    [SerializeField] private float speedBoostCooldown;
    
    [HideInInspector] public bool canSpeedBoost = true;
    [HideInInspector] public float currentSpeedMultiplier;
    
    [Header("Speed Boost Camera Effects")]
    [SerializeField] private float speedBoostCamFov;
    [SerializeField] private Vector3 speedBoostCamTilt;

    [Header("References")]
    private PlayerMovement pMovement;
    private PlayerKeybinds pKeybinds;
    public HUDController hud;

    private void Start()
    {
        pMovement = GetComponent<PlayerMovement>();
        pKeybinds = GetComponent<PlayerKeybinds>();
        currentSpeedMultiplier = 1f;
    }
    
    private void Update()
    {
        // Activate speed boost if key pressed and boost is available
        if (Input.GetKeyDown(pKeybinds.speedBoostKey) && canSpeedBoost && !pMovement.isSliding && !pMovement.isSwinging)
        {
            canSpeedBoost = false;
            pMovement.isBoosting = true;
            StartBoost();
            Invoke(nameof(ResetBoost), speedBoostDuration+speedBoostCooldown);
        }
    }

    private void FixedUpdate()
    {
        if (pMovement.isBoosting && !pMovement.isSwinging)
        {
            BoostMovement();
        }
    }

    private void StartBoost()
    {
        // Get current multiplier and start duration countdown
        currentSpeedMultiplier = speedBoostMultiplier;
        Invoke(nameof(StopBoost), speedBoostDuration);
        
        // Set camera effects
        if (speedBoostCamFov > 0)
        {
            pMovement.cam.DoFov(speedBoostCamFov);
        }
        pMovement.cam.DoTilt(speedBoostCamTilt);
    }

    private void BoostMovement()
    {
        // Get input direction and add boost speed to player's movement
        Vector3 inputDirection = pMovement.orientation.forward * pMovement.verticalInput + pMovement.orientation.right * pMovement.horizontalInput;

        pMovement.rBody.AddForce(inputDirection.normalized * currentSpeedMultiplier, ForceMode.Force);
    }

    private void StopBoost()
    {
        // Reset speed multiplier and set boost to false
        currentSpeedMultiplier = 1f;

        pMovement.isBoosting = false;
        
        hud.StartCountdown(speedBoostCooldown);

        // Reset camera
        pMovement.ResetCamera();
    }

    private void ResetBoost()
    {
        canSpeedBoost = true;
    }
}
