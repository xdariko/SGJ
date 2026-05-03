using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack-Simple Screamer", menuName = "Enemy Logic/Attack Logic/Simple Screamer")]
public class EnemyAttackSimpleScreamer : EnemyAttackSOBase
{
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private float _teleportDistance = 50f;
    [SerializeField] private float _screamerDuration = 0.5f;

    [SerializeField] private GameObject _screamer;

    private bool _hasAttacked = false;


    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        _hasAttacked = false;
        enemy.StartCoroutine(PerformAttack());
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        enemy.StopAllCoroutines();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
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

    private IEnumerator PerformAttack()
    {
        if (_screamer != null)
        {
            GameObject screamerInstance = GameObject.Instantiate(_screamer);
            GameObject.Destroy(screamerInstance, _screamerDuration);
        }

        Debug.Log("SCREAMER");

        yield return new WaitForSeconds(0.2f);

        TeleportAwayFromPlayer();

        _hasAttacked = true;

        yield return new WaitForSeconds(_attackCooldown);

        enemy.StateMachine.ChangeState(enemy.IdleState);
    }

    private void TeleportAwayFromPlayer()
    {
        Vector2 directionFromPlayer = (enemy.transform.position - playerTransform.position).normalized;
        Vector2 randomOffset = Random.insideUnitCircle.normalized * _teleportDistance;
        Vector2 teleportPosition = (Vector2)playerTransform.position + directionFromPlayer * _teleportDistance + randomOffset;

        enemy.transform.position = teleportPosition;

        Vector2 moveDirection = (teleportPosition - (Vector2)enemy.transform.position).normalized;
        enemy.CheckForLeftOrRightFacing(moveDirection);
    }
}
