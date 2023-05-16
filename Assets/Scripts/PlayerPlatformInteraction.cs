using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformInteraction : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            // Make the player a child of the platform
            other.transform.parent = transform;
        }
    }

    private void OnCollisionExit(Collision other)
    {
            // Remove the player as a child of the platform
            other.transform.parent = null;
    }
}
