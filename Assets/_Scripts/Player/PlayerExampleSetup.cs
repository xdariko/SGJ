using UnityEngine;

public class PlayerExampleSetup : MonoBehaviour
{
    [Header("Ability References")]
    public MeleeAbility meleeAbility;
    public RangedAbility rangedAbility;
    public HookAbility hookAbility;
    public AreaAttackAbility areaAttackAbility;
    public DashAbility dashAbility;

    [Header("Auto Setup")]
    public bool autoSetupPlayer = true;
    public bool unlockAllAbilitiesByDefault = true;

    private void Awake()
    {
        if (autoSetupPlayer)
        {
            SetupPlayerComponents();
        }
    }

    private void SetupPlayerComponents()
    {
        Player player = GetComponent<Player>();
        PlayerController playerController = GetComponent<PlayerController>();
        PlayerCombat combat = GetComponent<PlayerCombat>();
        AbilityManager abilityManager = GetComponent<AbilityManager>();

        if (player == null)
        {
            player = gameObject.AddComponent<Player>();
        }

        if (playerController == null)
        {
            playerController = gameObject.AddComponent<PlayerController>();
        }

        if (combat == null)
        {
            combat = gameObject.AddComponent<PlayerCombat>();
        }

        if (abilityManager == null)
        {
            abilityManager = gameObject.AddComponent<AbilityManager>();
        }

        // Setup combat abilities
        if (combat != null)
        {
            combat.SetMeleeAbility(meleeAbility);
            combat.SetRangedAbility(rangedAbility);
            combat.SetHookAbility(hookAbility);
            combat.SetAreaAttackAbility(areaAttackAbility);
            combat.SetDashAbility(dashAbility);
        }

        // Setup ability manager
        if (abilityManager != null)
        {
            abilityManager.SetPlayerCombat(combat);

            if (meleeAbility != null)
            {
                abilityManager.AddAbility("Melee", meleeAbility);
                meleeAbility.Initialize();
                meleeAbility.Unlock();
            }

            if (rangedAbility != null)
            {
                abilityManager.AddAbility("Ranged", rangedAbility);
                rangedAbility.Initialize();
                rangedAbility.Unlock();
                combat.UnlockRangedAbility();
            }

            if (hookAbility != null)
            {
                abilityManager.AddAbility("Hook", hookAbility);
                hookAbility.Initialize();
                hookAbility.Unlock();
                combat.UnlockHookAbility();
            }

            if (areaAttackAbility != null)
            {
                abilityManager.AddAbility("AreaAttack", areaAttackAbility);
                areaAttackAbility.Initialize();
                areaAttackAbility.Unlock();
                combat.UnlockAreaAttackAbility();
            }

            if (dashAbility != null)
            {
                abilityManager.AddAbility("Dash", dashAbility);
                dashAbility.Initialize();
                dashAbility.Unlock();
            }

            Debug.Log("PlayerExampleSetup: All abilities initialized and unlocked!");
        }
    }

    [ContextMenu("Setup Player Components")]
    void SetupPlayerComponentsEditor()
    {
        SetupPlayerComponents();
    }
}
