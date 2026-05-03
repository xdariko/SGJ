using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseSOBase : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;

    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObj != null ? playerObj.transform : null;
    }

    public virtual void DoEnterLogic() { }
    public virtual void DoExitLogic() { ResetValues(); }
    public virtual void DoFrameUpdateLogic()
    {
        if (enemy.IsWithinStrikingDistance)
        {
            enemy.StateMachine.ChangeState(enemy.AttackState);
        }
        else if (!enemy.IsAggroed)
        {
            enemy.InvestigationTargetPosition = playerTransform.position;
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
        }
    }

    public virtual void DoPhysicsLogic() { }
    public virtual void ResetValues() { }
}
