using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 초기 스포너 InitSpawn 메소드를 통해 외부에 스폰 
/// </summary>
public class InitSpawner : MonsterSpanwer
{
    /// <summary>
    /// 초기 몬스터 스폰 함수 
    /// </summary>
   /* public override void InitSpawn()
    {
        Debug.Log("스포너의 Spawn");
        foreach (MonsterSpawnTable table in curSpawnTable)
        {
            for (int i = 0; i < table.monsterCount; i++)
            {
                int inf = 0;
                Collider2D[] cols = null;
                Vector2 randPos = Vector2.zero;
                do
                {
                    
                    inf++;
                    randPos = new Vector2(transform.position.x, transform.position.y) + Random.insideUnitCircle * spawnRange;
                    cols = Physics2D.OverlapBoxAll(randPos,new Vector2 (3f,3f), 0, LayerMask.GetMask("Wall"));
                }
                while (cols.Length != 0 && inf < 30) ;


                MonsterSpawn(table.monsterTypes, randPos);
            }
        }
    }
    /// <summary>
    /// 몬스터가 사망했을 때 추가로 스폰할 메소드
    /// </summary>
    public override void AddtionalSpawn()
    {
       
        foreach (MonsterSpawnTable table in curSpawnTable)
        {
            
            for (int i = 0; i < table.monsterCount - spawnMonsters.Count; i++)
            {
                Collider2D[] cols = null;
                int inf = 0;
                Vector2 randPos = Vector2.zero;
                do
                {
                   
                    inf++;
                    randPos = new Vector2(transform.position.x, transform.position.y) + Random.insideUnitCircle * spawnRange;
                    cols = Physics2D.OverlapBoxAll(randPos, new Vector2(3f, 3f), 0, LayerMask.GetMask("Wall"));
                }
                while (cols.Length != 0 && inf < 30);

                MonsterSpawn(table.monsterTypes, randPos);
            }
        }
    }*/
}
