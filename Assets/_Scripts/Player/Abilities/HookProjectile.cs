using UnityEngine;

public class HookProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private float duration;
    private HookAbility ability;
    private GameObject owner;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private bool isReturning = false;
    private GameObject hookedEnemy;

    public GameObject Owner => owner;

    public void Initialize(Vector2 direction, float speed, float maxDistance, float duration, HookAbility ability, GameObject owner)
    {
        this.direction = direction;
        this.speed = speed;
        this.maxDistance = maxDistance;
        this.duration = duration;
        this.ability = ability;
        this.owner = owner;
        this.startPosition = transform.position;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.linearVelocity = direction * speed;
        rb.gravityScale = 0f;
        rb.freezeRotation = false; // Allow rotation

        // Rotate sprite to face direction of movement (front faces direction)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle; // Use Rigidbody2D rotation for proper physics

        Destroy(gameObject, duration);
    }

    private void Update()
    {
        // Check if reached max distance
        if (!isReturning && Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            StartReturning();
        }

        // If returning, continuously update direction towards owner
        if (isReturning && owner != null)
        {
            Vector2 returnDirection = ((Vector2)owner.transform.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = returnDirection * (speed * 1.5f);

            // Update rotation: hook should return with BACK facing player
            // So we add 180 degrees to the direction angle
            float angle = Mathf.Atan2(returnDirection.y, returnDirection.x) * Mathf.Rad2Deg + 180f;
            rb.rotation = angle;

            // Check if reached owner
            float distanceToOwner = Vector3.Distance(transform.position, owner.transform.position);
            if (distanceToOwner <= 0.5f) // Close enough to owner
            {
                DestroyProjectile();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore owner and triggers
        if (other.gameObject == owner || other.isTrigger) return;

        // Check if hit enemy
        if (other.TryGetComponent<Enemy>(out var enemy) && !isReturning)
        {
            hookedEnemy = enemy.gameObject;
            ability.OnHookHitEnemy(hookedEnemy, this);
            StartReturning();
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("PlayerProjectile"))
        {
            // Hit non-enemy object, start returning
            StartReturning();
        }
    }

    private void StartReturning()
    {
        if (isReturning) return;

        isReturning = true;
        Debug.Log("Hook returning to player");
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    public void Cancel()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (ability != null)
        {
            ability.OnHookDestroyed();
        }
    }
}