using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BossAttack", menuName = "Enemy Logic/Boss/Boss Attack")]
public class BossAttackSO : EnemyAttackSOBase
{
    private BossEnemy _boss;
    private Enemy _enemy;
    private Coroutine _attackRoutine;
    private int _currentAttackIndex;
    private BossAttackControllerSO _controller;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _enemy = enemy;
        _boss = enemy as BossEnemy;
        Debug.LogWarning($"[BossAttackSO] Initialize: boss={_boss?.name}, enemy={_enemy?.name}");
    }

    public override void DoEnterLogic()
    {
        Debug.LogWarning($"[BossAttackSO] DoEnterLogic on {_enemy?.gameObject.name}");
        
        if (_boss == null)
        {
            Debug.LogError("[BossAttackSO] Boss is NULL! Enemy component reference lost.");
            return;
        }

        var nav = _boss.GetComponent<EnemyNavMeshAgent2D>();
        Debug.LogWarning($"[BossAttackSO] NavMeshAgent2D: {(nav != null ? "OK" : "NULL")}");
        nav?.Stop();
        _boss.MoveEnemy(Vector2.zero);

        var animator = _boss.EnemyAnimator;
        Debug.LogWarning($"[BossAttackSO] EnemyAnimator: {(animator != null ? "OK" : "NULL")}");
        animator?.PlayState(EnemyAnimState.Attack);

        _controller = _boss.AttackController;
        if (_controller == null)
        {
            Debug.LogError("[BossAttackSO] AttackController is NULL! Assign it in BossEnemy inspector.");
            return;
        }

        _currentAttackIndex = _controller.GetCurrentAttackIndex();
        Debug.LogWarning($"[BossAttackSO] CurrentAttackIndex: {_currentAttackIndex}");
        
        if (_currentAttackIndex < 0)
        {
            Debug.LogError("[BossAttackSO] Invalid attack index! Attempting to select fallback...");
            
            // Fallback: find first usable non-special attack
            for (int i = 0; i < _controller.abilities.Count; i++)
            {
                var entry = _controller.abilities[i];
                if (!_controller.IsSpecialAbility(entry.Type))
                {
                    _controller.SetCurrentAttackIndex(i);
                    _currentAttackIndex = i;
                    Debug.LogWarning($"[BossAttackSO] Fallback selected attack index {i} ({entry.Type})");
                    break;
                }
            }
            
            if (_currentAttackIndex < 0)
            {
                Debug.LogError("[BossAttackSO] No valid attack index found and no fallback available! Check AttackController abilities list.");
                return;
            }
        }

        _attackRoutine = _boss.StartCoroutine(ExecuteAttackRoutine());
        Debug.LogWarning($"[BossAttackSO] Attack routine started for ability index {_currentAttackIndex}");
    }

    public override void DoExitLogic()
    {
        if (_attackRoutine != null)
        {
            _boss.StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
        base.DoExitLogic();
    }

    private IEnumerator ExecuteAttackRoutine()
    {
        BossAttackControllerSO.BossAbilityEntry entry = _controller.GetCurrentAttackEntry();
        if (entry == null) 
        {
            Debug.LogError("[BossAttackSO] Entry is null!");
            yield break;
        }

        Debug.LogWarning($"[BossAttackSO] Executing attack {_currentAttackIndex}: Type={entry.Type}");

        switch (entry.Type)
        {
            case BossAttackControllerSO.AbilityType.MeleeSlam:
                yield return ExecuteMeleeSlam(entry.meleeSlam);
                break;
            case BossAttackControllerSO.AbilityType.Ranged:
                yield return ExecuteRanged(entry.ranged);
                break;
            case BossAttackControllerSO.AbilityType.QuickShot:
                yield return ExecuteQuickShot(entry.quickShot);
                break;
            default:
                Debug.LogError($"[BossAttackSO] Unsupported attack type: {entry.Type}");
                yield break;
        }

        // Return to chase after attack
        Debug.LogWarning("[BossAttackSO] Attack complete, returning to ChaseState");
        
        // Safety checks
        if (_enemy == null)
        {
            Debug.LogError("[BossAttackSO] _enemy is null!");
            yield break;
        }
        if (_enemy.StateMachine == null)
        {
            Debug.LogError("[BossAttackSO] StateMachine is null!");
            yield break;
        }
        if (_enemy.ChaseState == null)
        {
            Debug.LogError("[BossAttackSO] ChaseState is null! Initialize it in Awake.");
            yield break;
        }

        _enemy.StateMachine.ChangeState(_enemy.ChaseState);
    }

    private IEnumerator ExecuteMeleeSlam(BossAttackControllerSO.MeleeSlamParams p)
    {
        if (_boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            _boss.Animator.SetTrigger(p.animationTrigger);

        yield return new WaitForSeconds(p.windup);

        // Find all colliders in radius, then filter manually
        Collider2D[] allHit = Physics2D.OverlapCircleAll((Vector2)_boss.transform.position, p.radius);
        Debug.LogWarning($"[BossAttackSO] MeleeSlam: radius={p.radius}, bossPos={_boss.transform.position}, allHits={allHit.Length}");
        
        int dealtDamageCount = 0;
        foreach (Collider2D col in allHit)
        {
            // Skip triggers (AggroRadius, StrikingDistance)
            if (col.isTrigger)
            {
                Debug.LogWarning($"[BossAttackSO] MeleeSlam skipped trigger: {col.gameObject.name}");
                continue;
            }
            
            // Skip the boss itself and its children
            if (col.transform.root == _boss.transform.root)
            {
                Debug.LogWarning($"[BossAttackSO] MeleeSlam skipped self/child: {col.gameObject.name}");
                continue;
            }
            
            // Only damage objects with IDamageable interface
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.LogWarning($"[BossAttackSO] >>> DEALING {p.damage} DAMAGE to {col.gameObject.name} (tag={col.tag})");
                damageable.TakeDamage(p.damage);
                dealtDamageCount++;
            }
            else
            {
                Debug.LogWarning($"[BossAttackSO] MeleeSlam: {col.gameObject.name} has no IDamageable (tag={col.tag})");
            }
        }
        
        Debug.LogWarning($"[BossAttackSO] MeleeSlam complete: totalHits={allHit.Length}, dealtDamageTo={dealtDamageCount}");
        yield return new WaitForSeconds(p.attackDuration);
    }

    private IEnumerator ExecuteRanged(BossAttackControllerSO.RangedParams p)
    {
        if (_boss.PlayerTarget == null) 
        {
            Debug.LogWarning("[BossAttackSO] Ranged: PlayerTarget is null!");
            yield break;
        }

        if (_boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            _boss.Animator.SetTrigger(p.animationTrigger);

        int burstCount = p.burstCount;
        float interval = 1f / p.fireRate;

        for (int i = 0; i < burstCount; i++)
        {
            if (_boss.PlayerTarget == null) 
            {
                Debug.LogWarning("[BossAttackSO] Ranged: PlayerTarget lost during burst");
                yield break;
            }
            FireProjectile(p);
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator ExecuteQuickShot(BossAttackControllerSO.QuickShotParams p)
    {
        if (_boss.PlayerTarget == null) 
        {
            Debug.LogWarning("[BossAttackSO] QuickShot: PlayerTarget is null!");
            yield break;
        }

        if (_boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            _boss.Animator.SetTrigger(p.animationTrigger);

        Vector2 baseDir = ((Vector2)_boss.PlayerTarget.position - (Vector2)_boss.transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float halfSpread = p.spreadAngle / 2f;

        for (int i = 0; i < p.volleySize; i++)
        {
            if (_boss.PlayerTarget == null) 
            {
                Debug.LogWarning("[BossAttackSO] QuickShot: PlayerTarget lost during volley");
                yield break;
            }
            float angle = baseAngle + (i == 0 ? -halfSpread : (i == 1 ? 0f : halfSpread));
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            if (p.projectilePrefab != null)
            {
                GameObject proj = Object.Instantiate(p.projectilePrefab, _boss.transform.position, Quaternion.identity);
                EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
                if (ep == null) ep = proj.AddComponent<EnemyProjectile>();
                ep.Initialize(dir, p.projectileSpeed, p.damage, p.projectileLifetime, _boss.gameObject);
                Debug.LogWarning($"[BossAttackSO] QuickShot fired projectile {i+1}/{p.volleySize}");
            }
            else
            {
                Debug.LogError("[BossAttackSO] QuickShot: projectilePrefab is NULL!");
            }

            yield return new WaitForSeconds(p.volleyDelay);
        }
    }

    private void FireProjectile(BossAttackControllerSO.RangedParams p)
    {
        if (p.projectilePrefab == null) 
        {
            Debug.LogError("[BossAttackSO] Ranged: projectilePrefab is NULL!");
            return;
        }
        if (_boss.PlayerTarget == null) 
        {
            Debug.LogWarning("[BossAttackSO] Ranged: PlayerTarget is null");
            return;
        }

        Vector2 dir = ((Vector2)_boss.PlayerTarget.position - (Vector2)_boss.transform.position).normalized;
        GameObject proj = Object.Instantiate(p.projectilePrefab, _boss.transform.position, Quaternion.identity);
        
        // Set the projectile's owner to this boss so it doesn't damage the boss
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep == null) ep = proj.AddComponent<EnemyProjectile>();
        ep.Initialize(dir, p.projectileSpeed, p.damage, p.projectileLifetime, _boss.gameObject);
        
        Debug.LogWarning($"[BossAttackSO] Ranged fired projectile");
    }
}
