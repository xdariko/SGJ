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
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.lifetime = lifetime;
        this.piercingCount = Mathf.Max(1, piercingCount);
        this.owner = owner;

        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = this.direction * speed;

        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (other == null)
            return;

        if (other.gameObject == owner || other.transform.IsChildOf(owner.transform))
            return;

        if (other.CompareTag("Player") || other.CompareTag("PlayerProjectile"))
            return;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            float finalDamage = damage;

            // Check for Boss vulnerability (Tank: vulnerable to ranged)
            BossEnemy boss = other.GetComponent<BossEnemy>();
            if (boss != null && boss.VulnerableToRanged)
            {
                finalDamage *= 2f; // Bonus damage when vulnerable
                Debug.Log($"Boss is vulnerable to ranged! Bonus damage applied. Final: {finalDamage}");
            }

            damageable.TakeDamage(finalDamage);

            piercingCount--;

            Debug.Log($"Projectile hit {other.name} for {finalDamage} damage! Piercing left: {piercingCount}");

            if (piercingCount <= 0)
                DestroyProjectile();

            return;
        }

        if (other.isTrigger)
            return;

        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}