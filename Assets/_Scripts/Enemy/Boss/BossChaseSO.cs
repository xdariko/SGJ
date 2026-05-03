using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Boss Chase Direct", menuName = "Enemy Logic/Boss/Boss Chase Direct")]
public class BossChaseSO : EnemyChaseSOBase
{
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float destinationRefreshRate = 0.1f;

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
            Debug.LogError($"[BossChaseSO] Enemy is not a BossEnemy! {enemy?.name}");
            return;
        }

        navAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        agent = gameObject.GetComponent<NavMeshAgent>();

        Debug.LogWarning($"[BossChaseSO] Initialize for {gameObject.name}");
        Debug.LogWarning($"  enemy.PlayerTarget: {(enemy.PlayerTarget != null ? enemy.PlayerTarget.name : "NULL")}");
        Debug.LogWarning($"  navAgent2D: {(navAgent2D != null ? "OK" : "NULL")}");
        Debug.LogWarning($"  NavMeshAgent: {(agent != null ? "OK" : "NULL")}, enabled: {(agent != null ? agent.enabled.ToString() : "N/A")}, isOnNavMesh: {(agent != null ? agent.isOnNavMesh.ToString() : "N/A")}");

        if (agent != null)
        {
            agent.enabled = true;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = movementSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
            Debug.LogWarning($"[BossChaseSO] Agent configured: speed={movementSpeed}, stoppingDistance={stoppingDistance}");
        }
        else
        {
            Debug.LogError("[BossChaseSO] NavMeshAgent component is MISSING on boss!");
        }
    }

    public override void DoEnterLogic()
    {
        Debug.LogWarning("[BossChaseSO] DoEnterLogic called");
        refreshTimer = 0f;

        enemy.MoveEnemy(Vector2.zero);

        if (agent != null)
        {
            agent.speed = movementSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
            Debug.LogWarning($"[BossChaseSO] DoEnterLogic: agent.speed set to {movementSpeed}, stoppingDistance={stoppingDistance}, isStopped={agent.isStopped}");
        }
        else
        {
            Debug.LogError("[BossChaseSO] DoEnterLogic: agent is NULL!");
        }
    }

    public override void DoExitLogic()
    {
        navAgent2D?.Stop();
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        Debug.LogWarning($"[BossChaseSO] DoFrameUpdateLogic: Aggro={enemy.IsAggroed}, Striking={enemy.IsWithinStrikingDistance}, PlayerTargetExists={enemy.PlayerTarget != null}");
        
        if (enemy.PlayerTarget == null)
        {
            Debug.LogWarning("[BossChaseSO] PlayerTarget is null.");
            return;
        }

        if (enemy.IsWithinStrikingDistance)
        {
            Debug.Log("[BossChaseSO] Within striking distance, stopping and switching to Attack");
            navAgent2D?.Stop();
            enemy.MoveEnemy(Vector2.zero);
            
            // Select an attack before switching state
            if (_boss.AttackController != null)
            {
                // Find first usable non-special attack (same logic as UpdateAbilities)
                for (int i = 0; i < _boss.AttackController.abilities.Count; i++)
                {
                    var entry = _boss.AttackController.abilities[i];
                    if (!_boss.AttackController.IsSpecialAbility(entry.Type))
                    {
                        _boss.AttackController.SetCurrentAttackIndex(i);
                        Debug.LogWarning($"[BossChaseSO] Selected attack index {i} ({entry.Type}) for AttackState");
                        break;
                    }
                }
            }
            
            enemy.StateMachine.ChangeState(enemy.AttackState);
            return;
        }

        if (!enemy.IsAggroed)
        {
            Debug.Log("[BossChaseSO] Not aggroed, stopping and switching to Investigate");
            navAgent2D?.Stop();
            enemy.MoveEnemy(Vector2.zero);
            enemy.InvestigationTargetPosition = enemy.PlayerTarget.position;
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        if (agent == null)
        {
            Debug.LogError("[BossChaseSO] NavMeshAgent is NULL! Add NavMeshAgent component.");
            return;
        }
        if (!agent.enabled)
        {
            Debug.LogError("[BossChaseSO] NavMeshAgent is disabled! Enable it.");
            return;
        }
        if (!agent.isOnNavMesh)
        {
            Debug.LogError("[BossChaseSO] Agent is NOT on NavMesh! Bake NavMesh or move boss onto it.");
            return;
        }

        agent.isStopped = false;

        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = destinationRefreshRate;
            Vector3 targetPos = enemy.PlayerTarget.position;
            bool moved = navAgent2D?.MoveTo(targetPos) ?? false;
            Debug.LogWarning($"[BossChaseSO] MoveTo({targetPos}): result={moved}, agent.speed={agent.speed}, agent.remainingDistance={agent.remainingDistance}, agent.stoppingDistance={agent.stoppingDistance}");
        }

        Vector2 velocity = agent.velocity;
        if (velocity.sqrMagnitude > 0.001f)
        {
            enemy.CheckForLeftOrRightFacing(velocity);
            Debug.Log($"[BossChaseSO] Update velocity: {velocity}");
        }
    }

    public override void DoPhysicsLogic()
    {
    }
}