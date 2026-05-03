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

        if (Agent != null)
        {
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
            
            // For 2D games, baseOffset is often too high (defaults for 3D meshes).
            // Set it to half of the radius (typical 2D sprite height).
            if (Agent.baseOffset > Agent.radius)
            {
                Debug.LogWarning($"[EnemyNavMeshAgent2D] {gameObject.name}: Adjusting baseOffset from {Agent.baseOffset} to {Agent.radius} for 2D");
                Agent.baseOffset = Agent.radius;
            }
            
            // AutoRepath can cause issues - disable it for more predictable behavior
            Agent.autoRepath = false;
        }

        Debug.Log($"[EnemyNavMeshAgent2D] {gameObject.name}: Awake - Agent: {(Agent != null ? "OK" : "NULL")}, Rigidbody2D: {(_rb != null ? "OK" : "NULL")}");

        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.isKinematic = true; // Make kinematic to allow NavMeshAgent to control transform without physics interference
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void Start()
    {
        TrySnapToNavMesh();
    }

    public bool MoveTo(Vector3 destination)
    {
        Debug.LogWarning($"[EnemyNavMeshAgent2D] MoveTo called: dest={destination}, agent={(Agent != null ? "exists" : "NULL")}, enabled={(Agent != null ? Agent.enabled.ToString() : "N/A")}, onNavMesh={(Agent != null ? Agent.isOnNavMesh.ToString() : "N/A")}");
        
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
        
        // Check if the destination is effectively where we already are
        if (Vector3.Distance(transform.position, destination) < _destinationUpdateDistance)
        {
            Log($"Destination is too close to current position: {destination}");
            Agent.isStopped = true;
            Agent.ResetPath();
            return true; // Consider it a success since we're already there
        }

        // Calculate path first to verify it's at least partially valid (not invalid)
        NavMeshPath path = new NavMeshPath();
        if (Agent.CalculatePath(destination, path))
        {
            if (path.status == NavMeshPathStatus.PathInvalid)
            {
                Log($"Path is invalid: {path.status}");
                return false;
            }
            // Accept PathComplete and PathPartial
        }
        else
        {
            Log($"Cannot calculate path to destination: {destination}");
            return false;
        }

        _lastDestination = destination;
        _hasLastDestination = true;

        Agent.isStopped = false;
        bool result = Agent.SetDestination(destination);
        
        // On 2D, SetDestination can return true even if the destination is not reachable.
        // We need to check the resulting path status.
        if (result && Agent.pathPending && !Agent.hasPath)
        {
            Debug.LogWarning($"[EnemyNavMeshAgent2D] SetDestination({destination}): result={result}, agent.speed={Agent.speed}, pathPending={Agent.pathPending}, hasPath={Agent.hasPath}, remainingDistance={Agent.remainingDistance} (WARNING: path may not be valid)");
        }
        else
        {
            Debug.LogWarning($"[EnemyNavMeshAgent2D] SetDestination({destination}): result={result}, agent.speed={Agent.speed}, pathPending={Agent.pathPending}, hasPath={Agent.hasPath}, remainingDistance={Agent.remainingDistance}");
        }
        
        if (!result)
            Log($"SetDestination failed: {destination}");
        else if (!Agent.hasPath)
        {
            Log($"SetDestination returned true but agent has no path");
            result = false;
        }
        else
            Debug.LogWarning($"[EnemyNavMeshAgent2D] Successfully set destination. Path status: {Agent.pathStatus}");
        
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

        // On 2D, remainingDistance can be 0 even when not at destination.
        // Fall back to distance check if remainingDistance seems incorrect.
        float distanceToTarget = Vector3.Distance(transform.position, _lastDestination);
        
        // Use the smaller of remainingDistance and actual distance for robustness
        float effectiveDistance = Mathf.Min(Agent.remainingDistance, distanceToTarget);
        
        return effectiveDistance <= Agent.stoppingDistance + extraDistance;
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
