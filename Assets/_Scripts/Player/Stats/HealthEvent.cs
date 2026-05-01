using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthEvent : MonoBehaviour
{
    public event Action<HealthEvent, HealthEventArgs> OnHealthChanged;

    public void CallHealthChanged(float currentHealth, float maxHealth, float deltaHealth)
    {
        OnHealthChanged?.Invoke(this, new HealthEventArgs() { Percent = (float) currentHealth /  maxHealth , Current = currentHealth, Max = maxHealth, Delta = deltaHealth});
    }
}

public class HealthEventArgs : StatEventArgs
{
    public bool IsDead => Current <= 0;
}