using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionReset : MonoBehaviour
{
    private Transform player;
    public Transform resetPosition;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            Debug.Log("Player entered!");
            player.transform.position = resetPosition.transform.position;
        }
    }
}
