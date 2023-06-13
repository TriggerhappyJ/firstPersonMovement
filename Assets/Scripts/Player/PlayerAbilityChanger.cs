using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityChanger : MonoBehaviour
{
    [Header("Available Settings")]
    [SerializeField] private bool boost;
    [SerializeField] private bool dash;
    [SerializeField] private bool wallrunning;
    [SerializeField] private bool grapple;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        GameObject.Find("Player").GetComponent<SpeedBoost>().enabled = boost;
        GameObject.Find("Player").GetComponent<PlayerDash>().enabled = dash;
        GameObject.Find("Player").GetComponent<PlayerWallRun>().enabled = wallrunning;
        GameObject.Find("Player").GetComponent<PlayerSwing>().enabled = grapple;
    }
}
