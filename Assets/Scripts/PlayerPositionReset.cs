using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionReset : MonoBehaviour
{
    public Transform resetPosition;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        GameObject.Find("Player").transform.position = resetPosition.transform.position;
    }
}
