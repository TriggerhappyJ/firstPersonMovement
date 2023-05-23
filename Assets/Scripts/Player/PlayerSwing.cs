using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    [Header("Swing Settings")] 
    [SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] private float horizontalThrust;
    [SerializeField] private float forwardThrust;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [Header("Prediction Settings")] 
    [SerializeField] private float predictionRadius;
    [SerializeField] private Transform predictionPoint;
    public RaycastHit predictionHit;
    
    [Header("References")] 
    [SerializeField] private LineRenderer lRenderer;
    [SerializeField] private Transform swingTip, cam, player;
    [SerializeField] private LayerMask swingMask;
    private PlayerKeybinds pKeybinds;
    private PlayerMovement pMovement;

    private Vector3 currentSwingPosition;

    private void Start()
    {
        pKeybinds = GetComponent<PlayerKeybinds>();
        pMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckForSwingPoints();
        
        // Start and stop swing on key press
        if (Input.GetKeyDown(pKeybinds.swingKey))
        {
            StartSwing();
        }

        if (Input.GetKeyUp(pKeybinds.swingKey))
        {
            EndSwing();
        }

        if (joint != null)
        {
            SwingMovemet();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void CheckForSwingPoints()
        {
            if (joint != null) return;
    
            RaycastHit sphereCastHit;
            Physics.SphereCast(cam.position, predictionRadius, cam.forward, out sphereCastHit, maxSwingDistance, swingMask);
            
            RaycastHit rayCastHit;
            Physics.Raycast(cam.position, cam.forward, out rayCastHit, maxSwingDistance, swingMask);
    
            Vector3 realHitPoint;
    
            // Check for swing point directly
            if (rayCastHit.point != Vector3.zero)
            {
                realHitPoint = rayCastHit.point;
            } 
            // Check for swing point with sphere cast
            else if (sphereCastHit.point != Vector3.zero)
            {
                realHitPoint = sphereCastHit.point;
            }
            // No swing point found
            else
            {
                realHitPoint = Vector3.zero;
            }
            
            // Set prediction point
            if (realHitPoint != Vector3.zero)
            {
                predictionPoint.gameObject.SetActive(true);
                predictionPoint.position = realHitPoint;
            }
            // Remove prediction point
            else
            {
                predictionPoint.gameObject.SetActive(false);
            }
            
            predictionHit = rayCastHit.point == Vector3.zero ? sphereCastHit : rayCastHit;
        }
    
    private void StartSwing()
    {
        // Check if predictionHit is not null
        if (predictionHit.point == Vector3.zero) return;
            
        Debug.Log("Set prediction point " + predictionHit.point);
            
        pMovement.isSwinging = true;

        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;
            
        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);
            
        // The distance grapple will try to keep from grapple point
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;
            
        // Swing joint config
        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;
            
        lRenderer.positionCount = 2;
        currentSwingPosition = swingTip.position;
        
    }
    
    private void EndSwing()
    {
        lRenderer.positionCount = 0;
        pMovement.isSwinging = false;
        Destroy(joint);
    }

    private void DrawRope()
    {
        // Don't draw rope if not swinging
        if (!joint) return;
        
        currentSwingPosition = Vector3.Lerp(currentSwingPosition, swingPoint, Time.deltaTime * 16f);
        
        lRenderer.SetPosition(0, swingTip.position);
        lRenderer.SetPosition(1, currentSwingPosition);
    }

    private void SwingMovemet()
    {
        // Right movement
        if (Input.GetKey(pKeybinds.rightKey))
        {
            pMovement.rBody.AddForce(pMovement.orientation.right * (horizontalThrust * Time.deltaTime));
        }
        
        // Left movement
        if (Input.GetKey(pKeybinds.leftKey))
        {
            pMovement.rBody.AddForce(-pMovement.orientation.right * (horizontalThrust * Time.deltaTime));
        }

        // Forward movement
        if (Input.GetKey(pKeybinds.forwardKey))
        {
            pMovement.rBody.AddForce(pMovement.orientation.forward * (forwardThrust * Time.deltaTime));
        }
    }

    
}
