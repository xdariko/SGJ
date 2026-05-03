using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Attack-Explode Cloud", menuName = "Enemy Logic/Attack Logic/Explode Cloud")]
public class EnemyAttackExplodeCloud : EnemyAttackSOBase
{
    [Header("Explosion")]
    [FormerlySerializedAs("_windup")]
    [SerializeField] private float _attackAnimationFallbackDuration = 0.35f;
    [SerializeField] private float _explosionDamage = 20f;
    [SerializeField] private float _explosionRadius = 1.2f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Cloud")]
    [SerializeField] private GameObject _cloudPrefab;
    [SerializeField] private float _cloudDamagePerTick = 5f;
    [SerializeField] private float _cloudTickRate = 0.5f;
    [SerializeField] private float _cloudDuration = 3f;
    [SerializeField] private float _cloudScale = 1.5f;

    [Header("Death")]
    [SerializeField] private bool _destroyEnemyAfterExplosion = true;
    [SerializeField] private float _destroyDelay = 0.05f;

    private EnemyNavMeshAgent2D _navMeshAgent2D;
    private NavMeshAgent _agent;
    private Coroutine _attackRoutine;
    private bool _hasExploded;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _navMeshAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        _agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        _hasExploded = false;
        StopEnemyCompletely();
        _attackRoutine = enemy.StartCoroutine(ExplodeRoutine());
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
    }

    private IEnumerator ExplodeRoutine()
    {
        if (enemy.EnemyAnimator != null)
            yield return enemy.EnemyAnimator.PlayAttackAndWait(_attackAnimationFallbackDuration);
        else
            yield return new WaitForSeconds(_attackAnimationFallbackDuration);

        Explode();
    }

    private void Explode()
    {
        if (_hasExploded)
            return;

        _hasExploded = true;
        StopEnemyCompletely();

        Collider2D playerHit = Physics2D.OverlapCircle(enemy.transform.position, _explosionRadius, _playerLayer);
        if (playerHit != null)
        {
            Player player = playerHit.GetComponentInParent<Player>();
            if (player != null)
                player.TakeDamage(_explosionDamage, enemy.gameObject);
        }

        SpawnCloud();

        if (_destroyEnemyAfterExplosion)
            Destroy(enemy.gameObject, _destroyDelay);
        else
            enemy.StateMachine.ChangeState(enemy.IdleState);
    }

    private void SpawnCloud()
    {
        if (_cloudPrefab == null)
            return;

        GameObject cloudObject = Instantiate(_cloudPrefab, enemy.transform.position, Quaternion.identity);
        cloudObject.transform.localScale = Vector3.one * _cloudScale;

        EnemyDamageCloud cloud = cloudObject.GetComponent<EnemyDamageCloud>();
        if (cloud == null)
            cloud = cloudObject.AddComponent<EnemyDamageCloud>();

        cloud.Initialize(_cloudDamagePerTick, _cloudTickRate, _cloudDuration);
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
