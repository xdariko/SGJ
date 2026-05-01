using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAbility", menuName = "Player/Abilities/Melee Ability")]
public class MeleeAbility : BaseAbility
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackWidth = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private bool isEnhanced = false;
    [SerializeField] private float enhancedDamageMultiplier = 2f;

    protected override void UseAbility(Player player)
    {
        if (player == null) return;

        PlayActivationSound(player.transform.position);
        InstantiateVisualEffect(player.transform.position, player.transform.rotation);

        // Calculate attack direction based on player's movement direction
        PlayerController playerController = player.GetComponent<PlayerController>();
        Vector2 attackDirection = playerController != null ?
            playerController.LastMoveDirection.normalized :
            (player.IsFacingRight ? Vector2.right : Vector2.left);

        // Perform melee attack
        PerformMeleeAttack(player.transform.position, attackDirection);

        Debug.Log($"Used {(isEnhanced ? "Enhanced " : "")}Melee Attack!");
        InvokeOnAbilityUsed();
    }

    private void PerformMeleeAttack(Vector2 origin, Vector2 direction)
    {
        // Create attack box
        Vector2 attackSize = new Vector2(attackRange, attackWidth);

        // Calculate rotation based on direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Detect enemies in attack range
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            origin + direction * (attackRange / 2),
            attackSize,
            angle,
            Vector2.zero,
            0f,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<Enemy>(out var enemy))
            {
                float finalDamage = attackDamage;
                if (isEnhanced)
                {
                    finalDamage *= enhancedDamageMultiplier;
                }

                // In a real implementation, this would call enemy.TakeDamage(finalDamage)
                Debug.Log($"Hit {enemy.name} with {(isEnhanced ? "enhanced " : "")}melee attack for {finalDamage} damage!");
            }
        }
    }

    public void SetEnhanced(bool enhanced)
    {
        isEnhanced = enhanced;
    }

    public override string ToString()
    {
        return $"{abilityName} {(isEnhanced ? "(Enhanced)" : "")} - Damage: {attackDamage}";
    }
}