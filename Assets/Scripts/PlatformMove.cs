using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMove : MonoBehaviour
{

    [Header("References")] 
    private Transform platform;

    [Header("Platform Settings")] 
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 finalPosition;
    private Vector3 startPosition;

    private void Start()
    {
        platform = GetComponent<Transform>();
        startPosition = transform.position;
    }
    
    private void Update()
    {
        platform.position = Vector3.Lerp(startPosition, finalPosition, Mathf.PingPong(Time.time * moveSpeed, 1));
        
        // Move platform using force
        
    }
}
