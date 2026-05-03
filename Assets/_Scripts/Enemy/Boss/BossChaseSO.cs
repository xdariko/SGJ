using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Boss Chase Direct", menuName = "Enemy Logic/Boss/Boss Chase Direct")]
public class BossChaseSO : EnemyChaseSOBase
{
    [Header("Profile")]
    [SerializeField] private BossProfileSO profile; // Настройки для этого босса
    
    [Header("Runtime (Auto-configured from profile)")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float destinationRefreshRate = 0.5f;
    [SerializeField] private float teleportDistance = 1.5f;
    [SerializeField] private float snapToNavMeshRadius = 2f;

    private EnemyNavMeshAgent2D navAgent2D;
    private NavMeshAgent agent;
    private float refreshTimer;
    private BossEnemy _boss;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);

        _boss = enemy as BossEnemy;
        if (_boss == null)
        {
            Debug.LogError("[BossChaseSO] Enemy is not a BossEnemy!");
            return;
        }

        // Load settings from profile or use defaults
        if (profile != null)
        {
            movementSpeed = profile.chaseSpeed;
            destinationRefreshRate = profile.chaseUpdateRate;
            teleportDistance = profile.teleportStepDistance;
            snapToNavMeshRadius = 2f; // Could also be in profile
            Debug.LogWarning($"[BossChaseSO] Using profile: {profile.name}");
        }
        else
        {
            Debug.LogWarning($"[BossChaseSO] No profile assigned, using default values");
        }

        navAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        agent = gameObject.GetComponent<NavMeshAgent>();

        Debug.LogWarning($"[BossChaseSO] Initialize for {gameObject.name}");
        Debug.LogWarning($"  Settings: speed={movementSpeed}, refreshRate={destinationRefreshRate}, teleportDist={teleportDistance}");
        Debug.LogWarning($"  navAgent2D: {(navAgent2D != null ? "OK" : "NULL")}, agent: {(agent != null ? "OK" : "NULL")}");

        if (agent != null)
        {
            agent.enabled = true;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = movementSpeed;
            agent.isStopped = true; // Using teleport movement, not agent
        }
    }

    public override void DoEnterLogic()
    {
        Debug.LogWarning("[BossChaseSO] DoEnterLogic - Starting teleport chase");
        refreshTimer = 0f;
        enemy.MoveEnemy(Vector2.zero);

        if (agent != null)
        {
            agent.isStopped = true;
        }

        // First teleport towards player
        if (enemy.PlayerTarget != null)
        {
            PerformTeleportStep();
        }
    }

    private void PerformTeleportStep()
    {
        if (enemy.PlayerTarget == null) return;

        Vector3 bossPos = _boss.transform.position;
        Vector3 playerPos = enemy.PlayerTarget.position;
        float currentDistance = Vector3.Distance(bossPos, playerPos);

        // Already close enough?
        float minApproach = profile != null ? profile.minApproachDistance : 0.8f;
        if (currentDistance <= minApproach)
        {
            Debug.LogWarning($"[BossChaseSO] Within approach distance ({currentDistance:F2}m), not teleporting");
            return;
        }

        Vector3 directionToPlayer = (playerPos - bossPos).normalized;
        
        // Determine target: maintain some distance from player
        float targetDistance = Mathf.Max(
            profile != null ? profile.minApproachDistance : 0.8f, 
            currentDistance - teleportDistance
        );
        Vector3 targetPos = playerPos - directionToPlayer * targetDistance;

        // Snap to NavMesh
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, snapToNavMeshRadius, NavMesh.AllAreas))
        {
            Vector3 finalPos = hit.position;
            finalPos.z = bossPos.z;

            // Teleport (instant move)
            _boss.transform.position = finalPos;
            Debug.LogWarning($"[BossChaseSO] Teleported to {finalPos} (dist to player: {Vector3.Distance(finalPos, playerPos):F2}m)");

            // Face player
            Vector2 faceDir = (Vector2)(playerPos - finalPos).normalized;
            if (faceDir.sqrMagnitude > 0.01f)
            {
                _boss.CheckForLeftOrRightFacing(faceDir);
            }

            // Set velocity for animation
            enemy.MoveEnemy(directionToPlayer * movementSpeed);
        }
        else
        {
            Debug.LogWarning($"[BossChaseSO] Teleport target {targetPos} not on NavMesh");
        }
    }

    public override void DoFrameUpdateLogic()
    {
        if (enemy.PlayerTarget == null)
        {
            Debug.LogWarning("[BossChaseSO] PlayerTarget is null.");
            return;
        }

        if (enemy.IsWithinStrikingDistance)
        {
            Debug.Log("[BossChaseSO] Within striking distance, stopping and switching to Attack");
            enemy.MoveEnemy(Vector2.zero);
            
            // Select attack
            if (_boss.AttackController != null)
            {
                for (int i = 0; i < _boss.AttackController.abilities.Count; i++)
                {
                    var entry = _boss.AttackController.abilities[i];
                    if (!_boss.AttackController.IsSpecialAbility(entry.Type))
                    {
                        _boss.AttackController.SetCurrentAttackIndex(i);
                        Debug.LogWarning($"[BossChaseSO] Selected attack index {i} ({entry.Type})");
                        break;
                    }
                }
            }
            
            enemy.StateMachine.ChangeState(enemy.AttackState);
            return;
        }

        if (!enemy.IsAggroed)
        {
            Debug.Log("[BossChaseSO] Not aggroed, switching to Investigate");
            enemy.MoveEnemy(Vector2.zero);
            enemy.InvestigationTargetPosition = enemy.PlayerTarget.position;
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        // Teleport at intervals
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = destinationRefreshRate;
            PerformTeleportStep();
        }
    }

    public override void DoExitLogic()
    {
        navAgent2D?.Stop();
        base.DoExitLogic();
    }

    public override void DoPhysicsLogic()
    {
        // Nothing needed
    }
}
