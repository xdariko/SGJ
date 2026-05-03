using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossSummonState : BossSpecialState
{
    private BossAttackControllerSO.SummonParams summonParams;
    private float summonTimer;

    public BossSummonState(BossEnemy boss, EnemyStateMachine stateMachine, BossAttackControllerSO.SummonParams summonParams)
        : base(boss, stateMachine)
    {
        this.summonParams = summonParams;
    }

    protected override IEnumerator Run()
    {
        // Stop movement during summon
        var nav = boss.GetComponent<EnemyNavMeshAgent2D>();
        nav?.Stop();
        boss.MoveEnemy(Vector2.zero);

        summonTimer = 0f;
        bool waveSpawned = false;

        while (true)
        {
            summonTimer += Time.deltaTime;

            if (!waveSpawned)
            {
                SpawnWave();
                waveSpawned = true;
            }

            // Check completion condition
            bool timeElapsed = summonTimer >= summonParams.summonDuration;
            bool allMinionsDead = summonParams.waitForMinionsDeath && boss.ActiveMinions.Count == 0;

            if (timeElapsed || allMinionsDead)
            {
                break;
            }

            yield return null;
        }

        // Post-action delay handled by Complete after this coroutine ends
        Complete();
    }

    private void SpawnWave()
    {
        if (summonParams.minionPrefabs == null || summonParams.minionPrefabs.Length == 0)
            return;

        for (int i = 0; i < summonParams.waveSize; i++)
        {
            GameObject prefab = summonParams.minionPrefabs[Random.Range(0, summonParams.minionPrefabs.Length)];
            if (prefab == null) continue;

            Vector2 spawnPos = (Vector2)boss.transform.position + Random.insideUnitCircle * 2f;
            GameObject minion = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            boss.RegisterMinion(minion);
        }
    }
}
