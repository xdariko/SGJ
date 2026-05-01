using UnityEngine;

[CreateAssetMenu(fileName = "HookAbility", menuName = "Player/Abilities/Hook Ability")]
public class HookAbility : BaseAbility
{
    [Header("Hook Settings")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private float hookSpeed = 15f;
    [SerializeField] private float maxHookDistance = 10f;
    [SerializeField] private float hookDuration = 3f;
    [SerializeField] private float pullSpeed = 20f;
    [SerializeField] private float enhancedAttackWindow = 2f;

    private HookProjectile activeHook;
    private bool isHookActive = false;
    private float enhancedWindowEndTime = 0f;

    public bool IsEnhancedAttackAvailable => Time.time < enhancedWindowEndTime;

    protected override void UseAbility(Player player)
    {
        if (player == null || hookPrefab == null) return;

        if (isHookActive)
        {
            // Cancel active hook
            CancelHook();
            return;
        }

        PlayActivationSound(player.transform.position);

        // Calculate hook direction based on player's facing direction
        Vector2 hookDirection = player.IsFacingRight ? Vector2.right : Vector2.left;

        // Instantiate hook
        GameObject hookObject = Instantiate(hookPrefab, player.transform.position, Quaternion.identity);
        activeHook = hookObject.GetComponent<HookProjectile>();

        if (activeHook != null)
        {
            activeHook.Initialize(
                hookDirection,
                hookSpeed,
                maxHookDistance,
                hookDuration,
                this,
                player.gameObject
            );

            isHookActive = true;
            InstantiateVisualEffect(player.transform.position, Quaternion.LookRotation(hookDirection));
        }

        InvokeOnAbilityUsed();
        Debug.Log("Fired hook!");
    }

    public void OnHookHitEnemy(GameObject enemy, HookProjectile hook)
    {
        if (hook != activeHook) return;

        // Start pulling player towards enemy
        // Note: In a real implementation, this would need to be started on a MonoBehaviour
        // For now, we'll use a simplified approach
        PullPlayerToTargetImmediate(hook.transform.position, enemy);
    }

    private void PullPlayerToTargetImmediate(Vector3 targetPosition, GameObject enemy)
    {
        Player player = activeHook.Owner.GetComponent<Player>();
        if (player == null) return;

        // Disable player movement during pull
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetMovementEnabled(false);
        }

        // Move player directly to target (simplified for ScriptableObject)
        player.transform.position = targetPosition;

        // Face player towards enemy
        if (targetPosition.x > player.transform.position.x)
        {
            player.SetFacingRight(true);
        }
        else
        {
            player.SetFacingRight(false);
        }

        // Re-enable movement
        if (movement != null)
        {
            movement.SetMovementEnabled(true);
        }

        // Activate enhanced attack window
        enhancedWindowEndTime = Time.time + enhancedAttackWindow;

        // Notify player about enhanced attack availability
        player.InvokeOnEnhancedAttackAvailable(true);

        Debug.Log("Player pulled to target! Enhanced attack available for " + enhancedAttackWindow + " seconds");
    }

    private System.Collections.IEnumerator PullPlayerToTarget(Vector3 targetPosition, GameObject enemy)
    {
        Player player = activeHook.Owner.GetComponent<Player>();
        if (player == null) yield break;

        float startTime = Time.time;
        Vector3 startPosition = player.transform.position;

        // Disable player movement during pull
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetMovementEnabled(false);
        }

        while (Time.time - startTime < hookDuration && Vector3.Distance(player.transform.position, targetPosition) > 0.1f)
        {
            // Move player towards target
            Vector3 direction = (targetPosition - player.transform.position).normalized;
            player.transform.position += direction * pullSpeed * Time.deltaTime;

            // Face player towards enemy
            if (targetPosition.x > player.transform.position.x)
            {
                player.SetFacingRight(true);
            }
            else
            {
                player.SetFacingRight(false);
            }

            yield return null;
        }

        // Re-enable movement
        if (movement != null)
        {
            movement.SetMovementEnabled(true);
        }

        // Activate enhanced attack window
        enhancedWindowEndTime = Time.time + enhancedAttackWindow;

        // Notify player about enhanced attack availability
        player.InvokeOnEnhancedAttackAvailable(true);

        Debug.Log("Player pulled to target! Enhanced attack available for " + enhancedAttackWindow + " seconds");
    }

    public void CancelHook()
    {
        if (activeHook != null)
        {
            activeHook.Cancel();
            activeHook = null;
            isHookActive = false;
        }
    }

    public void OnHookDestroyed()
    {
        activeHook = null;
        isHookActive = false;
    }

    public override void UpdateCooldown(float deltaTime)
    {
        base.UpdateCooldown(deltaTime);

        // Check if enhanced window has ended
        if (IsEnhancedAttackAvailable && Time.time >= enhancedWindowEndTime)
        {
            Player player = activeHook != null ? activeHook.Owner.GetComponent<Player>() : null;
            if (player != null)
            {
            player.InvokeOnEnhancedAttackAvailable(false);
            }
        }
    }
}