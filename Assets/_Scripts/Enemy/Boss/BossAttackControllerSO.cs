using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossAttackController", menuName = "Enemy Logic/Boss/Boss Attack Controller")]
public class BossAttackControllerSO : ScriptableObject
{
    public enum AbilityType
    {
        MeleeSlam,
        Ranged,
        QuickShot,
        Charge,
        Dash,
        Summon
    }

    [Header("Global Settings")]
    public float postSpecialDelay = 0.5f;

    [System.Serializable]
    public class BossAbilityEntry
    {
        public AbilityType Type;
        public float Cooldown = 5f;
        public float ChanceWeight = 1f;
        public bool RequireLineOfSight = false;
        public float MinRange = 0f;
        public float MaxRange = 20f;

        // Normal attack parameters
        public MeleeSlamParams meleeSlam;
        public RangedParams ranged;
        public QuickShotParams quickShot;

        // Special ability parameters
        public ChargeParams charge;
        public DashParams dash;
        public SummonParams summon;

        [HideInInspector] public float currentCooldown;
    }

    [System.Serializable]
    public class MeleeSlamParams
    {
        public float damage = 20f;
        public float radius = 1.5f;
        public float windup = 0.3f;
        public float attackDuration = 0.5f;
        public string animationTrigger = "MeleeSlam";
    }

    [System.Serializable]
    public class RangedParams
    {
        public float damage = 10f;
        public float projectileSpeed = 8f;
        public float projectileLifetime = 3f;
        public GameObject projectilePrefab;
        public float fireRate = 1f; // shots per second
        public int burstCount = 1;
        public string animationTrigger = "RangedAttack";
    }

    [System.Serializable]
    public class QuickShotParams
    {
        public int volleySize = 3;
        public float spreadAngle = 15f; // total cone angle
        public float damage = 8f;
        public float projectileSpeed = 10f;
        public float projectileLifetime = 2f;
        public GameObject projectilePrefab;
        public float volleyDelay = 0.1f; // delay between shots
        public string animationTrigger = "QuickShot";
    }

    [System.Serializable]
    public class ChargeParams
    {
        public float chargeSpeed = 12f;
        public float chargeDuration = 0.8f;
        public float chargeDamage = 25f;
        public float aoeRadius = 1.2f;
        public float windup = 0.2f;
        public float vulnerableDuration = 2f;
        public string animationTrigger = "Charge";
    }

    [System.Serializable]
    public class DashParams
    {
        public float dashSpeed = 15f;
        public float dashDuration = 0.4f;
        public bool dashAwayFromPlayer = true; // true = away, false = toward/perp
        public float vulnerableDuration = 2f;
        public string animationTrigger = "Dash";
    }

    [System.Serializable]
    public class SummonParams
    {
        public GameObject[] minionPrefabs;
        public int waveSize = 3;
        public float summonDuration = 5f;
        public bool waitForMinionsDeath = false;
        public string animationTrigger = "Summon";
    }

    [Header("Abilities (Ordered by Priority)")]
    public List<BossAbilityEntry> abilities = new List<BossAbilityEntry>();

    private Dictionary<int, float> cooldownTimers = new Dictionary<int, float>();
    private BossEnemy boss;

    public virtual void Initialize(BossEnemy boss)
    {
        this.boss = boss;
        cooldownTimers.Clear();
        for (int i = 0; i < abilities.Count; i++)
        {
            cooldownTimers[i] = 0f;
        }
        Debug.Log($"[BossAttackController] Initialized for boss: {boss.name}, abilities count: {abilities.Count}");
    }

    public virtual void UpdateAbilities(BossEnemy boss, EnemyStateMachine stateMachine)
    {
        Debug.Log($"[BossAttackController] UpdateAbilities: IsPerformingSpecial={boss.IsPerformingSpecial}, IsAggroed={boss.IsAggroed}, CurrentState={stateMachine?.CurrentEnemyState?.GetType().Name}");

        if (boss == null) return;

        // Update cooldowns always
        for (int i = 0; i < abilities.Count; i++)
        {
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;
            }
        }

        // If boss is performing a special ability, skip selecting new abilities
        if (boss.IsPerformingSpecial) return;

        // Try to find and execute an ability
        for (int i = 0; i < abilities.Count; i++)
        {
            BossAbilityEntry entry = abilities[i];
            if (cooldownTimers[i] > 0f) continue;

            bool canUse = CanUseAbility(i, boss);
            Debug.Log($"[BossAttackController] Checking ability {i} ({entry.Type}): CanUse={canUse}, Cooldown={cooldownTimers[i]:F2}");

            if (!canUse) continue;

            bool isSpecial = IsSpecialAbility(entry.Type);

            if (isSpecial)
            {
                Debug.Log($"[BossAttackController] >>> Using SPECIAL ability {i} ({entry.Type})");
                cooldownTimers[i] = entry.Cooldown;
                boss.StartSpecialAbility(i);
            }
            else
            {
                // Normal attack - ensure we set index before state change
                if (stateMachine.CurrentEnemyState is not EnemyAttackState)
                {
                    Debug.Log($"[BossAttackController] >>> Using NORMAL attack {i} ({entry.Type}), changing to AttackState");
                    SetCurrentAttackIndex(i);
                    cooldownTimers[i] = entry.Cooldown;
                    stateMachine.ChangeState(boss.AttackState);
                }
                else
                {
                    Debug.Log($"[BossAttackController] Skipping attack {i} because already in AttackState");
                }
            }

            break; // Only one ability per check cycle
        }
    }

    protected virtual bool CanUseAbility(int index, BossEnemy boss)
    {
        BossAbilityEntry entry = abilities[index];
        if (!boss.IsAggroed)
        {
            Debug.Log($"[CanUseAbility] {index} ({entry.Type}): FAILED - not aggroed");
            return false;
        }

        Transform player = boss.PlayerTarget;
        if (player == null)
        {
            Debug.Log($"[CanUseAbility] {index} ({entry.Type}): FAILED - PlayerTarget is null");
            return false;
        }

        // Check distance to player
        float distanceToPlayer = Vector3.Distance(boss.transform.position, player.position);
        if (distanceToPlayer < entry.MinRange || distanceToPlayer > entry.MaxRange)
        {
            Debug.Log($"[CanUseAbility] {index} ({entry.Type}): FAILED - distance {distanceToPlayer:F2} not in range [{entry.MinRange}, {entry.MaxRange}]");
            return false;
        }

        // Line of sight check
        if (entry.RequireLineOfSight)
        {
            Vector2 dir = (player.position - boss.transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(boss.transform.position, dir, distanceToPlayer);
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                Debug.Log($"[CanUseAbility] {index} ({entry.Type}): FAILED - no line of sight, hit {hit.collider.gameObject.name}");
                return false;
            }
        }

        Debug.Log($"[CanUseAbility] {index} ({entry.Type}): PASSED - distance {distanceToPlayer:F2}");
        return true;
    }

    public virtual float GetSpecialDuration(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Count) return 0f;
        BossAbilityEntry entry = abilities[abilityIndex];
        AbilityType type = entry.Type;

        switch (type)
        {
            case AbilityType.Charge:
                return entry.charge.windup + entry.charge.chargeDuration;
            case AbilityType.Dash:
                return entry.dash.dashDuration;
            case AbilityType.Summon:
                return entry.summon.summonDuration;
            default:
                return 0f;
        }
    }

    public virtual EnemyState CreateSpecialState(int abilityIndex, BossEnemy boss)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Count)
        {
            Debug.LogWarning($"[BossAttackController] CreateSpecialState: abilityIndex {abilityIndex} out of range");
            return null;
        }
        BossAbilityEntry entry = abilities[abilityIndex];
        EnemyStateMachine sm = boss.StateMachine;

        EnemyState state = entry.Type switch
        {
            AbilityType.Charge => new BossChargeState(boss, sm, entry.charge),
            AbilityType.Dash => new BossDashState(boss, sm, entry.dash),
            AbilityType.Summon => new BossSummonState(boss, sm, entry.summon),
            _ => null
        };

        Debug.Log($"[BossAttackController] CreateSpecialState: abilityIndex={abilityIndex}, type={entry.Type}, stateCreated={state != null}");
        return state;
    }

    public virtual string GetSpecialAnimation(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Count) return null;
        BossAbilityEntry entry = abilities[abilityIndex];
        switch (entry.Type)
        {
            case AbilityType.Charge: return entry.charge.animationTrigger;
            case AbilityType.Dash: return entry.dash.animationTrigger;
            case AbilityType.Summon: return entry.summon.animationTrigger;
            case AbilityType.MeleeSlam: return entry.meleeSlam.animationTrigger;
            case AbilityType.Ranged: return entry.ranged.animationTrigger;
            case AbilityType.QuickShot: return entry.quickShot.animationTrigger;
            default: return null;
        }
    }

    // For normal attacks
    private int currentAttackIndex = -1;
    public void SetCurrentAttackIndex(int index) => currentAttackIndex = index;
    public int GetCurrentAttackIndex() => currentAttackIndex;
    public BossAbilityEntry GetCurrentAttackEntry()
    {
        if (currentAttackIndex < 0 || currentAttackIndex >= abilities.Count) return null;
        return abilities[currentAttackIndex];
    }

    public MeleeSlamParams GetMeleeSlamParams(int index) => abilities[index].meleeSlam;
    public RangedParams GetRangedParams(int index) => abilities[index].ranged;
    public QuickShotParams GetQuickShotParams(int index) => abilities[index].quickShot;

    protected virtual bool IsSpecialAbility(AbilityType type)
    {
        return type == AbilityType.Charge || type == AbilityType.Dash || type == AbilityType.Summon;
    }
}
