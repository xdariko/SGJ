using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Direct To Player", menuName = "Enemy Logic/Chase Logic/Direct To Player")]
public class EnemyChaseDirectToPlayer : EnemyChaseSOBase
{
    [SerializeField] private float _movementSpeed = 1.75f;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        if (enemy.IsWithinStrikingDistance)
        {
            enemy.MoveEnemy(Vector2.zero);
            enemy.StateMachine.ChangeState(enemy.AttackState);
            return;
        }

        if (!enemy.IsAggroed)
        {
            enemy.MoveEnemy(Vector2.zero);
            enemy.InvestigationTargetPosition = playerTransform.position;
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        Vector2 moveDirection = (playerTransform.position - enemy.transform.position).normalized;
        enemy.MoveEnemy(moveDirection * _movementSpeed);
    }

    public override void DoPhysicsLogic()
    {
        base.DoPhysicsLogic();
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}
