using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavMeshAgent2D : MonoBehaviour
{
    [SerializeField] private float _destinationUpdateDistance = 0.15f;
    [SerializeField] private float _verticalPathDrift = 0.0001f;
    [SerializeField] private float _snapToNavMeshRadius = 2f;
    [SerializeField] private bool _debugLogs = true;

    public NavMeshAgent Agent { get; private set; }

    private Rigidbody2D _rb;
    private Vector3 _lastDestination;
    private bool _hasLastDestination;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody2D>();

        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        Debug.Log($"[EnemyNavMeshAgent2D] {gameObject.name}: Awake - Agent: {(Agent != null ? "OK" : "NULL")}, Rigidbody2D: {(_rb != null ? "OK" : "NULL")}");

        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void Start()
    {
        TrySnapToNavMesh();
    }

    public bool MoveTo(Vector3 destination)
    {
        if (Agent == null || !Agent.enabled)
        {
            Log("NavMeshAgent is missing or disabled.");
            return false;
        }

        if (!Agent.isOnNavMesh && !TrySnapToNavMesh())
        {
            Log("Agent is not on NavMesh. Put enemy on the blue NavMesh area or rebake NavMesh.");
            return false;
        }

        destination.z = transform.position.z;

        if (Mathf.Abs(transform.position.x - destination.x) < _verticalPathDrift)
            destination.x += _verticalPathDrift;

        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, _snapToNavMeshRadius, NavMesh.AllAreas))
        {
            Log($"Destination is not near NavMesh: {destination}");
            return false;
        }

        destination = hit.position;

        bool sameDestination = _hasLastDestination && Vector3.Distance(_lastDestination, destination) < _destinationUpdateDistance;
        bool alreadyMoving = !Agent.isStopped && Agent.hasPath && !Agent.pathPending;

        if (sameDestination && alreadyMoving)
            return true;

        _lastDestination = destination;
        _hasLastDestination = true;

        Agent.isStopped = false;
        bool result = Agent.SetDestination(destination);

        if (!result)
            Log($"SetDestination failed: {destination}");

        return result;
    }

    public void Stop()
    {
        _hasLastDestination = false;

        if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
            Agent.velocity = Vector3.zero;
        }

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }

    public bool HasReachedDestination(float extraDistance = 0f)
    {
        if (Agent == null || !Agent.enabled || !Agent.isOnNavMesh)
            return true;

        if (Agent.pathPending)
            return false;

        if (!Agent.hasPath)
            return true;

        return Agent.remainingDistance <= Agent.stoppingDistance + extraDistance;
    }

    private bool TrySnapToNavMesh()
    {
        if (Agent == null || !Agent.enabled)
            return false;

        if (Agent.isOnNavMesh)
            return true;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, _snapToNavMeshRadius, NavMesh.AllAreas))
        {
            bool warped = Agent.Warp(hit.position);
            if (warped)
                Log($"Snapped enemy to NavMesh: {hit.position}");

            return warped;
        }

        return false;
    }

    private void Log(string message)
    {
        if (_debugLogs)
            Debug.LogWarning($"[EnemyNavMeshAgent2D] {name}: {message}", this);
    }
}
