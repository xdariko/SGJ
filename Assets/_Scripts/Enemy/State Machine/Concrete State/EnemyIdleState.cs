using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
    }


    public override void EnterState()
    {
        base.EnterState();
        enemy.EnemyAnimator?.PlayState(EnemyAnimState.Idle);
        enemy.EnemyIdleBaseInstance.DoEnterLogic();
    }

    public override void ExitState()
    {
        base.ExitState();
        enemy.EnemyIdleBaseInstance.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        Debug.LogWarning($"[EnemyIdleState] FrameUpdate: Using idle logic type: {enemy.EnemyIdleBaseInstance.GetType().Name}");
        enemy.EnemyIdleBaseInstance.DoFrameUpdateLogic();

        // If enemy becomes aggroed, switch to Chase state
        if (enemy.IsAggroed)
        {
            Debug.LogWarning($"[EnemyIdleState] >>> Enemy {enemy.name} is aggroed, attempting state change to Chase");
            if (enemy.StateMachine == null)
            {
                Debug.LogError("[EnemyIdleState] StateMachine is NULL! Cannot change state.");
                return;
            }
            if (enemy.ChaseState == null)
            {
                Debug.LogError("[EnemyIdleState] ChaseState is NULL! Initialize it in Awake.");
                return;
            }
            // Avoid double-transition if already in Chase (e.g., DoFrameUpdateLogic already switched)
            if (enemy.StateMachine.CurrentEnemyState != enemy.ChaseState)
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
            else
            {
                Debug.LogWarning("[EnemyIdleState] Already in ChaseState, skipping redundant transition");
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        enemy.EnemyIdleBaseInstance.DoPhysicsLogic();
    }
}
