using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    [System.Serializable]
    public class AbilityUnlock
    {
        public string abilityName;
        public BaseAbility ability;
        public bool isUnlocked;
        public string unlockConditionDescription;
    }

    [SerializeField] private List<AbilityUnlock> abilities = new List<AbilityUnlock>();
    [SerializeField] private PlayerCombat playerCombat;

    private Dictionary<string, BaseAbility> abilityDictionary = new Dictionary<string, BaseAbility>();

    // Public methods for setting up abilities (used by PlayerExampleSetup)
    public void SetPlayerCombat(PlayerCombat combat) { playerCombat = combat; }
    public void AddAbility(string name, BaseAbility ability)
    {
        // Check for duplicates before adding
        if (abilities.Exists(a => a.abilityName == name))
        {
            Debug.Log($"AbilityManager: Ability '{name}' already exists, skipping duplicate.");
            return;
        }
        
        abilities.Add(new AbilityUnlock { abilityName = name, ability = ability });
    }

    private void Start()
    {
        InitializeAbilityDictionary();
    }

    private void InitializeAbilityDictionary()
    {
        abilityDictionary.Clear();
        foreach (var abilityUnlock in abilities)
        {
            if (abilityUnlock.ability != null)
            {
                abilityDictionary[abilityUnlock.abilityName] = abilityUnlock.ability;
                // Only lock if not already unlocked
                if (!abilityUnlock.ability.IsUnlocked)
                {
                    abilityUnlock.ability.Lock();
                }
            }
        }
    }

    public void UnlockAbility(string abilityName)
    {
        if (abilityDictionary.TryGetValue(abilityName, out var ability))
        {
            ability.Unlock();

            // Handle special cases for ability unlocks
            HandleSpecialAbilityUnlock(abilityName, ability);

            Debug.Log($"Ability unlocked: {abilityName}");
        }
        else
        {
            Debug.LogWarning($"Ability not found: {abilityName}");
        }
    }

    public void LockAbility(string abilityName)
    {
        if (abilityDictionary.TryGetValue(abilityName, out var ability))
        {
            ability.Lock();
            Debug.Log($"Ability locked: {abilityName}");
        }
    }

    public bool IsAbilityUnlocked(string abilityName)
    {
        return abilityDictionary.TryGetValue(abilityName, out var ability) && ability.IsUnlocked;
    }

    public BaseAbility GetAbility(string abilityName)
    {
        abilityDictionary.TryGetValue(abilityName, out var ability);
        return ability;
    }

    private void HandleSpecialAbilityUnlock(string abilityName, BaseAbility ability)
    {
        switch (abilityName)
        {
            case "Ranged":
                playerCombat?.UnlockRangedAbility();
                break;
            case "Hook":
                playerCombat?.UnlockHookAbility();
                break;
            case "AreaAttack":
                playerCombat?.UnlockAreaAttackAbility();
                break;
            // Add more special cases as needed
        }
    }

    public void UnlockAllAbilities()
    {
        foreach (var abilityUnlock in abilities)
        {
            if (abilityUnlock.ability != null)
            {
                abilityUnlock.ability.Unlock();
                HandleSpecialAbilityUnlock(abilityUnlock.abilityName, abilityUnlock.ability);
            }
        }
        Debug.Log("All abilities unlocked!");
    }

    public void ResetAllAbilities()
    {
        foreach (var abilityUnlock in abilities)
        {
            if (abilityUnlock.ability != null)
            {
                abilityUnlock.ability.Lock();
            }
        }

        // Reset combat system
        if (playerCombat != null)
        {
            // Would need to reset primary ability back to melee
            // This would require access to the melee ability
        }

        Debug.Log("All abilities reset!");
    }

    // Editor button for testing
    [ContextMenu("Unlock All Abilities")]
    private void UnlockAllAbilitiesEditor()
    {
        UnlockAllAbilities();
    }

    [ContextMenu("Reset All Abilities")]
    private void ResetAllAbilitiesEditor()
    {
        ResetAllAbilities();
    }
}