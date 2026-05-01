using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private PlayerDetailsSO playerDetails;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 lastNonZeroDirection = Vector2.down;

    private bool isSprinting = false;
    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimer;
    private float dashCooldownTimer;

    public event System.Action<Vector2> OnMove;

    private Player player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        ProcessInputs();
        HandleTimers();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        Move();
    }

    private void ProcessInputs()
    {
        if (Keyboard.current == null)
            return;

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

        rb.linearVelocity = moveDirection * currentSpeed;

        OnMove?.Invoke(moveDirection != Vector2.zero ? moveDirection : lastNonZeroDirection);
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            OnMove = null;
        }
    }
}