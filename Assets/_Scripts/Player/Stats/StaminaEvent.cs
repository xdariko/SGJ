using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaEvent : MonoBehaviour
{
    public event Action<StaminaEvent, StaminaEventArgs> OnStaminaChanged;

    public void CallStaminaChanged(float currentStamina, float maxStamina, float deltaStamina)
    {
        OnStaminaChanged?.Invoke(this, new StaminaEventArgs() { Percent = (float)currentStamina / maxStamina, Current = currentStamina, Max = maxStamina, Delta = deltaStamina });
    }
}

public class StaminaEventArgs : StatEventArgs
{
    public bool IsExhausted => Current <= 0;
}
