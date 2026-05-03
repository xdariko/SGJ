using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class BossChargeState : BossSpecialState
{
    private BossAttackControllerSO.ChargeParams chargeParams;
    private NavMeshAgent navAgent;

    public BossChargeState(BossEnemy boss, EnemyStateMachine stateMachine, BossAttackControllerSO.ChargeParams chargeParams)
        : base(boss, stateMachine)
    {
        this.chargeParams = chargeParams;
        navAgent = boss.GetComponent<NavMeshAgent>();
    }

    protected override IEnumerator Run()
    {
        // Set vulnerability (includes duration)
        boss.SetVulnerableToRanged(true, chargeParams.vulnerableDuration);

        // Windup
        yield return new WaitForSeconds(chargeParams.windup);

        // Charge towards player
        Vector3 playerPos = boss.PlayerTarget.position;
        Vector3 bossPos = boss.transform.position;
        Vector3 dir = (playerPos - bossPos).normalized;
        Vector3 targetPos = bossPos + dir * (chargeParams.chargeDuration * chargeParams.chargeSpeed);

        if (navAgent != null)
        {
            navAgent.speed = chargeParams.chargeSpeed;
            navAgent.SetDestination(targetPos);
        }
        else
        {
            boss.RB.linearVelocity = new Vector2(dir.x, dir.y) * chargeParams.chargeSpeed;
        }

        yield return new WaitForSeconds(chargeParams.chargeDuration);

        // Stop movement
        if (navAgent != null)
        {
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }
        else
        {
            boss.RB.linearVelocity = Vector2.zero;
        }

        // AoE damage at impact location - ONLY damage Player
        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)boss.transform.position, chargeParams.aoeRadius);
        foreach (Collider2D col in hits)
        {
            // Skip self
            if (col.gameObject == boss.gameObject) continue;
            
            // Only damage Player
            if (col.CompareTag("Player"))
            {
                IDamageable d = col.GetComponent<IDamageable>();
                if (d != null)
                    d.TakeDamage(chargeParams.chargeDamage);
            }
        }

        // Return to normal behavior
        Complete();
    }
}
