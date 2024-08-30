using Photon.Pun;
using System;
using UnityEngine;

/// <summary>
/// 개발자 : 서보운
/// 박스를 스폰할 스포너 스크립트
/// </summary>
public class BoxSpawner : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] BoxSpawnData curSpawnData;

    [Tooltip("스폰할 박스의 데이터")]
    [SerializeField] BoxData spawnBoxRank;
    [Tooltip("미믹 스폰 확률(10 : 10%, 50 : 50%)")]
    [SerializeField] protected int mimicSpawn;

    /// <summary>
    /// 박스 랭크를 결정할 메소드
    /// </summary>
    /// <param name="rank"></param>
    public void SetBoxRank(BoxRank rank)
    {
        // id -> 랭크 결정
        // 확률 -> 계산까지 해서
        id = (int)rank;
        //curSpawnData = CsvParser.Instance.BoxSpawnDic[id];        
    }

    /// <summary>
    /// 스폰 테이블을 받아오고 그에 따른 스폰 진행
    /// </summary>
    /// <param name="targetData"></param>
    public void SetBoxSpawnData(BoxSpawnData targetData)
    {
        curSpawnData = targetData;
        // 1. curSpawnData(BoxSpawnData)에 있는 chestSpawnProb의 확률 계산하기(1 ~ 100 사이의 int값)
        // 2. 스폰 상자들에 대한 가중치값 계산하기 

        // ex) chestSpawnProb가 30이고, 등장 상자의 종류가 2가지(1번 상자는 가중치 10, 2번 상자는 50)일 때의 스폰
        // 30%로 상자가 등장하고, 그 상자는 10 : 50으로 1번 상자(1/6) 혹은 2번 상자(5/6)가 등장함

        // 1. 토탈 가중치 계산
        // ex) 10, 30, 100
        // -> totalProb 140, rand (0 ~ 140), totalWeight -> 10 + 30 + 100 순
        int totalProb = curSpawnData.ReturnTotalWeight();

        int rand = UnityEngine.Random.Range(0, totalProb + 1);
        int totalWeight = 0;
        BoxRank spawnRank = BoxRank.Low;
        BoxData curData = new BoxData();

        // 2. 누적 계산과 확률 비교
        for (int i = 0; i < curSpawnData.chestTypeCount; i++)
        {
            curData = CsvParser.Instance.BoxDic[curSpawnData.chestIDs[i]];
            totalWeight += curData.weight;

            if (rand <= totalWeight)
            {
                spawnRank = (BoxRank)((int)spawnRank + i);
                break;
            }
        }

        SpawnBox(spawnRank, curData.id);
    }

    /// <summary>
    /// 실제 상자 스폰
    /// </summary>
    /// <param name="rank"></param>
    public void SpawnBox(BoxRank rank, int id)
    {
        int rand = UnityEngine.Random.Range(0, 101);

        switch (rank)
        {
            case BoxRank.Low:
                if (rand < mimicSpawn)
                {
                    PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Mimic_Normal", transform.position, Quaternion.identity);
                }
                else
                {
                    DropBox instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Chest/NormalBox", transform.position, Quaternion.identity).GetComponent<DropBox>();
                    instance.SetBoxTable(id);
                    if (ObjectGrouping.Instance != null)
                    {
                        instance.transform.parent = ObjectGrouping.Instance.BoxGroup;
                    }
                }
                break;
            case BoxRank.Middle:
                if (rand < mimicSpawn)
                {
                    PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Mimic_Middle", transform.position, Quaternion.identity);
                }
                else
                {
                    DropBox instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Chest/MiddleBox", transform.position, Quaternion.identity).GetComponent<DropBox>();
                    instance.SetBoxTable(id);
                    if (ObjectGrouping.Instance != null)
                    {
                        instance.transform.parent = ObjectGrouping.Instance.BoxGroup;
                    }
                }
                break;
            case BoxRank.High:
                {
                    DropBox instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Chest/HighBox", transform.position, Quaternion.identity).GetComponent<DropBox>();
                    instance.SetBoxTable(id);
                    if (ObjectGrouping.Instance != null)
                    {
                        instance.transform.parent = ObjectGrouping.Instance.BoxGroup;
                    }
                }
                break;
            case BoxRank.Legend:
                {
                    DropBox instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Chest/LegendBox", transform.position, Quaternion.identity).GetComponent<DropBox>();
                    instance.SetBoxTable(id);
                    if (ObjectGrouping.Instance != null)
                    {
                        instance.transform.parent = ObjectGrouping.Instance.BoxGroup;
                    }
                }
                break;
        }

        Destroy(gameObject);
    }

    /*
    /// <summary>
    /// 박스 스폰 메소드
    /// </summary>
    /// <param name="pos">스폰할 위치값</param>
    public void BoxSpawn(Vector3 pos)
    {
        // 1. curSpawnData(BoxSpawnData)에 있는 chestSpawnProb의 확률 계산하기(1 ~ 100 사이의 int값)
        // 2. 스폰 상자들에 대한 가중치값 계산하기 

        // ex) chestSpawnProb가 30이고, 등장 상자의 종류가 2가지(1번 상자는 가중치 10, 2번 상자는 50)일 때의 스폰
        // 30%로 상자가 등장하고, 그 상자는 10 : 50으로 1번 상자(1/6) 혹은 2번 상자(5/6)가 등장함

        // 1. curSpawnData(BoxSpawnData)에 있는 chestSpawnProb의 확률 계산하기(1 ~ 100 사이의 int값)
        int rand = UnityEngine.Random.Range(0, 101);

        if(rand <= curSpawnData.chestSpawnProb)
        {   // 
            return;
        }

        switch (spawnBoxRank)
        {
            case BoxRank.Low:
                if (rand < mimicSpawn)
                {
                    PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Mimic_Normal", pos, Quaternion.identity);
                }
                else
                {
                    DropBox instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Chest/NormalBox", pos, Quaternion.identity).GetComponent<DropBox>();
                    instance.SetBoxTable();
                }
                break;
            case BoxRank.Middle:
                if (rand < mimicSpawn)
                {
                    PhotonNetwork.InstantiateRoomObject("6.Prefab/Monster/Monster_Mimic_Middle", pos, Quaternion.identity);
                }
                else
                {
                    DropBox instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Chest/MiddleBox", pos, Quaternion.identity).GetComponent<DropBox>();
                    instance.SetBoxTable();
                }
                break;
            case BoxRank.High:
                {
                    DropBox instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Chest/HighBox", pos, Quaternion.identity).GetComponent<DropBox>();
                    instance.SetBoxTable();
                }
                break;
            case BoxRank.Legend:
                {
                    DropBox instance = PhotonNetwork.InstantiateRoomObject("6.Prefab/Chest/LegendBox", pos, Quaternion.identity).GetComponent<DropBox>();
                    instance.SetBoxTable();
                }
                break;

        }
    }
    */
}

[Serializable]
public struct BoxSpawnTable
{
    public BoxRank boxTypes;
}

