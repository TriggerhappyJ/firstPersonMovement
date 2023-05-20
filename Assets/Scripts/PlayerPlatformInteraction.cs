using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformInteraction : MonoBehaviour
{
    private Rigidbody rBody;
    private PlatformMove platMove;
    
    private bool playerIsTouching;
    private PlayerMovement pMovement;
    private Vector3 oldPosition;
    [HideInInspector] public Vector3 platformVelocity;

    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        platMove = GetComponent<PlatformMove>();
    }

    private void FixedUpdate()
    {
        // Calculate platforms velocity
        var newPosition = rBody.position;
        var platformDifference = newPosition - oldPosition;
        platformVelocity = platformDifference / Time.fixedDeltaTime;
        
        oldPosition = newPosition;
        
        if (playerIsTouching && pMovement)
        {
            Debug.Log("Sending velocity! "+platformVelocity);
            pMovement.addPlatformVelocity(platformVelocity);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        
        other.transform.parent = transform;
        
        if (pMovement == null)
        {
            pMovement = other.transform.GetComponent<PlayerMovement>();
        }
        
        playerIsTouching = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        pMovement.addPlatformVelocity(Vector3.zero);
        playerIsTouching = false;
        
        other.transform.parent = null;
    }
}
