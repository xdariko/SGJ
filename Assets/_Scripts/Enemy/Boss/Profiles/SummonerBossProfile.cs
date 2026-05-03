using UnityEngine;

[CreateAssetMenu(fileName = "Summoner Boss Profile", menuName = "Enemy Logic/Boss/Profiles/Summoner Boss")]
public class SummonerBossProfile : BossProfileSO
{
    // Summoner: средняя скорость, держится на расстоянии, часто призывает
    public SummonerBossProfile()
    {
        chaseSpeed = 1.8f;
        stoppingDistance = 2.5f;
        chaseUpdateRate = 0.6f;
        teleportStepDistance = 1.5f;
        minApproachDistance = 2f;
        baseAttackCooldown = 2.5f;
        specialAbilityCooldown = 6f;
        aggroRadius = 11f;
        strikeDistance = 5f;
    }
}
