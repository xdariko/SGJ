using UnityEngine;

[CreateAssetMenu(fileName = "AreaAttackAbility", menuName = "Player/Abilities/Area Attack Ability")]
public class AreaAttackAbility : BaseAbility
{
    [Header("Area Attack Settings")]
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject areaIndicatorPrefab;
    [SerializeField] private float indicatorDuration = 0.5f;

    protected override void UseAbility(Player player)
    {
        if (player == null) return;

        PlayActivationSound(player.transform.position);

        // Show area indicator
        if (areaIndicatorPrefab != null)
        {
            GameObject indicator = Instantiate(areaIndicatorPrefab, player.transform.position, Quaternion.identity);
            indicator.transform.localScale = new Vector3(attackRadius * 2, attackRadius * 2, 1f);
            Destroy(indicator, indicatorDuration);
        }

        // Perform area attack
        PerformAreaAttack(player.transform.position);

        InstantiateVisualEffect(player.transform.position, Quaternion.identity);
        Debug.Log($"Area attack with radius {attackRadius}!");
        InvokeOnAbilityUsed();
    }

    private void PerformAreaAttack(Vector2 center)
    {
        // Detect enemies in area
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(center, attackRadius, enemyLayer);

        foreach (var enemyCollider in hitEnemies)
        {
            if (enemyCollider.TryGetComponent<Enemy>(out var enemy))
            {
                // Deal damage to enemy
                // enemy.TakeDamage(attackDamage); // Would need to implement in Enemy class
                Debug.Log($"Area attack hit {enemy.name} for {attackDamage} damage!");
            }
        }
    }

    // Visualization helper for editor
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, attackRadius);
    }
}