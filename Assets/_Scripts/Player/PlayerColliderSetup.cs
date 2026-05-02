using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerColliderSetup : MonoBehaviour
{
    [Header("Lower Body Collider")]
    [SerializeField] private float lowerColliderWidth = 0.3f;
    [SerializeField] private float lowerColliderHeight = 0.2f;
    [SerializeField] private Vector2 lowerColliderOffset = new Vector2(0f, -0.4f);
    [SerializeField] private bool lowerColliderIsTrigger = false;

    [Header("Optional: Platform Detection")]
    [SerializeField] private bool usePlatformCheck = true;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private BoxCollider2D lowerCollider;

    public Rigidbody2D RB => rb;
    public BoxCollider2D LowerCollider => lowerCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Создаем или получаем коллайдер нижней части
        lowerCollider = GetComponent<BoxCollider2D>();
        if (lowerCollider == null)
        {
            lowerCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        SetupLowerCollider();
    }

    private void SetupLowerCollider()
    {
        lowerCollider.size = new Vector2(lowerColliderWidth, lowerColliderHeight);
        lowerCollider.offset = lowerColliderOffset;
        lowerCollider.isTrigger = lowerColliderIsTrigger;
    }

    /// <summary>
    /// Проверка, находится ли игрок на земле (для платформ)
    /// </summary>
    public bool IsGrounded()
    {
        if (!usePlatformCheck || groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (usePlatformCheck && groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
