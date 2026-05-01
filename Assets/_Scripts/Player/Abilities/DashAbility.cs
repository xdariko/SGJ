using UnityEngine;

[CreateAssetMenu(fileName = "DashAbility", menuName = "Player/Abilities/Dash Ability")]
public class DashAbility : BaseAbility
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private GameObject dashTrailPrefab;
    [SerializeField] private float trailSpawnInterval = 0.05f;

    protected override void UseAbility(Player player)
    {
        if (player == null) return;

        PlayActivationSound(player.transform.position);

        // Get current movement direction
        PlayerController playerController = player.GetComponent<PlayerController>();
        Vector2 dashDirection = Vector2.right; // Default right

        if (playerController != null)
        {
            dashDirection = playerController.LastMoveDirection;
            if (dashDirection == Vector2.zero)
            {
                // Use facing direction if not moving
                dashDirection = player.IsFacingRight ? Vector2.right : Vector2.left;
            }
        }
        else
        {
            // Use facing direction
            dashDirection = player.IsFacingRight ? Vector2.right : Vector2.left;
        }

        // Start dash
        StartDash(player, dashDirection.normalized);

        Debug.Log($"Dashing in direction: {dashDirection}");
        InvokeOnAbilityUsed();
    }

    private void StartDash(Player player, Vector2 direction)
    {
        // Make player invincible
        player.SetInvincible(true);

        // Calculate target position
        Vector3 startPosition = player.transform.position;
        Vector3 targetPosition = startPosition + (Vector3)direction * dashDistance;

        // Start dash coroutine (would need MonoBehaviour to actually run this)
        // In a real implementation, this would be handled by a DashController component
        Debug.Log($"Dash from {startPosition} to {targetPosition} over {dashDuration} seconds");

        // Spawn trail effects
        if (dashTrailPrefab != null)
        {
            // Would spawn trail objects along the dash path
        }

        // After dash duration, make player vulnerable again
        // This would be handled by a coroutine in a real implementation
        player.SetInvincible(false);
    }

    public override bool TryUseAbility(Player player)
    {
        // Check if player is already dashing
        // In real implementation, would check player.IsDashing
        return base.TryUseAbility(player);
    }
}