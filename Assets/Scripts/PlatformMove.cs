using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMove : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform platform;

    [Header("Platform Settings")] 
    [SerializeField] private float moveSpeed;
    public Vector3 finalPosition;
    private Vector3 startPosition;
    private Rigidbody rBody;
    
    private bool playerIsTouching;
    private PlayerMovement pMovement;
    private Vector3 oldPosition;
    [HideInInspector] public Vector3 platformVelocity;

    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        
        //platform = GetComponent<Transform>();
        startPosition = transform.position;
        oldPosition = startPosition;
    }
    
    private void FixedUpdate()
    {
        platform.position = Vector3.Lerp(startPosition, finalPosition, Mathf.PingPong(Time.time * moveSpeed, 1));

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
    }
}
