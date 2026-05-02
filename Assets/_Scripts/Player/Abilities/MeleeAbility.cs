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

        // Get attack direction from player controller (mouse direction)
        PlayerController playerController = player.GetComponent<PlayerController>();
        Vector2 attackDirection = playerController != null ?
            playerController.GetMouseDirection() :
            (player.IsFacingRight ? Vector2.right : Vector2.left);

        // Perform melee attack
        PerformMeleeAttack(player.transform.position, attackDirection);

        Debug.Log($"Used {(isEnhanced ? "Enhanced " : "")}Melee Attack!");
        InvokeOnAbilityUsed();
    }

    private void PerformMeleeAttack(Vector2 origin, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float halfAngle = 90f;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(origin, attackRange, enemyLayer);

        foreach (var enemyCollider in hitEnemies)
        {
            Vector2 toEnemy = ((Vector2)enemyCollider.transform.position - origin).normalized;
            float angleToEnemy = Mathf.Atan2(toEnemy.y, toEnemy.x) * Mathf.Rad2Deg;
            float angleDiff = Mathf.DeltaAngle(angle, angleToEnemy);

            if (Mathf.Abs(angleDiff) > halfAngle)
                continue;

            float finalDamage = attackDamage;

            if (isEnhanced)
                finalDamage *= enhancedDamageMultiplier;

             IDamageable damageable = enemyCollider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(finalDamage);
                Debug.Log($"Hit {enemyCollider.name} with {(isEnhanced ? "enhanced " : "")}melee attack for {finalDamage} damage!");
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