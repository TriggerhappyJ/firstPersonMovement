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
        if (Input.GetKeyDown(pKeybinds.dashKey) && canDash && !pMovement.OnSlope())
        {
            canDash = false;
            pMovement.isDashing = true;
            DoDash();
            Invoke(nameof(ResetDash), dashCooldown);
        }
    }

    private void DoDash()
    {
        pMovement.rBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        Vector3 inputDirection = pMovement.orientation.forward * pMovement.verticalInput + pMovement.orientation.right * pMovement.horizontalInput;
        
        pMovement.rBody.AddForce(inputDirection.normalized * dashSpeed, ForceMode.Impulse);

            // Set camera effects
        if (dashCamFov > 0)
        {
            pMovement.cam.DoFov(dashCamFov);
        }
        pMovement.cam.DoTilt(dashCamTilt);
        
        Invoke(nameof(EndDash), dashDuration);
    }

    private void EndDash()
    {
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
