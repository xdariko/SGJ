using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private PlayerDetailsSO playerDetails;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 lastNonZeroDirection = Vector2.down;

    private bool isDashing = false;
    private bool canDash = true;
    private float dashCooldownTimer;

    // Dash movement
    private Vector2 dashStartPos;
    private Vector2 dashTargetPos;
    private float dashElapsed;

    public event System.Action<Vector2> OnMove;
    public Vector2 LastMoveDirection => lastNonZeroDirection;

    private Player player;

    private void Awake()
    {
        Debug.Log("PlayerController.Awake() called");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PlayerController: Duplicate instance detected, destroying this one");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

        // Prevent tunneling during dash
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Debug.Log($"PlayerController initialized. Rigidbody: {rb != null}, Player: {player != null}");
    }

    private void Update()
    {
        ProcessInputs();
        HandleTimers();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            PerformDash();
        }
        else
        {
            Move();
        }
    }

    private void ProcessInputs()
    {
        if (Keyboard.current == null)
        {
            Debug.LogWarning("PlayerController: Keyboard.current is null!");
            return;
        }

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveX -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveX += 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveY -= 1f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveY += 1f;

        moveDirection = new Vector2(moveX, moveY).normalized;

        if (moveDirection != Vector2.zero)
            lastNonZeroDirection = moveDirection;

        bool dashInput = Keyboard.current.leftShiftKey.wasPressedThisFrame;

        if (dashInput && canDash && !isDashing && moveDirection != Vector2.zero && player.CurrentStamina >= playerDetails.DashStaminaCost)
        {
            TryDash();
        }
    }

    private void Move()
    {
        float currentSpeed = playerDetails.MoveSpeed;

        if (moveDirection.magnitude > 0.1f)
        {
            rb.linearVelocity = moveDirection * currentSpeed;
            lastNonZeroDirection = moveDirection;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        OnMove?.Invoke(moveDirection != Vector2.zero ? moveDirection : lastNonZeroDirection);
    }

    private void HandleTimers()
    {
        if (isDashing)
        {
            dashElapsed += Time.deltaTime;

            if (dashElapsed >= playerDetails.DashDuration)
            {
                EndDash();
            }
        }

        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownTimer <= 0f)
                canDash = true;
        }
    }

    private void TryDash()
    {
        if (!canDash || isDashing || player.CurrentStamina < playerDetails.DashStaminaCost)
            return;

        Vector2 direction = lastNonZeroDirection;
        float maxDashDistance = playerDetails.DashDistance;

        // Raycast to detect walls (tag "Wall") only, ignore enemies
        Vector2 rayStart = (Vector2)transform.position + direction * 0.1f;
        float availableDistance = maxDashDistance;

        RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, direction, maxDashDistance);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Wall"))
            {
                availableDistance = hit.distance - 0.1f;
                break;
            }
        }

        // Minimum distance required to dash; prevents zero-distance dashes
        if (availableDistance <= 0.05f)
            return;

        if (player.TryUseStamina(playerDetails.DashStaminaCost))
        {
            StartDash(direction, availableDistance);
        }
    }

    private void StartDash(Vector2 direction, float dashDistance)
    {
        isDashing = true;
        canDash = false;
        dashCooldownTimer = playerDetails.DashCooldown;

        // Ignore collisions with enemies during dash
        IgnoreEnemyCollisions(true);

        dashStartPos = rb.position;
        dashTargetPos = dashStartPos + direction * dashDistance;
        dashElapsed = 0f;
    }

    private void PerformDash()
    {
        dashElapsed += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(dashElapsed / playerDetails.DashDuration);
        Vector2 newPos = Vector2.Lerp(dashStartPos, dashTargetPos, t);
        rb.MovePosition(newPos);

        // End dash if reached target or time expired
        if (Vector2.Distance(rb.position, dashTargetPos) < 0.01f || dashElapsed >= playerDetails.DashDuration)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
        // Restore collisions with enemies
        IgnoreEnemyCollisions(false);
    }

    private void IgnoreEnemyCollisions(bool ignore)
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return;

        // Find all enemies by tag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D[] cols = enemy.GetComponents<Collider2D>();
            for (int i = 0; i < cols.Length; i++)
            {
                Collider2D col = cols[i];
                if (col != null && col.enabled && !col.isTrigger)
                {
                    Physics2D.IgnoreCollision(playerCollider, col, ignore);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing)
        {
            // Only abort dash on collision with walls (tag "Wall")
            if (collision.gameObject.CompareTag("Wall"))
            {
                EndDash();
            }
            // Collisions with enemies do not abort dash (ignored via IgnoreEnemyCollisions)
        }
    }

    public Vector2 GetMouseDirection()
    {
        if (Mouse.current == null)
        {
            Debug.LogWarning("PlayerController: Mouse.current is null!");
            return Vector2.right;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = ((Vector2)worldMousePos - (Vector2)transform.position).normalized;
        
        Debug.Log($"Mouse direction: {direction}");
        return direction;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            OnMove = null;
        }
    }
}
