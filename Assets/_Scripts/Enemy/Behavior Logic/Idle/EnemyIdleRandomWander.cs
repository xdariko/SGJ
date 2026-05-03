using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Idle-NavMesh Random Wander", menuName = "Enemy Logic/Idle Logic/NavMesh Random Wander")]
public class EnemyIdleRandomWander : EnemyIdleSOBase
{
    [Header("Wander")]
    [SerializeField] private float _wanderRange = 5f;
    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private float _pointReachedDistance = 0.25f;
    [SerializeField] private float _repathDelay = 0.5f;
    [SerializeField] private int _sampleAttempts = 30;
    [SerializeField] private float _minPointDistance = 0.75f;
    [SerializeField] private float _sampleRadius = 2f;
    [SerializeField] private bool _debugLogs = true;

    private EnemyNavMeshAgent2D _navMeshAgent2D;
    private NavMeshAgent _agent;
    private float _nextRepathTime;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);

        _navMeshAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        _agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override void DoEnterLogic()
    {
        enemy.MoveEnemy(Vector2.zero);
        _nextRepathTime = 0f;

        if (_agent == null)
        {
            Log("No NavMeshAgent on enemy object.");
            return;
        }

        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _agent.speed = _movementSpeed;
        _agent.stoppingDistance = 0f;
        _agent.isStopped = false;

        if (!_agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit hit, _sampleRadius, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
                Log($"Enemy was not on NavMesh. Warped to nearest point: {hit.position}");
            }
            else
            {
                Log("Enemy is not on NavMesh and no NavMesh point was found nearby. Put enemy on blue area or increase Sample Radius.");
                return;
            }
        }

        SetNewRandomDestination();
    }

    public override void DoExitLogic()
    {
        StopAgent();
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        if (enemy.IsAggroed)
        {
            StopAgent();
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (enemy.IsNoiseHeard)
        {
            StopAgent();
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            enemy.IsNoiseHeard = false;
            return;
        }

        if (_agent == null || !_agent.enabled)
            return;

        if (!_agent.isOnNavMesh)
        {
            Log("Agent left NavMesh during Idle.");
            return;
        }

        if (_agent.pathPending)
            return;

        bool needsNewPoint = !_agent.hasPath || _agent.remainingDistance <= _pointReachedDistance;

        if (needsNewPoint && Time.time >= _nextRepathTime)
        {
            SetNewRandomDestination();
            _nextRepathTime = Time.time + _repathDelay;
        }

        Vector2 velocity = _agent.velocity;
        if (velocity.sqrMagnitude > 0.001f)
            enemy.CheckForLeftOrRightFacing(velocity);
    }

    private void SetNewRandomDestination()
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            return;

        // First check if we're already at a valid position on navmesh
        if (NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit currentHit, _sampleRadius, NavMesh.AllAreas))
        {
            // Valid position
        }
        else
        {
            Log("Enemy is not on NavMesh, cannot find wander destination.");
            return;
        }

        for (int i = 0; i < _sampleAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * _wanderRange;
            Vector3 randomPoint = enemy.transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, _sampleRadius, NavMesh.AllAreas))
                continue;

            float distance = Vector3.Distance(enemy.transform.position, hit.position);
            if (distance < _minPointDistance)
                continue;

            // Use the NavMeshAgent2D's MoveTo which has better validation
            bool destinationSet = false;
            if (_navMeshAgent2D != null)
                destinationSet = _navMeshAgent2D.MoveTo(hit.position);
            else
            {
                // Fallback to direct agent.SetDestination with pre-check
                NavMeshPath path = new NavMeshPath();
                if (_agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    _agent.isStopped = false;
                    destinationSet = _agent.SetDestination(hit.position);
                }
            }

            if (destinationSet)
            {
                Log($"New wander destination: {hit.position} (distance: {distance:F2})");
                return;
            }
        }

        Log("Failed to find a valid random wander destination. Check NavMesh size, Wander Range, Sample Radius, and agent Radius.");
    }

    private void StopAgent()
    {
        if (_navMeshAgent2D != null)
        {
            _navMeshAgent2D.Stop();
            return;
        }

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }

        enemy.MoveEnemy(Vector2.zero);
    }

    private void Log(string message)
    {
        if (_debugLogs)
            Debug.LogWarning($"[EnemyIdleRandomWander] {enemy.name}: {message}", enemy);
    }
}