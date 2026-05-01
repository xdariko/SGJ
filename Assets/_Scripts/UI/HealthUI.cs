using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        //var player = GameManager.Instance.GetPlayer();
        //if (player != null)
        //{
        //    player.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
        //    healthText.text = player.CurrentHealth.ToString();
        //}

    }

    private void OnDestroy()
    {
        //GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        SetHealthUI(healthEventArgs);
    }

    private void SetHealthUI(HealthEventArgs healthEventArgs)
    {
        healthText.text = healthEventArgs.Current.ToString();
    }
}
