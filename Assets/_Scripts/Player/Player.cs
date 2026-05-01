using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerDetailsSO playerDetails;

    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public StaminaEvent staminaEvent;

    private float currentHealth;
    private float currentStamina;

    public float CurrentHealth => currentHealth;
    public float CurrentStamina => currentStamina;

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        staminaEvent = GetComponent<StaminaEvent>();

        currentHealth = playerDetails.maxHealth;
        currentStamina = playerDetails.maxStamina;
    }

    private void Update()
    {
        RegenerateStamina();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += OnPlayerHealthChanged;
    }

    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= OnPlayerHealthChanged;
    }

    private void OnPlayerHealthChanged(HealthEvent sender, HealthEventArgs args)
    {
        if (args.IsDead)
        {
            Debug.Log("Game Over!");
        }
    }

    public void TakeDamage(float damage)
    {
        float newHealth = Mathf.Max(0, currentHealth - damage);
        float delta = damage;
        currentHealth = newHealth;

        healthEvent.CallHealthChanged(currentHealth, playerDetails.maxHealth, delta);
    }

    private void RegenerateStamina()
    {
        if (currentStamina >= playerDetails.maxStamina) return;
        RestoreStamina(playerDetails.staminaRegenRate * Time.deltaTime);
    }

    private void RestoreStamina(float amount)
    {
        float newStamina = Mathf.Min(playerDetails.maxStamina, currentStamina + amount);
        float delta = amount;
        currentStamina = newStamina;

        staminaEvent.CallStaminaChanged(currentStamina, playerDetails.maxStamina, delta);
    }

    public bool TryUseStamina(float amount)
    {
        if (currentStamina < amount) return false;

        float newStamina = Mathf.Max(0, currentStamina - amount);
        float delta = -amount;
        currentStamina = newStamina;

        staminaEvent.CallStaminaChanged(currentStamina, playerDetails.maxStamina, delta);
        return true;
    }
}
