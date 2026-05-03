using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Boss Chase Direct", menuName = "Enemy Logic/Boss/Boss Chase Direct")]
public class BossChaseSO : EnemyChaseSOBase
{
    public enum ChaseStyle
    {
        AlwaysApproach,      // Tank: всегда сближается с игроком
        KeepDistance,        // Agile/Summoner: держит дистанцию, не подходит слишком близко
        Mixed                // Смешанный: приближается, затем отступает
    }

    [Header("Profile")]
    [SerializeField] private BossProfileSO profile; // Настройки для этого босса
    [SerializeField] private ChaseStyle chaseStyle = ChaseStyle.AlwaysApproach;
    
    [Header("Distance Settings (for KeepDistance)")]
    [SerializeField] private float preferredDistanceMin = 2f;
    [SerializeField] private float preferredDistanceMax = 4f;
    
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
            snapToNavMeshRadius = 2f;
            
            // Auto-set chase style based on profile parameters if not manually set
            if (chaseStyle == ChaseStyle.AlwaysApproach) // Only auto-set if not explicitly configured
            {
                if (profile.minApproachDistance < 1f)
                {
                    chaseStyle = ChaseStyle.AlwaysApproach; // Tank
                }
                else if (profile.stoppingDistance > 2f)
                {
                    chaseStyle = ChaseStyle.KeepDistance; // Ranged/Summoner
                }
            }

            Debug.LogWarning($"[BossChaseSO] Using profile: {profile.name}, Style: {chaseStyle}");
        }
        else
        {
            Debug.LogWarning($"[BossChaseSO] No profile assigned, using default values");
        }

        navAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        agent = gameObject.GetComponent<NavMeshAgent>();

        Debug.LogWarning($"[BossChaseSO] Initialize for {gameObject.name}");
        Debug.LogWarning($"  Settings: speed={movementSpeed}, refreshRate={destinationRefreshRate}, teleportDist={teleportDistance}, style={chaseStyle}");

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

        // First teleport towards/away from player
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

        float targetDistance = currentDistance;
        float minApproach = profile != null ? profile.minApproachDistance : 0.8f;
        float maxDistance = profile != null ? profile.stoppingDistance : 1f;

        // Determine desired distance based on chase style
        switch (chaseStyle)
        {
            case ChaseStyle.AlwaysApproach:
                // Always get closer until within striking distance
                targetDistance = Mathf.Max(minApproach, currentDistance - teleportDistance);
                break;

            case ChaseStyle.KeepDistance:
                // Maintain distance between minApproach and stoppingDistance (from profile)
                if (currentDistance > maxDistance)
                {
                    // Too far - approach
                    targetDistance = Mathf.Max(minApproach, currentDistance - teleportDistance);
                }
                else if (currentDistance < minApproach)
                {
                    // Too close - retreat!
                    targetDistance = currentDistance + teleportDistance;
                }
                else
                {
                    // In preferred range - don't move
                    Debug.LogWarning($"[BossChaseSO] In preferred range ({currentDistance:F2}m), not teleporting");
                    return;
                }
                break;

            case ChaseStyle.Mixed:
                // Approach until minApproach, then retreat
                if (currentDistance > minApproach + 1f)
                {
                    targetDistance = Mathf.Max(minApproach, currentDistance - teleportDistance);
                }
                else
                {
                    targetDistance = currentDistance + teleportDistance;
                }
                break;
        }

        // Don't teleport if already at ideal distance
        if (Mathf.Abs(currentDistance - targetDistance) < 0.3f)
        {
            return;
        }

        Vector3 directionToPlayer = (playerPos - bossPos).normalized;
        Vector3 targetPos = playerPos - directionToPlayer * targetDistance;

        // Snap to NavMesh
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, snapToNavMeshRadius, NavMesh.AllAreas))
        {
            Vector3 finalPos = hit.position;
            finalPos.z = bossPos.z;

            // Teleport (instant move)
            _boss.transform.position = finalPos;
            Debug.LogWarning($"[BossChaseSO] Teleported from {bossPos:F2} to {finalPos:F2} (dist to player: {Vector3.Distance(finalPos, playerPos):F2}m, style={chaseStyle})");

            // Face direction based on movement
            Vector2 faceDir = (Vector2)(playerPos - finalPos).normalized;
            if (chaseStyle == ChaseStyle.KeepDistance && currentDistance < minApproach)
            {
                // Retreat - face away from player
                faceDir = -faceDir;
            }
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
            Debug.Log("[BossChaseSO] Within striking distance, switching to Attack");
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
