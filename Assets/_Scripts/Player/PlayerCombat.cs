using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] private BaseAbility meleeAbility;
    [SerializeField] private BaseAbility rangedAbility;
    [SerializeField] private BaseAbility hookAbility;
    [SerializeField] private BaseAbility areaAttackAbility;

    [Header("Input Settings")]
    [SerializeField] private Key switchAbilityKey = Key.E;

    private Player player;
    private PlayerController playerController;
    private BaseAbility currentPrimaryAbility;
    private bool isUsingEnhancedAttack = false;

    public BaseAbility CurrentPrimaryAbility => currentPrimaryAbility;
    public bool IsUsingEnhancedAttack => isUsingEnhancedAttack;

    // Public methods for setting abilities (used by PlayerExampleSetup)
    public void SetMeleeAbility(BaseAbility ability) { meleeAbility = ability; }
    public void SetRangedAbility(BaseAbility ability) { rangedAbility = ability; }
    public void SetHookAbility(BaseAbility ability) { hookAbility = ability; }
    public void SetAreaAttackAbility(BaseAbility ability) { areaAttackAbility = ability; }

    private void Awake()
    {
        Debug.Log("PlayerCombat.Awake() called");
        // Не получаем компоненты здесь, чтобы избежать проблем с порядком Awake
    }

    private void Start()
    {
        player = GetComponent<Player>();
        playerController = GetComponent<PlayerController>();

        Debug.Log($"PlayerCombat.Start(): player = {player != null}, playerController = {playerController != null}");
        Debug.Log($"PlayerCombat.Start(): Mouse.current = {Mouse.current != null}, Keyboard.current = {Keyboard.current != null}");

        if (player != null)
        {
            player.OnEnhancedAttackAvailable += OnEnhancedAttackAvailableChanged;
        }

        InitializeAbilities();
        SetDefaultPrimaryAbility();

        Debug.Log($"PlayerCombat initialized. Melee ability: {meleeAbility != null}, Ranged: {rangedAbility != null}, Hook: {hookAbility != null}");
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnEnhancedAttackAvailable -= OnEnhancedAttackAvailableChanged;
        }
    }

    private void Update()
    {
        if (G.IsPaused) return;

        if (Mouse.current == null)
        {
            Debug.LogWarning("PlayerCombat: Mouse.current is null in Update!");
        }

        HandleAbilityInput();
        UpdateAbilities();
    }

    private void InitializeAbilities()
    {
        if (meleeAbility != null) meleeAbility.Initialize();
        if (rangedAbility != null) rangedAbility.Initialize();
        if (hookAbility != null) hookAbility.Initialize();
        if (areaAttackAbility != null) areaAttackAbility.Initialize();
    }

    private void SetDefaultPrimaryAbility()
    {
        if (currentPrimaryAbility == null)
        {
            if (meleeAbility != null) currentPrimaryAbility = meleeAbility;
            else if (rangedAbility != null) currentPrimaryAbility = rangedAbility;
            else if (hookAbility != null) currentPrimaryAbility = hookAbility;
            else if (areaAttackAbility != null) currentPrimaryAbility = areaAttackAbility;

            Debug.Log($"Current primary ability set to: {currentPrimaryAbility?.name ?? "null"}");
        }
    }

    private void HandleAbilityInput()
    {
        // Debug mouse state
        if (Mouse.current == null)
        {
            Debug.LogWarning("PlayerCombat: Mouse.current is null!");
            return;
        }

        Debug.Log($"PlayerCombat: Mouse.current available. Left button: {Mouse.current.leftButton.isPressed}, Right button: {Mouse.current.rightButton.isPressed}");

        // Primary attack (Left Mouse Button)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("LEFT MOUSE BUTTON PRESSED - Trying primary ability");
            TryUsePrimaryAbility();
        }

        // Secondary attack (Right Mouse Button)
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("RIGHT MOUSE BUTTON PRESSED - Trying secondary ability");
            TryUseSecondaryAbility();
        }

        // Tertiary attack (Space)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("SPACE PRESSED - Trying tertiary ability");
            TryUseTertiaryAbility();
        }

        // Switch ability (E key)
        if (Keyboard.current != null && Keyboard.current[switchAbilityKey].wasPressedThisFrame)
        {
            Debug.Log("SWITCH ABILITY KEY PRESSED - Switching primary ability");
            SwitchPrimaryAbility();
        }
    }

    private void UpdateAbilities()
    {
        meleeAbility?.UpdateCooldown(Time.deltaTime);
        rangedAbility?.UpdateCooldown(Time.deltaTime);
        hookAbility?.UpdateCooldown(Time.deltaTime);
        areaAttackAbility?.UpdateCooldown(Time.deltaTime);
    }

    private void TryUsePrimaryAbility()
    {
        if (currentPrimaryAbility == null)
        {
            Debug.LogWarning("PlayerCombat: currentPrimaryAbility is null!");
            return;
        }

        Debug.Log($"PlayerCombat: Trying to use primary ability. Type: {currentPrimaryAbility.GetType().Name}, CanUse: {currentPrimaryAbility.CanUse}");

        // Check if enhanced attack is available and we're using melee
        if (isUsingEnhancedAttack && currentPrimaryAbility is MeleeAbility melee)
        {
            melee.SetEnhanced(true);
            currentPrimaryAbility.TryUseAbility(player);
            melee.SetEnhanced(false);
            isUsingEnhancedAttack = false;
        }
        else
        {
            currentPrimaryAbility.TryUseAbility(player);
        }
    }

    private void TryUseSecondaryAbility()
    {
        hookAbility?.TryUseAbility(player);
    }

    private void TryUseTertiaryAbility()
    {
        areaAttackAbility?.TryUseAbility(player);
    }

    private void SwitchPrimaryAbility()
    {
        // Only switch between melee and ranged if both are unlocked
        if (meleeAbility == null || rangedAbility == null)
        {
            Debug.LogWarning("Cannot switch: melee or ranged ability not assigned!");
            return;
        }

        if (!meleeAbility.IsUnlocked || !rangedAbility.IsUnlocked)
        {
            Debug.LogWarning("Cannot switch: one or both abilities not unlocked!");
            return;
        }

        // Toggle between melee and ranged
        if (currentPrimaryAbility == meleeAbility)
        {
            currentPrimaryAbility = rangedAbility;
            Debug.Log("Switched to ranged ability");
        }
        else if (currentPrimaryAbility == rangedAbility)
        {
            currentPrimaryAbility = meleeAbility;
            Debug.Log("Switched to melee ability");
        }
        else
        {
            // Default to melee if current is something else
            currentPrimaryAbility = meleeAbility;
            Debug.Log("Switched to melee ability (default)");
        }

        Debug.Log($"Current primary ability is now: {currentPrimaryAbility.name}");
    }

    public void UnlockRangedAbility()
    {
        if (rangedAbility != null)
        {
            rangedAbility.Unlock();
            Debug.Log("Ranged ability unlocked!");
        }
    }

    public void UnlockHookAbility()
    {
        if (hookAbility != null)
        {
            hookAbility.Unlock();
            Debug.Log("Hook ability unlocked!");
        }
    }

    public void UnlockAreaAttackAbility()
    {
        if (areaAttackAbility != null)
        {
            areaAttackAbility.Unlock();
            Debug.Log("Area attack ability unlocked!");
        }
    }

    private void OnEnhancedAttackAvailableChanged(bool available)
    {
        isUsingEnhancedAttack = available;
        Debug.Log($"Enhanced attack available: {available}");
    }

    // Visualization for ability cooldowns
    private void OnDrawGizmosSelected()
    {
        if (meleeAbility != null && !meleeAbility.CanUse)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.3f);
        }

        if (rangedAbility != null && !rangedAbility.CanUse)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + Vector3.right * 0.3f, 0.2f);
        }
    }
}