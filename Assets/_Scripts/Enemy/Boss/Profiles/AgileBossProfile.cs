using UnityEngine;

[CreateAssetMenu(fileName = "Agile Boss Profile", menuName = "Enemy Logic/Boss/Profiles/Agile Boss")]
public class AgileBossProfile : BossProfileSO
{
    // Agile: быстрый, но хрупкий, постоянно движется
    public AgileBossProfile()
    {
        chaseSpeed = 3.5f;
        stoppingDistance = 0.8f;
        chaseUpdateRate = 0.3f;
        teleportStepDistance = 2f;
        minApproachDistance = 0.6f;
        baseAttackCooldown = 1.5f;
        specialAbilityCooldown = 4f;
        aggroRadius = 12f;
        strikeDistance = 2.5f;
    }
}
