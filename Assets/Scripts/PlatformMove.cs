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

    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void FixedUpdate()
    {
        // Set platform position to move between start and end positions
        platform.position = Vector3.Lerp(startPosition, finalPosition, Mathf.PingPong(Time.time * moveSpeed, 1.0f));
    }

}
