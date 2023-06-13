using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    [Header("Swing Settings")] 
    [SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] private float horizontalThrust;
    [SerializeField] private float forwardThrust;
    [Space(10)]
    [SerializeField] private float swingSpring;
    [SerializeField] private float swingDamper;
    [SerializeField] private float swingMass;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [Header("Prediction Settings")] 
    [SerializeField] private float predictionRadius;
    [SerializeField] private Transform predictionPoint;
    [SerializeField] private bool LOSRequired;
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

        // Do swinging movement when swing has started (joint exists)
        if (joint != null)
        {
            SwingMovement();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
        CheckLineOfSight();
    }

    private void CheckForSwingPoints()
        {
            // Don't check for point if already swinging
            if (joint != null) return;
    
            // Check for point to swing to    
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
        
        pMovement.isSwinging = true;

        // Set point to swing to and joint on player
        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;
            
        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);
            
        // Distance config for joint, pulls player into the min from max distance
        joint.maxDistance = Mathf.Lerp(distanceFromPoint * 0.75f, distanceFromPoint * 0.20f, 2f);
        joint.minDistance = distanceFromPoint * 0.20f;
            
        // Swing joint config
        joint.spring = swingSpring;
        joint.damper = swingDamper;
        joint.massScale = swingMass;
        
        // Setup line renderer for rope
        lRenderer.positionCount = 2;
        currentSwingPosition = swingTip.position;
    }
    
    private void EndSwing()
    {
        // Stop Rendering rope, stop swinging state, destroy joint on player
        lRenderer.positionCount = 0;
        pMovement.isSwinging = false;
        Destroy(joint);
    }

    private void DrawRope()
    {
        // Don't draw rope if not swinging
        if (!joint) return;
        
        // Animate rope from player to point
        currentSwingPosition = Vector3.Lerp(currentSwingPosition, swingPoint, Time.deltaTime * 16f);
        
        // Set the two points of rope
        lRenderer.SetPosition(0, swingTip.position);
        lRenderer.SetPosition(1, currentSwingPosition);
    }

    private void SwingMovement()
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

    private void CheckLineOfSight()
    {
        // Check if swinging
        if (predictionHit.point == Vector3.zero) return;
        
        // Check if player has line of sight of swing point
        RaycastHit hit;
        Physics.Raycast(cam.position, predictionHit.point - cam.position, out hit, maxSwingDistance, swingMask);
        
        // Stop swing if player loses line of sight
        if (hit.point != predictionHit.point && LOSRequired)
        {

            EndSwing();
        }
    }
}
