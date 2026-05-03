using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Attack-Simple Melee", menuName = "Enemy Logic/Attack Logic/Simple Melee")]
public class EnemyAttackSimpleMelee : EnemyAttackSOBase
{
    [Header("Attack Settings")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _attackCooldown = 1f;
    [FormerlySerializedAs("_attackWindup")]
    [SerializeField] private float _attackAnimationFallbackDuration = 0.2f;
    [SerializeField] private float _attackRadius = 1f;
    [SerializeField] private LayerMask _playerLayer;

    private EnemyNavMeshAgent2D _navMeshAgent2D;
    private Coroutine _attackRoutine;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _navMeshAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        StopEnemyCompletely();
        _attackRoutine = enemy.StartCoroutine(AttackLoop());
    }

    public override void DoExitLogic()
    {
        if (_attackRoutine != null)
        {
            enemy.StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        StopEnemyCompletely();
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        StopEnemyCompletely();
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
            StopEnemyCompletely();
            FacePlayer();

            if (enemy.EnemyAnimator != null)
                yield return enemy.EnemyAnimator.PlayAttackAndWait(_attackAnimationFallbackDuration);
            else
                yield return new WaitForSeconds(_attackAnimationFallbackDuration);

            if (enemy.IsAggroed && enemy.IsWithinStrikingDistance)
                TryDamagePlayer();

            yield return new WaitForSeconds(_attackCooldown);
        }
    }

    private void TryDamagePlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(enemy.transform.position, _attackRadius, _playerLayer);
        if (hit == null)
            return;

        hit.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);
    }

    private void StopEnemyCompletely()
    {
        _navMeshAgent2D?.Stop();
        enemy.MoveEnemy(Vector2.zero);
    }

    private void FacePlayer()
    {
        if (playerTransform == null)
            return;

        Vector2 directionToPlayer = playerTransform.position - enemy.transform.position;
        enemy.CheckForLeftOrRightFacing(directionToPlayer);
    }
}
