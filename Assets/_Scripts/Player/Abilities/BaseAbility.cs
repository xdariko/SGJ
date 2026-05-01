using UnityEngine;
using UnityEngine.Events;

public abstract class BaseAbility : ScriptableObject
{
    [Header("Basic Settings")]
    [SerializeField] protected string abilityName = "New Ability";
    [SerializeField] [TextArea(3, 5)] protected string description = "Ability description";
    [SerializeField] protected Sprite icon;
    [SerializeField] protected float cooldown = 1f;
    [SerializeField] protected AudioClip activationSound;
    [SerializeField] protected bool isUnlocked = false;

    [Header("Visual Feedback")]
    [SerializeField] protected GameObject visualEffectPrefab;
    [SerializeField] protected float effectDuration = 1f;

    protected float currentCooldown;
    protected bool isOnCooldown => currentCooldown > 0f;

    public string AbilityName => abilityName;
    public string Description => description;
    public Sprite Icon => icon;
    public float Cooldown => cooldown;
    public float CurrentCooldown => currentCooldown;
    public bool IsUnlocked => isUnlocked;
    public bool CanUse => isUnlocked && !isOnCooldown;

    public event UnityAction OnAbilityUsed;
    public event UnityAction OnCooldownStarted;
    public event UnityAction OnCooldownEnded;

    protected void InvokeOnAbilityUsed()
    {
        OnAbilityUsed?.Invoke();
    }

    protected void InvokeOnCooldownStarted()
    {
        OnCooldownStarted?.Invoke();
    }

    protected void InvokeOnCooldownEnded()
    {
        OnCooldownEnded?.Invoke();
    }

    public virtual void Initialize()
    {
        currentCooldown = 0f;
    }

    public virtual bool TryUseAbility(Player player)
    {
        if (!CanUse) return false;

        UseAbility(player);
        StartCooldown();
        return true;
    }

    protected abstract void UseAbility(Player player);

    protected virtual void StartCooldown()
    {
        currentCooldown = cooldown;
        OnCooldownStarted?.Invoke();

        if (cooldown > 0)
        {
            // In a real implementation, this would use a coroutine or timer system
            // For now, we'll rely on external update calls
        }
    }

    public virtual void UpdateCooldown(float deltaTime)
    {
        if (isOnCooldown)
        {
            currentCooldown -= deltaTime;
            if (currentCooldown <= 0f)
            {
                currentCooldown = 0f;
                OnCooldownEnded?.Invoke();
            }
        }
    }

    public virtual void Unlock()
    {
        isUnlocked = true;
    }

    public virtual void Lock()
    {
        isUnlocked = false;
    }

    protected void PlayActivationSound(Vector3 position)
    {
        if (activationSound != null)
        {
            AudioSource.PlayClipAtPoint(activationSound, position);
        }
    }

    protected GameObject InstantiateVisualEffect(Vector3 position, Quaternion rotation)
    {
        if (visualEffectPrefab != null)
        {
            return Instantiate(visualEffectPrefab, position, rotation);
        }
        return null;
    }
}