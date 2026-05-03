using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] private BaseAbility meleeAbility;
    [SerializeField] private BaseAbility rangedAbility;
    [SerializeField] private BaseAbility hookAbility;
    [SerializeField] private BaseAbility areaAttackAbility;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string meleeAttackTriggerName = "Attack";
    [SerializeField] private string rangedAttackTriggerName = "Shoot";
    [SerializeField] private string hookTriggerName = "Hook";
    [SerializeField] private string areaAttackTriggerName = "AreaAttack";

    [Header("Animator Controllers")]
    [SerializeField] private RuntimeAnimatorController[] mutationAnimators;

    [Header("Input Settings")]
    [SerializeField] private Key switchAbilityKey = Key.E;

    private Player player;
    private PlayerController playerController;
    private BaseAbility currentPrimaryAbility;
    private bool isUsingEnhancedAttack = false;
    private int currentMutationStage = 0;

    public BaseAbility CurrentPrimaryAbility => currentPrimaryAbility;
    public bool IsUsingEnhancedAttack => isUsingEnhancedAttack;
    public int CurrentMutationStage => currentMutationStage;

    public void SetMeleeAbility(BaseAbility ability) { meleeAbility = ability; }
    public void SetRangedAbility(BaseAbility ability) { rangedAbility = ability; }
    public void SetHookAbility(BaseAbility ability) { hookAbility = ability; }
    public void SetAreaAttackAbility(BaseAbility ability) { areaAttackAbility = ability; }

    private void Awake()
    {
        Debug.Log("PlayerCombat.Awake() called");
    }

    private void Start()
    {
        player = GetComponent<Player>();
        playerController = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        Debug.Log($"PlayerCombat.Start(): player = {player != null}, playerController = {playerController != null}, animator = {animator != null}");
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

        // Secondary attack (Right Mouse Button) — Hook
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("RIGHT MOUSE BUTTON PRESSED - Trying secondary ability");
            TryUseSecondaryAbility();
        }

        // Tertiary attack (Space) — Area Attack
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

        if (!currentPrimaryAbility.IsUnlocked)
        {
            Debug.LogWarning("PlayerCombat: currentPrimaryAbility is locked!");
            return;
        }

        if (!currentPrimaryAbility.CanUse)
        {
            Debug.LogWarning("PlayerCombat: currentPrimaryAbility is on cooldown or unavailable!");
            return;
        }

        Debug.Log($"PlayerCombat: Trying to use primary ability. Type: {currentPrimaryAbility.GetType().Name}, CanUse: {currentPrimaryAbility.CanUse}");

        // Запускаем соответствующую анимацию атаки
        PlayAttackAnimation();

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

    private void PlayAttackAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("PlayerCombat: Animator is not assigned!");
            return;
        }

        if (currentPrimaryAbility is MeleeAbility)
        {
            animator.ResetTrigger(meleeAttackTriggerName);
            animator.SetTrigger(meleeAttackTriggerName);
            Debug.Log("Playing MELEE attack animation");
        }
        else if (currentPrimaryAbility is RangedAbility)
        {
            animator.ResetTrigger(rangedAttackTriggerName);
            animator.SetTrigger(rangedAttackTriggerName);
            Debug.Log("Playing RANGED attack animation");
        }
    }

    private void TryUseSecondaryAbility()
    {
        if (hookAbility == null)
        {
            Debug.LogWarning("PlayerCombat: hookAbility is null!");
            return;
        }

        if (!hookAbility.IsUnlocked || !hookAbility.CanUse)
        {
            Debug.LogWarning("PlayerCombat: hookAbility is locked or on cooldown!");
            return;
        }

        // Запускаем анимацию хука
        PlayHookAnimation();

        hookAbility.TryUseAbility(player);
    }

    private void PlayHookAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("PlayerCombat: Animator is not assigned!");
            return;
        }

        animator.ResetTrigger(hookTriggerName);
        animator.SetTrigger(hookTriggerName);
        Debug.Log("Playing HOOK animation");
    }

    private void TryUseTertiaryAbility()
    {
        if (areaAttackAbility == null)
        {
            Debug.LogWarning("PlayerCombat: areaAttackAbility is null!");
            return;
        }

        if (!areaAttackAbility.IsUnlocked || !areaAttackAbility.CanUse)
        {
            Debug.LogWarning("PlayerCombat: areaAttackAbility is locked or on cooldown!");
            return;
        }

        // Запускаем анимацию area-атаки
        PlayAreaAttackAnimation();

        areaAttackAbility.TryUseAbility(player);
    }

    private void PlayAreaAttackAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("PlayerCombat: Animator is not assigned!");
            return;
        }

        animator.ResetTrigger(areaAttackTriggerName);
        animator.SetTrigger(areaAttackTriggerName);
        Debug.Log("Playing AREA ATTACK animation");
    }

    private void SwitchPrimaryAbility()
    {
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
            currentPrimaryAbility = meleeAbility;
            Debug.Log("Switched to melee ability (default)");
        }

        Debug.Log($"Current primary ability is now: {currentPrimaryAbility.name}");
    }

    // ============================================================
    // СИСТЕМА МУТАЦИЙ — смена аниматора
    // ============================================================

    public void MutateToNextStage()
    {
        int nextStage = currentMutationStage + 1;

        if (mutationAnimators == null || nextStage >= mutationAnimators.Length)
        {
            Debug.LogWarning($"PlayerCombat: No animator for mutation stage {nextStage}! Max stage: {(mutationAnimators != null ? mutationAnimators.Length - 1 : 0)}");
            return;
        }

        SetMutationStage(nextStage);
    }

    public void SetMutationStage(int stage)
    {
        if (mutationAnimators == null || stage < 0 || stage >= mutationAnimators.Length)
        {
            Debug.LogWarning($"PlayerCombat: Invalid mutation stage {stage}!");
            return;
        }

        if (mutationAnimators[stage] == null)
        {
            Debug.LogWarning($"PlayerCombat: Animator for stage {stage} is not assigned!");
            return;
        }

        currentMutationStage = stage;
        animator.runtimeAnimatorController = mutationAnimators[stage];

        Debug.Log($"PlayerCombat: Mutated to stage {stage}. Animator changed to: {mutationAnimators[stage].name}");
    }

    public void ResetMutation()
    {
        SetMutationStage(0);
    }

    // ============================================================

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