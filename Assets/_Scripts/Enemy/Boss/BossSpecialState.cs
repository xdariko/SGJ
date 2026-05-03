using UnityEngine;
using System.Collections;

public abstract class BossSpecialState : EnemyState
{
    protected BossEnemy boss;

    protected BossSpecialState(BossEnemy boss, EnemyStateMachine stateMachine) : base(boss, stateMachine)
    {
        this.boss = boss;
    }

    public override void EnterState()
    {
        base.EnterState();
        boss.IsPerformingSpecial = true;
        boss.StartCoroutine(Run());
    }

    protected abstract IEnumerator Run();

    protected void Complete()
    {
        boss.IsPerformingSpecial = false;
        enemyStateMachine.ChangeState(boss.ChaseState);
    }

    public override void ExitState()
    {
        base.ExitState();
        boss.IsPerformingSpecial = false;
    }
}
