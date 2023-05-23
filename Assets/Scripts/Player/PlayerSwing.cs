using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    [Header("Swing Settings")] 
    [SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] private float horizontalThrust;
    [SerializeField] private float forwardThrust;
    private Vector3 swingPoint;
    private SpringJoint joint;

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

    private void StartSwing()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, swingMask))
        {
            pMovement.isSwinging = true;

            swingPoint = hit.point;
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
        
        currentSwingPosition = Vector3.Lerp(currentSwingPosition, swingPoint, Time.deltaTime * 8f);
        
        lRenderer.SetPosition(0, swingTip.position);
        lRenderer.SetPosition(1, swingPoint);
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
