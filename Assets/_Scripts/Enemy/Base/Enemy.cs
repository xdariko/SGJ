using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IEnemyMoveable, ITriggerCheckable
{
    public Rigidbody2D RB { get; set; }
    public bool IsFacingRight { get; set; } = true;

    public bool IsAggroed { get; set; }
    public bool IsWithinStrikingDistance { get; set; }
    public bool IsNoiseHeard { get; set; }

    public Transform PlayerTarget { get; private set; }
    public Vector3 InvestigationTargetPosition { get; set; }
    public float InvestigationDuration { get; set; } = 3f;

    protected virtual void Awake()
    {
        EnemyIdleBaseInstance = Instantiate(EnemyIdleBase);
        EnemyChaseBaseInstance = Instantiate(EnemyChaseBase);
        EnemyInvestigateBaseInstance = Instantiate(EnemyInvestigateBase);
        EnemyAttackBaseInstance = Instantiate(EnemyAttackBase);
        
        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
        InvestigateState = new EnemyInvestigateState(this, StateMachine);
        // Ensure enemy has a body collider for hit detection
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null)
                col.radius = agent.radius;
            else
                col.radius = 0.5f;
        }
    }

    protected virtual void Start()
    {
        RB = GetComponent<Rigidbody2D>();

        EnemyIdleBaseInstance.Initialize(gameObject, this);
        EnemyChaseBaseInstance.Initialize(gameObject, this);
        EnemyInvestigateBaseInstance.Initialize(gameObject, this);
        EnemyAttackBaseInstance.Initialize(gameObject, this);

        StateMachine.Initialize(IdleState);
    }

    protected virtual void Update()
    {
        // Keep PlayerTarget up to date (find if null or destroyed)
        if (PlayerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                PlayerTarget = playerObj.transform;
        }

        StateMachine.CurrentEnemyState?.FrameUpdate();
    }

    protected virtual void FixedUpdate()
    {
        StateMachine.CurrentEnemyState?.PhysicsUpdate();
    }

    #region SO Variables

    [SerializeField] private EnemyIdleSOBase EnemyIdleBase;
    [SerializeField] private EnemyChaseSOBase EnemyChaseBase;
    [SerializeField] private EnemyInvestigateSOBase EnemyInvestigateBase;
    [SerializeField] private EnemyAttackSOBase EnemyAttackBase;

    public EnemyIdleSOBase EnemyIdleBaseInstance { get; set; }
    public EnemyChaseSOBase EnemyChaseBaseInstance { get; set; }
    public EnemyInvestigateSOBase EnemyInvestigateBaseInstance { get; set; }
    public EnemyAttackSOBase EnemyAttackBaseInstance { get; set; }

    #endregion

    #region State Machine Variables

    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public EnemyAttackState AttackState { get; set; }
    public EnemyInvestigateState InvestigateState { get; set; }

    #endregion

    #region Movement Functions

    public void MoveEnemy(Vector2 velocity)
    {
        RB.linearVelocity = velocity;
        CheckForLeftOrRightFacing(velocity);
    }

    public void CheckForLeftOrRightFacing(Vector2 velocity)
    {
        if (IsFacingRight && velocity.x < 0f) 
        { 
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
        }

        if (!IsFacingRight && velocity.x > 0f)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
        }
    }

    #endregion

    #region Distance Checks

    public void SetAggroStatus(bool isAggroed)
    {
        IsAggroed = isAggroed;
        //Debug.Log("SetAggroStatus " + isAggroed);
    }

    public void SetStrikingDistance(bool isWithinStrikingDistance)
    {
        IsWithinStrikingDistance = isWithinStrikingDistance;
        //Debug.Log("SetStrikingDistance " + isWithinStrikingDistance);
    }

    public void SetNoiseHeard(bool isNoiseHeard, Vector3 noisePosition = default)
    {
        IsNoiseHeard = isNoiseHeard;
        if (isNoiseHeard)
        {
            InvestigationTargetPosition = noisePosition;
        }
    }

    #endregion

    #region Animation Triggers
    public void TriggerAnimationEvent(AnimationTriggerType triggerType)
    {
        StateMachine.CurrentEnemyState.AnimationTriggerEvent(triggerType);
    }

    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootstepSound
    }

    #endregion
}
