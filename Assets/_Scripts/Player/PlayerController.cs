using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private PlayerDetailsSO playerDetails;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Player player;

    private Vector2 moveDirection;
    private Vector2 lastNonZeroDirection = Vector2.down;

    private bool isSprinting = false;
    private bool isDashing = false;
    private bool canDash = true;

    private float dashTimer;
    private float dashCooldownTimer;

    public event System.Action<Vector2> OnMove;
    public Vector2 LastMoveDirection => lastNonZeroDirection;

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

        if (animator == null)
            animator = GetComponent<Animator>();

        Debug.Log($"PlayerController initialized. Rigidbody: {rb != null}, Player: {player != null}, Animator: {animator != null}");
    }

    private void Update()
    {
        ProcessInputs();
        HandleTimers();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        Move();
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

        bool sprintInput = Keyboard.current.leftShiftKey.isPressed;
        bool dashInput = Keyboard.current.spaceKey.wasPressedThisFrame;

        if (!dashInput)
        {
            if (sprintInput && !isSprinting && moveDirection != Vector2.zero)
                StartSprint();
            else if ((!sprintInput || moveDirection == Vector2.zero) && isSprinting)
                StopSprint();
        }

        if (
            dashInput &&
            canDash &&
            !isDashing &&
            moveDirection != Vector2.zero &&
            player.CurrentStamina >= playerDetails.DashStaminaCost
        )
        {
            TryDash();

            if (isSprinting)
                StopSprint();
        }
    }

    private void Move()
    {
        float currentSpeed = playerDetails.MoveSpeed *
                             (isSprinting ? playerDetails.SprintMultiplier : 1f);

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

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        bool isMoving = moveDirection.sqrMagnitude > 0.01f || isDashing;
        animator.SetBool("IsMoving", isMoving);

        // Если хочешь, чтобы при спринте анимация игралась быстрее:
        animator.speed = (isMoving && isSprinting) ? playerDetails.SprintMultiplier : 1f;
    }

    private void HandleTimers()
    {
        if (isSprinting)
        {
            if (!player.TryUseStamina(playerDetails.SprintStaminaCostPerSecond * Time.deltaTime))
                StopSprint();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
                EndDash();
        }

        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownTimer <= 0f)
                canDash = true;
        }
    }

    private void StartSprint()
    {
        if (player.CurrentStamina > 0)
            isSprinting = true;
    }

    private void StopSprint()
    {
        isSprinting = false;
    }

    private void TryDash()
    {
        if (canDash && player.CurrentStamina >= playerDetails.DashStaminaCost && !isDashing)
        {
            if (player.TryUseStamina(playerDetails.DashStaminaCost))
                StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;

        dashTimer = playerDetails.DashDuration;
        dashCooldownTimer = playerDetails.DashCooldown;

        float dashSpeed = playerDetails.DashDistance / playerDetails.DashDuration;
        rb.linearVelocity = lastNonZeroDirection * dashSpeed;
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
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