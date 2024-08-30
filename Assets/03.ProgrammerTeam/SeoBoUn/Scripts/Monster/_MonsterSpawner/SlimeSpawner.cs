using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 슬라임 스포너
/// 최대 7마리까지.
/// 최소 2마리 되었을 때 스폰예정
/// </summary>
public class SlimeSpawner : MonsterSpanwer
{
   /* public override void InitSpawn()
    {
        foreach(MonsterSpawnTable table in curSpawnTable)
        {
            for(int i = 0; i < table.monsterCount; i++)
            {
                Vector2 randPos = new Vector2(transform.position.x, transform.position.y) + Random.insideUnitCircle * spawnRange;

                MonsterSpawn(table.monsterTypes, randPos);
            }
        }
    }


    public override void AddtionalSpawn()
    {
        foreach (MonsterSpawnTable table in curSpawnTable)
        {
            for (int i = 0; i < table.monsterCount-spawnMonsters.Count; i++)
            {
                Vector2 randPos = new Vector2(transform.position.x, transform.position.y) + Random.insideUnitCircle * spawnRange;

                MonsterSpawn(table.monsterTypes, randPos);
            }
        }
    }*/
}
