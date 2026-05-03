using UnityEngine;
using System.Collections;

public class BossAttackState : EnemyAttackState
{
    private BossEnemy boss;
    private BossAttackControllerSO attackController;
    private int currentAttackIndex = -1;
    private Coroutine attackRoutine;

    public BossAttackState(Enemy enemy, EnemyStateMachine stateMachine, BossAttackControllerSO controller)
        : base(enemy, stateMachine)
    {
        this.boss = enemy as BossEnemy;
        this.attackController = controller;
    }

    public override void EnterState()
    {
        if (boss == null || attackController == null) return;

        // Stop movement during attack
        var nav = boss.GetComponent<EnemyNavMeshAgent2D>();
        nav?.Stop();

        currentAttackIndex = attackController.GetCurrentAttackIndex();
        if (currentAttackIndex < 0) return;

        attackRoutine = boss.StartCoroutine(ExecuteAttackRoutine(currentAttackIndex));
    }

    public override void FrameUpdate() { }
    public override void PhysicsUpdate() { }

    public override void ExitState()
    {
        if (attackRoutine != null)
        {
            boss.StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private IEnumerator ExecuteAttackRoutine(int index)
    {
        BossAttackControllerSO.BossAbilityEntry entry = attackController.GetCurrentAttackEntry();
        if (entry == null) yield break;

        switch (entry.Type)
        {
            case BossAttackControllerSO.AbilityType.MeleeSlam:
                yield return ExecuteMeleeSlam(entry.meleeSlam);
                break;
            case BossAttackControllerSO.AbilityType.Ranged:
                yield return ExecuteRanged(entry.ranged);
                break;
            case BossAttackControllerSO.AbilityType.QuickShot:
                yield return ExecuteQuickShot(entry.quickShot);
                break;
            default:
                yield break;
        }

        enemy.StateMachine.ChangeState(boss.ChaseState);
    }

    private IEnumerator ExecuteMeleeSlam(BossAttackControllerSO.MeleeSlamParams p)
    {
        if (boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            boss.Animator.SetTrigger(p.animationTrigger);

        yield return new WaitForSeconds(p.windup);

        Collider2D[] hit = Physics2D.OverlapCircleAll((Vector2)enemy.transform.position, p.radius);
        foreach (Collider2D col in hit)
        {
            if (col.CompareTag("Player") || col.CompareTag("PlayerProjectile")) continue;
            if (col.gameObject == enemy.gameObject) continue; // Prevent self-damage
            IDamageable d = col.GetComponent<IDamageable>();
            d?.TakeDamage(p.damage);
        }

        yield return new WaitForSeconds(p.attackDuration);
    }

    private IEnumerator ExecuteRanged(BossAttackControllerSO.RangedParams p)
    {
        if (boss.PlayerTarget == null) yield break;

        if (boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            boss.Animator.SetTrigger(p.animationTrigger);

        int burstCount = p.burstCount;
        float interval = 1f / p.fireRate;

        for (int i = 0; i < burstCount; i++)
        {
            if (boss.PlayerTarget == null) yield break;
            FireProjectile(p);
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator ExecuteQuickShot(BossAttackControllerSO.QuickShotParams p)
    {
        if (boss.PlayerTarget == null) yield break;

        if (boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            boss.Animator.SetTrigger(p.animationTrigger);

        Vector2 baseDir = ((Vector2)boss.PlayerTarget.position - (Vector2)boss.transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float halfSpread = p.spreadAngle / 2f;

        for (int i = 0; i < p.volleySize; i++)
        {
            if (boss.PlayerTarget == null) yield break;
            float angle = baseAngle + (i == 0 ? -halfSpread : (i == 1 ? 0f : halfSpread));
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            if (p.projectilePrefab != null)
            {
                GameObject proj = Object.Instantiate(p.projectilePrefab, boss.transform.position, Quaternion.identity);
                EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
                if (ep == null) ep = proj.AddComponent<EnemyProjectile>();
                ep.Initialize(dir, p.projectileSpeed, p.damage, p.projectileLifetime, boss.gameObject);
            }

            yield return new WaitForSeconds(p.volleyDelay);
        }
    }

    private void FireProjectile(BossAttackControllerSO.RangedParams p)
    {
        if (p.projectilePrefab == null) return;
        if (boss.PlayerTarget == null) return;

        Vector2 dir = ((Vector2)boss.PlayerTarget.position - (Vector2)boss.transform.position).normalized;
        GameObject proj = Object.Instantiate(p.projectilePrefab, boss.transform.position, Quaternion.identity);
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep == null) ep = proj.AddComponent<EnemyProjectile>();
        ep.Initialize(dir, p.projectileSpeed, p.damage, p.projectileLifetime, boss.gameObject);
    }
}
