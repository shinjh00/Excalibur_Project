using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 베이스 몬스터 스포너 스크립트
/// </summary>
public class MonsterSpanwer : MonoBehaviourPun
{
    [SerializeField] protected MonsterSpawnData curData;
    [SerializeField] protected int id;

    [SerializeField] protected bool isSetting;
    [SerializeField] protected int monsterTotalCount;
    [SerializeField] public int MonsterTotalCount { get { return monsterTotalCount; } set { monsterTotalCount = value; } }
    [SerializeField] protected float spawnRange;
    [SerializeField] protected MonsterSpawnTable[] curSpawnTable;
    [SerializeField] protected List<BaseMonster> spawnMonsters;
    [SerializeField] protected int minCount;

    [SerializeField] protected LayerMask layerMask;
    [SerializeField] protected LayerMask playerMask;
    public bool IsSetting { get { return isSetting; } }

    protected void Awake()
    {
        spawnMonsters = new List<BaseMonster>();
        layerMask = LayerMask.GetMask("Wall") | LayerMask.GetMask("Trap");
        playerMask = LayerMask.GetMask("Player");
    }

    /// <summary>
    /// ID를 전달받아서 스포너 설정
    /// </summary>
    /// <param name="id"></param>
    public void SetSpawn(int id)
    {
        curData = CsvParser.Instance.MonsterSpawnDic[id];

        curSpawnTable = new MonsterSpawnTable[curData.mobTypeCount];

        for (int i = 0; i < curSpawnTable.Length; i++)
        {
            curSpawnTable[i].monsterTypes = (MonsterType)curData.mobSpawns[i].mobID;
            curSpawnTable[i].monsterMinCount = curData.mobSpawns[i].mobMinCount;
            curSpawnTable[i].monsterMaxCount = curData.mobSpawns[i].mobMaxCount;
            curSpawnTable[i].SetMonsterCount();
            curSpawnTable[i].spawner = this;
        }

        isSetting = true;


    }
    [PunRPC]
    public void SyncCount(int value)
    {
        monsterTotalCount = value;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerMask.Contain(collision.gameObject.layer))
        {
            SpawnMethod();
        }
    }
    void SpawnMethod()
    {
        if (monsterTotalCount <= 1 && PhotonNetwork.IsMasterClient)
        {
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
                        randPos = new Vector2(transform.position.x, transform.position.y) + UnityEngine.Random.insideUnitCircle * spawnRange;
                        cols = Physics2D.OverlapBoxAll(randPos, new Vector2(5f, 5f), 0, layerMask);
                    }
                    while (cols.Length != 0 && inf < 30);

                    MonsterSpawn(table.monsterTypes, randPos, table);
                }
            }
        }

    }
    
    void AdditionalSpawnMethod()
    {
        if(UnityEngine.Random.value < 0.5f)
        {
            foreach (MonsterSpawnTable table in curSpawnTable)
            {
                if (table.monsterCount < table.monsterMinCount)
                {
                    for (int i = 0; i < table.monsterMaxCount - table.monsterCount; i++)
                    {
                        int inf = 0;
                        Collider2D[] cols = null;
                        Vector2 randPos = Vector2.zero;
                        do
                        {
                            inf++;
                            randPos = new Vector2(transform.position.x, transform.position.y) + UnityEngine.Random.insideUnitCircle * spawnRange;
                            cols = Physics2D.OverlapBoxAll(randPos, new Vector2(5f, 5f), 0, layerMask);
                        }
                        while (cols.Length != 0 && inf < 30);

                        MonsterSpawn(table.monsterTypes, randPos, table);
                    }
                }

            }
        }

    }


    /// <summary>
    /// 타입에 맞는 몬스터를 스폰하기 위한 메소드
    /// </summary>
    /// <param name="monsterType">스폰할 몬스터</param>
    /// <param name="pos">스폰 위치</param>
    protected void MonsterSpawn(MonsterType monsterType, Vector3 pos, MonsterSpawnTable table)
    {
        BaseMonster instance = null;
        switch (monsterType)
        {
            case MonsterType.Slime:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Slime", pos, Quaternion.identity).GetComponent<BaseMonster>();
                if (ObjectGrouping.Instance != null)
                {
                    instance.transform.parent = ObjectGrouping.Instance.MonsterGroup;
                }
                break;
            case MonsterType.Orc:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Orc", pos, Quaternion.identity).GetComponent<BaseMonster>();
                if (ObjectGrouping.Instance != null)
                {
                    instance.transform.parent = ObjectGrouping.Instance.MonsterGroup;
                }
                break;
            case MonsterType.Skeleton:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Skeleton", pos, Quaternion.identity).GetComponent<BaseMonster>();
                if (ObjectGrouping.Instance != null)
                {
                    instance.transform.parent = ObjectGrouping.Instance.MonsterGroup;
                }
                break;
            case MonsterType.ArmorOrc:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Armor_Orc", pos, Quaternion.identity).GetComponent<BaseMonster>();
                if (ObjectGrouping.Instance != null)
                {
                    instance.transform.parent = ObjectGrouping.Instance.MonsterGroup;
                }
                break;
            case MonsterType.ArmorSkeleton:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Armor_Skeleton", pos, Quaternion.identity).GetComponent<BaseMonster>();
                if (ObjectGrouping.Instance != null)
                {
                    instance.transform.parent = ObjectGrouping.Instance.MonsterGroup;
                }
                break;
            case MonsterType.Elite_Orc:
                instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Elite_Orc", pos, Quaternion.identity).GetComponent<BaseMonster>();
                if (ObjectGrouping.Instance != null)
                {
                    instance.transform.parent = ObjectGrouping.Instance.MonsterGroup;
                }
                break;
        }

        instance.SetStatRate(MonsterStat.Hp, SecondMapGen.Ins.CurDunegon.monHpReinforceRate);
        instance.SetStatRate(MonsterStat.Atk, SecondMapGen.Ins.CurDunegon.monAtkReinforceRate);
        instance.TableSetting(table);
        table.curCount++;
        spawnMonsters.Add(instance);
        MonsterTotalCount++;
    }
    /// <summary>
    /// 몬스터가 사망했을 때의 메소드
    /// </summary>
    public void MonsterDie(BaseMonster instance, bool additional)
    {
        MonsterTotalCount--;
        spawnMonsters.Remove(instance);
        if (additional && PhotonNetwork.IsMasterClient)
        {
            AdditionalSpawnMethod();
        }
    }


    [Serializable]
    public struct MonsterSpawnTable
    {
        public MonsterType monsterTypes;
        public int monsterCount;

        public int monsterMinCount;
        public int monsterMaxCount;

        public int curCount;

        public MonsterSpanwer spawner;

        public void SetMonsterCount()
        {
            monsterCount = UnityEngine.Random.Range(monsterMinCount, monsterMaxCount + 1);
        }

    }

}
