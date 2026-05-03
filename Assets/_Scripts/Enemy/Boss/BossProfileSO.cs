using UnityEngine;

[CreateAssetMenu(fileName = "Boss Profile", menuName = "Enemy Logic/Boss/Boss Profile")]
public class BossProfileSO : ScriptableObject
{
    [Header("Chase Settings")]
    [Range(1f, 5f)]
    [Tooltip("Скорость преследования игрока")]
    public float chaseSpeed = 2f;
    
    [Range(0.5f, 3f)]
    [Tooltip("Дистанция, на которой босс останавливается и атакует")]
    public float stoppingDistance = 1f;
    
    [Range(0.1f, 2f)]
    [Tooltip("Как часто босс совершает рывок/телепорт (в секундах)")]
    public float chaseUpdateRate = 0.5f;

    [Header("Teleport/Dash Movement")]
    [Range(0.5f, 4f)]
    [Tooltip("Длина одного рывка/телепорта")]
    public float teleportStepDistance = 1.5f;
    
    [Range(1f, 5f)]
    [Tooltip("Минимальная дистанция до игрока, при которой босс перестаёт двигаться и атакует")]
    public float minApproachDistance = 0.8f;

    [Header("Special Ability Cooldowns")]
    [Range(1f, 10f)]
    [Tooltip("Базовая перезарядка обычных атак")]
    public float baseAttackCooldown = 2f;

    [Range(3f, 15f)]
    [Tooltip("Перезарядка специальных способностей (Dash, Charge, Summon)")]
    public float specialAbilityCooldown = 5f;

    [Header("Combat")]
    [Range(5f, 30f)]
    [Tooltip("Радиус агресссии босса")]
    public float aggroRadius = 10f;

    [Range(2f, 10f)]
    [Tooltip("Радиус обнаружения игрока для атаки")]
    public float strikeDistance = 3f;
}
