using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [HideInInspector] public bool canDash = true;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashDuration;
    
    [Header("Dash Camera Effects")]
    [SerializeField] private float dashCamFov;
    [SerializeField] private Vector3 dashCamTilt;

    [Header("References")]
    private PlayerMovement pMovement;
    private PlayerKeybinds pKeybinds;
    public HUDController hud;
    
    private void Start()
    {
        pMovement = GetComponent<PlayerMovement>();
        pKeybinds = GetComponent<PlayerKeybinds>();
    }

    private void Update()
    {
        // Start dash if key is pressed and conditions are met
        if (Input.GetKeyDown(pKeybinds.dashKey) && canDash && !pMovement.OnSlope() && pMovement.moveDirection.magnitude != 0)
        {
            canDash = false;
            pMovement.isDashing = true;
            DoDash();
            Invoke(nameof(ResetDash), dashCooldown);
        }
    }

    private void DoDash()
    {
        // Freeze Y constraint to prevent player flinging off slops if hit mid dash
        pMovement.rBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        // Get input direction and do dash in that direction
        Vector3 inputDirection = pMovement.orientation.forward * pMovement.verticalInput + pMovement.orientation.right * pMovement.horizontalInput;
        pMovement.rBody.AddForce(inputDirection.normalized * dashSpeed, ForceMode.Impulse);

        // Set camera effects
        if (dashCamFov > 0)
        {
            pMovement.cam.DoFov(dashCamFov);
        }
        pMovement.cam.DoTilt(dashCamTilt);
        
        // End dash after duration
        Invoke(nameof(EndDash), dashDuration);
    }

    private void EndDash()
    {
        // Set constraints back to normal
        pMovement.rBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        
        pMovement.isDashing = false;
        
        // Reset camera
        pMovement.ResetCamera();
        pMovement.isDashing = false;
    }

    private void ResetDash()
    {
        canDash = true;
    }
}
