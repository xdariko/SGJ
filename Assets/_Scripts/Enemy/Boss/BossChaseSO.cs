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

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        navAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        agent = gameObject.GetComponent<NavMeshAgent>();

        // Disable NavMeshAgent if present, we'll use Rigidbody2D directly
        if (agent != null)
        {
            agent.enabled = false;
        }

        Debug.Log($"[BossChaseSO] Initialized for {gameObject.name}. navAgent2D: {(navAgent2D != null ? "OK" : "NULL")}, agent disabled: {agent != null}");
    }

    public override void DoEnterLogic()
    {
        refreshTimer = 0f;
        enemy.MoveEnemy(Vector2.zero);
    }

    public override void DoExitLogic()
    {
        //navAgent2D?.Stop(); // not needed if using direct movement
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        // Do not call base.DoFrameUpdateLogic to avoid auto state changes
        if (enemy.PlayerTarget == null)
        {
            Debug.Log("[BossChaseSO] PlayerTarget is null, returning");
            return;
        }

        // If not aggroed, go to investigate
        if (!enemy.IsAggroed)
        {
            enemy.MoveEnemy(Vector2.zero);
            enemy.InvestigationTargetPosition = enemy.PlayerTarget.position;
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        // Direct movement towards player using Rigidbody2D
        Vector2 direction = ((Vector2)enemy.PlayerTarget.position - (Vector2)enemy.transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.PlayerTarget.position);

        if (distanceToPlayer > stoppingDistance)
        {
            enemy.MoveEnemy(direction * movementSpeed);
            Debug.Log($"[BossChaseSO] Moving towards player, dir={direction}, speed={movementSpeed}");
        }
        else
        {
            enemy.MoveEnemy(Vector2.zero);
        }

        // Face player based on movement direction
        if (direction.sqrMagnitude > 0.001f)
        {
            enemy.CheckForLeftOrRightFacing(direction);
        }
    }

    public override void DoPhysicsLogic()
    {
        // Not needed; movement handled in FrameUpdate via MoveEnemy
    }
}
