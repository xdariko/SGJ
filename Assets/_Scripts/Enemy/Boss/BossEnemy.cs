using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] protected BossAttackControllerSO attackController;
    [SerializeField] public float postSpecialDelay = 0.5f;
    [SerializeField] protected float vulnerableDuration = 2f;

    [Header("Vulnerability (Tank/Agile)")]
    [SerializeField] protected bool useRangedVulnerability = false;
    [SerializeField] protected bool useHookVulnerability = false;

    public bool IsPerformingSpecial { get; set; }
    public bool VulnerableToRanged { get; private set; }
    public bool VulnerableToHook { get; private set; }
    public bool UsesRangedVulnerability => useRangedVulnerability;
    public bool UsesHookVulnerability => useHookVulnerability;
    public List<GameObject> ActiveMinions { get; } = new List<GameObject>();
    public Animator Animator { get; set; }

    protected BossAttackState bossAttackState;

    protected override void Awake()
    {
        base.Awake();
        Animator = GetComponent<Animator>();
        Debug.Log($"[BossEnemy] Awake: Animator present: {Animator != null}");
    }

    protected override void Start()
    {
        base.Start();
        Debug.Log($"[BossEnemy] Start: attackController={(attackController != null ? attackController.name : "NULL")}");
        if (attackController != null)
        {
            attackController.Initialize(this);
            bossAttackState = new BossAttackState(this, StateMachine, attackController);
            AttackState = bossAttackState;
            Debug.Log($"[BossEnemy] BossAttackState created and assigned. Abilities count: {attackController.abilities?.Count ?? 0}");
        }
        else
        {
            Debug.LogWarning($"BossEnemy {name} has no AttackController assigned!", this);
        }

        VulnerableToRanged = false;
        VulnerableToHook = false;
    }

    private new void Update()
    {
        base.Update();
        if (!IsPerformingSpecial)
        {
            attackController?.UpdateAbilities(this, StateMachine);
        }
        Debug.Log($"[BossEnemy] Update: IsAggroed={IsAggroed}, State={StateMachine?.CurrentEnemyState?.GetType().Name}, PlayerTargetExists={PlayerTarget != null}, IsPerformingSpecial={IsPerformingSpecial}");
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public virtual void StartSpecialAbility(int abilityIndex)
    {
        if (IsPerformingSpecial) return;
        if (attackController == null) return;

        Debug.Log($"[BossEnemy] StartSpecialAbility: abilityIndex={abilityIndex}, type={attackController.GetSpecialAnimation(abilityIndex)}");
        IsPerformingSpecial = true;

        EnemyState specialState = attackController.CreateSpecialState(abilityIndex, this);
        if (specialState != null)
        {
            StateMachine.ChangeState(specialState);
        }
        else
        {
            Debug.LogWarning($"[BossEnemy] CreateSpecialState returned null for abilityIndex {abilityIndex}");
        }

        string animTrigger = attackController.GetSpecialAnimation(abilityIndex);
        if (!string.IsNullOrEmpty(animTrigger) && Animator != null)
        {
            Animator.SetTrigger(animTrigger);
        }
    }

    public virtual void SetVulnerableToRanged(bool vulnerable)
    {
        SetVulnerableToRanged(vulnerable, vulnerableDuration);
    }

    public virtual void SetVulnerableToRanged(bool vulnerable, float duration)
    {
        VulnerableToRanged = vulnerable;
        if (vulnerable && duration > 0f)
        {
            CancelInvoke(nameof(ResetVulnerableRanged));
            Invoke(nameof(ResetVulnerableRanged), duration);
        }
    }

    private void ResetVulnerableRanged() => VulnerableToRanged = false;

    public virtual void SetVulnerableToHook(bool vulnerable)
    {
        SetVulnerableToHook(vulnerable, vulnerableDuration);
    }

    public virtual void SetVulnerableToHook(bool vulnerable, float duration)
    {
        VulnerableToHook = vulnerable;
        if (vulnerable && duration > 0f)
        {
            CancelInvoke(nameof(ResetVulnerableHook));
            Invoke(nameof(ResetVulnerableHook), duration);
        }
    }

    private void ResetVulnerableHook() => VulnerableToHook = false;

    public virtual void RegisterMinion(GameObject minion)
    {
        if (!ActiveMinions.Contains(minion))
        {
            ActiveMinions.Add(minion);
            EnemyHealth health = minion.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.OnDeath += () => UnregisterMinion(minion);
            }
        }
    }

    public virtual void UnregisterMinion(GameObject minion) => ActiveMinions.Remove(minion);

    public virtual void ClearMinions()
    {
        foreach (GameObject minion in new List<GameObject>(ActiveMinions))
        {
            if (minion != null) Destroy(minion);
        }
        ActiveMinions.Clear();
    }

    private void OnDestroy() => ClearMinions();
}
