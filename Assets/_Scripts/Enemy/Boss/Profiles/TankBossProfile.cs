using UnityEngine;

[CreateAssetMenu(fileName = "Tank Boss Profile", menuName = "Enemy Logic/Boss/Profiles/Tank Boss")]
public class TankBossProfile : BossProfileSO
{
    // Tank: медленный, мощный, с большими шагами
    public TankBossProfile()
    {
        chaseSpeed = 1.2f;
        stoppingDistance = 1.5f;
        chaseUpdateRate = 0.8f;
        teleportStepDistance = 1f;
        minApproachDistance = 1.2f;
        baseAttackCooldown = 3f;
        specialAbilityCooldown = 8f;
        aggroRadius = 9f;
        strikeDistance = 4f;
    }
}
