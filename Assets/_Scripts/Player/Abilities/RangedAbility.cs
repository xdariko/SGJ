using UnityEngine;

[CreateAssetMenu(fileName = "RangedAbility", menuName = "Player/Abilities/Ranged Ability")]
public class RangedAbility : BaseAbility
{
    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private int maxPiercingCount = 0; // 0 = no piercing

    protected override void UseAbility(Player player)
    {
        if (player == null || projectilePrefab == null) return;

        PlayActivationSound(player.transform.position);

        // Calculate shoot direction based on player's facing direction
        Vector2 shootDirection = player.IsFacingRight ? Vector2.right : Vector2.left;

        // Instantiate projectile
        GameObject projectile = Instantiate(projectilePrefab, player.transform.position, Quaternion.identity);
        RangedProjectile projectileComponent = projectile.GetComponent<RangedProjectile>();

        if (projectileComponent != null)
        {
            projectileComponent.Initialize(
                shootDirection,
                projectileSpeed,
                projectileDamage,
                projectileLifetime,
                maxPiercingCount,
                player.gameObject
            );
        }
        else
        {
            // Fallback: simple movement
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = shootDirection * projectileSpeed;
            }
            Destroy(projectile, projectileLifetime);
        }

        InstantiateVisualEffect(player.transform.position, Quaternion.LookRotation(shootDirection));
        Debug.Log("Fired ranged projectile!");
        InvokeOnAbilityUsed();
    }
}