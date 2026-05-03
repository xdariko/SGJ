using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggroCheck : MonoBehaviour
{
    public GameObject PlayerTarget { get; set; }
    private Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
        Debug.LogWarning($"[EnemyAggroCheck] {gameObject.name}: Awake - Enemy: {(_enemy != null ? _enemy.name : "NULL")}, collider: {GetComponent<Collider2D>() != null}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.LogWarning($"[EnemyAggroCheck] {gameObject.name}: OnTriggerEnter2D with {collision.gameObject.name}, tag={collision.tag}");
        
        if (collision.CompareTag("Player"))
        {
            Debug.LogWarning($"[EnemyAggroCheck] >>> Player detected! Setting Aggro=TRUE on {_enemy?.name}");
            _enemy?.SetAggroStatus(true);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[EnemyAggroCheck] {gameObject.name}: Player STAYS in trigger (IsAggroed={_enemy?.IsAggroed})");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.LogWarning($"[EnemyAggroCheck] >>> Player exited trigger, setting Aggro=FALSE on {_enemy?.name}");
            _enemy?.SetAggroStatus(false);
        }
    }
}
