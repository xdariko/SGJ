using UnityEngine;

public abstract class PlayerTransformationBase : ScriptableObject, IPlayerTransformation
{
    [SerializeField] private string transformationName;
    [SerializeField] private Sprite transformationIcon;
    [SerializeField] [TextArea(3, 5)] private string description;
    [SerializeField] private float cooldown = 10f;
    [SerializeField] private float duration = 15f;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip deactivationSound;

    protected float currentCooldown;
    protected float currentDuration;
    protected bool isActive;

    public string TransformationName => transformationName;
    public Sprite TransformationIcon => transformationIcon;
    public string Description => description;
    public float Cooldown => cooldown;

    public virtual void Activate(Player player)
    {
        if (!CanActivate(player)) return;

        isActive = true;
        currentDuration = duration;
        currentCooldown = cooldown;

        // Apply transformation effects
        ApplyTransformationEffects(player);

        if (activationSound != null && G.main != null)
        {
            AudioSource.PlayClipAtPoint(activationSound, player.transform.position);
        }
    }

    public virtual void Deactivate(Player player)
    {
        isActive = false;
        currentCooldown = cooldown;

        // Remove transformation effects
        RemoveTransformationEffects(player);

        if (deactivationSound != null && G.main != null)
        {
            AudioSource.PlayClipAtPoint(deactivationSound, player.transform.position);
        }
    }

    public virtual void UpdateTransformation(Player player)
    {
        if (isActive)
        {
            currentDuration -= Time.deltaTime;
            if (currentDuration <= 0f)
            {
                Deactivate(player);
            }
            else
            {
                UpdateActiveEffects(player);
            }
        }
        else
        {
            currentCooldown -= Time.deltaTime;
            currentCooldown = Mathf.Max(0f, currentCooldown);
        }
    }

    public virtual bool CanActivate(Player player)
    {
        return !isActive && currentCooldown <= 0f && player.CurrentHealth > 0;
    }

    protected abstract void ApplyTransformationEffects(Player player);
    protected abstract void RemoveTransformationEffects(Player player);
    protected abstract void UpdateActiveEffects(Player player);
}