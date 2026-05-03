using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class BossDashState : BossSpecialState
{
    private BossAttackControllerSO.DashParams dashParams;
    private NavMeshAgent navAgent;

    public BossDashState(BossEnemy boss, EnemyStateMachine stateMachine, BossAttackControllerSO.DashParams dashParams)
        : base(boss, stateMachine)
    {
        this.dashParams = dashParams;
        navAgent = boss.GetComponent<NavMeshAgent>();
    }

    protected override IEnumerator Run()
    {
        // Set vulnerability after dash
        boss.SetVulnerableToHook(true, dashParams.vulnerableDuration);

        // Determine direction
        Vector3 playerPos = boss.PlayerTarget.position;
        Vector3 bossPos = boss.transform.position;
        Vector3 toPlayer = (playerPos - bossPos).normalized;
        Vector3 dashDir = dashParams.dashAwayFromPlayer ? -toPlayer : toPlayer;
        Vector3 targetPos = bossPos + dashDir * (dashParams.dashDuration * dashParams.dashSpeed);

        // Execute dash
        if (navAgent != null)
        {
            navAgent.speed = dashParams.dashSpeed;
            navAgent.SetDestination(targetPos);
        }
        else
        {
            boss.RB.linearVelocity = new Vector2(dashDir.x, dashDir.y) * dashParams.dashSpeed;
        }

        yield return new WaitForSeconds(dashParams.dashDuration);

        // Stop
        if (navAgent != null)
        {
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }
        else
        {
            boss.RB.linearVelocity = Vector2.zero;
        }

        // Post-action delay handled by BossSpecialState.Complete after this coroutine ends
        Complete();
    }
}
