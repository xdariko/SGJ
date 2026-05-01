using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack-Simple Melee", menuName = "Enemy Logic/Attack Logic/Simple Melee")]
public class EnemyAttackSimpleMelee : EnemyAttackSOBase
{
    [Header("Attack Settings")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _attackCooldown = 1f;
    [SerializeField] private float _attackWindup = 0.25f;
    [SerializeField] private float _attackRange = 1f;
    [SerializeField] private float _attackRadius = 0.45f;
    [SerializeField] private LayerMask _playerLayer;

    private Coroutine _attackRoutine;

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        enemy.MoveEnemy(Vector2.zero);
        _attackRoutine = enemy.StartCoroutine(AttackLoop());
    }

    public override void DoExitLogic()
    {
        if (_attackRoutine != null)
        {
            enemy.StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        FacePlayer();

        if (!enemy.IsAggroed)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }

        if (!enemy.IsWithinStrikingDistance)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
        }
    }

    private IEnumerator AttackLoop()
    {
        while (enemy.IsAggroed && enemy.IsWithinStrikingDistance)
        {
            enemy.MoveEnemy(Vector2.zero);
            FacePlayer();

            // enemy.GetComponent<Animator>()?.SetTrigger("Attack");

            yield return new WaitForSeconds(_attackWindup);

            TryDamagePlayer();

            yield return new WaitForSeconds(_attackCooldown);
        }

        if (enemy.IsAggroed)
            enemy.StateMachine.ChangeState(enemy.ChaseState);
        else
            enemy.StateMachine.ChangeState(enemy.IdleState);
    }

    private void TryDamagePlayer()
    {
        Vector2 attackCenter = GetAttackCenter();
        Collider2D hit = Physics2D.OverlapCircle(attackCenter, _attackRadius, _playerLayer);

        if (hit == null)
            return;

        hit.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);
    }

    private Vector2 GetAttackCenter()
    {
        Vector2 directionToPlayer = playerTransform != null
            ? ((Vector2)playerTransform.position - (Vector2)enemy.transform.position).normalized
            : (enemy.IsFacingRight ? Vector2.right : Vector2.left);

        return (Vector2)enemy.transform.position + directionToPlayer * _attackRange;
    }

    private void FacePlayer()
    {
        if (playerTransform == null)
            return;

        Vector2 directionToPlayer = playerTransform.position - enemy.transform.position;
        enemy.CheckForLeftOrRightFacing(directionToPlayer);
    }
}