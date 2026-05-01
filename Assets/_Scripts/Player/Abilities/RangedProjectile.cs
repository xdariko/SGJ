using UnityEngine;

public class RangedProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private float lifetime;
    private int piercingCount;
    private GameObject owner;
    private Rigidbody2D rb;

    public void Initialize(Vector2 direction, float speed, float damage, float lifetime, int piercingCount, GameObject owner)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        this.lifetime = lifetime;
        this.piercingCount = piercingCount;
        this.owner = owner;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.linearVelocity = direction * speed;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore owner and triggers
        if (other.gameObject == owner || other.isTrigger) return;

        // Check if hit enemy
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            // Deal damage to enemy
            // enemy.TakeDamage(damage); // Would need to implement in Enemy class

            // Reduce piercing count
            piercingCount--;

            if (piercingCount <= 0)
            {
                DestroyProjectile();
            }

            Debug.Log($"Projectile hit {enemy.name} for {damage} damage! Piercing left: {piercingCount}");
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("PlayerProjectile"))
        {
            // Hit non-enemy object, destroy projectile
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        // Add impact effect if available
        // Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
}