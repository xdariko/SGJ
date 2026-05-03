using UnityEngine;

public class EnemyInvestigateState : EnemyState
{
    public EnemyInvestigateState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) { }


    public override void EnterState()
    {
        base.EnterState();
        enemy.EnemyAnimator?.PlayState(EnemyAnimState.Alert);
        enemy.EnemyInvestigateBaseInstance.DoEnterLogic();
    }

    public override void ExitState()
    {
        base.ExitState();
        enemy.EnemyInvestigateBaseInstance.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        Debug.LogWarning($"[EnemyInvestigateState] FrameUpdate: Using investigate logic type: {enemy.EnemyInvestigateBaseInstance.GetType().Name}");
        enemy.EnemyInvestigateBaseInstance.DoFrameUpdateLogic();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        enemy.EnemyInvestigateBaseInstance.DoPhysicsLogic();
    }
}