using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float velocityPower = 0.9f;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference movementAction;
    [SerializeField] private Key moveUpKey = Key.W;
    [SerializeField] private Key moveDownKey = Key.S;
    [SerializeField] private Key moveLeftKey = Key.A;
    [SerializeField] private Key moveRightKey = Key.D;

    private Rigidbody2D rb;
    private Player player;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection;
    private bool isMovementEnabled = true;

    public Vector2 LastMoveDirection => lastMoveDirection;
    public bool IsMoving => moveInput.magnitude > 0.1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    private void OnEnable()
    {
        if (movementAction != null && movementAction.action != null)
        {
            movementAction.action.performed += OnMovementPerformed;
            movementAction.action.canceled += OnMovementCanceled;
            movementAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (movementAction != null && movementAction.action != null)
        {
            movementAction.action.performed -= OnMovementPerformed;
            movementAction.action.canceled -= OnMovementCanceled;
            movementAction.action.Disable();
        }
    }

    private void Update()
    {
        // Fallback to legacy input system if InputAction not set
        if (movementAction == null || movementAction.action == null)
        {
            HandleLegacyInput();
        }
    }

    private void FixedUpdate()
    {
        if (!isMovementEnabled || G.IsPaused) return;

        HandleMovement();
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        UpdateFacingDirection();
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void HandleLegacyInput()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current[moveUpKey].isPressed) input.y += 1;
        if (Keyboard.current[moveDownKey].isPressed) input.y -= 1;
        if (Keyboard.current[moveLeftKey].isPressed) input.x -= 1;
        if (Keyboard.current[moveRightKey].isPressed) input.x += 1;

        moveInput = input.normalized;
        UpdateFacingDirection();
    }

    private void HandleMovement()
    {
        if (moveInput.magnitude > 0.1f)
        {
            // Calculate target velocity
            Vector2 targetVelocity = moveInput * moveSpeed;

            // Calculate velocity difference
            Vector2 velocityDiff = targetVelocity - rb.linearVelocity;

            // Apply acceleration/deceleration based on direction
            float accelerationFactor = (Vector2.Dot(velocityDiff, moveInput) > 0) ? acceleration : deceleration;

            // Apply velocity with power curve for smoother movement
            rb.linearVelocity += velocityDiff * accelerationFactor * Time.fixedDeltaTime;

            // Store last move direction
            lastMoveDirection = moveInput;
        }
        else
        {
            // Apply deceleration when no input
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }
    }

    private void UpdateFacingDirection()
    {
        if (player == null) return;

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            // Horizontal movement dominates
            player.SetFacingRight(moveInput.x > 0);
        }
        else if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            // Vertical movement - keep last horizontal facing
            // Could add up/down facing logic here if needed
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        isMovementEnabled = enabled;

        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Visualization for editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if (IsMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)moveInput);
        }
    }
}