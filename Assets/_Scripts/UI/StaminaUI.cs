using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text staminaText;

    private void Start()
    {
        //var player = GameManager.Instance.GetPlayer();
        //if (player != null)
        //{
        //    player.staminaEvent.OnStaminaChanged += StaminaEvent_OnStaminaChanged;
        //    staminaText.text = player.CurrentStamina.ToString();
        //}

    }

    private void OnDestroy()
    {
        //GameManager.Instance.GetPlayer().staminaEvent.OnStaminaChanged -= StaminaEvent_OnStaminaChanged;
    }

    private void StaminaEvent_OnStaminaChanged(StaminaEvent staminaEvent, StaminaEventArgs staminaEventArgs)
    {
        SetStaminaUI(staminaEventArgs);
    }

    private void SetStaminaUI(StaminaEventArgs staminaEventArgs)
    {
        staminaText.text = Mathf.Round(staminaEventArgs.Current).ToString();
    }
}
