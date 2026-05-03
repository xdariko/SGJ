using UnityEngine;

[CreateAssetMenu(fileName = "HookAbility", menuName = "Player/Abilities/Hook Ability")]
public class HookAbility : BaseAbility
{
    [Header("Hook Settings")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private float hookSpeed = 15f;
    [SerializeField] private float maxHookDistance = 10f;
    [SerializeField] private float hookDuration = 3f;
    [SerializeField] private float enhancedAttackWindow = 2f;

    [Header("Camera Shake")]
    [SerializeField] private float screenShakeForce = 0.2f;

    private HookProjectile activeHook;
    private bool isHookActive = false;
    private float enhancedWindowEndTime = 0f;
    private Player currentOwnerPlayer;

    public bool IsEnhancedAttackAvailable => enhancedWindowEndTime > 0f && Time.time < enhancedWindowEndTime;

    protected override void UseAbility(Player player)
    {
        if (player == null || hookPrefab == null) return;

        currentOwnerPlayer = player;

        if (isHookActive)
        {
            // Cancel active hook
            CancelHook();
            return;
        }

        PlayActivationSound(player.transform.position);

        // Get hook direction from mouse position
        PlayerController playerController = player.GetComponent<PlayerController>();
        Vector2 hookDirection = playerController != null
            ? playerController.GetMouseDirection()
            : (player.IsFacingRight ? Vector2.right : Vector2.left);

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
        G.screenShake?.Shake(screenShakeForce);
        Debug.Log("Fired hook!");
    }

    public void OnHookHitEnemy(GameObject enemy, HookProjectile hook)
    {
        if (hook != activeHook) return;

        Player player = activeHook.Owner.GetComponent<Player>();
        if (player == null) return;

        // Сохраняем позицию игрока ДО телепорта
        float playerXBeforeTeleport = player.transform.position.x;
        float hookX = hook.transform.position.x;

        // Телепортируем игрока к точке попадания хука
        player.transform.position = hook.transform.position;

        // Разворачиваем только на основе старой позиции игрока
        if (Mathf.Abs(hookX - playerXBeforeTeleport) > 0.01f)
        {
            player.SetFacingRight(hookX > playerXBeforeTeleport);
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

        // Если окно усиленной атаки закончилось — выключаем его
        if (enhancedWindowEndTime > 0f && Time.time >= enhancedWindowEndTime)
        {
            enhancedWindowEndTime = 0f;

            if (currentOwnerPlayer != null)
            {
                currentOwnerPlayer.InvokeOnEnhancedAttackAvailable(false);
            }
        }
    }
}