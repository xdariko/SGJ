using UnityEngine;

[CreateAssetMenu(fileName = "AreaAttackAbility", menuName = "Player/Abilities/Area Attack Ability")]
public class AreaAttackAbility : BaseAbility
{
    [Header("Area Attack Settings")]
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effect")]
    [SerializeField] private float effectScaleMultiplier = 2f;

    [Header("Camera Shake")]
    [SerializeField] private float screenShakeForce = 0.6f;

    protected override void UseAbility(Player player)
    {
        if (player == null)
            return;

        Vector3 attackPosition = player.transform.position;

        PlayActivationSound(attackPosition);

        SpawnAreaAttackEffect(attackPosition);

        PerformAreaAttack(attackPosition);

        G.screenShake?.Shake(screenShakeForce);

        Debug.Log($"Area attack with radius {attackRadius}!");
        InvokeOnAbilityUsed();
    }

    private void SpawnAreaAttackEffect(Vector3 position)
    {
        GameObject effect = InstantiateVisualEffect(position, Quaternion.identity);

        if (effect == null)
            return;

        effect.transform.localScale = Vector3.one * attackRadius * effectScaleMultiplier;
    }

    private void PerformAreaAttack(Vector2 center)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(center, attackRadius, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            IDamageable damageable = enemyCollider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"Area attack hit {enemyCollider.name} for {attackDamage} damage!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, attackRadius);
    }
}