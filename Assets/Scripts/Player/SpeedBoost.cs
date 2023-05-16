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
        if (Input.GetKeyDown(pKeybinds.speedBoostKey) && canSpeedBoost && !pMovement.isSliding)
        {
            canSpeedBoost = false;
            StartBoost();
            Invoke(nameof(ResetBoost), speedBoostDuration+speedBoostCooldown);
        }
        
        Debug.Log(currentSpeedMultiplier);
    }

    private void StartBoost()
    {
        currentSpeedMultiplier = speedBoostMultiplier;
        Invoke(nameof(StopBoost), speedBoostDuration);
        
        // Set camera effects
        if (speedBoostCamFov > 0)
        {
            pMovement.cam.DoFov(speedBoostCamFov);
        }
        pMovement.cam.DoTilt(speedBoostCamTilt);
    }

    private void StopBoost()
    {
        currentSpeedMultiplier = 1f;
        
        hud.StartCountdown(speedBoostCooldown);

        // Reset camera
        pMovement.ResetCamera();
    }

    private void ResetBoost()
    {
        canSpeedBoost = true;
    }
}
