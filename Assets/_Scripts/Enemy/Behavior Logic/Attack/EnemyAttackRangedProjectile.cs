using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Attack-Ranged Projectile", menuName = "Enemy Logic/Attack Logic/Ranged Projectile")]
public class EnemyAttackRangedProjectile : EnemyAttackSOBase
{
    [Header("Projectile")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _damage = 8f;
    [SerializeField] private float _projectileSpeed = 6f;
    [SerializeField] private float _projectileLifetime = 4f;

    [Header("Attack Timing")]
    [SerializeField] private float _attackCooldown = 1.5f;

    [Tooltip("Used only if the Animator cannot report the real Attack clip length.")]
    [FormerlySerializedAs("_attackWindup")]
    [FormerlySerializedAs("_attackAnimationFallbackDuration")]
    [SerializeField] private float _projectileSpawnDelayAfterAnimation = 0.6f;

    [SerializeField] private bool _stopWhileAttacking = true;

    private EnemyNavMeshAgent2D _navMeshAgent2D;
    private NavMeshAgent _agent;
    private Coroutine _attackRoutine;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _navMeshAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        _agent = gameObject.GetComponent<NavMeshAgent>();
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
        if (_stopWhileAttacking)
            StopEnemyCompletely();

        FacePlayer();

        if (!enemy.IsAggroed)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (enemy.IsAggroed)
        {
            StopEnemyCompletely();
            FacePlayer();

            yield return PlayAttackAnimationBeforeProjectile();

            // Projectile is spawned only after the attack animation has finished.
            if (enemy.IsAggroed)
                ShootProjectile();

            // Do not keep Animator in Attack during cooldown.
            enemy.EnemyAnimator?.PlayState(EnemyAnimState.Idle);

            yield return new WaitForSeconds(_attackCooldown);
        }

        enemy.StateMachine.ChangeState(enemy.IdleState);
    }


    private IEnumerator PlayAttackAnimationBeforeProjectile()
    {
        if (enemy.EnemyAnimator == null)
        {
            yield return new WaitForSeconds(_projectileSpawnDelayAfterAnimation);
            yield break;
        }

        enemy.EnemyAnimator.PlayAttack();
        yield return enemy.EnemyAnimator.WaitForAttackAnimation(_projectileSpawnDelayAfterAnimation);
    }

    private void ShootProjectile()
    {
        if (_projectilePrefab == null || playerTransform == null)
            return;

        Vector3 spawnPosition = _firePoint != null ? _firePoint.position : enemy.transform.position;
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)spawnPosition).normalized;

        GameObject projectileObject = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();

        if (projectile == null)
            projectile = projectileObject.AddComponent<EnemyProjectile>();

        projectile.Initialize(direction, _projectileSpeed, _damage, _projectileLifetime, enemy.gameObject);
    }

    private void StopEnemyCompletely()
    {
        _navMeshAgent2D?.Stop();

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }

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
