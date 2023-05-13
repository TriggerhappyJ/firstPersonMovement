using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    [Header("Crouch Settings")]
    [SerializeField] private float crouchYScale;
    
    private PlayerMovement pMovement;
    private PlayerKeybinds pKeybinds;

    [Header("Slide Camera Effects")]
    [SerializeField] private float crouchCamFov;
    [SerializeField] private Vector3 crouchCamTilt;
    
    private void Start()
    {
        pMovement = GetComponent<PlayerMovement>();
        pKeybinds = GetComponent<PlayerKeybinds>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(pKeybinds.crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            pMovement.rBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            pMovement.isCrouching = true;
            
            // Set camera effects
            if (crouchCamFov > 0)
            {
                pMovement.cam.DoFov(crouchCamFov);
            }
            pMovement.cam.DoTilt(crouchCamTilt);
        }
        else if (Input.GetKeyUp(pKeybinds.crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, pMovement.startYScale, transform.localScale.z);

            pMovement.isCrouching = false;
            
            // Reset camera effects
            pMovement.ResetCamera();
        } 
    }
}
