using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 30f;
    [SerializeField] private bool _destroyOnDeath = true;
    [SerializeField] private float _destroyDelay = 0.1f;

    [Header("Damage Popup")]
    [SerializeField] private DamagePopup _damagePopupPrefab;
    [SerializeField] private Vector3 _damagePopupOffset = new Vector3(0f, 0.75f, 0f);
    [SerializeField] private float _damagePopupRandomX = 0.25f;

    private float _currentHealth;
    private bool _isDead;

    private HitEffect _hitEffect;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float HealthPercent => _maxHealth <= 0f ? 0f : _currentHealth / _maxHealth;
    public bool IsDead => _isDead;

    public event Action<float, float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _hitEffect = GetComponent<HitEffect>();
    }

    public void TakeDamage(float damage)
    {
        if (_isDead || damage <= 0f)
            return;

        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        float delta = _currentHealth - oldHealth;

        ShowDamagePopup(damage);

        if (_hitEffect != null)
            _hitEffect.ApplyHitEffect();

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, delta);

        if (_currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        float delta = _currentHealth - oldHealth;

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, delta);
    }

    private void ShowDamagePopup(float damage)
    {
        if (_damagePopupPrefab == null)
            return;

        Vector3 spawnPosition = transform.position + _damagePopupOffset;
        spawnPosition.x += UnityEngine.Random.Range(-_damagePopupRandomX, _damagePopupRandomX);

        DamagePopup popup = Instantiate(_damagePopupPrefab, spawnPosition, Quaternion.identity);
        popup.Setup(damage);
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.StopAllCoroutines();
            enemy.MoveEnemy(Vector2.zero);
            enemy.enabled = false;
        }

        EnemyAnimator enemyAnimator = enemy != null && enemy.EnemyAnimator != null
            ? enemy.EnemyAnimator
            : GetComponentInChildren<EnemyAnimator>();

        if (enemyAnimator != null)
            enemyAnimator.PlayDeath();

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
            collider.enabled = false;

        OnDeath?.Invoke();

        if (_destroyOnDeath)
            StartCoroutine(DeathRoutine(enemyAnimator));
    }

    private IEnumerator DeathRoutine(EnemyAnimator enemyAnimator)
    {
        if (enemyAnimator != null)
            yield return enemyAnimator.PlayDeathAndWait(_destroyDelay);
        else
            yield return new WaitForSeconds(_destroyDelay);

        Destroy(gameObject);
    }
}