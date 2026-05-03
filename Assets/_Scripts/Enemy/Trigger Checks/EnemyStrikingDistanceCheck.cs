using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStrikingDistanceCheck : MonoBehaviour
{
    public GameObject PlayerTarget { get; set; }
    private Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
        Debug.Log($"[EnemyStrikingDistanceCheck] {gameObject.name}: Awake - Enemy: {(_enemy != null ? _enemy.name : "NULL")}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[EnemyStrikingDistanceCheck] {gameObject.name}: Player entered striking range, setting IsWithinStrikingDistance=true");
            _enemy.SetStrikingDistance(true);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[EnemyStrikingDistanceCheck] {gameObject.name}: Player STAYS in striking range (IsWithinStrikingDistance={_enemy.IsWithinStrikingDistance})");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[EnemyStrikingDistanceCheck] {gameObject.name}: Player exited striking range, setting IsWithinStrikingDistance=false");
            _enemy.SetStrikingDistance(false);
        }
    }
}
