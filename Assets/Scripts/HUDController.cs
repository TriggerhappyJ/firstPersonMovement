using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Text References")] 
    [SerializeField] private TextMeshProUGUI cooldownText;

    public void StartCountdown(float countdownValue)
    {
        // Get the countdown value and start the countdown
        float currentCountdown = countdownValue;
        StartCoroutine(DoCountdown(currentCountdown));
    }

    private IEnumerator DoCountdown(float currentCountdown)
    {
        // Display the countdown and update text on HUD
        while (currentCountdown > 0)
        {
            cooldownText.text = currentCountdown.ToString("F0");
            yield return new WaitForSeconds(1f);
            currentCountdown--;
        }

        cooldownText.text = "";
    }
}
