using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggroCheck : MonoBehaviour
{
    public GameObject PlayerTarget { get; set; }
    private Enemy _enemy;

    private void Awake()
    {
        PlayerTarget = GameObject.FindGameObjectWithTag("Player");
        _enemy = GetComponentInParent<Enemy>();
        Debug.Log($"[EnemyAggroCheck] {gameObject.name}: Awake - PlayerTarget found: {PlayerTarget != null}, Enemy: {(_enemy != null ? _enemy.name : "NULL")}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == PlayerTarget)
        {
            Debug.Log($"[EnemyAggroCheck] {gameObject.name}: Player entered trigger, setting Aggro=true");
            _enemy.SetAggroStatus(true);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject == PlayerTarget)
        {
            Debug.Log($"[EnemyAggroCheck] {gameObject.name}: Player STAYS in trigger (IsAggroed={_enemy.IsAggroed})");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == PlayerTarget)
        {
            Debug.Log($"[EnemyAggroCheck] {gameObject.name}: Player exited trigger, setting Aggro=false");
            _enemy.SetAggroStatus(false);
        }
    }
}
