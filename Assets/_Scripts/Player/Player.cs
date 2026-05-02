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
    private bool isInvincible = false;
    private bool isFacingRight = true;

    public float CurrentHealth => currentHealth;
    public float CurrentStamina => currentStamina;
    public float MaxHealth => playerDetails.maxHealth;
    public float MaxStamina => playerDetails.maxStamina;
    public bool IsFacingRight => isFacingRight;
    public bool IsInvincible => isInvincible;

    public event System.Action<bool> OnEnhancedAttackAvailable;

    public void InvokeOnEnhancedAttackAvailable(bool available)
    {
        OnEnhancedAttackAvailable?.Invoke(available);
    }

    private void Awake()
    {
        G.player = this;

        healthEvent = GetComponent<HealthEvent>();
        staminaEvent = GetComponent<StaminaEvent>();

        currentHealth = playerDetails.maxHealth;
        currentStamina = playerDetails.maxStamina;

        // Ensure lower body collider exists for environment collisions
        if (GetComponent<PlayerColliderSetup>() == null)
        {
            gameObject.AddComponent<PlayerColliderSetup>();
        }
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

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        Debug.Log($"Player invincibility set to: {invincible}");
    }

    public void SetFacingRight(bool facingRight)
    {
        isFacingRight = facingRight;

        // Flip sprite
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void TakeDamage(float damage, GameObject attacker = null)
    {
        if (isInvincible)
        {
            Debug.Log("Player is invincible, damage blocked!");
            return;
        }

        float newHealth = Mathf.Max(0, currentHealth - damage);
        float delta = damage;
        currentHealth = newHealth;

        healthEvent.CallHealthChanged(currentHealth, playerDetails.maxHealth, delta);

        if (newHealth <= 0)
        {
            // Player died - would need to implement death logic
            Debug.Log("Player died!");
        }
    }
}