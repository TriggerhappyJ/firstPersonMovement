using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Text References")] 
    [SerializeField] private TextMeshProUGUI boostText;
    
    public void StartCountdown(float countdownValue)
    {
        float currentCountdown = countdownValue;
        StartCoroutine(DoCountdown(currentCountdown));
        boostText.text = currentCountdown.ToString("F0");
    }

    private IEnumerator DoCountdown(float currentCountdown)
    {
        Debug.Log("Countdown is underway!");
        while (currentCountdown > 0)
        {
            Debug.Log("Counted down once! " + currentCountdown);
            boostText.text = currentCountdown.ToString("F0");
            yield return new WaitForSeconds(1f);
            currentCountdown--;
        }

        boostText.text = "";
    }
}
