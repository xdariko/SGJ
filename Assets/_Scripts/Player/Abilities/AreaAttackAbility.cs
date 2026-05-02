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

    [Header("Camera Shake")]
    [SerializeField] private float screenShakeForce = 0.6f;

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
        G.screenShake?.Shake(screenShakeForce);
        Debug.Log($"Area attack with radius {attackRadius}!");
        InvokeOnAbilityUsed();
    }

    private void PerformAreaAttack(Vector2 center)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(center, attackRadius, enemyLayer);

        foreach (var enemyCollider in hitEnemies)
        {
            IDamageable damageable = enemyCollider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"Area attack hit {enemyCollider.name} for {attackDamage} damage!");
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