using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Chase-Direct To Player", menuName = "Enemy Logic/Chase Logic/Direct To Player")]
public class EnemyChaseDirectToPlayer : EnemyChaseSOBase
{
    [SerializeField] private float _movementSpeed = 1.75f;
    [SerializeField] private float _stoppingDistance = 0.1f;
    [SerializeField] private float _destinationRefreshRate = 0.1f;

    private EnemyNavMeshAgent2D _navMeshAgent2D;
    private NavMeshAgent _agent;
    private float _refreshTimer;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _navMeshAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        _agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override void DoEnterLogic()
    {
        // Не вызываем base.DoEnterLogic(): в базовом методе сейчас пусто.
        _refreshTimer = 0f;
        enemy.MoveEnemy(Vector2.zero);

        if (_agent != null)
        {
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _agent.speed = _movementSpeed;
            _agent.stoppingDistance = _stoppingDistance;
            _agent.isStopped = false;
        }
    }

    public override void DoExitLogic()
    {
        _navMeshAgent2D?.Stop();
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        Debug.Log($"[EnemyChaseDirectToPlayer] FrameUpdate: Aggroed={enemy.IsAggroed}, Striking={enemy.IsWithinStrikingDistance}, HasNavAgent={_navMeshAgent2D != null}");
        
        if (playerTransform == null)
        {
            Debug.LogWarning("[EnemyChaseDirectToPlayer] playerTransform is NULL!");
            return;
        }

        if (enemy.IsWithinStrikingDistance)
        {
            Debug.Log("[EnemyChaseDirectToPlayer] Within striking distance - stopping and switching to Attack");
            _navMeshAgent2D?.Stop();
            enemy.MoveEnemy(Vector2.zero);
            enemy.StateMachine.ChangeState(enemy.AttackState);
            return;
        }

        if (!enemy.IsAggroed)
        {
            Debug.Log("[EnemyChaseDirectToPlayer] Not aggroed - switching to Investigate");
            _navMeshAgent2D?.Stop();
            enemy.MoveEnemy(Vector2.zero);
            enemy.InvestigationTargetPosition = playerTransform.position;
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        {
            Debug.LogError($"[EnemyChaseDirectToPlayer] NavMeshAgent problems: agent={( _agent != null ? "exists" : "NULL")}, enabled={_agent?.enabled}, onNavMesh={_agent?.isOnNavMesh}");
            return;
        }

        _agent.isStopped = false;
        bool moved = _navMeshAgent2D?.MoveTo(playerTransform.position) ?? false;
        Debug.Log($"[EnemyChaseDirectToPlayer] MoveTo result: {moved}, agent speed={_agent.speed}, dest={playerTransform.position}");

        FacePlayerByAgentVelocity();
    }

    private void FacePlayerByAgentVelocity()
    {
        if (_agent == null)
            return;

        Vector2 velocity = _agent.velocity;
        if (velocity.sqrMagnitude > 0.001f)
            enemy.CheckForLeftOrRightFacing(velocity);
    }
}
