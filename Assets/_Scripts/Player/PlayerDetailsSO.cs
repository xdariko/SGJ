using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDetails_", menuName = "Player/Player Details")]
public class PlayerDetailsSO : ScriptableObject
{
    [Header("Movement Settings")]
    public float MoveSpeed;
    public float SprintMultiplier;
    public int SprintStaminaCostPerSecond;

    [Header("Dash Settings")]
    public float DashDistance;
    public float DashDuration;
    public float DashCooldown;
    public float DashStaminaCost;

    [Header("Health Settings")]
    public float maxHealth = 100;
    public float healthRegenRate = 5f;

    [Header("Stamina Settings")]
    public float maxStamina = 100;
    public float staminaRegenRate = 5f;
}
