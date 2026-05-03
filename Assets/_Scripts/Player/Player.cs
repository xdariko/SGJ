using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerDetailsSO playerDetails;
    [SerializeField] private GameObject deathPanelPrefab;

    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public StaminaEvent staminaEvent;

    private HitEffect hitEffect;

    private float currentHealth;
    private float currentStamina;
    private bool isInvincible = false;
    private bool isFacingRight = true;
    private bool isDead = false;

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
        hitEffect = GetComponent<HitEffect>();

        currentHealth = playerDetails.maxHealth;
        currentStamina = playerDetails.maxStamina;

        // Ensure lower body collider exists for environment collisions
        if (GetComponent<PlayerColliderSetup>() == null)
        {
            gameObject.AddComponent<PlayerColliderSetup>();
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(float damage, GameObject attacker = null)
    {
        if (isInvincible)
        {
            Debug.Log("Player is invincible, damage blocked!");
            return;
        }

        if (damage <= 0f)
            return;

        float newHealth = Mathf.Max(0, currentHealth - damage);
        float delta = damage;
        currentHealth = newHealth;

        if (hitEffect != null)
            hitEffect.ApplyHitEffect();

        healthEvent.CallHealthChanged(currentHealth, playerDetails.maxHealth, delta);

        if (newHealth <= 0)
        {
            Die();
        }
    }

    private void Update()
    {
        RegenerateStamina();
        RegenerateHealth();
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
    private void RegenerateHealth()
    {
        if (currentHealth <= 0f) return;
        if (currentHealth >= playerDetails.maxHealth) return;

        RestoreHealth(playerDetails.healthRegenRate * Time.deltaTime);
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f)
            return;

        if (currentHealth <= 0f)
            return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(playerDetails.maxHealth, currentHealth + amount);

        float delta = currentHealth - oldHealth;

        if (delta <= 0f)
            return;

        healthEvent.CallHealthChanged(currentHealth, playerDetails.maxHealth, delta);
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

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Player died!");

        SpawnDeathPanel();

        Destroy(gameObject);
    }

    private void SpawnDeathPanel()
    {
        if (deathPanelPrefab == null)
        {
            Debug.LogWarning("Player: Death panel prefab is not assigned.", this);
            return;
        }

        if (G.ui == null)
        {
            Debug.LogWarning("Player: G.ui is null, cannot spawn death panel.", this);
            return;
        }

        Instantiate(deathPanelPrefab, G.ui.transform);
    }
}