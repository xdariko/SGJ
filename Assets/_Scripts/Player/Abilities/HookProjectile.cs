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
        rb.freezeRotation = true;
        rb.isKinematic = true;

        Destroy(gameObject, duration);
    }

    private void Update()
    {
        // Check if reached max distance
        if (!isReturning && Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            StartReturning();
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
        rb.linearVelocity = -direction * (speed * 1.5f); // Return faster

        // Change visual to indicate returning
        // hookRenderer.color = returnColor;

        Destroy(gameObject, 2f); // Destroy after short time when returning
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